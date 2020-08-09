using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public class DelegateModuleHostBuilderConfigurationConfigurer : IModuleConfigurationFilter
    {
        private readonly Action<IModulesHostBuilderContext, IConfigurationBuilder> _configureDelegate;

        public DelegateModuleHostBuilderConfigurationConfigurer(Action<IModulesHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _configureDelegate = configureDelegate;
        }

        public DelegateModuleHostBuilderConfigurationConfigurer(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _configureDelegate = (ctx, config) => configureDelegate(ctx.HostBuilderContext, config);
        }

        public Action<IModulesHostBuilderContext, IConfigurationBuilder> Invoke(Action<IModulesHostBuilderContext, IConfigurationBuilder> next)
        {
            return (ctx, config) =>
            {
                _configureDelegate(ctx, config);
                next(ctx, config);
            };
        }
    }
}