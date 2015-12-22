using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using CExtensions.SqlCE;
using CExtensions.Test.Model;
using System.Linq;
using Shouldly;
using System.Diagnostics;

namespace CExtensions.SqlCE.Test
{
    public class ToDbContextTest
    {
        [Fact]
        public void TestMethod1()
        {
            string _rootPath = this.GetType().AssemblyDirectory() + "\\input\\test1_in.xml";

            SampleContext result = _rootPath.ToDbContext<SampleContext>("in");

            result.Authors.ToList().Count().ShouldBe(3);
        }

      
    }
}
