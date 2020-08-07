using System;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleInjector;
using Xunit;

namespace Hosuto.SimpleInjector.Tests.Modules.Hosting
{
    public class UseContainerTests
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
            
        }
    }

    
}
