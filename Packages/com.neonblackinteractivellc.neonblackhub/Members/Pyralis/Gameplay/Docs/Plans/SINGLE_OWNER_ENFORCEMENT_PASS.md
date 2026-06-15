# Single Owner Enforcement Pass

Goal: make core gameplay setup easier to explain by choosing one owner for each runtime responsibility, deleting or demoting competing owner paths, and updating authoring reflection so the chosen owner is visible.

Status: implemented through the runtime spine, feature-service policy, navigation handoff, participant input, 2D game-flow compatibility, source tests, and active architecture docs. Unity Test Runner remains the final gate for serialized scene/prefab behavior.

Architecture:

- `GameplaySessionBootstrap` is the Unity scene entrypoint and serialized handoff.
- `PyralisGameplayLifetimeScope` owns runtime service creation and registration.
- `SessionDefinition`, `GameModeDefinition`, `ParticipantDefinition`, and `PawnDefinition` own authored setup.
- `ParticipantDefinition.inputProfile` owns authored input.
- `ParticipantDefinition.defaultPawn -> PawnDefinition -> pawnPrefab` owns pawn spawning.
- `CinemachineCameraRigController` owns camera bounds and camera profile runtime.
- `ISceneNavigator` owns gameplay scene navigation; static scene loading is utility-only.
- Contracts and dependency reflection explain ownership to authoring; fallback wording does not become a second setup path.

## Phase 1: Runtime Service Ownership

Files:

- `Features/Platform/Session/GameplaySessionBootstrap.cs`
- `Features/Platform/Composition/PyralisGameplayLifetimeScope.cs`
- editor tests that assert service ownership wording and behavior
- active docs that describe Bootstrap/LifetimeScope ownership

Steps:

1. Move creation of `SessionStateService`, `ParticipantRosterService`, `ParticipantSpawnService`, and `ParticipantInputRouter` into `PyralisGameplayLifetimeScope`.
2. Keep Bootstrap as the scene object that passes `SessionDefinition`, spawn points, PlayerInputManager, camera rig, scene navigation, time, and camera shake references to the lifetime scope.
3. Keep authored service references as optional overrides only when they are serialized on Bootstrap.
4. Preserve networked service selection, but move type resolution/creation behind lifetime scope ownership.
5. Update contract language so Bootstrap no longer claims to auto-create services directly.

Acceptance:

- Bootstrap no longer calls `GetOrCreatePersistentService`.
- Lifetime scope resolves or creates core services before registration.
- Existing public startup behavior remains local-first and network-capable.

## Phase 2: Fallback Classification

Files:

- `Core/SceneNavigator.cs`
- `Core/SceneLoader.cs`
- `Core/Navigation/UI/SceneFader.cs`
- `Features/Platform/Composition/PyralisRuntimeFeatureServicePolicy.cs`
- authoring docs/tests

Steps:

1. Classify `SceneFader` as primary `ISceneNavigator`.
2. Classify `SceneLoader` as `DevFallback`.
3. Remove `SceneNavigator` from authoring-contract truth or demote it to utility-only with no setup guidance.
4. Replace keyword-based feature-service activation with contract/module evidence where possible.
5. Mark scene-component scanning as `MigrationLegacy` if it remains.

Acceptance:

- Authoring no longer promotes static scene navigation as a setup path.
- Feature service activation uses authored mode flags, resolved feature contracts, and actual scene components. Uncontracted module text/tags do not create runtime service behavior.
- Remaining fallbacks are named by category.

## Phase 3: Participant, Pawn, Input, And Game Flow

Files:

- `Data/Definitions/ParticipantDefinition.cs`
- `Data/Definitions/PawnDefinition.cs`
- `Features/Input/ParticipantInputRouter.cs`
- `Features/Characters/PawnRoot.cs`
- `Features/GameFlow/2D/GameManager.cs`
- `Features/Characters/Runtime/Shared/Services/ParticipantSpawnService.cs`

Steps:

1. Keep `ParticipantDefinition.inputProfile` as the only authored input owner.
2. Keep `ParticipantDefinition.defaultPawn -> PawnDefinition -> pawnPrefab` as the only normal pawn route.
3. Make `PawnRoot.pawnDefinition` explicitly standalone/dev-only if it remains.
4. Make `GameManager` prefer participant roster players; explicit player fields must be standalone-only or removed.
5. Ensure validation/authoring wording names those ownership decisions directly.

Acceptance:

- Authoring no longer suggests multiple normal input or pawn ownership locations.
- `ParticipantInputRouter` does not read `PlayerInputManager.instance`.
- `GameManager` no longer carries a separate serialized `player` GameObject path; explicit `playerControllers` are standalone compatibility only.
- Standalone fields are explicitly named as standalone/dev paths or removed.

## Phase 4: Authoring And Tests

Files:

- authoring contracts on touched runtime/data types
- `Editor/Authoring/Spine/*`
- `Tests/Editor/*`
- active docs under `Docs/Authoring`

Steps:

1. Update reflected contracts and native setup wording to match the single owners.
2. Add smoke tests for no stale owner paths in contracts/docs.
3. Ensure Hygiene can expose fallback/dependency pressure without turning fallback into setup truth.
4. Keep Map/Overview/Guide language beginner-focused and ownership-specific.

Acceptance:

- Tests protect the single-owner language.
- Docs and authoring tabs agree on owner paths.

## Phase 5: Verification

Commands:

```powershell
dotnet restore "NeonBlack.Gameplay.Editor.Tests.csproj"
dotnet build "NeonBlack.Gameplay.Editor.Tests.csproj" --no-restore
```

Unity gate:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Acceptance:

- CLI build passes.
- Cameron runs Unity EditMode and PlayMode tests, or Codex runs the full gate with Unity closed.
