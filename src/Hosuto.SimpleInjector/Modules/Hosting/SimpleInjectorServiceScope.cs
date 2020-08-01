using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class SimpleInjectorServiceScope : IServiceScope
    {
        private readonly Scope _scope;

        public SimpleInjectorServiceScope(Scope scope)
        {
            _scope = scope;
        }

        public void Dispose()
        {
            _scope.Dispose();
        }

        public IServiceProvider ServiceProvider => _scope.Container;
    }
}