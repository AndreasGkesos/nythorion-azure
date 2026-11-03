# Build context is the repository root: docker build -f src/Nythorion.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Nythorion.Api/Nythorion.Api.csproj src/Nythorion.Api/
COPY src/Modules/Nythorion.Documents/Nythorion.Documents.csproj src/Modules/Nythorion.Documents/
COPY src/Modules/Nythorion.Embeddings/Nythorion.Embeddings.csproj src/Modules/Nythorion.Embeddings/
COPY src/Modules/Nythorion.Search/Nythorion.Search.csproj src/Modules/Nythorion.Search/
COPY src/Modules/Nythorion.Learning/Nythorion.Learning.csproj src/Modules/Nythorion.Learning/
RUN dotnet restore src/Nythorion.Api/Nythorion.Api.csproj

COPY src/Nythorion.Api src/Nythorion.Api
COPY src/Modules src/Modules
RUN dotnet publish src/Nythorion.Api/Nythorion.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nythorion.Api.dll"]
