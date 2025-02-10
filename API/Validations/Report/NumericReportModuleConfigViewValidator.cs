using API.Models.Views.Report;
using FluentValidation;

namespace API.Validations.Report
{
    public class NumericReportModuleConfigViewValidator : AbstractValidator<NumericReportModuleConfigViewModel>
    {
        public NumericReportModuleConfigViewValidator()
        {
            RuleFor(x => x.EffectId).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.isEmpty).NotNull();
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.StrategyId).NotEmpty();
            RuleFor(x => x.TimePreset).NotEmpty();
            RuleFor(x => x.TimeUnit).NotEmpty();
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.Start.Value.Date).NotNull().When(x => x.TimePreset == "custom");
            RuleFor(x => x.End.Value.Date).NotNull().When(x => x.TimePreset == "custom");
            RuleFor(m => m.End.Value.Date)
                .NotEmpty()
                .GreaterThanOrEqualTo(m => m.Start.Value.Date)
                .When(m => m.Start.HasValue)
                .When(x => x.TimePreset == "custom");
        }
    }
}
