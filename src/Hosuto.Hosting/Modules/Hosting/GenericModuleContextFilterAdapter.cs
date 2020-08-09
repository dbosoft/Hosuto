using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Modules.Hosting
{



    public class GenericModuleContextFilterAdapter<TFilter, TModule, T1> : IFilter<IModuleContext, T1> 
        where TModule : IModule 
        where TFilter : IFilter<IModuleContext<TModule>, T1>
    {
        public Action<IModuleContext, T1> Invoke(Action<IModuleContext, T1> next) =>
            (context, p2) =>
            {
                if (context is IModuleContext<TModule> genericContext)
                {
                    var services =
                        genericContext.Advanced.FrameworkServices.GetServices<TFilter>();

                    Filters.BuildFilterPipeline
                        (services.Cast<IFilter<IModuleContext<TModule>, T1>>(),
                            (ctx, c) => { next(ctx, p2); })
                        (genericContext, p2);
                }
                else
                {
                    next(context, p2);
                }
            };
    }

    public class GenericModuleContextFilterAdapter<T1> : IFilter<IModuleContext, T1>
    {
        private readonly Type _filterType;

        private GenericModuleContextFilterAdapter(Type filterType)
        {
            _filterType = filterType;
        }

        public static IFilter<IModuleContext, T1> Create(Type filterType)
        {
            return new GenericModuleContextFilterAdapter<T1>(filterType);
        }

        public Action<IModuleContext, T1> Invoke(Action<IModuleContext, T1> next) =>
            (context, p2) =>
            {
                var moduleType = context.Module.GetType();

                var genericFilterType = _filterType.MakeGenericType(moduleType);
                var moduleAdapter =
                    typeof(GenericModuleContextFilterAdapter<,,>).MakeGenericType(genericFilterType, moduleType, typeof(T1));

                var filter = Activator.CreateInstance(moduleAdapter) as IFilter<IModuleContext, T1>;

                filter?.Invoke(next)(context, p2);
            };
    }
}