using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class ConfigureAppStartupConfigurer<TModule> : IConfigureAppStartupConfigurer<TModule> where TModule: IModule
    {

        public void Configure(ModuleStartupContext<TModule> startupContext, IApplicationBuilder app)
        {
            //if (startupContext is ContainerModuleStartupContext<TModule> context)
            //{
            //    app.UseSimpleInjector(context.Container);
            //}
        }
    }
}