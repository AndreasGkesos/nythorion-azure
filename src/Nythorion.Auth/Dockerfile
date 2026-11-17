# Build context is the repository root: docker build -f src/Nythorion.Auth/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Nythorion.Auth/Nythorion.Auth.csproj src/Nythorion.Auth/
RUN dotnet restore src/Nythorion.Auth/Nythorion.Auth.csproj

COPY src/Nythorion.Auth src/Nythorion.Auth
RUN dotnet publish src/Nythorion.Auth/Nythorion.Auth.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
LABEL org.opencontainers.image.source="https://github.com/AndreasGkesos/nythorion-azure"
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nythorion.Auth.dll"]
