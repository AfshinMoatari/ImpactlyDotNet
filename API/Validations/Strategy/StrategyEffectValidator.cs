using API.Models.Views.Strategy;
using FluentValidation;

namespace API.Validations.Strategy
{
    public class StrategyEffectValidator : AbstractValidator<StrategyEffectViewModel>
    {
        public StrategyEffectValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.Index).NotNull();
            RuleFor(x => x.Category).NotEmpty().When(x => x.Type.Equals("status"));
        }
    }
}
