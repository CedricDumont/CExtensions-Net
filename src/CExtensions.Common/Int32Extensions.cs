using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{ 
    public static class Int32Extensions
    { 
        public static decimal AsInt(this Int32? nullablevalue)
        {
            return nullablevalue.GetValueOrDefault();
        }
    }
}
