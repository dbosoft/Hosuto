using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Samples.AspNetCore
{
    class Program
    {
        static Task Main(string[] args)
        {
            var builder = ModuleHost.CreateDefaultBuilder();
            builder.UseEnvironment(Environments.Development);

            builder.HostModule<SampleWebModule>();
            builder.UseAspNetCoreWithDefaults((module, webBuilder) =>
            {
            });
            return builder.RunConsoleAsync();
        }
    }
}
