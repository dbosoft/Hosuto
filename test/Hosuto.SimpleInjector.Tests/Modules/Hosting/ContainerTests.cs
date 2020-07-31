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
        public void Module_can_resolve_services_from_outer_container()
        {
            var container = new Container();
            var serviceMock = new Mock<IService>();
            serviceMock.Setup(x=>x.CallMe()).Verifiable();
            container.RegisterInstance(serviceMock.Object);

            var builder = ModuleHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(container);
            builder.HostModule<SomeModule>();
            var host = builder.Build();

            serviceMock.Verify(x=>x.CallMe());

        }

        [Fact]
        public void Module_uses_SimpleInjector_as_inner_container()
        {
            var serviceMock = new Mock<IService>();
            serviceMock.Setup(x => x.CallMe()).Verifiable();

            var builder = ModuleHost.CreateDefaultBuilder();
            builder.UseSimpleInjector();
            builder.HostModule<SomeModule>();
            var host = builder.Build();
            var moduleHost = host.ModuleHostServices.GetRequiredService<IModuleHost<SomeModule>>();
            Assert.IsType<Container>(moduleHost.ModuleContext.Services);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public interface IService
        {
            void CallMe();
        }

        private class SomeModule : IModule
        {
            public string Name => "I'm a module";

            public void ConfigureServices(IServiceProvider sp, IServiceCollection services)
            {
                var service = sp.GetService<IService>();
                service?.CallMe();
            }

        }
    }

    
}
