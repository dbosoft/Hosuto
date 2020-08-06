using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting.Internal
{
    public static class ModuleContextExtensions
    {
        public static IModuleBootstrapContext<TModule> ToBootstrapContext<TModule>(
            this IModuleContext<TModule> moduleContext) where TModule : IModule
        {
            return moduleContext.Advanced.FrameworkServices.GetRequiredService<IModuleContextFactory<TModule>>()
                .CreateModuleBootstrapContext(moduleContext.Module, moduleContext.ModuleHostServices,
                    moduleContext.Advanced.FrameworkServices);

        }

        public static IModulesHostBuilderContext ToModuleHostBuilderContext<TModule>(
            this IModuleContext<TModule> moduleContext, HostBuilderContext hostBuilderContext = null) where TModule : IModule
        {
            return new ModulesHostBuilderContext<TModule>(hostBuilderContext,moduleContext.ToBootstrapContext());

        }

        public static IModulesHostBuilderContext ToModuleHostBuilderContext<TModule>(
            this IModuleBootstrapContext<TModule> bootstrapContext, HostBuilderContext hostBuilderContext = null) where TModule : IModule
        {
            return new ModulesHostBuilderContext<TModule>(hostBuilderContext, bootstrapContext);

        }
    }
}
