using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class SimpleInjectorScopeFactory : IServiceScopeFactory
    {
        private readonly Container _container;

        public SimpleInjectorScopeFactory(Container container)
        {
            _container = container;
        }

        public IServiceScope CreateScope()
        {
            return new SimpleInjectorServiceScope(AsyncScopedLifestyle.BeginScope(_container));
            
        }
    }
}