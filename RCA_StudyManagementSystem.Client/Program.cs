using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using RCA_StudyManagementSystem.Client;
using RCA_StudyManagementSystem.Client.Interfaces;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Client.Utilities;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddMudServices();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;

    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 1000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("Geoapify", client =>
{
    client.BaseAddress = new Uri("https://api.geoapify.com");
    // Add other configurations like default headers, timeouts, etc.
});

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Optional, but typical:
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


// Register data services
builder.Services.AddScoped<IPatientData, PatientData>();
builder.Services.AddScoped<IPathReportData, PathReportData>();
builder.Services.AddScoped<IStudyData, StudyData>();
builder.Services.AddScoped<IStudyLookupData, StudyLookupData>();
builder.Services.AddScoped<ILookupData, LookupData>();
builder.Services.AddScoped<IHistologyData, HistologyData>();
builder.Services.AddScoped<IStudyHistologyData, StudyHistologyData>();
builder.Services.AddScoped<IStudyHeaderData, StudyHeaderData>();
builder.Services.AddScoped<IStudyReportHeaderData, StudyReportHeaderData>();
builder.Services.AddScoped<IHospitalData, HospitalData>();
builder.Services.AddScoped<IDoctorData, DoctorData>();
builder.Services.AddScoped<IDoNotContactData, DoNotContactData>();
builder.Services.AddScoped<IBatchData, BatchData>();
builder.Services.AddScoped<IEmailData, EmailData>();
builder.Services.AddScoped<IReimbursementEntityData, ReimbursementEntityData>();
builder.Services.AddScoped<IRCAContactData, RCAContactData>();
builder.Services.AddScoped<IPathReportExportData, PathReportExportData>();
builder.Services.AddScoped<IInvoiceData, InvoiceData>();
builder.Services.AddScoped<IReportData, ReportData>();
builder.Services.AddScoped<IDailyPathSubmissionData, DailyPathSubmissionData>();
builder.Services.AddScoped<IPatientStatusData, PatientStatusData>();
builder.Services.AddScoped<IUserData, UserData>();
builder.Services.AddScoped<GenerateCaseNumber>();
builder.Services.AddScoped<GenerateBatchNumber>();

builder.Services.AddScoped<GridStateView<Patient>>(); // Register for a specific data item type
builder.Services.AddScoped<GridStateView<PathReportView>>(); // Register for a specific data item type
builder.Services.AddScoped<GridStateView<Hospital>>(); // Register for a specific data item type
builder.Services.AddScoped<GridStateView<Doctor>>(); // Register for a specific data item type
builder.Services.AddScoped<GridStateView<DoNotContact>>(); // Register for a specific data item type
builder.Services.AddScoped<GridStateView<RCAContact>>(); // Register for a specific data item type
builder.Services.AddScoped<GridStateView<ReimbursementEntity>>(); // Register for a specific data item type
builder.Services.AddScoped<GridStateView<Invoice>>(); // Register for a specific data item type

builder.Services.AddBlazoredLocalStorage();

await builder.Build().RunAsync();
