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
using CExtensions.EntityFramework.Converters;

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
                string fromFile = await ctx.AsXmlAsync(DbContextConverterOptions.DEFAULT.WithAll());
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

                string fromFile = await emptyContext.AsXmlAsync(DbContextConverterOptions.DEFAULT.WithNullValues());

                dynamic obj = fromFile.XmlToDynamic();

                ((string)obj.AUTHOR[0].AUT_FIRSTNAME).ShouldBe("testname");
            }
        }

        [Theory]
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

                var result = await xmlFileCtx.Compare(xml_result_actual, xml_result_exepcted);
                result.AreEqual.ShouldBe(true, result.ToString());
            }
        }

        [Theory]
        [InlineData("test_2", 1, ContextDataEnum.Relations)]
        [InlineData("test_2", 2, ContextDataEnum.Relations)]
        public async Task TestLoadingChilds(string testin, Int64 autId, ContextDataEnum contextData)
        {
            XmlFileContext<SampleContext> xmlFileCtx = new XmlFileContext<SampleContext>(this.GetType());

            string testOut = testin + contextData.ToString("G") + autId;

            using (SampleContext ctx = xmlFileCtx.InputContext(testin, "as-xml-tests"))
            {
                ctx.Authors.Find(autId);
               
                String xml_result_exepcted = GetOutFileContent(testOut,"as-xml-tests");
                string xml_result_actual = await ctx.AsXmlAsync(contextData);

                var result = await xmlFileCtx.Compare(xml_result_actual, xml_result_exepcted);
                result.AreEqual.ShouldBe(true, result.ToString());
            }
        }

        [Theory]
        [InlineData("test_3", 3, ContextDataEnum.ParentRelations)]
        public async Task TestLoadingParent(string testin, Int64 comId, ContextDataEnum contextData)
        {
            XmlFileContext<SampleContext> xmlFileCtx = new XmlFileContext<SampleContext>(this.GetType());

            string testOut = testin + contextData.ToString("G") + comId;

            using (SampleContext ctx = xmlFileCtx.InputContext(testin, "as-xml-tests"))
            {
                ctx.Comments.Find(comId);

                String xml_result_exepcted = GetOutFileContent(testOut, "as-xml-tests");
                string xml_result_actual = await ctx.AsXmlAsync(contextData);

                var result = await xmlFileCtx.Compare(xml_result_actual, xml_result_exepcted);
                result.AreEqual.ShouldBe(true, result.ToString());
            }
        }
    }
}
