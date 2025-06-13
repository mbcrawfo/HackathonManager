FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build

# See https://github.com/NuGet/Home/issues/13062
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION=false

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/HackathonManager.Migrator/HackathonManager.Migrator.csproj ./HackathonManager.Migrator/HackathonManager.Migrator.csproj
COPY src/HackathonManager/HackathonManager.csproj ./HackathonManager/HackathonManager.csproj
RUN dotnet restore ./HackathonManager/HackathonManager.csproj

COPY src/HackathonManager.Migrator ./HackathonManager.Migrator
COPY src/HackathonManager ./HackathonManager
RUN dotnet publish ./HackathonManager/HackathonManager.csproj --no-restore --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HackathonManager.dll"]
