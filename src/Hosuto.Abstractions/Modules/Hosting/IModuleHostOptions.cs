using System;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleHostingOptions
    {
        IModuleHostingOptions Configure(Action<IServiceProvider> configureAction);
        IModuleHostingOptions Configure(Action<IHostBuilder> configureAction);

        IModuleHostingOptions Configure(Action<IModuleContext> configureAction);

        IModuleHostingOptions Configure<TContext>(Action<TContext> configureAction)
            where TContext : class, IModuleContext;

    }
}