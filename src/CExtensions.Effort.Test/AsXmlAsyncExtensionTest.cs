using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using CExtensions.Effort;
using Xunit;
using CExtensions.Test.Model;
using CExtensions.EntityFramework;
using System.Xml;
using Shouldly;
using Test.Helper;
using CExtensions.Xml;

namespace CExtensions.Test
{
    public class AsXmlAsyncExtensionTest : BaseUnitTest
    {
        private string[] _tableList = new string[] { "Author", "Post" };

        //[Fact]
        //public async Task TestFromRealDb()
        //{
        //    string connectionFromAppConfig = "SampleContextConn";

        //    using (SampleContext ctx = new SampleContext(connectionFromAppConfig))
        //    {
        //        string fromDb = await ctx.AsXmlAsync("CExtensions.SampleApp", false);
        //    }
        //}

        [Fact]
        public async Task TestWithXmlFIles()
        {
            //load a context
            XmlFileContext<SampleContext> xmlFileCtx = new XmlFileContext<SampleContext>(this.GetType());

            using (SampleContext ctx = xmlFileCtx.InputContext("test1"))
            {

                string fromFile = await ctx.AsXmlAsync(ContextDataEnum.All);
                dynamic obj = fromFile.XmlToDynamic();

                ((string)obj.AUTHOR[0].AUT_FIRSTNAME).ShouldBe("John");
            }
        }

        [Fact]
        public async Task TestReturnEmptyXml()
        {
            XmlFileContext<SampleContext> xmlFileCtx = new XmlFileContext<SampleContext>(this.GetType());

            using (SampleContext emptyContext = xmlFileCtx.Empty())
            {
                Author aut1 = new Author();
                aut1.FirstName = "testname";

                emptyContext.Authors.Attach(aut1);

                string fromFile = await emptyContext.AsXmlAsync(ContextDataEnum.Local, includeNull: true);

                dynamic obj = fromFile.XmlToDynamic();

                ((string)obj.AUTHOR[0].AUT_FIRSTNAME).ShouldBe("testname");
            }
        }

        [Theory]
        [InlineData("test_1", 2, ContextDataEnum.Local)]
        [InlineData("test_1", 2, ContextDataEnum.All)]
        [InlineData("test_1", 2, ContextDataEnum.Relations)]
        [InlineData("test_1", 1, ContextDataEnum.Relations)]
        public async Task TestWithObjectWithParentAndChild(string testin, Int64 postId, ContextDataEnum contextData)
        {
            XmlFileContext<SampleContext> xmlFileCtx = new XmlFileContext<SampleContext>(this.GetType());

            using (SampleContext ctx = xmlFileCtx.InputContext(testin, "as-xml-tests"))
            {
                ctx.Posts.Find(postId);
                String xml_result_exepcted = GetOutFileContent(testin + contextData.ToString("G") + postId, "as-xml-tests");
                string xml_result_actual = await ctx.AsXmlAsync(contextData);

                var result = new XmlComparisonUtils().CompareXml(xml_result_actual, xml_result_exepcted);
                result.AreEqual.ShouldBe(true, result.ToString());
            }
        }
    }
}
