using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Dbosoft.Hosuto.Modules;

namespace Dbosoft.Hosuto.Samples.Minimal.RazorModule
{
    // A Razor web module on the minimal-API WebApplication inner host, authored with the three
    // module contracts - services, middleware, and (minimal-API-style) endpoints:
    //   IServiceConfiguringModule    -> AddRazorPages
    //   IApplicationConfiguringModule -> UseStaticFiles (middleware)
    //   IEndpointConfiguringModule    -> MapRazorPages (idiomatic endpoint mapping)
    public sealed class RazorGreetModule
        : WebModule, IServiceConfiguringModule, IApplicationConfiguringModule, IEndpointConfiguringModule
    {
        public override string Path { get; } = "";

        public void ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services)
        {
            services.AddRazorPages();
        }

        public void Configure(IServiceProvider serviceProvider, IApplicationBuilder app)
        {
            app.UseStaticFiles();
        }

        public void MapEndpoints(IServiceProvider serviceProvider, IEndpointRouteBuilder endpoints)
        {
            endpoints.MapRazorPages();
        }
    }
}
