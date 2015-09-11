using CExtensions.Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Helper;
using Xunit;
using Shouldly;

namespace CExtensions.Common.Test
{
    public class FileContentEnumeratorTest : BaseUnitTest
    {
        [Fact]
        public void ShouldReturnCorrectLineNumbers()
        {
            string filePath = GetFilePath("testFile1.txt");

            using(FileContentEnumerator enumerator = new FileContentEnumerator(filePath))
            {
                if (enumerator.MoveNext())
                {
                    enumerator.Current.ShouldBe("first line");
                }
                enumerator.MoveNext();
                enumerator.MoveNext();
                enumerator.MoveNext();
                bool moved = enumerator.MoveNext();
                moved.ShouldBe(true);
                moved = enumerator.MoveNext();
                moved.ShouldBe(false);

            }

            
        }
    }
}
