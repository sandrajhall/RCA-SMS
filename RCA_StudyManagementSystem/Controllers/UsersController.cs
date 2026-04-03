using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RCA_StudyManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace RCA_StudyManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("by-email/{email}")]
        public async Task<string> GetIdByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Guid.Empty.ToString();
            return user.Id;
        }

        [HttpGet("displayname/{id}")]
        public async Task<string> GetDisplayName(string id)
        {
            if (id == null || id == Guid.Empty.ToString()) return string.Empty;
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return string.Empty;
            return user.DisplayName;
        }

        [HttpGet("all")]
        public async Task<Dictionary<string, string>> GetAllUsers()
        {
            return await _userManager.Users
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? "Unknown");

        }
    }
}
