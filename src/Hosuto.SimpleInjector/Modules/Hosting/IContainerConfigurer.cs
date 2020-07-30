using System;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IContainerConfigurer<TModule> where TModule : IModule
    {
        void ConfigureContainer(TModule module, Container container, IServiceProvider outerServiceProvider, IServiceProvider moduleServices);
    }
}