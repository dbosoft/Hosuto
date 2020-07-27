namespace Dbosoft.Hosuto.Modules
{
    public interface IModuleStartupHandlerFactory
    {
        IModuleStartupHandler CreateStartupHandler<TModule>(ModuleStartupContext<TModule> startupContext)
            where TModule : IModule;

    }
}