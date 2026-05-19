using Microsoft.AspNetCore.Identity;
using RCA_StudyManagementSystem.Data;

// This middleware is intended for development purposes only. It automatically logs in a specified user if no user is currently authenticated.


namespace RCA_StudyManagementSystem.Middleware
{
    public class AutoLoginMiddleware
    {
        private readonly RequestDelegate _next;

        // Only inject RequestDelegate here
        public AutoLoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Inject Scoped services directly into the method
        public async Task InvokeAsync(
                HttpContext context,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                // change user@email to the email of the user you want to auto-login as
                var testUser = await userManager.FindByEmailAsync("user@email");
                if (testUser != null)
                {
                    await signInManager.SignInAsync(testUser, isPersistent: true);
                }
            }
            await _next(context);
        }
    }
}
