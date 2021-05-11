using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{

    public interface IModuleHost<TModule> : IHost where TModule : class
    {
        IModuleContext<TModule> ModuleContext { get;  }
    } 
}