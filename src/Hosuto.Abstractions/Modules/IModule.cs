
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules
{
    /// <summary>
    /// This interface marks a class as Hosuto module.
    /// The module follows the same (at least for common scenarios) conventions as a aspnetcore Startup class.
    /// The hosuto framework will search for a method ConfigureServices with a argument of type <see cref="IServiceCollection"/>
    /// to configure the module' s host container. If this method has a argument of type <see cref="IServiceProvider"/> as
    /// first argument, it will be container of the modules host (also called shared container).
    /// Further arguments will be filled from a temporary container with all instances registered before ConfigureServices is called.
    /// This should be used only for global singletons like <see cref="IConfiguration"/> and <see cref="IHostingEnvironment"/>. 
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Name of the module. It is recommended to use the same name for the module folder,
        /// as the <see cref="IHostingEnvironment.ApplicationName"/> of the host will be set by
        /// the module name. 
        /// </summary>
        string Name { get; }


    }
}
