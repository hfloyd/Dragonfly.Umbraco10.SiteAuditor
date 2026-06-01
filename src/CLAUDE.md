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
# Build the package
dotnet build Dragonfly/Dragonfly.csproj

# Run the test site (starts Umbraco at https://localhost:44394 by default)
dotnet run --project SiteAuditor.TestSite/SiteAuditor.TestSite.csproj

# Pack the NuGet package
dotnet pack Dragonfly/Dragonfly.csproj
```

### TypeScript Client (run from `src/Dragonfly/Client/`)

```powershell
npm install           # First-time setup
npm run build         # Compile TypeScript + bundle with Vite
npm run watch         # Watch mode for development

# Regenerate the OpenAPI TypeScript client (requires the test site to be running)
npm run generate-client
```

The `generate-client` script fetches the swagger JSON from the running test site at `https://localhost:44394/umbraco/swagger/dragonflysiteauditorui/swagger.json` and runs `@hey-api/openapi-ts`. All files in `src/Dragonfly/Client/src/api/` ending in `.gen.ts` are auto-generated — do not edit them directly.

The TestSite `.csproj` has a `CopyAppPlugins` target that copies `Dragonfly/App_Plugins/` into `SiteAuditor.TestSite/App_Plugins/` (the content root, **not** wwwroot) before each build. Frontend JS bundles in `Dragonfly/wwwroot/App_Plugins/` are served via ASP.NET Core static web assets and do not need copying.

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
