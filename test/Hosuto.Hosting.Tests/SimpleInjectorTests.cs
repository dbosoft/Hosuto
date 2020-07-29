using System;
using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules;
using Hosuto.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using Xunit;

namespace Hosuto.Hosting.Tests
{
    public class SimpleInjectorTests
    {
        [Fact]
        public void Test1()
        {
            var container = new Container();
            var builder = ModuleHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(container);
            builder.HostModule<SimpleInjectorModule>();
            builder.HostModule<SimpleInjectorWebModule>();
            builder.UseAspNetCoreWithDefaults((module, webBuilder) => { });

            builder.UseEnvironment(Environments.Development); 
            builder.Build().Start();
        }
    }

    public class SimpleInjectorModule : IModule
    {
        public string Name => "I'm a module";

        public SimpleInjectorModule(IHostEnvironment environment)
        {

        }

        public void ConfigureServices(IServiceCollection services, Container container)
        {
            services.AddHostedHandler((sp, stopToken) =>
            {

                return Task.CompletedTask;
            });
        }

    }

    public class SimpleInjectorWebModule : IWebModule
    {
        public string Name => "I'm a module";
        public string Path => "path";

        public SimpleInjectorWebModule(IHostEnvironment environment)
        {

        }

        public void ConfigureServices(IServiceProvider serviceProvider, IServiceCollection collection, IConfiguration configuration)
        {

        }

        public void Configure(IServiceProvider serviceProvider, IApplicationBuilder app, IConfiguration configuration)
        {
            //app.UseRouting();
            //app.UseEndpoints(route =>
            //{
            //    route.MapGet("/", context => context.Response.WriteAsync("Hello world"));
            //});
        }

    }


}