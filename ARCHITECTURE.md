# EI Dungeon Backend — Architecture

## Stack
- **.NET 9** ASP.NET Core (csproj: `net9.0`, but Dockerfile uses `mcr.microsoft.com/dotnet/aspnet:8.0` — mismatch)
- **PostgreSQL** + EF Core 8 + Npgsql
- **JWT** auth with roles: `Admin`, `CommonUser`, `PremiumUser`
- **SignalR** hub at `/hubs` — JWT passed via `?access_token=` query param for WebSocket connections
- **GenAI**: Gemini via `GeminiDotnet` (key: `keys:GeminiApiKey`), also has OpenAIApiToken unused
- **xUnit** + **FakeItEasy** + **FluentAssertions** for tests
- **Swagger** at `/swagger` (dev only), **Health** at `/health`, dashboard at `/healthDashboard`
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

## Deployment (Railway)
- `railway.toml` at root — Docker build, start command `dotnet ei-back.dll`
- Dockerfile uses .NET 8 (note: project targets .NET 9)
- Exposes 8080/8081
