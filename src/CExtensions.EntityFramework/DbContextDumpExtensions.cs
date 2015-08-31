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
        All
    }
    public static class DbContextDumpExtensions
    {

        
        public static async Task WriteXmlAsync(
            this DbContext dbContext,
            string assemblyName, 
            string file,
            ContextDataEnum contextData = ContextDataEnum.Local, 
            string root = "Root", 
            Boolean append = false, 
            string entityName = null, 
            Object id = null,
            bool includeNull = false)
        {
            FileInfo fi = new FileInfo(file);

            FileMode fileMode = append ? FileMode.Append : FileMode.Create;

            using (var fileStream = new FileStream(file, fileMode))
            {
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    string result = await dbContext.ToXml(root, contextData, entityName, id, includeNull);
                    await sw.WriteAsync(result.FormatXml());
                }
            }
        }

       
        public static async Task<string> AsXmlAsync(
            this DbContext dbContext, 
            ContextDataEnum contextData = ContextDataEnum.Local,
            string root = "Root",
            string entityToLoad = null,
            object objectEntityId = null,
            bool includeNull = false)
        {
            return ((await dbContext.ToXml(root, contextData, entityToLoad, objectEntityId,includeNull)).FormatXml());
        }


       
    }
}
