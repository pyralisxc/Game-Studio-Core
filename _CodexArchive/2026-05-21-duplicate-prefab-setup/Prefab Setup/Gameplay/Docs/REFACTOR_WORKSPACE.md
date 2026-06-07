# NeonBlack Gameplay Refactor Workspace

This document is the active workspace for the NeonBlack Gameplay refactor.

It is meant to stay practical and current. When priorities or sequencing change, update this file.

## Refactor Status

**The original folder migration and the deferred-cleanup follow-through are complete.** The package is now in a maintenance and expansion phase: stronger tests, doc hygiene, and continued cleanup of historical compatibility edges.

This document is now a historical record of the refactor and a reference for the work rules and forward direction that replaced it.

## Active Documents

- `../README.md`
- `WORK_PRODUCT_EXPECTATIONS.md`
- `ARCHITECTURE_BLUEPRINT.md`
- `REFACTOR_WORKSPACE.md`
- `CURRENT_STATE_AUDIT.md`

These files should stay aligned. If one changes meaningfully, check whether the others also need an update.

## Current Working Assumptions

- The package-level structure is `Core/`, `Data/`, `Editor/`, `Features/`, `Networking/`, `Presentation/`, `Integrations/`, `Docs/`, `Samples/`, and `Tests/`.
- Feature folders may still contain historical `2D/` and `3D/` slices, while migrated feature runtimes increasingly use `Runtime/Shared`, `Runtime/2D`, and `Runtime/3D`.
- One-player and two-player support are implemented as configurations of an N-participant core.
- Future larger multiplayer goals matter and should continue to influence model boundaries.
- The Inspector should be the main authoring surface whenever practical.

## Immediate Refactor Priorities

### Phase 0: Standards And Planning

Deliverables:

- work product expectations,
- architecture blueprint,
- refactor roadmap,
- updated NeonBlack Gameplay README.

Status:

- completed for the initial documentation pass.
- runtime implementation is now in progress and the first shared-core slice is landed.

Current shipped pieces:

- `ParticipantId`
- `ParticipantHandle`
- `IParticipantRoster`
- `ParticipantRosterService`
- `ParticipantSpawnService`
- `SessionStateService`
- `GameplaySessionBootstrap`

### Phase 1: Participant Foundations

Goals:

- introduce a participant roster concept,
- stop assuming one active player,
- separate participant identity from pawn identity,
- define join/spawn ownership rules.

Expected outputs:

- participant-facing interfaces,
- roster service design,
- spawn model design,
- migration plan for existing single-player registries.

### Phase 2: Input Refactor

Goals:

- move input handling to per-participant ownership,
- prefer Unity Input System multiplayer-ready flows,
- support local solo and local multiplayer without separate controller families.

Expected outputs:

- input architecture doc updates,
- participant input profile design,
- replacement plan for single receiver and singleton input paths.

Status:

- partially implemented.

Current shipped pieces:

- `InputProfile`
- `ParticipantInputRouter`
- `SettingsManager` now supports multiple input receivers
- `Motor2DInputAdapter` now exposes the supported runtime action reassignment surface for 2D scenes

### Phase 3: Pawn Decomposition

Goals:

- split monolithic pawn scripts into modules,
- move mode-independent behavior into shared modules,
- define profile-driven pawn composition.

Expected outputs:

- target module list,
- first extraction candidates,
- migration notes for current player controllers.

Status:

- **complete for 3D brawler.** `PlayerActions` removed. `Motor3D` + four focused modules shipped.
- complete for the 2D arcade stack at the controller level: `Motor2D` is now a compatibility facade over `Pawn2DMovementComponent` and `Pawn2DPresentationComponent`.

Current shipped pieces:

- `PawnRoot`
- `IPawnMotor`, `IPawnCombatModule`, `IPawnTraversalModule`, `IPawnPresentationModule`
- `IFeatureModuleRuntime`
- `Motor3D` â€” 3D pawn coordinator; implements `ICharacterMotorState`
- `Pawn3DInputModule` â€” all Input System binding; produces `FrameInput` per frame
- `Pawn3DMovementComponent` â€” `BrawlerMovementModel` + `CharacterController`; implements `IPawnMotor`
- `Pawn3DTraversalComponent` â€” climb, hang, shimmy, ledge detection; implements `IPawnTraversalModule`
- `Pawn3DPresentationComponent` â€” Animator, billboard, land squash, debug HUD; implements `IPawnPresentationModule`
- `FrameInput` â€” per-frame input snapshot struct; produced by `Pawn3DInputModule`, consumed by all modules
- `Pawn2DMovementComponent` â€” 2D movement, dash, bounds, and reaction-lock ownership
- `Pawn2DPresentationComponent` â€” 2D presentation ownership
- `CockroachController.ApplyMovementProfile`, `ApplyPresentationProfile` â€” public profile API added

### Phase 4: Mode As Data

Goals:

- express mode identity through definitions and enabled modules,
- reduce adapter code to composition and migration logic,
- support hybrid modes cleanly.

Expected outputs:

- game mode definition assets,
- playfield and camera profile separation,
- feature-module composition rules.

Status:

- implemented at the authoring-model level, with runtime adoption still in progress.

Current shipped pieces:

- `SessionDefinition`
- `ParticipantDefinition`
- `PawnDefinition`
- `GameModeDefinition`
- `PlayfieldProfile`
- `CameraRigProfile`
- `SettingsProfile`
- `FeatureModuleDefinition`
- example authoring pack generator in the Editor

### Phase 5: Cleanup And Retirement

Goals:

- remove stale legacy paths,
- remove duplicate systems,
- update docs to match the new truth,
- archive deprecated patterns clearly.

Status:

- substantially complete.

Completed in this pass:

- reflection removed from the old legacy pawn bridge path before that path was retired,
- `GameConfig` restructured: `SessionDefinition` is now the preferred authoring entry point; `playerPrefab`, `defaultInputConfig`, and `enemyPrefabs` moved to a clearly labelled Legacy header,
- `SettingsManager` moved into `NeonBlack.Gameplay.Features.Settings` namespace,
- `SettingsManager` now accepts a `SettingsProfile` as its defaults source instead of hardcoded constants,
- `SettingsManager` now tracks `MasterVolume` via `AudioListener.volume`,
- `SettingsScreen` master slider now correctly calls `SetMasterVolume` and reads `MasterVolume`,
- all `SettingsManager.Instance` call sites updated with the new `using` directive,
- `SettingsMenu` settings duplication resolved â€” now delegates all volume changes to `SettingsManager`; duplicate AudioMixer and `Vol_*` PlayerPrefs keys removed,
- namespace declarations added to all runtime scripts; consumer `using` directives updated throughout,
- `LegacyBrawlerPawnBridge` removed,
- `LegacyArcadePawnBridge` removed from the supported architecture,
- `CameraShake` moved to canonical location `Presentation/Visuals/`; stale `Shared/Core/Runtime/` copy deleted,
- older `Runtime2D/`, top-level `Shared/`, and `Legacy/` folders fully deleted; feature-local `Runtime/Shared` slices remain valid,
- NGO extracted into a separate `NeonBlack.Gameplay.Networking` assembly; the three participant services are now NGO-free,
- `PlayerActions` (1,199 lines) removed â€” replaced by `Motor3D` (coordinator) + `Pawn3DInputModule`, `Pawn3DMovementComponent`, `Pawn3DTraversalComponent`, `Pawn3DPresentationComponent`; `FrameInput` struct introduced,
- `PlayerActionsEditor` (403 lines) removed; module components expose domain-specific Inspector fields,
- all callers (`GrabDetector`, `ClimbZone`, `GameplaySessionBootstrap`, `PlayerSpawner`) updated to reference `Motor3D`,
- all doc comments across `Combat/`, `Movement/`, and `Characters/` updated to reflect new component owners.

Remaining:

- `PlayerRegistry` static single-pawn fields remain; new scenes should route through `ParticipantRosterService`,
- integration tests: gameplay bootstrap, spawn, and combat coverage not yet written.

## Implemented Folder Structure

The folder direction from this workspace has been fully implemented:

- `Core/` — contracts, config, scene utilities, and platform service seams
- `Data/` â€” definitions and profiles
- `Editor/` â€” flat editor tooling
- `Features/Characters/` (+ `2D/`, `3D/`) â€” pawns, participants, bridges
- `Features/Combat/` (+ `2D/`, `UI/`)
- `Features/Encounters/` (+ `3D/`)
- `Features/Enemies/` (+ `3D/`)
- `Features/Environment/` (+ `3D/`)
- `Features/GameFlow/` (+ `2D/`)
- `Features/Hazards/` (+ `2D/`)
- `Features/Input/` (+ `2D/`)
- `Features/Pickups/` (+ `Runtime/`, `2D/`, `3D/`)
- `Features/Respawn/` (+ `3D/`)
- `Features/Scoring/` (+ `Runtime/`, `2D/`, `UI/`)
- `Features/Settings/` (+ `UI/`)
- `Features/Spawning/` (+ `3D/`)
- `Features/UI/`
- `Features/Zones/` (+ `3D/`)
- `Presentation/Camera/` (+ `2D/`, `3D/`) — shared camera infrastructure
- `Presentation/Visuals/` (+ `3D/`) — shared visual presentation utilities
- `Core/Navigation/` (+ `UI/`) — scene navigation and scene-flow helpers
- `Networking/` (+ `Characters/`) â€” optional NGO extension layer; separate assembly with no core reverse-dependency
- `Docs/` â€” internal notes

## Current High-Leverage Pain Points

These are the best early refactor seams because they unlock broader change:

- single-active-player registry patterns,
- single receiver settings push patterns,
- monolithic controller ownership,
- tag-based player discovery,
- data that only covers bindings and not full pawn behavior,
- duplicated settings or service logic across modes.

## Current Implemented Workspace Tools

- `Assets/Create/NeonBlack/Gameplay/Example Authoring Pack` generates a starter profile/definition asset set.
- `Tests/Runtime/ParticipantRuntimeTests.cs` covers basic participant registration and capacity behavior.
- `Tests/Editor/DefinitionValidationTests.cs` covers core profile and definition sanitization.

## Work Rules During Refactor

- Prefer enhancing a current tool over making a new one.
- Prefer shared modules over adapter forks.
- Prefer data assets over hardcoded branching when the behavior is a tuning or composition choice.
- Update docs in the same pass as architecture-affecting code.
- Remove stale notes rather than leaving contradictory history in active docs.
- Keep examples and tooltips useful to beginner-to-adept users.

## Definition Of A Good Refactor Slice

A refactor slice is strong when it:

- removes a real architectural assumption,
- improves shared reuse,
- keeps or improves Inspector authoring,
- narrows runtime responsibilities,
- makes future multiplayer support easier,
- does not require a rewrite of every dependent tool.

## Open Questions To Resolve In Later Design Passes

- How should participant seats, teams, and pawn ownership be represented in assets?
- Which current adapter systems are truly unique enough to remain mode-specific?
- Should camera remain custom, or should high-level follow/group behavior migrate toward Cinemachine?
- Which current controller systems should be extracted first for the best risk-to-value ratio?
- What minimum automated test coverage should be added alongside the first runtime refactor passes?

## Update Rule

Whenever the active refactor plan changes:

1. update this file,
2. update `ARCHITECTURE_BLUEPRINT.md` if the target shape changed,
3. update `README.md` if folder intent or package language changed,
4. remove or rewrite any stale guidance immediately.

