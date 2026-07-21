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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using Xunit;

namespace Hosuto.SimpleInjector.Tests.Modules.Hosting
{
    public class WebApplicationHostTests
    {
        // A web module hosted on the opt-in minimal-API WebApplication inner host: module services +
        // container (ConfigureContainer) + endpoint (Configure) all work end-to-end, and a
        // container-only dependency is resolved by the mapped endpoint.
        [Fact]
        public async Task WebModule_on_minimal_web_application_serves_endpoint()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.UseAspNetCoreMinimal(app => app.WebHost.UseUrls("http://127.0.0.1:0")); // ephemeral port
            builder.HostModule<GreeterWebModule>();

            using var host = builder.Build();
            await host.StartAsync();

            // discover the actual bound address of the module's inner web host
            var moduleHost = host.Services.GetRequiredService<IModuleHost<GreeterWebModule>>();
            var baseUrl = moduleHost.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>().Addresses.First();

            using (var http = new HttpClient())
            {
                var body = await http.GetStringAsync(baseUrl + "/greet");
                Assert.Equal("Hello from the module container", body);
            }

            await host.StopAsync();
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
    }
}
