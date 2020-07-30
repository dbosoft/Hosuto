//using System;
//using System.Threading.Tasks;
//using Dbosoft.Hosuto.HostedServices;
//using Dbosoft.Hosuto.Modules;
//using Dbosoft.Hosuto.Modules.Hosting;
//using Microsoft.AspNetCore;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.TestHost;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using SimpleInjector;
//using Xunit;

//namespace Hosuto.Hosting.Tests
//{
//    public class SimpleInjectorTests
//    {
//        [Fact]
//        public void Test1()
//        {
//            var container = new Container();
//            var builder = ModuleHost.CreateDefaultBuilder();
//            builder.UseSimpleInjector(container);
//            builder.HostModule<SimpleInjectorModule>();
//            builder.HostModule<SimpleInjectorWebModule>();
//            builder.UseAspNetCore(WebHost.CreateDefaultBuilder, (m, webBuilder) =>
//            {
//                //webBuilder.UseTestServer();
//            });
//            builder.UseEnvironment(EnvironmentName.Development); 
//            builder.Build().Start();
//        }
//    }

//    public class SimpleInjectorModule : IModule
//    {
//        public string Name => "I'm a module";

//        public SimpleInjectorModule()
//        {

//        }

//        public void ConfigureServices(IServiceCollection services, Container container)
//        {
//            services.AddHostedHandler((c, stopToken) =>
//            {
//                var handler = c.GetInstance<IMessageHandler>();
//                handler.HandleMessage("Peng");
//                return Task.CompletedTask;
//            });
//        }

//        public void ConfigureContainer(Container container)
//        {
//            container.RegisterSingleton<IMessageHandler, MessageHandler>();
//        }

//    }

//    public interface IMessageHandler
//    {
//        void HandleMessage(string message);
//    }

//    class MessageHandler : IMessageHandler
//    {
//        public void HandleMessage(string message)
//        {
            
//        }
//    }

//    public class SimpleInjectorWebModule : WebModule
//    {
//        public override string Name => "I'm a module";
//        public override string Path => "path";

//        public SimpleInjectorWebModule()
//        {

//        }

//        public void ConfigureServices(IServiceProvider serviceProvider, IServiceCollection collection, IConfiguration configuration)
//        {

//        }

//        public void Configure(IApplicationBuilder app, IConfiguration configuration, Container container)
//        {
//            //app.UseRouting();
//            //app.UseEndpoints(route =>
//            //{
//            //    route.MapGet("/", context => context.Response.WriteAsync("Hello world"));
//            //});
//        }

//    }


//}