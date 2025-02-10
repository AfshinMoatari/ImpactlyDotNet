using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Models.Projects;
using API.Models.Strategy;
using API.Repositories;

namespace API.Services
{
    public interface IPatientService
    {
        /// <summary>
        /// Update the patient record with strategy information
        /// </summary>
        /// <returns>A list of newly modified patients with strategyId/strategyName</returns>
        public Task<List<ProjectPatient>> CreateStrategyPatients(List<StrategyPatient> strategyPatients, string projectId, string strategyId, string strategyName);
        /// <summary>
        /// Gets all the patients by their strategyId
        /// </summary>
        /// <returns>A list of patients assigned to the strategyId.</returns>
        public Task<IEnumerable<ProjectPatient>> GetStrategyPatients(string strategyId);
        /// <summary>
        /// Remove and update the strategy name/id from project patients
        /// </summary>
        /// <returns>A list of project patients with sucessfully removed strategy.</returns>
        public Task<List<ProjectPatient>> DeleteStrategyPatients(string projectId, List<StrategyPatient> strategyPatients);
        /// <summary>
        /// Update the strategy name for project patients
        /// </summary>
        /// <returns>A list of project patients with sucessfully updated strategy name.</returns>
        public Task<List<ProjectPatient>> UpdateStrategyPatients(string projectId, List<StrategyPatient> strategyPatients, string strategyName);
    }

    /// <summary>
    /// Service class for managing projects.
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly IPatientContext _patientContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PatientService"/> class.
        /// </summary>
        /// <param name="patientContext">The _patientContext.</param>
        public PatientService(IPatientContext patientContext)
        {
            _patientContext = patientContext;
        }

        public async Task<List<ProjectPatient>> CreateStrategyPatients(List<StrategyPatient> strategyPatients, string projectId, string strategyId, string strategyName)
        {
            var results = new List<ProjectPatient>();
            try
            {
                foreach (var strategyPatient in strategyPatients)
                {
                    try
                    {
                        var result = await _patientContext.ProjectPatients.UpdateValue(
                            projectId, strategyPatient.Id, p =>
                            {
                                p.StrategyId = strategyId;
                                p.StrategyName = strategyName;
                            });
                        results.Add(result);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ArgumentException($"Something went wrong with fetching the patientId: {strategyPatient.Id} and projectId: {projectId}", e);
                    }
                }
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with updating the project patients", e);
            }
            return results;
        }

        public async Task<IEnumerable<ProjectPatient>> GetStrategyPatients(string strategyId)
        {
            try
            {
                return await _patientContext.GetPatientsByStrategyId(strategyId);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with getting all the patients for strategyId: {strategyId}", e);
            }
        }

        public async Task<List<ProjectPatient>> DeleteStrategyPatients(string projectId, List<StrategyPatient> strategyPatients)
        {
            var results = new List<ProjectPatient>();

            foreach (var strategyPatient in strategyPatients)
            {
                try
                {
                    var result = await _patientContext.ProjectPatients.UpdateValue(
                        projectId, strategyPatient.Id, p =>
                        {
                            p.StrategyId = string.Empty;
                            p.StrategyName = string.Empty;
                        });
                    results.Add(result);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException($"Something went wrong with deleting the strategyPatients: {strategyPatients}", e);
                }
            }
            return results;
        }

        public async Task<List<ProjectPatient>> UpdateStrategyPatients(string projectId, List<StrategyPatient> strategyPatients, string strategyName)
        {
            var results = new List<ProjectPatient>();

            foreach (var strategyPatient in strategyPatients)
            {
                try
                {
                    var result = await _patientContext.ProjectPatients.UpdateValue(
                        projectId, strategyPatient.Id, p =>
                        {
                            p.StrategyName = strategyName;
                        });
                    results.Add(result);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException($"Something went wrong with deleting the strategyPatients: {strategyPatients}", e);
                }
            }
            return results;
        }
    }
}