using System;
using API.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Impactly.Test.IntegrationTests.Lib
{
    [Collection("Integration Test")]
    public class LibCronExpressionTimes
    {
        private readonly ITestOutputHelper _output;


        public LibCronExpressionTimes(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void EnsureTimesWeeks()
        {
            var now = DateTime.Now.ToUniversalTime();

            var cronExpressionTimes =
                CronExpressionTimes.Parse($"{now.Minute} {now.Hour} * * {(int) now.DayOfWeek}X2");
            
            var dateTime = (DateTimeOffset) cronExpressionTimes.GetNextOccurrence(now);
            
            var diff = Math.Ceiling((dateTime - now.ToUniversalTime()).TotalDays);
            
            Assert.Equal(14, diff);
        }
    }
}