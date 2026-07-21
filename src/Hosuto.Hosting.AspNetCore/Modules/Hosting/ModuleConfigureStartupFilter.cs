#if NET6_0_OR_GREATER
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    // Runs the module's Configure delegate when the request pipeline is built (at start), which is
    // after the module container has been configured by the bootstrap pipeline.
    internal sealed class ModuleConfigureStartupFilter : IStartupFilter
    {
        private readonly Action<IApplicationBuilder> _configure;

        public ModuleConfigureStartupFilter(Action<IApplicationBuilder> configure)
        {
            _configure = configure;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                _configure(app);
                next(app);
            };
        }
    }

    /// <summary>
    /// Opt-in hook to configure the minimal-API <see cref="WebApplicationBuilder"/> of a web module
    /// hosted via <c>UseAspNetCoreMinimal()</c>.
    /// </summary>
    public interface IWebApplicationBuilderConfigurer
    {
        void Configure(WebApplicationBuilder builder);
    }

    internal sealed class DelegateWebApplicationBuilderConfigurer : IWebApplicationBuilderConfigurer
    {
        private readonly Action<WebApplicationBuilder> _configure;

        public DelegateWebApplicationBuilderConfigurer(Action<WebApplicationBuilder> configure)
        {
            _configure = configure;
        }

        public void Configure(WebApplicationBuilder builder) => _configure(builder);
    }
}
#endif
