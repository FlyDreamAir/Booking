using ClientServices = FlyDreamAir.Client.Services;
using FlyDreamAir.Components;
using FlyDreamAir.Components.Account;
using FlyDreamAir.Data;
using FlyDreamAir.Data.Seeders;
using FlyDreamAir.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using PostmarkDotNet;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Secrets.json", optional: true);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();

var adminDomain = builder.Configuration["Admin:Domain"];
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("FlyDreamAirEmployee", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.FindFirst(ClaimTypes.Email)?.Value?.EndsWith($"@{adminDomain}")
                ?? false
        );
    });
});
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException("Google OAuth ClientId not found.");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
            ?? throw new InvalidOperationException("Google OAuth ClientSecret not found.");
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityEmailSender>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IEmailService, NoOpEmailService>();
}
else
{
    builder.Services.AddSingleton(new PostmarkClient(builder.Configuration["ApiKeys:Postmark"]
        ?? throw new InvalidOperationException("Postmark API key not found.")));
    builder.Services.AddSingleton<IEmailService, PostmarkEmailService>();
}

builder.Services.AddControllers();

builder.Services.AddScoped<AddOnService>();
builder.Services.AddScoped<AirportsService>();
builder.Services.AddScoped<BookingsService>();
builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<FlightsService>();

// Hacks for prerendering to work.
builder.Services.AddScoped(sp => new HttpClient());
builder.Services.AddScoped<ClientServices.BookingsService>();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.SeedBookingData();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FlyDreamAir.Client._Imports).Assembly);

app.MapControllers();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
