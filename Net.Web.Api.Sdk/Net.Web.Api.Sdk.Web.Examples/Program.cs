using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApiSdk();

var app = builder.Build();

app.UseWebApiSdk();

app.Run();
