#if NETCOREAPP
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    internal class WebModuleWebHostBuilderInitializer : IWebModuleWebHostBuilderInitializer
    {
        public void ConfigureWebHost(WebModule module, IHostBuilder builder, IEnumerable<IWebModuleWebHostBuilderFilter> filters)
        {
            builder.ConfigureWebHost(webHostBuilder =>
            {
                Filters.BuildFilterPipeline(filters, (_, __) => { })(module, webHostBuilder);

            });
        }
    }
}
#endif