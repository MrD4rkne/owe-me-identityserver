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

# Copy all .csproj files to restore dependencies
COPY src/OweMe.Identity.Server/*.csproj ./src/OweMe.Identity.Server/
COPY src/OweMe.Identity.Persistence/*.csproj ./src/OweMe.Identity.Persistence/
COPY src/OweMe.Identity.Migrator/*.csproj ./src/OweMe.Identity.Migrator/

# Restore dependencies (cached layer unless csproj changes)
RUN dotnet restore ./src/OweMe.Identity.Server/OweMe.Identity.Server.csproj
RUN dotnet restore ./src/OweMe.Identity.Migrator/OweMe.Identity.Migrator.csproj

# Copy remaining source files
COPY src/ ./src/

# Build the projects
WORKDIR /src/src/OweMe.Identity.Server
RUN dotnet build "OweMe.Identity.Server.csproj" -c "$BUILD_CONFIGURATION" --no-restore -o /app/build

WORKDIR /src/src/OweMe.Identity.Migrator
RUN dotnet build "OweMe.Identity.Migrator.csproj" -c "$BUILD_CONFIGURATION" --no-restore -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# Publish Web App to its own temporary folder
WORKDIR /src/src/OweMe.Identity.Server
RUN dotnet publish "OweMe.Identity.Server.csproj" \
    -c "$BUILD_CONFIGURATION" \
    --no-restore  \
    -o /app/publish/server \
    /p:UseAppHost=false

# Publish Migrator to its own temporary folder
WORKDIR /src/src/OweMe.Identity.Migrator
RUN dotnet publish "OweMe.Identity.Migrator.csproj" \
    -c "$BUILD_CONFIGURATION" \
    --no-restore  \
    -o /app/publish/migrator \
    /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app

COPY --from=publish /app/publish/server ./server
COPY --from=publish /app/publish/migrator ./migrator

WORKDIR /app/server
ENTRYPOINT ["dotnet", "OweMe.Identity.Server.dll"]
