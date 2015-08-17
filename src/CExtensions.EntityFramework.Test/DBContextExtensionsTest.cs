using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Shouldly;
using CExtensions.Effort.SampleApp;
using CExtensions.EF;
using CExtensions.Effort;

namespace CExtensions.EntityFramework.Test
{
    public class DBContextExtensionsTest
    {
        private string _modelNamespace = "CExtensions.Effort.SampleApp";

        [Fact]
        public void TestMethod1()
        {
            using (SampleContext context = new XmlFileContext<SampleContext>(this.GetType()).Empty())
            {
                var result = context.DbSets(_modelNamespace);

                result.Count.ShouldBe(2);
            }
        }
    }
}
