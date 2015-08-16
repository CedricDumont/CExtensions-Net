using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Shouldly;
using System.Xml;

namespace CExtensions.Xml.Test
{
    public class XMLObjectTest
    {
        [Fact]
        public void ShouldRetrieveObjectPropertiesFromXml()
        {
            string xmlString = "<Root><Author><FirstName>Cedric</FirstName><LastName></LastName></Author></Root>";

            var xmlObject = xmlString.XmlToDynamic();

            ((string)xmlObject.Author[0].FirstName).ShouldBe("Cedric");

        }

        [Fact]
        public void ShouldRetrieveObjectWithIndex()
        {
            string xmlString = "<Root><Author><FirstName>Cedric</FirstName><LastName></LastName></Author> " +
                "<Author><FirstName>Second Cedric</FirstName><LastName></LastName></Author></Root>";

            var xmlObject = xmlString.XmlToDynamic();

            ((string)xmlObject.Author[0].FirstName).ShouldBe("Cedric");
            ((string)xmlObject.Author[1].FirstName).ShouldBe("Second Cedric");

        }
    }
}
