using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Dbosoft.Hosuto.Modules.Hosting
{
    public interface IModuleHostingOptions
    {
        IDictionary<string,object> Properties { get; }
        IModuleHostingOptions Configure(Action<IServiceProvider> configureAction);
        IModuleHostingOptions Configure(Action<IHostBuilder> configureAction);

        IModuleHostingOptions Configure(Action<IModuleContext> configureAction);

        IModuleHostingOptions Configure<TContext>(Action<TContext> configureAction)
            where TContext : class, IModuleContext;

        IModuleHostingOptions ModuleFactoryCallback(Func<IServiceProvider, object> moduleFactory);

        /// <summary>
        /// Explicitly controls how the module host's service provider is validated when it is built.
        /// </summary>
        /// <remarks>
        /// When this is not called the module host follows the underlying host defaults. Note that
        /// since .NET 9 the ASP.NET Core host enables both scope and build-time validation in the
        /// Development environment. Because Hosuto configures the module container
        /// (SimpleInjector/Autofac) only after the host has been built, build-time validation can
        /// fail for services whose dependencies live in that container. Use this to override that
        /// behaviour - e.g. <c>ValidateServiceProvider(validateScopes: true, validateOnBuild: false)</c>.
        /// Both flags are applied together. This call has no effect at all on hosts built against
        /// Microsoft.Extensions.DependencyInjection older than 3.0 (the netstandard2.0 build and
        /// classic ASP.NET Core &lt; 3.0), where the underlying <c>UseDefaultServiceProvider</c> /
        /// <c>ValidateOnBuild</c> APIs are unavailable.
        /// </remarks>
        /// <param name="validateScopes">Whether scoped services are validated (never resolved from the root provider).</param>
        /// <param name="validateOnBuild">Whether all service descriptors are validated when the provider is built.</param>
        IModuleHostingOptions ValidateServiceProvider(bool validateScopes, bool validateOnBuild);
    }
}
