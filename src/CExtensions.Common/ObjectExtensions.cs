using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this Object obj)
        {
            return obj == null;
        }

        public static string[] PropertiesWithAttribute<T>(this Object obj) where T : Attribute
        {
            if (obj == null)
            {
                return new string[0];
            }
            var properties = obj.GetType().GetProperties()
                        .Where(prop => prop.IsDefined(typeof(T), false));

            var propertyNames = from e in properties select e.Name;

            return propertyNames.ToArray();
        }

        public static void DisposeIfNotNull(this IDisposable disposableObj)
        {
            if (disposableObj != null)
            {
                disposableObj.Dispose();
            }
        }
    }
}
