using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    public static class FileExtensions
    {
        public static long CountLines(this FileInfo f)
        {
            long count = 0;

            using (StreamReader r =  f.OpenText())
            {
                while (r.ReadLine() != null)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
