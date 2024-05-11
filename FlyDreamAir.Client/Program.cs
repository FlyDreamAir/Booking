using FlyDreamAir.Client;
using FlyDreamAir.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Security.Claims;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var adminDomain = builder.Configuration["Admin:Domain"];
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("FlyDreamAirEmployee", policy =>
    {
        policy.RequireAssertion(context =>
            context.User.FindFirst(ClaimTypes.Email)?.Value?.EndsWith($"@{adminDomain}")
                ?? false
        );
    });
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    });
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<BookingsService>();
builder.Services.AddScoped<NewsService>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
