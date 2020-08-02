using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ServicesStartupConfigurer : IModuleServicesConfigurer
    {
        public void ConfigureServices(IModuleHostBuilderContext context, IServiceCollection services)
        {
            if (context.Advanced.RootContext is ISimpleInjectorModuleContext containerContext)
            {
                services.AddSimpleInjector(containerContext.Container);

            }
        }
    }
}