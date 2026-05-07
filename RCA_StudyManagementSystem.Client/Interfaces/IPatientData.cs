using RCA_StudyManagementSystem.Shared.Domain;

namespace RCA_StudyManagementSystem.Client.Interfaces
{
    public interface IPatientData
    {
        Task<IEnumerable<Patient>> ListPatientsAsync();

        Task<Patient> GetPatientAsync(Guid id);

        Task<Patient> GetPatientByCaseNumberAsync(string caseNumber);

        Task<List<Patient>> GetPatientHistoryAsync(Guid id);

        Task<List<PatientPhoneNumber>> GetPatientPhoneNumberHistoryAsync(Guid id);

        Task<List<PathReport>> GetPathReportHistoryAsync(Guid id);

        Task<Patient> GetPatientExportHistoryAsync(Guid id);

        Task<Guid> GetPatientIdByCCRNoAsync(string ccrno);

        Task<string> ClearCCRNosAsync();

        Task<string> GetPatientPrimaryPhoneAsync(Guid id);

        Task<bool> CheckSSNAsync(string ssn);

        Task<bool> CheckDOBAsync(DateTime? dob);

        Task<bool> CheckNameAsync(string name);

        Task<Guid> CreatePatientAsync(string userId, Patient patient);

        Task UpdatePatientAsync(Guid id, string userId, Patient patient);

        Task DeletePatientAsync(Guid id);

        Task<string> GetLastCaseNumberAsync(string prefix);
    }
}
