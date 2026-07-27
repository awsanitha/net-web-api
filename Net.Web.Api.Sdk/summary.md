# Migration Summary: .NET Framework 4.8 → net10.0

## Status: ✅ COMPLETE — `dotnet build` exits with 0 errors, 0 warnings

---

## Projects Migrated

### 1. Net.Web.Api.Sdk (class library)
- **Target framework**: `net10.0`
- **Project type**: SDK-style (was already converted prior to this run)

### 2. Net.Web.Api.Sdk.Web.Examples (web application)
- **Target framework**: `net10.0`
- **Project type**: Converted from old-style `.csproj` (ToolsVersion="15.0") to `Microsoft.NET.Sdk.Web`

---

## Key Changes Made

### Project Files
- `Net.Web.Api.Sdk.csproj`: Added `GenerateAssemblyInfo=false` to fix duplicate assembly attribute errors; replaced all legacy packages; added `FrameworkReference` for `Microsoft.AspNetCore.App`; removed `Microsoft.AspNetCore.Mvc.Core` explicit reference (redundant with framework reference)
- `Net.Web.Api.Sdk.Web.Examples.csproj`: Full rewrite from old-style to SDK-style `Microsoft.NET.Sdk.Web`

### Package Replacements (Net.Web.Api.Sdk)
| Old Package | New Package |
|---|---|
| `Microsoft.AspNet.WebApi.Core 5.2.7` | `Microsoft.AspNetCore.App` (framework reference) |
| `Microsoft.AspNet.WebApi.Cors 5.2.7` | `Microsoft.AspNetCore.App` (framework reference) |
| `Microsoft.AspNet.WebApi.Versioning 4.0.0` | `Asp.Versioning.Mvc 8.1.0` |
| `Microsoft.AspNet.WebApi.Versioning.ApiExplorer 4.0.0` | `Asp.Versioning.Mvc.ApiExplorer 8.1.0` |
| `Swashbuckle.Core 5.6.0` | `Swashbuckle.AspNetCore 6.9.0` + `Swashbuckle.AspNetCore.Annotations 6.9.0` |
| `NuGet.Core 2.14.0` | Removed — replaced with `System.Xml.Linq` XML parsing |
| `Castle.Windsor 5.0.1` | Removed — replaced with `Microsoft.Extensions.DependencyInjection` (built-in) |
| `Castle.Facilities.AspNet.SystemWeb` | Removed |
| `Castle.Windsor.Lifestyles` | Removed |
| `Havit.CastleWindsor.WebForms` | Removed |
| `MultipartDataMediaFormatter.V2` | Removed — replaced with `IFormFile` (ASP.NET Core built-in) |
| `Microsoft.AspNet.WebApi.WebHost` | Removed (IIS hosting replaced by Kestrel) |
| `Microsoft.Web.Xdt` | Removed |
| `Microsoft.Web.Infrastructure` | Removed |
| `Microsoft.AspNet.Identity.Core` | Removed |
| `Microsoft.IdentityModel.JsonWebTokens 5.6.0` | `8.9.0` |
| `System.IdentityModel.Tokens.Jwt 5.6.0` | `8.9.0` |
| `Microsoft.IdentityModel.Tokens 5.6.0` | `8.9.0` |
| `LiteDB 4.1.4` | `5.0.21` |
| `ByteSize 1.3.0` | `2.1.2` |
| `ExpressiveAnnotations.dll` | Removed (not used in migrated code) |

### API Framework Migration
| Old | New |
|---|---|
| `System.Web.Http.ApiController` | `Microsoft.AspNetCore.Mvc.ControllerBase` |
| `IHttpActionResult` | `IActionResult` |
| `IAuthenticationFilter` | `IAsyncActionFilter` |
| `AuthorizeAttribute` (WebApi) | `IAuthorizationFilter` |
| `HttpRequestMessage` | `HttpRequest` (ASP.NET Core) |
| `HttpResponseMessage` | `IActionResult` |
| `System.Web.Http.Filters.ActionFilterAttribute` | `Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute` |
| `System.Web.Http.Dependencies.IDependencyResolver` | `Microsoft.Extensions.DependencyInjection` |
| `DelegatingHandler` (JWT) | `IMiddleware` |
| `[RoutePrefix]` | `[Route]` |
| `[EnableCors("*","*","*")]` | `[EnableCors]` + default policy |
| `HttpConfiguration.RegisterWebApi()` | `IServiceCollection.AddWebApiSdk()` + `IApplicationBuilder.UseWebApiSdk()` |

### DI Container Migration
- Removed Castle.Windsor entirely
- `InjectionContainer` now wraps `IServiceProvider` (Microsoft.Extensions.DependencyInjection)
- Windsor-specific classes (WindsorCompositionRoot, WindsorDependencyResolver, WindsorDependencyScope, ControllerInstaller, ServiceInstaller) replaced with stub files

### System.Web Removal
- `HttpContext.Current.Server.MapPath()` → `AppContext.BaseDirectory`
- `HttpContext.Current.Request.Url` → `IHttpContextAccessor` + `HttpRequest`
- All `using System.Web;` / `using System.Web.Http;` removed

### Swagger Migration
- Swashbuckle.Core 5.x `SwaggerDocument`, `SchemaRegistry`, `PathItem`, `Operation` → OpenApi equivalents (`OpenApiDocument`, `SchemaRepository`, `OpenApiPathItem`, `OpenApiOperation`)
- `IDocumentFilter` / `IOperationFilter` interfaces updated to Swashbuckle.AspNetCore signatures

### X509 Certificates
- `X509Certificate2.Import()` (obsolete in .NET 5, removed in .NET 9) → `X509CertificateLoader.LoadCertificate()` / `X509CertificateLoader.LoadPkcs12()`

### Hosting & Startup
- `Global.asax` / `HttpApplication` → `Program.cs` using `WebApplication.CreateBuilder()`
- `Web.config` pattern no longer used (ASP.NET Core uses `appsettings.json` / environment variables)

### File Upload
- `MultipartDataMediaFormatter.Infrastructure.HttpFile` → `Microsoft.AspNetCore.Http.IFormFile`

---

## Next Steps / Notes

- The `InjectionContainer` singleton (originally used by `TokenAuthorizeAttribute` etc.) now delegates to `IServiceProvider`. In ASP.NET Core, `TokenAuthorizeAttribute` uses `context.HttpContext.RequestServices` for DI — this is the correct pattern and no longer needs `InjectionContainer`.
- Consider adding `appsettings.json` for configuration (currently token config files are loaded from `AppContext.BaseDirectory` by convention).
- The Web.Examples project's `Properties/Resources.Designer.cs` references legacy resource patterns — it builds cleanly but may not be necessary in net10.0. Review if the resources are actually needed.
- Swagger UI is configured with a single v1 endpoint. If multi-versioning is needed, update `WebApiSdkExtensions.SetupSwaggerGen` to register per-version docs.
- The `SwaggerConsumesFilter` and `SwaggerProducesFilter` set content types on request body/responses; verify they produce the desired Swagger output in your environment.
