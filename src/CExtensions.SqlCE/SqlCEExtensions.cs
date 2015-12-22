using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CExtensions.EntityFramework;
using ErikEJ.SqlCe;

namespace CExtensions.SqlCE
{
    public static class SqlCEExtensions
    {
        public static Stopwatch deleteWatch = new Stopwatch();
        public static Stopwatch insertWatch = new Stopwatch();
        public static Stopwatch overAllWatch = new Stopwatch();
        public static Stopwatch dsWathc = new Stopwatch();
        public static Stopwatch ActivatorWatch = new Stopwatch();
        public static Stopwatch DbCreationWatch = new Stopwatch();


        public static void ResteWatch()
        {
            deleteWatch.Reset();
            insertWatch.Reset();
            overAllWatch.Reset();
            dsWathc.Reset();
            ActivatorWatch.Reset();
            DbCreationWatch.Reset();
        }

        public static void StopAllWatch()
        {
            deleteWatch.Stop();
            insertWatch.Stop();
            overAllWatch.Stop();
            dsWathc.Stop();
            ActivatorWatch.Stop();
            DbCreationWatch.Stop();
        }

        public static T ToDbContext<T>(this string filePath, string dir) where T : DbContext, new()
        {
            string connectionString = "Data Source = MyDatabase" + dir + ".sdf";

            overAllWatch.Start();
            SqlCeConnection conn = new SqlCeConnection(connectionString);

            ActivatorWatch.Start();
            T result = (T)Activator.CreateInstance(typeof(T), new Object[] { conn });
            ActivatorWatch.Stop();

            DbCreationWatch.Start();
            result.Database.CreateIfNotExists();
            DbCreationWatch.Stop();

            conn.Open();

            deleteWatch.Start();
            DeleteAll(result, conn);
            deleteWatch.Stop();

            dsWathc.Start();
            DataSet ds = new DataSet();
            ds.ReadXml(filePath);
            dsWathc.Stop();

            insertWatch.Start();
            foreach (DataTable table in ds.Tables)
            {
                var idColumnName = result.PrimaryKeyColumnFor(table.TableName);
                InsertRaw(idColumnName, table, conn);
               // DoBulkCopy(table.TableName, table, conn);
            }
            insertWatch.Stop();

            overAllWatch.Stop();

            conn.Close();
            return result;
        }

        private static IDictionary<string, String> dic = new Dictionary<string, String>();


        private static void InsertRaw(String  primaryKeyName, DataTable dataTable, SqlCeConnection conn)
        {
            String sqlCommandInsert = null;
            if (!dic.ContainsKey(dataTable.TableName + primaryKeyName))
            {
                var columnNames = dataTable.Columns.Cast<DataColumn>().Where(c => c.ColumnName.ToUpper() != primaryKeyName.ToUpper()).Select(c => c.ColumnName);

                string columns = string.Join(","
                        , columnNames);

                string values = string.Join(","
                    , columnNames.Select(c => string.Format("@{0}", c)));

                 sqlCommandInsert = string.Format("INSERT INTO " + dataTable.TableName + "({0}) VALUES ({1})", columns, values);

                dic.Add(dataTable.TableName + primaryKeyName, sqlCommandInsert);
        }
            else
            {
                sqlCommandInsert = dic[dataTable.TableName + primaryKeyName];
            }


           
                using (var cmd = new SqlCeCommand(sqlCommandInsert, conn))
                {
                    //conn.Open();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        cmd.Parameters.Clear();
                        foreach (DataColumn col in dataTable.Columns)
                            cmd.Parameters.AddWithValue("@" + col.ColumnName, row[col]);
                        int inserted = cmd.ExecuteNonQuery();
                    }
                }
            //conn.Close();


        }

        private static void DoBulkCopy(string  tableName, DataTable dataTable, SqlCeConnection conn)
        {
           
            //SqlCeBulkCopyOptions options = new SqlCeBulkCopyOptions();
            //if (keepNulls)
            //{
            //    options = options |= SqlCeBulkCopyOptions.KeepNulls;
            //}
            using (SqlCeBulkCopy bc = new SqlCeBulkCopy(conn))
            {
                bc.DestinationTableName = tableName;
                bc.WriteToServer(dataTable);
            }
        }

        private static void DeleteAll(DbContext dbContext, SqlCeConnection conn)
        {
           // conn.Open();
            using (SqlCeCommand cmd = new SqlCeCommand())
            {
                foreach (var tableName in dbContext.TableNames())
                {
                    var idColumnName = dbContext.PrimaryKeyColumnFor(tableName);
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM " + tableName;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "ALTER TABLE["+ tableName + "] ALTER COLUMN "+ idColumnName+" IDENTITY (1, 1) ";
                    cmd.ExecuteNonQuery();
                }
            }
            //conn.Close();
        }

    }
}
