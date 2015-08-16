using CExtensions.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace System.Xml
{
    public static class XmlExtensions
    {
        public static string FormatXml(this string xmlString, bool idented = true)
        {
            try
            {
                var CleanedxmlString = xmlString.Replace("&", "");

                XDocument doc = XDocument.Parse(CleanedxmlString);

                System.Xml.Linq.SaveOptions identedFormat =
                    idented ? System.Xml.Linq.SaveOptions.None : System.Xml.Linq.SaveOptions.DisableFormatting;

                string indented = doc.ToString(identedFormat);
                return indented;
            }
            catch (Exception ex)
            {
                throw new Exception("error whil parsing xml " + ex.Message, ex);
            }
        }
        static string RemoveInvalidXmlChars(string text)
        {
            var validXmlChars = text.Where(ch => XmlConvert.IsXmlChar(ch)).ToArray();
            return new string(validXmlChars);
        }

        static bool IsValidXmlString(string text)
        {
            try
            {
                XmlConvert.VerifyXmlChars(text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static dynamic XmlToDynamic(this string s)
        {
            return new XmlObject(s);
        }

    }
}
