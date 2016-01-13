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

namespace CExtensions.EntityFramework.Serializer
{

    public enum EndToEndEnum
    {
        OneToMany,
        OneToOne
    }

    public class XmlDbContextConverter : AbstractDbContextSerializer
    {
        public XmlDbContextConverter(DbContext context) : base(context)
        {
        }

        public String RootName { get; set; } = "Root";

        public ContextDataEnum ContextData { get; set; } = ContextDataEnum.Local;

        public Boolean IncludeNull { get; set; }

        public Boolean Idented { get; set; } = true;

        public async Task<String> Serialize()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<" + RootName + ">");

            switch (ContextData)
            {
                case ContextDataEnum.Relations:
                case ContextDataEnum.ParentRelations:
                    LoadRelations(Context, ContextData);
                    goto case ContextDataEnum.Local;
                case ContextDataEnum.Local:
                    WriteLocalItems(sb, IncludeNull);
                    break;
                case ContextDataEnum.All:
                    await WriteAll(Context, sb, IncludeNull);
                    break;
            }

            sb.Append("</" + RootName + ">");

            String result = sb.ToString();

            if (Idented)
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

        private async Task WriteAll(DbContext dbContext, StringBuilder sb, bool includeNull = true)
        {
            foreach (DbSet dbset in dbContext.DbSets().OrderBy(s => s.ElementType.Name))
            {
                var itemList = await dbset.ToListAsync();

                foreach (var item in itemList)
                {
                    var itemEntry = dbContext.AsObjectContext().ObjectStateManager.GetObjectStateEntry(item);
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
