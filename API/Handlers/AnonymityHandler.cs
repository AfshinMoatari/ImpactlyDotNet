using API.Constants;
using API.Models.Projects;
using System.Collections.Generic;

namespace API.Handlers
{
    public interface IAnonymityHandler
    {
        public ProjectPatient HidePatient(ProjectPatient projectPatient);
        public List<ProjectPatient> HidePatients(IEnumerable<ProjectPatient> projectPatients);
    }

    public class AnonymityHandler: IAnonymityHandler
    {
        public ProjectPatient HidePatient(ProjectPatient projectPatient)
        {
            if (!projectPatient.Anonymity)
            {
                return projectPatient;
            }
            else
            {
                projectPatient.FirstName = AnonymityMessage.HiddingMessage;
                projectPatient.LastName = AnonymityMessage.HiddingMessage;
                projectPatient.Email = AnonymityMessage.HiddingMessage;
                projectPatient.PhoneNumber = AnonymityMessage.HiddingMessage;
                projectPatient.Sex = AnonymityMessage.HiddingMessage;
                projectPatient.Municipality = AnonymityMessage.HiddingMessage;
                projectPatient.Region = AnonymityMessage.HiddingMessage;
                projectPatient.PostalNumber = AnonymityMessage.HiddingMessage;
                projectPatient.BirthDate = null;
            }
            return projectPatient;
        }

        public List<ProjectPatient> HidePatients(IEnumerable<ProjectPatient> projectPatients)
        {
            var hiddenProjectPatients = new List<ProjectPatient>(); 
            foreach (var projectPatient in projectPatients)
            {
                if (projectPatient.Anonymity)
                {
                    projectPatient.FirstName = AnonymityMessage.HiddingMessage;
                    projectPatient.LastName = AnonymityMessage.HiddingMessage;
                    projectPatient.Email = AnonymityMessage.HiddingMessage;
                    projectPatient.PhoneNumber = AnonymityMessage.HiddingMessage;
                    projectPatient.Sex = AnonymityMessage.HiddingMessage;
                    projectPatient.Municipality = AnonymityMessage.HiddingMessage;
                    projectPatient.Region = AnonymityMessage.HiddingMessage;
                    projectPatient.PostalNumber = AnonymityMessage.HiddingMessage;
                    projectPatient.BirthDate = null;
                }
                hiddenProjectPatients.Add(projectPatient);
            }
            
            return hiddenProjectPatients;
        }
    }
}
