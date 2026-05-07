namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IUserData
    {
        Task<string> GetIdByEmailAsync(string email);

        Task<string> GetDisplayNameAsync(string id);

        Task<Dictionary<string, string>> GetAllUsersAsync();
    }
}
