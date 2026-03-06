using Microsoft.AspNetCore.Components;
using RCA_StudyManagementSystem.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCA_StudyManagementSystem.Client.Utilities
{
    public class GenerateBatchNumber
    {
        private readonly BatchData _batchData;
        public GenerateBatchNumber(BatchData batchData)
        {
            _batchData = batchData;
        }
        public async Task<string> Generate(string prefix)
        {
            string batchnumber = string.Empty;
            string lastBatchNumber = string.Empty;
            //// Get last batch number from the database
            //lastBatchNumber = await _batchData.GetLastBatchNumberAsync(prefix);
            //// Strip the study prefix
            //if (!string.IsNullOrEmpty(lastBatchNumber) && lastBatchNumber.StartsWith(prefix))
            //{
            //    lastBatchNumber = lastBatchNumber.Substring(prefix.Length + 1); // +1 for the hyphen
            //}
            //// Convert to int and increment by 1
            //int lastBatchNumberInt = 0;
            //if (int.TryParse(lastBatchNumber, out lastBatchNumberInt))
            //{
            //    lastBatchNumberInt++;
            //}
            //else
            //{
            //    lastBatchNumberInt = 1; // If parsing fails, start from 1
            //}
            // Add leading zeros if necessary
            //batchnumber = lastBatchNumberInt.ToString("D5"); // 5 digits with leading zeros
            
            var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            batchnumber = $"{prefix}-{dateStr}";
        
            return batchnumber;
        }
       
    }
}
