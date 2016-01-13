using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.EntityFramework.Serializer
{
    public class AbstractDbContextSerializer : IDbContextConverter
    {
        public AbstractDbContextSerializer(DbContext context)
        {
            Context = context;
        }

        protected DbContext Context
        {
            get; private set;
        }

        protected void LoadRelations(DbContext dbContext, ContextDataEnum contextData)
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

        protected void LoadRelations(DbContext dbContext, object item, EndToEndEnum endToEndType)
        {
            IEnumerable<IRelatedEnd> relEnds = GetAllRelatedEnds(item, endToEndType);

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

        private IEnumerable<IRelatedEnd> GetAllRelatedEnds(Object item, EndToEndEnum endToEndEnum)
        {
            var itemEntry = Context.AsObjectContext().ObjectStateManager.GetObjectStateEntry(item);

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
                    else if (endToEndEnum == EndToEndEnum.OneToMany && !(relEnd is System.Data.Entity.Core.Objects.DataClasses.EntityReference))
                    {
                        //here big assumption....
                        relEndsResult.Add(relEnd);
                    }
                }
            }

            return relEndsResult;
        }

        protected Object GetPrimitivePropertyValue(Object item, string proName)
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

    }
}
