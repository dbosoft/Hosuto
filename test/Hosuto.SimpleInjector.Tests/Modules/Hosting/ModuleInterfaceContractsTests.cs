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
using SimpleInjector.Integration.ServiceCollection;
using Xunit;

namespace Hosuto.SimpleInjector.Tests.Modules.Hosting
{
    // The module methods are implemented EXPLICITLY on purpose: an explicit interface implementation
    // is not discoverable via Type.GetMethod("<name>"), so the reflection convention in
    // ModuleMethodInvoker cannot find them. If the interface-dispatch branch at any call site were
    // removed or broken, these tests would fail (the reflection fallback would find nothing) - which
    // is what proves the interface path, not the fallback, is exercised.
    public class ModuleInterfaceContractsTests
    {
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

        [Fact]
        public void SimpleInjector_hook_interfaces_are_invoked()
        {
            SimpleInjectorHooksModule.Reset();

            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.HostModule<SimpleInjectorHooksModule>();

            builder.Build().Dispose();

            Assert.True(SimpleInjectorHooksModule.AddCalled, "IAddSimpleInjectorModule.AddSimpleInjector was not invoked");
            Assert.True(SimpleInjectorHooksModule.UseCalled, "IUseSimpleInjectorModule.UseSimpleInjector was not invoked");
            Assert.True(SimpleInjectorHooksModule.ContainerConfigured, "IContainerConfiguringModule.ConfigureContainer was not invoked");
        }

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

        // Explicit implementations -> invisible to Type.GetMethod, so only interface dispatch works.
        public class InterfaceModule : IServiceConfiguringModule, IContainerConfiguringModule
        {
            void IServiceConfiguringModule.ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services)
            {
                services.AddHostedHandler<HostedServiceHandler>();
            }

            void IContainerConfiguringModule.ConfigureContainer(IServiceProvider serviceProvider, Container container)
            {
                var service = serviceProvider.GetRequiredService<IService>();
                container.RegisterInstance(service);
                container.Register<HostedServiceHandler>();
            }
        }

        public class SimpleInjectorHooksModule : IContainerConfiguringModule, IUseSimpleInjectorModule, IAddSimpleInjectorModule
        {
            public static bool ContainerConfigured;
            public static bool UseCalled;
            public static bool AddCalled;

            public static void Reset() => ContainerConfigured = UseCalled = AddCalled = false;

            void IContainerConfiguringModule.ConfigureContainer(IServiceProvider serviceProvider, Container container)
                => ContainerConfigured = true;

            void IUseSimpleInjectorModule.UseSimpleInjector(SimpleInjectorUseOptions options)
                => UseCalled = true;

            void IAddSimpleInjectorModule.AddSimpleInjector(SimpleInjectorAddOptions options)
                => AddCalled = true;
        }

        public class InterfaceWebModule : WebModule, IApplicationConfiguringModule
        {
            public static bool Configured;

            public override string Path { get; } = "";

            void IApplicationConfiguringModule.Configure(IServiceProvider serviceProvider, IApplicationBuilder app)
            {
                Configured = true;
            }
        }
    }
}
