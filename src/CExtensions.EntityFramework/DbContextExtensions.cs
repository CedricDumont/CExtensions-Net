using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CExtensions.EntityFramework
{
    public static class DbContextExtensions
    {

        public static ObjectContext AsObjectContext(this DbContext dbContext)
        {
            return ((IObjectContextAdapter)dbContext).ObjectContext;
        }

        public static MetadataWorkspace MetadataWorkspace(this DbContext dbContext)
        {
            return dbContext.AsObjectContext().MetadataWorkspace;;
        }

        public static IList<EntitySetBase> StoreEntitySets(this DbContext dbContext)
        {
            var sets = dbContext.MetadataWorkspace()
             .GetItems<EntityContainer>(DataSpace.SSpace)
             .Single()
             .BaseEntitySets;

            return sets;
        }

        public static IList<EntitySetBase> ConceptualEntitySets(this DbContext dbContext)
        {
            var sets = dbContext.MetadataWorkspace()
             .GetItems<EntityContainer>(DataSpace.CSpace)
             .Single()
             .BaseEntitySets;

            return sets;
        }

        public static IList<EntitySet> EntitySets(this DbContext dbContext)
        {
            EntityContainerMapping mapping = dbContext.MetadataWorkspace().GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single();

            var sets = mapping.ConceptualEntityContainer.EntitySets;

            return sets;
        }

        [Obsolete("The parameter 'assemblyName' is no longer used. it will be cleaned in next releases. it was marked as optional")]
        public static IEnumerable<DbSet> DbSets(this DbContext dbContext, string assemblyName = null)
        {
            return dbContext.DbSets();
        }

        public static IEnumerable<DbSet> DbSets(this DbContext dbContext)
        {
            IList<DbSet> result = new List<DbSet>();

            foreach (EntitySet set in dbContext.EntitySets())
            {
                var objtype = set.ElementType.GetClrType();

                if (objtype != null)
                {
                    var dbset = dbContext.Set(objtype);

                    result.Add(dbset);
                }
            }

            return result;
        }

        [Obsolete("will be removed in further release : assemblyName no longer used")]
        public static DbSet DbSetFor(this DbContext dbContext, String entityName, string assemblyName )
        {
            return dbContext.Set(entityName);
        }

        public static DbSet Set(this DbContext dbContext, String entityName)
        {
            var dbSet = (from dbs in dbContext.DbSets() where dbs.ElementType.Name == entityName select dbs).FirstOrDefault();

            return dbSet;
        }

        public static IList<PropertyColumnMapping> MappingTable(this DbContext dbContext, string entityName)
        {
            IList<PropertyColumnMapping> result = new List<PropertyColumnMapping>();

            var storageMetadata = dbContext.MetadataWorkspace().GetItems(DataSpace.SSpace);
            var entityProps = (from s in storageMetadata where s.BuiltInTypeKind == BuiltInTypeKind.EntityType select s as EntityType);
            var personRightStorageMetadata = (from m in entityProps where m.Name == entityName select m).Single();

            foreach (var item in personRightStorageMetadata.Properties)
            {
                if (item.MetadataProperties.Contains("Configuration"))
                {
                    if (item.MetadataProperties.Contains("PreferredName"))
                    {
                        var colName = item.Name;
                        var propertyName = item.MetadataProperties["PreferredName"].Value;
                        result.Add(new PropertyColumnMapping(propertyName.ToString(), colName));
                    }
                }

            }

            return result;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return !s.IsNotNullOrEmpty();
        }

        public static bool IsNotNullOrEmpty(this string s)
        {
            if (s == null)
            {
                return false;
            }

            if (s == "")
            {
                return false;
            }
            return true;
        }

        public static Object GetPropertyValue(this Object item, string proName)
        {
            if (item != null)
            {
                PropertyInfo value = item.GetType().GetProperty(proName);

                if (value != null)
                {
                    return value.GetValue(item);
                }

            }

            return null;
        }



        public static Object GetPrimitivePropertyValue(this Object item, string proName)
        {

            if (proName.IsNullOrEmpty())
            {
                return null; 
            }

            string[] AcceptedTypes = {
                                         "System.String",
                                         "System.Int32",
                                         "System.Int64",
                                         "System.Int16",
                                         "System.Long",
                                         "System.DateTime",
                                         "System.Boolean",
                                         "System.Decimal"
                                     };
            object result = null;

            if (item != null)
            {
                PropertyInfo value = item.GetType().GetProperty(proName);

                if (value.PropertyType.FullName.ContainsOneOf(AcceptedTypes))
                {

                    if (value != null)
                    {
                        object val = value.GetValue(item);

                        result = val == null ? null : val;
                    }
                }

            }

            return result;
        }

        public static String MappedTable(this DbContext dbContext, string entityName)
        {
            string tableName = (from t in dbContext.StoreEntitySets() where t.Name.ToUpper() == entityName.ToUpper() select t.Table).FirstOrDefault();

            return tableName;

        }

        public static String MappedEntity(this DbContext dbContext, string tableName)
        {
            string entityName = (from t in dbContext.StoreEntitySets() where t.Table.ToUpper() == tableName.ToUpper() select t.Name).FirstOrDefault();

            return entityName;

        }

        internal static async Task<string> ToXml(this DbContext dbContext, string rootName = "Root", ContextDataEnum contextData = ContextDataEnum.Local, string entityLoLoad = null, object objectEntityId = null, bool includeNull = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<" + rootName + ">");

            if (entityLoLoad.IsNotNullOrEmpty())
            {
                await WriteDbSet(dbContext, contextData, sb, dbContext.Set(entityLoLoad), objectEntityId, includeNull: includeNull);
            }
            else
            {
                foreach (DbSet dbset in dbContext.DbSets())
                {
                    await WriteDbSet(dbContext, contextData, sb, dbset, null, includeNull:includeNull);
                }
            }

            sb.Append("</" + rootName + ">");

            return sb.ToString();

        }

        public static String PrimaryKeyColumnFor(this DbContext dbContext, Type type)
        {
            var set = dbContext.StoreEntitySets().Where(s => s.ElementType.Name == type.Name).FirstOrDefault();

            IEnumerable<string> keyNames = set.ElementType
                                                        .KeyMembers
                                                        .Select(k => k.Name);

            return keyNames.FirstOrDefault();
        }

        public static String KeyMemberFor(this DbContext dbContext, Type type)
        {
            var set = dbContext.EntitySets().Where(s => s.ElementType.Name == type.Name).FirstOrDefault();

            IEnumerable<string> keyNames = set.ElementType
                                                        .KeyMembers
                                                        .Select(k => k.Name);

            return keyNames.FirstOrDefault();

            #region old code
            //try
            //{

            //    var metadata = dbContext.MetadataWorkspace()
            //                        .GetType(type.Name, type.Namespace, DataSpace.CSpace)
            //                        .MetadataProperties;
           

            //IEnumerable retval = (IEnumerable)metadata
            //                    .Where(mp => mp.Name == "KeyMembers")
            //                    .First()
            //                    .Value;

            //foreach (var i in retval)
            //{
            //    return i.ToString();
            //}
            //return null;
            //}
            //catch (Exception ex)
            //{
            //    var s = ex.Data;
            //}
            //return null;

            #endregion
        }

        private static Boolean CanWrite(this List<string> container, string key)
        {
            if(container.Contains(key))
            {
                return false;
            }
            else
            {
                container.Add(key);
                return true;
            }
        }

        private static async Task WriteDbSet(DbContext dbContext, ContextDataEnum contextData, StringBuilder sb, DbSet dbset, Object objectId = null, List<string> loadTracker = null, bool includeNull = true)
        {
            if (loadTracker == null)
            {
                loadTracker = new List<string>();
            }

            string entityName = dbset.ElementType.Name;

            if (objectId == null)
            {
                var itemList = contextData == ContextDataEnum.Local ? dbset.Local : await dbset.ToListAsync();

                foreach (var item in itemList)
                {
                    WriteElement(sb, item, dbContext, dbset.ElementType.Name, includeNull);
                }
            }
            else
            {
                var item = await dbset.FindAsync(objectId);

                if (item == null)
                {
                    throw new Exception("could not find item of type : " + entityName + " with id  : " + objectId);
                }

                if (loadTracker.CanWrite(dbset.ElementType.Name + objectId))
                {
                    WriteElement(sb, item, dbContext, dbset.ElementType.Name, includeNull);
                }

                if (true)
                {
                    var collectionProps = from prop in item.GetType().GetProperties() where prop.IsACollectionType() select prop;

                    foreach (var prop in collectionProps)
                    {
                        //invoke on the item
                        IEnumerable linkedCollection = (IEnumerable)item.GetPropertyValue(prop.Name);

                        Type type = prop.PropertyType.GetGenericArguments()[0];

                        DbSet set = dbContext.Set(type.Name);

                        if (set != null)
                        {
                            await WriteCollection(sb, linkedCollection, dbContext, type.Name, loadTracker);
                        }
                    }
                }
            }
        }

        public static EntityMapping GetMappings(this DbContext dbContext, String TableName)
        {
            string entityName = dbContext.MappedEntity(TableName);

            EntityType entityType = dbContext.EntitySets().Where(e => e.ElementType.Name == entityName).Single().ElementType;

            return GetEntityMapping(dbContext, entityType.GetClrType());

        }

        public static EntityMapping GetMappings<T>(this DbContext dbContext) where T : class
        {
            Type t = typeof(T);

            return GetEntityMapping(dbContext, t);
           
        }

        private static EntityMapping GetEntityMapping(DbContext dbContext, Type clrType)
        {
            string tableName = dbContext.MappedTable(clrType.Name);

            IEnumerable<PropertyColumnMapping> propertiesMapping = dbContext.MappingTable(clrType.Name);

            return new EntityMapping(clrType, tableName, propertiesMapping);
        }

        public static String MappedColumnName(this DbContext dbContext, string entityName, string propertyName)
        {
            var mapping = dbContext.MappingTable(entityName);

            string colName = (from m in mapping where m.PropertyName == propertyName select m.ColumnName).FirstOrDefault();

            return colName;
        }

        public static String MappedPropertyName(this DbContext dbContext, string tableName, string columnName)
        {
            var entityName = dbContext.MappedEntity(tableName);

            var mapping = dbContext.MappingTable(entityName);

            string colName = (from m in mapping where m.ColumnName == columnName select m.PropertyName).FirstOrDefault();

            return colName;
        }


        private static async Task WriteCollection(StringBuilder sb, IEnumerable items, DbContext dbContext, String elementName, List<string> loadTracker)
        {

            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                DbSet dbset = dbContext.Set(elementName);

                string columnIdName = dbContext.KeyMemberFor(dbset.ElementType);

                object idVal = item.GetPrimitivePropertyValue(columnIdName);

                await WriteDbSet(dbContext, ContextDataEnum.All, sb, dbset, idVal, loadTracker);

            }

        }

        private static void WriteElement(StringBuilder sb, Object item, DbContext dbContext, String elementName, Boolean includeNull)
        {
            if(item == null)
            {
                return;
            }

            string tableName = dbContext.MappedTable(elementName);

            sb.Append("<" + tableName + ">");

            var properties = item.GetType().GetProperties();

            foreach (var prop in properties)
            {
                string proName = prop.Name;
                string colName = dbContext.MappedColumnName(elementName, proName);

                if (colName.IsNotNullOrEmpty())
                {
                    object val = item.GetPrimitivePropertyValue(proName);

                    if (val != null || includeNull)
                    {
                        sb.Append("<" + colName + ">");
                        if(val is DateTime)
                        {
                            val = XmlConvert.ToString((DateTime)val);
                        }
                        if (val is decimal)
                        {
                            val = XmlConvert.ToString((decimal)val);
                        }
                        sb.Append(val);
                        sb.Append("</" + colName + ">");
                    }
                    else
                    {
                        //sb.Append("<NullVal>");
                        //sb.Append(proName);
                        //sb.Append("</NullVal>");
                    }
                }
                else
                {
                    //sb.Append("<ColumnNull>");
                    //sb.Append(elementName + "." + proName);
                    //sb.Append("</ColumnNull>");
                }
            }

            sb.Append("</" + tableName + ">");
        }
    }

    public class EntityMapping
    {
        internal EntityMapping(Type clrType, string tableName, IEnumerable<PropertyColumnMapping> propertiesMapping)
        {
            // TODO: Complete member initialization
            this.ClrType = clrType;
            this.Table = tableName;
            this.PropertiesMapping = propertiesMapping;
        }

        public Type ClrType { get; set; }

        public String Entity { get { return ClrType.Name; } }

        public String Table { get; private set; }

        public IEnumerable<PropertyColumnMapping> PropertiesMapping { get; private set; }

        public String MappedColumn(string property)
        {
            return (from p in PropertiesMapping where p.PropertyName == property select p.ColumnName).FirstOrDefault();
        }

        public String MappedProperty(string column)
        {
            return (from p in PropertiesMapping where p.ColumnName == column select p.PropertyName).FirstOrDefault();
        }

    }

    public struct PropertyColumnMapping
    {

        public PropertyColumnMapping(String propertyName, String columnName)
        {
            _columnName = columnName;
            _propertyName = propertyName;
        }

        private string _columnName;

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        private string _propertyName;

        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        public override string ToString()
        {
            return _propertyName + " => " + _columnName;
        }


    }
}
