using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Models.Strategy;
using API.Repositories;
using Nest;

namespace API.Services
{
    public interface ISurveyService
    {
        /// <summary>
        /// Retrieves all survey choices by the survey fieldId.
        /// </summary>
        /// <returns>A list of all choices for a survey field.</returns>
        public Task<IEnumerable<FieldChoice>> GetFieldChoicesByFieldId(string fieldId);

        public Task<SurveyField> GetFieldById(string surveyId, string fieldId);
    }

    /// <summary>
    /// Service class for managing surveys.
    /// </summary>
    public class SurveyService : ISurveyService
    {
        private readonly IStrategyContext _strategyContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyService"/> class.
        /// </summary>
        /// <param name="analytics">The _survey.</param>
        public SurveyService(IStrategyContext strategyContext)
        {
            _strategyContext = strategyContext;
        }

        public async Task<IEnumerable<FieldChoice>> GetFieldChoicesByFieldId(string fieldId)
        {
            try
            {
                var fieldChoice = await _strategyContext.FieldChoices.ReadAll(fieldId);
                return fieldChoice;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with fetching the choices for a survey field", e);
            }
        }

        public async Task<SurveyField> GetFieldById(string surveyId, string fieldId)
        {
            return await _strategyContext.SurveyFields.Read(surveyId, fieldId);
        }
    }
}