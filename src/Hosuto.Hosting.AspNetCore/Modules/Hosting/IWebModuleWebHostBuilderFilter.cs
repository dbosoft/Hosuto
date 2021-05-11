using Microsoft.AspNetCore.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IWebModuleWebHostBuilderFilter : IFilter<IWebModule, IWebHostBuilder>
    {
    }
}

