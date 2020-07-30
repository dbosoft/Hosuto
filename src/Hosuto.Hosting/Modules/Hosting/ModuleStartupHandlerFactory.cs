namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class DefaultModuleStartupHandlerFactory : IModuleStartupHandlerFactory
    {

        public IModuleStartupHandler CreateStartupHandler<TModule>(ModuleStartupContext<TModule> startupContext) where TModule : IModule
        {
            return new DefaultModuleStartupHandler<TModule>(startupContext);
        }

    }
}