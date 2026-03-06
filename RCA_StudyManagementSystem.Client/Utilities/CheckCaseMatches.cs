using Microsoft.AspNetCore.Components;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Client.Utilities
{
    public class CheckCaseMatches
    {
        [Inject]
        public static PatientData _patientData { get; set; }
        public CheckCaseMatches(PatientData patientData)
        {
            _patientData = patientData;
         }

        public static async Task<bool> DoesCaseMatchAsync(string lastName, string firstName, DateTime? dob, string ssn)
        {
            if (string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(firstName) ||
                dob != null)
            {
                return false; // If any input is null or empty, return false
            }

            var patients = await _patientData.ListPatientsAsync();

            // Normalize inputs by trimming whitespace and converting to lowercase
            lastName = lastName.Trim().ToLower();
            firstName = firstName.Trim().ToLower();

            if (ssn == "000-00-0000" || ssn == "999-99-9999")
            {
                ssn = string.Empty; // Treat these SSNs as empty
            }
            else
            {
                ssn = ssn.Trim().ToLower();
            }

            if (!string.IsNullOrWhiteSpace(ssn))
            {
                if (patients.Any(p => p.SocialSecurityNumber == ssn))
                {
                    return true;
                }
            }

            return patients.Any(p => p.LastName == lastName && p.FirstName == firstName && p.DateOfBirth == dob);
        }
    }
}
