using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reflection
{
    public static class PropertyInfoExtension
    {

        public static bool HasAttribute<T>(this PropertyInfo pi) where T : Attribute  
        {
             return Attribute.IsDefined(pi, typeof(T));
        }
    }
}
