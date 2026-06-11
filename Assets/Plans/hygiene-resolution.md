# Pyralis Clean & Modern Upgrade (v0.2.0)

## Phase 1: Mark & Prep (Version Bump)
- [ ] Update `package.json` to version `0.2.0`.
- [ ] Flag `PlatformServiceRegistry` and legacy samples with `RemovableInVersion = "0.2.0"` in their `AuthoringContract`.

## Phase 2: Hard Deletion of Assets
- [ ] Delete `Assets/Scenes/SampleScene.unity`.
- [ ] Delete legacy RPG samples folder: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Features/Rpg/Samples/`.
- [ ] Delete legacy documentation: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Setup/RPG_Golden_Sample_Setup.md`.

## Phase 3: Service Refactoring (Removing Static Registry)
- [ ] Delete `PlatformServiceRegistry.cs`.
- [ ] Refactor `FeatureHostInitializationContext.cs` and `FeatureRuntimeInitializationContext.cs` to remove the legacy registry property.
- [ ] Update `GameplaySessionBootstrap.cs` to remove bridge-registry logic.
- [ ] Update `PyralisGameplayLifetimeScope.cs` to handle full DI registration without the registry bridge.

## Phase 4: API Consolidation (HitBox2D)
- [ ] Refactor `HitBox2D.cs` to use the modern `Fire()` one-shot query pattern.
- [ ] Delete legacy `Enable()`, `Disable()`, `EnableHitBox()`, and `DisableHitBox()` methods from `HitBox2D.cs`.
- [ ] Update any callers of `HitBox2D` to use `Fire()`.

## Phase 5: Cleanup & Verification
- [ ] Fix all compilation errors resulting from deletions and refactoring.
- [ ] Validate `RpgHubProof.unity` scene wiring.
- [ ] Run Authoring Hygiene audit to confirm 100% cleanliness.
