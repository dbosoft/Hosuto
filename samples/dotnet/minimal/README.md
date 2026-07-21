# Minimal-API host sample (net10, Razor)

Demonstrates the opt-in **`UseAspNetCoreMinimal()`** path: a Hosuto web module is hosted on a
minimal-API `WebApplication` inner host instead of the classic `ConfigureWebHostDefaults` host. The
module (`RazorModule`) is an ordinary Hosuto `WebModule` using the module interfaces
(`IServiceConfiguringModule` / `IApplicationConfiguringModule`) and is authored exactly like a
module for the multi-host builder.

Run it:

```
dotnet run --project App
```

The host starts the module on an ephemeral port, then probes two URLs and prints the results.

## What it shows

- `GET /` → **200** — Razor Pages are discovered and rendered on the minimal-API module host.
- `GET /css/site.css` → **200** — the module's **static web assets** are served by the module host,
  in both `dotnet run` (dev) and `dotnet publish` output.
- Dependency injection / SimpleInjector module container (`ConfigureContainer`) works post-build.
- The module is authored with the three module contracts: `IServiceConfiguringModule` (AddRazorPages),
  `IApplicationConfiguringModule` (UseStaticFiles middleware) and `IEndpointConfiguringModule`
  (MapRazorPages).

## How module static web assets are served

Each module's inner `WebApplication` uses `ApplicationName` = the module assembly, so the module's
own static web assets manifest is already root-mapped. The host composes those assets onto the
module's `WebRootFileProvider` after build (`StaticWebAssetsLoader.UseStaticWebAssets` for dev +
`ModuleWebAssetsLoader.UseModuleAssets` for published output) — no `.modules/{module}` prefix
stripping is needed, and each module host serves only its own assets.
