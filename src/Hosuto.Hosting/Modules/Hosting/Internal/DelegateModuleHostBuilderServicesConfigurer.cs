using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public class DelegateModuleServicesConfigurer : IModuleServicesConfigurer
    {
        private readonly Action<IModuleHostBuilderContext, IServiceCollection> _configureDelegate;

        public DelegateModuleServicesConfigurer(Action<IModuleHostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureDelegate = configureDelegate;
        }

        public DelegateModuleServicesConfigurer(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureDelegate = (ctx, config) => configureDelegate(ctx.HostBuilderContext, config);
        }

        public void ConfigureServices(IModuleHostBuilderContext context, IServiceCollection services) =>
            _configureDelegate(context, services);
    }
}