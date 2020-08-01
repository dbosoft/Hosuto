using System;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Hosuto.Hosting.Tests
{
    public class ModuleBuildingTests
    {
        [Theory]
        [InlineData("Development")]
        [InlineData("Staging")]
        [InlineData("Production")]
        public void Build_module_has_environment_of_host(string environmentName)
        {
            var builder = ModuleHost.CreateDefaultBuilder();
            builder.HostModule<SomeModule>();
            builder.UseEnvironment(environmentName);
            var host = builder.Build();

            var module = host.Services.GetRequiredService<SomeModule>();
            Assert.Equal(environmentName, module.Environment);
        }

        [Fact]
        public void Multiple_modules_can_be_bootstrapped_and_see_each_other()
        {
            var builder = ModuleHost.CreateDefaultBuilder();
            builder.HostModule<SomeModule>();
            builder.HostModule<OtherModule>();

            var host = builder.Build();
            var module = host.Services.GetRequiredService<OtherModule>();

            Assert.NotNull(module.SomeModule);
        }

        private class SomeModule : IModule
        {
            public string Name => "I'm a module";
            public string Environment { get; private set; }

            public void ConfigureServices(IServiceCollection services, IHostingEnvironment environment)
            {
                Environment = environment.EnvironmentName;
            }

        }

        private class OtherModule : IModule
        {
            public string Name => "I'm a module, too";
            public SomeModule SomeModule { get; private set;  }

            public void ConfigureServices(IServiceProvider sp, IServiceCollection services)
            {
                SomeModule = sp.GetRequiredService<SomeModule>();
            }

        }
    }
}