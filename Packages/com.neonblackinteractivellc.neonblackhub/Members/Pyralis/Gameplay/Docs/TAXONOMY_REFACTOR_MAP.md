# Pyralis Taxonomy Refactor Map

This is the live move map for the Pyralis module/glue taxonomy refactor.

Use this file before moving folders, renaming namespaces, or renaming asmdefs. The goal is a hard-cut source layout where Unity developers can find reusable modules, glue, authored data, presentation, networking, authoring, and tests without learning old internal history.

## Ownership Legend

| Owner | Meaning |
|---|---|
| Core | Stable contracts and tiny shared type vocabulary that gameplay code can safely reference. No top-level Core source files. |
| Data | ScriptableObject definitions, profiles, and config assets that express authored setup and tuning. |
| Module | Reusable gameplay capability a creator can choose, compose, inspect, validate, or prove. |
| Glue | Bootstrap, lifetime scope, session, participant, input routing, spawning, service registration, camera routing, and scene-flow composition. |
| Presentation | Visual, animation, camera, feedback-display, and HUD surfaces that render gameplay meaning. |
| Networking | Optional NGO/network authority, spawn, ownership, and adapter layer. |
| Authoring | Editor-only graph, reflection, dependency, projection, vocabulary, export, hygiene, facts, window, and inspector tooling. |
| Tests | Automated seam protection for contracts, projections, data transfer, and validation. |
| No Move | User-authored scenes, prefabs, temp diagnostics, generated Unity state, and proof objects outside this hard-cut source pass. |

## Top-Level Target Map

Status note: Phase B has extracted the old platform folders into first-class glue folders: `Glue/Bootstrap`, `Glue/Lifetime`, `Glue/ServiceRegistration`, `Glue/Participants`, `Glue/InputRouting`, `Glue/Session`, and `Glue/Spawning`. Phase C has removed the active `Features` source root by moving reusable gameplay families into `Modules`, route/mode orchestration into `Glue/SceneFlow`, and visual surfaces into `Presentation`. Phase D has reduced Core to contracts and tiny shared type vocabulary by moving scene flow to `Glue/SceneFlow/Navigation`, RPG runtime to `Modules/Rpg/Runtime`, tabletop rules to `Modules/Tabletop/Runtime`, queued action execution to `Modules/Actions/Runtime`, animation/movement vocabulary to `Core/Types`, authored config assets to `Data/Config`, runtime session context to `Glue/Lifetime`, and shared participant identity/query/profile-application contracts to `Data/Participants`. `InputConfig` was removed because `InputProfile` is the single authored input setup asset. Phase E has hard-cut module/editor assembly names to `NeonBlack.Gameplay.Modules.*`, moved public namespaces away from `NeonBlack.Gameplay.Features.*`, and renamed lane folders from `2D`/`3D` to `Sprite2D`/`Rigged3D`. The module assembly graph is now acyclic: modules do not import `Glue`, Character owns shared pawn/traversal contracts, Traversal owns concrete traversal behavior, Presentation owns camera targets and camera rigs, and authored input helpers live in `Data/Profiles`. Phase F has hard-cut Authoring into explicit `Dependency`, `Evidence`, `Facts`, `Graph`, `Routes`, `Intent`, `Projections`, `Exports`, `Hygiene`, `Validation`, `Vocabulary`, `Window`, and `Editor/Inspectors/Pyralis` owners. Old `Features/...` and `Editor/Authoring/Spine|Grammar|Surfaces` rows below are historical move-map entries, not active source paths.

| Current Path | Target Path | Owner | Assembly | Reason | Phase |
|---|---|---|---|---|---|
| `Core/AuthoringContracts` | `Core/Contracts/Authoring` | Core | `NeonBlack.Gameplay.Core` | Runtime-visible contract metadata must stay available to gameplay code without pulling Editor. | Done |
| `Core/RuntimeContracts` | `Core/Contracts/Runtime` | Core | `NeonBlack.Gameplay.Core` | Runtime interfaces should be obvious as contracts, not mixed with implementation. | Done |
| `Core/Actions` | `Core/Types/Actions` plus `Modules/Actions/Runtime` | Core/Module | `NeonBlack.Gameplay.Core` and `NeonBlack.Gameplay.Modules.Actions` | Core keeps tiny shared action vocabulary; queued action execution belongs to the action module. | Done |
| `Core/Rules` | `Modules/Tabletop/Runtime/Rules` | Module | `NeonBlack.Gameplay.Modules.Tabletop` | Board/turn primitives are tabletop gameplay, not universal Core. | Done |
| `Core/Rpg` and `Core/Contracts/Rpg` | `Modules/Rpg/Runtime/Domain`, `Contracts`, and `Services` | Module | `NeonBlack.Gameplay.Modules.Rpg` | RPG domain, interfaces, persistence, and services are optional feature runtime, not universal Core. | Done |
| `Core/Navigation` and `Core/Navigation/UI` | `Glue/SceneFlow/Navigation` | Glue | `NeonBlack.Gameplay` | Level flow, loading, menu shell navigation, and scene guards are route/session glue. | Done |
| `Core/Animation` | `Core/Types/Animation` | Core | `NeonBlack.Gameplay.Core` | Presentation lanes and animation signals are tiny shared type vocabulary, not a Core implementation folder. | Done |
| `Core/MovementMode.cs` | `Core/Types/MovementMode.cs` | Core | `NeonBlack.Gameplay.Core` | Movement mode is tiny shared type vocabulary used by data and character modules. | Done |
| `Core/InputConfig.cs` | Removed | Data/Input | n/a | `InputProfile` already owns authored input asset, action-map, gameplay action rows, validation, and participant input meaning. | Done |
| `Data/Config/GameConfig.cs` namespace | `NeonBlack.Gameplay.Data.Config` | Data | `NeonBlack.Gameplay.Data` | Game config is an authored data asset, not Core. | Done |
| `Glue/Lifetime/GameplayRuntimeContext.cs` namespace | `NeonBlack.Gameplay.Glue.Lifetime` | Glue | `NeonBlack.Gameplay` | Runtime session context is lifetime glue, not Core config. | Done |
| `Data/Definitions` | `Data/Definitions` | Data | `NeonBlack.Gameplay.Data` | Existing data root is understandable and should stay stable unless subfolders need module alignment. | C/D |
| `Data/Profiles` | `Data/Profiles` | Data | `NeonBlack.Gameplay.Data` | Existing profile root owns authored profile assets plus shared profile helpers such as `ParticipantInputProfileUtility` and `InputZoneSet`; modules should consume the data contract rather than own authored input setup meaning. | Done |
| `Features/Platform/Composition` | `Glue/Lifetime` and `Glue/ServiceRegistration` | Glue | `NeonBlack.Gameplay.Glue` | Lifetime scope and service installers compose the route; they are not gameplay features. | B |
| `Features/Platform/Session` | `Glue/Bootstrap`, `Glue/Session`, `Glue/Participants`, `Glue/Spawning` | Glue | `NeonBlack.Gameplay.Glue` | Session startup, participant roster, spawn service, and session state are route glue. | B |
| `Features/Input/ParticipantInputRouter.cs` | `Glue/InputRouting/ParticipantInputRouter.cs` | Glue | `NeonBlack.Gameplay.Glue` | Participant input routing is session/control glue, not a selectable input gameplay module. | B |
| `Features/Input/2D` | `Modules/Input/Sprite2D` | Module | `NeonBlack.Gameplay.Modules.Input` and `NeonBlack.Gameplay.Modules.Input.Editor` | Sprite2D player input components are a reusable input module; authored input profiles and zone-set data live in `Data/Profiles`. | Done |
| `Features/Composition` | `Modules/Actor/Composition` | Module | `NeonBlack.Gameplay.Modules.Actor.Composition` | Actor feature host and module runtime contracts are reusable actor capability composition, not session glue. | Done |
| `Features/Characters` | `Modules/Character` | Module | `NeonBlack.Gameplay.Modules.Character` | Pawn/actor character stack owns shared pawn contracts and lane motors, but consumes traversal through `IPawnTraversalModule` instead of depending on Traversal's concrete implementation. | Done |
| `Features/Combat` | `Modules/Combat` | Module | `NeonBlack.Gameplay.Modules.Combat` | Combat, projectile, hitbox, health, reaction, and status surfaces are reusable combat modules. | Done |
| `Features/Traversal` | `Modules/Traversal` | Module | `NeonBlack.Gameplay.Modules.Traversal` | Traversal owns concrete climb/ledge/hop behavior and implements Character-owned traversal contracts. | Done |
| `Features/Pickups` | `Modules/Pickups` | Module | `NeonBlack.Gameplay.Modules.Pickups` | Pickup collection/spawning/feedback are reusable gameplay modules. | Done |
| `Features/Hazards` | `Modules/Hazards` | Module | `NeonBlack.Gameplay.Modules.Hazards` | Hazards are reusable gameplay modules with lane-specific runtime and editor ownership. | Done |
| `Features/Enemies` | `Modules/Enemies` | Module | `NeonBlack.Gameplay.Modules.Enemies` | Enemy AI/modules are a reusable enemy module family with their own runtime and editor assembly. | Done |
| `Features/Interaction` | `Modules/Interaction` | Module | `NeonBlack.Gameplay.Modules.Interaction` | Interaction is a reusable actor capability. | Done |
| `Features/Feedback` | `Modules/Feedback`, with HUD presenters in `Presentation` when split by surface | Module/Presentation | `NeonBlack.Gameplay.Modules.Feedback` | Feedback has gameplay signal ownership; visible HUD/presenter surfaces belong in Presentation when they are separated by owner. | Done |
| `Features/Scoring` | `Modules/Scoring`, with HUD presenters in `Presentation` when split by surface | Module/Presentation | `NeonBlack.Gameplay.Modules.Scoring` | Scoring service is gameplay; leaderboard/HUD surfaces are presentation. | Done |
| `Features/Tabletop` | `Modules/Tabletop` | Module | `NeonBlack.Gameplay.Modules.Tabletop` | Board/grid/turn surfaces are reusable tabletop capability. | C |
| `Features/Rpg` | `Modules/Rpg` | Module | `NeonBlack.Gameplay.Modules.Rpg` | RPG is optional platform feature work, not universal core. | Done |
| `Features/Settings` | `Modules/Settings` and `Presentation/HUD/Settings` | Module/Presentation | `NeonBlack.Gameplay.Modules.Settings` and `NeonBlack.Gameplay.Presentation` | Settings runtime state stays a module; settings screens are HUD/menu presentation. | Done |
| `Features/UI` | `Presentation/HUD/UI` | Presentation | `NeonBlack.Gameplay.Presentation` | Generic UI orientation/display code renders HUD layout and does not own gameplay rules. | C |
| `Features/GameFlow` | `Glue/SceneFlow/Arcade2D` and `Presentation/HUD/GameFlow` | Glue/Presentation | `NeonBlack.Gameplay` and `NeonBlack.Gameplay.Presentation` | Arcade score-loop orchestration is route/mode glue; its panel controller is HUD presentation. A separate Glue asmdef remains an optional future hardening pass, not a hidden module dependency. | Done |
| `Features/Spawning` | `Modules/Spawning` | Module | `NeonBlack.Gameplay.Modules.Spawning` | Generic spawner utilities are reusable module behavior. Participant pawn placement and respawn/lives/countdown coordination stay in `Glue/Spawning`. | Done |
| `Features/Zones` | `Modules/Hazards/Zones` and `Presentation/Camera/Zones` | Module/Presentation | `NeonBlack.Gameplay.Modules.Hazards` and `NeonBlack.Gameplay.Presentation` | Damage zones are hazard/combat scene surfaces; camera zones are presentation camera routing surfaces. | Done |
| `Features/Encounters` | `Modules/Encounters` | Module | `NeonBlack.Gameplay.Modules.Encounters` | Arena encounter gating is reusable encounter gameplay, not generic scene flow. | Done |
| `Features/Environment` | `Modules/Environment` | Module | `NeonBlack.Gameplay.Modules.Environment` | Tilemap ground and depth sorting are environment authoring/runtime helpers. | Done |
| `Presentation` | `Presentation` | Presentation | `NeonBlack.Gameplay.Presentation` | Presentation owns camera rigs, camera targets, animation/visual adapters, and HUD surfaces. It does not depend on Character. | Done |
| `Networking/Characters` | `Networking/CharacterAdapters` and `Networking/Glue` | Networking | `NeonBlack.Gameplay.Networking` | Networked participant services are glue variants; network motor is a character adapter. | E |
| `Networking/Runtime` | `Networking/Authority` and `Networking/Glue` | Networking | `NeonBlack.Gameplay.Networking` | Runtime authority/ownership names should expose the networking concern directly. | E |
| `Editor/Authoring/Spine/Graph` | `Editor/Authoring/Graph`, `Editor/Authoring/Projections`, `Editor/Authoring/Exports` | Authoring | `NeonBlack.Gameplay.Editor` | Graph building, tab projections, and export serialization should be findable by responsibility. | Done |
| `Editor/Authoring/Spine/Routes` | `Editor/Authoring/Routes` and `Editor/Authoring/Intent` | Authoring | `NeonBlack.Gameplay.Editor` | Route analysis and intent descriptors should not be buried under the old spine name. | Done |
| `Editor/Authoring/Spine/DependencyTree` | `Editor/Authoring/Dependency` | Authoring | `NeonBlack.Gameplay.Editor` | Dependency reflection should be a first-class authoring input. | Done |
| `Editor/Authoring/Spine/Evidence` | `Editor/Authoring/Evidence` | Authoring | `NeonBlack.Gameplay.Editor` | Scene/reflection evidence should be findable as graph input. | Done |
| `Editor/Authoring/Spine/Hygiene` | `Editor/Authoring/Hygiene` | Authoring | `NeonBlack.Gameplay.Editor` | Hygiene is its own audit workbench. | Done |
| `Editor/Authoring/Spine/Facts` | `Editor/Authoring/Facts` | Authoring | `NeonBlack.Gameplay.Editor` | Facts is its own dictionary/provenance workbench. | Done |
| `Editor/Authoring/Spine/Validation` | `Editor/Authoring/Validation` | Authoring | `NeonBlack.Gameplay.Editor` | Scene readiness and graph-facing validation should be explicit. | Done |
| `Editor/Authoring/Grammar` | `Editor/Authoring/Vocabulary` | Authoring | `NeonBlack.Gameplay.Editor` | Vocabulary is the Unity-developer term; grammar is implementation flavor. | Done |
| `Editor/Authoring/Surfaces/AuthoringWindow` | `Editor/Authoring/Window` | Authoring | `NeonBlack.Gameplay.Editor` | The main editor window should be easy to locate. | Done |
| `Editor/Authoring/Surfaces/Inspectors` | `Editor/Inspectors/Pyralis` | Authoring | `NeonBlack.Gameplay.Editor` | Inspector overlays/handoffs should live with inspectors, not graph surfaces. | Done |
| `Tests` | `Tests` | Tests | Test assemblies | Existing package test location is acceptable; add only seam-protecting tests. | H |

## No-Move Surfaces

| Current Path | Target Path | Owner | Assembly | Reason | Phase |
|---|---|---|---|---|---|
| `Assets/Scenes/**` | No move | No Move | None | Cameron will recreate proof scenes after the source taxonomy is clean. | All |
| `Assets/**/Prefabs/**` | No move | No Move | None | Prefab migration is not evidence for source taxonomy and should not be touched without explicit request. | All |
| `Temp/**` | No move; may clear diagnostics before validation | No Move | None | Temp exports are evidence only and should not become durable source truth. | H |
| `Library/**` | No move | No Move | None | Unity-generated cache. | All |
| `Logs/**` | No move | No Move | None | Validation evidence only. | All |
| `UserSettings/**` | No move | No Move | None | Developer-local Unity state. | All |
| `Packages/com.neonblackinteractivellc.neonblackhub/Members/Public/Audio/**` | No taxonomy move | No Move | None | Public audio cleanup is separate from Pyralis source taxonomy. | All |

## Early Red-Flag Rows

| Current Path | Target Path | Owner | Assembly | Reason | Phase |
|---|---|---|---|---|---|
| `NeonBlack.Gameplay.asmdef` | Facade or remove | Glue/Core | TBD | Aggregate references nearly everything and can hide accidental coupling. Decide deliberately after module/glue asmdefs settle. | E |
| `Modules/Actor/Composition/NeonBlack.Gameplay.Modules.Actor.Composition.asmdef` | module-specific actor composition assembly | Module | `NeonBlack.Gameplay.Modules.Actor.Composition` | Actor composition assembly now matches module ownership. | E |
| `Editor/Authoring/Projections/PyralisAuthoringSetupGraphProjection.cs` | Split only by real projection owners | Authoring | `NeonBlack.Gameplay.Editor` | Very large projection coordinator; split only when ownership boundaries are clear. | G |
| `Editor/Authoring/Graph/PyralisAuthoringSetupGraphBuilder.cs` | Split only when graph compiler boundaries are clear | Authoring | `NeonBlack.Gameplay.Editor` | Large but central graph compiler; folder owner is correct, behavioral split remains optional. | G |
| `Editor/Authoring/Intent/PyralisAuthoringCapabilityDescriptor.cs` | Rename/refine descriptor registry vocabulary | Authoring | `NeonBlack.Gameplay.Editor` | Intent capability descriptors need clean naming because they drive visible taxonomy. | G |
| `Modules/Character/Sprite2D/ActorGuardInputBridge2D.cs` | Rename or relabel after move | Module | `NeonBlack.Gameplay.Modules.Character` or `Combat` | Raw bridge naming leaks plumbing into Intent; classify as input adapter or guard action support. | G |
| `Modules/Combat/BattleManager.cs` | Rename or relabel after move | Module | `NeonBlack.Gameplay.Modules.Combat` | Manager vocabulary is vague; contract should expose creator-facing combat coordination meaning. | C/G |
| `Modules/Combat/Sprite2D/HitBox2D.cs` | Keep file during class-name pass with display label `Hit Box 2D` | Module | `NeonBlack.Gameplay.Modules.Combat` | Class can stay if needed, but visible vocabulary should space lane suffixes clearly. | G |

## Validation Commands

Use these during move phases:

```powershell
dotnet build "NeonBlack.Gameplay.Editor.Tests.csproj" --no-restore -v:minimal -m:1 /nodeReuse:false /p:UseSharedCompilation=false
```

Use the full project gate only when the GUI Unity Editor is closed and the phase includes compile-affecting source or asmdef changes:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

## Completion Rule

This refactor is not complete until:

- top-level source folders communicate ownership without reading docs;
- asmdefs mirror that ownership;
- Intent vocabulary no longer exposes raw plumbing as gameplay ingredients;
- Guide, Overview, Map, Hygiene, Facts, and exports still obey projection ownership;
- docs describe the current folderbase directly;
- Cameron can recreate proof scenes from the cleaned Authoring path.
