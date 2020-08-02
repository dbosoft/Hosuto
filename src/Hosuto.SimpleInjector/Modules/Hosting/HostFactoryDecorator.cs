using System;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
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

        private void UseSimpleInjector<TModule>(IModuleContext<TModule> moduleContext, SimpleInjectorUseOptions options) where TModule: IModule
        {
            ModuleMethodInvoker.CallOptionalMethod(moduleContext.ToBootstrapContext(), "UseSimpleInjector", moduleContext.Services, options);
        }

        private void ConfigureContainer<TModule>(IModuleContext<TModule> moduleContext, Container container) where TModule : IModule
        {
            foreach(var configurer in moduleContext.Advanced.FrameworkServices.GetServices<IContainerConfigurer<TModule>>())
            {
                configurer.ConfigureContainer(moduleContext, container);
            }

            ModuleMethodInvoker.CallOptionalMethod(moduleContext.ToBootstrapContext(), "ConfigureContainer", moduleContext.Services, container);
        }

        public (IHost Host, IModuleContext<TModule> ModuleContext) CreateHost<TModule>(IModuleBootstrapContext<TModule> bootstrapContext, ModuleHostingOptions options) where TModule : IModule
        {
            var (host,moduleContext) = _decoratedHostFactory.CreateHost(bootstrapContext, options);
            if (!(moduleContext is ModuleContextWithContainer<TModule> context)) return (host, moduleContext);

            var container = context.Container;
            host.UseSimpleInjector(container, o => UseSimpleInjector(moduleContext, o));
            ConfigureContainer(moduleContext, container);
            container.RegisterInstance((IServiceProvider) container);

            options.ConfigureContextCalled = true;
            options.ConfigureContextAction?.Invoke(moduleContext);

            container.Verify();

            return (host, moduleContext);
        }
    }
}