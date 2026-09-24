using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Web.Api.Sdk.Initialization;
using Net.Web.Api.Sdk.Models;

var builder = WebApplication.CreateBuilder(args);

// Register SDK services (controllers, API versioning, Swagger, JWT handler, file services, etc.)
builder.Services.AddSdkServices(options =>
{
    var uploadSection = builder.Configuration.GetSection("UploadSettings");
    options.MaxAllowedUploadSize = uploadSection.GetValue<long>("MaxAllowedUploadSize");
    var mimeTypes = uploadSection.GetSection("AllowedMimeTypes").Get<string[]>();
    if (mimeTypes != null)
    {
        options.AllowedMimeTypes = mimeTypes;
    }
});

// Add Newtonsoft.Json support for controllers (already configured in AddSdkServices, but ensuring it's wired)
builder.Services.AddControllers().AddNewtonsoftJson();

// Configure CORS (equivalent to [EnableCors("*", "*", "*")] in the original)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Copy token-example.config to content root so JwtTokenService can find it
var tokenConfigSource = Path.Combine(app.Environment.ContentRootPath, "token-example.config");
if (File.Exists(tokenConfigSource))
{
    // The SDK's JwtTokenService loads token*.config from the content root,
    // which is already where token-example.config lives (CopyToOutputDirectory preserves it).
    // No additional copy needed when running from the project directory.
}

// Use SDK middleware (JWT handler, Swagger, static files, token DB cleanup)
app.UseSdkMiddleware();

// Enable CORS
app.UseCors();

// Map controller routes
app.MapControllers();

app.Run();
