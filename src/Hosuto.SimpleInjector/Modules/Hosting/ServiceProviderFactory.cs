using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class ServiceProviderFactory : IModuleHostServiceProviderFactory
    {
        private readonly Container _container;

        public ServiceProviderFactory(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public object ConfigureServices(IServiceCollection services)
        {
            services.AddSimpleInjector(_container);
            return _container;
        }

        public IServiceProvider ReplaceServiceProvider(object state, IServiceProvider services)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (services == null) throw new ArgumentNullException(nameof(services));

            Debug.Assert(ReferenceEquals(state, _container));
            services.UseSimpleInjector(_container);

            return _container;
        }

        public void ConfigureModule(Type moduleType, Func<IServiceProvider, object> moduleFactory)
        {

            switch (moduleFactory)
            {
                case null:
                    _container.Register(moduleType);
                    break;
                default:
                    _container.Register(moduleType, () => moduleFactory(_container) );
                    break;
            }
        }
    }
    
}