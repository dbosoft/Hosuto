using System.Collections.Generic;
using Dbosoft.Hosuto.Modules;
using Hosuto.AspNetCore.Hosting;

namespace Hosuto.AspNetCore
{
    public interface IWebModuleWebHostBuilderInitializer
    {
        void ConfigureWebHost(IWebModule module, Microsoft.Extensions.Hosting.IHostBuilder builder, IEnumerable<IWebModuleWebHostBuilderConfigurer> configurers);
    }
}