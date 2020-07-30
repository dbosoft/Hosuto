using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class HostFactoryDecorator : IHostFactory
    {
        private readonly IHostFactory _decoratedHostFactory;

        public HostFactoryDecorator(IHostFactory decoratedHostFactory)
        {
            _decoratedHostFactory = decoratedHostFactory;
        }

        private void UseSimpleInjector<TModule>(ModuleStartupContext<TModule> startupContext, SimpleInjectorUseOptions options, IServiceProvider serviceProvider) where TModule: IModule
        {
            ModuleMethodInvoker.CallOptionalMethod(startupContext.Module, "UseSimpleInjector", startupContext.ServiceProvider, serviceProvider, options);
        }

        private void ConfigureContainer<TModule>(ModuleStartupContext<TModule> startupContext, IServiceProvider serviceProvider, Container container) where TModule : IModule
        {
            foreach(var configurer in startupContext.BuilderSettings.FrameworkServiceProvider.GetServices<IContainerConfigurer<TModule>>())
            {
                configurer.ConfigureContainer(startupContext.Module, container, startupContext.ServiceProvider,serviceProvider);
            }

            ModuleMethodInvoker.CallOptionalMethod(startupContext.Module, "ConfigureContainer", startupContext.ServiceProvider, serviceProvider, container);
        }

        public IHost CreateHost<TModule>(ModuleStartupContext<TModule> startupContext, Action<IHostBuilder> configureHostBuilderAction) where TModule : IModule
        {
            var host = _decoratedHostFactory.CreateHost(startupContext, configureHostBuilderAction);

            if (!(startupContext is ContainerModuleStartupContext<TModule> context)) return host;


            var container = context.Container;
            host.UseSimpleInjector(container, options => UseSimpleInjector(startupContext, options, host.Services));
            ConfigureContainer(startupContext, host.Services, container);

            container.Verify();

            return host;
        }
    }
}