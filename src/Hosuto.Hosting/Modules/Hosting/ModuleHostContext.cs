using System;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ModuleContext<TModule> : IModuleContext<TModule>, IModuleBootstrapContext<TModule> where TModule : IModule
    {
        public ModuleContext(TModule module, IServiceProvider moduleServices, IServiceProvider serviceProvider, IServiceProvider frameworkServices)
        {
            Services = moduleServices;
            ModuleHostServices = serviceProvider;
            Module = module;
            Advanced = new AdvancedModuleContext(frameworkServices, moduleServices, null);
        }

        public TModule Module { get; }
        public virtual IServiceProvider Services { get; }
        public IAdvancedModuleContext Advanced { get; }
        object IModuleContext.Module => Module;

        public IServiceProvider ModuleHostServices { get; }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class AdvancedModuleContext : IAdvancedModuleContext
    {
        public AdvancedModuleContext(IServiceProvider frameworkServices, IServiceProvider hostServices, object rootContext)
        {
            FrameworkServices = frameworkServices;
            HostServices = hostServices;
            RootContext = rootContext;
        }

        public IServiceProvider FrameworkServices { get; }
        public IServiceProvider HostServices { get; }
        public object RootContext { get; }
    }

}