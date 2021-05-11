using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public class ModuleHostingOptions : IModuleHostingOptions
    {
        public Action<IServiceProvider> BootstrapAction { get; private set; }
        public Action<IHostBuilder> ConfigureBuilderAction { get; private set; }

        public Action<IModuleContext> ConfigureContextAction { get; private set; }

        public Func<IServiceProvider, object> ModuleFactory { get; private set; }

        public bool ConfigureContextCalled { get; set; }

        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public IModuleHostingOptions Configure(Action<IServiceProvider> configureAction)
        {
            BootstrapAction = configureAction;
            return this;
        }

        public IModuleHostingOptions Configure(Action<IHostBuilder> configureAction)
        {
            ConfigureBuilderAction = configureAction;
            return this;
        }

        public IModuleHostingOptions Configure(Action<IModuleContext> configureAction)
        {
            Configure<IModuleContext>(configureAction);
            return this;
        }

        public IModuleHostingOptions Configure<TContext>(Action<TContext> configureAction) where TContext : class, IModuleContext
        {
            ConfigureContextAction = (ctx) => configureAction(ctx as TContext);
            return this;
        }

        public IModuleHostingOptions ModuleFactoryCallback(Func<IServiceProvider, object> moduleFactory)
        {
            ModuleFactory = moduleFactory;
            return this;
        }
    }
}