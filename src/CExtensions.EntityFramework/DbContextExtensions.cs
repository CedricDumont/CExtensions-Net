using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
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

        public static IList<EntitySetBase> EntitySets(this DbContext dbContext)
        {
            var expectedMetaData = dbContext.AsObjectContext().MetadataWorkspace;

            var tables = expectedMetaData.GetItemCollection(DataSpace.SSpace)
             .GetItems<EntityContainer>()
             .Single()
             .BaseEntitySets;

            return tables;
        }

        public static Type GetCorrespondingType(this DbContext dbContext, String EntityName)
        {
            //TODO : This code should be rewworked : check in OSpace, I think I've seen the type that is easier to access.
            var expectedMetaData = dbContext.AsObjectContext().MetadataWorkspace;

            var tables = expectedMetaData.GetItemCollection(DataSpace.CSSpace);

            //var ospace = expectedMetaData.GetItemCollection(DataSpace.OSpace);

            //var fromospace = ospace.Where(p => p.BuiltInTypeKind == BuiltInTypeKind.EntityType).First();

            //var someOtherresult = fromospace.MetadataProperties;

            var ConceptualEntityContainer = ((System.Data.Entity.Core.Mapping.EntityContainerMapping)(tables[0])).ConceptualEntityContainer;

            var entitySet = ConceptualEntityContainer.EntitySets.Where(p => p.ElementType.Name == EntityName).FirstOrDefault();

            if(entitySet == null)
            {
                return null;
            }

            var elementType = entitySet.ElementType;

            var typefound = (elementType.MetadataProperties.Where(p => p.Name == "http://schemas.microsoft.com/ado/2013/11/edm/customannotation:ClrType").FirstOrDefault());
            
            if(typefound == null)
            {
                throw new Exception("could not infer clr type using the db context");
            }

            return (Type)typefound.Value;

        }

         [Obsolete("The parameter 'assemblyName' is no longer used. it will be cleaned in next releases. it was marked as optional")]
       
        public static IList<DbSet> DbSets(this DbContext dbContext,  string assemblyName = null)
        {
            return dbContext.DbSets();
        }

        public static IList<DbSet> DbSets(this DbContext dbContext)
        {
            IList<DbSet> result = new List<DbSet>();

            foreach (EntitySetBase type in dbContext.EntitySets())
            {
                var objtype = dbContext.GetCorrespondingType(type.Name);

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
            return dbContext.DbSetFor(entityName);
        }

        public static DbSet DbSetFor(this DbContext dbContext, String entityName)
        {
            var dbSet = (from dbs in dbContext.DbSets() where dbs.ElementType.Name == entityName select dbs).FirstOrDefault();

            return dbSet;
        }

        public static IList<PropertyColumnMapping> MappingTable(this DbContext dbContext, string entityName)
        {
            IList<PropertyColumnMapping> result = new List<PropertyColumnMapping>();

            var storageMetadata = ((EntityConnection)dbContext.AsObjectContext().Connection).GetMetadataWorkspace().GetItems(DataSpace.SSpace);
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
                        result.Add(new PropertyColumnMapping(colName, propertyName.ToString()));
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
            string tableName = (from t in dbContext.EntitySets() where t.Name == entityName select t.Table).FirstOrDefault();

            return tableName;

        }

        public static String MappedEntity(this DbContext dbContext, string tableName)
        {
            string entityName = (from t in dbContext.EntitySets() where t.Table == tableName select t.Name).FirstOrDefault();

            return entityName;

        }

        internal static async Task<string> ToXml(this DbContext dbContext, string assemblyName, string rootName = "Root", ContextDataEnum contextData = ContextDataEnum.Local, string entityLoLoad = null, object objectEntityId = null, bool includeNull = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<" + rootName + ">");

            if (entityLoLoad.IsNotNullOrEmpty())
            {
                await WriteDbSet(dbContext, contextData, sb, dbContext.DbSetFor(entityLoLoad, assemblyName), assemblyName, objectEntityId, includeNull: includeNull);
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



        public static String IdPropertyName(this DbContext dbContext, Type type)
        {
            var metadata = dbContext.AsObjectContext().MetadataWorkspace
                                .GetType(type.Name, type.Namespace, System.Data.Entity.Core.Metadata.Edm.DataSpace.CSpace)
                                .MetadataProperties;

            IEnumerable retval = (IEnumerable)metadata
                                .Where(mp => mp.Name == "KeyMembers")
                                .First()
                                .Value;

            foreach (var i in retval)
            {
                return i.ToString();
            }
            return null;
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

        private static async Task WriteDbSet(DbContext dbContext, ContextDataEnum contextData, StringBuilder sb, DbSet dbset, string assemblyName, Object objectId = null, List<string> loadTracker = null, bool includeNull = true)
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

                        DbSet set = dbContext.DbSetFor(type.Name, assemblyName);

                        if (set != null)
                        {
                            await WriteCollection(sb, linkedCollection, dbContext, type.Name, loadTracker, assemblyName);
                        }
                    }
                }
            }
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


        private static async Task WriteCollection(StringBuilder sb, IEnumerable items, DbContext dbContext, String elementName, List<string> loadTracker, string assemblyName)
        {

            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                DbSet dbset = dbContext.DbSetFor(elementName, assemblyName);

                string columnIdName = dbContext.IdPropertyName(dbset.ElementType);

                object idVal = item.GetPrimitivePropertyValue(columnIdName);

                await WriteDbSet(dbContext, ContextDataEnum.All, sb, dbset, assemblyName, idVal, loadTracker);

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

    public struct PropertyColumnMapping
    {

        public PropertyColumnMapping(String columnName, String propertyName)
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
