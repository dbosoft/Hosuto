#if NETCOREAPP
using System;
using Microsoft.AspNetCore.Routing;

namespace Dbosoft.Hosuto.Modules
{
    /// <summary>
    /// Minimal-API-style module entry point for mapping endpoints. Implement this to map endpoints
    /// the idiomatic way (<c>endpoints.MapGet(...)</c>, <c>endpoints.MapRazorPages()</c>,
    /// <c>endpoints.MapControllers()</c>, …) instead of the Startup-era
    /// <c>Configure(app => app.UseRouting()/app.UseEndpoints(...))</c>. The framework sets up routing
    /// and calls this within the endpoint middleware, so it works the same on the classic and the
    /// minimal-API (<c>UseAspNetCoreMinimal</c>) host. A module may also implement
    /// <see cref="IApplicationConfiguringModule"/> for non-endpoint middleware.
    /// </summary>
    public interface IEndpointConfiguringModule
    {
        /// <param name="serviceProvider">The modules host's service provider.</param>
        /// <param name="endpoints">The endpoint route builder of the module's host.</param>
        void MapEndpoints(IServiceProvider serviceProvider, IEndpointRouteBuilder endpoints);
    }
}
#endif
