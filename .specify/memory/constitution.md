<!--
  Sync Impact Report
  Version change: 1.0.0 → 1.1.0 (streaming use-case exception)
  Modified principles: Principle II (UseCase commit responsibility), Principle III (transaction owner)
  Added sections: N/A
  Removed sections: N/A
  Templates requiring updates: N/A
  Follow-up TODOs: None
-->

# EI Dungeon Backend Constitution

## Core Principles

### I. Clean Architecture Layering (NON-NEGOTIABLE)
Core/Domain MUST NOT reference Infrastructure or UserInterface in any way.
Dependencies flow inward: UserInterface → Infrastructure → Core/Application →
Core/Domain. No circular dependencies between layers. UseCase orchestration,
Service logic, and Repository interfaces belong in Core/Application. EF Core
context, GenAI client, and external concerns belong in Infrastructure.

### II. UseCase-Mediated Controller Logic
Controllers MUST NOT contain business logic. They parse HTTP input, call a
single UseCase, and return a DTO. UseCases coordinate domain Services, enforce
business rules, and call Repository interfaces. All DTO mappings go through
AutoMapper (MappingsProfile.cs). Streaming use cases (e.g., NewUserPlayUseCase)
handle their own IUnitOfWork.CommitAsync() internally since atomic persistence
must occur before the stream finalizes.

### III. Repository + Unit of Work
All data access flows through Repository interfaces — never direct DbContext
references in application code. EIContext is the single EF Core DbContext.
Transactions are managed via IUnitOfWork committed after UseCase execution —
typically by the controller, but streaming use cases commit internally
(see Principle II exception).

### IV. Testable Dependencies (NON-NEGOTIABLE)
Services and UseCases MUST accept all dependencies via constructor injection.
Tests use FakeItEasy (A.Fake&lt;T&gt;()) for mocking and FluentAssertions for
assertions. Global usings are Xunit, FakeItEasy, FluentAssertions. Repository
interfaces must remain mockable — no direct DbContext in test targets.

### V. AI Provider Abstraction
AI interaction goes through IGenAi (GeminiDotnet implementation in GenAi.cs).
Provider choice is configurable via GenAISettings:AiModel and keys:GeminiApiKey
in appsettings. Token limits are enforced through system prompt instructions,
not ChatOptions, because the Gemini client does not reliably respect maxOutputTokens.

## Security & Authentication

JWT bearer authentication with HS256 and three roles: Admin, CommonUser,
PremiumUser. SignalR hub (/hubs) authenticates via ?access_token= query
parameter. Secrets (JWT secret, API keys, connection strings) MUST be
externalized via .NET User Secrets or environment variables — never committed
(appsettings.*.json contains placeholders only). CORS defaults to
http://localhost:8000.

## Development Workflow

Read this constitution first before any task. Read ARCHITECTURE.md for project
architecture. Read LOCAL_GUIDES.md for machine-specific dev environment info.
Update this constitution, ARCHITECTURE.md, and LOCAL_GUIDES.md after any
implementation if their information is outdated. Build commands: dotnet build (port
5231), dotnet test. EF migrations require cd ei-back before running. Default
login: admin / admin123. Seed game info via POST /api/game/GameInfo.

## Governance

This constitution supersedes all other development guidance. Amendments require:
(a) documentation of the change, (b) update of ARCHITECTURE.md and AGENTS.md if
affected, (c) version bump per semantic versioning. Non-negotiable principles
require explicit exception approval with documented rationale.

**Version**: 1.1.0 | **Ratified**: 2026-05-29 | **Last Amended**: 2026-06-01
