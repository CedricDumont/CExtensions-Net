using KellermanSoftware.CompareNetObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;
using CExtensions.EF;

namespace CExtensions.Effort
{

    internal class DbContextObjectComparer
    {
        private DbContextObjectComparer()
        {

        }

        private static ComparisonResult CompareObjects(Object expectedObj, Object actualObj, string objectName = "", String id = "", String[] ignoredMembers = null)
        {

            ComparisonConfig cfg = new ComparisonConfig()
            {
                IgnoreObjectTypes = true,
                IgnoreCollectionOrder = true,
                IgnoreUnknownObjectTypes = true,
                ComparePrivateFields = false,
                ComparePrivateProperties = false,
                MembersToIgnore = new List<string>() { "_entityWrapper" },
                CompareChildren = false,
                MaxStructDepth = 1,
                MaxDifferences = 20,
                TreatStringEmptyAndNullTheSame = true

            };
            if (ignoredMembers != null)
            {
                cfg.MembersToIgnore.AddRange(ignoredMembers);
            }
            //also add not mapped attributes
            if(actualObj.PropertiesWithAttribute<NotMappedAttribute>().Length > 0)
            {
                cfg.MembersToIgnore.AddRange(actualObj.PropertiesWithAttribute<NotMappedAttribute>());
            }

            CompareLogic cpl = new CompareLogic(cfg);


            var result = cpl.Compare(expectedObj, actualObj);


            return result;
        }


        private static async Task<IList<DbContextCheckEntry>> CheckCollectionCount(IList<DbSet> expectedDbSets, DbContext actualContext, string assemblyName)
        {
            IList<DbContextCheckEntry> result = new List<DbContextCheckEntry>();
            foreach (DbSet dbSet in expectedDbSets)
            {
                var expectedList = await dbSet.ToListAsync();
                var actualList = await actualContext.DbSetFor(dbSet.ElementType.Name, assemblyName).ToListAsync();
                if (actualList.Count != expectedList.Count)
                {
                    DbContextCheckEntry entry = new DbContextCheckEntry();
                    entry.ActualPropertyContent = actualList.Count.ToString();
                    entry.ExpectedPropertyContent = expectedList.Count.ToString();
                    entry.PropertyName = "Collection Count";
                    entry.ObjectName = dbSet.ElementType.Name;
                    entry.ObjectId = "-";
                    result.Add(entry);
                }
            }

            return result;
        }


        public static async Task<DbContextCheckResult> ChecDbCOntext(DbContext expectedctx, DbContext actualContext, string assemblyName, string[] ignoreFields)
        {
            IList<DbSet> expectedDbSets = expectedctx.DbSets(assemblyName);

            IList<DbContextCheckEntry> collectionCountResult = await CheckCollectionCount(expectedDbSets, actualContext, assemblyName);

            if(collectionCountResult.Count > 0)
            {
                return new DbContextCheckResult(false, collectionCountResult);
            }

            foreach (DbSet dbSet in expectedDbSets)
            {
                var expectedList = await dbSet.ToListAsync();

                var ignoredMembers = GetIgnoredMembers(dbSet.ElementType.Name, ignoreFields);

                foreach (Object expectedObject in expectedList)
                {
                    Object actualObject = null;

                    var idProp = expectedctx.IdPropertyName(dbSet.ElementType);

                    var prop = expectedObject.GetType().GetProperty(idProp, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        
                            var idValue = prop.GetValue(expectedObject);

                            var actualDbSet = actualContext.Set(dbSet.ElementType);

                            actualObject = actualDbSet.Find(idValue);

                            ComparisonResult cr = DbContextObjectComparer.CompareObjects(expectedObject, actualObject, dbSet.ElementType.Name, idValue + "", ignoredMembers);

                            if (!cr.AreEqual)
                            {
                                return new DbContextCheckResult(false,
                                    cr.Differences.ToDbContextCheckEntry(idValue.ToString(), dbSet.ElementType.Name));
                            }
                       
                    }
                }
            }

            return new DbContextCheckResult();
        }

        private static String[] GetIgnoredMembers(string entityName, string[] properties)
        {
            if (properties == null)
            {
                return null;
            }

            List<string> result = new List<string>();
            foreach (var item in properties)
            {
                if (item.Contains(entityName))
                {
                    string[] splitted = item.Split('.');
                    result.Add(splitted[1]);
                }
            }
            return result.ToArray(); ;
        }
    }

    

    public static class DbContextCompareExtensions
    {
        public static async Task<DbContextCheckResult> CompareTo(this DbContext dbcontext, DbContext expectedContext, string assemblyName, string[] ignoredProperties = null)
        {
            return await DbContextObjectComparer.ChecDbCOntext(expectedContext, dbcontext, assemblyName, ignoredProperties);
        }


        internal static IList<DbContextCheckEntry> ToDbContextCheckEntry(this IEnumerable<Difference> differences, Object objectId, string objectName)
        {
            IList<DbContextCheckEntry> entries = new List<DbContextCheckEntry>();

            foreach (var item in differences)
            {
                //The property starts with a dot in KellermannDifferences
                var propName = item.PropertyName;
                if (item.PropertyName.StartsWith("."))
                {
                    propName = item.PropertyName.Remove(0, 1);
                }

                var entry = DbContextCheckEntry
                                .ForObject(objectId.ToString(), objectName)
                                .WithProperty(propName)
                                .Was(item.Object2Value)
                                .InsteadOf(item.Object1Value);
                entries.Add(entry);
            }

            return entries;
        }

    }
}
