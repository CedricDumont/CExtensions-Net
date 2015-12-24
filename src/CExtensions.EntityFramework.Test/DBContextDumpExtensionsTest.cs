using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;
using Shouldly;
using CExtensions.Test.Model;
using CExtensions.EntityFramework;
using CExtensions.Effort;
using System.Threading.Tasks;
using System.Linq;

namespace CExtensions.EntityFramework.Test
{
    public class DBContextDumpExtensionsTest
    {

        [Fact]
        public void ShouldGetoriginalValues()
        {
            using (SampleContext context = new XmlFileContext<SampleContext>(this.GetType()).Empty())
            {
            }
        }

    }
}
