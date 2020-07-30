using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ServicesStartupConfigurer<TModule> : IServicesStartupConfigurer<TModule> where TModule: IModule
    {
        public void ConfigureServices(ModuleStartupContext<TModule> startupContext, IServiceCollection services)
        {
            if (startupContext is ContainerModuleStartupContext<TModule> context)
            {
                services.AddSimpleInjector(context.Container);
            }
        }
    }
}