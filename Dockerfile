FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/DummyApp.BlobService.Functions/DummyApp.BlobService.Functions.csproj", "src/DummyApp.BlobService.Functions/"]
RUN dotnet restore "./src/DummyApp.BlobService.Functions/DummyApp.BlobService.Functions.csproj"
COPY . .
WORKDIR "/src/src/DummyApp.BlobService.Functions"
RUN dotnet build "./DummyApp.BlobService.Functions.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/src/DummyApp.BlobService.Functions"
RUN dotnet publish "./DummyApp.BlobService.Functions.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4 AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
ENV FUNCTIONS_WORKER_RUNTIME=dotnet-isolated
RUN apt-get update \
    && apt-get install -y --no-install-recommends wget ca-certificates apt-transport-https gnupg \
    && wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb \
    && dpkg -i /tmp/packages-microsoft-prod.deb \
    && rm /tmp/packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y --no-install-recommends dotnet-runtime-8.0 \
    && rm -rf /var/lib/apt/lists/*
EXPOSE 80
