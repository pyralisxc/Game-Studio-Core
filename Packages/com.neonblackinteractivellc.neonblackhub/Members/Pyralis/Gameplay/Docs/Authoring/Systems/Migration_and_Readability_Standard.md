# Migration and Readability Standard

This page defines how NeonBlack Gameplay runtime code is migrated inside `Members/Pyralis/Gameplay` and how readability is enforced.

## Migration rules

1. Move `.cs` and `.meta` files together.
2. Move in small batches by risk level.
3. Low risk: data, interfaces, isolated utilities.
4. Medium risk: reusable `MonoBehaviour` components with limited scene coupling.
5. High risk: scene orchestration, persistent scene services, and compatibility code.
6. Run compile diagnostics after each batch.
7. Validate scenes and prefabs for missing scripts after scene-bound moves.
8. Do not keep duplicate or staging copies of package code under `Packages/`; Unity imports every `.asmdef` there and duplicate assembly names will break compilation.
9. When touching service ownership, prefer moving dependencies toward the `PyralisGameplayLifetimeScope`/`VContainer` graph instead of adding new static accessors.
10. When touching participant, input, respawn, HUD, camera, or player lookup code, prefer participant/session references over `PlayerRegistry`, tag lookup, or one-primary-player assumptions.
11. When touching hot-path runtime code, check for per-frame allocations, broad polling, `RaycastAll`, repeated `renderer.materials` access, and uncached `GetComponents*` calls.
12. When touching aggregate assembly references, avoid broadening `NeonBlack.Gameplay.asmdef` for convenience unless the dependency is intentionally package-level.
13. Treat setup guidance as product code. When changing beginner setup behavior, update contracts, dependency-tree setup references, graph projection, validators, grammar/vocabulary, the Authoring Window, and setup docs together. Route analysis is a graph input/helper, not the source of truth.
14. Keep route detection in shared route analysis instead of duplicating pawn, pattern, participant, camera/input, playfield, or scoring checks in individual inspectors or windows.

## Readability rules

1. Add XML summary comments to all public classes.
2. Add XML summary comments to major public methods.
3. Keep method names explicit and responsibility-focused.
4. Use concise comments only where behavior is non-obvious.
5. Avoid unexplained magic numbers.
6. Keep comments plain ASCII or valid UTF-8. Do not add decorative separator comments that are likely to corrupt into mojibake.
7. If a touched file already contains corrupted text or replacement characters, clean the nearby comments you are editing without churning unrelated logic.
8. Label compatibility paths directly in comments, inspectors, and docs so they do not read as preferred new architecture.

## Hot-path rules

Treat these as performance review triggers:

- `Update`, `FixedUpdate`, and `LateUpdate`
- camera occlusion and camera follow
- projectile and hitbox spawning
- hazard, pickup, and feedback swarms
- input routing and local multiplayer join/leave tracking
- UI elements that update every frame

Preferred patterns:

- use non-alloc physics APIs where possible
- cache component arrays or renderer/material state when object topology is stable
- reuse buffers for temporary lists in per-frame code
- move polling to events or dirty checks when Unity exposes a reliable signal
- profile before expanding content density around a known hot path

## Definition of done per migration batch

- files moved to the correct `Features/[Name]/2D`, `Features/[Name]/3D`, `Core`, `Data`, or `Editor` path
- compile diagnostics pass
- no missing script references in touched scenes or prefabs
- public API surface has clear summaries
- setup docs updated when user-facing behavior changes
- service ownership, participant assumptions, and hot-path allocation risks reviewed for touched files
