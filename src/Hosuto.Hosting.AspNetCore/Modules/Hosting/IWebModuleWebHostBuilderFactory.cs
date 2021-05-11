namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IWebModuleWebHostBuilderFactory
    {
        Microsoft.AspNetCore.Hosting.IWebHostBuilder CreateWebHost(IWebModule module);
    }
}