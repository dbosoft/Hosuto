using System;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IContainerConfigurer<TModule> where TModule : IModule
    {
        void ConfigureContainer(TModule module, IServiceProvider serviceProvider, Container container);
    }
}