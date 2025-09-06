FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/OweMe.Identity.Server/OweMe.Identity.Server.csproj", "src/OweMe.Identity.Server/"]
RUN dotnet restore "src/OweMe.Identity.Server/OweMe.Identity.Server.csproj"
COPY . .
WORKDIR "/src/src/OweMe.Identity.Server"
RUN dotnet build "./OweMe.Identity.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OweMe.Identity.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "OweMe.Identity.Server.dll"]
