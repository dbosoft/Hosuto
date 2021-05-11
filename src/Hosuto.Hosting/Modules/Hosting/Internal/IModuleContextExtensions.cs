using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public static class ModuleContextExtensions
    {
        public static IModuleBootstrapContext<TModule> ToBootstrapContext<TModule>(
            this IModuleContext<TModule> moduleContext) where TModule : class
        {
            return moduleContext.Advanced.FrameworkServices.GetRequiredService<IModuleContextFactory<TModule>>()
                .CreateModuleBootstrapContext(moduleContext.Module, moduleContext.ModulesHostServices,
                    moduleContext.Advanced.FrameworkServices);

        }

        public static IModulesHostBuilderContext ToModuleHostBuilderContext<TModule>(
            this IModuleContext<TModule> moduleContext, HostBuilderContext hostBuilderContext = null) where TModule : class
        {
            return new ModulesHostBuilderContext<TModule>(hostBuilderContext,moduleContext.ToBootstrapContext());

        }

        public static IModulesHostBuilderContext ToModuleHostBuilderContext<TModule>(
            this IModuleBootstrapContext<TModule> bootstrapContext, HostBuilderContext hostBuilderContext = null) where TModule : class
        {
            return new ModulesHostBuilderContext<TModule>(hostBuilderContext, bootstrapContext);

        }

        public static IModuleContext ToModuleContext<TModule>(
            this IModuleBootstrapContext<TModule> bootstrapContext, IServiceProvider moduleServices) where TModule : class
        {
            return new ModuleContext<TModule>(
                bootstrapContext.Module, 
                moduleServices, 
                bootstrapContext.ModulesHostServices, 
                bootstrapContext.Advanced.FrameworkServices,
                bootstrapContext);

        }
    }
}
