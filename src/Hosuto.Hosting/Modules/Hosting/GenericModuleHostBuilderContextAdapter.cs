using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Modules.Hosting
{



    public class GenericModuleHostBuilderContextAdapter<TFilter, TModule, T1> : IFilter<IModulesHostBuilderContext, T1> 
        where TModule : class
        where TFilter : IFilter<IModulesHostBuilderContext<TModule>, T1>
    {
        public Action<IModulesHostBuilderContext, T1> Invoke(Action<IModulesHostBuilderContext, T1> next) =>
            (context, p2) =>
            {
                if (context is IModulesHostBuilderContext<TModule> genericContext)
                {
                    var services =
                        genericContext.Advanced.FrameworkServices.GetServices<TFilter>();

                    Filters.BuildFilterPipeline
                        (services.Cast<IFilter<IModulesHostBuilderContext<TModule>, T1>>(),
                            (ctx, c) => { next(ctx, p2); })
                        (genericContext, p2);
                }
                else
                {
                    next(context, p2);
                }
            };
    }

    public class GenericModuleHostBuilderContextAdapter<T1> : IFilter<IModulesHostBuilderContext, T1>
    {
        private readonly Type _filterType;

        private GenericModuleHostBuilderContextAdapter(Type filterType)
        {
            _filterType = filterType;
        }

        public static IFilter<IModulesHostBuilderContext, T1> Create(Type filterType)
        {
            return new GenericModuleHostBuilderContextAdapter<T1>(filterType);
        }

        public Action<IModulesHostBuilderContext, T1> Invoke(Action<IModulesHostBuilderContext, T1> next) =>
            (context, p2) =>
            {
                var moduleType = context.Module.GetType();

                var genericFilterType = _filterType.MakeGenericType(moduleType);
                var moduleAdapter =
                    typeof(GenericModuleHostBuilderContextAdapter<,,>).MakeGenericType(genericFilterType, moduleType, typeof(T1));

                var filter = Activator.CreateInstance(moduleAdapter) as IFilter<IModulesHostBuilderContext, T1>;

                filter?.Invoke(next)(context, p2);
            };
    }
}