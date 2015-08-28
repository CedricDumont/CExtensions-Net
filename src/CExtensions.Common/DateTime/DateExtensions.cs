using System;
using System.Collections.Generic;
using System.Globalization;
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

        public static Quarter Quarter(this DateTime dt)
        {
            return new Quarter(dt);
        }

        public static int WeekOfYear(this DateTime dt)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            int weekOfYear = cal.GetWeekOfYear(dt, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
            return weekOfYear;

        }

    }
}
