using Microsoft.AspNetCore.Components;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Client.Utilities
{
    public class GenerateCaseNumber
    {
        private readonly IPatientData _patientData;
        public GenerateCaseNumber(IPatientData patientData)
        {
            _patientData = patientData;
        }

        public async Task<string> Generate(string prefix)
        {

            string casenumber = string.Empty;
            string lastCaseNumber = string.Empty;

            // Get last case number from the database
            lastCaseNumber = await _patientData.GetLastCaseNumberAsync(prefix);

            // Strip the study prefix
            if (!string.IsNullOrEmpty(lastCaseNumber) && lastCaseNumber.StartsWith(prefix))
            {
                lastCaseNumber = lastCaseNumber.Substring(prefix.Length + 1); // +1 for the hyphen
            }

            // Convert to int and increment by 1
            int lastCaseNumberInt = 0;
            if (int.TryParse(lastCaseNumber, out lastCaseNumberInt))
            {
                lastCaseNumberInt++;
            }
            else
            {
                lastCaseNumberInt = 1; // If parsing fails, start from 1
            }

            // Add leading zeros if necessary
            casenumber = lastCaseNumberInt.ToString("D6"); // 6 digits with leading zeros

            // Add the study prefix back
            casenumber = $"{prefix}-{casenumber}";


            return casenumber;
        }
    }
}
