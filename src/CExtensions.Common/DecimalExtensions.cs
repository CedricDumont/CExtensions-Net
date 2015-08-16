using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DecimalExtensions
    {

        public static decimal AsDecimal(this decimal? nullablevalue)
        {
            return nullablevalue.GetValueOrDefault();
        }

        public static double AsDouble(this decimal? nullablevalue)
        {
            return Convert.ToDouble(nullablevalue.GetValueOrDefault());
        }

     }
}
