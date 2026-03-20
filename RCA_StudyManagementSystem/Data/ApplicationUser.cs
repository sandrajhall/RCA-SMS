using Microsoft.AspNetCore.Identity;

namespace RCA_StudyManagementSystem.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName => string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) ? UserName : $"{FirstName} {LastName}".Trim();
    }

}
