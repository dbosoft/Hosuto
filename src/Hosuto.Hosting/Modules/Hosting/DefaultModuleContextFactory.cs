using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class DefaultModuleContextFactory<TModule> : IModuleContextFactory<TModule> where TModule : IModule
    {


        public IModuleContext<TModule> CreateModuleContext(IModuleBootstrapContext<TModule> bootstrapContext, IServiceProvider moduleServices)
        {
            return CreateModuleContextInternal(bootstrapContext.Module, 
                moduleServices, 
                bootstrapContext.ModulesHostServices, 
                bootstrapContext.Advanced.FrameworkServices);
        }

        public IModuleBootstrapContext<TModule> CreateModuleBootstrapContext(TModule module, IServiceProvider moduleHostServices,
            IServiceProvider frameworkServices)
        {
            return CreateModuleContextInternal(module, null, moduleHostServices, frameworkServices);
        }

        private ModuleContext<TModule> CreateModuleContextInternal(TModule module, IServiceProvider moduleServices, IServiceProvider moduleHostServices,
            IServiceProvider frameworkServices)
        {
            return new ModuleContext<TModule>(module, moduleServices, moduleHostServices, frameworkServices);
        }
    }
}