using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public struct Quarter
    {
        public int _internalRepresentation;

        public Quarter(DateTime dt)
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
            _internalRepresentation = q;
        }

        public int Value { get { return _internalRepresentation; } }



    }
}
