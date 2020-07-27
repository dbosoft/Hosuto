using Dbosoft.Hosuto.Modules;

namespace Hosuto.AspNetCore.Hosting
{
    public class WebModuleStartupFactory : IModuleStartupHandlerFactory
    {
        public IModuleStartupHandler CreateStartupHandler<TModule>(
            ModuleStartupContext<TModule> startupContext) where TModule : IModule
        {
            return typeof(IWebModule).IsAssignableFrom(typeof(TModule))
                ? new WebModuleStartupHandler<TModule>(startupContext)
                : null;
        }
    }
}
