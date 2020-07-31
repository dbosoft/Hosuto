using System;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IContainerConfigurer<TModule> where TModule : IModule
    {
        void ConfigureContainer(IModuleContext<TModule> module, Container container);
    }
}