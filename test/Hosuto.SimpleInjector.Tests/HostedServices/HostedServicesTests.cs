using System;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.HostedServices;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleInjector;
using Xunit;

namespace Hosuto.SimpleInjector.Tests.HostedServices
{
    public class HostedServicesTests
    {
        [Fact]
        public async Task ServiceHandler_in_Module_uses_SimpleInjector()
        {
            var container = new Container();

            var serviceMock = new Mock<IService>();
            serviceMock.Setup(x => x.CallMe()).Verifiable();


            var builder = ModulesHost.CreateDefaultBuilder();
            
            builder.UseSimpleInjector(container);
            container.RegisterInstance(serviceMock.Object);
            builder.HostModule<SomeModule>();

            var host = builder.Build();
            await host.StartAsync().ConfigureAwait(false);
            //await Task.Delay(2000).ConfigureAwait(false);

            serviceMock.Verify(x => x.CallMe());
            host.Dispose();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public class HostedServiceHandler : IHostedServiceHandler
        {
            private readonly IService _service;

            public HostedServiceHandler(IService service)
            {
                _service = service;
            }

            public Task Execute(CancellationToken stoppingToken)
            {

                _service.CallMe();
                return Task.CompletedTask;
            }
        }

        public interface IService
        {
            void CallMe();
        }

        public class SomeModule : IModule
        {
            public string Name => "I'm a module";

            public void ConfigureServices( IServiceCollection services)
            {
                services.AddHostedHandler<HostedServiceHandler>();
            }

            public void ConfigureContainer(IServiceProvider sp, Container container)
            {
                var service = sp.GetRequiredService<IService>();
                container.RegisterInstance(service);

                container.Register<HostedServiceHandler>();
            }

        }
    }

    
}
