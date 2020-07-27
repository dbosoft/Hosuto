using Dbosoft.Hosuto.Modules.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    public class DefaultModuleStartupHandlerFactory : IModuleStartupHandlerFactory
    {

        public IModuleStartupHandler CreateStartupHandler<TModule>(ModuleStartupContext<TModule> startupContext) where TModule : IModule
        {
            return new DefaultModuleStartupHandler<TModule>(startupContext);
        }

    }
}