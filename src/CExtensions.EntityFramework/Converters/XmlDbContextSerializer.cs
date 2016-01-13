using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CExtensions.EntityFramework;

namespace CExtensions.EntityFramework.Converters
{

    public enum EndToEndEnum
    {
        OneToMany,
        OneToOne
    }

    public class XmlDbContextConverterOptions : DbContextConverterOptions
    {
        public XmlDbContextConverterOptions(DbContextConverterOptions options = null) : base (options)
        {
        }

        public String RootName { get; private set; } = "Root";

        public XmlDbContextConverterOptions WithThisRootName(string rootName)
        {
            this.RootName = rootName;
            return this;
        }
    }

    public class XmlDbContextConverter : AbstractDbContextSerializer
    {
        public XmlDbContextConverter(DbContext context, DbContextConverterOptions options = null) : base(context, new XmlDbContextConverterOptions(options))
        {
        }

        protected override DbContextConverterOptions GetDefaultOptions()
        {
            return new XmlDbContextConverterOptions();
        }

        public async Task<String> Serialize()
        {
            StringBuilder sb = new StringBuilder();

            var options = Options as XmlDbContextConverterOptions;

            string rootName = options.RootName;

            sb.Append("<" + rootName + ">");

            switch (Options.ContextData)
            {
                case ContextDataEnum.Relations:
                case ContextDataEnum.ParentRelations:
                    LoadRelations();
                    goto case ContextDataEnum.Local;
                case ContextDataEnum.Local:
                    WriteLocalItems(sb, Options.IncludeNull);
                    break;
                case ContextDataEnum.All:
                    await WriteAll(sb, Options.IncludeNull);
                    break;
            }

            sb.Append("</" + rootName + ">");

            String result = sb.ToString();

            if (Options.Idented)
            {
                result = result.FormatXml();
            }

            return result;

        }

        public void WriteLocalItems(StringBuilder sb, bool includeNull)
        {
            var localItems = Context.GetLocalList();

            foreach (var item in localItems)
            {
                WriteElement(sb, item.Value, item.Key.Item1, includeNull);
            }
        }

        private async Task WriteAll(StringBuilder sb, bool includeNull = true)
        {
            foreach (DbSet dbset in Context.DbSets().OrderBy(s => s.ElementType.Name))
            {
                var itemList = await dbset.ToListAsync();

                foreach (var item in itemList)
                {
                    var itemEntry = Context.AsObjectContext().ObjectStateManager.GetObjectStateEntry(item);
                    var itemId = itemEntry.EntityKey.EntityKeyValues[0].Value;

                    WriteElement(sb, item, dbset.ElementType.Name, includeNull);
                }
            }
        }
       
        private void WriteElement(StringBuilder sb, Object item, String elementName, Boolean includeNull)
        {
            if (item == null)
            {
                return;
            }

            string tableName = Context.MappedTable(elementName);

            sb.Append("<" + tableName + ">");

            var properties = item.GetType().GetProperties();

            foreach (var prop in properties)
            {
                string proName = prop.Name;
                string colName = Context.MappedColumnName(elementName, proName);

                if (colName.IsNotNullOrEmpty())
                {
                    object val = GetPrimitivePropertyValue(item, proName);

                    if (val != null || includeNull)
                    {
                        sb.Append("<" + colName + ">");
                        if (val is DateTime)
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
}
