using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public class DelegateModuleHostBuilderConfigurationConfigurer : IModuleConfigurationConfigurer
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


        public void ConfigureModuleConfiguration(IModulesHostBuilderContext context, IConfigurationBuilder configuration)
            => _configureDelegate(context, configuration);
    }
}