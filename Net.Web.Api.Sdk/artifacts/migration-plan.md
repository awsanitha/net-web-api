# Migration Plan: Net.Web.Api.Sdk

## Solution Overview

| Attribute | Value |
|-----------|-------|
| **Solution** | Net.Web.Api.Sdk.sln |
| **Solution Path** | /data/code/Net.Web.Api.Sdk/Net.Web.Api.Sdk.sln |
| **Target Framework** | net10.0 |
| **Total Projects** | 2 |
| **Projects to Migrate** | 2 |

Execution Mode: parallel

## Dependency Levels

### Level 1 — Leaf Libraries (1 project)

#### Project: Net.Web.Api.Sdk — DONE

- **Type:** Class Library (net48 → net10.0)
- **Complexity:** High
- **Dependencies:** None
- **LOC:** 5684 | **Packages:** 26 (13 incompatible)

**Transformation Steps:**

1. **Convert project format** — Convert old-style .csproj to SDK-style (`Microsoft.NET.Sdk`) targeting net10.0. Migrate packages.config to PackageReference. Remove Properties/AssemblyInfo.cs and legacy .resx if auto-generated.
2. **Replace incompatible NuGet packages** — Remove ASP.NET Web API 2 packages (Microsoft.AspNet.WebApi.*), Castle.Facilities.AspNet.SystemWeb, Castle.Windsor.Lifestyles, Havit.CastleWindsor.WebForms, ExpressiveAnnotations.dll, MultipartDataMediaFormatter.V2, NuGet.Core, Swashbuckle.Core. Add Swashbuckle.AspNetCore, Asp.Versioning.Mvc.ApiExplorer. Upgrade compatible packages (ByteSize, Castle.Core, LiteDB, Newtonsoft.Json, Microsoft.IdentityModel.*, System.IdentityModel.Tokens.Jwt).
3. **Migrate ASP.NET Web API 2 to ASP.NET Core** — Replace `System.Web.Http` namespaces with `Microsoft.AspNetCore.Mvc`. Convert `ApiController` → `ControllerBase` with `[ApiController]`. Convert `IHttpActionResult` → `IActionResult`. Convert `HttpResponseMessage` return types to appropriate `ActionResult` types.
4. **Replace Castle Windsor DI with built-in DI** — Remove WindsorCompositionRoot, WindsorDependencyResolver, WindsorDependencyScope, ControllerInstaller, ServiceInstaller, InjectionContainer. Create `IServiceCollection` extension methods (e.g., `AddSdkServices()`) that replicate the registration logic.
5. **Convert ConfigurationSection to IOptions<T>** — Replace TokenConfigurationSection/TokenElement/TokenDefinitionElement/TokenSignatureElement/TokenElementCollection with POCO classes. Implement IOptions<T> binding pattern for appsettings.json.
6. **Port Swashbuckle filters** — Migrate all 8 Swashbuckle.Core filter classes (SwaggerConsumesFilter, SwaggerProducesFilter, SwaggerSecurityTypeAttributeFilter, SwaggerUploadOperationFilter, SwaggerOrderingFilter, SwaggerDocumentOrderingFilter, SwaggerMethodOrderingFilter, SwaggerOperationOrderingFilter) from Swashbuckle.Swagger interfaces to Swashbuckle.AspNetCore equivalents (OpenApiDocument/OpenApiOperation).
7. **Migrate authorization attributes** — Convert BasicAuthorizeAttribute and TokenAuthorizeAttribute from System.Web.Http.Filters.AuthorizationFilterAttribute to ASP.NET Core IAuthorizationFilter or policy-based authorization.
8. **Convert JwtTokenHandler** — Replace DelegatingHandler with ASP.NET Core middleware or authentication handler.
9. **Replace HttpConfigurationExtensions** — Convert RegisterWebApi/UnRegisterWebApi extension methods to ASP.NET Core service registration and middleware configuration extensions.
10. **Replace ExpressiveAnnotations** — Convert custom validation attributes (TokenNameExistsAttribute, TokenPayloadValidAttribute) to standard DataAnnotations or custom validators.
11. **Replace NuGet.Core usage** — Replace assembly version reading in InformationService with Assembly.GetName().Version or FileVersionInfo.
12. **Build verification** — Ensure the project compiles cleanly targeting net10.0.

── STOP: Present progress to user. Wait for acknowledgement. ──

### Level 2 — Dependent Projects (1 project)

#### Project: Net.Web.Api.Sdk.Web.Examples — DONE

- **Type:** Web Application (net48 → net10.0)
- **Complexity:** Medium
- **Dependencies:** Net.Web.Api.Sdk
- **LOC:** 650 | **Packages:** 26 (13 incompatible)

**Transformation Steps:**

1. **Convert project format** — Convert old-style web application .csproj to SDK-style (`Microsoft.NET.Sdk.Web`) targeting net10.0. Migrate packages.config to PackageReference. Remove Properties/AssemblyInfo.cs.
2. **Replace incompatible NuGet packages** — Same package replacements as the SDK project. Most packages are inherited through the project reference; only add packages unique to this project.
3. **Replace Global.asax with Program.cs** — Create Program.cs using `WebApplication.CreateBuilder` pattern. Map the Application_Start logic (GlobalConfiguration.Configuration.RegisterWebApi()) to builder.Services registration and app middleware pipeline.
4. **Convert Web.config to appsettings.json** — Migrate appSettings, connectionStrings, and custom configuration sections. Remove assembly binding redirects and system.webServer handler configuration.
5. **Migrate controllers** — Convert ExampleController, ExampleTokenController, ExampleUploadController from ApiController to ControllerBase with [ApiController]. Update IHttpActionResult return types to IActionResult.
6. **Register services in Program.cs** — Wire up IPrimeNumberService → PrimeNumberService via builder.Services.AddScoped<>(). Register SDK services via the SDK's new extension methods (AddSdkServices()).
7. **Migrate static content and certificates** — Move certificate files from App_Data/Certficates to appropriate location. Move static image content to wwwroot/. Configure UseStaticFiles().
8. **Remove legacy files** — Delete Global.asax, Global.asax.cs, Web.config transform files (Web.Debug.config, Web.Release.config), and other legacy artifacts.
9. **Build verification** — Ensure the project and full solution compile cleanly targeting net10.0.

── STOP: Present progress to user. Wait for acknowledgement. ──

## Reference Documents

The following skill reference documents apply to this migration:

| Reference | Applies To |
|-----------|-----------|
| sdk-conversion-steps | Both projects (old-style → SDK-style conversion) |
| shared-migration-rules | Both projects (System.Web removal, ConfigurationManager) |
| aspnet-webapi-to-core | Both projects (Web API 2 → ASP.NET Core) |
| mvc-startup-composition | Net.Web.Api.Sdk.Web.Examples (Program.cs composition) |
| webconfig-migration | Net.Web.Api.Sdk.Web.Examples (Web.config → appsettings.json) |
| custom-config-to-ioptions | Net.Web.Api.Sdk (ConfigurationSection → IOptions<T>) |
| aspnet-security-migration | Net.Web.Api.Sdk (authorization attributes) |
| aspnet-static-files-content | Net.Web.Api.Sdk.Web.Examples (static files, App_Data) |
| di-graph-completeness | Both projects (Castle Windsor → built-in DI validation) |
| anti-patterns | Both projects (post-build quality gate) |
