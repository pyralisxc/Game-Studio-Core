# Authoring Surface Refactor Changelog

This file records the package-wide authoring-surface refactor for review. It is intentionally separate from package release notes.

Scope rules:

- Do not modify `Assets/` during this lane unless Cameron explicitly asks.
- Do not auto-generate samples, scenes, prefabs, or ScriptableObject assets.
- Commit small verified slices as work proceeds.
- Prefer clean compatibility cuts when references allow.
- Keep PYS focused primarily on codebase quality, system health, ownership pressure, route semantics, topology, proof readiness, and cross-object setup truth.

## 2026-07-04

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
