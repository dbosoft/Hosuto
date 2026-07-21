using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using Xunit;

namespace Hosuto.SimpleInjector.Tests.Modules.Hosting
{
    public class WebApplicationHostTests
    {
        // A web module hosted on the opt-in minimal-API WebApplication inner host: module services +
        // container (ConfigureContainer) + Configure/endpoint all work, and a container-only
        // dependency is resolved by the mapped endpoint.
        [Fact]
        public async Task WebModule_on_minimal_web_application_serves_endpoint()
        {
            using var host = await StartMinimalHost<GreeterWebModule>();
            var body = await Get(host, BaseUrl<GreeterWebModule>(host) + "/greet");
            Assert.Equal("Hello from the module container", body);
            await host.StopAsync();
        }

        // The minimal-API-native endpoint entry point: a module maps endpoints idiomatically via
        // IEndpointConfiguringModule (endpoints.MapGet), no Startup-style UseRouting/UseEndpoints.
        [Fact]
        public async Task WebModule_maps_endpoints_via_IEndpointConfiguringModule()
        {
            using var host = await StartMinimalHost<EndpointModule>();
            var body = await Get(host, BaseUrl<EndpointModule>(host) + "/hello");
            Assert.Equal("mapped via IEndpointConfiguringModule", body);
            await host.StopAsync();
        }

        // Endpoints are also mappable via a conventional MapEndpoints method (no interface),
        // consistent with the other module methods' interface-or-convention model.
        [Fact]
        public async Task WebModule_maps_endpoints_via_convention_method()
        {
            using var host = await StartMinimalHost<ConventionEndpointModule>();
            var body = await Get(host, BaseUrl<ConventionEndpointModule>(host) + "/conv");
            Assert.Equal("mapped via convention", body);
            await host.StopAsync();
        }

#if NET9_0_OR_GREATER
        // On .NET 9+ the minimal host enables ValidateOnBuild in Development, so a container-only
        // dependency fails validation - unless the module opts out via ValidateServiceProvider.
        [Fact]
        public void Minimal_in_development_without_override_fails_validation()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.UseEnvironment("Development");
            builder.UseAspNetCoreMinimal(app => app.WebHost.UseUrls("http://127.0.0.1:0"));
            builder.HostModule<BrokenValidationModule>();

            Assert.ThrowsAny<Exception>(() => builder.Build().Dispose());
        }

        [Fact]
        public void Minimal_ValidateServiceProvider_can_disable_build_validation()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.UseEnvironment("Development");
            builder.UseAspNetCoreMinimal(app => app.WebHost.UseUrls("http://127.0.0.1:0"));
            builder.HostModule<BrokenValidationModule>(
                options => options.ValidateServiceProvider(validateScopes: false, validateOnBuild: false));

            builder.Build().Dispose();
        }
#endif

        private static async Task<IHost> StartMinimalHost<TModule>() where TModule : class
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.UseAspNetCoreMinimal(app => app.WebHost.UseUrls("http://127.0.0.1:0"));
            builder.HostModule<TModule>();

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }

        private static string BaseUrl<TModule>(IHost host) where TModule : class =>
            host.Services.GetRequiredService<IModuleHost<TModule>>().Services
                .GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>().Addresses.First();

        private static async Task<string> Get(IHost host, string url)
        {
            using var http = new HttpClient();
            return await http.GetStringAsync(url);
        }

        public interface IGreeter
        {
            string Greet();
        }

        public sealed class Greeter : IGreeter
        {
            public string Greet() => "Hello from the module container";
        }

        public sealed class GreeterWebModule : WebModule, IContainerConfiguringModule, IApplicationConfiguringModule
        {
            public override string Path { get; } = "";

            private Container _container;

            // ReSharper disable once UnusedMember.Global
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddRouting();
            }

            void IContainerConfiguringModule.ConfigureContainer(IServiceProvider serviceProvider, Container container)
            {
                container.Register<IGreeter, Greeter>(Lifestyle.Singleton);
                _container = container;
            }

            void IApplicationConfiguringModule.Configure(IServiceProvider serviceProvider, IApplicationBuilder app)
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                    endpoints.MapGet("/greet", async context =>
                        await context.Response.WriteAsync(_container.GetInstance<IGreeter>().Greet())));
            }
        }

        public sealed class ConventionEndpointModule : WebModule, IServiceConfiguringModule
        {
            public override string Path { get; } = "";

            void IServiceConfiguringModule.ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services)
            {
                services.AddRouting();
            }

            // conventional MapEndpoints (no IEndpointConfiguringModule) - discovered by reflection.
            // ReSharper disable once UnusedMember.Global
            public void MapEndpoints(IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/conv", () => "mapped via convention");
            }
        }

        public sealed class EndpointModule : WebModule, IServiceConfiguringModule, IEndpointConfiguringModule
        {
            public override string Path { get; } = "";

            void IServiceConfiguringModule.ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services)
            {
                services.AddRouting();
            }

            void IEndpointConfiguringModule.MapEndpoints(IServiceProvider serviceProvider, IEndpointRouteBuilder endpoints)
            {
                endpoints.MapGet("/hello", () => "mapped via IEndpointConfiguringModule");
            }
        }

        public interface IBus
        {
        }

        public sealed class FakeBus : IBus
        {
        }

        public sealed class NeedsBus
        {
            public NeedsBus(IBus bus)
            {
                _ = bus;
            }
        }

        // impl-type singleton whose dependency lives only in the module container.
        public sealed class BrokenValidationModule : WebModule, IServiceConfiguringModule, IContainerConfiguringModule
        {
            public override string Path { get; } = "";

            void IServiceConfiguringModule.ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services)
            {
                services.AddSingleton<NeedsBus>();
            }

            void IContainerConfiguringModule.ConfigureContainer(IServiceProvider serviceProvider, Container container)
            {
                container.RegisterInstance<IBus>(new FakeBus());
            }
        }
    }
}
