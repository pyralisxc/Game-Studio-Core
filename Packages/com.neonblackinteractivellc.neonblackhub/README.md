# Neon Black Hub

Neon Black Hub is the shared gameplay package in this project. Its active game-facing toolkit is `NeonBlack Gameplay`, which now physically lives under `Members/Pyralis/Gameplay`.

Use this package when you want to:

- build participant-driven gameplay with authored `ScriptableObject` data
- compose shared gameplay features across 2D and 3D projects
- reuse shared services such as scene loading, time, camera shake, participant spawning, input routing, and VContainer-backed runtime composition
- bring existing arcade-style or brawler-style routes onto the shared feature-first runtime

## Important folders

| Location | Purpose |
| --- | --- |
| `Members/Pyralis/Gameplay/Docs/` | Current architecture and setup guidance for authored gameplay routes. |
| `Members/Pyralis/Gameplay/Core/` | Stable runtime contracts, narrow seams, and small shared type vocabulary. |
| `Members/Pyralis/Gameplay/Data/` | ScriptableObject definitions, profiles, config assets, and data-backed handoff contracts. |
| `Members/Pyralis/Gameplay/Editor/` | Tactical local inspectors, validation helpers, diagnostics, and editor utilities. PYS Authoring lives in the external `com.pys.authoring` package. |
| `Members/Pyralis/Gameplay/Modules/` | Reusable gameplay capability families. Lane-specific code uses `Sprite2D`, `Billboard2_5D`, and `Rigged3D` folders where applicable. |
| `Members/Pyralis/Gameplay/Glue/` | Bootstrap, lifetime, session, participant, input-routing, spawning, service-registration, and scene-flow composition that wires authored modules together. |
| `Members/Pyralis/Gameplay/Networking/` | Optional networking ownership, authority, and replication-facing runtime extensions. |
| `Members/Pyralis/Gameplay/Presentation/` | Cross-feature animation, camera, audio, and HUD infrastructure. |

## Install prerequisites for another Unity project

Neon Black Hub currently has two non-Unity-registry prerequisites: `jp.hadashikick.vcontainer` and the external `com.pys.authoring` package.
Unity packages cannot reliably add scoped registries, Git dependencies, or local disk dependencies from inside their own `package.json`, so the consuming project must install or expose both prerequisites before adding this package.

Recommended project manifest setup:

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "jp.hadashikick"
      ]
    }
  ],
  "dependencies": {
    "jp.hadashikick.vcontainer": "1.17.0"
  }
}
```

After VContainer resolves in the target project, add `com.neonblackinteractivellc.neonblackhub` through Package Manager or the project manifest. If Package Manager reports invalid dependencies before Unity imports scripts, check the target project's manifest first; copying only this package without the VContainer registry/package will not be enough.

Game Studio Core does not embed PYS Authoring source. Install `com.pys.authoring` through Unity Package Manager from the source package's `package.json`, then let Unity refresh package resolution before compiling NeonBlack Gameplay.

## Handoff verification for another computer

When another teammate opens the shared Game Studio Core project folder, Unity should rebuild local generated state from the committed/project-shared files. `Library/`, `Temp/`, `Logs/`, `UserSettings/`, generated `.csproj`, and generated `.sln` files are local machine state and are not the source of truth.

Before debugging script errors on another machine, check the package source that Unity is actually reading:

1. Open `Packages/com.neonblackinteractivellc.neonblackhub/package.json`.
2. Confirm `"version": "0.2.9"`.
3. Confirm the package has `Members/Pyralis/Gameplay/`.
4. Confirm `Packages/manifest.json` does not reference `com.studiotools.core`.

If `package.json` still says an older version, that machine has an older copy of the package folder and needs the current project files. If `package.json` says `0.2.9` but Unity Package Manager still displays an older version, close Unity, remove the local `Library/PackageCache/com.neonblackinteractivellc.neonblackhub*` cache entry if one exists, reopen the project, and let Unity re-resolve packages. The shared project uses the embedded package under `Packages/`; the package cache is only Unity-generated local state.

## Quick start

1. Read `Members/Pyralis/Gameplay/README.md`, then use Unity Inspector fields, project assets, and PYS Authoring evidence for scene-specific setup guidance.
2. Add `GameplaySessionBootstrap` to your scene.
3. Open the PYS Authoring Window or select the bootstrap and use its `Setup Flow` foldout.
4. In the Project window, select the folder you want to own the setup assets, then right-click and create the needed `NeonBlack/Gameplay/...` definitions and profiles.
5. Use the Authoring Window as the route map, and use Inspector fields plus Project-window drag/drop to wire the scene natively.
6. Use module-owned inspectors for local field setup and PYS Authoring projections for route evidence, proof readiness, and cross-object setup truth.

## Current source of truth

- Active runtime code is under `Members/Pyralis/Gameplay/`.
- `GameplaySessionBootstrap` is the Unity-facing startup path for new scenes.
- `GameplayLifetimeScope` is the runtime DI graph. It registers owned services for the active bootstrap/session path.
- The active folder structure is `Core/`, `Data/`, `Glue/`, `Modules/`, `Networking/`, `Presentation/`, `Editor/`, and `Docs/`; there are no `Shared/`, `Runtime2D/`, `Runtime3D/`, or `Integrations/` top-level folders.
- Scene setup should follow Unity-native scene, prefab, and profile ownership, with PYS Authoring projections and current gameplay docs used as evidence and guidance.
