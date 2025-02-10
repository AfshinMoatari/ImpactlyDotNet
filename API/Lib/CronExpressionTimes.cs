using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Amazon.Util;
using API.Constants;
using Cronos;
using TimeZoneConverter;

namespace API.Lib
{

    /// <summary>
    /// The current cron language does not support skipping weeks.
    /// CronExpressionTimes is a class for this operation on the week parameter of the expression
    /// </summary>
    public class CronExpressionTimes
    {
        private CronExpression _cronExpression;
        private int _times = 1;
        private const string TimesSymbol = "X";

        public string GetNextOccurrenceISO8601DateFormat(
            DateTime fromUtc, TimeZoneInfo timeZoneInfo = null,
            bool inclusive = false, bool endOnSunday = true)
        {
            try
            {
                return GetNextOccurrence(fromUtc, timeZoneInfo, inclusive, endOnSunday)
                    .Value.ToUniversalTime().DateTime
                    .ToString(Languages.ISO8601DateFormat);
            }
            catch (InvalidOperationException e)
            {
                // TODO send email or prompt for this error
                Console.WriteLine(e);
                throw;
            }
        }

        public static CronExpressionTimes Parse(string expression)
        {
            var rgxX = new Regex(@"X\d*");
            var match = rgxX.Match(expression);

            if (!match.Success)
                return new CronExpressionTimes
                {
                    _cronExpression = CronExpression.Parse(expression)
                };

            var replace = expression.Replace(match.Value, "");
            return new CronExpressionTimes
            {
                _times = int.Parse(match.Value.Replace(TimesSymbol, "")),
                _cronExpression = CronExpression.Parse(replace)
            };
        }

        public DateTimeOffset? GetNextOccurrence(DateTime fromUtc, TimeZoneInfo timeZoneInfo = null,
            bool inclusive = false, bool endOnSunday = true)
        {
            // TODO: DEFAULT CPH, store timezone as a type in DB for each job
            timeZoneInfo ??= TZConvert.GetTimeZoneInfo("Europe/Copenhagen");

            // Case 0: no times expression
            if (_times == 1) return _cronExpression.GetNextOccurrence(fromUtc, timeZoneInfo, inclusive);

            // build all for weekSeq
            var dayOfWeeksSeq = new List<int>();
            var nextInSeq = DateTime.UtcNow;
            while (true)
            {
                var tempNextInSeq = _cronExpression.GetNextOccurrence(nextInSeq, timeZoneInfo, inclusive);
                if (tempNextInSeq != null) nextInSeq = (DateTime) tempNextInSeq;
                if (dayOfWeeksSeq.Contains((int) nextInSeq.DayOfWeek)) break;
                dayOfWeeksSeq.Add((int) nextInSeq.DayOfWeek);
            }

            if (endOnSunday) dayOfWeeksSeq = dayOfWeeksSeq.Select(dow => dow == 0 ? 7 : dow).ToList();
            dayOfWeeksSeq.Sort();

            // Case 1: if we are still going trough the current seq
            var nextOccurrence = _cronExpression.GetNextOccurrence(fromUtc, timeZoneInfo, inclusive);
            if (nextOccurrence == null) return null;

            var nexDayOfWeekOfOccurrence = (int) ((DateTime) nextOccurrence).DayOfWeek;
            var currentStepInSeq = dayOfWeeksSeq.IndexOf(nexDayOfWeekOfOccurrence);
            if (currentStepInSeq != 0) return nextOccurrence;

            // Case 2: skip x times of weeks before next run
            var skipped = fromUtc.AddDays(7 * (_times - 1));
            nextOccurrence = _cronExpression.GetNextOccurrence(skipped, timeZoneInfo, inclusive);

            return nextOccurrence;
        }
    }
}