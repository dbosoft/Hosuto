using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Samples
{
    class Program
    {
        static Task Main(string[] args)
        {

            var builder = ModuleHost.CreateDefaultBuilder(args);
            
            // you can configure a module host builder like a host builder.
            // All configurations set with ConfigureHostConfiguration will be shared between all modules.
            builder.UseEnvironment(EnvironmentName.Development);

            //here we add a ServiceCollection to build a DI container that is available for all modules. 
            var sc = new ServiceCollection();
            sc.AddSingleton<IMessageDispatcher, MessageDispatcher>();
            builder.UseServiceCollection(sc);

            builder.HostModule<SimpleModule>();
            builder.HostModule<SampleWebModule>();

            //If you host WebModules you have to call UseAspNetCore to initialize the aspdotnetcore runtime
            builder.UseAspNetCore(() => WebHost.CreateDefaultBuilder(args), (module, webBuilder) =>
            {
            });
            var host = builder.Build();

            return host.RunAsync();
        }
    }
}
