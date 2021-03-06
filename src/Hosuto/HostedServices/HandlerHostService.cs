﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.HostedServices
{
    internal class HandlerHostService<TServiceHandler> : BackgroundService where TServiceHandler : class, IHostedServiceHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public HandlerHostService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var services = _serviceProvider.AsModuleServices();
            using (var scope = services.CreateScope())
            {
                var serviceHandler = scope.ServiceProvider.GetRequiredService<TServiceHandler>();
                return serviceHandler.Execute(stoppingToken);
            }
        }
    }
}