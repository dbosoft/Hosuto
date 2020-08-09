using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class ModuleContextFactory<TModule> : IModuleContextFactory<TModule> where TModule: IModule
    {

        public IModuleContext<TModule> CreateModuleContext(IModuleBootstrapContext<TModule> bootstrapContext, IServiceProvider moduleServices)
        {
            if(!(bootstrapContext is ISimpleInjectorModuleContext containerContext))
                throw new InvalidOperationException($"{nameof(bootstrapContext)} contains no SimpleInjector container. This is not expected here. Actual type: {bootstrapContext.GetType()}");

            return new ModuleContextWithContainer<TModule>(containerContext.Container,
                bootstrapContext.Module,
                moduleServices,
                bootstrapContext.ModulesHostServices,
                bootstrapContext.Advanced.FrameworkServices);
        }

        public IModuleBootstrapContext<TModule> CreateModuleBootstrapContext(TModule module, IServiceProvider moduleHostServices,
            IServiceProvider frameworkServices)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Options.ResolveUnregisteredConcreteTypes = false;
            container.Register<IServiceScopeFactory, SimpleInjectorScopeFactory>();
            return new ModuleContextWithContainer<TModule>(container, module, null, moduleHostServices, frameworkServices);
        }
    }
}