namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class WebModuleStartupFactory : IModuleStartupHandlerFactory
    {
        public IModuleStartupHandler CreateStartupHandler<TModule>(
            ModuleStartupContext<TModule> startupContext) where TModule : IModule
        {
            return typeof(WebModule).IsAssignableFrom(typeof(TModule))
                ? new WebModuleStartupHandler<TModule>(startupContext)
                : null;
        }
    }
}
