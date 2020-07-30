namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleStartupHandlerFactory
    {
        IModuleStartupHandler CreateStartupHandler<TModule>(ModuleStartupContext<TModule> startupContext)
            where TModule : IModule;

    }
}