using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dbosoft.Hosuto.Modules
{
    /// <summary>
    /// Opt-in, statically typed alternative to a module's conventional <c>ConfigureServices</c>
    /// method. When a module implements this interface the framework invokes it directly instead of
    /// locating the method via reflection - which is trimming/AOT friendly and checked at compile
    /// time. Modules that do not implement it keep working through the reflection convention.
    /// </summary>
    public interface IServiceConfiguringModule
    {
        /// <param name="serviceProvider">The modules host service provider (same instance the
        /// conventional method receives as its first <see cref="IServiceProvider"/> parameter).</param>
        /// <param name="services">The module host's service collection.</param>
        void ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services);
    }
}
