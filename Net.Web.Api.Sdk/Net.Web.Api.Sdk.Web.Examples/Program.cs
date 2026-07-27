using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Add SDK services (controllers, JWT, Swagger, DI, CORS, API versioning)
builder.Services.AddWebApiSdk();

var app = builder.Build();

// Use SDK middleware pipeline
app.UseWebApiSdk();

app.Run();
