using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RCA_StudyManagementSystem.Data;
using System.Security.Claims;

namespace RCA_StudyManagementSystem.Components
{
    public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public ApplicationUserClaimsPrincipalFactory(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (!string.IsNullOrEmpty(user.DisplayName))
            {
                identity.AddClaim(new Claim("DisplayName", user.DisplayName));
            }

            return identity;
        }
    }
}