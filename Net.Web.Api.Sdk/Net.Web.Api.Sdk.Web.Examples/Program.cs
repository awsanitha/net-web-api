using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Initialization;
using Net.Web.Api.Sdk.Web.Examples.Classes.Implementations;
using Net.Web.Api.Sdk.Web.Examples.Classes.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Add CORS (allow all origins for example purposes)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Web API SDK services (registers IJwtTokenService, IFileService, IInformationService, etc.)
builder.Services.AddWebApiSdk();

// Register example-specific services
builder.Services.AddSingleton<IPrimeNumberService, PrimeNumberService>();

var app = builder.Build();

// Use CORS
app.UseCors();

// Configure SDK middleware (JWT handler, extract embedded resources)
app.UseWebApiSdk();

// Use routing and authorization
app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Setup Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Net.Web.Api.Sdk.Web.Examples v1");
});

app.Run();
