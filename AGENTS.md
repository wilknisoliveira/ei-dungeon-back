# EI Dungeon Backend — AGENTS.md

## MANDATORY — Read Constitution First
**Always read `.specify/memory/constitution.md` before any task, implementation, or analysis.**

## MANDATORY — Keep Constitution Updated
**After any implementation, review and update `.specify/memory/constitution.md` if any information is outdated.**

## MANDATORY — Read Architecture First
**Always read `ARCHITECTURE.md` before any task, implementation, or analysis.**

## MANDATORY — Keep Architecture Updated
**After any implementation, review and update `ARCHITECTURE.md` if the architecture changed.**

## MANDATORY — Log Local Environment Info
**Read `LOCAL_GUIDES.md` for user-specific environment info before development tasks. Update it with any local development environment details (connection strings, API keys, tool versions, paths, ports, etc.) that are specific to this machine and not suitable for version control.**

## Build & Run
```powershell
# Build
dotnet build .\ei-back\ei-back.csproj

# Run (starts on http://localhost:5231)
dotnet run --project .\ei-back\ei-back.csproj

# Tests
dotnet test .\ei-back.Tests\ei-back.Tests.csproj
```

## EF Migrations
```powershell
# Must run from within ei-back/ directory
cd ei-back
dotnet ef migrations add <name> --context EIContext
dotnet ef database update --context EIContext
```

## Pre-Setup
1. Create Postgres schema `ei_db`
2. Enable `uuid-ossp` extension: `CREATE EXTENSION IF NOT EXISTS "uuid-ossp";`
3. Configure `PostgresConnection:PostgresConnectionString` and `keys:OpenRouterApiKey`
4. Default login: `admin` / `admin123`
5. Seed game info: `POST /api/game/GameInfo` with body from `Infrastructure/Utils/GameInfoSeeding.json`

## Tests (xUnit)
- Test project: `ei-back.Tests/` (targets `net9.0`)
- Global usings: `Xunit`, `FakeItEasy`, `FluentAssertions`
- Mock pattern: `A.Fake<T>()` + `A.CallTo(() => ...).Returns(...)`
- Single test file: `Domain/User/UserServiceTests.cs`

```powershell
# Run all tests
dotnet test .\ei-back.Tests\ei-back.Tests.csproj

# Run single test class
dotnet test .\ei-back.Tests\ei-back.Tests.csproj --filter "FullyQualifiedName~UserServiceTests"
```

## Config Values (appsettings.json)
- `PostgresConnection:PostgresConnectionString`
- `TokenConfigurations:Secret` (min ~64 chars for HS256)
- `keys:OpenRouterApiKey`
- `GenAISettings:AiModel` (default: `gpt-3.5-turbo-0125`; passed to OpenRouter via OpenAI connector)
- `Gateways:OpenRouterApi` — OpenRouter endpoint URL (`https://openrouter.ai/api/v1`)
- `PlayOptions:LimitTokens` (default: 10000)
- `Cors:AllowedOrigins` — comma-separated; defaults to `http://localhost:8000`
- User secrets ID: `aef84015-3b51-4aa5-8200-b13cd0be70b9`

## Gotchas
- **Dockerfile SDK mismatch**: targets `aspnet:8.0`/`sdk:8.0` but project is `net9.0`. Update both if build fails.
- No CI workflows (`.github/workflows/` is empty).
- `AddInfraHttpClients` is a no-op stub.
- `appsettings.*.json` contains placeholder values — use User Secrets or env vars in development.

<!-- SPECKIT START -->
Active plan: `specs/001-stream-initial-master-play/plan.md`
Feature: Stream initial master play — move from blocking CreateGameUseCase to
streaming NewUserPlayUseCase on first play. See the plan for detailed context,
constitution gates, project structure, and implementation quickstart.
[x] All 16 tasks complete. Build 0 errors/0 warnings, 5/5 tests pass.
<!-- SPECKIT END -->
