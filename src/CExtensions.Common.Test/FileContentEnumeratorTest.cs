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
        public void ShouldMoveNextCorrectly()
        {
            string filePath = GetFilePath("testFile1.txt");

            using(FileContentEnumerator enumerator = new FileContentEnumerator(filePath))
            {
                enumerator.CurrentIteration.ShouldBe(0);

                if (enumerator.MoveNext()) //1
                {
                    
                    enumerator.Current.ShouldBe("first line");
                    enumerator.CurrentIteration.ShouldBe(1);
                }
                enumerator.MoveNext();//2
                enumerator.Current.ShouldBe("second line");
                enumerator.MoveNext();//3
                enumerator.MoveNext();//4
                bool moved = enumerator.MoveNext();//5
                enumerator.CurrentIteration.ShouldBe(5);
                moved.ShouldBe(true);
                moved = enumerator.MoveNext();//6
                moved.ShouldBe(false);
                enumerator.CurrentIteration.ShouldBe(0);
                moved = enumerator.MoveNext();//7
                moved.ShouldBe(false);
                enumerator.Count.ShouldBe(5);

            }

            
        }

        [Fact]
        public void ShouldBeTreatedAsEnumerable()
        {
            string filePath = GetFilePath("testFile1.txt");

            int count = 1;
            string current = null;

            var fileContents = new FileContentEnumerator(filePath);

            foreach (var item in fileContents)
            {
                current = item;
                if(count == 1)
                {
                    current.ShouldBe("first line");
                }
                count++;
            }

            current.ShouldBe(@"some other line with \n and some text after");
            count.ShouldBe(6);
            fileContents.IsOpen.ShouldBe(false);

        }
    }
}
