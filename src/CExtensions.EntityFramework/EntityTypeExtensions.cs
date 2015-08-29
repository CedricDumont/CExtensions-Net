using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Metadata.Edm
{
    public static class EntityTypeExtensions
    {
        public static Type GetClrType(this EntityType elementType)
        {
            var typefound = (elementType.MetadataProperties.Where(p => p.Name == "http://schemas.microsoft.com/ado/2013/11/edm/customannotation:ClrType").FirstOrDefault());

            if (typefound == null)
            {
                throw new Exception("could not infer clr type using the elementType : " + elementType);
            }

            return (Type)typefound.Value;

        }

        
    }
}
