using System;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;

namespace Dbosoft.Hosuto.Modules
{
    /// <summary>
    /// Opt-in, statically typed alternative to a module's conventional <c>ConfigureContainer</c>
    /// method. Implementing this interface lets the framework invoke it directly instead of via
    /// reflection.
    /// </summary>
    public interface IContainerConfiguringModule
    {
        /// <param name="serviceProvider">The module's service provider - in the SimpleInjector
        /// hosting model this is the module <see cref="Container"/> itself, matching the first
        /// <see cref="IServiceProvider"/> parameter of the conventional method.</param>
        /// <param name="container">The module container to configure.</param>
        void ConfigureContainer(IServiceProvider serviceProvider, Container container);
    }

    /// <summary>
    /// Opt-in, statically typed alternative to a module's conventional <c>UseSimpleInjector</c> method.
    /// </summary>
    public interface IUseSimpleInjectorModule
    {
        void UseSimpleInjector(SimpleInjectorUseOptions options);
    }

    /// <summary>
    /// Opt-in, statically typed alternative to a module's conventional <c>AddSimpleInjector</c> method.
    /// </summary>
    public interface IAddSimpleInjectorModule
    {
        void AddSimpleInjector(SimpleInjectorAddOptions options);
    }
}
