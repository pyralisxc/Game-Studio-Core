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

## Readability rules

1. Add XML summary comments to all public classes.
2. Add XML summary comments to major public methods.
3. Keep method names explicit and responsibility-focused.
4. Use concise comments only where behavior is non-obvious.
5. Avoid unexplained magic numbers.

## Definition of done per migration batch

- files moved to the correct `Features/[Name]/2D`, `Features/[Name]/3D`, `Core`, `Data`, or `Editor` path
- compile diagnostics pass
- no missing script references in touched scenes or prefabs
- public API surface has clear summaries
- setup docs updated when user-facing behavior changes
