using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Shouldly;
using System.Xml;
using System.Linq;

namespace CExtensions.Xml.Test
{
    public class XMLComparisonUtilsTest
    {
        [Fact]
        public void ShouldCompareIdenticalXml()
        {
            XmlComparisonUtils xmlUtils = new XmlComparisonUtils();
            string xmlString = "<Root><Author><FirstName>Cedric</FirstName><LastName></LastName></Author></Root>";

            var result = xmlUtils.CompareXml(xmlString, xmlString);

            result.AreEqual.ShouldBe(true);
            result.Errors.ShouldBe(null);

        }

        [Fact]
        public void ShouldFailInCompaingTwoXml()
        {
            XmlComparisonUtils xmlUtils = new XmlComparisonUtils();
            string expected = "<Root><Author><FirstName>Cedric</FirstName><LastName>Dumont</LastName><Age></Age></Author></Root>";
            string actual = "<Root><Author><FirstName>AnotherName</FirstName><LastName>AnotherLastName</LastName></Author></Root>";

            var result = xmlUtils.CompareXml(actual, expected);

            result.AreEqual.ShouldBe(false);
            result.Errors.Count().ShouldBe(4);


        }
    }
}
