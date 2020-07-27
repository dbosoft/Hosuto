using Dbosoft.Hosuto.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Hosuto.Hosting.Tests
{
    public class ModuleBuildingTests
    {
        [Fact]
        public void Test1()
        {
            var sc = new ServiceCollection();
            var builder = ModuleHost.CreateDefaultBuilder();
            builder.UseServiceCollection(sc);
            builder.HostModule<SomeModule>();
            builder.UseEnvironment(Environments.Development);

        }
    }
}