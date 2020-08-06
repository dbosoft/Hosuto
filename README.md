# Hosuto
Advanced application hosting for .NET Generic Host and Asp.Net Core.


## Description

Hosuto contains currently the following features to extend .NET Generic Host: 

- **modules**: The hosuto module system allows you to setup multiple independed hosts within a single application. Modules can share configuration and a DI container for global settings and interaction. The module system supports both .NET Generic host and Asp.Net Core 2.1 and higher. 
- **HostedServices from delegates**: With Hosuto you can add HostedServices from delegates to the Generic Host. 

## Platforms & Prerequisites

Hosuto supports .NET Standard 2.0 or higher and Asp.Net Core >= 2.1 and <= 3.1.


## Getting started

The easiest way to get started is by installing [the available NuGet package](https://www.nuget.org/packages/Dbosoft.Hosuto). 
Take a look at the [Using](#using) section learning how to configure and use Hosuto.

## Using

### Modules

With Hosuto modules you can create isolated parts of your application that will all run in their own host but share global settings and a DI container. 
As each module has it's own service provider you can register different services in each module. For example if you host a server component in one module you could configure another module to be a client of the same component. If you now distribute the application you can choose to run each module in it's own application or to run the modules in the same application. 

To use modules first create a modules host in your applications main method. This host replaces the default .Net Generic Host. 

Add a reference to the nuget package [Dbosoft.Hosuto.Hosting](https://www.nuget.org/packages/Dbosoft.Hosuto.Hosting) and build the modules host from a ModulesHostBuilder: 

```csharp
        static Task Main(string[] args)
        {

            var builder = ModulesHost.CreateDefaultBuilder(args);
            
            // you can configure a module host builder like a host builder.
            // All configurations set with ConfigureHostConfiguration will be shared between all modules.
            builder.UseEnvironment(EnvironmentName.Development);
            return builder.RunConsoleAsync();

        }

  ```

Then you can define your modules. A module is typical placed in a sperated assembly but this is not required. 
Modules only have to implement the IModule interface but have a convention based setup logic like the Startup class for Asp.Net Core. 

```csharp
    public class SimpleModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services)
        {

            [...]
  ```

The configure service method will configure the DI container of the module host. Todo something useful you will typical have to register at least one HostedServices here. 

The module has now to be added to the modules host builder: 

```csharp
        static Task Main(string[] args)
        {

            var builder = ModulesHost.CreateDefaultBuilder(args);
            
            [...]
            
            builder.HostModule<SomeModule>();
            return builder.RunConsoleAsync();

        }

  ```


## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/dbosoft/Hosuto/tags). 

## Authors

* **Frank Wagner** - *Initial work* - [fw2568](https://github.com/fw2568)

See also the list of [contributors](https://github.com/Dbosoft/Hosuto/contributors) who participated in this project.


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details


