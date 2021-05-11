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
            var host = builder.Build();
            var moduleHost = host.Services.GetRequiredService<IModuleHost<SomeModule>>();

            serviceMock.Verify(x=>x.CallMe());

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
    }

    
}
