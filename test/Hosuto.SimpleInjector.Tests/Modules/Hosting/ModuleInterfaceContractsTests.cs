using System;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.HostedServices;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleInjector;
using Xunit;

namespace Hosuto.SimpleInjector.Tests.Modules.Hosting
{
    public class ModuleInterfaceContractsTests
    {
        // A module authored purely against the opt-in interfaces (no convention methods, no
        // reflection) is configured end-to-end: ConfigureServices + ConfigureContainer run, and the
        // hosted handler resolves the container-registered service.
        [Fact]
        public async Task Interface_based_module_is_configured_and_runs()
        {
            var container = new Container();

            var serviceMock = new Mock<IService>();
            serviceMock.Setup(x => x.CallMe()).Verifiable();

            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(container);
            container.RegisterInstance(serviceMock.Object);
            builder.HostModule<InterfaceModule>();

            var host = builder.Build();
            await host.StartAsync();

            serviceMock.Verify(x => x.CallMe());
            host.Dispose();
        }

        // The IApplicationConfiguringModule.Configure contract is invoked for a web module.
        [Fact]
        public async Task Interface_based_web_module_Configure_is_invoked()
        {
            InterfaceWebModule.Configured = false;

            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.UseAspNetCoreWithDefaults((_, webHostBuilder) =>
                webHostBuilder.UseUrls("http://127.0.0.1:0")); // ephemeral port, no collision
            builder.HostModule<InterfaceWebModule>();

            var host = builder.Build();
            await host.StartAsync();     // Configure runs when the module's request pipeline is built
            await host.StopAsync();
            host.Dispose();

            Assert.True(InterfaceWebModule.Configured);
        }

        public interface IService
        {
            void CallMe();
        }

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

        public class InterfaceModule : IServiceConfiguringModule, IContainerConfiguringModule
        {
            public void ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services)
            {
                services.AddHostedHandler<HostedServiceHandler>();
            }

            public void ConfigureContainer(IServiceProvider serviceProvider, Container container)
            {
                var service = serviceProvider.GetRequiredService<IService>();
                container.RegisterInstance(service);
                container.Register<HostedServiceHandler>();
            }
        }

        public class InterfaceWebModule : WebModule, IApplicationConfiguringModule
        {
            public static bool Configured;

            public override string Path { get; } = "";

            public void Configure(IServiceProvider serviceProvider, IApplicationBuilder app)
            {
                Configured = true;
            }
        }
    }
}
