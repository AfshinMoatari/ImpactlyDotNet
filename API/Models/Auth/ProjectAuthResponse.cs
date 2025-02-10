using API.Models.Projects;

namespace API.Models.Auth
{
    public class ProjectAuthResponse
    {
        public Authorization Authorization { get; set; }
        public Project Project { get; set; }
        public ProjectUser User { get; set; }
    }
}