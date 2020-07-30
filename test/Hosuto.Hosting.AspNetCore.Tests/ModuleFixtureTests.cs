//using System.Diagnostics;
//using System.Threading.Tasks;
//using Dbosoft.Hosuto.Modules.Testing;
//using Microsoft.AspNetCore.TestHost;
//using Xunit;

//namespace Hosuto.Hosting.Tests
//{
//    public class ModuleFixtureTests : IClassFixture<WebModuleFactory<SomeWebModule>>
//    {
//        private readonly WebModuleFactory<SomeWebModule> _factory;

//        public ModuleFixtureTests(WebModuleFactory<SomeWebModule> factory)
//        {
//            _factory = factory;
//        }

//        [Fact]
//        public async Task SomeModuleRepliesWithOk()
//        {
//            var result = await _factory.WithWebHostBuilder(b =>
//                {
//                    b.UseSolutionRelativeContentRoot("");
//                })
//                .CreateClient().GetStringAsync("/");

//            Assert.Equal("Ok", result);
//        }
//    }
//}