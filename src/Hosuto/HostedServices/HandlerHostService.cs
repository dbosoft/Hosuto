using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.HostedServices
{
    internal class HandlerHostService<TServiceHandler> : BackgroundService where TServiceHandler : class, IHostedServiceHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TServiceHandler _serviceHandler;

        public HandlerHostService(IServiceProvider serviceProvider, TServiceHandler serviceHandler)
        {
            _serviceProvider = serviceProvider;
            _serviceHandler = serviceHandler;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        { 
            return _serviceHandler.Execute(_serviceProvider, stoppingToken);
        }
    }
}