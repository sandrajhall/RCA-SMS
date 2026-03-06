using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using RCA_StudyManagementSystem.Client.Pages;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Client.Utilities;
using RCA_StudyManagementSystem.Components;
using RCA_StudyManagementSystem.Components.Account;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.AddMudServices();

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



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(RCA_StudyManagementSystem.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
