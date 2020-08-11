using System;
using System.Linq;
using Dbosoft.Hosuto.Modules.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public class AddSimpleInjectorModuleServicesFilter : IModuleServicesFilter
    {
        public Action<IModulesHostBuilderContext, IServiceCollection> Invoke(Action<IModulesHostBuilderContext, IServiceCollection> next) =>
            (context, services) =>
            {
                next(context, services);

                if (context.Advanced.RootContext is ISimpleInjectorModuleContext containerContext)
                {
                    services.AddSimpleInjector(containerContext.Container, options =>
                    {
                        Filters.BuildFilterPipeline(
                            context.Advanced.FrameworkServices.GetServices<IAddSimpleInjectorFilter>()
                                .Append(GenericModuleHostBuilderContextAdapter<SimpleInjectorAddOptions>.Create(typeof(IAddSimpleInjectorFilter<>))),
                            (ctx, o) =>
                            {
                                ModuleMethodInvoker.CallOptionalMethod(containerContext, "AddSimpleInjector", o);

                            })(context, options);

                    });
                }

            };
    }
}