using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;

using Shouldly;

namespace CExtensions.Common.Test
{
    public class StringExtensionsTest
    {
        [Theory]
        [InlineData("someValue", 2,2)]
        [InlineData("someValue", 11,9)]
        [InlineData("someValue", -1, 0)]
        [InlineData(null, 10, 0)]
        public void ShouldLimitString(String s, int limit, int result)
        {
            s = s.Limit(limit);

            if (s != null)
            {
                s.Length.ShouldBe(result);
            }
        }

        [Theory]
        [InlineData("someValue", 2, "so")]
        [InlineData("someValue", 1, "s")]
        [InlineData("someValue", 0, "")]
        [InlineData("someValue", -1, "")]
        [InlineData("someValue", -50, "")]
        [InlineData("someValue", 50, "someValue")]   
        public void ShouldKeepLeftPartOfString(String s, int val, string result)
        {
            s = s.Left(val);

            if (s != null)
            {
                s.ShouldBe(result);
            }
        }

        [Theory]
        [InlineData("someValue", 2, "ue")]
        [InlineData("someValue", 1, "e")]
        [InlineData("someValue", 0, "")]
        [InlineData("someValue", -1, "")]
        [InlineData("someValue", -50, "")]
        [InlineData("someValue", 50, "someValue")]
        public void ShouldKeepRightPartOfString(String s, int val, string result)
        {
            s = s.Right(val);

            if (s != null)
            {
                s.ShouldBe(result);
            }
        }
    }
}
