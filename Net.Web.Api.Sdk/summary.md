# Migration Summary — Net.Web.Api.Sdk: .NET Framework 4.8 → .NET 10

## Result

`dotnet build Net.Web.Api.Sdk.sln` exits with **0 errors, 0 warnings**.

---

## Changes Made

### Project Files

| File | Change |
|------|--------|
| `Net.Web.Api.Sdk/Net.Web.Api.Sdk.csproj` | Dropped `netstandard2.0` (see rationale below); set `<TargetFramework>net10.0</TargetFramework>`; removed all incompatible .NET Framework packages; added ASP.NET Core packages; `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to avoid duplicate attribute errors; `<NoWarn>CS1591</NoWarn>` to suppress condensed model XML-doc warnings. |
| `Net.Web.Api.Sdk.Web.Examples/Net.Web.Api.Sdk.Web.Examples.csproj` | Converted from legacy `.csproj` format to `Microsoft.NET.Sdk.Web`; target `net10.0`; removed `packages.config` and all legacy `<Reference>` entries; excluded `Global.asax.cs` from compilation. |

### Package Changes (Net.Web.Api.Sdk)

**Removed:**
- `Castle.Facilities.AspNet.SystemWeb` (System.Web-specific)
- `Castle.Windsor.Lifestyles` (no longer needed)
- `Havit.CastleWindsor.WebForms` (WebForms-specific)
- `ExpressiveAnnotations.dll` (unused in source)
- `Microsoft.AspNet.Identity.Core`
- `Microsoft.AspNet.WebApi.Client/Core/Cors/Versioning/WebHost`
- `Microsoft.AspNet.WebApi.Versioning.ApiExplorer`
- `Microsoft.AspNetCore.SystemWebAdapters` / `CoreServices` (workaround no longer needed)
- `Microsoft.Web.Xdt`
- `MultipartDataMediaFormatter.V2` (replaced by `IFormFile`)
- `NuGet.Core` (Windows-only; package listing removed)
- `Swashbuckle.Core` → replaced by `Swashbuckle.AspNetCore`
- `System.Data.DataSetExtensions`
- `Microsoft.CSharp` (automatically available; NU1510)

**Added:**
- `<FrameworkReference Include="Microsoft.AspNetCore.App" />`
- `Asp.Versioning.Mvc` 8.1.0
- `Asp.Versioning.Mvc.ApiExplorer` 8.1.0
- `Swashbuckle.AspNetCore` 6.9.0
- `Swashbuckle.AspNetCore.Annotations` 6.9.0
- `Swashbuckle.AspNetCore.Newtonsoft` 6.9.0
- `Microsoft.AspNetCore.Mvc.NewtonsoftJson` 10.0.0

**Updated:**
- `ByteSize` 1.3.0 → 2.1.1
- `LiteDB` 4.1.4 → 5.0.21 (`Delete` → `DeleteMany`, `Find().FirstOrDefault()` → `FindOne()`)
- `Microsoft.IdentityModel.*` 5.6.0 → 8.2.1
- `System.IdentityModel.Tokens.Jwt` 5.6.0 → 8.2.1
- `System.Configuration.ConfigurationManager` 10.0.13 → 9.0.3

### Code Changes

#### New Files
- `Net.Web.Api.Sdk/Initialization/ConfigureSwaggerOptions.cs` — `IConfigureOptions<SwaggerGenOptions>` for per-version Swagger docs
- `Net.Web.Api.Sdk.Web.Examples/Program.cs` — Entry point replacing `Global.asax.cs`
- `Net.Web.Api.Sdk.Web.Examples/appsettings.json` — Replaces `Web.config`

#### Deleted / Emptied (Legacy Web API 2 infrastructure)
- `Injection/Resolvers/WindsorDependencyResolver.cs` — `IDependencyResolver` (Web API 2 specific)
- `Injection/Scopes/WindsorDependencyScope.cs` — `IDependencyScope` (Web API 2 specific)
- `Injection/Compositions/WindsorCompositionRoot.cs` — `IHttpControllerActivator` (Web API 2 specific)
- `Injection/Installers/ControllerInstaller.cs` — Windsor controller installer (not needed in ASP.NET Core)
- `Extensions/HttpAuthenticationChallengeContextExtensions.cs` — `HttpAuthenticationChallengeContext` (Web API 2 specific)

#### Major Rewrites
| File | Summary |
|------|---------|
| `Initialization/HttpConfigurationExtensions.cs` | Replaced `RegisterWebApi(HttpConfiguration)` with `AddWebApiSdk(IServiceCollection)` and `UseWebApiSdk(IApplicationBuilder)` |
| `Injection/Containers/InjectionContainer.cs` | Changed backing store from `IWindsorContainer` to `IServiceProvider` |
| `Injection/Installers/ServiceInstaller.cs` | Migrated from Windsor `Install(IWindsorContainer)` to `Install(IServiceCollection)`; fixed `RelativeSearchPath` → `BaseDirectory` for .NET Core |
| `Security/Attributes/BasicAuthorizeAttribute.cs` | Replaced `IAuthenticationFilter` (Web API 2) with `IAsyncAuthorizationFilter` (ASP.NET Core) |
| `Security/Attributes/TokenAuthorizeAttribute.cs` | Replaced Web API 2 `AuthorizeAttribute` base with `Attribute` + `IAsyncAuthorizationFilter` |
| `Security/Handlers/JwtTokenHandler.cs` | Replaced `DelegatingHandler` with ASP.NET Core middleware (`JwtTokenMiddleware`) |
| `Common/Http/ResponseActionResult.cs` | Replaced `IHttpActionResult` with `IActionResult` |
| `Common/Validations/ParameterValidationActionFilterAttribute.cs` | Replaced Web API 2 `ActionFilterAttribute` with ASP.NET Core equivalent |
| `Controllers/Common/SdkController.cs` | `ApiController` → `ControllerBase` |
| `Controllers/v1/SdkInformationController.cs` | Updated attributes: `RoutePrefix`→`Route`, Swashbuckle.Core→Swashbuckle.AspNetCore.Annotations |
| `Documentation/Filters/Common/SwaggerOrderingFilter.cs` | Rewritten for Swashbuckle.AspNetCore (`OpenApiDocument`, `DocumentFilterContext`) |
| `Documentation/Filters/Swagger*.cs` | All filters rewritten for `OpenApiOperation`, `OperationFilterContext` |
| `Extensions/JwtTokenExtensions.cs` | Removed `HttpActionContext` overloads; kept string/claim-based extensions |
| `Extensions/ControllerExtensions.cs` | `ApiController` → `ControllerBase`; `ActionContext.RequestContext.Principal` → `User` |
| `Extensions/ListExtensions.cs` | Removed custom `DistinctBy` (now a built-in LINQ method in .NET 6+) |
| `Implementations/Token/JwtTokenService.cs` | Replaced `HttpContext.Current.Server.MapPath()` with `IWebHostEnvironment`; replaced `HttpActionContext` with `HttpContext`; updated LiteDB API |
| `Implementations/Information/InformationService.cs` | Removed `NuGet.Core` dependency; removed `HttpContext.Current` |
| `Implementations/File/FileService.cs` | Replaced `HttpContext.Current` with `IHttpContextAccessor` |
| `Interfaces/Token/IJwtTokenService.cs` | Changed `GetTokenPayload`/`GetIdentityPayload` from `HttpActionContext` to `HttpContext` |
| `Attributes/Validations/UploadFileAttribute.cs` | `HttpFile` → `IFormFile` |
| `Attributes/Validations/TokenNameExistsAttribute.cs` | Replaced `InjectionContainer` with `validationContext.GetService<T>()` |
| `Models/Token/JwtTokenModel.cs` | Removed `HttpContext.Current`; replaced obsolete `X509Certificate2.Import()` with `X509CertificateLoader.LoadCertificate()` / `LoadPkcs12()` (SYSLIB0057 fix); added `rootPath` parameter to constructor |
| `Net.Web.Api.Sdk.Web.Examples/Models/UploadRequest.cs` | `HttpFile` → `IFormFile` |
| `Net.Web.Api.Sdk.Web.Examples/Controllers/v1/ExampleTokenController.cs` | Updated to ASP.NET Core attributes and patterns |
| `Net.Web.Api.Sdk.Web.Examples/Controllers/v1/ExampleUploadController.cs` | Updated to ASP.NET Core `[FromForm]`; `Stream.ReadExactly()` for CA2022 |
| `Net.Web.Api.Sdk.Web.Examples/Controllers/Common/ExampleController.cs` | Removed legacy `System.Web.Http` imports |

### Architectural Decisions

1. **Castle Windsor retained** for service scanning/registration via `ServiceInstaller`, but the Castle Windsor Web API 2 integration classes (DependencyResolver, DependencyScope, CompositionRoot) were removed. `InjectionContainer` now wraps an `IServiceProvider` instead of `IWindsorContainer`.

2. **netstandard2.0 target removed** — The SDK is fundamentally an ASP.NET Core infrastructure library (controllers, middleware, Swagger). ASP.NET Core packages (`Microsoft.AspNetCore.App`, `Asp.Versioning.Mvc`, `Swashbuckle.AspNetCore`) do not support `netstandard2.0` at runtime. Guarding the entire library with `#if` would leave an empty/non-functional netstandard2.0 build.

3. **NuGet.Core package info endpoint removed** — `NuGet.Core` is Windows-only and does not have a .NET 10 compatible version. The `GetSdkInformations()` endpoint still returns library metadata and available tokens; the packages list is no longer returned.

4. **Swagger UI customization** — The SDK still extracts its embedded `index.html` and `swagger-ui-override.{css,js}` resources but wires them through Swashbuckle.AspNetCore's `IndexStream` and embedded resource configuration rather than the Swashbuckle.Core custom asset approach.

---

## Next Steps

- The `token-example.config` loaded at runtime requires a `.config` file on disk. The `JwtTokenService` loads all `token*.config` files from `ContentRootPath`. Ensure the deployed environment copies these files to the application root.
- `packages.config` from the Web.Examples project is no longer used; the example project now uses `PackageReference` transitively via the SDK library reference.
- Consider migrating the remaining Castle Windsor DI scanning to Microsoft.Extensions.DependencyInjection conventions (e.g., `Scrutor`) for a fully Windsor-free stack.
- The Web.Examples `Global.asax.cs` is excluded from compilation but physically remains on disk. It can be deleted safely.
- The `Properties/AssemblyInfo.cs` files in both projects remain and are picked up (with `GenerateAssemblyInfo=false`). Verify version numbers are correct for any NuGet publishing workflows.
