using System;
using System.Threading;
using System.Threading.Tasks;
using Dbosoft.Hosuto.HostedServices;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleInjector;
using Xunit;

namespace Hosuto.SimpleInjector.Tests.HostedServices
{
    public class HostedHandlerValidateOnBuildTests
    {
        // Regression test for a web module that registers a hosted handler via AddHostedHandler
        // while the handler's dependency lives in the SimpleInjector container (configured after
        // the inner web host is built). Since .NET 9 the built-in container enables
        // ValidateOnBuild by default in the Development environment; before the fix AddHostedHandler
        // registered the handler by implementation type, so ValidateOnBuild eagerly constructed it
        // at Build() time - before ConfigureContainer had run - and threw:
        //   "Unable to resolve service for type '...IBus' while attempting to activate '...Handler'".
        // ValidateOnBuild is forced on here so the test reproduces the failure on every target
        // framework, mirroring the .NET 9 Development default.
        [Fact]
        public void WebModule_hosted_handler_with_container_dependency_builds()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.UseAspNetCoreWithDefaults((_, webHostBuilder) =>
            {
                webHostBuilder.UseDefaultServiceProvider(options =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                });
            });
            builder.HostModule<WebModuleWithHostedHandler>();

            // Before the fix this threw during Build() while validating the handler descriptor.
            builder.Build().Dispose();
        }

        // These two tests rely on the Development environment enabling ValidateOnBuild by default,
        // which only happens on .NET 9+ (the regression this change addresses). On earlier targets
        // Development does not enable build-time validation, so the premise does not hold there.
#if NET9_0_OR_GREATER
        // Proves the ValidateServiceProvider option turns build-time validation off ...
        // Without the override this module (a container-only dependency) would fail to build in
        // Development (see the sanity test below); the option suppresses it.
        [Fact]
        public void ValidateServiceProvider_can_disable_build_validation()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.UseEnvironment("Development");
            builder.UseAspNetCoreWithDefaults();
            builder.HostModule<WebModuleWithContainerOnlyService>(
                options => options.ValidateServiceProvider(validateScopes: false, validateOnBuild: false));

            builder.Build().Dispose();
        }

        // Sanity guard for the test above: with the same Development environment but no override,
        // the container-only dependency must fail build-time validation (so the test above is meaningful).
        [Fact]
        public void WebModule_in_development_without_override_fails_validation()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.UseEnvironment("Development");
            builder.UseAspNetCoreWithDefaults();
            builder.HostModule<WebModuleWithContainerOnlyService>();

            Assert.ThrowsAny<Exception>(() => builder.Build().Dispose());
        }
#endif

        // ... and on again, independently of the hosting environment.
        [Fact]
        public void ValidateServiceProvider_can_enable_build_validation()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.UseAspNetCoreWithDefaults();
            builder.HostModule<WebModuleWithContainerOnlyService>(
                options => options.ValidateServiceProvider(validateScopes: false, validateOnBuild: true));

            Assert.ThrowsAny<Exception>(() => builder.Build().Dispose());
        }

        // The option also flows through the non-web DefaultBootstrapHostHandler path. A plain module
        // host does not validate on build by default, so enabling it is the meaningful direction here.
        [Fact]
        public void ValidateServiceProvider_applies_to_non_web_module_host()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.HostModule<NonWebModuleWithContainerOnlyService>(
                options => options.ValidateServiceProvider(validateScopes: false, validateOnBuild: true));

            Assert.ThrowsAny<Exception>(() => builder.Build().Dispose());
        }

        [Fact]
        public void NonWebModule_without_override_builds()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(new Container());
            builder.HostModule<NonWebModuleWithContainerOnlyService>();

            builder.Build().Dispose();
        }

        public interface IBus
        {
        }

        public class FakeBus : IBus
        {
        }

        public class StartBusHandler : IHostedServiceHandler
        {
            // Depends on a service that is only registered in the SimpleInjector container.
            public StartBusHandler(IBus bus)
            {
                _ = bus;
            }

            public Task Execute(CancellationToken stoppingToken) => Task.CompletedTask;
        }

        public class WebModuleWithHostedHandler : WebModule
        {
            public override string Path { get; } = "";

            // ReSharper disable once UnusedMember.Global
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddHostedHandler<StartBusHandler>();
            }

            // ReSharper disable once UnusedMember.Global
            public void ConfigureContainer(Container container)
            {
                container.RegisterInstance<IBus>(new FakeBus());
            }

            // ReSharper disable once UnusedMember.Global
            public void Configure()
            {
            }
        }

        // Registers a concrete service by implementation type (so ValidateOnBuild would try to
        // construct it) whose only dependency is registered in the SimpleInjector container.
        public class NeedsBus
        {
            public NeedsBus(IBus bus)
            {
                _ = bus;
            }
        }

        public class WebModuleWithContainerOnlyService : WebModule
        {
            public override string Path { get; } = "";

            // ReSharper disable once UnusedMember.Global
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<NeedsBus>();
            }

            // ReSharper disable once UnusedMember.Global
            public void ConfigureContainer(Container container)
            {
                container.RegisterInstance<IBus>(new FakeBus());
            }

            // ReSharper disable once UnusedMember.Global
            public void Configure()
            {
            }
        }

        // Non-web counterpart of WebModuleWithContainerOnlyService, hosted via DefaultBootstrapHostHandler.
        public class NonWebModuleWithContainerOnlyService
        {
            // ReSharper disable once UnusedMember.Global
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddSingleton<NeedsBus>();
            }

            // ReSharper disable once UnusedMember.Global
            public void ConfigureContainer(Container container)
            {
                container.RegisterInstance<IBus>(new FakeBus());
            }
        }
    }
}
