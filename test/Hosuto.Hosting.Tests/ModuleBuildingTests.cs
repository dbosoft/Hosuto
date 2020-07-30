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
            var sc = new ServiceCollection();
            var builder = ModuleHost.CreateDefaultBuilder();
            builder.UseServiceCollection(sc);
            builder.HostModule<SomeModule>();
            builder.UseEnvironment(environmentName);
            var host = builder.Build();
            var module = host.Services.GetRequiredService<SomeModule>();

            Assert.Equal(environmentName, module.Environment);
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
    }
}