using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public class DelegateModuleServicesFilter: IModuleServicesFilter
    {
        private readonly Action<IModulesHostBuilderContext, IServiceCollection> _configureDelegate;

        public DelegateModuleServicesFilter(Action<IModulesHostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureDelegate = configureDelegate;
        }

        public DelegateModuleServicesFilter(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _configureDelegate = (ctx, config) => configureDelegate(ctx.HostBuilderContext, config);
        }

        public Action<IModulesHostBuilderContext, IServiceCollection> Invoke(Action<IModulesHostBuilderContext, IServiceCollection> next) =>
            (ctx, services) =>
            {
                _configureDelegate(ctx, services);
                next(ctx, services);
            };
    }
}