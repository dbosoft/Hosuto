using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Samples.AspNetCore.WebModule;
using Hosuto.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;

namespace App
{
    class Program
    {
        static Task Main(string[] args)
        {
            var builder = ModuleHost.CreateDefaultBuilder();
            builder.UseEnvironment(EnvironmentName.Development);

            builder.HostModule<SampleWebModule>();
            builder.UseAspNetCore(() => WebHost.CreateDefaultBuilder(args), (module, webBuilder) =>
            {
            });
            return builder.RunConsoleAsync();
        }
    }
}
