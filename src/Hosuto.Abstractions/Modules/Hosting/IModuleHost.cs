using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{

    public interface IModuleHost<TModule> : IHost where TModule : IModule
    {
        IModuleContext<TModule> ModuleContext { get;  }
    } 
}