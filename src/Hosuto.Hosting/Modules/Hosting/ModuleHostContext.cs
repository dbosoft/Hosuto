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
            Advanced = new AdvancedModuleContext(frameworkServices, null);
        }

        public TModule Module { get; }
        public virtual IServiceProvider Services { get; }
        public IAdvancedModuleContext Advanced { get; }
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
        public AdvancedModuleContext(IServiceProvider frameworkServices, object rootContext)
        {
            FrameworkServices = frameworkServices;
            RootContext = rootContext;
        }

        public IServiceProvider FrameworkServices { get; }
        public object RootContext { get; }
    }

}