using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface ILookupData
    {
        Task<IEnumerable<Lookup>> ListLookupsAsync();

        Task<IEnumerable<string>> ListLookupsByTypeAsync(string type);

        Task<Lookup> GetLookupAsync(Guid id);

        Task<string> GetCountyByFIPSAsync(string fips);

        Task<string> GetTypeByCodeAsync(string type, string typeCode);

        Task<string> GetCodeByTypeAsync(string type, string typeValue);

        Task<Guid> CreateLookupAsync(string userId, Lookup lookup);

        Task UpdateLookupAsync(Guid id, string userId, Lookup lookup);

        Task DeleteLookupAsync(Guid id);
    }
}
