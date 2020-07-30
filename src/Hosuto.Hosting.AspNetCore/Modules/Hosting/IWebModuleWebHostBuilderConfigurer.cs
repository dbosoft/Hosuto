using Microsoft.AspNetCore.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IWebModuleWebHostBuilderConfigurer
    {
        void ConfigureWebHost(WebModule module, IWebHostBuilder builder);
    }
}

