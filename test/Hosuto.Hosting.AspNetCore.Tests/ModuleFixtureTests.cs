using System.Threading.Tasks;
using Dbosoft.Hosuto.Modules.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Hosuto.Hosting.AspNetCore.Tests
{
    public class ModuleFixtureTests : IClassFixture<WebModuleFactory<SomeWebModule>>
    {
        private readonly WebModuleFactory<SomeWebModule> _factory;

        public ModuleFixtureTests(WebModuleFactory<SomeWebModule> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task SomeModuleRepliesWithOk()
        {
            var result = await _factory.WithWebHostBuilder(b =>
                {
                    b.UseSolutionRelativeContentRoot("");
                })
                .CreateClient().GetStringAsync("/");

            Assert.Equal("Ok", result);
        }

        [Fact]
        public async Task ConfigureTestServices_is_called_after_module_setup()
        {
            var someServiceMoq = new Mock<ISomeService>();

            var result = await _factory.WithWebHostBuilder(b =>
                {
                    b.UseSolutionRelativeContentRoot("");
                    b.ConfigureTestServices(services =>
                    {
                        services.AddTransient(sp => someServiceMoq.Object);
                    });
                })
                .CreateClient().GetStringAsync("/");

            someServiceMoq.Verify(x => x.CallMe());
            
        }
    }
}