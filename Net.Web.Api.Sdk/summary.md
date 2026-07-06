# Migration Summary: .NET Framework 4.8 → .NET 10

## Overview
Full migration of `Net.Web.Api.Sdk.sln` from .NET Framework 4.8 to `net10.0` targeting ASP.NET Core.

## Build Result
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Changes Made

### 1. Project Files

#### `Net.Web.Api.Sdk/Net.Web.Api.Sdk.csproj`
- Already converted to SDK-style; updated `TargetFramework` remains `net10.0`
- **Removed incompatible packages:**
  - `Castle.Facilities.AspNet.SystemWeb` → Not compatible with .NET Core
  - `Castle.Windsor.Lifestyles` → Not compatible with .NET Core
  - `Havit.CastleWindsor.WebForms` → WebForms-only package
  - `Microsoft.AspNet.Identity.Core` → Legacy ASP.NET Identity
  - `Microsoft.AspNet.WebApi.Client/Core/Cors/WebHost` (5.2.7) → Legacy Web API
  - `Microsoft.AspNet.WebApi.Versioning/ApiExplorer` (4.0.0) → Legacy versioning
  - `MultipartDataMediaFormatter.V2` → Legacy multipart formatter
  - `NuGet.Core` → Used for package info listing (removed feature)
  - `Swashbuckle.Core` (5.6.0) → Old Swashbuckle for .NET Framework
  - `Microsoft.Web.Xdt` → Not needed
  - `Microsoft.AspNetCore.SystemWebAdapters` / `CoreServices` → Full migration, not needed
  - `System.Data.DataSetExtensions` → Not needed
- **Added modern packages:**
  - `Asp.Versioning.Mvc` 8.1.0 + `Asp.Versioning.Mvc.ApiExplorer` 8.1.0
  - `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.0
  - `Swashbuckle.AspNetCore` 8.1.0 + `Swashbuckle.AspNetCore.Annotations` 8.1.0
  - `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.0
  - `Microsoft.Extensions.Hosting.Abstractions` 10.0.0
  - Updated `Microsoft.IdentityModel.*` to 8.9.0
  - Updated `LiteDB` to 5.0.21
  - Updated `Castle.Core` to 5.1.1, `Castle.Windsor` to 6.0.0
- Added `GenerateAssemblyInfo=false`, `NoWarn=1591`

#### `Net.Web.Api.Sdk.Web.Examples/Net.Web.Api.Sdk.Web.Examples.csproj`
- Converted from old-style `.csproj` (ToolsVersion 15.0) to SDK-style `Microsoft.NET.Sdk.Web`
- `TargetFramework` set to `net10.0`
- Removed all `<Reference>` and `<HintPath>` entries
- Removed `<Import>` of legacy WebApplication.targets
- Removed `Global.asax` and `Properties/AssemblyInfo.cs` from compilation

### 2. Dependency Injection System
**Windsor DI → ASP.NET Core built-in DI**

- **Removed:** `WindsorDependencyResolver.cs`, `WindsorDependencyScope.cs`, `WindsorCompositionRoot.cs`, `ControllerInstaller.cs`
- **Updated:** `InjectionContainer.cs` — now wraps `IServiceProvider` instead of `IWindsorContainer`
- **Updated:** `ServiceInstaller.cs` — scans assemblies and registers into `IServiceCollection` (singleton)
- **New:** `WebApiSdkExtensions.AddWebApiSdk()` extension method for `IServiceCollection`
- **New:** `WebApiSdkExtensions.UseWebApiSdk()` extension method for `IApplicationBuilder`

### 3. Application Startup
- **Removed:** `Global.asax` / `Global.asax.cs` — replaced by `Program.cs`
- **New:** `Net.Web.Api.Sdk.Web.Examples/Program.cs` — ASP.NET Core minimal hosting

### 4. Web API Controllers
**`System.Web.Http.ApiController` → `Microsoft.AspNetCore.Mvc.ControllerBase`**

- `SdkController.cs` — now extends `ControllerBase` with `[ApiController]`
- `SdkInformationController.cs` — uses `[Route]`, `[ApiVersion]`, returns `IActionResult`
- `ExampleTokenController.cs` — uses ASP.NET Core routing + `IActionResult`
- `ExampleUploadController.cs` — uses `[FromForm]`, `IFormFile` instead of `HttpFile`

### 5. Authentication & Security
- **`BasicAuthorizeAttribute`** — migrated from `IAuthenticationFilter` (WebAPI) to `IAsyncAuthorizationFilter` (ASP.NET Core)
- **`TokenAuthorizeAttribute`** — migrated from `AuthorizeAttribute` (WebAPI) to `Attribute + IAuthorizationFilter` (ASP.NET Core); no longer extends `AuthorizeAttribute` to avoid being overridden by `[AllowAnonymous]`
- **`JwtTokenHandler`** — migrated from `DelegatingHandler` to ASP.NET Core middleware

### 6. Action Filters
- **`ParameterValidationActionFilterAttribute`** — migrated from `System.Web.Http.Filters.ActionFilterAttribute` to `Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute`

### 7. Action Results
- **`ChallengeOnUnauthorizedResult`** — migrated from `IHttpActionResult` to `IActionResult`
- **`ResponseActionResult`** — migrated from `IHttpActionResult` to `IActionResult`

### 8. Services
- **`JwtTokenService`** — removed `HttpContext.Current`, now uses `IHostEnvironment.ContentRootPath`; removed `System.Web.Http.Controllers.HttpActionContext`, now uses `Microsoft.AspNetCore.Mvc.ActionContext`
- **`FileService`** — removed `HttpContext.Current`, now uses `IHostEnvironment` + `IHttpContextAccessor`
- **`InformationService`** — removed `HttpContext.Current` and `NuGet.Core` (package listing feature removed; packages.config listing is not available in .NET Core)

### 9. Models
- **`JwtTokenModel`** — removed `System.Web.HttpContext.Current`, uses `rootPath` parameter instead; updated `X509Certificate2` constructors to use `X509CertificateLoader` (fixes SYSLIB0057)

### 10. Extensions
- **`JwtTokenExtensions`** — removed `System.Web.Http.Controllers.HttpActionContext`, `HttpRequestMessage`; new extension methods on `HttpContext` and `HttpRequest`
- **`ControllerExtensions`** — changed from `ApiController` to `ControllerBase`
- **`HttpAuthenticationChallengeContextExtensions`** — replaced `HttpAuthenticationChallengeContext` with `HttpResponse` extension methods

### 11. Swagger / Documentation
**`Swashbuckle.Core` → `Swashbuckle.AspNetCore`**

- All filters updated from old API (`Operation`, `SchemaRegistry`, `ApiDescription`, `IApiExplorer`, `SwaggerDocument`) to new Swashbuckle.AspNetCore API (`OpenApiOperation`, `OperationFilterContext`, `OpenApiDocument`, `DocumentFilterContext`)
- `SwaggerConsumesFilter`, `SwaggerProducesFilter` — updated
- `SwaggerUploadOperationFilter` — updated to use `OpenApiSchema` with `IFormFile`
- `SwaggerSecurityTypeAttributeFilter` — updated to use `OperationFilterContext`
- `SwaggerOrderingFilter`, `SwaggerOperationOrderingFilter`, `SwaggerMethodOrderingFilter`, `SwaggerDocumentOrderingFilter` — updated to use `OpenApiDocument` + `DocumentFilterContext`

### 12. API Versioning
**`Microsoft.Web.Http.ApiVersion` → `Asp.Versioning.ApiVersion`**

- Updated `[ApiVersion]` attribute import
- `RouteConstants.ROUTE_PREFIX_VERSION` updated to `"api/v{version:apiVersion}"`
- Versioning registered via `services.AddApiVersioning()` / `.AddApiExplorer()`

### 13. Configuration
- **`Properties/Settings.Designer.cs`** — excluded from compilation (uses `ApplicationSettingsBase` which is .NET Framework only)
- **New:** `Properties/SdkSettings.cs` — static class providing `MaxAllowedUploadSize` and `AllowedMimeTypes`
- `Properties/AssemblyInfo.cs` — reduced to only `[ComVisible]` and `[Guid]` attributes; `GenerateAssemblyInfo=false` prevents conflicts

### 14. Upload Feature
- `MultipartDataMediaFormatter.HttpFile` → `Microsoft.AspNetCore.Http.IFormFile`
- `UploadRequest.FileInformation` type changed from `HttpFile` to `IFormFile`
- `UploadFileAttribute` updated to validate `IFormFile`

---

## Next Steps

1. **Review `InformationService`** — Package listing feature removed (NuGet.Core not available in .NET Core). If needed, implement using project metadata or a custom packages.json approach.
2. **Swagger configuration** — The embedded `SwaggerConfigurationSdk.json` and `swagger.html` are extracted to the content root on startup. Verify Swagger UI loads correctly at `/swagger`.
3. **Token configuration files** — `token*.config` files are searched in `ContentRootPath`. Ensure they are present in the deployment directory. In development, `token-example.config` is copied to output directory.
4. **X509 Certificate loading** — Updated to use `X509CertificateLoader` (net10.0 recommended API). Test certificate loading with the PFX and CER files in `App_Data/Certficates/`.
5. **CORS policy** — Currently configured as allow-all in `Program.cs`. Tighten for production environments.
6. **Windsor removed entirely** — `Castle.Windsor` is still a dependency in csproj (for `Castle.Core` transitive deps) but is no longer used for DI. The Windsor `Castle.Windsor` NuGet package can be removed if no other code uses it.
7. **ServiceInstaller scanning** — The service scanner uses `AppDomain.CurrentDomain.GetAssemblies()`. Ensure all application assemblies are loaded before calling `AddWebApiSdk()`.
8. **InjectionContainer static service locator** — Used by `TokenNameExistsAttribute` and `TokenAuthorizeAttribute`. This is a known limitation of validation attributes not having constructor injection; the pattern is preserved intentionally.
