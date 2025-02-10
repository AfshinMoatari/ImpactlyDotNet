using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Constants;
using API.Models.Analytics;
using API.Models.Projects;
using API.Models.Reports;
using API.Models.Strategy;
using API.Operators;
using Microsoft.IdentityModel.Tokens;
using static API.Models.Analytics.PointSystemTypeEnum;
using static API.Models.Reports.ReportModuleConfig;

namespace API.Services
{
    public interface IReportService
    {
        /// <summary>
        /// Get the custom survey data by the list of choices.
        /// </summary>
        /// <returns>A list of chart data for custom survey based on survey field chices.</returns>
        public Task<ChartData> GetCustomSurveyChartData(ReportModuleConfig reportModuleConfig, 
            IEnumerable<ProjectPatient> population);
        /// <summary>
        /// Get the validated survey data.
        /// </summary>
        /// <returns>A list of chart data for validated survey.</returns>
        public Task<ChartData> GetValidatedSurveyChartData(ReportModuleConfig reportModuleConfig, 
            IEnumerable<ProjectPatient> population);

        public Task<ChartData> GetSurveyChartMultipleQuestions(ReportModuleConfig reportModuleConfig, IEnumerable<ProjectPatient> population);
        
        public Task<IEnumerable<ProjectReportStats>> GetSurveyMultipleStats(ReportModuleConfig reportModuleConfig, IEnumerable<ProjectPatient> population);

        /// <summary>
        /// Get the population size of strategy.
        /// </summary>
        /// <returns>A population size of a strategy.</returns>
        public Task<IEnumerable<ProjectPatient>> GetPopulationByStrategyIdAndTags(string strategyId, List<ProjectTag> tags, List<string> filters);
        /// <summary>
        /// Get the status registeration chart data.
        /// </summary>
        /// <returns>A chart data for status registeration.</returns>
        public Task<ChartData> GetStatusRegChartData(string projectId, string strategyId, string category, List<DateTime?> endDates, PointSystemType pointSystemType, List<ProjectTag> tags);
        /// <summary>
        /// Get the numerical registeration chart data.
        /// </summary>
        /// <returns>A chart data for numerical registeration.</returns>
        public Task<ChartData> GetNumericRegChartData(string effectId, string projectId, string strategyId, List<ProjectTag> tags, string frequency, string period, DateTime? start, DateTime? end, ISystemMessage message, bool isEmpty);
        /// <summary>
        /// Get the incident registeration chart data.
        /// </summary>
        /// <returns>A chart data for incident registeration.</returns>
        public Task<ChartData> GetIncidentRegChartData(string effectId, string projectId, string strategyId, List<ProjectTag> tags, string frequency, string period, DateTime? start, DateTime? end, ISystemMessage message, bool isEmpty);


    }

    /// <summary>
    /// Service class for managing surveys.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ISurveyService _surveyService;
        private readonly IStrategyService _strategyService;
        private readonly IFrequencyOperatorContext _frequencyOperatorContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportService"/> class.
        /// </summary>
        /// <param name="analyticsService">The _analyticsService.</param>
        /// <param name="surveyService">The _surveyService.</param>
        /// <param name="strategyService">The _strategyService.</param>
        /// <param name="frequencyOperatorContext">The _frequencyOperatorContext.</param>
        public ReportService(
            IAnalyticsService analyticsService, 
            ISurveyService surveyService, 
            IStrategyService strategyService, 
            IFrequencyOperatorContext frequencyOperatorContext)
        {
            _analyticsService = analyticsService;
            _surveyService = surveyService;
            _strategyService = strategyService;
            _frequencyOperatorContext = frequencyOperatorContext;
        }

        public async Task<ChartData> GetCustomSurveyChartData(ReportModuleConfig reportModuleConfig,   
            IEnumerable<ProjectPatient> population)
        {
            var choices = await _surveyService.GetFieldChoicesByFieldId(reportModuleConfig.FieldId);
            var projectPatients = population.ToList();
            ChartData chartData;
            if (reportModuleConfig.XAxisDataType is XAxisDataTypeChoices)
            {
                chartData = await GetCustomSurveyDataChoices(reportModuleConfig, choices, projectPatients);
            }
            else
            {
                chartData = await GetCustomSurveyDataPeriods(reportModuleConfig, projectPatients);
            }
    
            return chartData;
        }

        private async Task<ChartData> GetCustomSurveyDataChoices(ReportModuleConfig reportModuleConfig,
            IEnumerable<FieldChoice> choices, IEnumerable<ProjectPatient> patients)
        {
            var chartData = new ChartData();
            var datesRanges = reportModuleConfig.dateRanges;
            var patientIds = patients.Select(p => p.Id);
            var entriesAll = await _analyticsService.GetCustomFieldEntriesAllByStrategyIdAndSurveyId(new SurveyAccess()
            {
                ProjectId = reportModuleConfig.ProjectId,
                StrategyId = reportModuleConfig.StrategyId,
                SurveyId = reportModuleConfig.SurveyId,
            }, reportModuleConfig.Tags);
            foreach (var choice in choices.OrderBy(x => x.Index))
            {
                chartData.SampleSizes = new Dictionary<string, int>();
                chartData.PopulationSizes = new Dictionary<string, int>();
                var chartValues = new Dictionary<string, object> { { "Name", choice.Text } };
                for (int i = 0; i < reportModuleConfig.dateRanges.Count(); i++)
                {

                    var entries = entriesAll.Where(f => f.AnsweredAt <= GetEndDate(datesRanges[i].end.Value.Date) &&
                                                        f.AnsweredAt >= GetStartDate(datesRanges[i].start.Value.Date));
                    
                    var fields = entries.Where(r => r.FieldId == reportModuleConfig.FieldId 
                                                    && FieldEntryInPopulation(r, patientIds)).ToList();

                    
                    var decoratedDateRange = DuplicateRangeDecorator(datesRanges, datesRanges[i], i);

                    var smallN = getSmalln(fields, patients, datesRanges[i], reportModuleConfig.IsExcludeOnlyOneAnswer);
                    if (reportModuleConfig.IsAverageScore)
                    {
                        var sum = fields.Count(f => f.ChoiceId == choice.Id);
                        chartValues.Add(decoratedDateRange, TPScoreAverage(reportModuleConfig.pointSystemType, sum, smallN));

                    }
                    else
                    {
                        //temp fix
                        float value = fields.Count(f => f.ChoiceText == choice.Text);
                        //previus implimention
                        //float value = fields.Count(f => f.ChoiceId == choice.Id);
                        chartValues.Add(decoratedDateRange, TPScore(reportModuleConfig.pointSystemType, 
                            value, smallN));
                        
                    }

                }
                chartData.ChartValues.Add(chartValues);
                
            }
            for (int i = 0; i < reportModuleConfig.dateRanges.Count(); i++)
            {
                var entries = entriesAll.Where(f => f.AnsweredAt <= GetEndDate(datesRanges[i].end.Value.Date) &&
                                                    f.AnsweredAt >= GetStartDate(datesRanges[i].start.Value.Date));
                    
                var fields = entries.Where(r => r.FieldId == reportModuleConfig.FieldId 
                                                && FieldEntryInPopulation(r, patientIds)).ToList();
                var smallN = getSmalln(fields, patients, datesRanges[i], reportModuleConfig.IsExcludeOnlyOneAnswer);

                var bigN = getBigN(fields, patients, datesRanges[i], reportModuleConfig.IsExcludeOnlyOneAnswer);
                var decoratedDateRange = DuplicateRangeDecorator(datesRanges, datesRanges[i], i);
                chartData.SampleSizes.Add(decoratedDateRange, smallN);
                chartData.PopulationSizes.Add(decoratedDateRange, bigN);


            }


            return chartData;
        }


        private async Task<ChartData> GetValidateSurveyDataPeriods(ReportModuleConfig reportModuleConfig,
            IEnumerable<ProjectPatient> patients)
        {
            var chartData = new ChartData();
            var patientIds = patients.Select(p => p.Id);
            var datesRanges = reportModuleConfig.dateRanges;
            var surveyaccess = new SurveyAccess()
            {
                ProjectId = reportModuleConfig.ProjectId,
                StrategyId = reportModuleConfig.StrategyId,
                SearchEnd = DateTime.MaxValue,
                SearchStart = DateTime.MinValue,
                SurveyId = reportModuleConfig.SurveyId
            };
            var batchesAll = await _analyticsService.GetValidatedEntryBatchesInBetweenByStrategyIdAndSurveyId(surveyaccess, reportModuleConfig.Tags);
            for (int i = 0; i < datesRanges.Count(); i++)
            {
                var chartValues = new Dictionary<string, object>();
                var distinctPatients = new List<EntryBatch>();

                
                var entryBatches = batchesAll.Where(e => e.AnsweredAt <= GetEndDate(datesRanges[i].end.Value.Date) &&
                    e.AnsweredAt >= GetStartDate(datesRanges[i].start.Value.Date));
                if (entryBatches != null)
                {
                    distinctPatients = entryBatches.DistinctBy(x => x.PatientId).ToList();
                    distinctPatients = distinctPatients.Where(p => patientIds.Contains(p.PatientId)).ToList();
                    int responseCount = (entryBatches.Count() == 0) ? 1 : distinctPatients.Count();
                    var scores = new List<double>();

                    foreach (var distinctPatient in distinctPatients)
                    {
                        var entry = entryBatches.Where(x => x.PatientId.Equals(distinctPatient.PatientId)).OrderByDescending(i => i.AnsweredAt).FirstOrDefault();
                        scores.Add(entry.Score);
                    }

                    decimal v = (decimal)scores.Sum() / responseCount;
                    chartValues.Add(
                        "Value",
                        Math.Round(v, 2)
                    );
                }

                var smallN = entryBatches.Count();

                var bigN = patients.Count(p => p.CreatedAt <= GetEndDate(datesRanges[i].end.Value.Date) && p.IsActive);
                
                var decoratedDateRange = DuplicateRangeDecorator(datesRanges, datesRanges[i], i);
                var decoratedDataRangeName = DuplicateRangeDecoratorName(datesRanges, datesRanges[i], i);
                if (reportModuleConfig.ShowTimeSeriesPopulation)
                {
                    var decoratedDateRangeWithPop =
                        DuplicateRangeDecoratorWithPopulation(datesRanges, datesRanges[i], i, distinctPatients.Count());
                    chartValues.Add("Name", decoratedDateRangeWithPop);
                }
                else
                {
                    chartValues.Add("Name", decoratedDataRangeName);
                }
                chartData.SampleSizes.Add(decoratedDateRange, smallN);
                chartData.PopulationSizes.Add(decoratedDateRange, bigN);
                chartData.ChartValues.Add(chartValues);
            }
            return chartData;
        }

        private async Task<ChartData> GetCustomSurveyDataPeriods(ReportModuleConfig reportModuleConfig,
            IEnumerable<ProjectPatient> patients)
        {
            var chartData = new ChartData();
            var patientIds = patients.Select(p => p.Id);
            var datesRanges = reportModuleConfig.dateRanges;
            var entriesAll = await _analyticsService.GetCustomFieldEntriesAllByStrategyIdAndSurveyId(new SurveyAccess()
            {
                ProjectId = reportModuleConfig.ProjectId,
                StrategyId = reportModuleConfig.StrategyId,
                SurveyId = reportModuleConfig.SurveyId,
            }, reportModuleConfig.Tags);
            for (int i = 0; i < reportModuleConfig.dateRanges.Count(); i++)
            {
                var chartValues = new Dictionary<string, object>();
                var entries = entriesAll.Where(f => f.AnsweredAt <= GetEndDate(datesRanges[i].end.Value.Date) &&
                                                    f.AnsweredAt >= GetStartDate(datesRanges[i].start.Value.Date));
                    
                var fields = entries.Where(r => r.FieldId == reportModuleConfig.FieldId 
                                                && FieldEntryInPopulation(r, patientIds)).ToList();

                var decoratedDateRange = DuplicateRangeDecorator(datesRanges, datesRanges[i], i);
                var smallN = getSmalln(fields, patients, datesRanges[i], reportModuleConfig.IsExcludeOnlyOneAnswer);
                var bigN = getBigN(fields, patients, datesRanges[i], reportModuleConfig.IsExcludeOnlyOneAnswer);
                if (reportModuleConfig.IsAverageScore)
                {
                    var sum = fields.Where(f => FieldEntryInPopulation(f, patientIds)).Sum(p => p.Value);
                    chartValues.Add(decoratedDateRange, TPScoreAverage(reportModuleConfig.pointSystemType, sum, smallN));
                    chartData.SampleSizes.Add(decoratedDateRange, smallN);
                    chartData.PopulationSizes.Add(decoratedDateRange, bigN);
                }
                else
                {
                    int value;
                    switch (reportModuleConfig.pointSystemType)
                    {
                        case PointSystemType.Point:
                        {
                            value = fields.Count();
                            chartValues.Add(decoratedDateRange, TPScore(reportModuleConfig.pointSystemType, 
                                value, smallN));
                            chartData.SampleSizes.Add(decoratedDateRange, smallN);
                            chartData.PopulationSizes.Add(decoratedDateRange, bigN);
                            break;
                        }
                        case PointSystemType.Percentage:
                            chartValues.Add(decoratedDateRange, TPScore(reportModuleConfig.pointSystemType, 
                                smallN, patientIds.Count()));
                            chartData.SampleSizes.Add(decoratedDateRange, smallN);
                            break;
                        default:
                            value = fields.Count();
                            chartValues.Add(decoratedDateRange, TPScore(reportModuleConfig.pointSystemType, 
                                value, smallN));
                            chartData.SampleSizes.Add(decoratedDateRange, smallN);
                            chartData.PopulationSizes.Add(decoratedDateRange, bigN);
                            break;
                    }
                }
                var decoratedDataRangeName = DuplicateRangeDecoratorName(datesRanges, datesRanges[i], i);
                if (reportModuleConfig.ShowTimeSeriesPopulation)
                {
                    var decoratedDateRangeWithPop =
                        DuplicateRangeDecoratorWithPopulation(datesRanges, datesRanges[i], i, smallN);
                    chartValues.Add("Name", decoratedDateRangeWithPop);
                }
                else
                {
                    chartValues.Add("Name", decoratedDataRangeName);
                }   
                chartData.ChartValues.Add(chartValues);   
            }
            
            return chartData;            
        }
        
        
        public async Task<ChartData> GetSurveyChartMultipleQuestions(ReportModuleConfig reportModuleConfig, IEnumerable<ProjectPatient> population)
        {
            var chartData = new ChartData();
            var fieldsIds = reportModuleConfig.MultipleQuestionsIds;
            var patientIds = population.Select(p => p.Id);
            var datesRanges = reportModuleConfig.dateRanges;
            foreach (var fieldId in fieldsIds.Values)
            {
                chartData.SampleSizes = new Dictionary<string, int>();
                chartData.PopulationSizes = new Dictionary<string, int>();
                var chartValues = new Dictionary<string, object>();
                var field = await _surveyService.GetFieldById(reportModuleConfig.SurveyId, fieldId);
                if (field == null)
                {
                    continue;
                }
                chartValues.Add("Name",field.Text);
                var entriesAll = await _analyticsService.GetCustomFieldEntriesAllByStrategyIdAndSurveyId(new SurveyAccess()
                {
                    ProjectId = reportModuleConfig.ProjectId,
                    StrategyId = reportModuleConfig.StrategyId,
                    SurveyId = reportModuleConfig.SurveyId,
                }, reportModuleConfig.Tags);
                for (int i = 0; i < datesRanges.Count(); i++)
                {
                    var entries = entriesAll.Where(f => f.AnsweredAt <= GetEndDate(datesRanges[i].end.Value.Date) &&
                                                        f.AnsweredAt >= GetStartDate(datesRanges[i].start.Value.Date));

                    var fields = entries.Where(r => r.FieldId == fieldId && FieldEntryInPopulation(r, patientIds))
                        .ToList();

                    var smallN = getSmalln(fields, population, datesRanges[i],
                        reportModuleConfig.IsExcludeOnlyOneAnswer);

                    var bigN = getBigN(fields, population, datesRanges[i], reportModuleConfig.IsExcludeOnlyOneAnswer);
                    
                    if (reportModuleConfig.IsAverageScore)
                    {
                        var sum = fields.Sum(p=>p.Value);
                        var decoratedDateRange = DuplicateRangeDecorator(datesRanges, datesRanges[i], i);
                        var decoratedName = DuplicateRangeDecoratorName(datesRanges, datesRanges[i], i);
                        if (reportModuleConfig.ShowTimeSeriesPopulation)
                        {
                            decoratedDateRange =
                                DuplicateRangeDecoratorWithPopulation(datesRanges, datesRanges[i], i, smallN);
                        }
                        chartValues.Add(decoratedDateRange, TPScoreAverage(reportModuleConfig.pointSystemType, sum, smallN));
                        chartData.SampleSizes.Add(decoratedDateRange, smallN);
                        chartData.PopulationSizes.Add(decoratedDateRange, bigN);
                    }
                    else
                    {
                        var value = fields.Count();
                        var decoratedDateRange = DuplicateRangeDecorator(datesRanges, datesRanges[i], i);
                        chartValues.Add(decoratedDateRange, TPScore(reportModuleConfig.pointSystemType, value, smallN));
                        chartData.SampleSizes.Add(decoratedDateRange, smallN);
                        chartData.PopulationSizes.Add(decoratedDateRange, bigN);
                    }
                    
                }
                
                chartData.ChartValues.Add(chartValues);
            }
            return chartData;
        }

        public async Task<IEnumerable<ProjectReportStats>> GetSurveyMultipleStats(ReportModuleConfig reportModuleConfig,
            IEnumerable<ProjectPatient> population)
        {
            var projectReportStats = new List<ProjectReportStats>();
            var fieldsIds = reportModuleConfig.MultipleQuestionsIds;
            var patientIds = population.Select(p => p.Id);
            var datesRanges = reportModuleConfig.dateRanges;
            foreach (var fieldId in fieldsIds.Values)
            {
                var entriesAll = await _analyticsService.GetCustomFieldEntriesAllByStrategyIdAndSurveyId(new SurveyAccess()
                {
                    ProjectId = reportModuleConfig.ProjectId,
                    StrategyId = reportModuleConfig.StrategyId,
                    SurveyId = reportModuleConfig.SurveyId,
                }, reportModuleConfig.Tags);
                for (int i = 0; i < datesRanges.Count(); i++)
                {
                    var projectReportStat = new ProjectReportStats
                    {
                        FieldId = fieldId
                    };
                    var field = await _surveyService.GetFieldById(reportModuleConfig.SurveyId, fieldId);
                    if (field == null)
                    {
                        continue;
                    }
                    var entries = entriesAll.Where(f => f.AnsweredAt <= GetEndDate(datesRanges[i].end.Value.Date) &&
                                                        f.AnsweredAt >= GetStartDate(datesRanges[i].start.Value.Date));

                    var fields = entries.Where(r => r.FieldId == fieldId && FieldEntryInPopulation(r, patientIds))
                        .ToList();
                    var smallN = getSmalln(fields, population, datesRanges[i],
                        reportModuleConfig.IsExcludeOnlyOneAnswer);

                    var bigN = getBigN(fields, population, datesRanges[i], reportModuleConfig.IsExcludeOnlyOneAnswer);

                    var answerRate = getAnswerRate(smallN, bigN);

                    projectReportStat.SmallN = smallN;
                    projectReportStat.BigN = bigN;
                    projectReportStat.AnswerRate = answerRate;
                    projectReportStat.FieldText = field.Text;
                    projectReportStat.DatePeriod = datesRanges[i];
                    projectReportStats.Add(projectReportStat);
                }

            }
            return projectReportStats;
        }
        
        
        public async Task<ChartData> GetValidatedSurveyChartData(ReportModuleConfig reportModuleConfig,
            IEnumerable<ProjectPatient> population)
        {
            return await GetValidateSurveyDataPeriods(reportModuleConfig, population);
        }

        public async Task<ChartData> GetStatusRegChartData(string projectId, string strategyId, string category, List<DateTime?> endDates, PointSystemType pointSystemType, List<ProjectTag> tags)
        {
            var chartData = new ChartData();
            var registeredPatients = new List<string>();
            var strategyEffects = await _strategyService.GetStrategyEffectsByStrategyId(strategyId);
            var selectedStrategyEffects = strategyEffects.Where(x => x.Category == category).ToList();
            selectedStrategyEffects.Sort((a, b) => a.Index - b.Index);
            endDates.OrderByDescending(d => d.Value);

            for (var i = 0; i < endDates.Count(); i++)
            {
                var decoratedRange = DuplicateDatesDecorator(endDates, endDates[i].Value, i);
                chartData.SampleSizes.Add(decoratedRange, 0);
                chartData.PopulationSizes.Add(decoratedRange, await _analyticsService.GetActivePatientsCount(strategyId, tags, null, endDates[i].Value));
            }

            for (var j = 0; j < selectedStrategyEffects.Count(); j++)
            {
                chartData.ChartValues.Add(
                new Dictionary<string, object>() {
                            { "Name", selectedStrategyEffects[j].Name},
                });
            }

            var unSortedRegs = new Dictionary<DateTime, List<Registration>>();
            for (var j = 0; j < endDates.Count(); j++)
            {
                var regs = new List<Registration>();
                for (var i = 0; i < selectedStrategyEffects.Count(); i++)
                {
                    var regAccess = new RegistrationAccess()
                    {
                        ProjectId = projectId,
                        StrategyId = strategyId,
                        EffectId = selectedStrategyEffects[i].Id,
                        Type = "STATUS",
                        SearchEnd = endDates[j].Value.Date
                    };
                    var categoryRegistrations = await _analyticsService.GetStrategyRegsByEffectIdAndTypes(regAccess, tags);
                    regs.AddRange(categoryRegistrations);
                }

                unSortedRegs.Add(endDates[j].Value.AddMicroseconds(j), regs);//issue
            }

            for (var i = 0; i < unSortedRegs.Count(); i++)
            {

                for (var j = 0; j < selectedStrategyEffects.Count(); j++)
                {
                    int v = 0;
                    var undisUnSortedRegs = unSortedRegs.Values.ElementAt(i);
                    foreach (var uniqueRegs in undisUnSortedRegs.DistinctBy(x => x.PatientId))
                    {
                        var latestStatus = undisUnSortedRegs.Where(x => x.PatientId == uniqueRegs.PatientId).OrderByDescending(t => t.Date).FirstOrDefault();

                        if (latestStatus.EffectId == selectedStrategyEffects[j].Id)
                        {
                            v += 1;
                        }
                    }

                    var ChartValuesAt = chartData.ChartValues.ElementAt(j);
                    var SampleSizesAt = chartData.SampleSizes.ElementAt(i);
                    var dates = unSortedRegs.Keys.Cast<DateTime?>().ToList();
                    var barCount = unSortedRegs.Values.ElementAt(i).DistinctBy(x => x.PatientId).Count();
                    var dupedDateKey = DuplicateDatesDecorator(dates, unSortedRegs.Keys.ElementAt(i), i);
                    ChartValuesAt.Add(dupedDateKey, TPScore(pointSystemType, v, barCount));
                    chartData.SampleSizes[dupedDateKey] = SampleSizesAt.Value + v;
                    
                }
            }
            return chartData;
        }

        public async Task<ChartData> GetNumericRegChartData(string effectId, string projectId, string strategyId, List<ProjectTag> tags, string frequency, string period, DateTime? start, DateTime? end, ISystemMessage message, bool isEmpty)
        {
            var result = await _frequencyOperatorContext.GetFrequentRegs(frequency, projectId, strategyId, effectId, period, tags, "NUMERIC", start, end, message);

            return TrimmeResponse(isEmpty, result);
        }

        public async Task<ChartData> GetIncidentRegChartData(string effectId, string projectId, string strategyId, List<ProjectTag> tags, string frequency, string period, DateTime? start, DateTime? end, ISystemMessage message, bool isEmpty)
        {
            var result = await _frequencyOperatorContext.GetFrequentRegs(frequency, projectId, strategyId, effectId, period, tags, "COUNT", start, end, message);
            return TrimmeResponse(isEmpty, result);
        }

        public async Task<IEnumerable<ProjectPatient>> GetPopulationByStrategyIdAndTags(string strategyId, List<ProjectTag> tags, List<string> filters)
        {
            return await _analyticsService.GetPopulationByStrategyIdAndTags(strategyId, tags, filters);
        }

        private object TPScore(PointSystemType pointSystemType, object primaryValue, object secondaryValue)
        {
            switch ((int)pointSystemType)
            {
                case 1:
                    return primaryValue;
                case 2:
                    decimal a = Convert.ToDecimal(primaryValue);
                    decimal b = Convert.ToDecimal(secondaryValue);
                    decimal ab = 0;
                    if (b != 0)
                    {
                        ab = (decimal)a / b * 100;
                    }
                    else
                    {
                        ab = 0;
                    }
                    return Math.Round(ab, 2);
                default:
                    return primaryValue;
            }
        }

        private object TPScoreAverage(PointSystemType pointSystemType, object primaryValue, object secondaryValue)
        {                    
            decimal a = Convert.ToDecimal(primaryValue);
            decimal b = Convert.ToDecimal(secondaryValue);
            decimal ab = 0;
            switch ((int)pointSystemType)
            {
                case 1:
                    if (b != 0)
                    {
                        ab = a / b;
                    }
                    else
                    {
                        ab = 0;
                    }
                    return Math.Round(ab, 2);
                case 2:

                    if (b != 0)
                    {
                        ab = (decimal)a / b * 100;
                    }
                    else
                    {
                        ab = 0;
                    }
                    return Math.Round(ab, 2);
                default:
                    return primaryValue;
            }
        }

        
        private string DuplicateRangeDecorator(List<DateRanges> DatesRanges, DateRanges selectedDatesRange, int i)
        {
            var dateGroupsStrat = from DatesRange in DatesRanges
                                  group DatesRange by DatesRange.start into g
                                  select new { Key = g.Key, Count = g.Count() };
            var dateGroupsEnd = from DatesRange in DatesRanges
                                group DatesRange by DatesRange.end into g
                                select new { Key = g.Key, Count = g.Count() };

            var selectedStartCount = dateGroupsStrat.Where(x => x.Key == selectedDatesRange.start.Value).FirstOrDefault();
            var selectedEndCount = dateGroupsEnd.Where(x => x.Key == selectedDatesRange.end.Value).FirstOrDefault();
            var startIndex = (selectedStartCount != null && selectedStartCount.Count >= 1) ? $"[{selectedStartCount.Count + i}]" : null;
            var endIndex = (selectedEndCount != null && selectedEndCount.Count >= 1) ? $"[{selectedEndCount.Count + i}]" : null;
            var start = $"{selectedDatesRange.start.Value.ToString("dd/MM/yy", new CultureInfo("en-US"))}{startIndex}";
            var end = $"{selectedDatesRange.end.Value.ToString("dd/MM/yy", new CultureInfo("en-US"))}{endIndex}";
            return $"{start}-{end}";
        }


        private string DuplicateRangeDecoratorWithPopulation(List<DateRanges> dataRangesList,
            DateRanges selectedDatesRange, int i, int population)
        {
            var dateGroupsStrat = from DatesRange in dataRangesList
                group DatesRange by DatesRange.start into g
                select new { Key = g.Key, Count = g.Count() };
            var dateGroupsEnd = from DatesRange in dataRangesList
                group DatesRange by DatesRange.end into g
                select new { Key = g.Key, Count = g.Count() };

            var selectedStartCount = dateGroupsStrat.Where(x => x.Key == selectedDatesRange.start.Value).FirstOrDefault();
            var selectedEndCount = dateGroupsEnd.Where(x => x.Key == selectedDatesRange.end.Value).FirstOrDefault();
            var startIndex = (selectedStartCount != null && selectedStartCount.Count >= 1) ? $"[{selectedStartCount.Count + i}]" : null;
            var endIndex = (selectedEndCount != null && selectedEndCount.Count >= 1) ? $"[{selectedEndCount.Count + i}]" : null;
            var start = $"{selectedDatesRange.start.Value.ToString("dd/MM/yy", new CultureInfo("en-US"))}{startIndex}";
            var end = $"{selectedDatesRange.end.Value.ToString("dd/MM/yy", new CultureInfo("en-US"))}{endIndex}";
            return $"{start}-{end}  (N={population})";
        }
        
        private string DuplicateRangeDecoratorName(List<DateRanges> DatesRanges, DateRanges selectedDatesRange, int i)
        {
            var dateGroupsStrat = from DatesRange in DatesRanges
                group DatesRange by DatesRange.start into g
                select new { Key = g.Key, Count = g.Count() };
            var dateGroupsEnd = from DatesRange in DatesRanges
                group DatesRange by DatesRange.end into g
                select new { Key = g.Key, Count = g.Count() };

            var selectedStartCount = dateGroupsStrat.Where(x => x.Key == selectedDatesRange.start.Value).FirstOrDefault();
            var selectedEndCount = dateGroupsEnd.Where(x => x.Key == selectedDatesRange.end.Value).FirstOrDefault();
            var start = $"{selectedDatesRange.start.Value.ToString("dd/MM/yy", new CultureInfo("en-US"))}";
            var end = $"{selectedDatesRange.end.Value.ToString("dd/MM/yy", new CultureInfo("en-US"))}";
            return $"{start}-{end}";
        }


        private string DuplicateDatesDecorator(List<DateTime?> Dates, DateTime selectedDate, int i)
        {
            var dupedCounter = Dates.Where(d => d.Value.Date == selectedDate.Date).Count() > 1 ? $"[{(i + 1)}]" : null;
            return $"{selectedDate.ToString("dd/MM/yy", new CultureInfo("en-US"))}{dupedCounter}";
        }

        private ChartData TrimmeResponse(bool isEmpty, ChartData chartData)
        {
            if (isEmpty)
            {
                var toRemove = new List<Dictionary<string, object>>();
                foreach (var chartValues in chartData.ChartValues)
                {
                    foreach (var Dic in chartValues.Where(x => x.Key != "Name").Where(x => (dynamic)x.Value == 0))
                    {
                        chartValues.Remove(Dic.Key);
                    }
                    if (chartValues.Count() == 1)
                    {
                        toRemove.Add(chartValues);
                    }
                }
                if (chartData.SampleSizes != null)
                {
                    foreach (var dic in chartData.SampleSizes.Where(x => (dynamic)x.Value == 0))
                    {
                        chartData.SampleSizes.Remove(dic.Key);
                    }
                }

                chartData.ChartValues.RemoveAll(x => toRemove.Contains(x));
            }
            return chartData;
        }

        private List<EntryBatch> FilterEntryBatches(List<EntryBatch> entries, List<string> filters)
        {
            if (filters == null || !filters.Contains(FilterExcludeOnlyOneAnswer)) return entries;
            var filtered = entries.GroupBy(p => p.PatientId).Where(g => g.Count() > 1);
            var filteredEntries = filtered.SelectMany(group => group).ToList();
            return filteredEntries;
        }

        private bool FieldEntryInPopulation(FieldEntry entry, IEnumerable<string> populationIds)
        {
            return entry.PatientId != null && populationIds.ToList().Contains(entry.PatientId);
        }

        private int getSmalln(IEnumerable<FieldEntry> fieldEntries, IEnumerable<ProjectPatient> patients, DateRanges dateRanges,
            bool isExcludeOnlyOneAnswer)
        {
            if (!isExcludeOnlyOneAnswer)
            {
                return fieldEntries.Count();
            }
            var itemsByGroupWithCount = fieldEntries.GroupBy(f => f.PatientId )
                .Select(item => new { 
                    Number = item.Key, 
                    Total = item.Count()})
                .Where(t=>t.Total > 1);
            return itemsByGroupWithCount.Sum(f => f.Total);
        }
        
        private int getBigN(IEnumerable<FieldEntry> fieldEntries, IEnumerable<ProjectPatient> patients, DateRanges dateRanges,
            bool isExcludeOnlyOneAnswer)
        {
            if (!isExcludeOnlyOneAnswer)
            {
                var c =patients.Count(p => p.CreatedAt <= dateRanges.end.Value.Date && p.IsActive );
                var pc = fieldEntries.Select(p => p.PatientId).Distinct().Count();
                return c>pc?c:pc;
            }
            var itemsByGroupWithCount = fieldEntries.GroupBy(f => f.PatientId )
                .Select(item => new { 
                    Number = item.Key, 
                    Total = item.Count()})
                .Where(t=>t.Total > 1);
            return itemsByGroupWithCount.Count();
        }




        private decimal getAnswerRate(int n, int N)
        {
            if (N == 0)
            {
                return 0;
            }
            decimal ab = ((decimal)n / N) * 100;
            return Math.Round(ab, 2);
        }


        private static DateTime GetStartDate(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day);
        }

        private static DateTime GetEndDate(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }
    }
}