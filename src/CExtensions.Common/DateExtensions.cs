using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class DateExtensions
    {

        public static DateTime AsDateTime(this DateTime? nullablevalue)
        {
            return nullablevalue.GetValueOrDefault();
        }

        public static string Trimestre (this DateTime dt)
        {
            int m = dt.Month;
            int y = dt.Year;
            int q = 4;
            if (m <= 9) 
            {
                q--;
                if (m <= 6) 
                {
                    q--;
                    if (m <= 3) q--;
                }
            }
            return String.Format ("{0}{1}", y, q);
        }

        public static int Quarter(this DateTime dt)
        {
            int m = dt.Month;
            int y = dt.Year;
            int q = 4;
            if (m <= 9)
            {
                q--;
                if (m <= 6)
                {
                    q--;
                    if (m <= 3) q--;
                }
            }
            return q;
        }

    }
}
