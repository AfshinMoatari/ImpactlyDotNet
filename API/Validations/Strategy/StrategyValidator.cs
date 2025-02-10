using API.Models.Views.Strategy;
using FluentValidation;
using System.Linq;

namespace API.Validations.Strategy
{
    public class StrategyValidator : AbstractValidator<StrategyViewModel>
    {
        public StrategyValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Patients).Must(x => x.Any());
            RuleForEach(x => x.Patients).SetValidator(new PatientValidator());
            RuleForEach(x => x.Frequencies).SetValidator(new BatchSendoutFrequencyValidator()).When(x => x.Frequencies.Any());
            RuleForEach(x => x.Surveys).SetValidator(new SurveyPropertyValidator()).When(x => x.Surveys.Any());
        }
    }
}
