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
    public class InnerContainerTests
    {
        [Fact]
        public void Module_uses_SimpleInjector_as_inner_container()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector();
            builder.HostModule<SomeModule>();
            var host = builder.Build();
            var moduleHost = host.Services.GetRequiredService<IModuleHost<SomeModule>>();
            Assert.IsType<Container>(moduleHost.ModuleContext.Services);
            host.Dispose();
        }


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
                     It.Is<IServiceProvider>(sp=> ReferenceEquals(sp, container)),
                     It.IsAny<Container>())).Verifiable();

            BuildAndVerifyModuleMock(moduleMock, container);

        }

        private void BuildAndVerifyModuleMock<TModule>(Mock<TModule> moduleMock, Container container = null) where TModule : class, IModule
        {
            var builder = ModulesHost.CreateDefaultBuilder();

            if (container == null)
                builder.UseSimpleInjector();
            else
                builder.UseSimpleInjector(container);

            builder.HostModule<SomeModule>(
                options => options.ModuleFactoryCallback(_ => moduleMock.Object));

            builder.Build().Dispose();

            moduleMock.Verify();
        }

        [Fact]
        public void ConfigureContainer_extensions_are_called()
        {
            var configureMock = new Mock<IContainerConfigurer<SomeModule>>();
            configureMock.Setup(x => 
                x.ConfigureContainer(It.IsAny<IModuleContext<SomeModule>>(), It.IsAny<Container>())
            ).Verifiable();

            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector();
            builder.HostModule<SomeModule>();
            builder.ConfigureFrameworkServices((ctx, services) => services.AddTransient(sp=>configureMock.Object));
            builder.Build().Dispose();

            configureMock.Verify();


        }

        public class SomeModule : IModule
        {
            public string Name => "I'm a module";


            public virtual void ConfigureContainer(Container container, IHostingEnvironment env)
            {

            }
        }

        public abstract class ModuleWithConfigureContainer : IModule
        {
            public abstract string Name { get; }


            public abstract void ConfigureContainer(Container container);

        }

        public abstract class ModuleWithConfigureContainerAndInjection : IModule
        {
            public abstract string Name { get; }


            public abstract void ConfigureContainer(Container container, IHostingEnvironment env);

        }


        public abstract class ModuleWithConfigureContainerAndServiceProvider : IModule
        {
            public abstract string Name { get;  }

            public abstract void ConfigureContainer(IServiceProvider sp, Container container);
        }

    }

    
}
