# Pyralis Setup: Start Here

This is the first-scene path. Use it when you are opening Unity and asking, "What do I create first?"

## Native-First Setup Rule

For your first playable proof, use only native Unity actions:

- **Hierarchy/Project**: create objects and assets from Unity menus.
- **Inspector Add Component**: use search to add scripts/components to game objects.
- **Field assignment**: drag references to serialized fields in the Inspector.
- **Authoring Window**: use this for route awareness and validation, not for scene auto-construction.

### 1P Movement First Proof (required native route)

Run this route before adding score, combat, networking, or extras:

- Open `authoring-native-1p-proof-checklist.md` (repo `docs/` root) and follow it in sequence.
- In the Authoring Window, stay in **Overview -> Map -> Validate** until
  - Do Now is clear
  - required links are green
  - Play blockers are clear.
- Press Play and confirm the 1P movement proof passes.
- Only then add one optional lane and re-run the same proof check.

If your route terms are still unclear (`Definition`, `Profile`, `Pattern`), read
`AUTHORING_MODEL.md` right after this checklist.

## End-to-End First Test

You need not build every support system first. Start with one `GameplaySessionBootstrap`,
one session chain, one participant, one pawn route, one spawn point, and one input profile.
That is enough for a first proof of life.


## Use The Inspector Guide

Core Pyralis setup assets include compact Inspector guidance, and the **Pyralis Authoring Window** is the central setup UI layer. Keep the Authoring Window open while you select scene objects and assets; it summarizes the selected route, validation state, next step, and Pyralis component stack. Use the Inspector for direct field editing and field-local help.

The top of the Authoring Window shows the **Active Setup**. Leave it following selection for quick browsing, or use **Pin Selection As Active Setup** on a bootstrap, session, game mode, or setup profile when you want the window to keep tracking one game setup while you inspect child objects, components, prefabs, and assets. **Overview** and **Map** use the active setup; **Guide** still explains the current Unity selection.

Start from the `GameplaySessionBootstrap` Inspector whenever you are unsure where you are in setup. Its **Setup Flow** monitor reads the scene root, `SessionDefinition`, `GameModeDefinition`, `GameSetupProfile`, selected runtime capabilities and resolved runtime patterns, participants, pawns, spawn points, camera, input, playfield, and scoring settings, then shows the next required step first.

The Authoring Window has six modes:

- **Overview**: shows the route-aware decision dashboard: current setup state, Best Next Action, first proof, and the next one to three useful moves.
- **Intent**: lets you describe the route with world/playfield, control shape, presentation lane, and capability toggles without applying presets.
- **Guide**: shows graph-filtered route rows for the active setup, then explains what the selected script or asset does, which values matter, and what to wire first.
- **Map**: shows the setup topology, current links, readiness rows, scene-surface evidence, and Inspector jump targets without editing fields.
- **Validate**: shows validation issues for the selected setup object.
- **Facts**: shows the read-only cookbook and dictionary so coverage and provenance can be audited.

For the first-pass proof, use this native flow:

- create objects in **Hierarchy/Project**
- add components through **Inspector → Add Component** search on selected objects
- make field assignments in the **Inspector**

The Authoring Window also shows a **Setup Chain** map for the current selection:

```text
Bootstrap -> Session -> Game Mode -> Setup Profile -> Runtime Capabilities -> Participants -> Pawns
```

Use it as a setup-prep surface, not as another Inspector. It diagnoses which part of the chain is ready, which part needs setup, and when a core link is missing it tells you the native Unity path to create the asset or object and which Inspector field owns the assignment. Use **Inspect Asset** only when you need to jump to the Inspector for field-level editing.

The Setup Flow monitor is intentionally safe. It is an expert guide, not an auto-builder. It does not create a whole scene, wire referenced assets, edit prefabs, choose art, choose character speed, or make design choices for you. It tells you what kind of Unity object to create, which Pyralis component belongs on it, which field to drag into, and why that step matters. Its buttons mainly select/ping and copy the checklist; any helper action (like adding `PyralisGameplayLifetimeScope` or restoring defaults) is optional.

For a first-playability proof, keep this as your mental baseline:
- movement proof first (pawn spawns, input, and one visible movement loop),
- then add one optional system at a time (score, combat, UI, network, etc.).

The Authoring Window is the route guide. Open it from the Setup Flow when you need overall progress, intent shaping, selection guidance, setup mapping, validation, or fact coverage. Use **Overview** for the route-aware next decision, **Intent** for route/capability thinking, **Guide** for the selected script or asset, **Map** for setup connections and scene-surface evidence, **Validate** for issues, and **Facts** when you need to audit where guidance came from. Overview reads the active route before judging readiness: pawn-backed routes require participant pawns and spawn points, while tabletop or other no-pawn routes treat empty pawn fields as correct.

Use the Authoring Window as a senior setup companion, not a scene generator. It should explain why a route needs a pawn, board surface, camera/cursor, action resolver, input profile, or UI presenter, then send you to the normal Unity object or Inspector field where you make the creative choice.

When a `GameSetupProfile` is active, the Authoring Window projects the same resolved setup graph through each tab. **Intent** owns route shaping, **Guide** owns graph-filtered route rows and selected-object help, **Overview** owns the next one to three moves, **Map** owns topology, **Validate** owns readiness issues, and **Facts** owns the full dictionary. The tabs read selected runtime capabilities first and optional runtime patterns second, then explain:

- the **Intent** tab DNA axioms, presentation lane, and Engine Spine capabilities that define what kind of game route is being authored
- design questions to answer before setup, such as what the player controls, what kind of space the game happens in, and what the first proof of interaction should be
- what kind of route the setup currently resembles, such as pawn action, brawler/fighter, tabletop, action-selection, projectile, or scoring loop
- what each selected capability changes in the game
- how the ground, board, arena, room, platform, card table, or environment affects Pyralis even when it is mostly plain Unity art
- what Unity object or component to wire next
- which fields and profiles hold customization, such as movement speed, presentation mode, combat timing, projectile cadence, scoring rules, HUD labels, board spaces, or action buttons
- which optional capabilities naturally come next

The Authoring Window guidance is route-aware:

- `GameSetupProfile` stores the runtime capability ingredients you selected. Optional `RuntimePatternDefinition` assets can enrich advanced route metadata, but they are not required for the generic setup path.
- Intent is the visible route-shaping surface. When a setup profile is active, Intent writes matching runtime capability rows from reflected capability descriptors so Overview, Guide, Map, and Validate read one shared graph.
- `GameModeDefinition` uses its setup profile to explain the active route.
- `SessionDefinition` explains whether participants need pawns, input, seats, hands, factions, camera, cursor, or menu surfaces.
- `GameplaySessionBootstrap` checks the assigned session chain and shows the consolidated Setup Flow checklist.

Use the visible Authoring Window summary for route progress, first proof, setup-map status, scene-surface evidence, and validation. Inspector guidance should stay compact: field tooltips, local validation, and an **Open Authoring Window** handoff. Broader wiring steps, valid path choices, and common mistakes belong in the Authoring Window.

## The Short Version

1. Pick the route first:
   - pawn-backed movement route -> realtime character pattern, participant, pawn, input, spawn point, camera
   - no-pawn route (board/card/cursor/action) -> tabletop/action/camera patterns, participant seats, control surface, UI or board state
2. In the Project window, open the folder that should own this proof, check that the Project content pane/breadcrumb is inside that folder, then create the needed definition/profile assets there with **Create -> NeonBlack**.
3. Open your session + profile assets (`SessionDefinition`, `GameSetupProfile`, etc.) and confirm whether the flow is pawn-backed.
4. In Hierarchy, right-click -> **Create Empty** and name it `Gameplay Root`.
5. Select `Gameplay Root` and use the Inspector **Add Component** search bar to add:
   - `GameplaySessionBootstrap`
   - `PyralisGameplayLifetimeScope`
6. Assign the `SessionDefinition` on `GameplaySessionBootstrap` and wire the minimum required game state:
   - `SessionDefinition` -> your session asset
   - one default participant
   - for pawn routes: one `Default Pawn` and one `PawnDefinition`
   - keep the standard bootstrap-owned service path enabled for this first proof so gameplay-state services are created automatically.
7. Add only the required proof objects:
   - pawn routes: one `Spawn Point` transform and assign it to `Spawn Points`
   - input route for the route (`InputProfile` on participant; keep `Player Input Manager` empty unless you need local join)
   - no-pawn routes: control-surface objects for camera/cursor/menu/board/action selection instead of pawn spawn points
   - keep bootstrap and pawn prefab in the same test scene so input/service routing stays local
   - optional `Camera Aspect` bounds object if you want a visible camera boundary check in the same run
8. Press Play and run the shortest proof:
   - pawn route: confirm one pawn spawns at the assigned spawn point and moves from input
   - no-pawn route: confirm one control surface interaction reaches a resolver (selection, action, camera move, or board command)
   - confirm gameplay services initialize without blocked setup issues
   - confirm camera view stays inside the intended bounds if bounds are set
9. Add score, combat, HUD, and extras only after this proof is reliable.

Think of it this way: `GameplaySessionBootstrap` is the first runtime object. `SessionDefinition` is the first authoring asset it reads.

### Minimal 1P Playable Proof (copy when starting fresh)

- Required:
  - one `Gameplay Root` with `GameplaySessionBootstrap` (+ optional `PyralisGameplayLifetimeScope`);
  - `SessionDefinition -> GameModeDefinition -> GameSetupProfile -> runtime capability rows`;
  - one default participant with `Input Profile` and `Default Pawn`;
  - one pawn prefab with `PawnRoot`, `Motor2D` + `Motor2DInputAdapter` + `Pawn2DMovementComponent` + `Pawn2DPresentationComponent` (or 2.5D/3D equivalent);
  - the pawn prefab inspected in Prefab Mode or Inspector so every required component/reference is understood and editable. The guided proof should treat the prefab as user-owned setup, not a hidden generated answer;
  - one `PawnDefinition.pawnPrefab`;
  - one `GameplaySessionBootstrap.SpawnPoints` transform;
  - the standard bootstrap-owned service path on.
- Optional for this pass:
  - HUD, scoring, combat, hazards, scene flow, pickups, projectiles, networking, local join.
- Validation:
  - no blocked item in `Overview` → press Play → one pawn spawns and moves with input.

## Step 1: Pick The Game Surface

Choose the closest starting point:

| If you are making... | Start with these runtime patterns | Pawn needed? |
|---|---|---|
| 2D character game | Realtime Character, Camera/Cursor Control | Usually |
| 2D arcade score loop | Realtime Character, Scoring/Objectives, Camera/Cursor Control | Usually |
| Brawler or fighter | Realtime Character, Combat, Animation/Presentation | Usually |
| Shooter or projectile-heavy game | Realtime Character or Camera/Cursor Control, Projectile Combat, Combat | Maybe |
| Board game or tabletop variant | Board/Card/Tabletop, Turn/Menu Action, Camera/Cursor Control | No |
| Card game | Board/Card/Tabletop, Turn/Menu Action, Scoring/Objectives, Camera/Cursor Control | No |
| Camera-only interaction | Camera/Cursor Control, Scoring/Objectives if needed | No |

The patterns are not genres. They are setup signals. A game can use several at once.

## Step 2: Create The Authoring Assets

Before creating assets, remember:

- Definitions describe what exists and what points to what.
- Profiles describe tuning, behavior, presentation, or settings.

Manual path:

- `GameSetupProfile`
- optional existing `RuntimePatternDefinition` assets only when the generic runtime capability rows need advanced reusable metadata
- `SessionDefinition`
- `GameModeDefinition`
- `ParticipantDefinition`
- `PawnDefinition` only for pawn-backed games
- supporting profiles such as input, movement, combat, camera, settings, presentation, and animation

Create new `RuntimePatternDefinition` assets only when the cookbook facts do not describe the kind of setup you are building. For a first afternoon game, start with Intent capabilities and existing route contracts; do not begin by designing a new setup taxonomy.

Wire the assets in this order:

1. Use the Authoring Window **Intent** tab to choose DNA axioms, presentation lane, and Engine Spine capabilities. When a `GameSetupProfile` is active, those choices save to `runtimeCapabilities`. Optional `RuntimePatternDefinition` assets are advanced route contracts for extra metadata.
2. Assign `GameSetupProfile` to `GameModeDefinition.setupProfile`.
3. Assign `GameModeDefinition` to `SessionDefinition.defaultGameMode`.
4. Assign participant definitions to the `SessionDefinition`.
5. For pawn games, assign each participant a `PawnDefinition`.

Template or scaffold tooling is not the current first-test path. Use manual native authoring while validating the guide. After a route is proven repeatable, future tooling may capture that route as editable project assets, but it should never replace the manual proof that the setup chain is understandable.

## Step 3: Assign The Bootstrap

Create one empty GameObject:

- `Gameplay Root`

Attach using the Inspector Add Component search:

- `GameplaySessionBootstrap`
- `PyralisGameplayLifetimeScope`

On `GameplaySessionBootstrap`, assign the `SessionDefinition` you just wired. Then assign only the scene references your setup actually uses:

- `Session Definition`
- `Spawn Points` only if pawns spawn into the scene
- `Player Input Manager` only for local player joining
- `Camera Rig Controller` only if using the shared Pyralis camera flow

Leave these on for first-scene proofs that use the standard bootstrap-owned service path:

- `Auto Create Core Services`
- `Inject Loaded Scenes On Build`

At runtime, the bootstrap creates service children for scene loading, time, camera shake, session state, participant roster, spawning, and input routing.

After assigning the `SessionDefinition`, keep the `GameplaySessionBootstrap` selected and work down the **Setup Flow** list. Fix the selected intent's Do Now items first. Treat recommended items as proof enhancers, not universal requirements. Optional items can stay empty until the selected intent needs them.

For the first route proof, treat these as Do Now only when the selected intent asks for them:

- `Session Definition`
- one participant with required input profile for that route
- pawn routes: one participant with `Default Pawn` and one `Spawn Point`
- no-pawn routes: required control-surface assets (camera/cursor/menu/board/action)
- `Auto Create Core Services` for the standard bootstrap-owned service path

Treat these as optional until route proof works:

- camera/profile objects (if bounds are not in play yet)
- scoring roots/services
- scene flow/menu extras
- combat/projectile/health systems
- board/card action systems

## Step 4: Add Optional Scene Roots

Only add what matches your setup profile.

| Root object | Add this when... | Common components |
|---|---|---|
| `Camera Root` | the game has a camera rig, cursor, board view, follow camera, split screen, or 2D visible bounds | `CinemachineCameraRigController` plus `CameraRigProfile`, GameObject -> Cinemachine -> Cinemachine Camera assigned as Shared Camera Behaviour, physical Main Camera assigned as Target Camera, and Cinemachine Brain verified on that physical Main Camera. The normal Cinemachine route keeps or creates one real Unity Camera, usually the default Main Camera; do not delete it unless you intentionally replace it with another single physical render camera. The Cinemachine Camera composes the view; the physical Main Camera renders it and usually keeps the `MainCamera` tag. For a shared-camera proof, keep one enabled physical render camera; disable or remove accidental extra physical Camera objects unless they are intentional overlay, split-screen, minimap, or render-texture cameras. Unity usually adds the Brain when you create a Cinemachine Camera; add it manually only if missing. For 2D movement or bounded views, set Main Camera > Camera > Projection to Orthographic or use an orthographic `CameraRigProfile`. |
| `Input Root` | multiple local players can join during play | Unity `PlayerInputManager`, project input prefab only if you own one |
| `UI Root` | the game has HUD, menus, board UI, card UI, turn UI, action buttons, prompts, or settings screens | Canvas, EventSystem, `ParticipantHealthHudBinder`, `ParticipantFeedbackHudPresenter`, `UIManager`, menu/settings presenters |
| `Playfield Root` | the game has bounds, board spaces, card zones, encounter zones, pickups, hazards, or generated chunks | placed anchors, zones, spawners |
| `Scoring Root` | the game tracks score, timers, victory points, resources, or round results | `ParticipantScoreService` |
| `Settings Root` | the game needs volume, fullscreen, deadzone, or persistent settings | `SettingsManager` |
| `Scene Flow Root` | the game has fades, menu-to-game transitions, restart, or return-to-menu | `SceneFader` |

Beginner rule: if you are unsure, do not add it yet.

The environment and background are allowed to be ordinary Unity content. A platform, wall, terrain mesh, tilemap, flat image backdrop, skybox, board square, table prop, card slot, room, menu canvas background, or arena boundary does not need a Pyralis script just to exist. Pyralis should guide how those objects affect gameplay, not choose the project's art pipeline.

Common valid background/world-art routes:

- 2D flat sprite or imported PNG backdrop behind actors
- 2D Tilemap or TilemapCollider2D for tile-authored worlds
- 2.5D layered sprites, props, depth sorting, and colliders
- 3D meshes, terrain, skybox, lighting, and world colliders
- Canvas images for menus, HUD panels, maps, overlays, or title screens
- future procedural chunks, rooms, terrain pieces, or encounter zones when generation owns placement

Those objects affect Pyralis through the parts Unity and gameplay systems can observe:

- colliders and triggers
- physics layers and ground layer masks
- camera/playfield bounds
- spawn, pickup, hazard, encounter, and safe-zone anchors
- board/card/action coordinates or selectable surfaces
- sorting, depth, and presentation rules

Use `PlayfieldProfile` when the mode needs authored bounds. Use the assigned `CinemachineCameraRigController` when camera-aware spawners, hazards, pickups, generated content, or framing need visible bounds; reserve `Camera Bounds Source` for specialized custom `ICameraBoundsProvider` services. Use helpers such as `TilemapGround`, `DepthSorting`, `ArenaZone`, and future procedural-generation surfaces only when they match the scene; otherwise plain Unity art and geometry with correct layers/colliders is enough.

When you do add one, keep the feedback loop small:

- camera: first prove the view follows or frames the playfield
- input: first prove one participant receives the intended action
- HUD: first prove one label, panel, or button updates from a Pyralis service
- menus: first prove one button reaches scene flow or settings
- board/card/action UI: first prove one selection can call the intended action

## Step 5: Decide Whether You Need A Pawn

Use a pawn when a participant owns an actor body with movement, combat, health, animation, traversal, pickups, or feature modules.

Do not use a pawn just because there is a player. Board games, card games, camera-only scenes, menu combat, and many turn-based games can start with no pawn prefab.

For pawn-backed games, the pawn prefab should include:

- `PawnRoot`
- `ActorAnimationDriver` if animated
- one supported movement/presentation stack

Pyralis will not tell you which sprite to draw or the perfect character speed. It should help you choose where those decisions live: art and rendering choices belong in presentation profiles/components, while speed, acceleration, jump feel, braking, and similar feel values belong in movement profiles and the movement components that read them.

Common 2D movement stack:

- `Motor2D`
- `Motor2DInputAdapter`
- `Pawn2DMovementComponent`
- `Pawn2DPresentationComponent`
- `PawnCombatBehaviour2D` when combat is needed

Common 3D or 2.5D movement stack:

- `Motor3D`
- `Pawn3DInputModule`
- `Pawn3DMovementComponent`
- `Pawn3DTraversalComponent`
- `Pawn3DPresentationComponent`
- `PawnCombatBehaviour` when combat is needed

## Step 6: Press Play For The First Proof, Then Add Extras

Before adding more features, run this proof pass first:

- Required setup for movement proof:
  - `GameplaySessionBootstrap` reports no blocked setup issues.
  - one pawn prefab has `PawnRoot` and a full 2D movement stack (`Motor2D` + `Motor2DInputAdapter` + `Pawn2DMovementComponent` + `Pawn2DPresentationComponent`) and matching input profile assignment.
  - one spawn point is assigned, and the pawn spawns there.
  - movement input visibly moves exactly one participant.
- Required gameplay-state proof:
  - core services are present (session/state/roster/spawn/input) and no blocking service readiness issues remain.
- Required camera bounds proof (only if you added bounds objects):
  - camera bounds source receives a valid bounds target and the camera/clamped gameplay area is applied.
- Recommended next checks before moving on:
  - no missing scripts or duplicate assemblies in Console
  - only then add one optional lane feature (scoring, combat, HUD) and re-run this proof pass.

## What To Read Next

Read only the guide that matches your immediate task:

- Full setup manual: `MANUAL.md`
- Asset relationship map: `AUTHORING_MODEL.md`
- First scene root: `Prefabs/Bootstrap_Example_Setup.md`
- Pawn prefab: `Prefabs/Pawn_Setup.md`
- Camera: `Prefabs/Camera_Setup.md`
- Combat and projectiles: `Prefabs/Combat_Definitions_Setup.md`
- Health and hitboxes: `Prefabs/Health_Combat_Setup.md`
- Hazards and difficulty: `Prefabs/Hazard_Difficulty_Setup.md`
- Pickups: `Prefabs/Pickups_Setup.md`
- Scoring: `Prefabs/Scoring_Setup.md`
- UI/HUD: `Prefabs/UI_HUD_Setup.md`
- Board, card, seat, hand, faction, or tabletop: `Prefabs/Board_Card_Tabletop_Setup.md`
- Scene flow: `Prefabs/Scene_Flow_Setup.md`
- Deeper architecture: `CANONICAL_SETUP.md`

## Mental Model

Pyralis setup works like this:

- `GameSetupProfile` says what kind of runtime the game expects.
- `SessionDefinition` says who is playing and which mode starts.
- `GameplaySessionBootstrap` creates the shared runtime services.
- Optional scene roots provide camera, UI, scoring, settings, scene flow, and playfield behavior.
- Pawns are optional actor bodies, not required for every game.

