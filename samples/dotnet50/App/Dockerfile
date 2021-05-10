#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["samples/dotnet50/App/App.csproj", "samples/dotnet50/App/"]
COPY ["src/Hosuto.Hosting.AspNetCore/Hosuto.Hosting.AspNetCore.csproj", "src/Hosuto.Hosting.AspNetCore/"]
COPY ["src/Hosuto.Hosting/Hosuto.Hosting.csproj", "src/Hosuto.Hosting/"]
COPY ["src/Hosuto/Hosuto.csproj", "src/Hosuto/"]
COPY ["src/Hosuto.Abstractions/Hosuto.Abstractions.csproj", "src/Hosuto.Abstractions/"]
COPY ["samples/shared/Hosuto.Samples.SimpleModule/Hosuto.Samples.SimpleModule.csproj", "samples/shared/Hosuto.Samples.SimpleModule/"]
COPY ["samples/shared/Hosuto.Samples.Common/Hosuto.Samples.Common.csproj", "samples/shared/Hosuto.Samples.Common/"]
COPY ["src/Hosuto.SimpleInjector/Hosuto.SimpleInjector.csproj", "src/Hosuto.SimpleInjector/"]
COPY ["samples/dotnet50/SampleWebModule/SampleWebModule.csproj", "samples/dotnet50/SampleWebModule/"]
RUN dotnet restore "samples/dotnet50/App/App.csproj"
COPY . .
WORKDIR "/src/samples/dotnet50/App"
RUN dotnet build "App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dbosoft.Hosuto.Samples.App.dll"]