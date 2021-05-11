using System.Collections.Generic;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IWebModuleWebHostBuilderInitializer
    {
        void ConfigureWebHost(IWebModule module, Microsoft.Extensions.Hosting.IHostBuilder builder, IEnumerable<IWebModuleWebHostBuilderFilter> filters);
    }
}