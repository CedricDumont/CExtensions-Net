using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
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

        #region Original Values Extensions

        public static T StartRecordingOriginalValues<T>(this T dbContext) where T : DbContext
        {
            return OriginalDbContextTracker<T>.Instance.AddTracker(dbContext);
        }

        public static T GetRecordedContext<T>(this T dbContext) where T : DbContext
        {
            return (T) OriginalDbContextTracker<T>.Instance[dbContext];
        }

        public static void StopRecordingOriginalValues<T>(this T dbContext) where T : DbContext
        {
            OriginalDbContextTracker<T>.Instance.StopTracking(dbContext);
        }

        public static void PauseRecordingOriginalValues<T>(this T dbContext) where T : DbContext
        {
            OriginalDbContextTracker<T>.Instance.PauseTracking(dbContext);
        }

        #endregion

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


        public static DbSet Set(this DbContext dbContext, String entityName)
        {
            var dbSet = (from dbs in dbContext.DbSets() 
                         where dbs.ElementType.Name == entityName select dbs)
                         .FirstOrDefault();

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

        private static Object GetPrimitivePropertyValue(this Object item, string proName)
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

        internal static async Task<string> ToXml(this DbContext dbContext, string rootName = "Root", ContextDataEnum contextData = ContextDataEnum.Local, bool includeNull = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<" + rootName + ">");

                switch(contextData)
                {
                    case ContextDataEnum.Relations:
                    case ContextDataEnum.ParentRelations:
                        LoadRelations(dbContext, contextData);
                        goto case ContextDataEnum.Local;
                    case ContextDataEnum.Local:
                        WriteLocalItems(dbContext, sb, includeNull);
                        break;
                    case ContextDataEnum.All:
                        await WriteAll(dbContext, sb, includeNull);
                        break;
                }

            sb.Append("</" + rootName + ">");

            return sb.ToString();
        }

        public static void WriteLocalItems(DbContext context, StringBuilder sb, bool includeNull)
        {
            var localItems = GetLocalList(context);

            foreach (var item in localItems)
            {
                WriteElement(sb, item.Value, context, item.Key.Item1, includeNull);
            }
        }

        public static String PrimaryKeyColumnFor(this DbContext dbContext, Type clrType)
        {
            var set = dbContext.StoreEntitySets().Where(s => s.ElementType.Name == clrType.Name).FirstOrDefault();

            IEnumerable<string> keyNames = set.ElementType
                                                .KeyMembers
                                                .Select(k => k.Name);

            return keyNames.FirstOrDefault();
        }

        public static String PrimaryKeyColumnFor(this DbContext dbContext, String TableName)
        {
            var set = dbContext.StoreEntitySets().Where(s => s.Table == TableName).FirstOrDefault();

            IEnumerable<string> keyNames = set.ElementType
                                                .KeyMembers
                                                .Select(k => k.Name);

            return keyNames.FirstOrDefault();
        }

        public static String KeyMemberFor(this DbContext dbContext, Type clrType)
        {
            var set = dbContext.EntitySets().Where(s => s.ElementType.Name == clrType.Name).FirstOrDefault();



            IEnumerable<string> keyNames = set.ElementType
                                                        .KeyMembers
                                                        .Select(k => k.Name);

            return keyNames.FirstOrDefault();
        }

        private static void LoadRelations(DbContext dbContext, ContextDataEnum contextData)
        {
            IEnumerable localList = dbContext.AllLocalItems();

            foreach (var item in localList)
            {
                if (contextData == ContextDataEnum.ParentRelations)
                {
                    LoadRelations(dbContext, item, EndToEndEnum.OneToOne);
                }
                else
                {
                    LoadRelations(dbContext, item, EndToEndEnum.OneToOne);
                    LoadRelations(dbContext, item, EndToEndEnum.OneToMany);
                }
            }
        }

        private static IEnumerable AllLocalItems(this DbContext dbContext)
        {
            IList<object> result = new List<object>();

            foreach (DbSet dbset in dbContext.DbSets().OrderBy(s => s.ElementType.Name))
            {
                var itemList = dbset.Local;
                foreach (var item in itemList)
                {
                    result.Add(item);
                }
            }
            return result;
        }

        private static IDictionary<Tuple<string, Object>, object> GetLocalList(DbContext dbContext)
        {
            IDictionary<Tuple<string, Object>, object> localList = new Dictionary<Tuple<string, object>, object>();

            foreach (DbSet dbset in dbContext.DbSets().OrderBy(s => s.ElementType.Name))
            {
                var itemList = dbset.Local;

                foreach (var item in itemList)
                {
                    var itemEntry = dbContext.AsObjectContext().ObjectStateManager.GetObjectStateEntry(item);
                    var itemId = itemEntry.EntityKey.EntityKeyValues[0].Value;

                    var key = Tuple.Create(dbset.ElementType.Name, itemId);

                    localList.Add(key, item);
                }
            }

            return localList;
        }

        private static void LoadRelations(DbContext dbContext, object item, EndToEndEnum endToEndType)
        {
            IEnumerable<IRelatedEnd> relEnds = dbContext.GetAllRelatedEnds(item, endToEndType);

            foreach (IRelatedEnd relEnd in relEnds)
            {
                if (!relEnd.IsLoaded)
                {
                    relEnd.Load();
                }

                foreach (var loeadedItem in relEnd)
                {
                    LoadRelations(dbContext, loeadedItem, EndToEndEnum.OneToOne);
                }
            }
        }


        private enum EndToEndEnum
        {
            OneToMany,
            OneToOne
        }

        private static IEnumerable<IRelatedEnd> GetAllRelatedEnds(this DbContext dbContext, Object item, EndToEndEnum endToEndEnum)
        {
            var itemEntry = dbContext.AsObjectContext().ObjectStateManager.GetObjectStateEntry(item);

            List<IRelatedEnd> relEndsResult = new List<IRelatedEnd>();

            IEnumerable<IRelatedEnd> relEnds =
               itemEntry.RelationshipManager
                    .GetAllRelatedEnds();

            if (relEnds.Count() > 0)
            {
                foreach (IRelatedEnd relEnd in relEnds)
                {
                    var typeofrel = relEnd.GetType();

                    if (endToEndEnum == EndToEndEnum.OneToOne && relEnd is System.Data.Entity.Core.Objects.DataClasses.EntityReference)
                    {
                        relEndsResult.Add(relEnd);
                    }
                    else if(endToEndEnum == EndToEndEnum.OneToMany && !(relEnd is System.Data.Entity.Core.Objects.DataClasses.EntityReference)) { 
                        //here big assumption....
                        relEndsResult.Add(relEnd);
                    }
                }
            }

            return relEndsResult;
        }

        private static async Task WriteAll(DbContext dbContext, StringBuilder sb, bool includeNull = true)
        {
            foreach (DbSet dbset in dbContext.DbSets().OrderBy(s => s.ElementType.Name))
            {
                var itemList = await dbset.ToListAsync();

                foreach (var item in itemList)
                {
                    var itemEntry = dbContext.AsObjectContext().ObjectStateManager.GetObjectStateEntry(item);
                    var itemId = itemEntry.EntityKey.EntityKeyValues[0].Value;

                    WriteElement(sb, item, dbContext, dbset.ElementType.Name, includeNull);
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

        public static IEnumerable<String> TableNames(this DbContext dbContext)
        {
            var tableNames = from t in dbContext.StoreEntitySets() select t.Table;

            List<string> result = tableNames.ToList();

            result.RemoveAll(item => item == null);

            return result;

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
