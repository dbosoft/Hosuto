using System;
using Microsoft.AspNetCore.Builder;

namespace Dbosoft.Hosuto.Modules
{
    /// <summary>
    /// Opt-in, statically typed alternative to a web module's conventional <c>Configure</c> method.
    /// When a module implements this interface the framework invokes it directly instead of locating
    /// the method via reflection. Modules that do not implement it keep working through the
    /// reflection convention.
    /// </summary>
    public interface IApplicationConfiguringModule
    {
        /// <param name="serviceProvider">The modules host service provider (same instance the
        /// conventional method receives as its first <see cref="IServiceProvider"/> parameter).</param>
        /// <param name="app">The module's application pipeline builder.</param>
        void Configure(IServiceProvider serviceProvider, IApplicationBuilder app);
    }
}
