using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class ServiceProviderFactory : IServiceProviderFactory<Container>
    {
        private readonly Container _container;

        public ServiceProviderFactory(Container container)
        {
            _container = container;
        }

        public Container CreateBuilder(IServiceCollection services)
        {
            foreach (var service in services)
            {
                Lifestyle lifestyle;

                switch (service.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        lifestyle = Lifestyle.Singleton;
                        break;
                    case ServiceLifetime.Scoped:
                        lifestyle = Lifestyle.Scoped;
                        break;
                    case ServiceLifetime.Transient:
                        lifestyle = Lifestyle.Transient;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (service.ImplementationInstance != null)
                    _container.Register(service.ServiceType, () => service.ImplementationInstance, lifestyle);
                else
                {
                    if(service.ImplementationType != null)
                        _container.Register(service.ServiceType, service.ImplementationType, lifestyle);
                    else
                    {
                        _container.Register(service.ServiceType, () => service.ImplementationFactory(_container), lifestyle);

                    }

                }
                    
            }

            return _container;
        }

        public IServiceProvider CreateServiceProvider(Container containerBuilder)
        {
            return _container;
        }
    }
}