using API.Models.Strategy;
using API.Models.Views.Strategy;
using FluentValidation;
using System.Linq;

namespace API.Validations.Strategy
{
    public class BatchSendoutFrequencyValidator : AbstractValidator<BatchSendoutFrequencyViewModel>
    {
        public BatchSendoutFrequencyValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.PatientsId).NotEmpty();
            RuleFor(x => x.CronExpression).NotEmpty().When(x => x.End.Type != EndType.IMMEDIATE);
            RuleFor(x => x.PatientsId).Must(x => x.Any());
            RuleFor(x => x.End).NotNull();
            RuleForEach(x => x.PatientsId).NotNull().NotEmpty();
            RuleForEach(x => x.Surveys).SetValidator(new SurveyPropertyValidator());
        }
    }
}
