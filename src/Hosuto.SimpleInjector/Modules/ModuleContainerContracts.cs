using System;
using SimpleInjector;
using SimpleInjector.Integration.ServiceCollection;

namespace Dbosoft.Hosuto.Modules
{
    /// <summary>
    /// Opt-in, statically typed alternative to a module's conventional <c>ConfigureContainer</c>
    /// method. Implementing this interface lets the framework invoke it directly instead of via
    /// reflection. If a module implements it, any conventional <c>ConfigureContainer</c> method is
    /// ignored.
    /// </summary>
    public interface IContainerConfiguringModule
    {
        /// <param name="serviceProvider">The modules host's service provider - the same instance the
        /// convention injects as a method's first <see cref="IServiceProvider"/> parameter. This is
        /// distinct from the <paramref name="container"/> being configured.</param>
        /// <param name="container">The module container to configure.</param>
        void ConfigureContainer(IServiceProvider serviceProvider, Container container);
    }

    /// <summary>
    /// Opt-in, statically typed alternative to a module's conventional <c>UseSimpleInjector</c>
    /// method. If a module implements it, any conventional <c>UseSimpleInjector</c> method is ignored.
    /// </summary>
    public interface IUseSimpleInjectorModule
    {
        void UseSimpleInjector(SimpleInjectorUseOptions options);
    }

    /// <summary>
    /// Opt-in, statically typed alternative to a module's conventional <c>AddSimpleInjector</c>
    /// method. If a module implements it, any conventional <c>AddSimpleInjector</c> method is ignored.
    /// </summary>
    public interface IAddSimpleInjectorModule
    {
        void AddSimpleInjector(SimpleInjectorAddOptions options);
    }
}
