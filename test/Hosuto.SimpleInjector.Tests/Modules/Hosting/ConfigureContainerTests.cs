using System;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using SimpleInjector;
using Xunit;

namespace Hosuto.SimpleInjector.Tests.Modules.Hosting
{
    public class ConfigureContainerTests
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
                It.IsAny<Container>(), It.IsAny<IHostingEnvironment>())).Verifiable();

            BuildAndVerifyModuleMock(moduleMock);

        }

        private void BuildAndVerifyModuleMock<TModule>(Mock<TModule> moduleMock, Container container = null) where TModule : class, IModule
        {
            var builder = ModulesHost.CreateDefaultBuilder();

            if (container == null)
                builder.UseSimpleInjector();
            else
                builder.UseSimpleInjector(container);

            builder.HostModule<TModule>(
                options => options.ModuleFactoryCallback(_ => moduleMock.Object));

            builder.Build().Dispose();

            moduleMock.Verify();
        }

        [Fact]
        public void ConfigureContainer_extensions_are_called()
        {
            var configureMock = new Mock<IContainerConfigurer<ModuleWithConfigureContainer>>();
            configureMock.Setup(x =>
                x.ConfigureContainer(It.IsAny<IModuleContext<ModuleWithConfigureContainer>>(), It.IsAny<Container>())
            ).Verifiable();

            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector();
            builder.HostModule<ModuleWithConfigureContainer>();
            builder.ConfigureFrameworkServices((ctx, services) => services.AddTransient(sp => configureMock.Object));
            builder.Build().Dispose();

            configureMock.Verify();


        }

        public class ModuleWithConfigureContainer : IModule
        {
            public string Name  => nameof(ModuleWithConfigureContainer);


            public virtual void ConfigureContainer(Container container)
            {}

        }

        public  class ModuleWithConfigureContainerAndInjection : IModule
        {
            public string Name => nameof(ModuleWithConfigureContainerAndInjection);


            public virtual void ConfigureContainer(Container container, IHostingEnvironment env)
            {}

        }


        public class ModuleWithConfigureContainerAndServiceProvider : IModule
        {
            public string Name => nameof(ModuleWithConfigureContainerAndServiceProvider);

            public virtual void ConfigureContainer(IServiceProvider sp, Container container){}
        }

    }
}