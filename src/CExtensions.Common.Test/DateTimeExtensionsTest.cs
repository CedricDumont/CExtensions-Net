using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;

using Shouldly;

namespace CExtensions.Common.Test
{
    public class DateTimeExtensionsTest
    {
        [Theory]
        [InlineData(2015,1,1,1)]
        [InlineData(2015, 1, 8, 2)]
        [InlineData(2015, 12, 31, 53)]
        [InlineData(1, 12, 31, 53)]
        [InlineData(9999, 12, 31, 53)]
        public void ShouldReturnCorrectWeek(int year, int month, int day, int expectedWeekNumber)
        {
            DateTime dt = new DateTime(year, month, day);

            dt.WeekOfYear().ShouldBe(expectedWeekNumber);
        }
    }
}
