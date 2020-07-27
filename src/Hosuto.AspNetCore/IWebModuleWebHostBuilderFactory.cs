using Dbosoft.Hosuto.Modules;

namespace Hosuto.AspNetCore
{
    public interface IWebModuleWebHostBuilderFactory
    {
        Microsoft.AspNetCore.Hosting.IWebHostBuilder CreateWebHost(IWebModule module);
    }
}