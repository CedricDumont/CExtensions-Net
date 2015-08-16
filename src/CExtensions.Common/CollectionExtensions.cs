using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public static class CollectionExtensions
{
     
    public static bool IsACollectionType(this PropertyInfo property)
    {
        return (!typeof(String).Equals(property.PropertyType) &&
            typeof(IEnumerable).IsAssignableFrom(property.PropertyType));
        //if (typeof(String).Equals(property.PropertyType))
        //{
        //    return false;
        //}
        //if (property.PropertyType.IsArray)
        //{
        //    return true;
        //}
        //if(property.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null)
        //{
        //    return true;
        //}
        //if(typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
        //{
        //    return true;
        //}
        //return false;
    }
}
