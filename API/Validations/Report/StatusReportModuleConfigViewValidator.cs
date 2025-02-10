using API.Models.Views.Report;
using FluentValidation;

namespace API.Validations.Report
{
    public class StatusReportModuleConfigViewValidator : AbstractValidator<StatusReportModuleConfigViewModel>
    {
        public StatusReportModuleConfigViewValidator()
        {
            RuleFor(x => x.Category).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.isEmpty).NotNull();
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.StrategyId).NotEmpty();
            RuleFor(x => x.graphType).NotEmpty();
            RuleFor(x => x.endDates).NotNull();
            RuleFor(x => x.Type).NotEmpty();
        }
    }
}
