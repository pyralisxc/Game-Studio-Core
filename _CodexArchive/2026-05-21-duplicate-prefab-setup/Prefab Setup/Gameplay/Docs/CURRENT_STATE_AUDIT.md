# NeonBlack Gameplay Current State Audit

This document captures the current architectural state of NeonBlack Gameplay after the platform realignment, assembly split, deferred cleanup pass, and setup-doc consolidation.

Update it whenever a major blocker changes or a formerly open refactor concern is no longer one of the top priorities.

## Summary

NeonBlack Gameplay now has:

- a real multi-assembly shared core
- a `VContainer`-backed composition root
- enforced feature ownership across the main gameplay domains
- cleaned deferred slices for Pickups, Feedback, and Scoring
- consolidated setup docs built around `Docs/Setup/CANONICAL_SETUP.md`

The platform architecture is no longer the main blocker. The highest-value ongoing work is:

- expanding Unity-side behavioral coverage
- keeping docs and editor tooling aligned with the preferred authored path
- continuing to reduce remaining single-primary-player assumptions in older compatibility surfaces

## Stable Platform Foundations

These are now true platform capabilities, not transitional goals:

- participant/session runtime through `ParticipantRosterService`, `ParticipantSpawnService`, `SessionStateService`, and `GameplaySessionBootstrap`
- authored definitions for session, participant, pawn, game mode, and feature modules
- authored profiles for movement, combat, traversal, presentation, camera, playfield, input, and settings
- feature runtime initialization through `FeatureRuntimeInitializationContext`
- actor composition through `ActorFeatureContext` and narrowed core contracts
- transport-agnostic networking ownership contracts in `Core/Contracts/Networking`
- NGO-specific behavior isolated in `NeonBlack.Gameplay.Networking`

## Assembly State

The package is no longer operating as one broad gameplay assembly.

Stable cuts now exist for:

- `Core`
- `Data`
- `Characters`
- `Composition`
- `Presentation`
- `Networking`
- `Combat`
- `Traversal`
- `Interaction`
- `Feedback`
- `Scoring`
- `Pickups`

Current priority:

- enforce the boundaries we already created
- keep extracting small contracts when a feature seam is still too concrete
- avoid re-broadening assemblies for convenience

## Resolved In The Current Architecture

These were major refactor blockers and are now resolved or intentionally stabilized:

- `GameServices` is gone from the active runtime architecture
- `GameplaySessionBootstrap` is the supported startup path
- feature runtime installation is DI-backed
- the 3D pawn is decomposed into focused modules
- the 2D pawn is decomposed into focused components, with `Motor2D` retained only as a compatibility facade
- older `Runtime2D/`, top-level `Shared/`, and `Legacy/` folder drift has been removed from the active architecture; feature-local `Runtime/Shared` folders are intentional ownership slices
- `LegacyBrawlerPawnBridge` and `LegacyArcadePawnBridge` are no longer part of the supported path
- `PointPickup`, `PointPickupSpawner`, and `PointPickupFeedback` thin aliases have been removed
- deferred pickup, feedback, and scoring seams now use explicit contracts instead of hidden orchestration
- setup docs are consolidated around canonical naming rather than compatibility naming
- Unity Safe Mode compilation was cleared after the asmdef, editor, and test-reference stabilization pass

## Highest-Priority Remaining Issues

### 1. Some Legacy Gameplay Surfaces Still Assume One Primary Player

The participant model is real, but a few older systems still think in terms of one main player for compatibility.

Current examples:

- `Features/Characters/PlayerRegistry.cs`
- parts of `Features/Respawn/3D/PlayerSpawner.cs`
- some menu-driven and 2D scene-specific flows

Why it matters:

- keeps older gameplay layers from feeling fully participant-native
- makes local multiplayer harder to generalize than it should be
- encourages compatibility code to linger longer than necessary

### 2. Input Ownership Is Improved But Not Fully Normalized

The package already uses the Unity Input System and participant-aware routing, but some systems still retain direct or older assumptions.

Current example:

- `Features/Input/2D/Motor2DInputAdapter.cs`

Why it matters:

- local co-op and mixed-device flows remain more fragile than the shared core now deserves
- older input assumptions can quietly reintroduce single-player bias

### 3. Editor UX Still Trails The Runtime Architecture

The runtime seams are cleaner than the authoring experience in a few places.

Why it matters:

- a correct platform is still harder to use if the Inspector path does not strongly guide the preferred setup
- stale editor guidance can recreate old architectural habits

Current focus:

- keep setup docs, inspectors, and validation aligned with canonical types
- make preferred authored paths easier than compatibility paths

## Medium-Priority Issues

### 4. Behavioral Test Coverage Is Still Light

The package now has real architecture and validation coverage, but deeper runtime/editor behavior coverage is still thinner than the platform shape.

Why it matters:

- shared-runtime changes still rely too heavily on manual confidence
- bigger refactors become slower without stronger safety rails

Current focus:

- participant lifecycle
- feature installation order
- setup validation flows
- editor-created authored assets

### 5. Deferred Compatibility Surfaces Still Need Periodic Review

The highest-risk deferred slices have been cleaned, but some older scene-facing systems still deserve occasional review so they do not drift back toward hidden coupling.

Examples:

- some respawn helpers
- some scene/menu flow helpers
- some HUD and 2D adapter surfaces

## Lower-Priority But Important

### 6. Documentation Hygiene Is Now Ongoing Maintenance

The docs are in much better shape, but the rule now is maintenance discipline, not one more giant rewrite.

Why it matters:

- stale setup docs quickly become competing truths
- the cleaner the architecture gets, the more noticeable stale wording becomes

Current source-of-truth path:

- `Docs/Setup/CANONICAL_SETUP.md`
- `Docs/Setup/SCENE_SETUP_GUIDE.md`
- feature-specific setup docs beneath `Docs/Setup/Prefabs/`

## Recommended Order Of Attack

1. Keep `CURRENT_STATE_AUDIT.md`, `CANONICAL_SETUP.md`, and feature setup docs aligned with the real package shape.
2. Expand Unity-side behavioral coverage around participant flow, feature installation, and authored setup validation.
3. Continue reducing single-primary-player assumptions in older compatibility layers.
4. Tighten editor tooling so the preferred authored path is always easier than the legacy one.
5. Keep enforcing asmdef and runtime-boundary rules so the architecture does not drift backwards.

## Success Signals

The package is moving in the right direction when:

- participant-aware flows replace one-player assumptions without special-case architecture
- new setup guides can use canonical type names only
- feature boundaries stay stable without needing to merge assemblies back together
- editor tooling points people toward the preferred path by default
- tests catch drift before refactors become risky
