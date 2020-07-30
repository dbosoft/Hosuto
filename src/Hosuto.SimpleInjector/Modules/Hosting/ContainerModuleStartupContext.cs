using System;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ContainerModuleStartupContext<TModule> : ModuleStartupContext<TModule> where TModule : IModule
    {
        public Container Container { get; } = new Container();

        public ContainerModuleStartupContext(TModule module, ModuleHostBuilderSettings builderSettings, IServiceProvider serviceProvider) : base(module, builderSettings, serviceProvider)
        {
            Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Container?.Dispose();
        }
    }
}