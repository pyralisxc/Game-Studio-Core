# Pyralis Runtime Parity Matrix

This matrix tracks whether Pyralis routes and runtime lanes are ready for Beginner Prototype Ready through guided Unity setup.

## MVP Status Labels

- `Ready`: authored, guided, validated, and proven.
- `Guided Needs Proof`: setup exists, but runtime proof or scene/sample proof is thin.
- `Foundation Only`: core code exists, but beginner authoring is not real yet.
- `Not Started`: missing as a platform capability.
- `Deferred`: intentionally outside the current MVP.

## Five-Part Completion Bar

Every route or runtime lane must satisfy the same bar before it can be marked `Ready`:

- `Runtime`: the code actually executes the loop.
- `Authoring`: creators can assemble the route in Unity from assets, prefabs, and components.
- `Guidance`: inspectors or docs explain what to create, assign, and leave empty.
- `Validation`: common wrong wiring produces actionable warnings before or during first Play Mode.
- `Proof`: tests, validation gates, or reference scenes prove the route works.

## MVP Route Dimensions

The MVP readiness gate tracks these dimensions:

- Game Shell
- Pawn-Backed Action / `Sprite2D`
- Pawn-Backed Action / `Billboard2_5D`
- Pawn-Backed Action / `Rigged3D`
- Non-Pawn Tabletop
- Network Chain MVP

## MVP Route Matrix

| Capability | Game Shell | Pawn Action Sprite2D | Pawn Action Billboard2_5D | Pawn Action Rigged3D | Non-Pawn Tabletop |
| --- | --- | --- | --- | --- | --- |
| Route setup profile | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Starter pack | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Scene root setup | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Session and participant setup | Foundation Only | Ready | Ready | Ready | Guided Needs Proof |
| Pawn or no-pawn correctness | Deferred | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Prefab requirements | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Deferred |
| Input ownership | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Foundation Only |
| Movement | Deferred | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Deferred |
| Camera | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Foundation Only |
| Presentation and animation | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Foundation Only |
| Health, damage, and defeat | Deferred | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Deferred |
| Combat or interaction | Deferred | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Deferred |
| Projectiles/guns | Deferred | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Deferred |
| Scoring/HUD | Foundation Only | Foundation Only | Foundation Only | Foundation Only | Foundation Only |
| Board/rules/action queue | Deferred | Deferred | Deferred | Deferred | Guided Needs Proof |
| Turns/phases | Deferred | Deferred | Deferred | Deferred | Foundation Only |
| Menu/loading/settings/credits | Guided Needs Proof | Foundation Only | Foundation Only | Foundation Only | Foundation Only |
| Scene flow | Guided Needs Proof | Foundation Only | Foundation Only | Foundation Only | Foundation Only |
| Setup flow validation | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Scene/prefab readiness validation | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Docs | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| EditMode proof | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| PlayMode proof | Foundation Only | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof | Guided Needs Proof |
| Known limitations | Ready | Ready | Ready | Ready | Ready |

## Route Audit Summary

### Game Shell

Current status: `Guided Needs Proof`.

Strong foundations:

- `SceneLoader`, `SceneFader`, `LoadingScreenController`, `MainMenuManager`, `SettingsManager`, and `SettingsScreen` exist.
- `MainMenuManager` now exposes a credits panel route beside settings and co-op pages.
- Guided inspectors explain scene navigation, loading, menu, settings, credits, and required scene navigator wiring.
- Scene-flow docs now describe one beginner Game Shell MVP route across boot, loading, menu, settings, credits, and gameplay transition.

Main blockers before `Ready`:

- importable sample or proof scene for the complete shell route is still needed
- PlayMode or scene validation proof for the complete shell route is thin

### Pawn-Backed Action / Sprite2D

Current status: `Guided Needs Proof`.

Strong foundations:

- 2D pawn movement, input, combat, pickup, hazard, projectile, scoring, and camera pieces exist.
- Starter-pack generation creates `Sprite2DPawnDefinition`, `Sprite2DPresentationProfile`, and `Sprite2DPawnPrefab` with the 2D pawn stack.
- Projectile and 2D runtime tests exist for important foundations.

Main blockers before `Ready`:

- beginner route needs one clear Sprite2D prefab checklist
- scoring/HUD and scene-flow proof need to be tied to the route
- local participant and projectile setup need a beginner-readable proof path

### Pawn-Backed Action / Billboard2_5D

Current status: `Guided Needs Proof`.

Strong foundations:

- `PawnPresentationProfile` names `Billboard2_5D` as an official presentation lane.
- 3D pawn composition and camera foundations can carry the lane.
- Animation/presentation profile wiring exists.
- Starter-pack generation creates `Billboard25DPawnDefinition`, `Billboard25DPresentationProfile`, and `Billboard25DPawnPrefab` with the 3D pawn stack.

Main blockers before `Ready`:

- beginner route needs explicit Billboard2_5D prefab and presentation guidance
- validation must catch presentation/profile/component mismatches
- proof must show movement, camera, presentation, damage/combat or interaction, and scene flow in this lane

### Pawn-Backed Action / Rigged3D

Current status: `Guided Needs Proof`.

Strong foundations:

- 3D pawn movement, traversal, camera, combat, projectile, and Animator-driven presentation foundations exist.
- Rigged 3D is an official presentation lane.
- 3D projectile launcher and movement foundations have tests.
- Starter-pack generation creates `Rigged3DPawnDefinition`, `Rigged3DPresentationProfile`, and `Rigged3DPawnPrefab` with the 3D pawn stack.

Main blockers before `Ready`:

- beginner route needs explicit Rigged3D prefab and Animator mapping guidance
- validation must catch missing Animator/profile/component wiring
- proof must show the same action-route bar as Sprite2D and Billboard2_5D

### Non-Pawn Tabletop

Current status: `Guided Needs Proof`.

Strong foundations:

- board definitions, piece definitions, move policies, turn order, terminal conditions, action queue, board move resolver, selection bridge, and grid presenter exist.
- tabletop capability setup can create or assign baseline rules assets through native Unity authoring.
- docs explicitly state that no pawn is required.
- setup-flow validation preserves empty `Default Pawn` and empty spawn points as valid no-pawn tabletop choices.
- runtime tests prove board move resolution, selection bridge queueing, and grid presenter scene-object creation.

Main blockers before `Ready`:

- packaged proof scene is still needed for one-click beginner validation
- proof scene should show board selection, action queue, legal move resolution, turn flow, and terminal condition behavior together

### Network Chain MVP

Current status: `Guided Needs Proof`.

Strong foundations:

- core gameplay remains transport-agnostic while the optional `NeonBlack.Gameplay.Networking` assembly owns NGO-specific services.
- `SessionDefinition.networkMode` selects `LocalOnly`, `NetcodeHost`, `NetcodeClient`, or `NetcodeServer`.
- `GameplaySessionBootstrap` switches to networked session, roster, spawn, ownership, and authority services for NGO modes.
- setup validation checks `NetworkManager`, `UnityTransport`, pawn `NetworkObject`, and Network Prefab registration.
- runtime tests cover network-mode sanitizing, networked service selection, setup validation, and server-side spawn/despawn foundations.

Main blockers before `Ready`:

- a Unity-facing host/client proof scene is still needed.
- beginner docs should stay explicit that Pyralis uses NGO/Unity Transport for low-level networking and owns the authoring, validation, participant ownership, authority, and game-rule integration chain.
- later competitive/online features such as rollback, prediction, movement/projectile reconciliation, relay/lobby/matchmaking, and replicated animation state remain enhancement lanes on top of the current seams.

## Supporting Capability Inventory

This supporting inventory preserves the older feature-family view. The MVP Route Matrix above is the active readiness gate.

| Capability | Authoring Assets | Runtime State | Runtime Services | Editor Validation | Test Coverage | Sample Path | MVP Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Action and targeting | `ActionDefinition` | `ActionExecutionContext`, target descriptors, validation and resolution results, `QueuedAction` | `IActionResolver`, `IActionQueueService`, `ActionQueueService` | Definition validation | Runtime and editor tests | Generic projectile/action guidance uses this foundation | Guided Needs Proof |
| Guns and projectiles | `ProjectileDefinition`, `FireModeDefinition`, `ProjectileImpactDefinition` | Fire requests, spawn commands, magazine state | 2D and 3D launchers, pooling handle, impact effect player | Definition validation | Runtime and editor tests | Generic projectile/fire-mode/impact guidance | Guided Needs Proof |
| Runtime capability setup | `GameSetupProfile`, optional `RuntimePatternDefinition` | Capability/profile validation data | Game mode setup-profile validation | Custom inspectors and validation tests | Editor tests | Generic capability setup and optional contracts | Guided Needs Proof |
| Core Rules Spine | `BoardDefinition`, `BoardPieceDefinition`, `BoardMovePolicyDefinition`, `BoardTerminalConditionDefinition`, `PhaseDefinition`, `TurnOrderDefinition`, `GameModeDefinition` links | `BoardRuntimeState`, `BoardSpaceState`, `BoardPieceState`, `BoardCoordinate`, `TurnRuntimeState`, `BoardMoveActionPayload`, `BoardMovePolicy`, `BoardTerminalCondition` | `IBoardStateService`, `IBoardMovePolicy`, `IBoardTerminalCondition`, `ITurnOrderService`, `BoardMoveActionResolver`, `TurnAdvanceActionResolver` | Board, piece, move-policy, terminal-condition, phase, turn, and game-mode validation | Runtime and editor tests | Tabletop capability setup uses this spine | Guided Needs Proof |
| Board/card/tabletop | Board, move-policy, terminal-condition, and turn assets | Board occupancy, movement, capture state, shape and offset move policy evaluation, terminal-state evaluation, turn cursor | Board move and turn-advance action resolvers | Board, move-policy, terminal-condition, turn, and setup-flow claim validation | Foundation tests and authoring source contracts | Generic tabletop setup; scene/UI proof still pending | Guided Needs Proof |
| Realtime 2D arcade | Existing profiles, pawns, hazards, pickups, scoring | 2D pawn and feature state | Explicit scene/game flow services | Existing inspectors and setup checks | Existing EditMode and PlayMode coverage | Existing arcade setup paths | Guided Needs Proof |
| 3D brawler | Existing pawn, combat, traversal, enemy, zone, and camera definitions/profiles | 3D pawn, combat, traversal, enemy, and encounter runtime | Explicit runtime modules | Existing custom inspectors | Existing EditMode and PlayMode coverage | Generic brawler setup and existing scenes | Guided Needs Proof |
| Side-scrolling shooter | Realtime pattern plus projectile definitions | Projectile planning and 2D launcher state | 2D pawn, projectile launcher, scoring/hazard services | Partial via projectile/pawn/setup validation | Projectile and 2D tests | Not yet as a polished sample | Foundation Only |
| Chess variant | Board, piece, and turn authoring foundations | Board occupancy, piece capture, and turn order state | Board and turn contracts only | Foundation validation | Foundation tests | Not yet | Foundation Only |
| FPS | Realtime pattern plus projectile definitions | Projectile planning and 3D launcher state | 3D pawn/camera/projectile services | Partial via projectile/pawn/setup validation | Projectile and 3D tests | Not yet as a polished sample | Foundation Only |

## Core Rules Spine

The Core Rules Spine is the shared layer for non-realtime and rules-driven games. It intentionally does not decide named-game movement, card text, tactical AP costs, or custom win conditions yet. It does provide the reusable state and asset surface that those rule packs should stand on:

- board coordinates, spaces, pieces, occupancy, movement, and capture
- seat-based turn order and round tracking
- authorable board, piece, phase, and turn-order assets
- authorable board move policy primitives for common grid movement, exact offset/jump movement, and optional capture
- authorable board terminal conditions for baseline win/loss outcomes
- game-mode links so validation can catch bad rules assets before scene wiring
- action resolvers for queued board movement and turn advancement
- service boundaries for future scene components, UI, AI, networking, and save/load lanes

## Next Parity Targets

1. Move Game Shell MVP toward `Ready` with a guided boot/loading/menu/settings/credits route and shell proof.
2. Move Pawn-Backed Action toward `Ready` only when `Sprite2D`, `Billboard2_5D`, and `Rigged3D` all satisfy the five-part completion bar.
3. Move Non-Pawn Tabletop toward `Ready` with a no-pawn from-scratch walkthrough, board selection, action queue, turn flow, terminal condition proof, and no pawn/spawn false positives.
4. Move Network Chain MVP toward `Ready` with a clean host/client proof path, clear NGO setup guidance, and validation that keeps local co-op and networked sessions separate.
5. Keep Unity-only authoring UX current with guided inspectors, Create Asset menu coverage, cookbook facts, optional route contracts, and setup validation for each creator-facing asset.
6. Track the RPG Systems Platform through `RPG_SYSTEMS_ROADMAP.md` and move its phases toward `Ready` only when runtime, authoring, guidance, validation, and proof exist.

## RPG Systems Platform

Current status: `Foundation Only`.

The RPG Systems Platform is the dedicated roadmap for reusable progression, inventory, equipment, skill trees, quests, NPC hooks, hubs, persistence, and open-zone state. It must stay participant-owned and actor-agnostic so it can serve side-scrolling brawlers, tabletop tactics, survival loops, hub-launched minigames, action RPGs, and open-zone prototypes.

| RPG Phase | MVP Status | Main Proof Needed |
| --- | --- | --- |
| RPG Identity, Stats, And Progression | Foundation Only | runtime tests for owner keys, stats, XP, levels, and skill points are present; authoring guidance and setup validation still needed |
| Inventory And Item Catalog | Foundation Only | runtime and editor tests for item definitions, catalogs, stack limits, and owner-separated inventories are present; setup validation and sample proof still needed |
| Equipment And Effects | Foundation Only | runtime and editor tests for equipment slots, loadouts, slot mismatches, stat modifier effects, routed loadout panel equip/unequip playback, and hub proof route loadout playback are present; ability/action effects and inventory ownership policy still needed |
| Skill Trees | Foundation Only | runtime and editor tests for node prerequisites, point spending, stat unlock effects, repeatable nodes, malformed graph validation, routed skill tree/trainer unlock playback, and hub proof route trainer playback are present; ability/action effects and refunds still needed |
| Quests And Objectives | Foundation Only | runtime and editor tests for objective progress, completion, owner separation, XP/skill-point/item rewards, repeatability rules, and malformed quest authoring are present; setup validation, quest UI, event adapters, persistence, and non-item reward routing still needed |
| NPC And Dialogue Hooks | Foundation Only | runtime and editor tests for owner-separated dialogue sessions, condition-gated choices, dialogue flags, quest/item/progression effect dispatch, malformed NPC authoring, broken node links, invalid condition/effect targets, editor graph mutation, and native editor source contract are present; persistence, localization, graph-canvas polish, and dedicated trainer/portal services still needed |
| Hub Framework | Foundation Only | runtime and editor tests for hub interaction availability, visible locked prompts, hidden locked interactions, dialogue/panel/scene request results, invalid owners, malformed hub ids, duplicate interactables, missing portal scenes, missing dialogue graphs, missing panel routes, invalid condition/effect targets, HUD prompt ordering, prompt navigation, routed HUD state, notifications, HUD presenter validation, scene-controller/HUD bridging, route-panel opening/closing, native dialogue panel playback, quest board start/status playback, vendor buy/sell playback, loadout equip/unequip playback, skill tree/trainer unlock playback, and a package proof route covering prompt-to-dialogue-to-panel-to-scene request flow are present; persistence, localization, imported visual sample scene, and multiplayer replication still needed |
| Persistence | Foundation Only | runtime tests for `RpgOwnerSaveData` round-trip owner restore, hub return metadata, and tolerant loading of unknown inventory, quest, skill, flag, and missing equipment definition data are present; concrete file/cloud backend policy and migrations beyond schema version 1 still needed |
| Open-Zone Readiness | Foundation Only | runtime tests for durable zone ids, travel snapshots, zone flags, encounter/resource/pickup/NPC state restoration, reset-on-run policy, and `RpgOwnerSaveData` integration are present; terrain streaming, scene adapters, visual zone definitions, and sample content still needed |
| Golden RPG Sample | Foundation Only | code-backed runtime sample proves hub, NPC dialogue, quest acceptance/completion, vendor purchase, item reward, equipment, skill unlock, meadow/gameplay entrance, open-zone state, and save/load; imported visual scenes and prefab placement still needed |

## Action Queue Spine

The Action Queue Spine is the shared runtime lane for delayed and rules-driven action execution. It provides:

- FIFO queued action entries with stable queue ids and sequence ids
- validation through existing `IActionResolver` contracts before an action enters the queue
- cancellation of pending actions by queue id
- resolver-based processing through existing `ActionResolutionResult`

This is not yet a full card stack, named-game move engine, or tactical AP system. Those should layer on top of the queue instead of bypassing it.

The queue now has first runtime proof through `BoardMoveActionResolver` and `TurnAdvanceActionResolver`. A board move can be queued, validated against board occupancy and optional active-seat turn state, then resolved into `BoardRuntimeState`; an end-turn action can advance `TurnRuntimeState` through the same queue surface.
