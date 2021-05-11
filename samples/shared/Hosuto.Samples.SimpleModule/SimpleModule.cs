using System;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.HostedServices;
using Dbosoft.Hosuto.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;

namespace Dbosoft.Hosuto.Samples
{
    public class SimpleModule : IMessageRecipient
    {
        public void ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services)
        {
            //services.AddSingleton(serviceProvider.GetRequiredService<IMessageDispatcher>());

            services.AddHostedHandler((sp, cancelToken) =>
            {
                var dispatcher = sp.GetRequiredService<IMessageDispatcher>();
                dispatcher.RegisterRecipient(this);
                return Task.CompletedTask;
            });

            services.AddHostedHandler<HostedServiceHandler>();


        }

        public void ConfigureContainer(IServiceProvider serviceProvider, Container container)
        {
            container.RegisterInstance(serviceProvider.GetRequiredService<IMessageDispatcher>());
            container.Register<HostedServiceHandler>(Lifestyle.Scoped);
        }

        public string Name => "simple";
        public void ProcessMessage(object sender, string message)
        {
            Console.WriteLine($"Simple Module has received message '{message}' from '{sender.GetType()}'");
        }
    }

    public class HostedServiceHandler : IHostedServiceHandler, IMessageRecipient
    {
        private readonly IMessageDispatcher _dispatcher;

        public HostedServiceHandler(IMessageDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public Task Execute(CancellationToken stoppingToken)
        {
            _dispatcher.RegisterRecipient(this);
            return Task.CompletedTask;
        }

        public void ProcessMessage(object sender, string message)
        {
            Console.WriteLine($"HostedServiceHandler has received message '{message}' from '{sender.GetType()}'");

        }
    }
}
