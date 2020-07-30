using System;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.HostedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    internal class ModuleHostedHandler<TModuleHandler> : BackgroundService where TModuleHandler : class, IHostedServiceHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public ModuleHostedHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _serviceProvider.GetService<TModuleHandler>().Execute(stoppingToken);
        }
    }
}