# Assessment Report: Net.Web.Api.Sdk

## Solution Overview

| Attribute | Value |
|-----------|-------|
| **Solution Name** | Net.Web.Api.Sdk |
| **Total Projects** | 2 |
| **Target Framework** | net10.0 |
| **Total Lines of Code** | 6334 |
| **Overall Complexity** | High |
| **Total NuGet Packages** | 52 (across all projects) |
| **Incompatible Packages** | 26 |
| **.NET Core Readiness** | Not Ready |
| **Linux Readiness** | Not Ready |

## Executive Summary

**Solution Migration Mode: SIMPLE**

The Net.Web.Api.Sdk solution is a two-project ASP.NET Web API 2 solution built on .NET Framework 4.8. It follows a layered architecture: a reusable SDK class library (`Net.Web.Api.Sdk`) that provides controllers, JWT authentication, Swagger documentation, file upload handling, and Castle Windsor dependency injection infrastructure, consumed by an example web application (`Net.Web.Api.Sdk.Web.Examples`) that demonstrates how to host and configure the SDK.

Both projects use old-style `.csproj` format with `packages.config` for NuGet package management. The entire solution is deeply coupled to `System.Web.Http` (ASP.NET Web API 2) and `System.Web` APIs. Castle Windsor is the DI container, with custom installers, resolvers, and composition roots. Token-based authentication uses custom `ConfigurationSection` classes and `System.IdentityModel.Tokens.Jwt`. Half of the 26 NuGet packages per project are incompatible with modern .NET and require replacement.

- **0 Low-complexity projects** — N/A
- **0 Medium-complexity projects** — N/A
- **2 High-complexity projects** (Net.Web.Api.Sdk, Net.Web.Api.Sdk.Web.Examples) — 13 incompatible packages each, heavy System.Web.Http/Castle Windsor usage, old-style project format, ConfigurationSection classes, and custom DI infrastructure requiring full replacement

The primary transformation challenge is the wholesale migration from ASP.NET Web API 2 to ASP.NET Core: replacing `System.Web.Http` (`ApiController`, `IHttpActionResult`, `HttpConfiguration`, action filters, message handlers) with ASP.NET Core equivalents (`ControllerBase` + `[ApiController]`, `IActionResult`, middleware pipeline), migrating Castle Windsor DI to the built-in ASP.NET Core DI container, converting custom `ConfigurationSection` classes to the `IOptions<T>` pattern, and replacing `Swashbuckle.Core` with `Swashbuckle.AspNetCore`.

### Key Statistics

| Metric | Count |
|--------|-------|
| Projects requiring format conversion | 2 (legacy to SDK-style) |
| Blocking issues | 0 |
| Controllers to migrate | 5 |
| Total estimated changes | 65 |

## Project Analysis Table

| Project | Current Framework | Target | LOC | Packages | Incompatible | Complexity |
|---------|-------------------|--------|-----|----------|--------------|------------|
| Net.Web.Api.Sdk | net48 | net10.0 | 5684 | 26 | 13 | High |
| Net.Web.Api.Sdk.Web.Examples | net48 | net10.0 | 650 | 26 | 13 | High |

## Cross-Project Package Summary

| Package | Used By | Version(s) | Compatible | Notes |
|---------|---------|------------|------------|-------|
| ByteSize | Both | 1.3.0 | Yes | UpgradePackage to 2.1.2 (netstandard2.0+) |
| Castle.Core | Both | 4.4.0 | Yes | UpgradePackage to 5.2.1 (netstandard2.0+) |
| Castle.Facilities.AspNet.SystemWeb | Both | 5.0.1 | No | ReplacePackage — remove; .NET Framework System.Web only (latest 6.0.0 still net462 only) |
| Castle.Windsor | Both | 5.0.1 | Yes | UpgradePackage to 6.0.0 (netstandard2.0); recommend replacing with ASP.NET Core built-in DI |
| Castle.Windsor.Lifestyles | Both | 0.4.0 | No | ReplacePackage — remove; no netstandard TFM; use ASP.NET Core DI scoped lifetimes |
| ExpressiveAnnotations.dll | Both | 2.7.4 | No | ReplacePackage — net45 only; use System.ComponentModel.DataAnnotations or FluentValidation |
| Havit.CastleWindsor.WebForms | Both | 1.8.9 | No | ReplacePackage — remove; net472 only, WebForms/Castle-specific |
| LiteDB | Both | 4.1.4 | Yes | UpgradePackage to 5.0.21 (netstandard2.0) |
| Microsoft.AspNet.Cors | Both | 5.2.7 | No | ReplacePackage — remove; use ASP.NET Core CORS middleware (built-in) |
| Microsoft.AspNet.Identity.Core | Both | 2.2.2 | No | ReplacePackage with Microsoft.Extensions.Identity.Core |
| Microsoft.AspNet.WebApi.Client | Both | 5.2.7 | Yes | UpgradePackage to 6.0.0 (netstandard2.0); consider removing if System.Net.Http.Formatting no longer needed |
| Microsoft.AspNet.WebApi.Core | Both | 5.2.7 | No | ReplacePackage — remove; use ASP.NET Core MVC [ApiController] |
| Microsoft.AspNet.WebApi.Cors | Both | 5.2.7 | No | ReplacePackage — remove; use ASP.NET Core CORS middleware |
| Microsoft.AspNet.WebApi.Versioning | Both | 4.0.0 | No | ReplacePackage with Asp.Versioning.Mvc |
| Microsoft.AspNet.WebApi.Versioning.ApiExplorer | Both | 4.0.0 | No | ReplacePackage with Asp.Versioning.Mvc.ApiExplorer |
| Microsoft.AspNet.WebApi.WebHost | Both | 5.2.7 | No | ReplacePackage — remove; not needed in ASP.NET Core |
| Microsoft.IdentityModel.JsonWebTokens | Both | 5.6.0 | Yes | UpgradePackage to 8.23.0 (netstandard2.0+) |
| Microsoft.IdentityModel.Logging | Both | 5.6.0 | Yes | UpgradePackage to 8.23.0 (netstandard2.0+) |
| Microsoft.IdentityModel.Tokens | Both | 5.6.0 | Yes | UpgradePackage to 8.23.0 (netstandard2.0+) |
| Microsoft.Web.Infrastructure | Both | 1.0.0.0 | Yes* | ReplacePackage — tooling-only; remove from SDK-style project (not needed in Core) |
| Microsoft.Web.Xdt | Both | 3.0.0 | Yes* | ReplacePackage — build-time tool; remove from SDK-style project |
| MultipartDataMediaFormatter.V2 | Both | 2.0.2 | Yes | UpgradePackage to 2.1.1 (netstandard2.0) |
| Newtonsoft.Json | Both | 12.0.2 | Yes | UpgradePackage to 13.0.3 (netstandard2.0/net6.0) |
| NuGet.Core | Both | 2.14.0 | No | ReplacePackage — net40-Client only; use NuGet.Protocol or remove if not needed at runtime |
| Swashbuckle.Core | Both | 5.6.0 | No | ReplacePackage with Swashbuckle.AspNetCore |
| System.IdentityModel.Tokens.Jwt | Both | 5.6.0 | Yes | UpgradePackage to 8.23.0 (netstandard2.0+) |

\* Tooling/build-only packages — not marked INCOMPATIBLE per content/tooling rule; action is to remove from the project.

## Cross-Project Dependencies

Net.Web.Api.Sdk.Web.Examples (High)
  - Net.Web.Api.Sdk (High)

Net.Web.Api.Sdk (High)
  _(no project dependencies — leaf)_

### Recommended Transformation Order (Dependency-First)

1. **Net.Web.Api.Sdk** — Leaf library with zero project dependencies; must be migrated first so the web project can reference the updated SDK
2. **Net.Web.Api.Sdk.Web.Examples** — Depends on Net.Web.Api.Sdk; migrate last after the SDK library is on net10.0

## Key Findings

1. **Full ASP.NET Web API 2 → ASP.NET Core migration required**: Both projects are deeply coupled to `System.Web.Http` (`ApiController`, `IHttpActionResult`, `HttpConfiguration`, `DelegatingHandler`, action filters). Every controller, filter, and HTTP infrastructure class must be rewritten to use ASP.NET Core equivalents.
2. **Castle Windsor DI container must be replaced**: The SDK has a full Castle Windsor infrastructure — `WindsorCompositionRoot`, `WindsorDependencyResolver`, `WindsorDependencyScope`, `InjectionContainer`, `ControllerInstaller`, `ServiceInstaller`, and custom injection attributes. All of this must be replaced with ASP.NET Core's built-in DI (`IServiceCollection`/`IServiceProvider`).
3. **Custom ConfigurationSection classes require IOptions migration**: `TokenConfigurationSection`, `TokenElement`, `TokenElementCollection`, `TokenDefinitionElement`, and `TokenSignatureElement` use `System.Configuration.ConfigurationSection` which is not supported in ASP.NET Core. These must be converted to POCOs bound via `IOptions<T>` from `appsettings.json`.
4. **Swashbuckle.Core → Swashbuckle.AspNetCore**: The SDK has 8 custom Swagger filter classes that implement Swashbuckle.Core interfaces. These must be rewritten to implement `Swashbuckle.AspNetCore` interfaces (`IOperationFilter`, `IDocumentFilter`).
5. **Both projects use old-style .csproj with packages.config**: Both require conversion to SDK-style `.csproj` format with `<PackageReference>` elements. `Properties/AssemblyInfo.cs` files should be removed and replaced with `<GenerateAssemblyInfo>`.
6. **13 of 26 NuGet packages per project are incompatible**: Half the dependency stack is .NET Framework-only and must be replaced with modern equivalents (ASP.NET Core built-in middleware, Asp.Versioning, Swashbuckle.AspNetCore, etc.).
7. **JWT token handling uses outdated IdentityModel packages**: `System.IdentityModel.Tokens.Jwt` and related packages at 5.6.0 must be upgraded to 8.x with API surface changes (e.g. `JsonWebTokenHandler` replacing `JwtSecurityTokenHandler`).

## External Dependencies

No external dependencies detected.

## Actionable Next Steps

1. **Phase 1 — Project format conversion** (Low risk): Convert both `.csproj` files from old-style to SDK-style format. Migrate `packages.config` to `<PackageReference>`. Remove `AssemblyInfo.cs` files and enable `<GenerateAssemblyInfo>`.
2. **Phase 2 — SDK library migration** (High risk): Migrate `Net.Web.Api.Sdk` to net10.0. Replace all 13 incompatible packages. Rewrite `System.Web.Http`-based controllers, filters, and handlers to ASP.NET Core equivalents. Replace Castle Windsor DI with ASP.NET Core DI. Convert `ConfigurationSection` classes to `IOptions<T>`. Rewrite Swashbuckle filters for `Swashbuckle.AspNetCore`.
3. **Phase 3 — Web application migration** (High risk): Migrate `Net.Web.Api.Sdk.Web.Examples` to net10.0. Replace `Global.asax` with `Program.cs` and the ASP.NET Core middleware pipeline. Convert `Web.config` settings to `appsettings.json`. Wire up the migrated SDK services in the Core DI container.
4. **Phase 4 — Validation and testing** (Medium risk): Verify all JWT authentication flows, API versioning, Swagger documentation, file upload, and CORS configuration work correctly on the new stack. Run integration tests against the migrated API endpoints.

---

## Per-Project Assessment Details

### Net.Web.Api.Sdk

#### Project Metrics

| Metric | Value |
|--------|-------|
| **Framework** | net48 |
| **Lines of Code** | 5684 |
| **NuGet Packages** | 26 |
| **Project References** | 0 |
| **Complexity** | High |
| **Estimated Changes** | 31 |

#### Package Compatibility (26 packages, 13 incompatible)

| Package | Version | Compatibility | Recommendation |
|---------|---------|---------------|----------------|
| ByteSize | 1.3.0 | COMPATIBLE | UpgradePackage |
| Castle.Core | 4.4.0 | COMPATIBLE | UpgradePackage |
| Castle.Facilities.AspNet.SystemWeb | 5.0.1 | INCOMPATIBLE | ReplacePackage |
| Castle.Windsor | 5.0.1 | COMPATIBLE | UpgradePackage |
| Castle.Windsor.Lifestyles | 0.4.0 | INCOMPATIBLE | ReplacePackage |
| ExpressiveAnnotations.dll | 2.7.4 | INCOMPATIBLE | ReplacePackage |
| Havit.CastleWindsor.WebForms | 1.8.9 | INCOMPATIBLE | ReplacePackage |
| LiteDB | 4.1.4 | COMPATIBLE | UpgradePackage |
| Microsoft.AspNet.Cors | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.Identity.Core | 2.2.2 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Client | 5.2.7 | COMPATIBLE | UpgradePackage |
| Microsoft.AspNet.WebApi.Core | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Cors | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Versioning | 4.0.0 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Versioning.ApiExplorer | 4.0.0 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.WebHost | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.IdentityModel.JsonWebTokens | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.IdentityModel.Logging | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.IdentityModel.Tokens | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.Web.Infrastructure | 1.0.0.0 | COMPATIBLE | ReplacePackage |
| Microsoft.Web.Xdt | 3.0.0 | COMPATIBLE | ReplacePackage |
| MultipartDataMediaFormatter.V2 | 2.0.2 | COMPATIBLE | UpgradePackage |
| Newtonsoft.Json | 12.0.2 | COMPATIBLE | UpgradePackage |
| NuGet.Core | 2.14.0 | INCOMPATIBLE | ReplacePackage |
| Swashbuckle.Core | 5.6.0 | INCOMPATIBLE | ReplacePackage |
| System.IdentityModel.Tokens.Jwt | 5.6.0 | COMPATIBLE | UpgradePackage |

#### Legacy Files Inventory (4 files across 2 kinds)

| Kind | Files |
|------|------:|
| `.config` | 3 |
| `.resx` | 1 |

#### Migration Analysis

##### Migration Strategy

1. **Convert project format**: Convert old-style `.csproj` to SDK-style format. Change `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>` to `<TargetFramework>net10.0</TargetFramework>`. Remove `<Import>` elements for `Microsoft.Common.props` and `Microsoft.CSharp.targets`. Migrate `packages.config` entries to `<PackageReference>` elements. Remove `Properties/AssemblyInfo.cs` and enable `<GenerateAssemblyInfo>true</GenerateAssemblyInfo>`.
2. **Replace incompatible packages**: Remove `Microsoft.AspNet.WebApi.Core`, `Microsoft.AspNet.WebApi.Cors`, `Microsoft.AspNet.WebApi.WebHost`, `Microsoft.AspNet.Cors`, `Microsoft.AspNet.WebApi.Client` — these are replaced by ASP.NET Core's built-in MVC and CORS middleware. Replace `Microsoft.AspNet.WebApi.Versioning` (4.0.0) and `Microsoft.AspNet.WebApi.Versioning.ApiExplorer` (4.0.0) with `Asp.Versioning.Mvc` and `Asp.Versioning.Mvc.ApiExplorer`. Replace `Swashbuckle.Core` (5.6.0) with `Swashbuckle.AspNetCore`. Replace `Microsoft.AspNet.Identity.Core` with `Microsoft.Extensions.Identity.Core`. Remove `Castle.Facilities.AspNet.SystemWeb`, `Castle.Windsor.Lifestyles`, `Havit.CastleWindsor.WebForms`, `NuGet.Core`, `Microsoft.Web.Infrastructure`, `Microsoft.Web.Xdt`. Replace `ExpressiveAnnotations.dll` with `System.ComponentModel.DataAnnotations` built-in attributes.
3. **Migrate System.Web.Http → ASP.NET Core MVC**: Rewrite `SdkController.cs` and `SdkInformationController.cs` from `ApiController` to `ControllerBase` with `[ApiController]`. Replace `IHttpActionResult` with `IActionResult`. Replace `HttpConfiguration`-based initialization (`HttpConfigurationExtensions.cs`) with ASP.NET Core `IServiceCollection`/`IApplicationBuilder` extension methods. Migrate `ParameterValidationActionFilterAttribute` from `System.Web.Http.Filters.ActionFilterAttribute` to `Microsoft.AspNetCore.Mvc.Filters.IAsyncActionFilter`. Migrate `ChallengeOnUnauthorizedResult` and `ResponseActionResult` to Core `IActionResult` implementations.
4. **Replace Castle Windsor DI with ASP.NET Core DI**: Remove `WindsorCompositionRoot.cs`, `InjectionContainer.cs`, `WindsorDependencyResolver.cs`, `WindsorDependencyScope.cs`, `ControllerInstaller.cs`, `ServiceInstaller.cs`. Create ASP.NET Core `IServiceCollection` extension methods that register `IJwtTokenService`, `IFileService`, `IInformationService`, and other services. Remove `InjectInterfaceServiceAttribute` and `InjectServiceCustomAttribute` — use standard constructor injection.
5. **Convert ConfigurationSection → IOptions<T>**: Replace `TokenConfigurationSection`, `TokenElement`, `TokenElementCollection`, `TokenDefinitionElement`, `TokenSignatureElement` (5 classes using `System.Configuration`) with POCO configuration classes bound via `IOptions<T>`. Replace `ConfigurationManager.GetSection()` calls with injected `IOptions<TokenConfiguration>`.
6. **Rewrite Swagger filters**: Rewrite all 8 custom Swagger filter classes (`SwaggerConsumesFilter`, `SwaggerProducesFilter`, `SwaggerSecurityTypeAttributeFilter`, `SwaggerUploadOperationFilter`, `SwaggerOrderingFilter`, `SwaggerDocumentOrderingFilter`, `SwaggerMethodOrderingFilter`, `SwaggerOperationOrderingFilter`) to implement `Swashbuckle.AspNetCore` interfaces.
7. **Migrate JWT handler**: Rewrite `JwtTokenHandler.cs` (a `DelegatingHandler`) to an ASP.NET Core authentication handler or middleware. Rewrite `BasicAuthorizeAttribute` and `TokenAuthorizeAttribute` to ASP.NET Core authorization filters or policy-based authorization. Upgrade `System.IdentityModel.Tokens.Jwt` from 5.6.0 to 8.x and adapt to API changes.
8. **Handle legacy files**: Convert `app.config` settings to configuration POCO classes. Convert `token-sdk.config` embedded resource to `appsettings.json` section or embedded JSON. Migrate `Properties/Resources.resx` to `IStringLocalizer` or keep as embedded resources with updated project references.

##### Risks & Architectural Concerns

| Risk | Severity | Notes |
|------|----------|-------|
| Castle Windsor DI replacement | High | Entire DI infrastructure (6+ classes) must be rewritten; custom injection attributes and composition root have no direct Core equivalent |
| System.Web.Http API surface | High | Controllers, filters, handlers, and HTTP infrastructure all use Web API 2 APIs with no drop-in replacement |
| ConfigurationSection migration | Medium | 5 custom configuration classes using System.Configuration; must be redesigned as POCOs with IOptions<T> |
| Swashbuckle filter rewrite | Medium | 8 custom Swagger filters must be rewritten for Swashbuckle.AspNetCore's different interface signatures |
| JWT handler migration | Medium | DelegatingHandler-based JWT validation must become ASP.NET Core authentication middleware; IdentityModel 5.x → 8.x has breaking API changes |
| NuGet.Core dependency | Low | Used for SDK information/versioning features; may need NuGet.Protocol or custom replacement |

##### Recommendations

1. Replace Castle Windsor with ASP.NET Core built-in DI — do not port Castle Windsor to Core; the built-in container covers all registration patterns used here (transient, scoped, singleton).
2. Use `Microsoft.AspNetCore.Authentication.JwtBearer` for JWT validation instead of the custom `JwtTokenHandler` DelegatingHandler — this integrates natively with the Core auth pipeline.
3. Replace all 8 Swashbuckle filters by implementing `Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter` and `IDocumentFilter`; the attribute-based approach (`SwaggerConsumesAttribute`, etc.) can be preserved as metadata read by the new filters.
4. Convert `HttpConfigurationExtensions.RegisterWebApi()` into a pair of ASP.NET Core extension methods: `AddWebApiSdk(IServiceCollection)` for DI registration and `UseWebApiSdk(IApplicationBuilder)` for middleware — this preserves the SDK's consumer-facing API shape.

##### Cross-Project Impact

Net.Web.Api.Sdk is the foundational dependency. It must be fully migrated before Net.Web.Api.Sdk.Web.Examples can be updated, as the web project directly depends on SDK controllers, services, DI infrastructure, and configuration classes. Changes to the SDK's public API surface (e.g. new extension method signatures for `IServiceCollection`/`IApplicationBuilder` replacing `HttpConfiguration`) will require corresponding updates in the web project's startup code.

---

### Net.Web.Api.Sdk.Web.Examples

#### Project Metrics

| Metric | Value |
|--------|-------|
| **Framework** | net48 |
| **Lines of Code** | 650 |
| **NuGet Packages** | 26 |
| **Project References** | 1 |
| **Complexity** | High |
| **Estimated Changes** | 34 |

#### Package Compatibility (26 packages, 13 incompatible)

| Package | Version | Compatibility | Recommendation |
|---------|---------|---------------|----------------|
| ByteSize | 1.3.0 | COMPATIBLE | UpgradePackage |
| Castle.Core | 4.4.0 | COMPATIBLE | UpgradePackage |
| Castle.Facilities.AspNet.SystemWeb | 5.0.1 | INCOMPATIBLE | ReplacePackage |
| Castle.Windsor | 5.0.1 | COMPATIBLE | UpgradePackage |
| Castle.Windsor.Lifestyles | 0.4.0 | INCOMPATIBLE | ReplacePackage |
| ExpressiveAnnotations.dll | 2.7.4 | INCOMPATIBLE | ReplacePackage |
| Havit.CastleWindsor.WebForms | 1.8.9 | INCOMPATIBLE | ReplacePackage |
| LiteDB | 4.1.4 | COMPATIBLE | UpgradePackage |
| Microsoft.AspNet.Cors | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.Identity.Core | 2.2.2 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Client | 5.2.7 | COMPATIBLE | UpgradePackage |
| Microsoft.AspNet.WebApi.Core | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Cors | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Versioning | 4.0.0 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Versioning.ApiExplorer | 4.0.0 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.WebHost | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.IdentityModel.JsonWebTokens | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.IdentityModel.Logging | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.IdentityModel.Tokens | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.Web.Infrastructure | 1.0.0.0 | COMPATIBLE | ReplacePackage |
| Microsoft.Web.Xdt | 3.0.0 | COMPATIBLE | ReplacePackage |
| MultipartDataMediaFormatter.V2 | 2.0.2 | COMPATIBLE | UpgradePackage |
| Newtonsoft.Json | 12.0.2 | COMPATIBLE | UpgradePackage |
| NuGet.Core | 2.14.0 | INCOMPATIBLE | ReplacePackage |
| Swashbuckle.Core | 5.6.0 | INCOMPATIBLE | ReplacePackage |
| System.IdentityModel.Tokens.Jwt | 5.6.0 | COMPATIBLE | UpgradePackage |

#### Project Dependencies (1)

- Net.Web.Api.Sdk

#### Legacy Files Inventory (7 files across 3 kinds)

| Kind | Files |
|------|------:|
| `.asax` | 1 |
| `.config` | 5 |
| `.resx` | 1 |

#### Migration Analysis

##### Migration Strategy

1. **Convert project format**: Convert the old-style web application `.csproj` (with `ProjectTypeGuids` `{349c5851-65df-11da-9384-00065b846f21}`) to SDK-style format using `<Project Sdk="Microsoft.NET.Sdk.Web">`. Change `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>` to `<TargetFramework>net10.0</TargetFramework>`. Migrate `packages.config` to `<PackageReference>` elements. Remove `Properties/AssemblyInfo.cs`, `ProjectTypeGuids`, and legacy `<Import>` elements.
2. **Replace Global.asax with Program.cs**: The current `Global.asax.cs` calls `GlobalConfiguration.Configuration.RegisterWebApi()` in `Application_Start` and `UnRegisterWebApi()` in `Application_End`. Create a new `Program.cs` with the ASP.NET Core minimal hosting model: `builder.Services.AddWebApiSdk()` (calling the migrated SDK extension method), `builder.Services.AddControllers()`, `app.UseWebApiSdk()`, `app.MapControllers()`. Remove `Global.asax` and `Global.asax.cs`.
3. **Convert Web.config to appsettings.json**: Extract `<appSettings>`, `<connectionStrings>`, and custom configuration sections from `Web.config` into `appsettings.json`. Move the `token-example.config` settings into `appsettings.json` under a `TokenConfiguration` section. Remove `Web.config`, `Web.Debug.config`, and `Web.Release.config`.
4. **Replace incompatible packages**: Same 13 incompatible packages as the SDK project — remove ASP.NET Web API, Castle Windsor SystemWeb/Lifestyles, Swashbuckle.Core, and other framework-only packages. Many will be transitively resolved through the SDK project reference once it is migrated.
5. **Migrate controllers**: Rewrite `ExampleController.cs`, `ExampleTokenController.cs`, and `ExampleUploadController.cs` from `ApiController` to `ControllerBase` with `[ApiController]`. Replace `IHttpActionResult` return types with `IActionResult`. Update model binding for file uploads from `MultipartDataMediaFormatter` patterns to `IFormFile`.
6. **Move static content**: Move `Images/Icons/` and `Images/Logos/` to `wwwroot/images/`. Move certificate files from `App_Data/Certficates/` to a non-web-accessible location and load via `IConfiguration` or file system paths.
7. **Handle legacy files**: Delete `packages.config` after migration. Migrate `Properties/Resources.resx` to updated project references. Remove `Web.Debug.config` and `Web.Release.config` XML transforms (use environment-specific `appsettings.{Environment}.json` instead).

##### Risks & Architectural Concerns

| Risk | Severity | Notes |
|------|----------|-------|
| Startup/hosting model change | High | Global.asax → Program.cs is a complete rewrite of the application entry point and middleware pipeline |
| Dependency on SDK migration | High | This project cannot compile until Net.Web.Api.Sdk is fully migrated; any SDK API changes cascade here |
| Web.config custom sections | Medium | Token configuration in XML format must be converted to appsettings.json; any consumers of ConfigurationManager calls must be updated |
| App_Data certificate handling | Low | Certificate files in App_Data must be relocated; file paths in configuration must be updated |
| File upload handling | Low | MultipartDataMediaFormatter usage must be replaced with ASP.NET Core IFormFile model binding |

##### Recommendations

1. Migrate this project immediately after Net.Web.Api.Sdk is complete — it serves as the integration test for the SDK migration.
2. Use `Microsoft.AspNetCore.Mvc.NewtonsoftJson` if any Newtonsoft.Json-specific serialization behavior (e.g. `JsonProperty` attributes, custom converters) must be preserved; otherwise, switch to `System.Text.Json`.
3. Store certificates outside `App_Data` (which is an IIS convention) — use a dedicated `Certificates/` folder or configure paths via `appsettings.json` for environment-specific certificate locations.
4. Simplify the package list post-migration — many of the 26 packages are only needed because of the Web API 2 hosting model and will be replaced by the ASP.NET Core metapackage (`Microsoft.AspNetCore.App`).

##### Cross-Project Impact

This is the top-level consuming project and depends entirely on Net.Web.Api.Sdk. It cannot be migrated until the SDK project is complete. Once migrated, this project validates that the SDK's new ASP.NET Core extension methods (`AddWebApiSdk`/`UseWebApiSdk`) work correctly in a real host application. No other projects depend on this one — it is the dependency graph root.
