using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using CExtensions.EntityFramework.Converters;
using CExtensions.EntityFramework.Converters;

namespace CExtensions.EntityFramework
{

    public enum ContextDataEnum
    {
        Local,
        All,
        Relations,
        ParentRelations
    }
    public static class DbContextDumpExtensions
    {
        public static async Task<string> AsXmlAsync(
            this DbContext dbContext,
            DbContextConverterOptions options = null
           )
        {
            var converter = new XmlDbContextConverter(dbContext, options);
            return await converter.Serialize();
        }

        public static async Task<string> AsXmlAsync(
           this DbContext dbContext,
           ContextDataEnum contextData
          )
        {
            DbContextConverterOptions options = DbContextConverterOptions.DEFAULT.WithContextData(contextData);
            return await  dbContext.AsXmlAsync(options);
        }
    }
}
