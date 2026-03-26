using Blazored.LocalStorage;
using Humanizer.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using RCA_StudyManagementSystem.Client.Pages;
using RCA_StudyManagementSystem.Client.Services;
using RCA_StudyManagementSystem.Client.Utilities;
using RCA_StudyManagementSystem.Components;
using RCA_StudyManagementSystem.Components.Account;
using RCA_StudyManagementSystem.Components.Account.Pages;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Data;
using RCA_StudyManagementSystem.Services;
using RCA_StudyManagementSystem.Shared;
using RCA_StudyManagementSystem.Shared.Domain;
using RCA_StudyManagementSystem.Shared.ViewModels;
using System.Text.Json.Serialization;


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
builder.Services.AddAuthorization();

builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddAntiforgery();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
builder.Services.AddScoped<AuditInterceptor>();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options
        .UseSqlServer(connectionString)
        .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddServerSideBlazor()
    .AddHubOptions(options => {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
    })
    .AddCircuitOptions(options => {
        options.DetailedErrors = true; // Shows the real error in the browser console
    });

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromHours(2);   // or as needed
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.Lax;       // or .None (if cross-origin and HTTPS)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    // options.Cookie.Domain = "yourdomain.com"; // Only if using subdomains
});

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
builder.Services.AddScoped<UserContext>();
builder.Services.AddScoped<UserData>();

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

builder.Services.AddDataProtection()
    .SetApplicationName("RCA_StudyManagementSystem")
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "dp_keys")));

var app = builder.Build();

// --- ROLE SEEDING HERE ---
//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//    string[] roleNames = { "Admin", "Business", "User" };
//    foreach (var roleName in roleNames)
//    {
//        var roleExists = await roleManager.RoleExistsAsync(roleName);
//        if (!roleExists)
//            await roleManager.CreateAsync(new IdentityRole(roleName));
//    }
   
//    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
//    var userGail = await userManager.FindByEmailAsync("gail.cooley@dhhs.nc.gov");
//    var userJenn = await userManager.FindByEmailAsync("jennifer.oneill@dhhs.nc.gov");
//    var userSandy = await userManager.FindByEmailAsync("shall@med.unc.edu");
//    var userHeather = await userManager.FindByEmailAsync("heather_tipaldos@unc.edu");
//    var userEmma = await userManager.FindByEmailAsync("emma_blackwell@med.unc.edu");
//    var userJanae = await userManager.FindByEmailAsync("janae_simons@med.unc.edu");
//    var userSystem = await userManager.FindByEmailAsync("system_user@system.user");

//    if (userGail != null)
//    {
//        await userManager.AddToRoleAsync(userGail, "User");
//    }
//    if (userJenn != null)
//    {
//        await userManager.AddToRoleAsync(userJenn, "User");
//    }
//    if (userSandy != null)
//    {
//        await userManager.AddToRoleAsync(userSandy, "Admin");
//    }
//    if (userHeather != null)
//    {
//        await userManager.AddToRoleAsync(userHeather, "Business");
//    }
//    if (userEmma != null)
//    {
//        await userManager.AddToRoleAsync(userEmma, "Business");
//    }
//    if (userJanae != null)
//    {
//        await userManager.AddToRoleAsync(userJanae, "Admin");
//    }
//    if (userSystem != null)
//    {
//        await userManager.AddToRoleAsync(userSystem, "Admin");
//    }
//}




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

app.MapGet("/", (HttpContext ctx) =>
{
    return ctx.User?.Identity?.IsAuthenticated == true
        ? Results.Redirect("/app", permanent: false)
        : Results.Redirect("/Account/Login", permanent: false);
});

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

app.MapGet("/logout", async (
    HttpContext http,
    SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();

    var returnUrl = http.Request.Query["returnUrl"].ToString();
    if (string.IsNullOrWhiteSpace(returnUrl))
        returnUrl = "/";

    if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        returnUrl = "/";

    return Results.LocalRedirect(returnUrl);
});

app.UseHttpsRedirection();
app.MapStaticAssets();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.UseAntiforgery(); // <-- Put this here, before any endpoint mapping

// Map endpoints
app.MapAdditionalIdentityEndpoints();
app.MapRazorPages();
app.MapControllers();
app.MapRazorComponents<RCA_StudyManagementSystem.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(RCA_StudyManagementSystem.Shared.Layout.MainLayout).Assembly);

app.Run();


