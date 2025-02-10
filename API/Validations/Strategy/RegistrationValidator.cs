using API.Models.Views.Strategy;
using FluentValidation;

namespace API.Validations.Strategy
{
    public class RegistrationValidator : AbstractValidator<RegistrationViewModel>
    {
        public RegistrationValidator()
        {
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.PatientId).NotEmpty();
            RuleFor(x => x.EffectId).NotEmpty();
            RuleFor(x => x.EffectName).NotEmpty();
            RuleFor(x => x.Date).NotNull();
            RuleFor(x => x.Before).NotNull().SetValidator(new StrategyEffectValidator()).When(x => x.Type.Equals("status"));
            RuleFor(x => x.Now).NotNull().SetValidator(new StrategyEffectValidator()).When(x => x.Type.Equals("status"));
            RuleFor(x => x.Value).NotNull().When(x => x.Type.Equals("numeric"));
            RuleFor(x => x.Category).NotEmpty().When(x => x.Type.Equals("status"));
        }
    }
}
