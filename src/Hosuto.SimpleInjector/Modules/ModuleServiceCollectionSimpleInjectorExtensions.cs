using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Dbosoft.Hosuto.Modules
{
    public static class ModuleServiceCollectionSimpleInjectorExtensions
    {


        public static IServiceCollection AddScopedModuleHandler<TModuleHandler>(
            this IServiceCollection services)
            where TModuleHandler : class, IHostedServiceHandler
        {
            return services.AddSingleton<IHostedService, ScopedModuleHandler<TModuleHandler>>();
        }
    }
}