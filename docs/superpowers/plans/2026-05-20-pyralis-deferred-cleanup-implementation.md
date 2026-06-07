# Pyralis Deferred Cleanup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Finish the deferred implementation cleanup in Pickups, Feedback, and Scoring so the package is implementation-clean as well as assembly-clean.

**Architecture:** Keep the existing assembly cuts intact and clean the deferred slices by introducing narrow adapter seams instead of expanding feature runtime cores. Pickups, Feedback HUD, and Leaderboard UI should become thin implementations over already-established runtime services and contracts, with architecture tests enforcing that the deferred slices stay out of core runtime ownership.

**Tech Stack:** Unity, C#, VContainer, existing NeonBlack Gameplay asmdefs, editor architecture tests, `dotnet build` verification.

---

## File Structure

**Primary implementation files**

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/Collectible2D.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/3D/Collectible3D.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/CollectibleSpawner2D.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/CollectibleFeedback2D.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/PointPickup.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/PointPickupSpawner.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/PointPickupFeedback.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Feedback/ParticipantFeedbackRelay.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Feedback/ParticipantFeedbackService.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Feedback/UI/ParticipantHudFeedbackReceiver.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Feedback/UI/ParticipantHealthPanel.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Feedback/UI/ParticipantTimedTextPanel.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Scoring/LeaderboardManager.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Scoring/UI/LeaderboardScreen.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Scoring/2D/StillnessBonus2D.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

**Expected new seam files**

- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Contracts/IPickupAwardSink.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Contracts/IPickupSpawnSurface.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Contracts/IParticipantHudFeedbackStream.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Contracts/ILeaderboardQueryService.cs`

These new contracts should stay small and adapter-shaped. Do not create a second service framework; only introduce seams that remove direct implementation coupling.

---

### Task 1: Clean the Pickups Deferred Slice

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Contracts/IPickupAwardSink.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Contracts/IPickupSpawnSurface.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/Collectible2D.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/3D/Collectible3D.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/CollectibleSpawner2D.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/CollectibleFeedback2D.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/PointPickup.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/PointPickupSpawner.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Pickups/2D/PointPickupFeedback.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] Add `IPickupAwardSink` with one clear job: accept a collectible award payload and apply score/feedback side effects.
- [ ] Add `IPickupSpawnSurface` with one clear job: provide spawn positions and spatial rules without requiring `CollectibleSpawner2D` to reason like `GameManager`.
- [ ] Refactor `Collectible2D` and `Collectible3D` so collection resolves through `IPickupCollectible` plus the new award sink instead of embedding score/feedback assumptions in the collectible.
- [ ] Refactor `CollectibleSpawner2D` so camera bounds, participant-distance restrictions, and minimum-on-screen rules are grouped behind small helper methods or an injected/serialized spawn surface seam, not spread across the whole class.
- [ ] Decide alias strategy for `PointPickup*`: either make them thin wrappers over `Collectible*`/`CollectibleSpawner2D`/`CollectibleFeedback2D` or mark them for retirement and reduce them to compatibility shims.
- [ ] Update `DefinitionValidationTests.cs` to enforce that pickup runtime core remains under `Features/Pickups/Runtime`, while concrete collectible/spawner/feedback files remain explicitly deferred and singly owned.
- [ ] Run:
  - `dotnet build Neonblackinteractivellc.Neonblackhub.csproj --no-restore`
  - `dotnet build Neonblackinteractivellc.Neonblackhub.Tests.csproj --no-restore`

### Task 2: Separate Feedback Routing from HUD Rendering

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Contracts/IParticipantHudFeedbackStream.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Feedback/ParticipantFeedbackRelay.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Feedback/ParticipantFeedbackService.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Feedback/UI/ParticipantHudFeedbackReceiver.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Feedback/UI/ParticipantHealthPanel.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Feedback/UI/ParticipantTimedTextPanel.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] Add `IParticipantHudFeedbackStream` as a shaped stream for combo/status/score/combat-alert/health-facing data.
- [ ] Refactor `ParticipantFeedbackService` and `ParticipantFeedbackRelay` to own event shaping and participant routing, not HUD widget details.
- [ ] Refactor `ParticipantHudFeedbackReceiver` so it consumes the HUD stream and participant filter only; reduce direct assumptions about service internals and narrow any direct `HealthComponent` access behind one helper seam.
- [ ] Keep `ParticipantHealthPanel` and `ParticipantTimedTextPanel` dumb: they should render already-shaped values and timers, not resolve participant state.
- [ ] Update `DefinitionValidationTests.cs` so Feedback runtime ownership remains under `Features/Feedback/Runtime`, while UI and participant-HUD adapters stay explicitly deferred outside runtime core.
- [ ] Run:
  - `dotnet build Neonblackinteractivellc.Neonblackhub.csproj --no-restore`
  - `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.csproj --no-restore`
  - `dotnet build Neonblackinteractivellc.Neonblackhub.Tests.csproj --no-restore`

### Task 3: Clean the Deferred Scoring Slice

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Contracts/ILeaderboardQueryService.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Scoring/LeaderboardManager.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Scoring/UI/LeaderboardScreen.cs`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Scoring/2D/StillnessBonus2D.cs`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] Add `ILeaderboardQueryService` with one responsibility: asynchronously return leaderboard records for UI consumers.
- [ ] Refactor `LeaderboardManager` to implement that query contract while remaining outside the Scoring runtime asmdef.
- [ ] Refactor `LeaderboardScreen` so it consumes `ILeaderboardQueryService` instead of directly reaching for `LeaderboardManager.Instance`.
- [ ] Review `StillnessBonus2D` and remove any avoidable direct `GameManager` or broad gameplay coupling; leave only the minimum participant/round data it actually needs.
- [ ] Update `DefinitionValidationTests.cs` so `ParticipantScoreService` remains the only Scoring runtime core file, while leaderboard UI/manager and 2D bonus files stay explicitly deferred.
- [ ] Run:
  - `dotnet build Neonblackinteractivellc.Neonblackhub.csproj --no-restore`
  - `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.csproj --no-restore`
  - `dotnet build Neonblackinteractivellc.Neonblackhub.Tests.csproj --no-restore`

### Task 4: Normalize Deferred Folder and Namespace Ownership

**Files:**
- Modify: deferred files touched in Tasks 1-3 as needed
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/REFACTOR_WORKSPACE.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/SCENE_SETUP_GUIDE.md`
- Test: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/DefinitionValidationTests.cs`

- [ ] Align namespaces with actual ownership for every deferred file modified in Tasks 1-3.
- [ ] Make sure docs describe runtime core vs deferred adapter slices accurately for Pickups, Feedback, and Scoring.
- [ ] Add or update architecture checks in `DefinitionValidationTests.cs` for any new contract paths or deferred file locations introduced during cleanup.
- [ ] Run:
  - `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.csproj --no-restore`
  - `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.Tests.csproj --no-restore`

### Task 5: Final Verification and Regression Sweep

**Files:**
- Review: all modified files from Tasks 1-4

- [ ] Run the full package verification suite:
  - `dotnet build Neonblackinteractivellc.Neonblackhub.csproj --no-restore`
  - `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.csproj --no-restore`
  - `dotnet build Neonblackinteractivellc.Neonblackhub.Tests.csproj --no-restore`
  - `dotnet build Neonblackinteractivellc.Neonblackhub.Editor.Tests.csproj --no-restore`
- [ ] Review `DefinitionValidationTests.cs` one last time to ensure it enforces the final deferred boundaries rather than the pre-cleanup shape.
- [ ] Review docs for stale references to old ownership or direct singleton/service-locator patterns.
- [ ] Commit the cleanup in focused slices if execution is happening task-by-task.

---

## Acceptance Criteria

- Pickups concrete implementations no longer act as hidden orchestration centers.
- Participant feedback routing and HUD rendering are clearly separated.
- `LeaderboardScreen` no longer depends on direct singleton access to `LeaderboardManager`.
- Deferred file namespaces and docs match actual ownership.
- Architecture tests enforce the final deferred boundaries.
- All four `dotnet build` commands pass cleanly.
