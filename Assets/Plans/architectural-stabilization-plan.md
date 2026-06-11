# Architectural Stabilization Plan

## Goal
Secure the architectural baseline by migrating legacy persistent systems to pure DI, normalizing multi-participant input, and integrating reflective validation natively into the Editor UX.

## Phase 1: Editor UX Alignment
- [x] Create `PyralisBaseEditor` as the foundation for reflective authoring.
- [x] Migrate `ActionDefinitionEditor` to the new base.
- [x] Migrate `CameraRigProfileEditor` to the new base.
- [ ] Audit all custom editors in `Gameplay/Editor` and migrate those using "Guided Authoring" to `PyralisBaseEditor`.
- [ ] Ensure `PyralisReflectiveInspectorOverlay` correctly handles objects without custom editors while avoiding double-draws.

## Phase 2: Service Transition (Pure DI)
- [ ] Refactor `TimeManager`: Remove static instance, enforce `IHitPauseSink` usage.
- [ ] Refactor `SceneLoader`: Remove static instance, enforce `ISceneNavigator` usage.
- [ ] Refactor `CameraShake`: Remove static instance, enforce `ICameraShakeSink` usage.
- [ ] Update `GameplaySessionBootstrap` to remove manual service creation and defer registration to `PyralisGameplayLifetimeScope`.

## Phase 3: Input Normalization
- [ ] Verify `Pawn3DInputModule` device isolation for multi-participant setups.
- [ ] Verify `PlayerInputHandler` (2D) device isolation.
- [ ] Replace any hardcoded "Player" tags in input adapters with `ParticipantId` lookups.

## Phase 4: Profiling & Optimization
- [ ] Perform a deep Profiler pass on hot-path `Update()` cycles.
- [ ] Eliminate per-frame allocations in `DamageZone` and FX coroutine layers.
- [ ] Validate "ready for runtime" status across all core archetypes.
