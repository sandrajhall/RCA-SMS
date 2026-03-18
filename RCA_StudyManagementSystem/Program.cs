using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using RCA_StudyManagementSystem.Client.Pages;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Client.Utilities;
using RCA_StudyManagementSystem.Components;
using RCA_StudyManagementSystem.Components.Account;
using RCA_StudyManagementSystem.Components.Account.Pages;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using RCA_StudyManagementSystem.Shared;
using System.Text.Json.Serialization;
using RCA_StudyManagementSystem.Data;


var builder = WebApplication.CreateBuilder(args);

// Add output caching services
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("DoctorTagPolicy", policy =>
    {
        policy.Expire(TimeSpan.FromDays(7)); // A long expiration, relying on tag invalidation
        policy.Tag("doctor-api");
    });
    options.AddPolicy("PatientTagPolicy", policy =>
    {
        policy.Expire(TimeSpan.FromDays(7));
        policy.Tag("patient-api");
    });
    options.AddPolicy("HistologyTagPolicy", policy =>
    {
        policy.Expire(TimeSpan.FromDays(7));
        policy.Tag("histology-api");
    });
});

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddAntiforgery();

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

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Register HttpClient
builder.Services.AddScoped(sp =>
{
    NavigationManager navigation = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
});


builder.Services.AddLogging(builder =>
{
    builder.AddConsole(); // Adds console logging
    builder.SetMinimumLevel(LogLevel.Debug); // Sets default minimum log level
    // Add other providers as needed, e.g., builder.AddAzureWebAppDiagnostics();
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

builder.Services.AddSwaggerGen();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.MapGet("/", () => Results.Redirect("/Account/Login"));

//app.MapGet("/", (HttpContext ctx) =>
//{
//    return ctx.User?.Identity?.IsAuthenticated == true
//        ? Results.Redirect("/app/")
//        : Results.Redirect("/Account/Login");
//});

app.MapPost("/account/login-post", async (
    HttpContext http,
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager) =>
{
    var form = await http.Request.ReadFormAsync();
    var email = form["Input.Email"].ToString();
    var password = form["Input.Password"].ToString();
    var rememberMe = form["Input.RememberMe"].Count > 0;

    var user = await userManager.FindByEmailAsync(email);
    if (user is null)
    {
        return Results.Redirect("/Account/Login?error=InvalidLogin", permanent: false);
    }

    var result = await signInManager.PasswordSignInAsync(
        user.UserName!, password,
        isPersistent: rememberMe,
        lockoutOnFailure: true);

    return result.Succeeded
    ? Results.Redirect("/app", permanent: false)
    : Results.Redirect("/Account/Login?error=InvalidLogin", permanent: false);

    return Results.Redirect("/Account/Login?error=InvalidLogin", permanent: false);
}).DisableAntiforgery();

app.UseHttpsRedirection();


app.MapStaticAssets();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapRazorPages();
app.MapControllers();
//app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/Account"), accountApp =>
//{
//    // Let the normal endpoint routing handle /Account/*.
//    // IMPORTANT: do NOT map RazorComponents here.
//    accountApp.UseRouting();
//    accountApp.UseAuthentication();
//    accountApp.UseAuthorization();
//    accountApp.UseAntiforgery();

//    accountApp.UseEndpoints(endpoints =>
//    {
//        // Map Identity endpoints and anything else needed under /Account
//        endpoints.MapAdditionalIdentityEndpoints();
//        endpoints.MapRazorPages();
//        endpoints.MapControllers();
//    });
//});

app.MapRazorComponents<RCA_StudyManagementSystem.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(RCA_StudyManagementSystem.Shared.Layout.MainLayout).Assembly);

app.Run();
