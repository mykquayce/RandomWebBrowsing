FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build-env
WORKDIR /app

COPY . .
RUN dotnet restore RandomWebBrowsing.sln -s https://api.nuget.org/v3/index.json -s http://nuget/nuget
RUN dotnet publish RandomWebBrowsing.WorkerService/RandomWebBrowsing.WorkerService.csproj -c Release -o /app/publish -r linux-x64

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
ENV NETCORE_ENVIRONMENT=Production
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "RandomWebBrowsing.WorkerService.dll"]
