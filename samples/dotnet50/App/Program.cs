using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules.Hosting;
using Dbosoft.Hosuto.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Sample
{
    class Program
    {
        static Task Main(string[] args)
        {
            //this will create a ModulesHost - that is a host that hosts modules ;-)
            var builder = ModulesHost.CreateDefaultBuilder(args);

            // you can configure a module host builder like a host builder.
            // All configurations set with ConfigureAppConfiguration will be shared between all modules and the ModulesHost.

            //optional: here we add a ServiceCollection to build a DI container that is available for all modules. 
            var sc = new ServiceCollection();
            sc.AddSingleton<IMessageDispatcher, MessageDispatcher>();

            builder.UseServiceCollection(sc);

            builder.HostModule<SampleWebModule>();

            //to host WebModules you have to call this once. Use it to configure
            //web host settings like ports. 
            builder.UseAspNetCoreWithDefaults((module, webBuilder) =>
            {
            });

            var host = builder.Build();

            var test = host.Services.GetService<IMessageDispatcher>();
            var module = host.Services.GetService<SampleWebModule>();
            var moduleHost = host.Services.GetService<IModuleHost<SampleWebModule>>();

            return host.RunAsync();

        }
    }
}
