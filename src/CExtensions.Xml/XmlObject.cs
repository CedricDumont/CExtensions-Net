using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Xml.Linq;
using System.Collections;

namespace CExtensions.Xml
{
    public class XmlObject : DynamicObject, IEnumerable
    {
        private readonly List<XElement> _elements;

        public XmlObject(string text)
        {
            var doc = XDocument.Parse(text);
            _elements = new List<XElement> { doc.Root };
        }

        public XmlObject(XElement element)
        {
            _elements = new List<XElement> { element };
        }

        protected XmlObject(IEnumerable<XElement> elements)
        {
            _elements = new List<XElement>(elements);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (binder.Name == "Value")
                result = _elements[0].Value;
            else if (binder.Name == "Count")
                result = _elements.Count;
            else
            {
                var attr = _elements[0].Attribute(XName.Get(binder.Name));
                if (attr != null)
                    result = attr;
                else
                {
                    var items = _elements.Descendants(XName.Get(binder.Name));
                    if (items == null || items.Count() == 0) return false;
                    if (items.Count() == 1)
                    {
                        XElement element = new List<XElement>(items)[0];
                        if (element.HasElements)
                        {
                            result = new XmlObject(items);
                        }
                        else
                        {
                            result = new List<XElement>(items)[0].Value;
                        }
                    }
                    else
                    {
                        result = new XmlObject(items);
                    }
                }
            }
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            int ndx = (int)indexes[0];
            result = new XmlObject(_elements[ndx]);
            return true;
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var element in _elements)
                yield return new XmlObject(element);
        }
    }
}

