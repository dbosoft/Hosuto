using System;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class ModuleContextWithContainer<TModule> : ModuleContext<TModule>, ISimpleInjectorModuleContext where TModule : IModule
    {
        public Container Container { get; }
        

        public ModuleContextWithContainer(Container container, TModule module, IServiceProvider moduleServices, IServiceProvider moduleHostServices, IServiceProvider frameworkServices) 
            : base(module, moduleServices, moduleHostServices, frameworkServices)
        {
            Container = container;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Container?.Dispose();
        }

        public override IServiceProvider Services => Container;
    }
}