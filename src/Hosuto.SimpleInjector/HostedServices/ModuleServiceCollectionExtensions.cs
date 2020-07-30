using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

namespace Dbosoft.Hosuto.HostedServices
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHostedHandler(
            this IServiceCollection services, 
            Func<Container, CancellationToken, Task> handlerDelegate)
        {
            return services.AddHostedHandler((sp, cancelToken) =>
            {
                var container = sp.GetService<Container>();
                return handlerDelegate(container, cancelToken);
            });
        }

    }
}