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



        public class SomeModule
        {

#pragma warning disable 618
            public virtual void ConfigureContainer(Container container, IHostingEnvironment env)
#pragma warning restore 618
            {

            }
        }

    }

    
}
