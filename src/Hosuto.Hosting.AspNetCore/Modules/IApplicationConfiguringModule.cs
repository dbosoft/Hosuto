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
    /// <remarks>
    /// If a module implements this interface, the framework calls it and ignores any conventional
    /// <c>Configure</c> method. The <paramref name="serviceProvider"/> matches what the convention
    /// injects as a method's <em>first</em> <see cref="IServiceProvider"/> parameter (the modules
    /// host's service provider); a conventional method that placed <see cref="IServiceProvider"/> in
    /// a later position could receive a different provider.
    /// </remarks>
    public interface IApplicationConfiguringModule
    {
        /// <param name="serviceProvider">The modules host's service provider.</param>
        /// <param name="app">The module's application pipeline builder.</param>
        void Configure(IServiceProvider serviceProvider, IApplicationBuilder app);
    }
}
