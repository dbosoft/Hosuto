using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleServicesFilter : IFilter<IModulesHostBuilderContext, IServiceCollection>
    {
    }
}