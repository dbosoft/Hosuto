using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleHostingOptions
    {
        IDictionary<string,object> Properties { get; }
        IModuleHostingOptions Configure(Action<IServiceProvider> configureAction);
        IModuleHostingOptions Configure(Action<IHostBuilder> configureAction);

        IModuleHostingOptions Configure(Action<IModuleContext> configureAction);

        IModuleHostingOptions Configure<TContext>(Action<TContext> configureAction)
            where TContext : class, IModuleContext;

        IModuleHostingOptions ModuleFactoryCallback(Func<IServiceProvider, IModule> moduleFactory);


    }
}