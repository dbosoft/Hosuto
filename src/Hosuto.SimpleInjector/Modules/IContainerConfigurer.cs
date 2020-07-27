using System;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules
{
    public interface IContainerConfigurer<TModule> where TModule : IModule
    {
        void ConfigureContainer(TModule module, IServiceProvider serviceProvider, Container container);
    }
}