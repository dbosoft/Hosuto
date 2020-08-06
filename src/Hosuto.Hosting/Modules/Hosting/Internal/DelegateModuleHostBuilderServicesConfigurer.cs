using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public class DelegateModuleServicesConfigurer : IModuleServicesConfigurer
    {
        private readonly Action<IModulesHostBuilderContext, IServiceCollection> _configureDelegate;

        public DelegateModuleServicesConfigurer(Action<IModulesHostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureDelegate = configureDelegate;
        }

        public DelegateModuleServicesConfigurer(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureDelegate = (ctx, config) => configureDelegate(ctx.HostBuilderContext, config);
        }

        public void ConfigureServices(IModulesHostBuilderContext context, IServiceCollection services) =>
            _configureDelegate(context, services);
    }
}