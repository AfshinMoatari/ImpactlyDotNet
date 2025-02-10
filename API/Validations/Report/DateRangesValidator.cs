using FluentValidation;
using static API.Models.Reports.ReportModuleConfig;

namespace API.Validations.Report
{
    public class DateRangesValidator : AbstractValidator<DateRanges>
    {
        public DateRangesValidator()
        {
            RuleFor(x => x.start.Value.Date).NotNull();
            RuleFor(x => x.end.Value.Date).NotNull();
            RuleFor(m => m.end.Value.Date)
               .NotEmpty()
               .GreaterThanOrEqualTo(m => m.start.Value.Date)
               .When(m => m.start.HasValue);
        }
    }
}
