using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Modules
{
    public class ServiceCollectionServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        private readonly IServiceCollection _services;

        public ServiceCollectionServiceProviderFactory(IServiceCollection services)
        {
            _services = services;
        }

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            foreach (var service in services)
            {
                _services.Add(service);
            }

            return _services;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return containerBuilder.BuildServiceProvider(true);
        }

    
    }
}