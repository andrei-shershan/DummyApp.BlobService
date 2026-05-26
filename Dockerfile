# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build images.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DummyApp.BlobService.WebApi/DummyApp.BlobService.WebApi.csproj", "src/DummyApp.BlobService.WebApi/"]
RUN dotnet restore "./src/DummyApp.BlobService.WebApi/DummyApp.BlobService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/DummyApp.BlobService.WebApi"
RUN dotnet build "./DummyApp.BlobService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DummyApp.BlobService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DummyApp.BlobService.WebApi.dll"]
