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
    /// <remarks>
    /// If a module implements this interface, the framework calls it and ignores any conventional
    /// <c>ConfigureServices</c> method. The <paramref name="serviceProvider"/> matches what the
    /// convention injects as a method's <em>first</em> <see cref="IServiceProvider"/> parameter; a
    /// conventional method that declared <see cref="IServiceProvider"/> in a later position would
    /// receive a different provider.
    /// </remarks>
    public interface IServiceConfiguringModule
    {
        /// <param name="serviceProvider">The modules host's service provider.</param>
        /// <param name="services">The module host's service collection.</param>
        void ConfigureServices(IServiceProvider serviceProvider, IServiceCollection services);
    }
}
