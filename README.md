# Nythorion

A personal knowledge base powered by local AI. Upload your PDFs and DOCX files, then ask questions, generate summaries, build flashcards, quizzes and notes — everything runs on your own machine, no subscriptions, no data leaving your computer.

Tested on an NVIDIA RTX 3060 6GB with `qwen2.5:7b-instruct-q4_K_M`.

> Built as an exploration project to see how .NET 9, Angular, OpenIddict, pgvector, and local LLMs fit together in practice. Developed with the assistance of [Claude](https://claude.ai).

---

## Prerequisites

Download and install each of these before starting:

| Tool | Version |
|------|---------|
| .NET SDK | 9.0 |
| Node.js | 20+ |
| Docker Desktop | latest |
| Ollama | latest |

> **GPU note:** A dedicated GPU with 6GB+ VRAM is recommended for acceptable response times. Ollama can run on CPU only, but generation will be slow.

---

## First-time setup

### 1. Trust the HTTPS development certificate

Run this once per machine after installing the .NET SDK:

```bash
dotnet dev-certs https --trust
```

When prompted, click **Yes** to trust the certificate.

### 2. Start PostgreSQL

Make sure Docker Desktop is open and running, then from the repo root:

```bash
docker compose -f docker/docker-compose.yml up -d
```

This starts a PostgreSQL container on port 5432 and creates two databases: `nythorion` and `nythorion_auth`.

### 3. Pull Ollama models

Make sure Ollama is running (it should appear in your system tray after installation), then pull the two models the app uses:

```bash
ollama pull qwen2.5:7b-instruct-q4_K_M
ollama pull nomic-embed-text
```

The first model (~4.7GB) handles text generation. The second handles search embeddings. This only needs to be done once.

### 4. Configure the API

Copy the example config file:

- **bash:** `cp src/Nythorion.Api/appsettings.Development.json.example src/Nythorion.Api/appsettings.Development.json`
- **PowerShell:** `copy src/Nythorion.Api/appsettings.Development.json.example src/Nythorion.Api/appsettings.Development.json`

The default values work out of the box if you used the Docker Compose file from step 2:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=nythorion;Username=nythorion;Password=nythorion"
  },
  "Auth": {
    "Issuer": "https://localhost:7087"
  }
}
```

### 5. Configure the Auth server

Copy the example config file:

- **bash:** `cp src/Nythorion.Auth/appsettings.Development.json.example src/Nythorion.Auth/appsettings.Development.json`
- **PowerShell:** `copy src/Nythorion.Auth/appsettings.Development.json.example src/Nythorion.Auth/appsettings.Development.json`

Then open the file and set a password for the admin account you will use to log in:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=nythorion_auth;Username=nythorion;Password=nythorion"
  },
  "Registration": {
    "Enabled": true
  },
  "SeedUsers": [
    { "Username": "admin", "Password": "your-password-here" }
  ]
}
```

### 6. Restore .NET tools and run database migrations

First restore the local tools (includes the EF Core CLI):

```bash
dotnet tool restore
```

Then create the database tables:

```bash
dotnet ef database update --project src/Nythorion.Api
dotnet ef database update --project src/Nythorion.Auth
```

You only need to run migrations once, and again after pulling updates that include new migrations.

### 7. Install frontend dependencies

```bash
cd src/Nythorion.Web
npm install
cd ../..
```

---

## Running

You need three things running at the same time. Open three separate terminals from the repo root:

```bash
# Terminal 1 — Auth server (must use the https profile to bind to port 7087)
dotnet run --project src/Nythorion.Auth --launch-profile https

# Terminal 2 — API
dotnet run --project src/Nythorion.Api --launch-profile https

# Terminal 3 — Frontend
cd src/Nythorion.Web
npm start
```

Then open `http://localhost:4200` and log in with the admin credentials you set in step 5.

> **Docker and Ollama must be running** before you start the backend.

---

## Running tests

```bash
# Unit tests
dotnet test tests/Nythorion.Documents.Tests
dotnet test tests/Nythorion.Learning.Tests

# Integration tests (requires Docker)
dotnet test tests/Nythorion.Api.IntegrationTests
```

Integration tests use Testcontainers to automatically spin up a real PostgreSQL container just for the test run — no manual setup needed, but Docker must be running so the container can be created.

---

## Architecture

- **Modular monolith** — modules are .NET class libraries composed by the API host
- **Vertical Slice Architecture** — each feature is a self-contained request/handler pair
- **MediatR** — all features go through command/query handlers
- **Shared DbContext** — one `NythorionDbContext` in the API host; modules contribute EF configurations
- **Ollama** — runs on the host machine; `qwen2.5:7b` for generation, `nomic-embed-text` for embeddings (768 dimensions)
- **pgvector** — stores and queries embeddings for semantic search

---

## Ollama configuration

The default model and settings are in `src/Nythorion.Api/appsettings.json`. You can override them in `appsettings.Development.json`:

```json
"Ollama": {
  "BaseUrl": "http://localhost:11434",
  "LlmModel": "qwen2.5:7b-instruct-q4_K_M",
  "EmbedModel": "nomic-embed-text",
  "Temperature": 0.1,
  "NumPredict": 4096
}
```

A 6GB GPU is sufficient for `qwen2.5:7b-instruct-q4_K_M`. Larger models will require more VRAM. Document processing (embeddings + initial generation) runs in the background after upload.

- **Temperature** controls how creative or predictable the model's responses are. `0.1` is very low, meaning the model sticks closely to the facts in the document and avoids making things up. Higher values (e.g. `0.8`) produce more varied but less reliable answers.
- **NumPredict** is the maximum number of tokens (roughly words) the model can generate in a single response. `4096` is enough for detailed summaries and answers. Lower it if responses feel slow; raise it if they get cut off.
