using System;
using System.Collections.Generic;
using System.Text;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Hosuto.Hosting.Tests.Modules.Hosting
{
    public class ServiceCollectionContainerTests
    {
        [Fact]
        public void Module_can_resolve_services_from_outer_container()
        {
            var sc = new ServiceCollection();
            var serviceMock = new Mock<IService>();
            serviceMock.Setup(x=>x.CallMe()).Verifiable();
            sc.AddSingleton(serviceMock.Object);

            var builder = ModulesHost.CreateDefaultBuilder();
            builder.UseServiceCollection(sc);
            builder.HostModule<SomeModule>();
            var host = builder.Build();

            serviceMock.Verify(x=>x.CallMe());

        }

        // ReSharper disable once MemberCanBePrivate.Global
        public interface IService
        {
            void CallMe();
        }

        private class SomeModule
        {
            public void ConfigureServices(IServiceProvider sp, IServiceCollection services)
            {
                var service = sp.GetService<IService>();
                service.CallMe();
            }

        }
    }
}
