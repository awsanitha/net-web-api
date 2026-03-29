using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

var rootPath = builder.Environment.ContentRootPath;
var baseUrl = builder.Configuration["BaseUrl"] ?? "http://localhost:5000";

builder.Services.AddWebApiSdk(rootPath, baseUrl);

var app = builder.Build();

app.UseWebApiSdk();

app.Run();
