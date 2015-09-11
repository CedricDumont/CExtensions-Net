using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class StringExtensions
    {
        public static bool ContainsOneOf(this string s, string[] values)
        {
            if(values == null)
            {
                return false;
            }

            foreach (string val in values)
            {
                if (s.Contains(val))
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task WriteToFile(this string s, string filePath, bool append = false)
        {
            FileInfo fi = new FileInfo(filePath);

            FileMode fileMode = append ? FileMode.Append : FileMode.Create;

            using (var fileStream = new FileStream(filePath, fileMode))
            {
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    await sw.WriteAsync(s);
                }
            }
        }

        public static bool IsNullOrEmpty(this string s)
        {
            if (s == null)
            {
                return true;
            }
            else if(s.Equals(""))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsNotNullOrEmpty(this string s)
        {
            return !s.IsNullOrEmpty();
        }

        public static string Right(this string str, int length)
        {
            if(length < 0)
            {
                length = 0;
            }

            length = Math.Min(str.Length, length);

            return str.Substring(str.Length - length, length);
        }

        public static string Left(this string str, int length)
        {
            if(length < 0)
            {
                length = 0;
            }
            return str.Substring(0, Math.Min(str.Length, length));
        }

        /// <summary>
        /// How to:
        /// --------
        /// string title = "STRING";
        /// bool contains = title.Contains("string", StringComparison.OrdinalIgnoreCase);
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Limits the length of string to <code>limit</code>
        /// </summary>
        /// <param name="s"></param>
        /// <param name="limit">max length</param>
        /// <returns></returns>
        public static String Limit(this string s, int limit)
        {
            if (s != null)
            {
                if (s.Length > limit)
                {
                    if (limit < 0)
                    {
                        s = ""; ;
                    }
                    else
                    {
                        s = s.Substring(0, limit);
                    }
                }
            }
            return s;
        }
        
    }
}
