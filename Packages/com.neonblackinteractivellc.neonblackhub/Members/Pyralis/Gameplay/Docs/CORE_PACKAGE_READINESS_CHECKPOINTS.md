# Pyralis Core Package Readiness Checkpoints

Date: 2026-05-26

This document is the active product-scope guardrail for Pyralis before deeper scene building begins.

The goal is **Beginner Prototype Ready through guided Unity setup** across the whole project. Pyralis should not make a complete game for a creator. It should guide a creator through the assets, scene roots, prefabs, components, references, and validation required to make their own basic playable prototype across any supported route.

## Product Promise

Pyralis should let a creator open Unity, choose a supported game route, and follow guided setup to build a prototype without needing to understand the internal framework architecture.

The MVP supported routes are:

- Game Shell
- Pawn-Backed Action across `Sprite2D`, `Billboard2_5D`, and `Rigged3D`
- Non-Pawn Tabletop
- Network Chain MVP

The next major platform program is the RPG Systems Platform. It is tracked separately in `RPG_SYSTEMS_ROADMAP.md` so inventory, equipment, skill trees, quests, NPC hooks, hubs, persistence, and open-zone state can be implemented without replacing the current Beginner Prototype Ready gate.

Pawn-backed and non-pawn routes both need equivalent proof standards before the package is called ready. Pawn-backed proof must cover all three official runtime lanes.

## Checkpoint Definition Of Done

Every readiness checkpoint must meet the same five-part bar:

- `Runtime`: the code actually executes the loop.
- `Authoring`: creators can assemble the route in Unity from assets, prefabs, and components.
- `Guidance`: inspectors or docs explain what to create, assign, and leave empty.
- `Validation`: common wrong wiring produces actionable warnings before or during first Play Mode.
- `Proof`: tests, validation gates, or reference scenes prove the route works.

The active project route status lives in `RUNTIME_PARITY_MATRIX.md`.

## Checkpoint 1: Game Shell MVP

Purpose: prove every Pyralis game can start from a guided boot, loading, menu, settings, credits, and scene-flow path.

Required before this checkpoint is `Ready`:

- one beginner route covering boot scene, loading scene, main menu, settings, credits, and gameplay scene transition
- guided inspectors and docs for scene navigation, settings source assignment, button/page wiring, and Build Settings
- credits page or panel support in the shell route
- validation for missing scene names, missing navigator source, missing settings source, and missing required shell UI references
- PlayMode or scene-readiness proof for menu-to-loading-to-game and return/restart flow

Current slice status:

- Runtime, authoring guidance, and source-contract validation now cover the menu/settings/credits/gameplay transition path.
- Game Shell remains `Guided Needs Proof` until a complete shell proof scene or equivalent PlayMode fixture verifies boot-to-loading-to-game and return/restart flow.

## Checkpoint 2: Pawn-Backed Action MVP

Purpose: prove one authoring model can create pawn-backed action prototypes in all official runtime lanes.

Official runtime lanes:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

Required before this checkpoint is `Ready`:

- beginner prefab/setup checklist for every lane
- session, participant, pawn definition, pawn prefab, input, movement, camera, presentation, health/damage, combat or interaction, projectile, scoring/HUD, and scene-flow guidance for every lane
- lane-specific validation for missing or mismatched presentation profiles, animation profiles, pawn components, launchers, cameras, and scoring/HUD dependencies
- proof that every lane can reach a small playable loop

Current slice status:

- Generic capability setup now guides lane-specific presentation profiles, pawn definitions, and prefabs for `Sprite2D`, `Billboard2_5D`, and `Rigged3D` without relying on starter-pack generation.
- Pawn-Backed Action remains `Guided Needs Proof` until each lane has an end-to-end playable-loop proof covering movement, camera, presentation, health/damage, projectile/combat, scoring/HUD, and scene flow.

## Checkpoint 3: Non-Pawn Tabletop MVP

Purpose: prove Pyralis supports games where participants are seats, sides, factions, cursors, or board players instead of pawn owners.

Required before this checkpoint is `Ready`:

- no-pawn setup route through `SessionDefinition`, `GameModeDefinition`, `GameSetupProfile`, and participant definitions
- board definition, pieces, move policy, action queue, turn order, selection surface, and terminal condition guidance
- validation that avoids pawn and spawn-point false positives for no-pawn routes
- proof that board selection can queue and resolve a legal move, advance or respect turn flow, and reach a terminal condition

Current slice status:

- The no-pawn quick path now points beginners through manually authored tabletop/session/profile assets, empty `Default Pawn`, empty `Spawn Points`, `TabletopBoardGridPresenter`, `TabletopBoardSelectionBridge`, `ActionQueueService`, and `BoardMoveActionResolver`.
- Runtime and editor contracts already cover board move resolution, selection queueing, presenter creation, and no-pawn setup-flow guidance.
- Non-Pawn Tabletop remains `Guided Needs Proof` until a packaged proof scene verifies the full route in one Unity-facing setup.

## Checkpoint 4: Network Chain MVP

Purpose: prove Pyralis can guide creators through local multiplayer and host/client prototype networking without turning networking into hidden engine work.

Build-or-buy decision:

- use Unity Netcode for GameObjects and Unity Transport for the MVP backend
- write Pyralis-owned authoring, validation, participant ownership, authority, roster/session services, spawn adapters, and game-rule integration
- keep direct NGO dependencies out of core gameplay feature code

Required before this checkpoint is `Ready`:

- beginner setup route that clearly separates local `PlayerInputManager` multiplayer from NGO host/client/server sessions
- `SessionDefinition.networkMode` guidance for `LocalOnly`, `NetcodeHost`, `NetcodeClient`, and `NetcodeServer`
- validation for missing `NetworkManager`, missing or wrong `UnityTransport`, missing pawn `NetworkObject`, and missing Network Prefab registration
- proof that networked sessions select networked session, roster, spawn, ownership, and participant authority services
- proof scene or equivalent Unity-facing host/client fixture for a minimal networked pawn-backed scene

Current slice status:

- The networking docs now define the build-or-buy boundary: Pyralis owns the authoring and game-rule chain, while NGO and Unity Transport own low-level networking.
- Existing runtime and editor contracts cover service selection, setup validation, participant-specific ownership, and optional networking assembly boundaries.
- Network Chain MVP remains `Guided Needs Proof` until a Unity-facing host/client proof scene verifies the complete setup path.

## Supporting Backlog From Earlier Checkpoints

These older checkpoint details remain useful route backlog, but the three MVP routes above are the active readiness gate.

### RPG Systems Platform Backlog

The RPG Systems Platform is now a dedicated post-MVP platform program. It should be built as participant-owned and actor-agnostic capability families, not as one giant RPG manager or one fantasy-game template.

Tracked roadmap:

- `RPG_SYSTEMS_ROADMAP.md`

Required phase families:

- RPG Identity, Stats, And Progression
- Inventory And Item Catalog
- Equipment And Effects
- Skill Trees
- Quests And Objectives
- NPC And Dialogue Hooks
- Hub Framework
- Persistence
- Open-Zone Readiness
- Golden RPG Sample

Still useful before RPG systems reach `Ready`:

- stats, XP, levels, and skill points with runtime tests
- item catalog and per-participant inventory services
- equipment slots and effect application contracts
- skill tree definitions with prerequisite validation
- quest/objective tracking and reward grants
- hub setup pattern for NPCs, vendors, portals, loadouts, quest boards, and minigame entrances
- save/load contracts for RPG owner state

Out of scope for the first RPG slice:

- a full open-world streaming stack
- a polished commercial RPG campaign
- replacing existing pawn, tabletop, scoring, action, scene-flow, or networking routes

### Rules-Driven Tabletop Backlog

Already present:

- board, space, piece, occupancy, movement, capture, and turn runtime state
- board, piece, phase, turn order, and game mode authoring assets
- action queue service
- queued board-move and turn-advance resolvers
- authorable board move policy primitives for common grid movement shapes and optional capture
- authorable offset/jump move policies for knight-style and checker-style movement primitives
- authorable board terminal conditions for side-eliminated and objective-occupied outcomes
- editor validation and tests for the current foundations

Still useful before tabletop reaches `Ready`:

- expanded legal move policy coverage for named board-rule contracts such as directional token movement without bundled route generators
- richer capture and occupancy policy combinations that can be authored without code
- expanded terminal conditions such as no legal moves, score threshold, round limit, and multi-objective states
- setup docs that walk a beginner through creating a simple two-seat board game in Unity

Out of scope for the MVP:

- full official named-game completeness
- named-game AI
- online multiplayer
- visual node editing
- advanced card stack timing

### Local Two-Player Side-Scrolling Shooter Backlog

Already present:

- participant and session services
- 2D pawn movement stack
- projectile definitions, fire modes, magazines, 2D launchers, hitscan, prefab projectiles, impact effects, and tests
- runtime pattern setup assets that can compose realtime character control plus projectile combat

Still useful before the Sprite2D pawn action lane reaches `Ready`:

- local two-player setup profile and generic setup guidance
- a shooter-ready input and participant setup path
- weapon prefab or launcher setup path that a beginner can assign in Unity
- scene readiness validation for required pawn, launcher, camera, participant, and scoring pieces
- a small proof scene showing two local participants can move, fire, score, and be distinguished

Out of scope for the MVP:

- procedural stages
- online co-op
- inventory-heavy weapon systems
- polished art, bosses, or campaign flow

### FPS And 3D Projectile Backlog

Already present:

- 3D pawn and camera foundations
- projectile definitions and 3D launcher path
- hitscan and projectile-prefab command planning

Still useful before the Rigged3D or Billboard2_5D pawn action lanes reach `Ready`:

- first-person or over-shoulder setup guidance
- 3D projectile weapon prefab setup path
- camera and aim validation
- clear docs for converting the same projectile assets between 2D shooter, 3D shooter, turret, trap, or scripted use

Out of scope for the MVP:

- full FPS controller package replacement
- networked shooter architecture
- advanced recoil, sway, ballistics, or weapon attachment systems

### Unity-Only Authoring UX Backlog

Already present:

- guided inspectors for many shared definitions and profiles
- setup profile, runtime capability, and optional route-contract validation
- generic setup guidance for pawn and projectile assets

Still useful before routes reach `Ready`:

- guided inspectors for every new creator-facing rule, policy, and setup asset
- Create Asset menu coverage for all intended authoring assets
- cookbook facts and optional route contracts for tabletop MVP and pawn action lane variants
- scene readiness checks for missing services, participants, pawns, boards, cameras, action queues, and launchers
- docs that say exactly what a beginner creates first, second, and third

Out of scope for the MVP:

- full visual scripting replacement
- one-click generation of a finished commercial game

### Runtime Parity Backlog

Already present:

- runtime parity matrix
- feature inventory
- roadmap
- EditMode and PlayMode coverage for the current foundations

Still useful before routes reach `Ready`:

- keep each route/lane graded with the MVP status labels in `RUNTIME_PARITY_MATRIX.md`
- update docs whenever a lane changes status
- require tests for core state, authoring validation, and scene-facing behavior where appropriate
- preserve actor-agnostic boundaries so tabletop, shooter, FPS, and brawler work can share vocabulary

Out of scope for the MVP:

- treating every lane as production-ready before it has a proof path
- adding new genre labels without runtime proof

## Active Development Gate

Near-term work should advance these checkpoints in order:

1. Game Shell MVP, because every future prototype needs loading, menu, settings, credits, and scene flow.
2. Pawn-Backed Action MVP across `Sprite2D`, `Billboard2_5D`, and `Rigged3D`, because the official pawn route must be lane-honest before game scene work depends on it.
3. Non-Pawn Tabletop MVP, because tabletop support must remain a real no-pawn authoring path.
4. Network Chain MVP, because online or host/client prototypes need honest setup, authority, and validation before `.io`-style work begins.
5. Friend trial and friction capture, once the routes have proof paths.
6. Package docs alignment after each slice, so active docs never fall behind the real authoring surface.
7. RPG Systems Platform, after the MVP route proof work has a clean checkpoint or when a slice directly supports a validated creator route.

If a proposed task does not move one of these checkpoints closer to its definition of done, defer it.

## Pre-Scene Validation Gate

Before scene or prefab development starts, close the Unity Editor and run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

This is the current project-owned foundation gate. It runs restore/build, Unity EditMode, Unity PlayMode, and a final restore/build so the workspace is left ready for fast C# checks again. Unity tests must run without `-quit`; Unity Test Framework 1.6 warns that command-line tests do not work when `-quit` is supplied.

The gate also protects the local Unity layout before launching batchmode tests. If `UserSettings\Layouts\CurrentMaximizeLayout.dwlt` contains the Unity Version Control window, the script temporarily swaps in the default layout and restores the original layout afterward. This keeps validation rooted in Game Studio Core and prevents Unity Version Control/Gluon from opening a remembered unrelated workspace during test runs.

The gate should pass before opening deeper scene/prefab work. If it fails, fix the validation failure first unless the next task explicitly changes the failing system.
