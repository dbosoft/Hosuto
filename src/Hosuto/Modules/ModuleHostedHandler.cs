using System;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.HostedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    internal class ModuleHandlerHostService<TServiceHandler> : BackgroundService where TServiceHandler : class, IHostedServiceHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TServiceHandler _serviceHandler;

        public ModuleHandlerHostService(IServiceProvider serviceProvider, TServiceHandler serviceHandler)
        {
            _serviceProvider = serviceProvider;
            _serviceHandler = serviceHandler;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var serviceProvider = _serviceProvider.AsModuleServices();

            using(var scope = serviceProvider.CreateScope())
                return _serviceHandler.Execute(scope.ServiceProvider, stoppingToken);
        }
    }
}