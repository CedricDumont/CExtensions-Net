using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CExtensions.Xml
{
    public class XmlComparisonResult
    {
        public bool AreEqual { get; set; }

        public IEnumerable<String>  Errors { get; set; }
    }
}
