using API.Models.Views.Strategy;
using FluentValidation;

namespace API.Validations.Strategy
{
    public class PatientValidator : AbstractValidator<StrategyPatientViewModel>
    {
        public PatientValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.ParentId).NotEmpty();
        }
    }
}
