using System;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.EventLog;
using Moq;
using SimpleInjector;
using Xunit;

namespace Hosuto.SimpleInjector.Tests.Modules.Hosting
{
    public class WebModuleConfigureContainerTests
    {

        [Fact]
        public void ConfigureContainer_is_called()
        {

            var moduleMock = new Mock<ModuleWithConfigureContainer>();
            moduleMock.Setup(x => x.ConfigureContainer(
                It.IsAny<Container>())).Verifiable();

            BuildAndVerifyModuleMock(moduleMock);

        }

        [Fact]
        public void ConfigureContainer_with_ServiceProvider_is_called()
        {
            var container = new Container();

            var moduleMock = new Mock<ModuleWithConfigureContainerAndServiceProvider>();
            moduleMock.Setup(x => x.ConfigureContainer(
                It.Is<IServiceProvider>(sp => ReferenceEquals(sp, container)),
                It.IsAny<Container>())).Verifiable();

            BuildAndVerifyModuleMock(moduleMock, container);

        }

        [Fact]
        public void ConfigureContainer_with_Injection_is_called()
        {
            var moduleMock = new Mock<ModuleWithConfigureContainerAndInjection>();
            moduleMock.Setup(x => x.ConfigureContainer(
#pragma warning disable 618
                It.IsAny<Container>(), It.IsAny<IHostingEnvironment>())).Verifiable();
#pragma warning restore 618

            BuildAndVerifyModuleMock(moduleMock);

        }

        private void BuildAndVerifyModuleMock<TModule>(Mock<TModule> moduleMock, Container container = null) where TModule : class
        {
            var builder = ModulesHost.CreateDefaultBuilder();

            if (container == null)
                builder.UseSimpleInjector();
            else
                builder.UseSimpleInjector(container);

#if NETCOREAPP2_1
            builder.UseAspNetCore(WebHost.CreateDefaultBuilder,(module, webHostBuilder) => { });
#else
            builder.UseAspNetCore((module, webHostBuilder) => { });
#endif

            builder.HostModule<TModule>(
                options => options.ModuleFactoryCallback(_ => moduleMock.Object));

            builder.Build().Dispose();

            moduleMock.Verify();
        }

        [Fact]
        public void ConfigureContainer_extensions_are_called()
        {
            var configureMock = new Mock<IConfigureContainerFilter<ModuleWithConfigureContainer>>();
            configureMock.Setup(x =>
                x.Invoke(It.IsAny<Action<IModuleContext<ModuleWithConfigureContainer>, Container>>())
            ).Verifiable();

            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector();
#if NETCOREAPP2_1
            builder.UseAspNetCore(WebHost.CreateDefaultBuilder,(module, webHostBuilder) => { });
#else
            builder.UseAspNetCore((module, webHostBuilder) => { });
#endif
            builder.HostModule<ModuleWithConfigureContainer>();
            builder.ConfigureFrameworkServices((ctx, services) => services.AddTransient(sp => configureMock.Object));
            builder.Build().Dispose();

            configureMock.Verify();


        }

        public class ModuleWithConfigureContainer : WebModule
        {
            public override string Path { get; } = "";


            public virtual void ConfigureContainer(Container container)
            {}

        }

        public  class ModuleWithConfigureContainerAndInjection : WebModule
        {
            public override string Path { get; } = "";


#pragma warning disable 618
            public virtual void ConfigureContainer(Container container, IHostingEnvironment env)
#pragma warning restore 618
            {}

        }


        public class ModuleWithConfigureContainerAndServiceProvider : WebModule
        {
            public override string Path { get; } = "";
            public virtual void ConfigureContainer(IServiceProvider sp, Container container){}
        }

    }
}