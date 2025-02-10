﻿using API.Models.Views.Report;
using FluentValidation;
using System.Linq;

namespace API.Validations.Report
{
    public class CustomReportModuleConfigViewValidator : AbstractValidator<CustomReportModuleConfigViewModel>
    {
        public CustomReportModuleConfigViewValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.StrategyId).NotEmpty();
            RuleFor(x => x.SurveyId).NotEmpty();
            //RuleFor(x => x.FieldId).NotEmpty();
            RuleFor(x => x.graphType).NotNull();
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.dateRanges).Must(x => x.Any());
            RuleForEach(x => x.dateRanges).SetValidator(new DateRangesValidator());
        }
    }
}
