using System;
using System.Collections.Generic;
using API.Models.Auth;
using API.Models.Projects;

namespace API.Models.Admin
{
    public class OverviewUser
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string Name => $"{FirstName} {LastName}";
        public bool Active { get; set; }
        public bool PrivacyPolicy { get; set; }
        public string ImageUrl { get; set; }
        public DeviceToken Device { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        
        public bool IsAdmin { get; set; }

        public List<UserProject> Projects { get; set; }

        public static OverviewUser FromAuthUser(AuthUser authUser)
        {
            return new OverviewUser()
            {
                Id = authUser.Id,
                Email = authUser.Email,
                FirstName = authUser.FirstName,
                LastName = authUser.LastName,
                ImageUrl = authUser.ImageUrl,
                PhoneNumber = authUser.PhoneNumber,
                PrivacyPolicy = authUser.PrivacyPolicy,
                Active = authUser.Active,
                Device = authUser.Device,
                CreatedAt = authUser.CreatedAt,
                UpdatedAt = authUser.UpdatedAt,
                Projects = new List<UserProject>(),
                IsAdmin = false,
            };
        }
    }
}