using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.EntityFramework.Converters
{
    public class DbContextConverterOptions
    {
       

        private DbContextConverterOptions()
        {

        }

        public DbContextConverterOptions(DbContextConverterOptions fromOptions = null)
        {
            fromOptions = fromOptions ?? DbContextConverterOptions.DEFAULT;
            this.ContextData = fromOptions.ContextData;
            this.IncludeNull = fromOptions.IncludeNull;
            this.Idented = fromOptions.Idented;
        }

        public static DbContextConverterOptions DEFAULT
        {
            get
            {
                return new DbContextConverterOptions();
            }
        }

        public ContextDataEnum ContextData { get; private set; } = ContextDataEnum.Local;

        public Boolean IncludeNull { get; private set; }

        public Boolean Idented { get; private set; } = true;

        public DbContextConverterOptions WithNullValues()
        {
            this.IncludeNull = true;
            return this;
        }

        public DbContextConverterOptions WithRelations()
        {
            this.ContextData = ContextDataEnum.Relations;
            return this;
        }

        public DbContextConverterOptions WithParentRelations()
        {
            this.ContextData = ContextDataEnum.ParentRelations;
            return this;
        }

        public DbContextConverterOptions WithAll()
        {
            this.ContextData = ContextDataEnum.All;
            return this;
        }

        public DbContextConverterOptions WithContextData(ContextDataEnum contextData)
        {
            this.ContextData = contextData;
            return this;
        }

        public DbContextConverterOptions WithNoFormating()
        {
            this.Idented = false;
            return this;
        }
    }

    public abstract class AbstractDbContextSerializer : IDbContextConverter
    {
        public AbstractDbContextSerializer(DbContext context, DbContextConverterOptions options)
        {
            Context = context;

            if (options == null)
            {
                Options = GetDefaultOptions();
            }
            else
            {
                Options = options;
            }
        }

        protected abstract DbContextConverterOptions GetDefaultOptions();

        protected DbContext Context
        {
            get; private set;
        }

        protected DbContextConverterOptions Options
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
