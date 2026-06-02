# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

**Dragonfly Umbraco 10+ Site Auditor** — a NuGet package that adds a backoffice dashboard to Umbraco sites, exposing tools for auditing content nodes, media, document types, data types, templates, properties, and logs. The package major version matches the target Umbraco major version (currently v17 = Umbraco 17, .NET 10).

## Solution Structure

```
src/
  Dragonfly/                     # The NuGet package project
    SiteAuditor/
      Services/                  # Core service layer
      WebApi/                    # API controllers + config + route attribute
      Models/                    # Auditable* model classes
    App_Plugins/Dragonfly.SiteAuditor/   # Backoffice assets (Dashboard.html, lang, package.manifest)
      RazorViews/                # Razor .cshtml views — compiled into DLL (NOT in wwwroot)
    Client/                      # TypeScript/Vite frontend (new backoffice UI)
      src/
        api/                     # Auto-generated OpenAPI client (*.gen.ts — do not hand-edit)
        dashboards/              # Lit-based dashboard element + manifest
        entrypoints/             # Backoffice entry point + manifest
    wwwroot/App_Plugins/         # Built frontend output (JS bundles, icons, umbraco-package.json)
    Directory.Packages.props     # Central NuGet version management for the package
  SiteAuditor.TestSite/          # Umbraco test site for local development (IIS, "Local" env)
```

## Commands

### .NET (run from `src/`)

```powershell
# Build the package (always use Debug — Release triggers automatic nuget push via Custom.targets)
dotnet build Dragonfly/Dragonfly.csproj -c Debug

# Run the test site (starts Umbraco at https://localhost:44394 by default)
dotnet run --project SiteAuditor.TestSite/SiteAuditor.TestSite.csproj

# Pack the NuGet package for testing (Debug adds a -prerelease{timestamp} version suffix)
dotnet pack Dragonfly/Dragonfly.csproj -c Debug
```

> **Warning:** Never run `dotnet build` or `dotnet pack` with `-c Release` unless intentionally publishing to NuGet.org. `Custom.targets` runs `nuget.exe push` to `https://www.nuget.org` automatically after every Release build.

### TypeScript Client (run from `src/Dragonfly/Client/`)

```powershell
npm install           # First-time setup
npm run build         # Compile TypeScript + bundle with Vite
npm run watch         # Watch mode for development

# Regenerate the OpenAPI TypeScript client (requires the test site to be running)
npm run generate-client
```

The `generate-client` script fetches the swagger JSON from the running test site at `https://localhost:44394/umbraco/swagger/dragonflysiteauditorui/swagger.json` and runs `@hey-api/openapi-ts`. All files in `src/Dragonfly/Client/src/api/` ending in `.gen.ts` are auto-generated — do not edit them directly.

The TestSite `.csproj` has a `CopyAppPlugins` target that copies both:
- `Dragonfly/App_Plugins/` → `SiteAuditor.TestSite/App_Plugins/` (Razor views, lang, manifests — content root)
- `Dragonfly/wwwroot/App_Plugins/` → `SiteAuditor.TestSite/wwwroot/App_Plugins/` (JS bundles, icons, umbraco-package.json)

## Architecture

### Service Layer

Three scoped C# services registered in `SetupComposer.cs`:

- **`SiteAuditorService`** — the core data layer. Wraps Umbraco's `ServiceContext` and `UmbracoHelper` to fetch content nodes, media, doc types, data types, templates, and logs. All data retrieval goes through here.
- **`AuditorInfoService`** — helper service for formatting and supplemental info (e.g., node path as text).
- **`SiteAuditorApiContentService`** — the "render layer." Its methods accept an `HttpContext`, call `SiteAuditorService` for data, then render Razor views to HTML strings via `IViewRenderService.RenderToStringAsync()`. Controllers call into this service and return the HTML string as `Content(html, "text/html")`.

### API Controllers

Two distinct API surfaces in `WebApi/`:

1. **`SiteAuditorController`** (`WebApi/SiteAuditorApiController.cs`) — the main HTML report API. The custom `SiteAuditorApiRouteAttribute` produces a base URL of `/umbraco/backoffice/Dragonfly/SiteAuditor/`. Each action is accessed via `GET /umbraco/backoffice/Dragonfly/SiteAuditor/{ActionName}`, e.g.:
   - `GetAllContentAsHtmlTable`, `GetContentForDoctypeHtml`, `GetContentForElementHtml`, `GetContentWithValues`
   - `GetAllMediaAsHtmlTable`, `GetMediaForTypeHtml`, `GetMediaWithValues`
   - `GetAllDocTypesAsHtmlTable`, `GetAllPropertiesAsHtmlTable`, `GetPropertiesForDoctypeHtml`
   - `GetContentForElementType`, `GetAllDataTypesAsHtmlTable`
   - `GetAllTemplatesAsHtmlTable`, `TemplateUsageReport`
   - `GetLogs`, `GetLogsAsHtmlTable`

   Authorization is checked manually via `IsBackOfficeAuthorized()` (inherited from `DragonflyApiControllerBase`), returning an HTML error snippet rather than an HTTP 401 when not logged in.

2. **`SiteAuditorBackofficeUIApiController`** — the Umbraco-native backoffice UI API. Uses `[BackOfficeRoute("dragonflysiteauditorui/api/v{version:apiVersion}")]` and `[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]`. Exposed under the `dragonflysiteauditorui` Swagger document. Currently has example endpoints (`ping`, `whatsTheTimeMrWolf`, `whatsMyName`, `whoAmI`). This is the API the TypeScript client communicates with.

A separate `SiteAuditorApiComposer` in `WebApi/SiteAuditorApiConfig.cs` registers the Swagger `SwaggerGenOptions` for the `dragonflysiteauditorui` document.

### Razor Views

Views live in `App_Plugins/Dragonfly.SiteAuditor/RazorViews/` (not in `wwwroot/`, not in the standard `Views/` folder). Critical rules:

- **`<AddRazorSupportForMvc>true</AddRazorSupportForMvc>` in `Dragonfly.csproj`** enables the Razor SDK to compile these views into the DLL. Without this, files in `App_Plugins/` are not compiled (and the Razor SDK emits warning `RAZORSDK1004`).
- The views are pre-compiled into the assembly, so they work in any environment (including non-Development like the "Local" test site environment) without needing runtime compilation.
- `SetupComposer` registers extra `ViewLocationFormats` (`~/App_Plugins/Dragonfly.SiteAuditor/RazorViews/{0}.cshtml`) but these only affect `FindView`, not `GetView`.
- **Always pass the full relative path** to `ViewRenderService.RenderToStringAsync()` — e.g., `"~/App_Plugins/Dragonfly.SiteAuditor/RazorViews/AllContentAsHtmlTable.cshtml"`. The `_RazorFilesPath` field in `SiteAuditorApiContentService` provides the base path.
- Do NOT move views back to `wwwroot/` — the Razor SDK explicitly excludes `wwwroot/**` from compilation.

### NuGet Package File Deployment

Consumer sites use `PackageReference` (SDK-style projects). With `PackageReference`, `content/` files from NuGet packages are **NOT** automatically copied to the consumer project — they stay in the NuGet cache. The deployment mechanism is a `buildTransitive` MSBuild targets file.

`build/Dragonfly.Umbraco10.SiteAuditor.targets` (packed into `buildTransitive/` in the `.nupkg`) runs `BeforeTargets="Build"` in every consumer project and copies:
- `{cache}/content/App_Plugins/Dragonfly.SiteAuditor/**` → `{consumer}/App_Plugins/Dragonfly.SiteAuditor/`
- `{cache}/content/wwwroot/App_Plugins/Dragonfly.SiteAuditor/**` → `{consumer}/wwwroot/App_Plugins/Dragonfly.SiteAuditor/`

The wwwroot items are packed using `None` items with explicit `PackagePath`:
```xml
<None Include="wwwroot\App_Plugins\Dragonfly.SiteAuditor\**\*.*">
    <Pack>True</Pack>
    <PackagePath>content\wwwroot\App_Plugins\Dragonfly.SiteAuditor\%(RecursiveDir)%(Filename)%(Extension)</PackagePath>
</None>
```
Using `Content` items would put the files in `staticwebassets/` in the package (which is only served in Development mode), not in `content/wwwroot/`. `None` items with explicit `PackagePath` bypass the static web asset mechanism.

### NuGet Package Versioning

All Umbraco and Dragonfly dependency versions for the package are centrally managed in `src/Dragonfly/Directory.Packages.props`. Update versions there, not in `Dragonfly.csproj`. The test site (`SiteAuditor.TestSite.csproj`) manages its own versions directly since it is not part of the central package management scope.

### Frontend (TypeScript / Lit)

The backoffice UI extension uses:
- **Lit** (`@umbraco-cms/backoffice/external/lit`) for web components
- **Vite** for bundling
- **`@hey-api/openapi-ts`** for generating the typed API client from the Swagger spec

The entry point (`src/entrypoints/entrypoint.ts`) wires the generated API client to Umbraco's `UMB_AUTH_CONTEXT` so all API calls are authenticated automatically.

Extension manifests are assembled in `src/bundle.manifests.ts` from `src/dashboards/manifest.ts` and `src/entrypoints/manifest.ts`. The bundle is loaded by `umbraco-package.json` in `wwwroot/App_Plugins/Dragonfly.SiteAuditor/`.

The dashboard (`src/dashboards/dashboard.element.ts`) should appear in the Settings section (`Umb.Section.Settings`) since it is an admin/audit tool.

## Configuration

The package reads an optional `DragonflySiteAuditor` section from `appsettings.json`:

```json
"DragonflySiteAuditor": {
  "LimitProcessingLogsLargerThanBytes": 314572800,
  "ExcludeLevelsToManageLargeLogs": ["Verbose", "Debug", "Information"],
  "NeverProcessLogsLargerThanBytes": 1048576000,
  "DataPath": "~/App_Data/DragonflySiteAuditor/"
}
```

The log size settings prevent out-of-memory errors when Serilog log files grow large.
