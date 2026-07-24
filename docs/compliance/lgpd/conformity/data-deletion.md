# Data Deletion — Technical Conformity

**Audit Date:** 2026-07-24
**Repository:** ei-dungeon-back
**Status:** Based on code audit evidence only

---

## 1. Deletion Mechanisms Found

### 1.1 Game Deletion

- **Endpoint:** `DELETE /api/game/{gameId}`
- **Authorization:** `Admin, CommonUser, PremiumUser`
- **Use Case:** `DeleteGameUseCase.cs`
- **Behavior:**
  1. Validates game exists and belongs to authenticated user
  2. Calls `_gameRepository.Delete(gameId)` — removes Game entity from EF tracker
  3. Calls `_unitOfWork.CommitAsync()`
- **Cascade Behavior (via PostgreSQL FK constraints):**
  - Game → Players: CASCADE DELETE (`PlayerMap.cs:33`)
  - Game → Plays: CASCADE DELETE (`PlayMap.cs:22`)
  - Player → Plays: CASCADE DELETE (`PlayMap.cs:27`)
- **Result:** Deleting a game removes the game, all its players, and all associated plays
- **Evidence:** `DeleteGameUseCase.cs`, `GameController.cs:135-158`, `PlayerMap.cs:33`, `PlayMap.cs:22,27`

### 1.2 Refresh Token Revocation (Logout)

- **Endpoint:** `POST /api/user/auth/logout`
- **Authorization:** `Admin, CommonUser, PremiumUser`
- **Use Case:** `LogoutUseCase.cs`
- **Modes:**
  1. **Single session:** Deletes the specific refresh token by hash
  2. **Full logout:** Deletes ALL refresh tokens for the user
- **Evidence:** `LogoutUseCase.cs:24-41`

### 1.3 Refresh Token Rotation (Implicit Deletion)

- **Endpoint:** `POST /api/user/auth/refresh`
- **Use Case:** `RefreshTokenUseCase.cs`
- **Behavior:** On valid refresh, old token is deleted and new one is created
- **Evidence:** `RefreshTokenUseCase.cs:53`

---

## 2. Deletion Mechanisms NOT Found

### 2.1 User Account Deletion

- **Status:** NOT IMPLEMENTED
- **Evidence:**
  - `UserController.cs` has only `POST` (create), `GET` (list), and `GET check-userinfo`
  - No `DELETE` endpoint exists
  - No `DeleteUserUseCase` class exists
  - No user-facing account deletion mechanism
- **Impact:** Users cannot exercise the right to delete their personal data through the application

### 2.2 Expired Refresh Token Cleanup

- **Status:** NOT IMPLEMENTED
- **Evidence:**
  - No background services, scheduled tasks, or cleanup jobs
  - Expired tokens that are never refreshed remain in the database indefinitely
  - `RefreshTokenUseCase.cs:50` rejects expired tokens but does not delete them

### 2.3 Game Info Cleanup

- **Status:** NOT IMPLEMENTED
- **Evidence:**
  - `GameInfo` entity has no FK relationships to User, Game, or Player
  - No delete endpoint for GameInfo records
  - Admin can only create, not delete

---

## 3. Cascade Delete Chain Analysis

```
User (users.id)
    ↓ CASCADE DELETE (if user deletion were implemented)
Game (games.owner_user_id)
    ↓ CASCADE DELETE
Player (players.game_id)
    ↓ CASCADE DELETE
Play (plays.player_id)

Game (games.id)
    ↓ CASCADE DELETE
Play (plays.game_id)

User (users.id)
    ↓ CASCADE DELETE
RefreshToken (refresh_tokens.user_id)
```

**Observation:** The cascade chain is complete for Game deletion. If User deletion were implemented, the cascade would correctly remove all associated data. However, User deletion is not implemented.

---

## 4. Third-Party Data Deletion

### 4.1 OpenRouter/OpenAI

- **Status:** NOT_VERIFIABLE
- **Evidence:** The application sends data to OpenRouter during play interactions. There is no mechanism to request deletion of data previously sent to the LLM provider. OpenRouter's retention policies are external to the codebase.
- **Note:** OpenRouter and OpenAI have their own data retention policies. The application has no control over or visibility into how long the provider retains prompts and completions.

### 4.2 Railway Hosting

- **Status:** NOT_VERIFIABLE
- **Evidence:** Railway is the deployment platform. Data deletion would depend on Railway's infrastructure and the organization's management of the deployment.

---

## 5. Observations

### 5.1 No Soft Delete

The application uses hard deletes only. There is no `IsDeleted` flag, `DeletedAt` timestamp, or archival mechanism on any entity. Once deleted, data cannot be recovered from the application layer.

### 5.2 No Data Export / Download

There is no mechanism for users to export or download their data. The only way to view data is through the API endpoints (game list, play history).

### 5.3 No User-Initiated Deletion

The only deletion a regular user can perform is:
1. Deleting a game (which cascades to players and plays)
2. Logging out (which revokes refresh tokens)

There is no way for a user to:
- Delete their account
- Delete specific plays
- Delete their profile data
- Request admin deletion

---

## 6. Evidence Summary

| Deletion Mechanism | Status | Evidence |
|-------------------|--------|----------|
| Game deletion (cascade) | IMPLEMENTED | `DeleteGameUseCase.cs`, cascade FKs |
| Refresh token revocation | IMPLEMENTED | `LogoutUseCase.cs` |
| Refresh token rotation | IMPLEMENTED | `RefreshTokenUseCase.cs` |
| User account deletion | NOT IMPLEMENTED | No endpoint, no use case |
| Expired token cleanup | NOT IMPLEMENTED | No background services |
| GameInfo cleanup | NOT IMPLEMENTED | No delete endpoint |
| Data export/download | NOT IMPLEMENTED | No mechanism |
| Third-party data deletion | NOT_VERIFIABLE | External to codebase |
