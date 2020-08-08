using System;
using Microsoft.AspNetCore.Builder;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IWebModuleConfigureFilter : IFilter<IModuleContext, IApplicationBuilder>
    { }
}