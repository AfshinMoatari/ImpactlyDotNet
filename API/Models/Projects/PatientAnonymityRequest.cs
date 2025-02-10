namespace API.Models.Projects;

public class PatientAnonymityRequest
{
    public string PatientId { get; set; }
    public bool Anonymity { get; set; }
}