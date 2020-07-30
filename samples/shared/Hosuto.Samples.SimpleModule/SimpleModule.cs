using System;
using System.Threading.Tasks;
using Dbosoft.Hosuto.HostedServices;
using Dbosoft.Hosuto.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Samples
{
    public class SimpleModule : IModule, IMessageRecipient
    {
        public void ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services)
        {
            services.AddSingleton(serviceProvider.GetRequiredService<IMessageDispatcher>());
            services.AddHostedHandler((sp, cancelToken) =>
            {
                var dispatcher = sp.GetRequiredService<IMessageDispatcher>();
                dispatcher.RegisterRecipient(this);
                return Task.CompletedTask;
            });
        }


        public string Name => "simple";
        public void ProcessMessage(object sender, string message)
        {
            Console.WriteLine($"Simple Module has received message '{message}' from '{sender.GetType()}'");
        }
    }
}
