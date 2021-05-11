using System;
using System.Linq;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BootstrapHostFilter<TModule> : IBootstrapHostFilter<TModule> where TModule : class
    {

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



        public Action<BootstrapModuleHostCommand<TModule>> Invoke(Action<BootstrapModuleHostCommand<TModule>> next) =>
            (command) =>
            {
                next(command);

                if (!(command.ModuleContext is ModuleContextWithContainer<TModule> context)) return;

                var container = context.Container;
                command.Host.UseSimpleInjector(container, o => UseSimpleInjector(command.ModuleContext, o));
                ConfigureContainer(command.ModuleContext, container);
                container.RegisterInstance((IServiceProvider) container);

                command.Options.ConfigureContextCalled = true;
                command.Options.ConfigureContextAction?.Invoke(command.ModuleContext);

                container.Verify();

            };
    }
}