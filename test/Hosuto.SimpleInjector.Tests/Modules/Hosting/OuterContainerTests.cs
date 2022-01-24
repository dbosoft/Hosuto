using System;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleInjector;
using Xunit;

namespace Hosuto.SimpleInjector.Tests.Modules.Hosting
{
    public class OuterContainerTests
    {
        [Fact]
        public void Module_can_resolve_services_from_outer_container()
        {
            var container = new Container();
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(container);

            var serviceMock = new Mock<IService>();
            serviceMock.Setup(x => x.CallMe()).Verifiable();
            container.RegisterInstance(serviceMock.Object);


            builder.HostModule<SomeModule>();
            builder.Build();
            var moduleHost = container.GetRequiredService<IModuleHost<SomeModule>>();

            serviceMock.Verify(x=>x.CallMe());

        }

        [Fact]
        public void Module_dependencies_can_be_injected_from_outer_container()
        {
            var container = new Container();

            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseSimpleInjector(container);
            var serviceMock = Mock.Of<IService>();
            container.RegisterInstance(serviceMock);


            builder.HostModule<ModuleWithConstructorInjection>();

            var host = builder.Build();
            var module = container.GetRequiredService<ModuleWithConstructorInjection>();
            Assert.Equal(module.Dependency, serviceMock);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public interface IService
        {
            void CallMe();
        }

        private class SomeModule
        {
            public string Name => "I'm a module";

            public void ConfigureServices(IServiceProvider sp, IServiceCollection services)
            {
                var service = sp.GetService<IService>();
                service?.CallMe();
            }

        }


        // ReSharper disable once ClassNeverInstantiated.Local
        private class ModuleWithConstructorInjection
        {
            public IService Dependency { get; }
            public ModuleWithConstructorInjection(IService dep)
            {
                Dependency = dep;
            }

        }
    }

    
}
