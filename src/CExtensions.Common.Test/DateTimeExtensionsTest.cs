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

        [Theory]
        [InlineData(2015, 1, 1, 1)]
        [InlineData(2015, 2, 1, 1)]
        [InlineData(2015, 3, 1, 1)]
        [InlineData(2015, 4, 1, 2)]
        [InlineData(2015, 5, 1, 2)]
        [InlineData(2015, 6, 1, 2)]
        [InlineData(2015, 7, 1, 3)]
        [InlineData(2015, 8, 1, 3)]
        [InlineData(2015, 9, 1, 3)]
        [InlineData(2015, 10, 1, 4)]
        [InlineData(2015, 11, 1, 4)]
        [InlineData(2015, 12, 1, 4)]
        [InlineData(2015, 12, 31, 4)]
        [InlineData(1, 12, 31, 4)]
        [InlineData(9999, 12, 31, 4)]
        public void ShouldReturnCorrectQuarter(int year, int month, int day, int expectedQuarter)
        {
            DateTime dt = new DateTime(year, month, day);

            int quarter = dt.Quarter();

            quarter.ShouldBe(expectedQuarter);
        }
    }
}
