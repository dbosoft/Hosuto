using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    public class ModuleHostBuilderSettings
    {
        public readonly HostBuilderContext HostBuilderContext;
        public readonly IEnumerable<Action<HostBuilderContext, IConfigurationBuilder>> ConfigurationActions;
        public readonly IEnumerable<Action<HostBuilderContext, IServiceCollection>> ConfigureServicesActions;

        public ModuleHostBuilderSettings(HostBuilderContext hostBuilderContext,
            IEnumerable<Action<HostBuilderContext, IConfigurationBuilder>> configurationActions, 
            IEnumerable<Action<HostBuilderContext, IServiceCollection>> configureServicesActions, 
            IServiceProvider frameworkServiceProvider)
        {
            HostBuilderContext = hostBuilderContext;
            ConfigurationActions = configurationActions;
            ConfigureServicesActions = configureServicesActions;
            FrameworkServiceProvider = frameworkServiceProvider;
        }

        public IServiceProvider FrameworkServiceProvider { get;  }

    }
}