﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using CExtensions.Effort;
using Xunit;
using CExtensions.Effort.SampleApp;
using CExtensions.EF;
using System.Xml;
using Shouldly;

namespace CExtensions.Test
{
    public class AsXmlAsyncExtensionTest
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

            SampleContext ctx = xmlFileCtx.InputContext("test1");

            using (Service svc = new Service(ctx))
            {
                string fromFile = await ctx.AsXmlAsync("CExtensions.Effort.SampleApp", ContextDataEnum.All);
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

                string fromFile = await emptyContext.AsXmlAsync("CExtensions.Effort.SampleApp", ContextDataEnum.Local, includeNull: true);

                dynamic obj = fromFile.XmlToDynamic();

                ((string)obj.AUTHOR[0].AUT_FIRSTNAME).ShouldBe("testname");
            }
        }
    }
}