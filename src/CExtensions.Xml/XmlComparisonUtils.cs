using NetBike.XmlUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using netbike = NetBike.XmlUnit;

namespace CExtensions.Xml
{
    public class XmlComparisonUtils
    {
        public XmlComparisonResult CompareXml(string actualXml, string expectedXml, string[] ignoreFields = null)
        {
            var comparer = new netbike.XmlComparer
            {
                NormalizeText = true,
                Analyzer = NetBike.XmlUnit.XmlAnalyzer.Custom()
                    .SetEqual(NetBike.XmlUnit.XmlComparisonType.NodeListSequence)
                    .SetSimilar(NetBike.XmlUnit.XmlComparisonType.NamespacePrefix)
                //Handler = ComparisonHandler

            };

            netbike.XmlComparisonResult result = comparer.Compare(expectedXml, actualXml);

            bool isEqual = true;
            List<String> errorList = null;

            if (!result.IsEqual)
            {
                errorList = new List<string>();

                foreach (var item in result.Differences)
                {
                    XElement elem = item.Difference.ActualDetails.Node as XElement;
                    if (elem != null && !elem.Name.LocalName.ContainsOneOf(ignoreFields))
                    {
                        StringBuilder sb = new StringBuilder();

                        if(item.Difference.ComparisonType == XmlComparisonType.NodeList)
                        {
                           
                            sb.Append("Should Be [" + item.Difference.ExpectedDetails.Node);
                            sb.Append("] But Was [" + item.Difference.ActualDetails.Node + "]");
                            
                        }
                        else if (item.Difference.ComparisonType == XmlComparisonType.TextValue)
                        {
                            sb.Append(item.Difference.ActualDetails.XPath +  " Should Be [" 
                                + item.Difference.ExpectedDetails.Value);
                            sb.Append("] But Was [" + item.Difference.ActualDetails.Value + "]");
                        }
                        else if (item.Difference.ComparisonType == XmlComparisonType.NodeListLookup)
                        {
                            sb.Append(item.Difference.ActualDetails.XPath + " was not found in Actual value");
                        }
                        else
                        {
                            sb.Append("State: " + item.Difference.ComparisonType);
                            sb.Append(" Comparison: " + item.Difference);
                            sb.Append(Environment.NewLine);
                            isEqual = false;
                        }
                        sb.Append(Environment.NewLine);
                        errorList.Add(sb.ToString());
                        isEqual = false;
                    }

                }
            }


            return new XmlComparisonResult() { AreEqual = isEqual, Errors = errorList };
        }

    }
}
