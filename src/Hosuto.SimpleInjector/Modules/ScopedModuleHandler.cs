﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Dbosoft.Hosuto.Modules
{
    internal class ScopedModuleHandler<TModuleHandler> : BackgroundService where TModuleHandler : class, IHostedServiceHandler
    {
        private readonly Container _container;

        public ScopedModuleHandler(Container container)
        {
            _container = container;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = AsyncScopedLifestyle.BeginScope(_container))
            {
                return scope.GetInstance<TModuleHandler>().Execute(stoppingToken);
            }
        }

    }
}