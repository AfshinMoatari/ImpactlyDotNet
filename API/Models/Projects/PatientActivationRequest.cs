namespace API.Models.Projects;

public class PatientActivationRequest
{
    public string PatientId { get; set; }
    public bool IsActive { get; set; }
} 