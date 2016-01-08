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

namespace CExtensions.EntityFramework
{

    public enum ContextDataEnum
    {
        Local,
        All,
        Relations
    }
    public static class DbContextDumpExtensions
    {
        public static async Task<string> AsXmlAsync(
            this DbContext dbContext, 
            ContextDataEnum contextData = ContextDataEnum.Local,
            string root = "Root",
            string entityToLoad = null,
            object objectEntityId = null,
            bool includeNull = false,
            bool originalValues = false)
        {
            return ((await dbContext.ToXml(root, contextData, entityToLoad, objectEntityId,includeNull, originalValues)).FormatXml());
        }


       
    }
}
