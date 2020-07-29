using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules;
using Hosuto.AspNetCore;
using Microsoft.Extensions.Hosting;

namespace App
{
    class Program
    {
        static Task Main(string[] args)
        {
            var builder = ModuleHost.CreateDefaultBuilder();
            builder.UseEnvironment(Environments.Development);

            builder.HostModule<SampleWebModule.SampleWebModule>();
            builder.UseAspNetCoreWithDefaults((module, webBuilder) =>
            {
            });
            return builder.RunConsoleAsync();
        }
    }
}
