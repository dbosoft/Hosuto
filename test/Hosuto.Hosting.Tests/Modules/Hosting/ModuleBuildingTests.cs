using System;
using System.Collections.Generic;
using Dbosoft.Hosuto.Modules;
using Dbosoft.Hosuto.Modules.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hosuto.Hosting.Tests.Modules.Hosting
{
    public class ModuleBuildingTests
    {
        [Theory]
        [InlineData("Development")]
        [InlineData("Staging")]
        [InlineData("Production")]
        public void Build_module_has_environment_of_host(string environmentName)
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.HostModule<SomeModule>();
            builder.UseEnvironment(environmentName);
            var host = builder.Build();

            var module = host.Services.GetRequiredService<SomeModule>();
            Assert.Equal(environmentName, module.Environment);
        }

        [Fact]
        public void Build_module_has_configuration_from_host()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.HostModule<SomeModule>();
            builder.ConfigureAppConfiguration(c => 
                c.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["test"] = "testValue"
                }));

            var host = builder.Build();

            var module = host.Services.GetRequiredService<SomeModule>();
            Assert.Equal("testValue", module.Configuration["test"]);
        }

        [Fact]
        public void Build_module_has_logger_from_host()
        {
            var builder = ModulesHost.CreateDefaultBuilder();

            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.Log(LogLevel.Trace,
                        It.IsAny<EventId>(), null, It.IsAny<Exception>(), 
                        It.IsAny<Func<object, Exception, string>>())).Verifiable();

                var logProviderMoq = new Mock<ILoggerProvider>();
            logProviderMoq.Setup(x => 
                x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            builder.HostModule<SomeModule>();

            builder.ConfigureLogging(lb =>
                lb.SetMinimumLevel(LogLevel.Trace)
                .AddProvider(logProviderMoq.Object));

            var host = builder.Build();

            var moduleHost = host.Services.GetRequiredService<IModuleHost<SomeModule>>();
            var loggerFactory = moduleHost.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("dummy");
            logger.Log<object>(LogLevel.Trace, new EventId(0), null, null, (s, e) => "trace message");

            loggerMock.Verify();
        }

        [Fact]
        public void Multiple_modules_can_be_bootstrapped_and_see_each_other()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.HostModule<SomeModule>();
            builder.HostModule<OtherModule>();

            var host = builder.Build();
            var module = host.Services.GetRequiredService<OtherModule>();

            Assert.NotNull(module.SomeModule);
        }


        [Fact]
        public void Module_dependencies_can_be_injected_from_host_container()
        {
            var builder = ModulesHost.CreateDefaultBuilder();
            builder.HostModule<ModuleWithConstructorInjection>();

            var depMock = Mock.Of<IDep>();
            var sc = new ServiceCollection();
            sc.AddTransient(sp => depMock);

            builder.UseServiceCollection(sc);

            var host = builder.Build();
            var module = host.Services.GetRequiredService<ModuleWithConstructorInjection>();

            Assert.Equal(depMock,module.Dependency);
        }

        private class SomeModule
        {
            public string Environment { get; private set; }
            public IConfiguration Configuration { get; private set; }

            public SomeModule(IConfiguration appConfiguration)
            {
                
            }

#pragma warning disable 618
            public void ConfigureServices(IServiceCollection services, IHostingEnvironment environment, IConfiguration configuration)
#pragma warning restore 618
            {
                Environment = environment.EnvironmentName;
                Configuration = configuration;
            }

        }

        public interface IDep
        {
            
        }

        private class ModuleWithConstructorInjection
        {
            public IDep Dependency { get;  }
            public ModuleWithConstructorInjection(IDep dep)
            {
                Dependency = dep;
            }

        }

        private class OtherModule
        {
            public SomeModule SomeModule { get; private set;  }

            public void ConfigureServices(IServiceProvider sp, IServiceCollection services)
            {
                SomeModule = sp.GetRequiredService<SomeModule>();
            }

        }
    }
}