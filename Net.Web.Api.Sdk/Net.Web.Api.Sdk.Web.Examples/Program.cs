using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Add SDK services (Castle Windsor container + SDK services)
builder.Services.AddWebApiSdk();

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<Net.Web.Api.Sdk.Documentation.Filters.SwaggerConsumesFilter>();
    c.OperationFilter<Net.Web.Api.Sdk.Documentation.Filters.SwaggerProducesFilter>();
    c.OperationFilter<Net.Web.Api.Sdk.Documentation.Filters.SwaggerUploadOperationFilter>();
    c.OperationFilter<Net.Web.Api.Sdk.Documentation.Filters.SwaggerSecurityTypeAttributeFilter>();
    c.DocumentFilter<Net.Web.Api.Sdk.Documentation.Filters.SwaggerMethodOrderingFilter>();
    c.DocumentFilter<Net.Web.Api.Sdk.Documentation.Filters.SwaggerOperationOrderingFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseWebApiSdk();
app.UseAuthorization();
app.MapControllers();

app.Lifetime.ApplicationStopping.Register(WebApiSdkExtensions.DisposeWebApiSdk);

app.Run();
