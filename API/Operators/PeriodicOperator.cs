using API.Handlers;
using API.Models.Analytics;
using API.Models.Projects;
using API.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Operators
{
    public interface IPeriodicOperator
    {
        public Task<IEnumerable<Registration>> GetPeriodicRegs(string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod);
        public PeriodDateCombo GetPeriodicDate();
    }

    public class PeriodDateCombo
    {
        public DateTime startPeriod { get; set; }
        public DateTime endPeriod { get; set; }
    }

    public class ThisWeek : IPeriodicOperator
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITimeMachineHandler _timeMachineHandler;

        public ThisWeek(IAnalyticsService analyticsService, ITimeMachineHandler timeMachineHandler)
        {
            _analyticsService = analyticsService;
            _timeMachineHandler = timeMachineHandler;
        }

        public async Task<IEnumerable<Registration>> GetPeriodicRegs(string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod)
        {
            var periodDateCombo = GetPeriodicDate();
            var regAccess = new RegistrationAccess()
            {
                SearchStart = periodDateCombo.startPeriod,
                SearchEnd = periodDateCombo.endPeriod,
                StrategyId = strategyId,
                ProjectId = projectId,
                EffectId = effectId,
                Type = type
            };
            return await _analyticsService.GetStrategyRegsByEffectIdAndTypes(regAccess, tags);
        }

        public PeriodDateCombo GetPeriodicDate()
        {
            return new PeriodDateCombo
            {
                startPeriod = _timeMachineHandler.GetThisWeekStart(),
                endPeriod = _timeMachineHandler.GetThisWeekEnd()
            };
        }
    }
    public class LastWeek : IPeriodicOperator
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITimeMachineHandler _timeMachineHandler;

        public LastWeek(IAnalyticsService analyticsService, ITimeMachineHandler timeMachineHandler)
        {
            _analyticsService = analyticsService;
            _timeMachineHandler = timeMachineHandler;
        }

        public async Task<IEnumerable<Registration>> GetPeriodicRegs(string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod)
        {
            var periodDateCombo = GetPeriodicDate();
            var regAccess = new RegistrationAccess()
            {
                SearchStart = periodDateCombo.startPeriod,
                SearchEnd = periodDateCombo.endPeriod,
                StrategyId = strategyId,
                ProjectId = projectId,
                EffectId = effectId,
                Type = type
            };
            return await _analyticsService.GetStrategyRegsByEffectIdAndTypes(regAccess, tags);
        }

        public PeriodDateCombo GetPeriodicDate()
        {
            return new PeriodDateCombo
            {
                startPeriod = _timeMachineHandler.GetPreviousWeekStart(),
                endPeriod = _timeMachineHandler.GetPreviousWeekEnd()
            };
        }
    }
    public class ThisMonth : IPeriodicOperator
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITimeMachineHandler _timeMachineHandler;

        public ThisMonth(IAnalyticsService analyticsService, ITimeMachineHandler timeMachineHandler)
        {
            _analyticsService = analyticsService;
            _timeMachineHandler = timeMachineHandler;
        }

        public async Task<IEnumerable<Registration>> GetPeriodicRegs(string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod)
        {
            var periodDateCombo = GetPeriodicDate();
            var regAccess = new RegistrationAccess()
            {
                SearchStart = periodDateCombo.startPeriod,
                SearchEnd = periodDateCombo.endPeriod,
                StrategyId = strategyId,
                ProjectId = projectId,
                EffectId = effectId,
                Type = type
            };
            return await _analyticsService.GetStrategyRegsByEffectIdAndTypes(regAccess, tags);
        }

        public PeriodDateCombo GetPeriodicDate()
        {
            return new PeriodDateCombo
            {
                startPeriod = _timeMachineHandler.GetThisMonthStart(),
                endPeriod = _timeMachineHandler.GetThisMonthEnd()
            };
        }
    }
    public class LastMonth : IPeriodicOperator
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITimeMachineHandler _timeMachineHandler;

        public LastMonth(IAnalyticsService analyticsService, ITimeMachineHandler timeMachineHandler)
        {
            _analyticsService = analyticsService;
            _timeMachineHandler = timeMachineHandler;
        }

        public async Task<IEnumerable<Registration>> GetPeriodicRegs(string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod)
        {
            var periodDateCombo = GetPeriodicDate();
            var regAccess = new RegistrationAccess()
            {
                SearchStart = periodDateCombo.startPeriod,
                SearchEnd = periodDateCombo.endPeriod,
                StrategyId = strategyId,
                ProjectId = projectId,
                EffectId = effectId,
                Type = type
            };
            return await _analyticsService.GetStrategyRegsByEffectIdAndTypes(regAccess, tags);
        }

        public PeriodDateCombo GetPeriodicDate()
        {
            return new PeriodDateCombo
            {
                startPeriod = _timeMachineHandler.GetPreviousMonthStart(),
                endPeriod = _timeMachineHandler.GetPreviousMonthEnd()
            };
        }
    }
    public class ThisQuarter : IPeriodicOperator
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITimeMachineHandler _timeMachineHandler;

        public ThisQuarter(IAnalyticsService analyticsService, ITimeMachineHandler timeMachineHandler)
        {
            _analyticsService = analyticsService;
            _timeMachineHandler = timeMachineHandler;
        }

        public async Task<IEnumerable<Registration>> GetPeriodicRegs(string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod)
        {
            var periodDateCombo = GetPeriodicDate();
            var regAccess = new RegistrationAccess()
            {
                SearchStart = periodDateCombo.startPeriod,
                SearchEnd = periodDateCombo.endPeriod,
                StrategyId = strategyId,
                ProjectId = projectId,
                EffectId = effectId,
                Type = type
            };
            return await _analyticsService.GetStrategyRegsByEffectIdAndTypes(regAccess, tags);
        }

        public PeriodDateCombo GetPeriodicDate()
        {
            return new PeriodDateCombo
            {
                startPeriod = _timeMachineHandler.GetCurrentQuarterStart(),
                endPeriod = _timeMachineHandler.GetCurrentQuarterEnd()
            };
        }
    }
    public class LastQuarter : IPeriodicOperator
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITimeMachineHandler _timeMachineHandler;

        public LastQuarter(IAnalyticsService analyticsService, ITimeMachineHandler timeMachineHandler)
        {
            _analyticsService = analyticsService;
            _timeMachineHandler = timeMachineHandler;
        }

        public async Task<IEnumerable<Registration>> GetPeriodicRegs(string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod)
        {
            var periodDateCombo = GetPeriodicDate();
            var regAccess = new RegistrationAccess()
            {
                SearchStart = periodDateCombo.startPeriod,
                SearchEnd = periodDateCombo.endPeriod,
                StrategyId = strategyId,
                ProjectId = projectId,
                EffectId = effectId,
                Type = type
            };
            return await _analyticsService.GetStrategyRegsByEffectIdAndTypes(regAccess, tags);
        }

        public PeriodDateCombo GetPeriodicDate()
        {
            return new PeriodDateCombo
            {
                startPeriod = _timeMachineHandler.GetPreviousQuarterStart(),
                endPeriod = _timeMachineHandler.GetPreviousQuarterEnd()
            };
        }
    }
    public class ThisYear : IPeriodicOperator
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITimeMachineHandler _timeMachineHandler;

        public ThisYear(IAnalyticsService analyticsService, ITimeMachineHandler timeMachineHandler)
        {
            _analyticsService = analyticsService;
            _timeMachineHandler = timeMachineHandler;
        }

        public async Task<IEnumerable<Registration>> GetPeriodicRegs(string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod)
        {
            var periodDateCombo = GetPeriodicDate();
            var regAccess = new RegistrationAccess()
            {
                SearchStart = periodDateCombo.startPeriod,
                SearchEnd = periodDateCombo.endPeriod,
                StrategyId = strategyId,
                ProjectId = projectId,
                EffectId = effectId,
                Type = type
            };
            return await _analyticsService.GetStrategyRegsByEffectIdAndTypes(regAccess, tags);
        }

        public PeriodDateCombo GetPeriodicDate()
        {
            return new PeriodDateCombo
            {
                startPeriod = _timeMachineHandler.GetThisYearStart(),
                endPeriod = _timeMachineHandler.GetThisYearEnd()
            };
        }
    }
    public class LastYear : IPeriodicOperator
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITimeMachineHandler _timeMachineHandler;

        public LastYear(IAnalyticsService analyticsService, ITimeMachineHandler timeMachineHandler)
        {
            _analyticsService = analyticsService;
            _timeMachineHandler = timeMachineHandler;
        }

        public async Task<IEnumerable<Registration>> GetPeriodicRegs(string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod)
        {
            var periodDateCombo = GetPeriodicDate();
            var regAccess = new RegistrationAccess()
            {
                SearchStart = periodDateCombo.startPeriod,
                SearchEnd = periodDateCombo.endPeriod,
                StrategyId = strategyId,
                ProjectId = projectId,
                EffectId = effectId,
                Type = type
            };
            return await _analyticsService.GetStrategyRegsByEffectIdAndTypes(regAccess, tags);
        }

        public PeriodDateCombo GetPeriodicDate()
        {
            return new PeriodDateCombo
            {
                startPeriod = _timeMachineHandler.GetPreviousYearStart(),
                endPeriod = _timeMachineHandler.GetPreviousYearEnd()
            };
        }
    }
    public class Custom : IPeriodicOperator
    {
        private readonly IAnalyticsService _analyticsService;

        public Custom(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public async Task<IEnumerable<Registration>> GetPeriodicRegs(string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod)
        {
            var regAccess = new RegistrationAccess()
            {
                SearchStart = startPeriod.Value,
                SearchEnd = endPeriod.Value,
                StrategyId = strategyId,
                ProjectId = projectId,
                EffectId = effectId,
                Type = type
            };
            return await _analyticsService.GetStrategyRegsByEffectIdAndTypes(regAccess, tags);
        }

        public PeriodDateCombo GetPeriodicDate()
        {
            return null;
        }
    }

    public interface IPeriodicOperationContext
    {
        public Task<IEnumerable<Registration>> GetPeriodicRegs(string searchType, string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod);
        public PeriodDateCombo GetPeriodicDate(string searchType);
    }

    public class PeriodicOperationContext : IPeriodicOperationContext
    {
        private readonly Dictionary<string, IPeriodicOperator> _periodicOperator = new Dictionary<string, IPeriodicOperator>();

        public PeriodicOperationContext(IAnalyticsService analyticsService, ITimeMachineHandler timeMachineHandler)
        {
            _periodicOperator.Add("ThisWeek", new ThisWeek(analyticsService, timeMachineHandler));
            _periodicOperator.Add("LastWeek", new LastWeek(analyticsService, timeMachineHandler));
            _periodicOperator.Add("ThisMonth", new ThisMonth(analyticsService, timeMachineHandler));
            _periodicOperator.Add("LastMonth", new LastMonth(analyticsService, timeMachineHandler));
            _periodicOperator.Add("ThisQuarter", new ThisQuarter(analyticsService, timeMachineHandler));
            _periodicOperator.Add("LastQuarter", new LastQuarter(analyticsService, timeMachineHandler));
            _periodicOperator.Add("ThisYear", new ThisYear(analyticsService, timeMachineHandler));
            _periodicOperator.Add("LastYear", new LastYear(analyticsService, timeMachineHandler));
            _periodicOperator.Add("custom", new Custom(analyticsService));
        }

        public async Task<IEnumerable<Registration>> GetPeriodicRegs(string searchType, string projectId, string strategyId, string effectId, List<ProjectTag> tags, string type, DateTime? startPeriod, DateTime? endPeriod)
        {
            return await _periodicOperator[searchType].GetPeriodicRegs(projectId, strategyId, effectId, tags, type, startPeriod, endPeriod);
        }

        public PeriodDateCombo GetPeriodicDate(string searchType)
        {
            return _periodicOperator[searchType].GetPeriodicDate();
        }
    }
}
