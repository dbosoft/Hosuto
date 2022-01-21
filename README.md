## Hosuto
Advanced application hosting with .NET Generic Host and Asp.Net Core

Stable                     |  Latest                   |  Build Status
---------------------------|---------------------------|---------------------------
[![NuGet stable](https://img.shields.io/nuget/v/Dbosoft.Hosuto.svg?style=flat-square)](https://www.nuget.org/packages/Dbosoft.Hosuto) | [![NuGet pre](https://img.shields.io/nuget/vpre/Dbosoft.Hosuto.svg?style=flat-square)](https://www.nuget.org/packages/Dbosoft.Hosuto) | [![Build Status](https://dev.azure.com/dbosoft/public/_apis/build/status/dbosoft.Hosuto?branchName=master)](https://dev.azure.com/dbosoft/public/_build/latest?definitionId=32&branchName=master)


### Description

Hosuto is targeting microservice architectures, where services can be easily distributed as a single (micro) service or bundled into larger applications. 

To support service bundling it extends the [.NET generic host](https://docs.microsoft.com/en-us/dotnet/core/extensions/generic-host) by the capability to run multiple host in the same process. In addition the hosts can share common objects like configuration, hosting environment or a shared DI container. 

Hosuto contains currently the following features to extend .NET Generic Host: 

- **Modules**: The Hosuto module system allows you to setup multiple independed hosts within a single application. Modules can share configuration and a DI container for global settings and interaction. The module system supports both .NET Generic host and Asp.Net Core 2.1 and higher.  
  See [wiki](https://github.com/dbosoft/Hosuto/wiki/Modules) how to configure and use modules. 
- **Hosted Services extensions**: Hosuto contains extensions methods to simplify the use of hosted services in Generic Hosts and Asp.Net Core Hosts. 



### Platforms & Prerequisites

Hosuto supports .NET Standard 2.0 or higher, Asp.Net Core >= 2.1 and <= 3.1 and .NET 5.0 to .NET 6.0


### Getting started

The easiest way to get started is by installing [the available NuGet package](https://www.nuget.org/packages/Dbosoft.Hosuto). 
Take a look at the [Wiki](https://github.com/dbosoft/Hosuto/wiki) learning how to configure and use Hosuto.

**Samples:**

* For basic examples see the [samples](https://github.com/dbosoft/Hosuto/tree/master/samples) folder of the repository. 
* For a real world application example see our [SAPHub sample project](https://github.com/dbosoft/SAPHub).


### Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/dbosoft/Hosuto/tags).  

**Warning:**
Version 1.0 is still not released, that means that new minor versions can include breaking changes!



### Authors

* **Frank Wagner** - *Initial work* - [fw2568](https://github.com/fw2568)

See also the list of [contributors](https://github.com/Dbosoft/Hosuto/contributors) who participated in this project.



### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details


