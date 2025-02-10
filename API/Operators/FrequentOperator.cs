using API.Constants;
using API.Handlers;
using API.Models.Analytics;
using API.Models.Projects;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API.Repositories;
using API.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API.Operators
{
    public interface IFrequencyOperator
    {
        public Task<ChartData> GetFrequentRegs(string projectId, string strategyId, string effectId, string period, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod, ISystemMessage message);
    }

    public class Daily : IFrequencyOperator
    {
        private readonly IPeriodicOperationContext _periodicOperatorContext;
        private readonly IAnalyticsService _analyticsService;

        public Daily(IPeriodicOperationContext periodicOperatorContext, IAnalyticsService analyticsService)
        {
            _periodicOperatorContext = periodicOperatorContext;
            _analyticsService = analyticsService;
        }

        public async Task<ChartData> GetFrequentRegs(string projectId, string strategyId, string effectId, string period, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod, ISystemMessage message)
        {
            var periodRegs = await _periodicOperatorContext.GetPeriodicRegs(period, projectId, strategyId, effectId, tags, type, startPeriod, endPeriod);
            var periodDateCombo = _periodicOperatorContext.GetPeriodicDate(period);
            var startDate = (startPeriod.HasValue && period == "custom") ? startPeriod.Value : periodDateCombo.startPeriod;
            var endDate = (endPeriod.HasValue && period == "custom") ? endPeriod.Value : periodDateCombo.endPeriod;

            var chartData = new ChartData();
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var regs = periodRegs.Where(x => x.Date.Date == date.Date).ToList();
                var sampleSize = regs.Select(y => y.PatientId).Distinct().Count();
                double value = 0;

                if (type == "COUNT")
                {
                    value += regs.Count();
                }
                else if(type == "NUMERIC")
                {
                    var sum = regs.Count() == 0 ? 1 : regs.Count();
                    var subValue = regs.Sum(y => y.Value);
                    value = Math.Round(subValue / sum, 2);
                }

                var keyValue = message.ReportResponseKeys("Daily", date, null, null, null, null);
                var periodValue = message.ReportResponsePeriod(period, startPeriod, endPeriod);
                var periodPopulation = await _analyticsService.GetActivePatientsCount(strategyId, tags, null, date);
                chartData.ChartValues.Add(
                    new Dictionary<string, object> {
                        { "Name", keyValue },
                        { periodValue, value }
                    });
                chartData.SampleSizes.Add(keyValue, regs.Count());
                chartData.PopulationSizes.Add(keyValue, periodPopulation);
            }

            return chartData;
        }
    }
    public class Weekly : IFrequencyOperator
    {
        private readonly IPeriodicOperationContext _periodicOperatorContext;
        private readonly ITimeMachineHandler _timeMachineHandler;
        private readonly IAnalyticsService _analyticsService;
        
        public Weekly(IPeriodicOperationContext periodicOperatorContext, ITimeMachineHandler timeMachineHandler, IAnalyticsService analyticsService)
        {
            _periodicOperatorContext = periodicOperatorContext;
            _timeMachineHandler = timeMachineHandler;
            _analyticsService = analyticsService;
        }

        public async Task<ChartData> GetFrequentRegs(string projectId, string strategyId, string effectId, string period, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod, ISystemMessage message)
        {
            var periodRegs = await _periodicOperatorContext.GetPeriodicRegs(period, projectId, strategyId, effectId, tags, type, startPeriod, endPeriod);
            var periodDateCombo = _periodicOperatorContext.GetPeriodicDate(period);
            var startDate = (startPeriod.HasValue && period == "custom") ? startPeriod.Value : periodDateCombo.startPeriod;
            var endDate = (endPeriod.HasValue && period == "custom") ? endPeriod.Value : periodDateCombo.endPeriod;

            var chartData = new ChartData();
            for (int year = startDate.Year; year <= endDate.Year; year++)
            {
                //maybe this should be refactored
                DateTime currentDate = new DateTime(year, 1, 1);
                var totalWeekNo = _timeMachineHandler.GetYearlyTotalWeeksOfADate(new DateTime(year, 12, 31));
                var startWeekNo = 1;
                if (startDate.Year == endDate.Year && period != "ThisYear" && period != "LastYear")
                {
                    totalWeekNo = _timeMachineHandler.GetYearlyTotalWeeksOfADate(endDate);
                    if (startDate == currentDate)
                    {
                        while (currentDate.DayOfWeek != DayOfWeek.Monday)
                        {
                            currentDate = currentDate.AddDays(1);
                        }
                        startWeekNo = _timeMachineHandler.GetStartWeekOfDate(currentDate);
                    }
                    else
                    {
                        startWeekNo = _timeMachineHandler.GetStartWeekOfDate(startDate);
                    }
                }
                if (period != "ThisWeek" && period != "LastWeek" && period != "ThisMonth" && period != "LastMonth" && period != "ThisQuarter" && period != "LastQuarter" && period != "ThisYear" && period != "LastYear")
                {
                    if (startDate.Year != year)
                    {
                        startWeekNo = 1;
                        if (startDate.Year != endDate.Year)
                        {
                            if (endDate == currentDate)
                            {
                                while (currentDate.DayOfWeek != DayOfWeek.Monday)
                                {
                                    currentDate = currentDate.AddDays(1);
                                }
                                totalWeekNo = _timeMachineHandler.GetYearlyTotalWeeksOfADate(currentDate);
                            }
                            else if (startDate.Year != endDate.Year)
                            {
                                if (endDate.Year == year)
                                {
                                    totalWeekNo = _timeMachineHandler.GetYearlyTotalWeeksOfADate(endDate);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (startDate == currentDate)
                        {
                            while (currentDate.DayOfWeek != DayOfWeek.Monday)
                            {
                                currentDate = currentDate.AddDays(1);
                            }
                            startWeekNo = _timeMachineHandler.GetYearlyTotalWeeksOfADate(currentDate);
                            if (startDate.Year != year)
                            {
                                totalWeekNo = _timeMachineHandler.GetYearlyTotalWeeksOfADate(new DateTime(year, 1, 1));
                            }
                            else if (startDate.Year == endDate.Year)
                            {
                                totalWeekNo = _timeMachineHandler.GetYearlyTotalWeeksOfADate(endDate);
                            }
                        }
                        else
                        {
                            startWeekNo = _timeMachineHandler.GetYearlyTotalWeeksOfADate(startDate);
                            totalWeekNo = _timeMachineHandler.GetYearlyTotalWeeksOfADate(endDate);

                        }
                    }

                }
                //

                var midYear = new DateTime(year, 06, 15);
                while (midYear.DayOfWeek != _timeMachineHandler.GetFirstDayOfWeek())
                    midYear = midYear.AddDays(-1);
                int midWeekNr = _timeMachineHandler.GetYearlyTotalWeeksOfADate(midYear);
                var frequencyStartDate = midYear.AddDays(7 - (midWeekNr * 7));
                var frequencyEndDate = frequencyStartDate.AddDays(14);
                for (var currWeekNr = startWeekNo; currWeekNr <= totalWeekNo; currWeekNr++, frequencyStartDate = frequencyStartDate.AddDays(7))
                {
                    //maybe this should be refactored
                    DateTime jan1 = new DateTime(year, 1, 1);
                    int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;
                    DateTime firstThursday = jan1.AddDays(daysOffset);
                    int firstWeek = _timeMachineHandler.GetFirstWeekofADate(firstThursday);
                    var weekNum = currWeekNr;
                    if (firstWeek == 1)
                    {
                        weekNum -= 1;
                    }
                    var result = firstThursday.AddDays(weekNum * 7);
                    var firstday = result.AddDays(-3);
                    //

                    var smpleSizeBucket = new List<string>();

                    frequencyEndDate = frequencyStartDate.AddDays(6);
                    var regs = periodRegs.Where(x => x.Date <= firstday.AddDays(7) && x.Date >= firstday).ToList();
                    double value = 0;

                    if (type == "COUNT")
                    {
                        value = regs.Count();
                    }
                    else if (type == "NUMERIC")
                    {
                        var sum = regs.Count() == 0 ? 1 : regs.Count();
                        var subValue = regs.Sum(y => y.Value);
                        value = Math.Round(subValue / sum, 2);
                    }

                    var sampleSize = regs.Select(y => y.PatientId).Distinct().ToList();
                    var periodPopulation = await _analyticsService.GetActivePatientsCount(strategyId, tags, null, frequencyEndDate);
                    foreach (var sample in sampleSize)
                    {
                        smpleSizeBucket.Add(sample);
                    }

                    var keyValue = message.ReportResponseKeys("Weekly", null , year, currWeekNr, null, null);
                    var periodValue = message.ReportResponsePeriod(period, startPeriod, endPeriod);

                    chartData.ChartValues.Add(
                        new Dictionary<string, object> {
                        { "Name", keyValue },
                            { periodValue, value}
                        });
                    chartData.SampleSizes.Add(keyValue, regs.Count());
                    chartData.PopulationSizes.Add(keyValue, periodPopulation);
                }
            }

            return chartData;
        }
    }
    public class Monthly : IFrequencyOperator
    {
        private readonly IPeriodicOperationContext _periodicOperatorContext;
        private readonly IAnalyticsService _analyticsService;

        public Monthly(IPeriodicOperationContext periodicOperatorContext, IAnalyticsService analyticsService)
        {
            _periodicOperatorContext = periodicOperatorContext;
            _analyticsService = analyticsService;
        }

        public async Task<ChartData> GetFrequentRegs(string projectId, string strategyId, string effectId, string period, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod, ISystemMessage message)
        {
            var periodRegs = await _periodicOperatorContext.GetPeriodicRegs(period, projectId, strategyId, effectId, tags, type, startPeriod, endPeriod);
            var periodDateCombo = _periodicOperatorContext.GetPeriodicDate(period);
            var startDate = (startPeriod.HasValue && period == "custom") ? startPeriod.Value : periodDateCombo.startPeriod;
            var endDate = (endPeriod.HasValue && period == "custom") ? endPeriod.Value : periodDateCombo.endPeriod;

            var chartData = new ChartData();

            for (int year = startDate.Year; year <= endDate.Year; year++)
            {

                var frequencyStartDate = startDate;
                var frequencyEndDate = startDate.AddMonths(1);
                var startMonthNr = startDate.Month;
                var endDateMonthNr = 12;
                if (startDate.Year == endDate.Year)
                {
                    endDateMonthNr = endDate.Month;
                }
                for (var currMonthNr = startMonthNr; currMonthNr <= endDateMonthNr && startDate.Year <= endDate.Year; currMonthNr++, frequencyStartDate = frequencyStartDate.AddMonths(1))
                {
                    var smpleSizeBucket = new List<string>();
                    int daysInMonth = DateTime.DaysInMonth(year: frequencyStartDate.Year, month: frequencyStartDate.Month);
                    frequencyEndDate = new DateTime(frequencyStartDate.Year, frequencyStartDate.Month, daysInMonth);
                    var weeklySampleSizes = new List<string>();
                    var regs = periodRegs.Where(x => x.Date.Year == year).Where(x => x.Date.Month <= frequencyEndDate.Month && x.Date.Month >= frequencyStartDate.Month).ToList();
                    var sampleSize = regs.Select(y => y.PatientId).Distinct().ToList();

                    double value = 0;

                    if (type == "COUNT")
                    {
                        value = regs.Count();
                    }
                    else if (type == "NUMERIC")
                    {
                        var sum = regs.Count() == 0 ? 1 : regs.Count();
                        var subValue = regs.Sum(y => y.Value);
                        value = Math.Round(subValue / sum, 2);
                    }

                    foreach (var sample in sampleSize)
                    {
                        smpleSizeBucket.Add(sample);
                    }

                    var keyValue = message.ReportResponseKeys("Monthly", null, year, null, currMonthNr, null);
                    var periodValue = message.ReportResponsePeriod(period, startPeriod, endPeriod);
                    var population =
                        await _analyticsService.GetActivePatientsCount(strategyId, tags, null, frequencyEndDate);
                    chartData.ChartValues.Add(
                        new Dictionary<string, object> {
                            { "Name", keyValue },
                            { periodValue, value}
                        });
                    chartData.SampleSizes.Add(keyValue, regs.Count());
                    chartData.PopulationSizes.Add(keyValue, population);
                    if (frequencyStartDate.Month == 12 && startDate.Year < endDate.Year)
                    {
                        startDate = new DateTime(year + 1, 1, 1);
                        break;
                    }
                }
            }

            return chartData;
        }
    }
    public class Quarterly : IFrequencyOperator
    {
        private readonly IPeriodicOperationContext _periodicOperatorContext;
        private readonly IAnalyticsService _analyticsService;

        public Quarterly(IPeriodicOperationContext periodicOperatorContext, IAnalyticsService analyticsService)
        {
            _periodicOperatorContext = periodicOperatorContext;
            _analyticsService = analyticsService;
        }

        public async Task<ChartData> GetFrequentRegs(string projectId, string strategyId, string effectId, string period, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod, ISystemMessage message)
        {
            var periodRegs = await _periodicOperatorContext.GetPeriodicRegs(period, projectId, strategyId, effectId, tags, type, startPeriod, endPeriod);
            var periodDateCombo = _periodicOperatorContext.GetPeriodicDate(period);
            var startDate = (startPeriod.HasValue && period == "custom") ? startPeriod.Value : periodDateCombo.startPeriod;
            var endDate = (endPeriod.HasValue && period == "custom") ? endPeriod.Value : periodDateCombo.endPeriod;
            var chartData = new ChartData();

            for (int year = startDate.Year; year <= endDate.Year; year++)
            {
                var startQuarterNr = Convert.ToInt16((startDate.Month - 1) / 3) + 1;
                var freqStartMonth = 3 * startQuarterNr - 2;
                var freqEndMonth = 3 * startQuarterNr + 1;
                if (freqEndMonth > 12) freqEndMonth = 12;
                var frequencyStartDate = new DateTime(year, freqStartMonth, 1);
                var frequencyEndDate = new DateTime(year, freqEndMonth, 1).AddDays(-1);
                var endDateQuarterNr = 4;
                if (startDate.Year == endDate.Year)
                {
                    endDateQuarterNr = Convert.ToInt16((endDate.Month - 1) / 3) + 1;
                }
                for (var currQuarterNr = startQuarterNr; currQuarterNr <= endDateQuarterNr && startDate.Year <= endDate.Year; currQuarterNr++, frequencyStartDate = frequencyStartDate.AddMonths(3))
                {
                    var smpleSizeBucket = new List<string>();
                    frequencyEndDate = frequencyStartDate.AddMonths(3).AddSeconds(-1);
                    var regs = periodRegs.Where(x => x.Date.Year == year).Where(x => x.Date.Month <= frequencyEndDate.Month && x.Date.Month >= frequencyStartDate.Month).ToList();
                    var sampleSize = regs.Select(y => y.PatientId).Distinct().ToList();
                    double value = 0;

                    if (type == "COUNT")
                    {
                        value = regs.Count();
                    }
                    else if (type == "NUMERIC")
                    {
                        var sum = regs.Count() == 0 ? 1 : regs.Count();
                        var subValue = regs.Sum(y => y.Value);
                        value = Math.Round(subValue / sum, 2);
                    }

                    foreach (var sample in sampleSize)
                    {
                        smpleSizeBucket.Add(sample);
                    }

                    var keyValue = message.ReportResponseKeys("Quarterly", null, year, null, null, currQuarterNr);
                    var periodValue = message.ReportResponsePeriod(period, startPeriod, endPeriod);
                    var population =
                        await _analyticsService.GetActivePatientsCount(strategyId, tags, null, frequencyEndDate);
                    chartData.ChartValues.Add(
                        new Dictionary<string, object> {
                            { "Name", keyValue },
                            { periodValue, value}
                        });
                    chartData.SampleSizes.Add(keyValue, regs.Count());
                    chartData.PopulationSizes.Add(keyValue, population);
                    if (startDate.Year < endDate.Year && frequencyEndDate.Month == 12)
                    {
                        startDate = new DateTime(year + 1, 1, 1);
                        break;
                    }
                }
            }

            return chartData;
        }
    }
    public class Annual : IFrequencyOperator
    {
        private readonly IPeriodicOperationContext _periodicOperatorContext;
        private readonly IAnalyticsService _analyticsService;

        public Annual(IPeriodicOperationContext periodicOperatorContext, IAnalyticsService analyticsService)
        {
            _periodicOperatorContext = periodicOperatorContext;
            _analyticsService = analyticsService;
        }

        public async Task<ChartData> GetFrequentRegs(string projectId, string strategyId, string effectId, string period, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod, ISystemMessage message)
        {
            var periodRegs = await _periodicOperatorContext.GetPeriodicRegs(period, projectId, strategyId, effectId, tags, type, startPeriod, endPeriod);
            var periodDateCombo = _periodicOperatorContext.GetPeriodicDate(period);
            var startDate = (startPeriod.HasValue && period == "custom") ? startPeriod.Value : periodDateCombo.startPeriod;
            var endDate = (endPeriod.HasValue && period == "custom") ? endPeriod.Value : periodDateCombo.endPeriod;
            var chartData = new ChartData();

            for (int year = startDate.Year; year <= endDate.Year; year++)
            {
                var smpleSizeBucket = new List<string>();
                var regs = periodRegs.Where(x => x.Date.Year == year).ToList();
                var sampleSize = regs.Select(y => y.PatientId).Distinct().ToList();

                double value = 0;

                if (type == "COUNT")
                {
                    value = regs.Count();
                }
                else if (type == "NUMERIC")
                {
                    var sum = regs.Count() == 0 ? 1 : regs.Count();
                    var subValue = regs.Sum(y => y.Value);
                    value = Math.Round(subValue / sum, 2);
                }

                foreach (var sample in sampleSize)
                {
                    smpleSizeBucket.Add(sample);
                }

                var keyValue = message.ReportResponseKeys("Annual", null, year, null, null, null);
                var periodValue = message.ReportResponsePeriod(period, startPeriod, endPeriod);
                var population = await _analyticsService.GetActivePatientsCount(strategyId, tags, null, startDate.AddYears(year));
                chartData.ChartValues.Add(
                    new Dictionary<string, object> {
                        { "Name", keyValue },
                        { periodValue, value}
                    });
                chartData.SampleSizes.Add(keyValue, regs.Count());
                chartData.PopulationSizes.Add(keyValue, population);
            }

            return chartData;
        }
    }


    public interface IFrequencyOperatorContext
    {
        public Task<ChartData> GetFrequentRegs(string searchType, string projectId, string strategyId, string effectId, string period, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod, ISystemMessage message);
    }

    public class FrequentOperationContext : IFrequencyOperatorContext
    {
        private readonly Dictionary<string, IFrequencyOperator> _frequentOperator = new Dictionary<string, IFrequencyOperator>();
        
        public FrequentOperationContext(IPeriodicOperationContext periodicOperatorContext, ITimeMachineHandler timeMachineHandler, IAnalyticsService analyticsService)
        {
            _frequentOperator.Add("Daily", new Daily(periodicOperatorContext, analyticsService));
            _frequentOperator.Add("Weekly", new Weekly(periodicOperatorContext, timeMachineHandler, analyticsService));
            _frequentOperator.Add("Monthly", new Monthly(periodicOperatorContext, analyticsService));
            _frequentOperator.Add("Quarterly", new Quarterly(periodicOperatorContext, analyticsService));
            _frequentOperator.Add("Annual", new Annual(periodicOperatorContext, analyticsService));
        }

        public async Task<ChartData> GetFrequentRegs(string searchType, string projectId, string strategyId, string effectId, string period, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod, ISystemMessage message)
        {
            return await _frequentOperator[searchType].GetFrequentRegs(projectId, strategyId, effectId, period, tags, type, startPeriod, endPeriod, message);
        }
    }
}
