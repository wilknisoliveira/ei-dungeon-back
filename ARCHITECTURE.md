# EI Dungeon Backend — Architecture

## Stack
- **.NET 9** ASP.NET Core (`net9.0`; Docker SDK and runtime images also use .NET 9)
- **PostgreSQL** + EF Core 8 + Npgsql
- **JWT** auth with roles: `Admin`, `CommonUser`, `PremiumUser`
- **SignalR** hub at `/hubs` — JWT passed via `?access_token=` query param for WebSocket connections
- **GenAI**: Gemini via `GeminiDotnet` (key: `keys:GeminiApiKey`), also has OpenAIApiToken unused
- **xUnit** + **FakeItEasy** + **FluentAssertions** for tests
- **Swagger** at `/swagger` (dev only), **Health** at `/health`
- **AutoMapper**, **Serilog** (file + console), **IStringLocalizer** for multi-language

## Project Layout (single-project Clean Architecture)
```
ei-back/
├── Core/Domain/         — Entities, Enums, DomainExceptions
├── Core/Application/    — UseCases, Services, Repository interfaces
├── Infrastructure/      — EF Context, Migrations, GenAI, Token, Extensions, Swagger, Mappings
├── UserInterface/Api/   — Controllers
└── UserInterface/Hubs/  — SignalR hubs
```

- No separate projects per layer; namespaces enforce boundaries.
- Controllers call **UseCases** which use **Services** and **Repository** interfaces.
- DI registration in `Infrastructure/Extensions/ServiceCollectionExtensions.cs`.
- AutoMapper profile in `Infrastructure/Mappings/MappingsProfile.cs`.

## API Controllers
| Route | File |
|---|---|
| `api/user/auth/signin` | `AuthController.cs` |
| `api/user` | `UserController.cs` |
| `api/game` | `GameController.cs` (GET, POST, PATCH `/{gameId}`, DELETE `/{gameId}`) |
| `api/game/play` | `PlayController.cs` |
| `api/game/GameInfo` | `GameInfoController.cs` |
| `api/role` | `RoleController.cs` |

## Game Language
Each game has a `GameLanguage` (enum: `Portuguese`, `English`, `Spanish`; default `English`) stored in the `game_language` column. The language is set at creation time via `GameDtoRequest.GameLanguage` and can be updated via `PATCH /api/game/{gameId}`. All GenAI system prompts are in English with a `<language>` instruction tag appended per game language, so the Game Master responds in the configured language. Changing the language affects only future responses — existing history is untouched.

## Conventions
- Async methods suffix with `Async` only for EF/IO-bound operations; some sync UseCases exist (`Signin`).
- `IUnitOfWork.Commit()` / `CommitAsync()` called in controllers after use case handlers. **Exception**: `NewUserPlayUseCase` handles its own commit internally for both first-play and subsequent-play flows, since streaming requires atomic persistence before the stream finalizes.
- Enums serialized as strings (`JsonStringEnumConverter`).
- All responses include `application/json` content type.

## Container Deployment
- Multi-stage Dockerfile uses .NET 9 SDK and ASP.NET Core runtime images.
- Final image runs as the non-root `app` user and exposes HTTP port 8080.
- The target hosting platform is not yet defined.

## Local Docker Compose

The root `compose.yaml` defines two services:

```text
.env
  └─> Docker Compose substitutions
        ├─> backend (ASP.NET Core environment variables)
        │     └─> depends on postgres: service_healthy
        └─> postgres (database, user, password)
              └─> postgres_data named volume
```

- Developers copy `.env.example` to the Git-ignored `.env`. Compose explicitly maps those values to nested ASP.NET Core keys using double underscores; container environment values override `appsettings.json`.
- The backend connects to host `postgres` on the internal Compose network. Host ports for the backend and PostgreSQL remain configurable through `.env`.
- PostgreSQL 16 runs `docker/postgres/init-uuid-ossp.sql` only when initializing a fresh named volume. This makes `uuid_generate_v4()` available to the historical EF migrations.
- PostgreSQL has a database-aware health check. Compose starts the backend only after that check passes.
- Both Compose services use `restart: unless-stopped`, so Docker restarts them after failures or daemon restarts unless a user stopped them explicitly.
- The backend retains `dbContext.Database.Migrate()` at application startup, so it applies pending migrations after the database becomes ready.
- Service-scoped Compose commands manage `backend` without recreating or removing PostgreSQL. `docker compose down -v` is the explicit destructive database reset.
