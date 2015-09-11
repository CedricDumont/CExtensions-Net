using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Helper;
using Xunit;
using Shouldly;

namespace CExtensions.Common.Test
{
    public class FileExtensionsTest : BaseUnitTest
    {
        [Fact]
        public void ShouldReturnCorrectLineNumbers()
        {
            string filePath = GetFilePath("testFile1.txt");

            new FileInfo(filePath).CountLines().ShouldBe(5);
        }


    }
}
