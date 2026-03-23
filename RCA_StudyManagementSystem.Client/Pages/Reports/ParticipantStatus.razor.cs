using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using MudBlazor;
using RCA_StudyManagementSystem.Client.Pages.Archives;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using static MudBlazor.CategoryTypes;

namespace RCA_StudyManagementSystem.Client.Pages.Reports
{
    public partial class ParticipantStatus : Microsoft.AspNetCore.Components.ComponentBase
    {
        private List<PatientStatusView> Rows = new();

        private string _searchString = string.Empty;


        private IEnumerable<Study> StudyList = new List<Study>();
        private CancellationToken token;

        public Guid StudyId { get; set; }
        public string StudyColor { get; set; }

        protected Study studySelectValue;
        protected string studySelectText;
        protected string studyPrefix;

        private string startDateStr;
        private string endDateStr;

        private DateTime? startDate { get; set; }
        private DateTime? endDate { get; set; }

        private string SearchString
        {
            get => _searchString;
            set
            {
                if (_searchString != value)
                {
                    _searchString = value;
                }
            }
        }



        protected async override void OnInitialized()
        {

            StudyList = await StudyData.ListStudiesAsync();
            if (StudyId != Guid.Empty)
            {
                var study = await StudyData.GetStudyAsync(StudyId);
                if (study != null)
                {
                    await OnStudySelectChanged(study);
                }
            }
            startDateStr = DateTime.UtcNow.ToString("yyyy-MM-dd");
            endDateStr = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");

            await LoadGrid();
        }

        private async Task LoadGrid()
        {
            Rows.Clear();

            if (StudyId != Guid.Empty)
            {

                Rows = await PatientStatusData.ListPatientStatusesByStudyIdAsync(StudyId, startDateStr, endDateStr);
            }

        }



        private async Task OnStudySelectChanged(Study value)
        {
            studySelectValue = value;
            studySelectText = value.Name;
            studyPrefix = value.Prefix;

            StudyId = value.StudyId; // Update the StudyId for the form

            StudyColor = value.ColorLight; // Update the color based on the selected study

            //await LoadGrid();

            //await InvokeAsync(StateHasChanged);
        }

        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            if (StudyId == Guid.Empty)
            {
                ShowError();
                return;
            }

            // 1. Register encoding provider (required for ExcelDataReader)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            var file = e.File;
            using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using var ms = new MemoryStream();

            await stream.CopyToAsync(ms);
            ms.Position = 0;

            // 2. Create the reader from the stream
            using var reader = ExcelReaderFactory.CreateReader(ms);

            // 3. (Optional) Convert to DataSet for easy access to Tables/Rows
            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    //UseHeaderRow = true // Uses first row as column names
                }
            });
            var PStatusList = new List<PatientStatus>();

            // Example: Access the first sheet's data
            DataTable table = result.Tables[0];
            int rowIndex = 0;
            foreach (DataRow row in table.Rows)
            {
                if (row[0].ToString().Contains("PARTICIPANT"))
                {
                    rowIndex++;
                    continue; // Skip title row
                }
                else
                {
                    if (rowIndex == 0)
                    {

                        Snackbar.Add("You must have a properly formatted file that starts with PARTICIPANT in the title row.", Severity.Error, options =>
                        {
                            options.RequireInteraction = true; // User must manually dismiss
                        });
                    }
                }
                if (row[0].ToString() == "Last Name")
                {
                    rowIndex++;
                    continue; // Skip header row
                }
                else
                {
                    if (rowIndex == 1)
                    {
                        Snackbar.Add("You must have a properly formatted file with Last Name as the first header.", Severity.Error, options =>
                        {
                            options.RequireInteraction = true; // User must manually dismiss
                        });
                    }
                }
                var Patient = await PatientData.GetPatientByCaseNumberAsync(row[2].ToString());
                var pStatus = new PatientStatus();  
                var studyPrefix = row[2].ToString().Split('-')[0];
                if(studyPrefix != studyPrefix)
                {
                    Snackbar.Add($"Case number {row[2].ToString()} does not match the selected study prefix.", Severity.Error, options =>
                    {
                        options.RequireInteraction = true; // User must manually dismiss
                    });
                    break; // Stop processing if there's a mismatch in study prefix
                }
                pStatus.PatientId = Patient.PatientId;
                pStatus.Date = DateTime.UtcNow;
                pStatus.CaseNumber = row[2].ToString();
                pStatus.NoContact = row[4].ToString().ToLower() == "y" ? true : false;
                // set the string value by calling a method that checks the value and returns the appropriate string
                var statusValue = row[5].ToString();
                var statusString = await LookupData.GetTypeByCodeAsync("PatientStatus", statusValue);
                pStatus.Status = statusString;
                pStatus.DateOfDeath = DateTime.TryParse(row[6].ToString(), out DateTime dateOfDeath) ? dateOfDeath : (DateTime?)null;
                pStatus.DateLastContact = DateTime.TryParse(row[7].ToString(), out DateTime dateLastContact) ? dateLastContact : (DateTime?)null;
                pStatus.InformedOfCancerDiagnosis = row[8].ToString().ToLower() == "y" ? true : false;
                pStatus.StatedNoCancerDiagnosis = row[9].ToString().ToLower() == "y" ? true : false;
                pStatus.Comments = row[10].ToString();

                pStatus.StudyId = StudyId;

                PStatusList.Add(pStatus);
                rowIndex++;
            }

            foreach (var item in PStatusList)
            {
                var auth = await AuthStateProvider.GetAuthenticationStateAsync();
                var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var existingStatus = await PatientStatusData.GetPatientStatusByCaseNumberAsync(item.CaseNumber);
                if (existingStatus.CaseNumber != null)
                {
                    item.PatientStatusId = existingStatus.PatientStatusId; // Ensure the ID is set for update
                    await PatientStatusData.UpdatePatientStatusAsync(existingStatus.PatientStatusId, userId, item);
                }
                else
                {
                    await PatientStatusData.CreatePatientStatusAsync(userId, item);
                }
            }
            await LoadGrid();

            await InvokeAsync(StateHasChanged);
        }

        public async Task<DataGridEditFormAction> SaveCommentsAsync(PatientStatusView item)
        {
            var auth = await AuthStateProvider.GetAuthenticationStateAsync();
            var userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var existingStatus = await PatientStatusData.GetPatientStatusByCaseNumberAsync(item.CaseNumber);
            existingStatus.Comments = item.Comments;

            await PatientStatusData.UpdatePatientStatusAsync(existingStatus.PatientStatusId, userId, existingStatus);
            
            return DataGridEditFormAction.KeepOpen;
        }

        public async Task Generate()
        {
            if (StudyId == Guid.Empty)
            {
                ShowError();
                return;
            }
            if (startDate == null || endDate == null)
            {
                Snackbar.Add("Please select a valid start and end date.", Severity.Error, options =>
                {
                    options.RequireInteraction = true; // User must manually dismiss
                });
                return;
            }

            startDateStr = startDate.Value.ToString("yyyy-MM-dd");
            endDateStr = endDate.Value.AddDays(1).ToString("yyyy-MM-dd");

            Rows = await PatientStatusData.ListPatientStatusesByStudyIdAsync(StudyId, startDateStr, endDateStr);
        }

        private async Task OnExport()
        {
            if (StudyId == Guid.Empty || !startDate.HasValue || !endDate.HasValue)
            {
                Snackbar.Add("Please select a study and date range before downloading.", Severity.Warning, options =>
                {
                    options.RequireInteraction = true; // User must manually dismiss
                });
                return;
            }
            var Study = await StudyData.GetStudyAsync(StudyId);
            // create a new batch

            var batchPrefix = "EXP-" + Study.Prefix;


            var exportData = await PatientStatusData.ListPatientStatusesByStudyIdCSVAsync(StudyId, startDate.Value.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture), endDate.Value.AddDays(1).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture));

            var dateStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (exportData != null)
            {
                // Call JavaScript function to download CSV
                await JSRuntime.InvokeVoidAsync("downloadFile", $"{Study.Prefix}_ParticipantStatus_{dateStr}.csv", exportData);
            }
            else
            {
                Snackbar.Add("No data available for export.", Severity.Warning, options =>
                {
                    options.RequireInteraction = true; // User must manually dismiss
                });
            }


        }

        private void ShowError()
        {
            Snackbar.Add("You must select a study before uploading.", Severity.Error, options =>
            {
                options.RequireInteraction = true; // User must manually dismiss
            });
        }

    }

}
