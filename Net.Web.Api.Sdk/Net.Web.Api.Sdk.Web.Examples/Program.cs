using Microsoft.AspNetCore.Authentication.JwtBearer;
using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Register all SDK services: controllers, JSON, CORS, API versioning, Swagger, JWT, file, info services.
// Also scans assemblies for [InjectInterfaceService]-marked interfaces (registers IPrimeNumberService, etc.)
builder.Services.AddSdkApiServices();

// Register authentication scheme (JWT Bearer) — token validation is handled by the SDK's
// JwtTokenMiddleware and TokenAuthorizeAttribute; this wires the default scheme so the
// Authorization middleware has a scheme to reference.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

var app = builder.Build();

// Serve static files from wwwroot/ (images, icons, logos)
app.UseStaticFiles();

// SDK middleware: JWT token handler, CORS, Swagger, embedded resource extraction
app.UseSdkApiMiddleware();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
