#if NETCOREAPP

using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class WebModuleWebHostBuilderInitializerWithDefaults : IWebModuleWebHostBuilderInitializer
    {
        public void ConfigureWebHost(WebModule module, IHostBuilder builder, IEnumerable<IWebModuleWebHostBuilderConfigurer> configurers)
        {
            builder.ConfigureWebHostDefaults(webHostBuilder =>
            {
                foreach (var configurer in configurers)
                {
                    configurer.ConfigureWebHost(module, webHostBuilder);                        
                }
            });
        }
    }

    internal class WebModuleWebHostBuilderInitializer : IWebModuleWebHostBuilderInitializer
    {
        public void ConfigureWebHost(WebModule module, IHostBuilder builder, IEnumerable<IWebModuleWebHostBuilderConfigurer> configurers)
        {
            builder.ConfigureWebHost(webHostBuilder =>
            {
                foreach (var configurer in configurers)
                {
                    configurer.ConfigureWebHost(module, webHostBuilder);
                }
            });
        }
    }
}

#endif