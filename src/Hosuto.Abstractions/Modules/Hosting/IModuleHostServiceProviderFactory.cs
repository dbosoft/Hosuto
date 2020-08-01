using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleHostServiceProviderFactory
    {
        object ConfigureServices(IServiceCollection services);
        IServiceProvider ReplaceServiceProvider(object state, IServiceProvider services);
    }
}