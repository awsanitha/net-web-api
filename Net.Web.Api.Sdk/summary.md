# Migration Summary — .NET Framework 4.8 → .NET 10

## Build Result
✅ **`dotnet build` exits with code 0 — 0 errors, 0 warnings.**

Both projects build cleanly:
- `Net.Web.Api.Sdk` (library) → `bin/Debug/net10.0/Net.Web.Api.Sdk.dll`
- `Net.Web.Api.Sdk.Web.Examples` (web app) → `bin/Debug/net10.0/Net.Web.Api.Sdk.Web.Examples.dll`

---

## What Changed

### Project Files

| File | Change |
|------|--------|
| `Net.Web.Api.Sdk.csproj` | Replaced all .NET Framework / old Web API packages with ASP.NET Core equivalents. Added `<FrameworkReference Include="Microsoft.AspNetCore.App" />`. Removed `GenerateAssemblyInfo` conflict. |
| `Net.Web.Api.Sdk.Web.Examples.csproj` | Converted from old-style XML MSBuild format to `Microsoft.NET.Sdk.Web` SDK-style. |

### New Packages

| Package | Version | Replaces |
|---------|---------|---------|
| `Asp.Versioning.Mvc` | 8.1.0 | `Microsoft.AspNet.WebApi.Versioning` |
| `Asp.Versioning.Mvc.ApiExplorer` | 8.1.0 | `Microsoft.AspNet.WebApi.Versioning.ApiExplorer` |
| `Swashbuckle.AspNetCore` | 7.3.2 | `Swashbuckle.Core` |
| `Swashbuckle.AspNetCore.Annotations` | 7.3.2 | `Swashbuckle.Swagger.Annotations` |
| `Swashbuckle.AspNetCore.Newtonsoft` | 7.3.2 | (new — Newtonsoft schema support) |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | 10.0.0 | `Newtonsoft.Json` formatter wiring |
| `LiteDB` | 5.0.21 | `LiteDB` 4.1.4 (critical vulnerability fixed) |
| `Microsoft.IdentityModel.*` / `System.IdentityModel.Tokens.Jwt` | 8.9.0 | 5.6.0 (moderate vulnerability fixed) |
| `ByteSize` | 2.1.2 | 1.3.0 (now targets netstandard2.0) |

### Removed Packages
`Microsoft.AspNet.WebApi.*`, `Microsoft.AspNet.Identity.Core`, `MultipartDataMediaFormatter.V2`,
`Castle.Facilities.AspNet.SystemWeb`, `Castle.Windsor.Lifestyles`, `Havit.CastleWindsor.WebForms`,
`Microsoft.AspNetCore.SystemWebAdapters`, `Microsoft.Web.Xdt`, `NuGet.Core`,
`Swashbuckle.Core`, `ExpressiveAnnotations.dll`, `Microsoft.CSharp`, `System.Data.DataSetExtensions`,
`System.ComponentModel.Annotations` (in .NET 10 framework bundle), `Microsoft.Web.Infrastructure`

---

## Key Architecture Changes

### 1. Hosting Model — `Global.asax` → `Program.cs`
`Global.asax` / `HttpApplication` removed. New `Program.cs` calls:
```csharp
builder.Services.AddSdkWebApi(builder.Environment);
app.UseSdkWebApi(builder.Environment);
```

### 2. Initialization — `HttpConfigurationExtensions` → `WebApiSdkExtensions`
`HttpConfiguration.RegisterWebApi()` split into:
- `IServiceCollection.AddSdkWebApi()` — registers DI, versioning, Swagger, CORS
- `IApplicationBuilder.UseSdkWebApi()` — configures middleware pipeline

### 3. DI — Castle.Windsor Retained
Castle.Windsor is kept for service registration. `InjectionContainer.Instance.GetService<T>()` is preserved for use in filters and validation attributes that can't use constructor injection. The Windsor container is populated by `ServiceInstaller` using `AppDomain.CurrentDomain.GetAssemblies()` (replaces `FromAssemblyInDirectory` which required a file-system scan that doesn't work the same way in .NET 10).

### 4. Controllers — `ApiController` → `ControllerBase`
- `ApiController` → `ControllerBase` + `[ApiController]`
- `IHttpActionResult` → `IActionResult`
- `[RoutePrefix(...)]` → `[Route(...)]`
- `HttpActionContext` → `ActionContext` / `AuthorizationFilterContext`
- Route template: `api/v{api-version:apiVersion}` → `api/v{version:apiVersion}`

### 5. Security — `IAuthenticationFilter` / `DelegatingHandler` → ASP.NET Core Filters / Middleware
- `JwtTokenHandler` (DelegatingHandler) → `JwtTokenMiddleware` (ASP.NET Core middleware)
- `TokenAuthorizeAttribute` → implements `IAsyncAuthorizationFilter` directly
- `BasicAuthorizeAttribute` → implements `IAsyncAuthorizationFilter` (was `IAuthenticationFilter`)

### 6. Swagger — `Swashbuckle.Core` → `Swashbuckle.AspNetCore`
All filter interfaces rewritten:
- `IOperationFilter.Apply(Operation, SchemaRegistry, ApiDescription)` → `IOperationFilter.Apply(OpenApiOperation, OperationFilterContext)`
- `IDocumentFilter.Apply(SwaggerDocument, SchemaRegistry, IApiExplorer)` → `IDocumentFilter.Apply(OpenApiDocument, DocumentFilterContext)`
- Old `Swashbuckle.Swagger.Annotations.[SwaggerOperation/SwaggerResponse]` → `Swashbuckle.AspNetCore.Annotations`
- `[SwaggerResponse(HttpStatusCode.OK)]` → `[SwaggerResponse((int)HttpStatusCode.OK)]`

### 7. `HttpContext.Current` → Injected Alternatives
- `HttpContext.Current.Server.MapPath(@"\")` → `InjectionContainer.Instance.GetContentRootPath()` (set during startup from `IWebHostEnvironment.ContentRootPath`)
- `HttpContext.Current.Request.Url` → `IHttpContextAccessor.HttpContext.Request` in `FileService`
- `HttpContext.Current.User` → `context.HttpContext.User` / `Thread.CurrentPrincipal`

### 8. X509Certificate2 — `Import()` → `X509CertificateLoader`
- `new X509Certificate2(bytes)` → `X509CertificateLoader.LoadCertificate(bytes)`
- `new X509Certificate2(bytes, pwd, flags)` → `X509CertificateLoader.LoadPkcs12(bytes, pwd, flags)`
Resolves SYSLIB0057 obsolete-API warnings.

### 9. `Settings.Designer.cs` — `ApplicationSettingsBase` → Static Sealed Class
`System.Configuration.ApplicationSettingsBase` is not available in .NET 10. The designer file was replaced with a simple `sealed class Settings` with hard-coded default values that match the original `app.config` settings.

### 10. `MultipartDataMediaFormatter` → `IFormFile`
`HttpFile` removed throughout:
- `UploadRequest.FileInformation` is now `IFormFile`
- `UploadFileAttribute` validates `IFormFile` instead of `HttpFile`
- `ExampleUploadController` uses `[FromForm]` + `await file.CopyToAsync(ms)`
- `SwaggerUploadOperationFilter` builds the OpenAPI form-data schema from `IFormFile` properties

### 11. `NuGet.Core` Removed
`InformationService.GetSdkInformations()` no longer reads `packages.config` (not applicable in SDK-style projects). The packages section was removed from the response. Assembly metadata (version, title, etc.) is still returned.

---

## Behavioral Notes
- Token `.config` files are still read via `System.Configuration.ConfigurationManager` (unchanged).
- The `InjectionContainer` content-root path is now set during `AddSdkWebApi()` startup, so `JwtTokenService` and `JwtTokenModel` find config and certificate files relative to `IWebHostEnvironment.ContentRootPath`.
- Windsor `ControllerInstaller` is retained as a placeholder but no longer registers controllers (ASP.NET Core handles controller activation natively).

---

## Next Steps

- **Windsor assembly scanning**: `ServiceInstaller` uses `AppDomain.CurrentDomain.GetAssemblies()` which only returns assemblies already loaded at startup. If services live in lazily-loaded assemblies, consider explicitly loading them before `AddSdkWebApi()` is called, or pre-warm with an `Assembly.Load` call.
- **Swagger versioned endpoints**: The `UseSdkWebApi` call currently registers a single `/v1/swagger.json` endpoint. If multiple API versions are registered, the `SwaggerUI` endpoint list should be dynamically built from `IApiVersionDescriptionProvider`.
- **Production HTTPS**: Add `app.UseHttpsRedirection()` in the consuming app's `Program.cs` for production deployments.
- **Content negotiation**: The SDK uses Newtonsoft.Json exclusively. If the consuming application also needs `System.Text.Json` routes, the serializer configuration will need to be reconciled.
