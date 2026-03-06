using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
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


// Register data services
builder.Services.AddScoped<PatientData>();
builder.Services.AddScoped<PathReportData>();
builder.Services.AddScoped<StudyData>();
builder.Services.AddScoped<StudyLookupData>();
builder.Services.AddScoped<LookupData>();
builder.Services.AddScoped<HistologyData>();
builder.Services.AddScoped<StudyHistologyData>();
builder.Services.AddScoped<StudyHeaderData>();
builder.Services.AddScoped<StudyReportHeaderData>();
builder.Services.AddScoped<HospitalData>();
builder.Services.AddScoped<DoctorData>();
builder.Services.AddScoped<DoNotContactData>();
builder.Services.AddScoped<BatchData>();
builder.Services.AddScoped<EmailData>();
builder.Services.AddScoped<ReimbursementEntityData>();
builder.Services.AddScoped<RCAContactData>();
builder.Services.AddScoped<PathReportExportData>();
builder.Services.AddScoped<InvoiceData>();
builder.Services.AddScoped<ReportData>();
builder.Services.AddScoped<DailyPathSubmissionData>();
builder.Services.AddScoped<PatientStatusData>();
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
