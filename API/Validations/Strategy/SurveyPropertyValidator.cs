using API.Models.Views.Strategy;
using FluentValidation;

namespace API.Validations.Strategy
{
    public class SurveyPropertyValidator : AbstractValidator<SurveyPropertyViewModel>
    {
        public SurveyPropertyValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.ParentId).NotEmpty();
        }
    }
}
