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
| **Incompatible Packages** | 28 |
| **.NET Core Readiness** | Not Ready |
| **Linux Readiness** | Not Ready |

## Executive Summary

**Solution Migration Mode: SIMPLE**

This solution is a two-project ASP.NET Web API 2 SDK consisting of a reusable class library (`Net.Web.Api.Sdk`) and an example web application (`Net.Web.Api.Sdk.Web.Examples`). Both projects target .NET Framework 4.8 with old-style `.csproj` format and `packages.config` for NuGet dependency management. The architecture is a straightforward two-layer dependency: the web application references the SDK library.

The SDK library is the heart of the solution, providing Web API controllers, JWT token authentication, Castle Windsor dependency injection, Swagger documentation via Swashbuckle, file upload handling, and custom validation attributes — all built on the `System.Web.Http` (Web API 2) stack. The example web application is a thin host that registers the SDK's Web API configuration via `Global.asax`.

- **1 High-complexity project** (Net.Web.Api.Sdk) — Large class library (5,684 LOC) with 14 incompatible packages, deep dependency on System.Web.Http, Castle Windsor DI container with custom composition root, custom JWT token handling, Swashbuckle.Core Swagger integration, and custom ConfigurationSection classes.
- **1 Medium-complexity project** (Net.Web.Api.Sdk.Web.Examples) — Small web host (650 LOC) with 14 incompatible packages, Global.asax startup, and full dependency on the SDK library.

The primary transformation challenge is the wholesale migration from ASP.NET Web API 2 (`System.Web.Http`, `ApiController`, `IHttpActionResult`) to ASP.NET Core (`ControllerBase`, `[ApiController]`, `IActionResult`) across both projects. This entails replacing the Castle Windsor DI container with ASP.NET Core's built-in DI, migrating custom `ConfigurationSection` classes to `IOptions<T>`, replacing Swashbuckle.Core with Swashbuckle.AspNetCore, converting `Global.asax` startup to `Program.cs` with middleware pipeline, and upgrading or replacing 14 incompatible packages per project.

### Key Statistics

| Metric | Count |
|--------|-------|
| Projects requiring format conversion | 2 (legacy to SDK-style) |
| Blocking issues | 0 |
| Controllers to migrate | 4 |
| Total estimated changes | 65 |

## Project Analysis Table

| Project | Current Framework | Target | LOC | Packages | Incompatible | Complexity |
|---------|-------------------|--------|-----|----------|--------------|------------|
| Net.Web.Api.Sdk | net48 | net10.0 | 5684 | 26 | 14 | High |
| Net.Web.Api.Sdk.Web.Examples | net48 | net10.0 | 650 | 26 | 14 | Medium |

## Cross-Project Package Summary

| Package | Used By | Version(s) | Compatible | Notes |
|---------|---------|------------|------------|-------|
| ByteSize | Both | 1.3.0 | Yes | UpgradePackage to 2.1.2 (targets netstandard2.0/2.1/net5.0) |
| Castle.Core | Both | 4.4.0 | Yes | UpgradePackage to 5.2.1 (targets netstandard2.0/2.1/net6.0) |
| Castle.Facilities.AspNet.SystemWeb | Both | 5.0.1 | No | ReplacePackage — only targets .NETFramework4.6.2; remove (System.Web-specific Windsor facility) |
| Castle.Windsor | Both | 5.0.1 | Yes | ReplacePackage — 6.0.0 targets netstandard2.0, but replace with ASP.NET Core built-in DI |
| Castle.Windsor.Lifestyles | Both | 0.4.0 | No | ReplacePackage — only targets net45; replace with ASP.NET Core DI lifetime scoping |
| ExpressiveAnnotations.dll | Both | 2.7.4 | No | ReplacePackage — only targets .NETFramework4.5; replace with custom DataAnnotations or FluentValidation |
| Havit.CastleWindsor.WebForms | Both | 1.8.9 | No | ReplacePackage — only targets .NETFramework4.7.2; remove (WebForms-specific Windsor integration) |
| LiteDB | Both | 4.1.4 | Yes | UpgradePackage to 5.0.21 (targets netstandard2.0) |
| Microsoft.AspNet.Cors | Both | 5.2.7 | No | ReplacePackage — .NETFramework-only; use built-in ASP.NET Core CORS middleware |
| Microsoft.AspNet.Identity.Core | Both | 2.2.2 | No | ReplacePackage — .NETFramework-only; use Microsoft.AspNetCore.Identity |
| Microsoft.AspNet.WebApi.Client | Both | 5.2.7 | Yes | ReplacePackage — latest 6.0.0 targets netstandard2.0, but not needed in ASP.NET Core |
| Microsoft.AspNet.WebApi.Core | Both | 5.2.7 | No | ReplacePackage — .NETFramework-only; use Microsoft.AspNetCore.Mvc.Core |
| Microsoft.AspNet.WebApi.Cors | Both | 5.2.7 | No | ReplacePackage — .NETFramework-only; use built-in ASP.NET Core CORS |
| Microsoft.AspNet.WebApi.Versioning | Both | 4.0.0 | No | ReplacePackage — only targets .NETFramework4.5; use Asp.Versioning.Http |
| Microsoft.AspNet.WebApi.Versioning.ApiExplorer | Both | 4.0.0 | No | ReplacePackage — only targets .NETFramework4.5; use Asp.Versioning.ApiExplorer |
| Microsoft.AspNet.WebApi.WebHost | Both | 5.2.7 | No | ReplacePackage — .NETFramework-only; not needed in ASP.NET Core |
| Microsoft.IdentityModel.JsonWebTokens | Both | 5.6.0 | Yes | UpgradePackage to 8.23.0 (targets net10.0 directly) |
| Microsoft.IdentityModel.Logging | Both | 5.6.0 | Yes | UpgradePackage to 8.23.0 (targets net10.0 directly) |
| Microsoft.IdentityModel.Tokens | Both | 5.6.0 | Yes | UpgradePackage to 8.23.0 (targets net10.0 directly) |
| Microsoft.Web.Infrastructure | Both | 1.0.0.0 | No | ReplacePackage — .NETFramework-only runtime assembly; not needed in ASP.NET Core |
| Microsoft.Web.Xdt | Both | 3.0.0 | Yes* | ReplacePackage — targets netstandard2.0 but XML transforms not used in ASP.NET Core; remove |
| MultipartDataMediaFormatter.V2 | Both | 2.0.2 | Yes | ReplacePackage — latest 2.1.1 targets netstandard2.0, but use built-in ASP.NET Core multipart support |
| Newtonsoft.Json | Both | 12.0.2 | Yes | UpgradePackage to 13.0.4 (targets netstandard2.0/net6.0) |
| NuGet.Core | Both | 2.14.0 | No | ReplacePackage — .NETFramework-only; use NuGet.Protocol or NuGet.Packaging |
| Swashbuckle.Core | Both | 5.6.0 | No | ReplacePackage — .NETFramework-only; use Swashbuckle.AspNetCore |
| System.IdentityModel.Tokens.Jwt | Both | 5.6.0 | Yes | UpgradePackage to 8.23.0 (targets net10.0 directly) |

## Cross-Project Dependencies

Net.Web.Api.Sdk.Web.Examples (Medium)
  - Net.Web.Api.Sdk (High)

Net.Web.Api.Sdk (High)
  _(no project dependencies — leaf library)_

### Recommended Transformation Order (Dependency-First)

1. **Net.Web.Api.Sdk** — Leaf library with zero project dependencies; must be migrated first since the web host depends on it
2. **Net.Web.Api.Sdk.Web.Examples** — Depends on Net.Web.Api.Sdk; migrate last after the SDK library is on net10.0

## Key Findings

1. **Complete Web API 2 to ASP.NET Core rewrite required**: Both projects are built entirely on `System.Web.Http` (Web API 2). Every controller inherits from `ApiController`, uses `IHttpActionResult`, and relies on `HttpConfiguration` — all of which must be replaced with ASP.NET Core equivalents (`ControllerBase`, `[ApiController]`, `IActionResult`, middleware pipeline).
2. **Castle Windsor DI container must be replaced**: The SDK library has a full Castle Windsor composition root (`WindsorCompositionRoot`, `WindsorDependencyResolver`, `WindsorDependencyScope`, `InjectionContainer`, custom installers) that must be replaced with ASP.NET Core's built-in dependency injection. Three Windsor-specific packages (`Castle.Facilities.AspNet.SystemWeb`, `Castle.Windsor.Lifestyles`, `Havit.CastleWindsor.WebForms`) are incompatible.
3. **Custom ConfigurationSection classes need IOptions<T> migration**: `TokenConfigurationSection`, `TokenDefinitionElement`, `TokenElement`, `TokenElementCollection`, and `TokenSignatureElement` use `System.Configuration` and must be converted to strongly-typed options bound via `IOptions<T>` from `appsettings.json`.
4. **Swagger/Swashbuckle rewrite needed**: The SDK uses `Swashbuckle.Core` 5.6.0 with custom operation filters, document filters, and embedded Swagger UI resources — all specific to the legacy Swashbuckle for Web API 2. These must be rewritten for `Swashbuckle.AspNetCore`.
5. **Both projects use old-style .csproj with packages.config**: Both projects require conversion from legacy `.csproj` format with `packages.config` to SDK-style `.csproj` with `PackageReference`.
6. **JWT token handling infrastructure is substantial**: The SDK includes custom JWT token services (`JwtTokenService`, `JwtTokenHandler`), token authorization attributes (`TokenAuthorizeAttribute`, `BasicAuthorizeAttribute`), and token configuration — these must be adapted to ASP.NET Core's authentication/authorization middleware.
7. **14 of 26 packages per project are incompatible with net10.0**: Over half of all NuGet dependencies are .NET Framework-only and require replacement with modern equivalents.

## External Dependencies

No external dependencies detected.

## Actionable Next Steps

1. **Phase 1 — Project format conversion** (Low risk): Convert both old-style `.csproj` files to SDK-style format and migrate `packages.config` entries to `PackageReference`. Remove `AssemblyInfo.cs` and enable `GenerateAssemblyInfo`.
2. **Phase 2 — Migrate Net.Web.Api.Sdk library** (High risk): Replace all `System.Web.Http` references with ASP.NET Core equivalents, replace Castle Windsor DI with built-in DI, migrate `ConfigurationSection` classes to `IOptions<T>`, rewrite Swashbuckle filters for `Swashbuckle.AspNetCore`, upgrade compatible packages, and replace incompatible packages.
3. **Phase 3 — Migrate Net.Web.Api.Sdk.Web.Examples** (Medium risk): Replace `Global.asax` with `Program.cs` and middleware pipeline, wire up ASP.NET Core DI and Swagger, update controllers, and remove Web API 2 hosting infrastructure.
4. **Phase 4 — Validation and testing** (Low risk): Verify the full solution builds, validate API endpoints respond correctly, confirm Swagger UI renders, and test JWT authentication flow.

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

#### Package Compatibility (26 packages, 14 incompatible)

| Package | Version | Compatibility | Recommendation |
|---------|---------|---------------|----------------|
| ByteSize | 1.3.0 | COMPATIBLE | UpgradePackage |
| Castle.Core | 4.4.0 | COMPATIBLE | UpgradePackage |
| Castle.Facilities.AspNet.SystemWeb | 5.0.1 | INCOMPATIBLE | ReplacePackage |
| Castle.Windsor | 5.0.1 | COMPATIBLE | ReplacePackage |
| Castle.Windsor.Lifestyles | 0.4.0 | INCOMPATIBLE | ReplacePackage |
| ExpressiveAnnotations.dll | 2.7.4 | INCOMPATIBLE | ReplacePackage |
| Havit.CastleWindsor.WebForms | 1.8.9 | INCOMPATIBLE | ReplacePackage |
| LiteDB | 4.1.4 | COMPATIBLE | UpgradePackage |
| Microsoft.AspNet.Cors | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.Identity.Core | 2.2.2 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Client | 5.2.7 | COMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Core | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Cors | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Versioning | 4.0.0 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Versioning.ApiExplorer | 4.0.0 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.WebHost | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.IdentityModel.JsonWebTokens | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.IdentityModel.Logging | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.IdentityModel.Tokens | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.Web.Infrastructure | 1.0.0.0 | INCOMPATIBLE | ReplacePackage |
| Microsoft.Web.Xdt | 3.0.0 | COMPATIBLE | ReplacePackage |
| MultipartDataMediaFormatter.V2 | 2.0.2 | COMPATIBLE | ReplacePackage |
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

1. **Convert old-style `.csproj` to SDK-style format**: Replace the legacy `<Import>` / `<Reference>` / `<Compile Include>` project file with an SDK-style `<Project Sdk="Microsoft.NET.Sdk">` targeting `net10.0`. Remove `Properties/AssemblyInfo.cs` and enable `<GenerateAssemblyInfo>true</GenerateAssemblyInfo>`. Migrate all 26 `packages.config` entries to `<PackageReference>` elements.
2. **Replace System.Web.Http (Web API 2) with ASP.NET Core**: Rewrite `SdkController.cs` and `SdkInformationController.cs` from `ApiController` to `ControllerBase` with `[ApiController]` attribute. Replace `IHttpActionResult` with `IActionResult`, `HttpResponseMessage` with `ActionResult<T>`, and `System.Web.Http` namespaces with `Microsoft.AspNetCore.Mvc`. Migrate `HttpConfigurationExtensions.cs` (initialization entry point) to ASP.NET Core service registration extensions.
3. **Replace Castle Windsor DI with ASP.NET Core built-in DI**: Remove `WindsorCompositionRoot`, `WindsorDependencyResolver`, `WindsorDependencyScope`, `InjectionContainer`, `ControllerInstaller`, and `ServiceInstaller`. Replace with `IServiceCollection` extension methods for service registration. Remove `Castle.Facilities.AspNet.SystemWeb`, `Castle.Windsor.Lifestyles`, and `Havit.CastleWindsor.WebForms` packages. Convert `InjectInterfaceServiceAttribute` and `InjectServiceCustomAttribute` to standard DI registration conventions.
4. **Migrate ConfigurationSection classes to IOptions<T>**: Convert `TokenConfigurationSection`, `TokenDefinitionElement`, `TokenElement`, `TokenElementCollection`, and `TokenSignatureElement` from `System.Configuration.ConfigurationSection` / `ConfigurationElement` to POCO classes bound via `IOptions<T>` from `appsettings.json`.
5. **Rewrite Swagger/Swashbuckle integration**: Replace `Swashbuckle.Core` 5.6.0 with `Swashbuckle.AspNetCore`. Rewrite all custom filters (`SwaggerConsumesFilter`, `SwaggerProducesFilter`, `SwaggerSecurityTypeAttributeFilter`, `SwaggerUploadOperationFilter`, `SwaggerOrderingFilter`, `SwaggerDocumentOrderingFilter`, `SwaggerMethodOrderingFilter`, `SwaggerOperationOrderingFilter`) for the ASP.NET Core Swashbuckle API. Migrate embedded Swagger UI resources to the built-in Swagger UI middleware.
6. **Upgrade compatible packages**: ByteSize → 2.1.2, Castle.Core → 5.2.1, LiteDB → 5.0.21, Microsoft.IdentityModel.JsonWebTokens → 8.23.0, Microsoft.IdentityModel.Logging → 8.23.0, Microsoft.IdentityModel.Tokens → 8.23.0, Newtonsoft.Json → 13.0.4, System.IdentityModel.Tokens.Jwt → 8.23.0.
7. **Replace remaining incompatible packages**: Microsoft.AspNet.Cors → built-in CORS middleware; Microsoft.AspNet.Identity.Core → Microsoft.AspNetCore.Identity; Microsoft.AspNet.WebApi.Versioning → Asp.Versioning.Http; Microsoft.AspNet.WebApi.Versioning.ApiExplorer → Asp.Versioning.ApiExplorer; NuGet.Core → NuGet.Protocol; MultipartDataMediaFormatter.V2 → built-in ASP.NET Core multipart support; Microsoft.Web.Infrastructure and Microsoft.Web.Xdt → remove.
8. **Migrate security infrastructure**: Adapt `TokenAuthorizeAttribute`, `BasicAuthorizeAttribute`, and `JwtTokenHandler` from Web API 2 authentication filters to ASP.NET Core authentication/authorization middleware and `IAuthorizationFilter`.

##### Risks & Architectural Concerns

| Risk | Severity | Notes |
|------|----------|-------|
| Castle Windsor DI replacement | High | Full composition root with custom resolvers, scopes, attribute-based registration, and installers must be rebuilt on built-in DI. Lifetime semantics may differ. |
| Swashbuckle.Core → Swashbuckle.AspNetCore rewrite | High | 8 custom Swagger filters with different API surface. Embedded UI resources need new delivery mechanism. |
| System.Web.Http API surface migration | High | Controllers, action results, HTTP configuration, media formatters, CORS, and Web API versioning all change API shape. |
| ConfigurationSection → IOptions<T> | Medium | Complex nested configuration (TokenElementCollection with multiple TokenElement children) requires careful mapping to POCO hierarchies. |
| LiteDB 4 → 5 breaking changes | Medium | LiteDB 5.x has significant breaking API changes from 4.x (new query syntax, removed encryption, changed connection strings). |
| ExpressiveAnnotations removal | Medium | Custom validation expressions used in attributes must be reimplemented as DataAnnotations or FluentValidation rules. |
| NuGet.Core usage in InformationService | Medium | If used at runtime for package introspection, must be replaced with NuGet.Protocol APIs which have a different programming model. |

##### Recommendations

1. Migrate the Castle Windsor DI composition root first, as it underpins controller resolution, service injection, and request scoping throughout the entire SDK.
2. Replace Swashbuckle in a separate step after controllers are migrated, since the Swagger filters depend on the controller/action attribute model.
3. Upgrade Microsoft.IdentityModel.* and System.IdentityModel.Tokens.Jwt packages early — they target net10.0 directly and the JWT token service depends on them.
4. Test LiteDB 5.x migration carefully — review `JwtTokenUsedOrRevoked` storage to ensure compatibility with the new query API.
5. Consider replacing `Newtonsoft.Json` with `System.Text.Json` for alignment with ASP.NET Core defaults, or keep it as `Microsoft.AspNetCore.Mvc.NewtonsoftJson` if serialization behavior must be preserved.

##### Cross-Project Impact

Net.Web.Api.Sdk is the foundational library — **Net.Web.Api.Sdk.Web.Examples cannot be migrated until this project is fully converted to net10.0**. The SDK's public API surface (controllers, DI registration extensions, configuration models, service interfaces) defines the contract that the example web host consumes. Changes to the DI registration pattern (Castle Windsor → built-in DI) and startup initialization (`HttpConfigurationExtensions` → ASP.NET Core service extensions) will directly affect how the web host wires up the SDK.

---

### Net.Web.Api.Sdk.Web.Examples

#### Project Metrics

| Metric | Value |
|--------|-------|
| **Framework** | net48 |
| **Lines of Code** | 650 |
| **NuGet Packages** | 26 |
| **Project References** | 1 |
| **Complexity** | Medium |
| **Estimated Changes** | 34 |

#### Package Compatibility (26 packages, 14 incompatible)

| Package | Version | Compatibility | Recommendation |
|---------|---------|---------------|----------------|
| ByteSize | 1.3.0 | COMPATIBLE | UpgradePackage |
| Castle.Core | 4.4.0 | COMPATIBLE | UpgradePackage |
| Castle.Facilities.AspNet.SystemWeb | 5.0.1 | INCOMPATIBLE | ReplacePackage |
| Castle.Windsor | 5.0.1 | COMPATIBLE | ReplacePackage |
| Castle.Windsor.Lifestyles | 0.4.0 | INCOMPATIBLE | ReplacePackage |
| ExpressiveAnnotations.dll | 2.7.4 | INCOMPATIBLE | ReplacePackage |
| Havit.CastleWindsor.WebForms | 1.8.9 | INCOMPATIBLE | ReplacePackage |
| LiteDB | 4.1.4 | COMPATIBLE | UpgradePackage |
| Microsoft.AspNet.Cors | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.Identity.Core | 2.2.2 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Client | 5.2.7 | COMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Core | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Cors | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Versioning | 4.0.0 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.Versioning.ApiExplorer | 4.0.0 | INCOMPATIBLE | ReplacePackage |
| Microsoft.AspNet.WebApi.WebHost | 5.2.7 | INCOMPATIBLE | ReplacePackage |
| Microsoft.IdentityModel.JsonWebTokens | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.IdentityModel.Logging | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.IdentityModel.Tokens | 5.6.0 | COMPATIBLE | UpgradePackage |
| Microsoft.Web.Infrastructure | 1.0.0.0 | INCOMPATIBLE | ReplacePackage |
| Microsoft.Web.Xdt | 3.0.0 | COMPATIBLE | ReplacePackage |
| MultipartDataMediaFormatter.V2 | 2.0.2 | COMPATIBLE | ReplacePackage |
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

1. **Convert old-style `.csproj` to SDK-style format**: Replace the legacy web application `.csproj` (with `ProjectTypeGuids` and `Microsoft.WebApplication.targets`) with an SDK-style `<Project Sdk="Microsoft.NET.Sdk.Web">` targeting `net10.0`. Remove `Properties/AssemblyInfo.cs`. Migrate all 26 `packages.config` entries to `<PackageReference>`.
2. **Replace `Global.asax` with `Program.cs`**: Remove `Global.asax` and `Global.asax.cs`. Create a `Program.cs` with ASP.NET Core's `WebApplicationBuilder` pattern. Move the `GlobalConfiguration.Configuration.RegisterWebApi()` call to the new service registration and middleware pipeline, using the SDK library's updated ASP.NET Core extension methods.
3. **Migrate controllers from Web API 2 to ASP.NET Core**: Rewrite `ExampleController`, `ExampleTokenController`, and `ExampleUploadController` from `ApiController` to `ControllerBase` with `[ApiController]`. Replace `IHttpActionResult` with `IActionResult`, update routing attributes, and adapt file upload handling from `MultipartDataMediaFormatter` to built-in `IFormFile`.
4. **Migrate `Web.config` to `appsettings.json`**: Extract application settings and connection strings from `Web.config` to `appsettings.json`. Remove `Web.Debug.config` and `Web.Release.config` XML transforms — use environment-specific `appsettings.{Environment}.json` files instead. Move `token-example.config` settings into the `appsettings.json` hierarchy.
5. **Migrate static content**: Move `Images/` directory contents into `wwwroot/images/`. Move `App_Data/Certficates/` (certificate files) to a project-root or configuration-driven location accessible without `App_Data`.
6. **Wire up ASP.NET Core services in `Program.cs`**: Register the SDK library's services via DI extension methods, configure Swagger with `Swashbuckle.AspNetCore`, add CORS middleware, configure JWT authentication middleware, and set up API versioning with `Asp.Versioning.Http`.
7. **Upgrade and replace packages**: Same package set as Net.Web.Api.Sdk — upgrade compatible packages and replace incompatible ones per the package compatibility table above.

##### Risks & Architectural Concerns

| Risk | Severity | Notes |
|------|----------|-------|
| Dependency on Net.Web.Api.Sdk migration | High | This project cannot compile until the SDK library is fully migrated. Any API changes in the SDK (controller base classes, DI registration, configuration models) cascade here. |
| Global.asax → Program.cs startup rewrite | Medium | The `Application_Start` calls `RegisterWebApi()` and `Application_End` calls `UnRegisterWebApi()` — the SDK's initialization extension must be adapted for ASP.NET Core before this project can wire up correctly. |
| File upload handling change | Medium | `ExampleUploadController` uses `MultipartDataMediaFormatter` — must be rewritten to use `IFormFile` / `[FromForm]` model binding. |
| Static file and certificate path changes | Low | `App_Data/Certficates/` path references must be updated since `App_Data` is not a convention in ASP.NET Core. |

##### Recommendations

1. Wait until Net.Web.Api.Sdk is fully migrated and building on net10.0 before starting this project's migration.
2. Create `Program.cs` early in the migration to establish the middleware pipeline skeleton, then wire in SDK services incrementally.
3. Test the JWT token flow end-to-end after migration — the certificate loading paths and token configuration will change significantly.
4. Remove the 14 packages that are duplicated from the SDK library's transitive dependencies — the web host may not need direct references to all 26 packages after the SDK is properly factored.

##### Cross-Project Impact

Net.Web.Api.Sdk.Web.Examples is the **downstream consumer** of the SDK library. It has a direct `ProjectReference` to `Net.Web.Api.Sdk` and consumes the SDK's controllers, DI registration, token services, and Swagger configuration. This project must be migrated **after** the SDK library. Once both are migrated, the solution will be fully on net10.0 with no remaining .NET Framework dependencies.
