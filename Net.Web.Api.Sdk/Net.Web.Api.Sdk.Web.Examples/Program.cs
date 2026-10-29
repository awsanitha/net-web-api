using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Replaces Global.asax Application_Start: GlobalConfiguration.Configuration.RegisterWebApi().
builder.Services.AddNetWebApiSdk(builder.Configuration, builder.Environment);

var app = builder.Build();

// Replaces the Web API pipeline set up by RegisterWebApi and, on shutdown, the
// Application_End call to UnRegisterWebApi (registered as an application-stopping hook).
app.UseNetWebApiSdk();

app.Run();
