using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules.Hosting;
using Dbosoft.Hosuto.Samples.Minimal.RazorModule;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Dbosoft.Hosuto.Samples.Minimal.App
{
    public static class Program
    {
        public static async Task Main()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.UseAspNetCoreMinimal(app => app.WebHost.UseUrls("http://127.0.0.1:0"));
            builder.HostModule<RazorGreetModule>();

            using var host = builder.Build();
            await host.StartAsync();

            var moduleHost = host.Services.GetRequiredService<IModuleHost<RazorGreetModule>>();
            var baseUrl = moduleHost.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>().Addresses.First();

            using (var http = new HttpClient())
            {
                await Probe(http, baseUrl + "/", "Razor page");
                await Probe(http, baseUrl + "/css/site.css", "static asset (wwwroot)");
            }

            await host.StopAsync();
        }

        private static async Task Probe(HttpClient http, string url, string what)
        {
            try
            {
                var response = await http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();
                var snippet = body.Replace("\r", " ").Replace("\n", " ").Trim();
                if (snippet.Length > 70) snippet = snippet.Substring(0, 70) + "...";
                Console.WriteLine($"[{what,-22}] {(int)response.StatusCode} {response.StatusCode}  {url}");
                Console.WriteLine($"                         -> {snippet}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{what,-22}] ERROR {url} -> {ex.Message}");
            }
        }
    }
}
