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
