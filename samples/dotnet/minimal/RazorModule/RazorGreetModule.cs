using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Dbosoft.Hosuto.Modules;

namespace Dbosoft.Hosuto.Samples.Minimal.RazorModule
{
    // A Razor web module hosted on the minimal-API WebApplication inner host. Exercises Razor Pages
    // and module static assets (wwwroot/css/site.css, namespaced via StaticWebAssetBasePath).
    public sealed class RazorGreetModule : WebModule, IServiceConfiguringModule, IApplicationConfiguringModule
    {
        public override string Path { get; } = "";

        public void ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services)
        {
            services.AddRazorPages();
        }

        public void Configure(IServiceProvider serviceProvider, IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapRazorPages());
        }
    }
}
