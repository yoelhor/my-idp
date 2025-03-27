using Microsoft.Extensions.Logging.AzureAppServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Add Azure stream log service
builder.Logging.AddAzureWebAppDiagnostics();

// Enable Application Insights telemetry
//builder.Services.AddApplicationInsightsTelemetry();

// Add in memory cache                                                  
builder.Services.AddMemoryCache();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

// OpenID Connect well-known configuration and keys
app.MapControllerRoute(
    name: "oidc-config",
    pattern: ".well-known/openid-configuration",
    new { controller = "OpenIdConfiguration", action = "Index" });

app.MapControllerRoute(
    name: "oidc-jwks",
    pattern: ".well-known/keys",
    new { controller = "OpenIdKeys", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
