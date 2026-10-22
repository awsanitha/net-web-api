using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Register Web API SDK services (DI, versioning, Swagger, controllers, CORS, JWT middleware)
builder.Services.AddWebApiSdk();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure the SDK middleware pipeline (JWT middleware, Swagger UI, resource extraction)
app.UseWebApiSdk();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
