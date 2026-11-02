# Nythorion.Api — Guide

## Role
The composition root of the application. Hosts all Minimal API endpoints, wires up modules, and owns the shared `NythorionDbContext`.

## Key responsibilities
- Register all module services and EF configurations in `Program.cs`
- Define Minimal API endpoints (thin — no business logic here)
- Apply EF Core migrations
- Validate incoming requests at the boundary

## NythorionDbContext
- Lives in `src/Nythorion.Api/Data/NythorionDbContext.cs`
- Picks up entity configurations from all modules via `ApplyConfigurationsFromAssembly`
- pgvector extension is enabled via `modelBuilder.HasPostgresExtension("vector")`
- Database: `nythorion`

## Migrations
Run from the solution root:
```
dotnet dotnet-ef migrations add <Name> --project src/Nythorion.Api
dotnet dotnet-ef database update --project src/Nythorion.Api
```

## Endpoint conventions
- Use Minimal APIs — no controllers
- Group related endpoints with `RouteGroupBuilder`
- Return `TypedResults` for explicit response types
- Keep endpoint handlers thin — delegate to MediatR

## Connection string
- Key: `ConnectionStrings:Default`
- Points to the `nythorion` database
- Set in `appsettings.Development.json` (gitignored)
