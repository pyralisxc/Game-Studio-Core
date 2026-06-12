# Neon Black Hub

Neon Black Hub is the shared gameplay package in this project. Its active game-facing toolkit is `NeonBlack Gameplay`, which now physically lives under `Members/Pyralis/Gameplay`.

Use this package when you want to:

- build participant-driven gameplay with authored `ScriptableObject` data
- compose shared gameplay features across 2D and 3D projects
- reuse shared services such as scene loading, time, camera shake, participant spawning, input routing, and VContainer-backed runtime composition
- migrate older arcade-style or brawler-style scenes into the new feature-first runtime

## Important folders

| Location | Purpose |
| --- | --- |
| `Members/Pyralis/Gameplay/Docs/Authoring/` | Active setup guides for new scenes and authored gameplay routes. Start with `START_HERE.md`. |
| `Members/Pyralis/Gameplay/Core/` | Foundational services, VContainer composition, contracts, config, `SceneLoader`, and `TimeManager`. |
| `Members/Pyralis/Gameplay/Data/` | ScriptableObject definitions and profiles for sessions, participants, pawns, modes, and settings. |
| `Members/Pyralis/Gameplay/Editor/Authoring/` | Central Pyralis authoring spine, Authoring Window, Inspector guides, validation, diagnostics, and editor utilities. |
| `Members/Pyralis/Gameplay/Features/` | All gameplay systems. Each feature lives in `Features/[Name]/` with `2D/` and `3D/` subfolders where applicable. |
| `Members/Pyralis/Gameplay/Networking/` | Session ownership, authority, replication-facing contracts, and backend adapters. |
| `Members/Pyralis/Gameplay/Presentation/` | Cross-feature animation, camera, audio, and HUD infrastructure. |
| `Members/Pyralis/Gameplay/Integrations/` | Service and package adapters for the wider platform. |
| `Members/Pyralis/Gameplay/Docs/` | Internal architecture, standards, and scene wiring notes. |

## Install prerequisites for another Unity project

Neon Black Hub currently has one non-Unity-registry prerequisite: `jp.hadashikick.vcontainer`.
Unity packages cannot reliably add scoped registries or Git dependencies from inside their own `package.json`, so the consuming project must install or expose VContainer before adding this package.

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

## Handoff verification for another computer

When another teammate opens the shared Game Studio Core project folder, Unity should rebuild local generated state from the committed/project-shared files. `Library/`, `Temp/`, `Logs/`, `UserSettings/`, generated `.csproj`, and generated `.sln` files are local machine state and are not the source of truth.

Before debugging script errors on another machine, check the package source that Unity is actually reading:

1. Open `Packages/com.neonblackinteractivellc.neonblackhub/package.json`.
2. Confirm `"version": "0.2.0"`.
3. Confirm the package has `Members/Pyralis/Gameplay/`.
4. Confirm `Packages/manifest.json` does not reference `com.studiotools.core`.

If `package.json` still says an older version, that machine has an older copy of the package folder and needs the current project files. If `package.json` says `0.2.0` but Unity Package Manager still displays an older version, close Unity, remove the local `Library/PackageCache/com.neonblackinteractivellc.neonblackhub*` cache entry if one exists, reopen the project, and let Unity re-resolve packages. The shared project uses the embedded package under `Packages/`; the package cache is only Unity-generated local state.

## Quick start

1. Read `Members/Pyralis/Gameplay/Docs/Authoring/START_HERE.md`.
2. Add `GameplaySessionBootstrap` to your scene.
3. Open the Pyralis Authoring Window or select the bootstrap and use its `Setup Flow` foldout.
4. In the Project window, select the folder you want to own the setup assets, then right-click and create the needed `NeonBlack/Gameplay/...` definitions and profiles.
5. Use the Authoring Window as the route map, and use Inspector fields plus Project-window drag/drop to wire the scene natively.
6. Use the focused setup guides in `Members/Pyralis/Gameplay/Docs/Authoring/Prefabs/` for combat, health, tabletop, and authored prefab setup.

## Current source of truth

- Active runtime code is under `Members/Pyralis/Gameplay/`.
- `GameplaySessionBootstrap` is the Unity-facing startup path for new scenes.
- `PyralisGameplayLifetimeScope` is the runtime DI graph. It registers owned services for the active bootstrap/session path.
- The folder structure is `Core/`, `Data/`, `Editor/`, `Features/` with no `Shared/`, `Runtime2D/`, or `Runtime3D/` top-level folders.
- Scene setup should follow the current guides in `Members/Pyralis/Gameplay/Docs/Authoring/`.
