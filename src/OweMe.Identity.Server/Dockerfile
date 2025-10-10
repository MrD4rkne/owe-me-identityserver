FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only project files first for better layer caching
COPY version.json ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY OweMe.Identity.sln ./

# Copy all .csproj files to restore dependencies
COPY src/OweMe.Identity.Server/*.csproj ./src/OweMe.Identity.Server/
COPY src/OweMe.Identity.UnitTests/*.csproj ./src/OweMe.Identity.UnitTests/
COPY src/OweMe.Identity.IntegrationTests/*.csproj ./src/OweMe.Identity.IntegrationTests/

# Restore dependencies (cached layer unless csproj changes)
RUN dotnet restore OweMe.Identity.sln

# Copy remaining source files
COPY src/ ./src/

# Build the project
WORKDIR /src/src/OweMe.Identity.Server
RUN dotnet build "OweMe.Identity.Server.csproj" -c "$BUILD_CONFIGURATION" --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OweMe.Identity.Server.csproj" -c "$BUILD_CONFIGURATION" --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OweMe.Identity.Server.dll"]
