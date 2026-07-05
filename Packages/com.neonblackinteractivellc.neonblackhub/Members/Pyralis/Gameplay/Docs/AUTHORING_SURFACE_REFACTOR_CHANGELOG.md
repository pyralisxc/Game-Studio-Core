# Authoring Surface Refactor Changelog

This file records the package-wide authoring-surface refactor for review. It is intentionally separate from package release notes.

Scope rules:

- Do not modify `Assets/` during this lane unless Cameron explicitly asks.
- Do not auto-generate samples, scenes, prefabs, or ScriptableObject assets.
- Commit small verified slices as work proceeds.
- Prefer clean compatibility cuts when references allow.
- Keep PYS focused primarily on codebase quality, system health, ownership pressure, route semantics, topology, proof readiness, and cross-object setup truth.

## 2026-07-05

### Refactor Plan Checklist Accuracy

Changes:

- Updated the implementation plan checklist so completed baseline, inventory, living-doc, first-family selection, and runtime-created object classification steps match the current evidence.
- Left Unity Test Runner, manual Editor proof, sample authoring, and unfinished ownership consolidation unchecked.
- Kept this as a docs-only status correction.

Verification:

- Cross-checked the plan checklist against the current-status section and existing changelog evidence.
- Unity Test Runner was not run because this slice only corrects living-doc status.

### Feedback HUD Compatibility Note

Changes:

- Rechecked the Feedback HUD direct-label compatibility note.
- Updated the audit to state that direct TMP label compatibility is still current because `Members/Public/La Cucarachacha/Scenes/MainMenu.unity` serializes `statusLabel` on `ParticipantFeedbackHudPresenter`.
- Kept runtime code unchanged.

Verification:

- Scoped reference search confirmed `ParticipantFeedbackHudPresenter.statusLabel` is still serialized by the member scene.
- Scoped `git diff --check` passed for touched docs.
- Unity Test Runner was not run because this slice is docs-only.

### Unity Verification Queue

Changes:

- Added a focused Unity Editor verification queue to the refactor plan.
- Listed `GameplayWiringReportBuilderTests`, `FeedbackHudBindingTests`, the full `NeonBlack.Gameplay.Tests` EditMode pass, and the external `com.pys.authoring` package refresh as pending Editor proof.
- Kept shell checks scoped to commit hygiene rather than treating them as Unity proof.

Verification:

- Scoped `git diff --check` passed for touched docs.
- Unity Test Runner was not run from shell; the added queue names the Editor-side proof still required.

### Wiring Report Test Maintenance

Changes:

- Consolidated repeated complete-route test setup in `GameplayWiringReportBuilderTests`.
- Added helpers for creating complete session routes, creating participant definitions, and destroying temporary Unity objects.
- Kept runtime behavior unchanged.

Verification:

- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run from shell; run `GameplayWiringReportBuilderTests` in the Unity Editor Test Runner.

### Wiring Report Canonical Evidence Tests

Changes:

- Added focused runtime tests for `GameplayWiringReportBuilder`.
- Protected the current parity behavior where missing `SessionDefinition` setup is represented by one canonical `MissingProvider` row instead of a duplicate local validation row.
- Protected the route-deferral behavior where camera guidance waits while session route evidence is absent.
- Protected participant-route deferral so roster, spawn, input, camera guidance, and feature activation rows wait until the session asset has enough route evidence.
- Protected the matching route-complete behavior so concrete missing roster, spawn, and input providers appear once the session has a default game mode and participant route.
- Protected participant join timing evidence so PlayerInputManager with multiple auto-join participants reports one semantic `ParticipantJoinRoute` row.
- Protected authored combat feature activation evidence so complete combat routes report a `CombatServices` service activation row.
- Protected authored scoring feature activation evidence so complete scoring routes report `ScoringServices`, `GameFlowServices`, and `FeedbackServices` rows.
- Updated the plan, audit, and runtime wiring audit current-status sections to record this wiring-report proof slice.

Verification:

- Scoped code inspection confirmed `GameplayWiringReportBuilder` already suppresses the duplicate `GameplaySessionBootstrap.SessionDefinition.Missing` validation row and defers `GameplaySessionBootstrap.CameraRig.Optional` when session route evidence is unavailable.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run from shell; run `GameplayWiringReportBuilderTests` in the Unity Editor Test Runner.

## 2026-07-04

### Wiring Audit Status Accuracy

Changes:

- Updated `RUNTIME_WIRING_AUDIT.md` so it no longer says the next code slice is to create `GameplayWiringReport` from scratch.
- Clarified that the read-only report foundation and copy-menu path already exist, and the next wiring step is parity against current inspectors, validators, runtime warnings, and PYS exports.

Verification:

- Scoped code inspection confirmed `GameplayWiringReport`, `GameplayWiringReportBuilder`, `GameplayWiringReportTextFormatter`, and `Tools/NeonBlack/Gameplay/Wiring/Copy Selected Root Report` already exist.
- Scoped `git diff --check` passed for touched docs.
- Unity Test Runner was not run because this slice is docs-only.

### Package README Setup Guidance

Changes:

- Removed the stale package README claim that selecting `GameplaySessionBootstrap` exposes a `Setup Flow` foldout.
- Kept the supported setup path focused on opening PYS Authoring from the installed external package, then using Unity Inspector and Project-window authoring.

Verification:

- Scoped search found no gameplay-package bootstrap inspector or local `Setup Flow` implementation.
- Scoped `git diff --check` passed for touched docs.
- Unity Test Runner was not run because this slice is docs-only.

### Required Field Sweep Status

Changes:

- Updated the implementation plan and audit to record that the obvious numeric/tuning `RequiredFields` false-positive scan is clean.
- Clarified that remaining required-field hits are primarily identity, definitions, UI/prefab/component references, physics masks, or concrete route/service setup.

Verification:

- Scoped `RequiredFields` inventory confirmed no remaining obvious numeric/tuning required-field false positives matching range, priority, cooldown, damage, health, token, count, index, angle, reduction, speed, rate, duration, distance, height, grounded, or line-of-sight patterns.
- Scoped `git diff --check` passed for touched docs.
- Unity Test Runner was not run because this slice is docs-only.

### Movement And Detection Contract Accuracy

Changes:

- Removed `moveSpeed` from `Pawn2DMovementComponent` required PYS fields; movement speed remains covered by runtime validation.
- Removed unconditional required PYS fields from `EnemyDetectionModule` and added runtime validation for negative ranges, leash/aggro mismatch, and missing obstacle mask when line of sight is enabled.
- Removed required PYS fields from `IMovementModule`; it is an interface contract exposing runtime properties, not an Inspector-authored setup surface.
- Added an audit note clarifying that movement/detection tuning should be quality evidence while concrete physics setup remains owned by Unity components and masks.

Verification:

- Scoped code inspection confirmed `Pawn2DMovementComponent` already reports invalid move speed through `IRuntimeValidationProvider`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run from shell; run affected movement and enemy detection checks in the Unity Editor.

### Health Component Tuning Contract Accuracy

Changes:

- Removed `maxHealth` from `HealthComponent` required PYS fields; faction remains required as semantic combat identity.
- Added an audit note clarifying that health and regeneration values are component tuning, with invalid values reported through runtime validation.

Verification:

- Scoped code inspection confirmed `HealthComponent` already reports invalid max health, destroy delay, and regeneration settings through `IRuntimeValidationProvider`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run from shell; run affected combat-state checks in the Unity Editor.

### Enemy Attack Tuning Contract Accuracy

Changes:

- Removed `attackRange` and `aiPriority` from `EnemyAttack` required PYS fields; animation signal and hitbox zone remain the durable attack-meaning requirements.
- Added runtime validation for negative AI Priority so invalid attack-selection tuning is still reported as codebase quality evidence.
- Added an audit note clarifying that enemy attack range, damage, priority, weighting, and timing are tuning rather than setup references.

Verification:

- Scoped code inspection confirmed enemy attack range already falls back from hitbox range overrides or module/profile range overrides, and `EnemyCombatProcessor` clamps negative priorities during selection.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run from shell; run affected enemy combat checks in the Unity Editor.

### Pawn Combat Tuning Contract Accuracy

Changes:

- Removed `baseDamage` and `attackCooldown` from `PawnCombatProfile` required PYS fields; those numeric values are sanitized/profile-validated tuning, not missing setup references.
- Removed cooldown and aerial-count tuning from `PawnCombatBehaviour` required PYS fields while keeping authored combat sequences required.
- Removed `startingWeaponIndex`, `attackCooldown`, and `kickCooldown` from `PawnCombatBehaviour2D` required PYS fields while keeping hitbox, weapon-list, sequence, and projectile surfaces required.
- Removed combat token tuning from `CombatFlowController` required PYS fields and added runtime validation for negative token counts.
- Removed block reduction/angle tuning from `PawnBlockModule` required PYS fields and added runtime validation for invalid block ranges.
- Added an audit note clarifying that combat definitions and concrete combat surfaces are setup requirements, while numeric tuning should be validation evidence.

Verification:

- Scoped code inspection confirmed `PawnCombatProfile` sanitizes numeric tuning and reports invalid combat tuning through `IRuntimeValidationProvider`.
- Scoped code inspection confirmed `PawnCombatBehaviour` and `PawnCombatBehaviour2D` already validate missing action sequences and invalid cooldown/aerial values separately from PYS required fields.
- Scoped code inspection confirmed `CombatFlowController` and `PawnBlockModule` now report invalid numeric tuning through `IRuntimeValidationProvider`.
- Unity Test Runner was not run from shell; run affected combat checks in the Unity Editor.

### Level Session Contract Accuracy

Changes:

- Removed `ChosenSceneName` and `IsRandom` from `LevelSession` required PYS fields because they are static runtime navigation state, not Inspector-authored setup fields.
- Added an audit note clarifying that PYS may describe the level-selection route contract without treating transient handoff properties as authoring requirements.

Verification:

- Scoped reference search confirmed `ArcadeGameFlowController.Navigation` reads and updates `LevelSession` at runtime, and no serialized setup field backs those properties.
- Unity Test Runner was not run from shell; validate level-selection flow in the Unity Editor.

### Lifetime Scope Contract Accuracy

Changes:

- Removed `InjectLoadedScenesOnBuild` from `GameplayLifetimeScope` required PYS fields because it is a runtime property set by `GameplaySessionBootstrap`, not an Inspector-authored setup field.
- Added an audit note clarifying that PYS should require the visible lifetime-scope route anchor rather than the bootstrap handoff option.

Verification:

- Scoped reference search confirmed `GameplaySessionBootstrap` sets `GameplayLifetimeScope.InjectLoadedScenesOnBuild` before runtime configuration, and no serialized authoring field exists on `GameplayLifetimeScope` for that property.
- Unity Test Runner was not run from shell; validate the scene-root bootstrap/lifetime setup in the Unity Editor.

### Traversal Profile Contract Accuracy

Changes:

- Removed traversal capability toggles from `PawnTraversalProfile` required PYS fields; jump, dodge, climb, and hang are authored profile variation, not mandatory setup.
- Narrowed `Pawn3DTraversalComponent` required PYS fields to `traversalProfile`; climb/hang toggles, cooldown, and ledge probe settings are applied tuning owned by the component/profile.
- Added an audit note clarifying that traversal profile tuning is PYS-observable quality evidence, while concrete missing profile/component wiring stays in the Unity inspector path.

Verification:

- Scoped code inspection confirmed `Pawn3DTraversalComponent` applies `PawnTraversalProfile` into climb/hang/cooldown fields and the custom inspector reports missing profile/movement setup.
- Unity Test Runner was not run from shell; run affected traversal checks in the Unity Editor.

### Feedback Status Popup Ownership

Changes:

- Preserved `StatusEffectDefinition` payloads when `ActorFeedbackComponent.PublishStatusApplied(StatusEffectDefinition)` dispatches status feedback.
- Updated `ActorFloatingFeedbackReceiver` so status popups render either the authored status effect id or the string status value.
- Classified the Feedback HUD/popup scripts in the refactor audit as retained owners/adapters instead of open pass-through candidates.

Verification:

- Scoped reference search confirmed the status feedback path flows through `ActorFeedbackComponent`, `ActorFloatingFeedbackReceiver`, `ParticipantFeedbackRelay`, and `ParticipantFeedbackHudPresenter`.
- Unity Test Runner was not run from shell; run affected feedback tests in the Unity Editor.

### Text Flasher Unity-Native Cleanup

Changes:

- Updated `TextFlasher` to implement the same profile-backed `IVisualFlashPlayer` contract as `SpriteFlasher`.
- Added PYS authoring metadata and an Add Component menu entry for the text flash surface.
- Removed shared TMP material `_FaceColor` mutation; `TextFlasher` now drives only `TMP_Text.color` on assigned or auto-found text targets.
- Classified `SpriteFlasher` and `TextFlasher` as retained separate Unity target owners in the refactor audit.

Verification:

- Scoped search confirmed the shared material mutation path was removed from `TextFlasher`.
- Unity Test Runner was not run from shell; run affected presentation/feedback checks in the Unity Editor.

### Settings UI Contract Accuracy

Changes:

- Removed always-required PYS fields from `SettingsMenu`; settings services can be injected and individual controls are optional when a panel does not expose them.
- Narrowed `SettingsScreen` required fields to `_settingsPage`, the only field required for `Open()` to operate.
- Updated setup guidance so main-menu page, back button, controls, and settings source are described as authored/injected according to the actual UI surface.
- Classified `SettingsMenu` and `SettingsScreen` as separate retained owners in the refactor audit.

Verification:

- Scoped code inspection confirmed both settings components null-check optional controls and resolve settings services from injection or assigned `MonoBehaviour` sources.
- Unity Test Runner was not run from shell; run affected UI checks in the Unity Editor.

### Leaderboard UI Contract Accuracy

Changes:

- Narrowed `LeaderboardScreen` required PYS fields to the leaderboard page, row container, and row prefab.
- Changed missing back button and status label validation from required to recommended.
- Removed main-menu page from required validation because the screen already supports externally owned page routing.
- Updated setup guidance and audit notes to classify `LeaderboardScreen` as a Scoring-owned UI presenter.

Verification:

- Scoped code inspection confirmed `Open()`, `Close()`, and status updates null-check optional page, button, and label references.
- Unity Test Runner was not run from shell; run affected UI checks in the Unity Editor.

### Navigation UI Contract Accuracy

Changes:

- Narrowed `MainMenuController` required PYS fields to `gameSceneName`; panels, buttons, and navigator references are route-specific and already null-checked or runtime-settable.
- Removed optional progress bar and label from `LoadingScreenController` required PYS fields.
- Updated setup guidance and audit notes to keep navigation UI owned by scene-flow routes instead of treating every visible control as package-mandatory setup.

Verification:

- Scoped code inspection confirmed `MainMenuController` null-checks optional buttons/panels and can receive an `ISceneNavigator` through `SetSceneNavigator`.
- Scoped code inspection confirmed `LoadingScreenController` null-checks progress and label UI before updating them.
- Unity Test Runner was not run from shell; run affected navigation UI checks in the Unity Editor.

### Game Flow HUD Contract Accuracy

Changes:

- Narrowed `GameFlowHudController` required PYS fields to the live score and time labels.
- Rephrased setup guidance so panels, game-over labels, buttons, and settings references are route-owned optional UI surfaces.
- Added an audit note classifying `GameFlowHudController` as a focused live HUD presenter when those optional route surfaces are absent.

Verification:

- Scoped code inspection confirmed panel, button, settings, and game-over label references are null-checked before use.
- Unity Test Runner was not run from shell; run affected HUD checks in the Unity Editor.

### RPG Dialogue UI Contract Accuracy

Changes:

- Narrowed `RpgDialoguePanelPresenter` required PYS fields to authored dialogue graphs and the line label.
- Updated setup guidance so route presenter, NPC profiles, speaker labels, choice labels, and issue labels match the existing runtime fallback/optional behavior.
- Called the existing `EnsureService()` fallback before dialogue start, continue, choice selection, and choice refresh so standalone authored panels do not dereference a missing `DialogueService`.
- Added an audit note classifying the dialogue presenter as the retained dialogue panel owner.

Verification:

- Scoped code inspection confirmed `routePresenter` is auto-resolved from parent/child hierarchy and NPCs can fall back from graph speaker ids.
- Scoped search confirmed `EnsureService()` is now called before `_dialogueService` dialogue operations in `RpgDialoguePanelPresenter`.
- Unity Test Runner was not run from shell; run affected RPG UI checks in the Unity Editor.

### RPG Panel Action Service Fallbacks

Changes:

- Updated `RpgVendorPanelPresenter` to call its service fallback before buy/sell actions.
- Updated `RpgLoadoutPanelPresenter` to call its service fallback before equip/unequip actions.
- Added and used a quest service fallback in `RpgQuestBoardPanelPresenter` before starting quests.
- Updated `RpgSkillTreePanelPresenter` to call its service fallback before unlock actions.
- Added an audit note for RPG panel action ownership and service fallback expectations.

Verification:

- Scoped search confirmed the affected action methods now call their service fallback before service dereferences.
- Unity Test Runner was not run from shell; run affected RPG UI checks in the Unity Editor.

### UI Orientation Contract Accuracy

Changes:

- Removed misleading `portrait`/`landscape` required PYS fields from `UIOrientationHandler`; those serialized layout objects are always present.
- Added runtime validation for the actual setup requirement: portrait and landscape layouts must be captured.
- Added an audit note for UI orientation ownership and the inspector capture workflow.

Verification:

- Scoped code inspection confirmed layout application already ignores uncaptured layouts, and the custom inspector reports capture status.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run from shell; verify orientation capture behavior in the Unity Editor.

### Baseline Planning And Audit

Commits:

- `559bf90b` - `docs: plan codebase authoring surface refactor`
- `82914e22` - `docs: audit gameplay authoring surface ownership`
- `cc5ca80f` - `docs: define package-wide authoring surface direction`
- `9a3f2f28` - `docs: codify authoring surface refactor defaults`
- `53bbb15d` - `docs: keep agent guidance project-level`

Changes:

- Added the phased package-wide refactor plan.
- Added the ownership audit that classifies current code by owner/profile/adapter/pass-through/validator/presentation/scene-service/sample-only pressure.
- Updated living docs so the authoring-surface direction is package-wide rather than pawn-only.
- Updated `AGENTS.md` with durable project-management rules only; sequence-specific refactor guidance stays in the refactor plan and audit.

Verification:

- Scoped `git diff --check` passed for touched docs.
- Unity Test Runner was not run because these changes were docs-only.

Notes:

- Existing unrelated local/generated changes remain outside this refactor lane: `.plastic/*`, `Game Studio Core.slnx`, and `Tools/Validation/Run-PreSceneValidation.ps1`.

### Feedback HUD Health Binding Ownership

Changes:

- Simplified `ParticipantHealthHudBinder` so it only binds tracked participant health into `ParticipantHealthPanel` surfaces.
- Removed duplicate direct label/fill image ownership from the binder.
- Updated the binder's PYS contract metadata to require `ParticipantHealthPanel` instead of direct UI fields.
- Added runtime tests for binder validation with and without a child `ParticipantHealthPanel`.
- Added the Feedback module reference to the runtime test assembly.

Verification:

- Scoped `git diff --check` passed for touched files.
- Confirmed `Neonblackinteractivellc.Neonblackhub.Tests.asmdef` parses as JSON.
- Confirmed `ParticipantHealthHudBinder` no longer references `healthLabel`, `healthFillImage`, `TextMeshProUGUI`, or `UnityEngine.UI`.
- Unity Test Runner was not run from the shell because Unity is not available on `PATH`; run the affected runtime tests in the Unity Editor.

Unity proof still required:

- Run `FeedbackHudBindingTests` from Unity Test Runner.

Follow-up fix in same family:

- Updated `FeedbackServiceInstaller` so all `ParticipantHudTargetBinding` components receive the participant roster, including `ParticipantHealthHudBinder`.
- Kept feedback stream wiring on `ParticipantFeedbackHudPresenter`.
- Deferred cutting direct labels from `ParticipantFeedbackHudPresenter` because `Members/Public/La Cucarachacha/Scenes/MainMenu.unity` currently serializes `_statusLabel`.

Additional verification:

- Scoped `git diff --check` passed for the installer change.
- No Unity Test Runner execution from shell; run affected runtime tests in Unity.

### Actor Floating Feedback Contract Accuracy

Changes:

- Removed optional `damageNumberSink` and `popupCamera` from `ActorFloatingFeedbackReceiver` required PYS fields.
- Kept setup guidance, but phrased both fields as optional/runtime-configurable depending on enabled feedback categories.

Verification:

- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata/setup guidance only.

### Actor Shadow Driver Contract Accuracy

Changes:

- Removed runtime-resolved shadow fields from `ActorShadowDriver` PYS required fields.
- Rephrased setup steps so authored blob/model references are optional when the automatic presentation stack can resolve or create the shadow output.
- Downgraded validation for missing runtime-applied presentation profile and missing authored shadow renderers from required to recommended.

Verification:

- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this changes validation severity and contract metadata; visual behavior still requires Unity proof.

### Scene Flow Runtime Overlay Contract Accuracy

Changes:

- Kept both `SceneLoader` and `SceneFader` because member scenes currently serialize references to each service.
- Removed fade-duration settings from required PYS fields; they are tunable defaults, not missing setup.
- Updated setup guidance to state that fade canvas/overlay objects are created at runtime by the navigation service.

Verification:

- Reference search found `SceneLoader` serialized in `Members/Public/Apocalyptia/Scenes/MainMenu.unity` and `SceneFader` serialized in `Members/Public/La Cucarachacha/Scenes/MainMenu.unity`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata/setup guidance only.

### Collectible Feedback AudioSource Ownership

Changes:

- Added `RequireComponent(typeof(AudioSource))` to `CollectibleFeedback2D`.
- Removed runtime `AudioSource` creation from `CollectibleFeedback2D.Awake`; the AudioSource is now an authored Unity component that can be routed to the SFX mixer in the Inspector.
- Updated validation to report a missing `AudioSource` directly instead of only checking mixer routing when one already exists.

Verification:

- Reference search found no serialized member scene/prefab references to `CollectibleFeedback2D`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because Unity is not available on `PATH`; run collectible interaction tests/proofs in the Unity Editor.

### Pawn 2D Presentation Optional Audio Ownership

Changes:

- Removed automatic `AudioSource` creation from `Pawn2DPresentationComponent`.
- Kept dash/death clips optional; playback now no-ops when no authored AudioSource exists instead of creating hidden setup at runtime.
- Added validation that reports a missing pawn-root `AudioSource` only when dash or death clips are assigned.

Verification:

- Reference search found no serialized member scene/prefab references to `Pawn2DPresentationComponent`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because Unity is not available on `PATH`; run affected pawn presentation tests/proofs in the Unity Editor.

### Hazard Optional Audio Ownership

Changes:

- Removed automatic `AudioSource` creation from `HazardRuntimeReferences`.
- Kept hazard audio optional; hazard SFX playback already no-ops when no authored AudioSource exists.
- Added validation when `HazardData` assigns audio clips but the hazard prefab root has no `AudioSource`.

Verification:

- Reference search found member scenes serialize `HazardSpawner`, not direct `Hazard` components.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because Unity is not available on `PATH`; run affected hazard/spawner tests and Play Mode hazard proofs in the Unity Editor.

### Collectible Contract Required Field Accuracy

Changes:

- Removed bob animation tuning values from `Collectible2D` required PYS fields; collider setup is already expressed through Unity's `RequireComponent` plus runtime validation.
- Narrowed `CollectibleSpawner2D` required PYS fields to `_crumbPrefab`; pool size, initial counts, intervals, minimum count, and spawn margin are tunable defaults.

Verification:

- Reference search found no serialized member scene/prefab references to `Collectible2D`, `CollectibleSpawner2D`, or `ActorPickupCollector2D`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Scoring Contract And Optional Audio Accuracy

Changes:

- Removed UnityEvent fields from `ParticipantScoreService` required PYS fields; they are runtime event surfaces, not setup inputs.
- Removed numeric/default tuning and optional bonus clip from `StillnessBonus2D` required PYS fields.
- Removed automatic `AudioSource` creation from `StillnessBonus2D`; bonus audio now requires an authored AudioSource only when Bonus Clip is assigned.

Verification:

- Reference search found member scenes using older `ScoreManager`/`StillnessReward` identifiers, not serialized `ParticipantScoreService` or `StillnessBonus2D`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because Unity is not available on `PATH`; run affected scoring/stillness proofs in the Unity Editor.

### HitBox2D Optional Feedback Ownership

Changes:

- Removed optional `owner`, `weapon`, `hitFXPrefab`, `hitSFX`, and `hitPauseSink` fields from `HitBox2D` required PYS fields.
- Removed runtime `AudioSource` creation from `HitBox2D`; hit SFX now requires an authored AudioSource when used.
- Added a direct runtime error when Hit SFX is assigned without an AudioSource.

Verification:

- Reference search found no serialized member scene/prefab references to `HitBox2D`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because Unity is not available on `PATH`; run affected combat/hitbox proofs in the Unity Editor.

### Living Docs Ownership Refresh

Changes:

- Updated the package README folder table so it matches the actual gameplay package folders and current ownership boundaries.
- Removed the stale `Integrations/` folder row and old `Core` wording that implied VContainer, scene loading, and config ownership lived there.
- Rephrased quick-start guidance so Unity-native scene/prefab/profile setup remains primary and PYS Authoring is framed as evidence, route truth, proof readiness, and cross-object setup projection.
- Updated the refactor audit so already-cleaned optional audio owners and scene-flow runtime overlays are no longer listed as unresolved setup-repair pressure.
- Clarified that samples in this lane are Cameron-authored, not agent-generated.

Verification:

- Scoped `git diff --check` passed for touched docs.
- Unity Test Runner was not run because this is docs-only.

### Actor Shadow Authored Output Ownership

Changes:

- Removed the generic runtime `RuntimeShadow` GameObject and `SpriteRenderer` creation path from `ActorShadowDriver`.
- Kept profile `shadowPrefab` instantiation as authored runtime output.
- Preserved authored shadow renderer references separately from runtime prefab instances.
- Added validation that blob-shadow mode needs either an authored child `SpriteRenderer` or a profile `shadowPrefab`.

Verification:

- Reference search found no serialized member scene/prefab references to `ActorShadowDriver`.
- Scoped scan confirmed `ActorShadowDriver` no longer contains `RuntimeShadow`, `new GameObject`, or `AddComponent<SpriteRenderer>`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because Unity is not available on `PATH`; run affected presentation proofs in the Unity Editor.

### Participant Feedback HUD Contract Accuracy

Changes:

- Removed direct TMP label fields from `ParticipantFeedbackHudPresenter` required PYS fields.
- Kept direct labels and timed panels as supported alternative output surfaces.
- Rephrased setup guidance so child `ParticipantTimedTextPanel` surfaces satisfy the HUD feedback contract.

Verification:

- Reference search found `statusLabel` serialized in `Members/Public/La Cucarachacha/Scenes/MainMenu.unity`, so direct label fields were preserved.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata/setup guidance only.

### Participant Feedback Service Stream Ownership

Changes:

- Removed legacy inspector `UnityEvent` channels from `ParticipantFeedbackService`.
- Kept `FeedbackPublished` as the single participant feedback stream consumed by HUD presenters.
- Kept publish helper methods as typed code entrypoints over `ParticipantFeedbackMessage`.

Verification:

- Reference search found the removed UnityEvent fields were only referenced inside `ParticipantFeedbackService`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because Unity is not available on `PATH`; run affected feedback HUD proofs in the Unity Editor.

### Flasher Optional Event And Contract Cleanup

Changes:

- Removed unused private completion `UnityEvent` fields from `SpriteFlasher` and `TextFlasher`.
- Removed `SpriteFlasher` required PYS fields for renderer list, default profile, and play-on-start toggle because auto-find and explicit profile calls are valid setup paths.
- Kept profile-driven flash playback behavior unchanged.

Verification:

- Reference search found the removed flasher completion events were only referenced inside their owning flasher scripts.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because Unity is not available on `PATH`; run affected presentation/feedback proofs in the Unity Editor.

### Profile Numeric Required Field Accuracy

Changes:

- Removed `duration` from `StatusEffectDefinition` required PYS fields; identity fields remain required and duration is still range-validated.
- Removed `duration`, `height`, and `cooldown` from `TopDownHopProfile` required PYS fields; the action role remains the authored meaning field and numeric values remain sanitized.
- Removed `cooldown` and `burstCount` from `FireModeDefinition` required PYS fields; fire mode identity remains required and cadence values remain sanitized.

Verification:

- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Health Component Contract Accuracy

Changes:

- Removed `iFrameDuration` from `HealthComponent` required PYS fields; Max Health and Faction remain the authored setup identity.
- Rephrased feedback setup guidance so optional UnityEvents are not presented as the required feedback path.
- Preserved serialized `OnDamaged`, `OnHealed`, and `OnDeath` UnityEvent fields because member prefabs currently serialize them.

Verification:

- Reference search found `HealthComponent` UnityEvents serialized in member prefabs, so event fields were preserved.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata/setup guidance only.

### Runtime Output Classification Refresh

Changes:

- Updated the refactor audit to classify `WorldHealthBar` generated world-space UI as valid runtime presentation output.
- Updated the refactor audit to classify `TabletopBoardGridPresenter` generated board space/piece views as valid runtime board output from authored definitions.
- Kept `Modules/Spawning/Runtime/Rigged3D/Spawner.cs` in needs-review because sprite-to-GameObject spawning remains prototype utility pressure rather than the main authored package path.

Verification:

- Inspected `WorldHealthBar.Canvas.cs` and `TabletopBoardGridPresenter.cs` before classifying their generated objects.
- Scoped `git diff --check` passed for touched docs.
- Unity Test Runner was not run because this is docs-only classification.

### Utility And Traversal Contract Accuracy

Changes:

- Removed numeric traversal tuning fields from `PawnTraversalProfile` required PYS fields; feature toggles remain as the authored capability choice, while numeric values remain sanitized/validated.
- Removed camera fade tuning fields from `CameraOcclusionFader` required PYS fields; target and occlusion mask remain the setup references.
- Removed `Spawner` required PYS fields because prefab and sprite sources are alternative inputs and the runtime validator already reports when neither source exists.

Verification:

- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Tabletop Move Policy Contract Accuracy

Changes:

- Removed `maxDistance` from `BoardMovePolicyDefinition` required PYS fields; identity and shape remain required setup meaning.
- Kept `maxDistance` range validation in the local validation provider.

Verification:

- Scoped reference search found no serialized member scene/prefab/asset references that required a compatibility field change.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Collectible 3D Contract Required Field Accuracy

Changes:

- Removed bob animation tuning values from `Collectible3D` required PYS fields, matching the earlier `Collectible2D` contract cleanup.
- Kept collider setup expressed through Unity's `RequireComponent` and runtime validation.

Verification:

- Reference search found no serialized member scene/prefab/asset references to `Collectible3D`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Collectible Feedback 2D Contract Required Field Accuracy

Changes:

- Removed optional destroy feedback clip and particle references from `CollectibleFeedback2D` required PYS fields.
- Updated setup guidance so collection feedback is the required proof path and destroy feedback is explicitly optional.
- Kept runtime behavior unchanged.

Verification:

- `CollectibleFeedback2D.Validation.cs` already required collect clip, collect particle system, and audio setup, but did not require destroy feedback.
- Reference search found no serialized member scene/prefab/asset references to `CollectibleFeedback2D`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Tabletop Runtime Contract Required Field Accuracy

Changes:

- Narrowed `TabletopBoardGridPresenter` required PYS fields to `boardDefinition`.
- Reframed move policy, turn order, selection controller, space prefab, and piece prefab as optional setup inputs.
- Removed the boolean setup toggle from `TabletopBoardSelectionController` required PYS fields.
- Removed constructor-owned private runtime collections from `TabletopActionQueueService` required PYS fields.
- Kept runtime behavior unchanged.

Verification:

- `TabletopBoardGridPresenter` runtime validation already treats board definition and selection bridge as required, while treating missing visual prefabs as recommended fallback output.
- `TabletopActionQueueService` creates pending action and resolver collections in its constructor.
- Reference search found no serialized member scene/prefab/asset references to the tabletop presenter or selection controller.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Profile Required Field Accuracy

Changes:

- Narrowed `SettingsProfile` required PYS fields to the audio mixer reference.
- Removed default toggles, layer masks, intervals, radii, and sanitized numeric tuning from profile required PYS fields in `PickupProfile`, `EnemyAmbientProfile`, `InteractionProfile`, `ActorStatusEffectProfile`, and `ActorCombatReactionProfile`.
- Narrowed `HazardImpactProfile` required PYS fields to the authored `effectId`; damage and tick tuning remain sanitized/validated by the profile.
- Kept runtime behavior unchanged.

Verification:

- Profile validation either requires only the object reference/identity field or sanitizes numeric values through `OnValidate`.
- `PickupProfile` and `InteractionProfile` expose no runtime validation issues, so their default toggles are not required setup truth.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Presentation Feedback Profile Required Field Accuracy

Changes:

- Narrowed `PawnPresentationProfile` required PYS fields to the authored presentation mode lane.
- Removed optional HUD prefab, tint, flash profile, event-toggle, and reaction-timing defaults from presentation/feedback profile required PYS fields.
- Kept conditional validation in `HazardFeedbackProfile`, `ActorFeedbackProfile`, and `EnemyReactionProfile` as the source of actual profile health.
- Kept runtime behavior unchanged.

Verification:

- `HazardFeedbackProfile` already validates flash profile references only when the corresponding flash toggle is enabled.
- `ActorFeedbackProfile` validates the "all outputs disabled" case rather than requiring specific event toggles.
- `EnemyReactionProfile` sanitizes/validates timing ranges rather than requiring specific default values.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Camera And Billboard Contract Accuracy

Changes:

- Narrowed `CameraRigProfile` required PYS fields to the presentation and focus lane choices.
- Removed `CameraShake` required PYS fields because it can shake its own transform when no explicit target is assigned.
- Narrowed `BillboardFacing3D` required PYS fields to target and camera references; mirroring and sprite fields remain optional presentation outputs.
- Corrected the `BillboardFacing3D` validation message so it no longer claims an unimplemented main-camera fallback.
- Narrowed `CameraZone` required PYS fields to enter profile and player tag; exit profile and transition duration are optional route behavior.
- Kept runtime behavior unchanged.

Verification:

- `CameraShake.ResolveTarget` falls back to the component transform when no target is assigned.
- `BillboardFacing3D.ApplyBillboard` requires an active camera and target before applying facing.
- `CameraZone` skips exit switching when no exit profile is assigned.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is metadata/message cleanup only.

### Route Policy Required Field Accuracy

Changes:

- Removed `ParticipantInputRouter.autoRegisterDefaultParticipantsWithoutPlayerInput` from required PYS fields; injected session and roster references remain validated as recommended standalone setup concerns.
- Removed `NetworkedSessionStateService` pseudo-fields from required PYS metadata because network mode and auto-start policy live on `SessionDefinition`.
- Removed projectile pooling policy defaults from `ProjectileLauncherBase` required PYS fields.
- Narrowed `SessionDefinition` required PYS fields to session name, default game mode, and default participants; network mode and participant limit remain sanitized/validated.
- Kept runtime behavior unchanged.

Verification:

- `ParticipantInputRouter` runtime validation already reports injected dependencies as recommended, not required.
- `NetworkedSessionStateService` reads network startup policy from `ActiveSessionDefinition`.
- `ProjectileLauncherBase` can operate with pooling disabled and clamps pool size at return time.
- `SessionDefinition` runtime validation still checks participant limits and local/network consistency.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Movement And Enemy Profile Required Field Accuracy

Changes:

- Narrowed `PawnMovementProfile` required PYS fields to movement mode and 2D movement style lane choices.
- Narrowed `PlayfieldProfile` required PYS fields to movement mode; bounds remain sanitized/validated and only become meaningful when bounds or wrapping are enabled.
- Removed default enemy attack mode from `EnemyCombatProfile` required PYS fields; the attack sequence remains required.
- Kept runtime behavior unchanged.

Verification:

- `PawnMovementProfile` sanitizes movement tuning values through `OnValidate`.
- `PlayfieldProfile` validates bound ordering rather than requiring specific default bound values.
- `EnemyCombatProfile` runtime validation requires a non-empty attack sequence and treats attack mode as a default selection policy.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Pawn Presentation Component Required Field Accuracy

Changes:

- Removed `Pawn3DPresentationComponent.showDebugHUD` from required PYS fields.
- Narrowed `Pawn2DPresentationComponent` required PYS fields to the sprite renderer reference; tint, tilt, and squash/stretch values remain presentation tuning.
- Kept runtime behavior unchanged.

Verification:

- `Pawn3DPresentationComponent` runtime validation requires an `IActorAnimationController`, not the debug HUD toggle.
- `Pawn2DPresentationComponent.Validation.cs` validates that a `SpriteRenderer` can be resolved directly or from a child.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Route-Conditional Definition Required Field Accuracy

Changes:

- Narrowed `ProjectileDefinition` required PYS fields to projectile id and display name; prefab and speed remain required only for projectile-prefab delivery.
- Narrowed `WeaponData` required PYS fields to weapon name; projectile definitions and hitbox zones remain validated based on weapon type.
- Narrowed `ParticipantDefinition` required PYS fields to display name; pawn, input profile, and team setup remain route-dependent authoring choices.
- Kept runtime behavior unchanged.

Verification:

- `ProjectileDefinition` runtime validation requires prefab and speed only when `deliveryMode` is `ProjectilePrefab`, and max distance only for hitscan delivery.
- `WeaponData` runtime validation requires projectile definition for ranged/thrown weapons and hitbox zone for melee weapons.
- `ParticipantDefinition` setup guidance already states input profile and pawn are conditional by route.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Enemy Runtime Contract Required Field Accuracy

Changes:

- Narrowed `EnemyMovementModule` required PYS fields to movement mode and ground layer; gravity and ground-check radius remain movement tuning.
- Narrowed `EnemyAI` required PYS fields to `enemyProfile`; move speed is tuning and patrol points are optional because the AI can use random patrol targets.
- Narrowed `EnemySpawner` required PYS fields to enemy prefabs; spawn points fall back to the spawner transform and spawn mode has a default.
- Kept runtime behavior unchanged.

Verification:

- `EnemyAI.GetPatrolTarget` falls back to a random patrol target when no patrol points are assigned.
- `EnemySpawner.TryPickSpawnOrigin` falls back to the spawner transform when no valid spawn point exists.
- `EnemySpawner.Start` already guards against missing enemy prefabs before spawning.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Pawn 2D Movement Contract Required Field Accuracy

Changes:

- Narrowed `Pawn2DMovementComponent` required PYS fields to movement style, move speed, and ground layer.
- Removed dash toggle, dash tuning, jump toggle, jump tuning, and input zone references from unconditional required fields.
- Kept runtime behavior unchanged.

Verification:

- `Pawn2DMovementComponent.Validation.cs` already requires dash values only when dash is enabled and jump values only for side-view jump routes.
- `Pawn2DMovementComponent.Bounds.cs` treats `inputZones` as optional and only consults it when assigned.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Scene Service Contract Required Field Accuracy

Changes:

- Removed unconditional spawn point required metadata from `ParticipantSpawnService`; spawn points remain conditionally required when `spawnOnRegister` is enabled.
- Narrowed `SplashScreenController` required PYS fields to the next scene name; black overlay and video fields remain optional splash presentation.
- Kept runtime behavior unchanged.

Verification:

- `ParticipantSpawnService.GetRuntimeValidationIssues` already reports missing spawn points only when `spawnOnRegister` is enabled.
- `SplashScreenController` explicitly supports a static fallback path when video fields are empty, and skips fade when no black overlay exists.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Projectile Planner Contract Accuracy

Changes:

- Removed request-property `RequiredFields` metadata from `ProjectileFirePlanner`.
- Kept planner setup guidance focused on configuring `FireModeDefinition` and calling `BuildCommands` from weapon/action logic.
- Kept runtime behavior unchanged.

Verification:

- `ProjectileFirePlanner` is a static planner with no serialized Unity fields.
- `BuildCommands` handles a missing projectile by returning an empty command array and resolves missing direction to `Vector3.forward`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Enemy Combat Module Contract Ownership

Changes:

- Narrowed `EnemyCombatModule` required PYS fields to the combat profile and hitbox zones.
- Removed profile-owned attack sequence and attack mode fields from the module's unconditional required metadata.
- Kept runtime behavior unchanged.

Verification:

- `EnemyCombatModule.ApplyCombatProfile` copies attack sequence, attack mode, selection policy, cooldowns, range, and weighting from `EnemyCombatProfile`.
- `EnemyCombatProfile` already owns attack-sequence validation.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### RPG Item Definition Contract Accuracy

Changes:

- Narrowed `ItemDefinition` required PYS fields to item id, display name, and category.
- Removed rarity, max stack size, and tags from unconditional required metadata.
- Kept runtime behavior unchanged.

Verification:

- `ItemDefinition.GetValidationIssues` requires item id, display name, category, and validates max stack size range.
- `ItemDefinition.Sanitize` defaults rarity and clamps max stack size to at least 1; tags are optional and normalized when present.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Pawn 3D Movement Contract Required Field Accuracy

Changes:

- Narrowed `Pawn3DMovementComponent` required PYS fields to movement mode and ground layer.
- Removed walk speed and jump height from unconditional required metadata because those are default/profile-owned tuning values.
- Kept runtime behavior unchanged.

Verification:

- `Pawn3DMovementComponent` has nonzero serialized defaults for walk speed and jump height.
- `Pawn3DMovementComponent.Profiles.cs` applies movement speed from `PawnMovementProfile` and jump tuning from `PawnTraversalProfile`.
- Scoped `git diff --check` passed for touched files.
- Unity Test Runner was not run because this is contract metadata only.

### Plan And Audit Progress Consolidation

Changes:

- Added a current-status section to the implementation plan so unfinished checklist items do not imply no work has happened.
- Added a current-progress section to the authoring surface audit summarizing completed cleanup families and remaining proof.
- Kept refactor-specific sequencing out of `AGENTS.md`.

Verification:

- Confirmed Unity version remains `6000.4.0f1`.
- Confirmed PYS Authoring remains external in `Packages/manifest.json`.
- Scoped `git diff --check` passed for touched docs.
- Unity Test Runner was not run because this is docs-only consolidation.

### PYS Authoring Package Path Verification

Changes:

- Corrected the `com.pys.authoring` local package path in `Packages/manifest.json` and `Packages/packages-lock.json`.
- Updated refactor plan/audit docs to quote the corrected external package path.

Verification:

- Confirmed the corrected local package target contains `Packages/com.pys.authoring/package.json`.
- Unity batch launch previously failed package resolution because the old relative path resolved one directory too shallow.
- Unity batch smoke after the path fix resolved packages and exited with return code 0.
- Scoped `git diff --check` passed for touched files.
- Automated EditMode Test Runner did not produce test results in batch mode because Unity stalled on Licensing Client reconnects before test discovery. Editor Test Runner verification remains the appropriate next proof surface.

### Camera Rig Conditional Focus Target Contract

Changes:

- Removed `explicitFocusTarget` from the always-required PYS fields on `CinemachineCameraRigController`.
- Updated setup guidance so Explicit Focus Target is required only when `CameraRigProfile.focusMode` is `Explicit Scene Target`.

Verification:

- Confirmed `CameraRigProfile.focusMode` includes `ExplicitSceneTarget` as one option among manual, participant, group, and playfield focus modes.
- Confirmed `CinemachineCameraRigController` falls back to participant/pawn/playfield focus paths when explicit focus mode is not selected.
- Scoped `git diff --check` passed for touched files.
- Unity Editor Test Runner proof remains manual because batch mode currently stalls on Unity Licensing Client reconnects before test discovery.

### Billboard Camera Override Contract

Changes:

- Removed `cameraOverride` from the always-required PYS fields on `BillboardFacing3D`.
- Downgraded the missing camera override runtime issue from required to recommended because setup guidance already allows runtime camera assignment.

Verification:

- Confirmed `BillboardFacing3D.ApplyBillboard` safely returns when no active camera is available.
- Confirmed setup guidance already describes Camera Override as optional.
- Scoped `git diff --check` passed for touched files.
- Unity Editor Test Runner proof remains manual because batch mode currently stalls on Unity Licensing Client reconnects before test discovery.

### Optional Override And Feedback Sink Contracts

Changes:

- Removed `_mixerOverride` from the always-required PYS fields on `GameplaySettingsService`; the assigned `SettingsProfile` remains the settings owner.
- Removed `hitPauseSink` and `cameraShakeSink` from the always-required PYS fields on `EnemyReactionComponent`.
- Removed optional owner and hit feedback outputs from the always-required PYS fields on the 3D `HitBox`.
- Updated setup guidance so mixer override, inferred hitbox owner, hit FX, hit pause, and camera shake sinks are explicitly optional scene outputs.

Verification:

- Confirmed `GameplaySettingsService.Mixer` resolves the override only when assigned, otherwise it uses `SettingsProfile.mixer`.
- Confirmed 3D `HitBox` infers owner from a parent `HealthComponent` when the owner field is empty.
- Confirmed enemy reaction and hitbox feedback calls use null-safe optional sink resolution.
- Scoped `git diff --check` passed for touched files.
- Unity Editor Test Runner proof remains manual because batch mode currently stalls on Unity Licensing Client reconnects before test discovery.

### Actor Animation Conditional Reference Contract

Changes:

- Narrowed `ActorAnimationDriver` always-required PYS fields to the authored presentation and animation profiles.
- Moved Animator, Visual Root, Billboard Target, and Camera Override into conditional setup guidance because the component resolves child references and only needs camera override for billboard presentation.

Verification:

- Confirmed `ActorAnimationDriver.ResolveReferences` finds child `Animator`, child `SpriteRenderer`, `BillboardFacing3D`, `ActorShadowDriver`, `visualRoot`, and `billboardTarget` fallbacks.
- Confirmed runtime validation already reports missing Animator only when no child Animator exists, and reports missing camera override only for `Billboard2_5D`.
- Scoped `git diff --check` passed for touched files.
- Unity Editor Test Runner proof remains manual because batch mode currently stalls on Unity Licensing Client reconnects before test discovery.

### Optional Presentation Media Contracts

Changes:

- Removed collection audio/VFX fields from the always-required PYS fields on `CollectibleFeedback2D`.
- Removed `hitEffectPrefab` from the always-required PYS fields on `ProjectileImpactDefinition`.
- Removed `previewImage` from the always-required PYS fields on `LevelData`.
- Updated setup guidance so pickup media, projectile impact VFX, and level preview artwork are explicit optional presentation outputs.

Verification:

- Confirmed `CollectibleFeedback2D` safely skips missing collect clips/particles and only validates mixer routing when clips are assigned.
- Confirmed `ProjectileImpactDefinition.GetValidationIssues` requires identity and conditional hit-pause/camera-shake values, not impact effect prefabs.
- Confirmed `LevelData.GetRuntimeValidationIssues` requires scene name and display name, not preview artwork.
- Scoped `git diff --check` passed for touched files.
- Unity Editor Test Runner proof remains manual because batch mode currently stalls on Unity Licensing Client reconnects before test discovery.

### Board Piece Optional Visual Contract

Changes:

- Removed `visualPrefab` from the always-required PYS fields on `BoardPieceDefinition`.
- Added setup guidance explaining that a board piece visual prefab is only needed when the piece should override the presenter fallback visual.

Verification:

- Confirmed `BoardPieceDefinition.GetValidationIssues` requires piece identity fields, not visual prefab assignment.
- Confirmed `TabletopBoardGridPresenter` treats its fallback piece prefab as recommended and can use piece-specific visual prefabs or generated fallback pieces.
- Scoped `git diff --check` passed for touched files.
- Unity Editor Test Runner proof remains manual because batch mode currently stalls on Unity Licensing Client reconnects before test discovery.

### Pawn Definition Modular Profile Contract

Changes:

- Narrowed `PawnDefinition` always-required PYS fields to `pawnPrefab`.
- Updated setup guidance so movement, combat, traversal, presentation, and animation profiles are assigned only for capabilities the pawn supports.

Verification:

- Confirmed `PawnDefinition.BuildRuntimeValidationIssues` requires the pawn prefab and validates prefab composition directly.
- Confirmed presentation and animation profiles are required only when an `ActorAnimationDriver` needs profile data from either the `PawnDefinition` or the component itself.
- Confirmed top-down hop validation is conditional on the movement profile's effective movement style and jump setting.
- Scoped `git diff --check` passed for touched files.
- Unity Editor Test Runner proof remains manual because batch mode currently stalls on Unity Licensing Client reconnects before test discovery.

### Plan And Audit Status Refresh

Changes:

- Updated the implementation plan current-status section to include optional media, optional sink, and modular profile cleanup slices.
- Updated the authoring surface audit current-progress section to reflect the corrected PYS package path smoke proof and the remaining Editor Test Runner proof.
- Clarified that remaining `RequiredFields` hits are now mostly identity, prefab, UI, or concrete component setup surfaces rather than broad optional media/profile false positives.

Verification:

- Confirmed Unity version remains `6000.4.0f1`.
- Confirmed the current worktree dirty set still contains only known unrelated/generated files outside this lane.
- Scoped `git diff --check` passed for touched docs.
- Unity Editor Test Runner proof remains manual because batch mode currently stalls on Unity Licensing Client reconnects before test discovery.

### Generic Spawner Sprite Output Guidance

Changes:

- Added a recommended runtime validation issue when generic `Spawner` uses raw Sprite entries without prefab entries.
- Updated the audit so raw sprite-to-GameObject spawning is classified as retained prototype utility output with prefab-first guidance, not an unresolved hidden setup repair.

Verification:

- Confirmed `Spawner` is an optional scene utility and not the participant pawn or enemy spawn path.
- Confirmed prefab spawning remains unchanged and raw sprite spawning remains supported for prototype scenes.
- Scoped `git diff --check` passed for touched files.
- Unity Editor Test Runner proof remains manual because batch mode currently stalls on Unity Licensing Client reconnects before test discovery.
