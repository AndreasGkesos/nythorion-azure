# Nythorion — Project Guide

## This repo: nythorion-azure (Azure deployment PoC)
This is a one-off copy of the original local Nythorion project (`..\Nythorion`, which must stay untouched and keep running locally with Ollama). Purpose: prove a full Azure deployment (Container Apps, Static Web Apps, Postgres Flexible Server with pgvector, Azure OpenAI, Bicep, GitHub Actions) on a free trial. Ollama has been removed here: AI calls go through `Microsoft.Extensions.AI` to Azure OpenAI. The full plan, decisions and risks are in the project memory file `project_azure_fork.md` — read it before starting any work. Sections below describe the original local design and are outdated where they mention Ollama or local-only operation.

## What this is
A local AI-powered knowledge and learning platform. Runs entirely on the user's machine — no paid cloud APIs. Supports PDF/DOCX upload, semantic search, RAG chat, flashcards, quizzes, and summaries. Multilingual (English + German).

## Architecture
- **Modular monolith** — modules are .NET class libraries composed by the API host. Not microservices.
- **Vertical Slice Architecture (VSA)** — features are organized by slice, not by layer. No separate Data/Service/Controller layers.
- **MediatR** — each feature is a request/handler pair inside its module folder.
- One shared `NythorionDbContext` in the API host. Each module contributes its own entities and EF configurations via `IEntityTypeConfiguration<T>`.

## Solution structure
```
src/
  Nythorion.Api/          # ASP.NET Core host, Minimal APIs, composition root
  Nythorion.Auth/         # OpenIddict auth server, separate ASP.NET Core app
  Nythorion.Web/          # Angular SPA
  Modules/
    Nythorion.Documents/  # Upload, parse, chunk
    Nythorion.Embeddings/ # Embedding generation, pgvector storage
    Nythorion.Learning/   # Flashcards, quizzes, summaries, notes
    Nythorion.Search/     # Semantic search, RAG chat
tests/
docker/
```

## Technology stack
- .NET 9, ASP.NET Core, Minimal APIs
- OpenIddict for auth (OAuth2/OIDC)
- PostgreSQL 16 + pgvector
- Azure OpenAI through `Microsoft.Extensions.AI` (managed identity, no keys) — gpt-4.1-mini for chat, text-embedding-3-small for embeddings
- Angular (standalone components, Angular Material, signals)
- xUnit, FluentAssertions, Testcontainers

## Databases
- `nythorion` — main application database
- `nythorion_auth` — auth server database (OpenIddict tables)
- Both run in the same Postgres container via Docker Compose

## Coding standards
- Follow clean code principles — meaningful names, small focused methods, no unnecessary comments
- No repository pattern — use EF Core DbContext directly in handlers
- No magic strings — use constants or strongly typed values
- Prefer `record` types for DTOs and requests
- Use `sealed` on classes that are not meant to be inherited
- Async all the way — no `.Result` or `.Wait()`
- Validate at boundaries (API endpoints), trust internal code
- No over-engineering — build what the feature needs, nothing more

## Git rules
- Commit after each logical step
- Never push `appsettings.Development.json` — it is gitignored
- `appsettings.Development.json.example` is pushed as a template
- Keep commit messages clear and descriptive

## Secrets
- `appsettings.Development.json` — gitignored, contains real local values
- `appsettings.Development.json.example` — pushed, contains placeholder values
- Never hardcode secrets in any file that gets committed

## .NET modern patterns
This project uses .NET 9 patterns. There is no `Startup.cs`. Everything is configured in `Program.cs`:
- `builder.Services.Add...` replaces `ConfigureServices()`
- `app.Use...` / `app.Map...` replaces `Configure()`
