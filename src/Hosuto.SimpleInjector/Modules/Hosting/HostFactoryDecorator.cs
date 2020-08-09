using System;
using System.Linq;
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

        private static void UseSimpleInjector(IModuleContext moduleContext, SimpleInjectorUseOptions options)
        {
            Filters.BuildFilterPipeline(
                moduleContext.Advanced.FrameworkServices.GetServices<IUseSimpleInjectorFilter>()
                    .Append(GenericModuleContextFilterAdapter<SimpleInjectorUseOptions>.Create(typeof(IUseSimpleInjectorFilter<>))),
                (ctx, o) =>
                {
                    ModuleMethodInvoker.CallOptionalMethod(ctx, "UseSimpleInjector", o);

                })(moduleContext, options);
            
        }

        private static void ConfigureContainer(IModuleContext moduleContext, Container container)
        {
            Filters.BuildFilterPipeline(
                moduleContext.Advanced.FrameworkServices.GetServices<IConfigureContainerFilter>()
                    .Append(GenericModuleContextFilterAdapter<Container>.Create(typeof(IConfigureContainerFilter<>))),
                (ctx, c) =>
                {
                    ModuleMethodInvoker.CallOptionalMethod(ctx, "ConfigureContainer", container);

                })(moduleContext, container);

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