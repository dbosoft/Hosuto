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

## What works

- `GET /` → **200** — Razor Pages are discovered and rendered on the minimal-API module host.
- Dependency injection / SimpleInjector module container (`ConfigureContainer`) works post-build.

## Known limitation (tracked as a follow-up)

- `GET /css/site.css` → **404** — a module's **static web assets are not yet mapped** on the
  minimal-API host.

  The classic host wires module assets via `ModuleWebAssetsLoader`, which is **file-provider /
  XML-manifest** based (`{app}.StaticWebAssets.xml`) and manipulates `WebRootFileProvider`. Since
  .NET 9 static web assets are **endpoint-based** (`MapStaticAssets`,
  `{app}.staticwebassets.*.json`), that mechanism no longer applies. Mapping a module's static web
  assets on net9/10 (filtering the host manifest's `.modules/{module}` entries into the module host)
  is a separate piece of work and is intentionally **not** part of the initial `UseAspNetCoreMinimal`
  handler.
