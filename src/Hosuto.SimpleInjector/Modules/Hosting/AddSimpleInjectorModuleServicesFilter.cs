using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class AddSimpleInjectorModuleServicesFilter : IModuleServicesFilter
    {
        public Action<IModulesHostBuilderContext, IServiceCollection> Invoke(Action<IModulesHostBuilderContext, IServiceCollection> next) =>
            (ctx, services) =>
            {
                if (ctx.Advanced.RootContext is ISimpleInjectorModuleContext containerContext)
                {
                    services.AddSimpleInjector(containerContext.Container);
                }

                next(ctx, services);
            };
    }
}