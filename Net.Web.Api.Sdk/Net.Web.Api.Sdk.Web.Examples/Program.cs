using Net.Web.Api.Sdk.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Register all SDK services: controllers, JSON, CORS, API versioning, Swagger, JWT, file, info services.
// Also scans assemblies for [InjectInterfaceService]-marked interfaces (registers IPrimeNumberService, etc.)
builder.Services.AddSdkApiServices();

var app = builder.Build();

// Serve static files from wwwroot/ (images, icons, logos)
app.UseStaticFiles();

// SDK middleware: JWT token handler, CORS, Swagger, embedded resource extraction
app.UseSdkApiMiddleware();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
