using Dbosoft.Hosuto.Modules;
using Microsoft.AspNetCore.Hosting;

namespace Hosuto.AspNetCore.Hosting
{
    public interface IWebModuleWebHostBuilderConfigurer
    {
        void ConfigureWebHost(IWebModule module, IWebHostBuilder builder);
    }
}

