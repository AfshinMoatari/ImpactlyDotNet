using System;
using System.Globalization;
using System.Threading.Tasks;
using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace API.Handlers
{
    public interface ITimeMachineHandler
    {
        public Task<string> SetCultureInfoByProjectId(HttpRequest request, string projectId);
        public DateTime GetThisWeekStart();
        public DateTime GetThisWeekEnd();
        public DateTime GetPreviousWeekStart();
        public DateTime GetPreviousWeekEnd();
        public DateTime GetThisMonthStart();
        public DateTime GetThisMonthEnd();
        public DateTime GetPreviousMonthStart();
        public DateTime GetPreviousMonthEnd();
        public DateTime GetThisYearStart();
        public DateTime GetThisYearEnd();
        public DateTime GetPreviousYearStart();
        public DateTime GetPreviousYearEnd();
        public int GetCurrentQuarter();
        public int GetPreviousQuarter();
        public DateTime GetCurrentQuarterStart();
        public DateTime GetCurrentQuarterEnd();
        public DateTime GetPreviousQuarterStart();
        public DateTime GetPreviousQuarterEnd();
        public int GetYearlyTotalWeeksOfADate(DateTime date);
        public int GetStartWeekOfDate(DateTime date);
        public DayOfWeek GetFirstDayOfWeek();
        public int GetTheMiddleWeekOfAYear(DateTime date);
        public int GetFirstWeekofADate(DateTime date);
    }

    public class TimeMachineHandler : ITimeMachineHandler
    {
        private readonly IProjectService _projectService;
        private readonly CultureInfo currentCulture = CultureInfo.CurrentCulture;
        private readonly DateTime baseDate = DateTime.Today; 

        public TimeMachineHandler(IProjectService projectService)
        {
            _projectService = projectService;
        }

        public async Task<string> SetCultureInfoByProjectId(HttpRequest request, string projectId)
        {
            var checkProjectLanguage = await _projectService.GetProjectCultureByProjectId(request, projectId);
            if (!checkProjectLanguage.IsNullOrEmpty())
            {
                var cultureInfo = new CultureInfo(checkProjectLanguage);
                CultureInfo.CurrentCulture = cultureInfo;
                return checkProjectLanguage;
            }
            else
            {
                return null;
            }
        }

        //Weeks
        public DateTime GetThisWeekStart()
        {
            return baseDate.AddDays(-1 * ((7 + ((baseDate.DayOfWeek) - (currentCulture.DateTimeFormat.FirstDayOfWeek))) % 7)).Date;
        }
        public DateTime GetThisWeekEnd()
        {
            return GetThisWeekStart().AddDays(7).AddSeconds(-1);
        }
        public DateTime GetPreviousWeekStart()
        {
            return GetThisWeekStart().AddDays(-7);
        }
        public DateTime GetPreviousWeekEnd()
        {
            return GetThisWeekStart().AddSeconds(-1);
        }
        public int GetYearlyTotalWeeksOfADate(DateTime date)
        {
            return currentCulture
                    .Calendar
                    .GetWeekOfYear(
                        date,
                        CalendarWeekRule.FirstFullWeek,
                        DayOfWeek.Monday);
        }
        public int GetStartWeekOfDate(DateTime date)
        {
            return currentCulture
                    .Calendar
                    .GetWeekOfYear(
                        date,
                        CalendarWeekRule.FirstFullWeek,
                        DayOfWeek.Monday);  
        }
        public DayOfWeek GetFirstDayOfWeek()
        {
            return currentCulture.DateTimeFormat.FirstDayOfWeek;
        }
        public int GetTheMiddleWeekOfAYear(DateTime date)
        {
            return currentCulture.Calendar.GetWeekOfYear(date, currentCulture.DateTimeFormat.CalendarWeekRule, currentCulture.DateTimeFormat.FirstDayOfWeek);
        }
        public int GetFirstWeekofADate(DateTime date)
        {
           return currentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        //Months
        public DateTime GetThisMonthStart()
        {
            return baseDate.AddDays(1 - baseDate.Day);
        }
        public DateTime GetThisMonthEnd()
        {
            return GetThisMonthStart().AddMonths(1).AddSeconds(-1);
        }
        public DateTime GetPreviousMonthStart()
        {
            var prevMonthMonth = GetThisMonthStart().Month.Equals(1) ? 11 : -1;
            var prevMonthYear = GetThisMonthStart().Month.Equals(1) ? -1 : 0;
            return GetThisMonthStart().AddMonths(prevMonthMonth).AddYears(prevMonthYear);
        }
        public DateTime GetPreviousMonthEnd()
        {
            return GetThisMonthStart().AddSeconds(-1);
        }

        //Years
        public DateTime GetThisYearStart()
        {
            return new DateTime(baseDate.Year, 1, 1);
        }
        public DateTime GetThisYearEnd()
        {
            return new DateTime(baseDate.Year, 12, 31);
        }
        public DateTime GetPreviousYearStart()
        {
            return new DateTime(baseDate.Year - 1, 1, 1);
        }
        public DateTime GetPreviousYearEnd()
        {
            return new DateTime(GetPreviousYearStart().Year, 12, 31);
        }
        
        //Quarter
        public int GetCurrentQuarter()
        {
            return (baseDate.Month - 1) / 3 + 1;
        }
        public int GetPreviousQuarter()
        {
            return (GetCurrentQuarter() - 1);
        }
        public DateTime GetCurrentQuarterStart()
        {
            return new DateTime(baseDate.Year, Math.Abs(3 * GetCurrentQuarter() - 2) , 1);
        }
        public DateTime GetCurrentQuarterEnd()
        {
            int currentQuarter = GetCurrentQuarter();
            return (currentQuarter.Equals(4) ? GetThisYearEnd() : new DateTime(baseDate.Year, 3 * currentQuarter + 1, 1).AddDays(-1));
        }
        public DateTime GetPreviousQuarterStart()
        {
            var prevQuarter = GetPreviousQuarter().Equals(0) ? 4 : GetPreviousQuarter();
            var prevYear = GetPreviousQuarter().Equals(0) ? GetPreviousYearStart().Year : baseDate.Year;
            return new DateTime(prevYear, 3 * prevQuarter - 2, 1);
        }
        public DateTime GetPreviousQuarterEnd()
        {
            return GetCurrentQuarterStart().AddDays(-1);
        }
    }
}