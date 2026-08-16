using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Register SDK services (Castle.Windsor DI + ASP.NET Core pipeline)
builder.Services.AddSdkWebApi(builder.Environment);

var app = builder.Build();

// Configure SDK middleware (Swagger, JWT handler, controllers)
app.UseSdkWebApi(builder.Environment);

app.Run();
