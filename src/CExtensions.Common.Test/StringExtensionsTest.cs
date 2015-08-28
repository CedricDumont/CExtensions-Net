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
    }
}
