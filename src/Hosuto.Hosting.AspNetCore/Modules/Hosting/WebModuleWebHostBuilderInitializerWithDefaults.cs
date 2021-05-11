#if NETCOREAPP

using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class WebModuleWebHostBuilderInitializerWithDefaults : IWebModuleWebHostBuilderInitializer
    {
        public void ConfigureWebHost(IWebModule module, IHostBuilder builder, IEnumerable<IWebModuleWebHostBuilderFilter> filters)
        {
            builder.ConfigureWebHostDefaults(webHostBuilder =>
            {
                Filters.BuildFilterPipeline(filters, (_, __) => { })(module, webHostBuilder);
            });
        }
    }
}

#endif