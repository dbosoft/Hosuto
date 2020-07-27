#if NETCOREAPP

using System.Collections.Generic;
using Dbosoft.Hosuto.Modules;
using Hosuto.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Hosuto.AspNetCore
{
    internal class WebModuleWebHostBuilderInitializerWithDefaults : IWebModuleWebHostBuilderInitializer
    {
        public void ConfigureWebHost(IWebModule module, IHostBuilder builder, IEnumerable<IWebModuleWebHostBuilderConfigurer> configurers)
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
        public void ConfigureWebHost(IWebModule module, IHostBuilder builder, IEnumerable<IWebModuleWebHostBuilderConfigurer> configurers)
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