# Pyralis Feature Development Roadmap

This roadmap turns the broader feature scope into executable platform slices.

## Roadmap Rule

Each slice should produce reusable platform value, compile cleanly, update docs, and leave a small test or validation signal behind.

## Readiness Checkpoint Rule

Near-term work is governed by `CORE_PACKAGE_READINESS_CHECKPOINTS.md`. A slice should advance a named checkpoint toward Unity-authorable playable proof. If a slice only adds isolated infrastructure without moving a checkpoint gate, defer it.

## Route Completeness Rule

Pyralis becomes a game studio toolkit by completing routes, not by adding isolated mechanics.

A route is complete when it has:

- mechanic runtime
- definitions and profiles
- prefab and scene setup path
- Authoring Window guidance
- validation
- generic setup guidance, cookbook facts, or optional route contracts after the manual authoring path is proven
- sample or proof scene evidence
- first playable proof
- docs
- tests

Use this checklist as the product gate for every game lane. A brawler route, tabletop route, card route, survival route, shooter route, RPG route, or procedural route is not done just because the runtime system exists. It is done when a Unity developer can create the route, understand the setup chain, validate common mistakes, run the smallest proof, and keep going without rebuilding the framework.

During the authoring proof period, proof scenes and temporary authored values are evidence fixtures, not the product path. They should prove that the Authoring Window, inspectors, graph facts, validators, contracts, and runtime code can guide native Unity authoring. They must not become hidden route generators, starter packs, or preset paths. Multiple unfinished structural proofs are acceptable when the remaining work is explicitly Cameron/user-owned art, animation, input, tuning, game-feel, or design validation.

## Full Studio Toolkit Feature Map

The long-term map is broad, but it should still be sequenced through route completeness.

### Authoring And Learning Layer

This is the central near-term investment because it makes every other route usable.

Target capabilities:

- route-specific guided checklists
- first playable proof per route
- setup issue cards with exact fields or components to inspect
- scene surface detectors with evidence
- generic capability setup paths for each major game type
- disposable proof-scene evidence for each route
- in-window success prompts
- field explanations connected to actual Unity objects
- guided authoring surface with progressive disclosure
- guide-only Capability Vocabulary cards browsable by capability and runtime lane
- searchable mechanic glossary
- setup checklist export
- one-click scene audit
- one-click prefab audit
- common mistake detection
- guided repair actions
- visual route dependency graph

The Capability Vocabulary is a guide-only grammar slice. It should remain guide-only while the native Unity setup path is being proven: cards explain what a broad capability adds, when to use it, native setup surfaces, customization moments, deferrable work, and first proof vocabulary. Feature-specific requirements should move into contracts/reflection and graph evidence, not into new hardcoded vocabulary cards.

After the runtime capability slice is stable, expand the same contract/dependency-tree/graph model to the whole setup surface: pawns, NPCs/enemies, custom objects, UI, world, and networking. Vocabulary should supply fallback wording only; feature-specific setup truth should come from contracts, reflection, validators, and graph evidence.

Authoring now runs through the contract/dependency-tree/graph pipeline:

```text
gameplay code and authored setup
  -> contracts + reflected dependency tree + validators + grammar vocabulary
      -> resolved setup graph
          -> Authoring Window, inspectors, setup flow, validators, Facts, and docs
```

The active foundation is the resolved setup graph. `PyralisAuthoringGrammarRegistry`, `PyralisCapabilityVocabulary`, and `PyralisProofFamilyVocabulary` provide stable ids, fallback wording, native action vocabulary, and audit facts. Contracts own feature-specific setup meaning, `PyralisSetupDependencyTree` owns serialized reference discovery, and validators own runtime/scene readiness. The Authoring Window has a read-only `Facts` tab for grammar and provenance coverage, but visible setup guidance should project from graph output instead of rebuilding route, proof, or validation meaning locally.

Authoring rollout order:

1. normalize existing runtime capability cards, setup-flow rows, and common guide language into typed facts - active foundation started
2. migrate one complete route, starting with 2D pawn movement, through the registry - setup-node relationships, first proof fact, and first Sprite2D convention provider are started
3. prove the route manually through native Unity authoring before calling it authoring-ready
4. add typed validator issues with stable issue codes - started in the Validate model and visible cards, still needs broader inspector/field coverage
5. add route proof facts that connect setup nodes, scene evidence, and first Play Mode proof - broad proof targets and scene-evidence fact anchors are in place, still needs manual Computer Use proof notes
6. add inspector handoff facts for selected field/component guidance - expanded across core setup, 2D pawn/input, tabletop, camera, feature-module, and selected route fields; still needs more per-route field audits after live testing
7. add a read-only Fact Explorer tab that shows provenance, confidence, and missing coverage - started in the Authoring Window
8. add reflection/convention providers for boring facts such as `CreateAssetMenu`, `AddComponentMenu`, `RequireComponent`, serialized fields, and known suffixes - provider spine is active, 1P Sprite2D convention facts have moved out of the bridge provider, and remaining route surfaces still need automated coverage discovery plus per-route pruning after Unity proof
9. expand by proven route surface: pawns, NPCs/enemies, custom objects/features, UI/HUD/menus, world/environment, networking, and tabletop/card/procedural - route-family facts, broad proof targets, scene-evidence anchors, and first inspector/convention coverage are in place; still needs route-specific validators and manual proof coverage before any sample promotion
10. add a BuildReport / Export Footprint promotion gate for route builds once representative route exports exist - keep editor-only authoring contracts/facts/providers/validators out of player builds, and use Unity build reports to catch unexpected editor assemblies, unrelated runtime modules, or large unused assets before export-footprint-sensitive routes are promoted
11. add optional beginner semantic location tags from facts/actions rather than hand-colored prose - started with central palette, top legend, and fact/action badges

New runtime features should not be considered authoring-ready until they contribute facts or convention coverage, feature-owned contracts when represented as feature modules, route/lane support including unsupported lanes, native setup actions, validation issues when setup can fail, first-proof guidance that maps to a real proof fact, and tests proving the facts and proof targets are discoverable.

### Runtime And Production Layers

After the authoring layer can explain and validate routes, expand the runtime surface by lane:

- Core game loop: state machine, save/load, settings persistence, scene transitions, checkpoints, retry flow, progression state, unlocks, result summaries, run history, and meta progression.
- Combat and action: abilities, cooldowns, costs, charges, buff/debuff rules, damage types, resistances, armor, crits, status ticks, combos, hitstop, i-frames, targeting, area effects, interrupts, parry/block/dodge, and finishers.
- Weapons and projectiles: inventory, equip/switching, ammo/reload, charge shots, beams, hitscan, ricochet/pierce/split, homing, explosions, bullet patterns, turrets/traps, upgrades, and stress validation.
- Enemy and AI: behavior-tree or utility AI integration, state-machine AI authoring, perception, patrol/chase/attack/flee, groups, formations, boss phases, telegraphs, intent previews, spawners, wave directors, encounter budgets, scaling, threat, pathfinding, and debugging.
- UI/HUD/menus: main menu, pause, settings, rebinding, save slots, HUD binding, bars/widgets, card hand, board selection, inventory, dialogue, results/game over, co-op panels, tooltips, navigation, and accessibility.
- Camera and presentation: Cinemachine presets, 2D bounds, multi-target camera, split screen, board/tactics camera, camera shake, hit flash, damage popups, event hooks, transitions, slow motion, post-processing, and validation.
- Procedural and content generation: rooms/chunks, sockets, encounter budgets, spawn tables, seeded generation, previews, unreachable-room validation, loot/reward/wave generation, board layouts, difficulty curves, and biomes.
- Multiplayer and networking: lobby/host/client flow, network prefab validation, authority models, ownership transfer, replicated health/score/state, input streaming, prediction/reconciliation, projectile replication, disconnect/reconnect, match flow, and Steamworks lobby integration.
- Steam and shipping: Steamworks, achievements, stats, cloud saves, rich presence, controller and Steam Deck checks, build pipeline, versioning, logs/crashes, localization, credits/legal, save migration, and demo support.
- Studio production tools: project naming/folder conventions, asset/build validation, automated playtest scenes, content dashboards, balance tables, generated designer docs, mechanic test scenes, performance budgets, dependency health checks, sample projects, cookbook facts, optional route contracts, and reusable features.

### Major Game Lanes

These lanes should be completed one route at a time:

- Brawler / beat-em-up: 2.5D lane movement, arena locks, encounter gates, combo authoring, wave rooms, grab/throw, juggle/air combos, knockdown/get-up, co-op revive, boss authoring, pickups, stage progression, crowd-control tuning, and multi-player camera framing.
- Arena shooter / twin-stick: twin-stick aiming, dodge roll, room/door/wave flow, bullet patterns, pickups/shops, loot tables, procedural rooms, arenas, impact tuning, minimap, risk/reward pickups, and run progression.
- Survival swarm: auto-attack abilities, XP gems, level-up choices, upgrade pools, passive modifiers, swarm spawning, timed waves, elites/bosses, pickup magnet/radius, DPS/performance tools, evolutions, run timers/objectives, pooling, and damage-number throttling.
- Board / tactics: grid model, tiles, legal moves, pathfinding, range/area previews, turn order, undo/confirm, enemy intent, displacement, terrain, tile hazards, objectives, stats, action points, tactics camera, save/load, and scenario editor.
- Card / deckbuilder: cards, card effects, deck/hand/discard/draw piles, energy/resources, turn phases, card targeting, rewards, relics/passives, status cards, enemy intent, map progression, shops/rest/events, upgrades, deck viewer, UI presenter, and validation.
- RPG / progression: stats, levels, XP, skills, inventory, equipment, consumables, loot, vendors, quests, dialogue, party/companions, reputation/factions, persistence, zones, spawn persistence, and save migration.

### Priority Order

Do not build the map flat. Sequence it this way:

1. Authoring Window as route auditor and tutor.
2. First proof loops for existing mechanics.
3. Disposable proof-scene evidence and generic native-authoring guidance for current routes.
4. UI/HUD/menu basics.
5. Save/settings/progression basics.
6. One deep route: tabletop/tactics or pawn action.
7. Steam/shipping layer.
8. Expand into card, swarm, roguelite, RPG, and multiplayer routes.

Current authoring focus: deepen graph-centered proof routes from the package docs and Authoring Window itself. The tabletop/action route should prove one board/card/seat/action selection surface, one rules-backed accepted or rejected action, one visible or inspectable board/card/turn-state change, and clear guidance about deferring pawn actors, final art, full card UX, AI, networking, and campaign flow until that first proof works. Proof-test agents should use `Docs/Authoring/START_HERE.md`, `Docs/Authoring/AUTHORING_MODEL.md`, `Docs/Authoring/CANONICAL_SETUP.md`, and the live Authoring Window tabs as the operating packet.

## Slice 1: Action And Targeting Core

Status: foundational code added.

Goal:

- create shared action, targeting, validation, and resolution vocabulary that does not require a pawn or character controller

Why first:

- guns, projectiles, brawler moves, card plays, board moves, turn-based commands, traps, and menu abilities all need action, target, and resolve language

Initial deliverables:

- runtime target kinds
- action execution timing
- action resolution status
- target rule validation
- target descriptors
- action execution context
- validation and resolution results
- authored `ActionDefinition`
- queued action runtime state
- in-memory FIFO `ActionQueueService`
- `IActionQueueService`
- `BoardMoveActionPayload`
- `BoardMoveActionResolver`
- `TurnAdvanceActionResolver`
- runtime and editor tests

Not included yet:

- visual target selection
- turn order
- projectile spawning
- card or board rules

## Slice 2: Guns And Projectiles

Status: active foundation.

Goal:

- build a reusable projectile and hitscan delivery layer on top of action and targeting

Initial deliverables added:

- `ProjectileDefinition`
- `ProjectileDeliveryMode`
- `FireModeDefinition`
- `ProjectileFireRequest`
- `ProjectileSpawnCommand`
- `ProjectileFirePlanner`
- `ProjectileSpawnStatus`
- `ProjectileSpawnResult`
- `ProjectileImpactDefinition`
- `ProjectileImpactEffectPlayer`
- `ProjectileMagazineState`
- `ProjectileLauncherBase`
- `ProjectileLauncher3D`
- `ProjectileLauncher2D`
- `ProjectilePoolHandle`
- editor validation tests for projectile and fire-mode authoring
- runtime planner tests for burst, spread, and action-context direction targeting
- runtime launcher tests for 3D hitscan, 2D hitscan, prefab spawning, and magazine state
- editor validation tests for impact authoring
- generic projectile authoring guidance for sample hitscan, fire mode, and impact definitions

Not included yet:

- charge and recoil policies
- sample weapon prefab path
- richer pool prewarm/editor setup tools
- inventory-level ammo ownership across multiple fire modes
- projectile trail/impact material presets

## Slice 3: Runtime Pattern Setup

Status: foundational code added.

Goal:

- create composable setup profiles that describe overlapping game-loop expectations without making game type an exclusive dropdown

Initial deliverables added:

- `RuntimeControlSurface`
- `ParticipantEmbodimentRequirement`
- `RuntimeCapabilityFamily`
- `RuntimePatternDefinition`
- `GameSetupProfile`
- validation for missing identity, missing control surfaces, pawn/non-pawn mismatches, duplicate setup patterns, and cautionary pattern combinations
- runtime capability families and optional contracts for realtime character, projectile combat, turn/menu action, board/card/tabletop, camera/cursor control, and composed brawler-with-projectiles setup
- setup-profile and runtime-pattern custom inspectors
- optional `GameModeDefinition.setupProfile` linkage so game-mode validation includes setup-profile issues

Not included yet:

- scene generation wizard
- board/card rules
- turn queues
- procedural segment generation
- full scene/service analysis

## Slice 4: Animation Compatibility

Goal:

- make Pyralis animation mappings easier to use with prebuilt Animator Controllers

Expected deliverables:

- stronger validation for missing or partial mappings
- support for common bool, float, trigger, and int parameter patterns
- editor inspection of Animator Controller parameters where practical
- mapping suggestions or setup diagnostics

## Slice 5: Side-Scrolling 2D Procedural Generation

Goal:

- support authored, inspectable side-scrolling generation before opaque algorithmic generation

Expected deliverables:

- segment or chunk definitions
- socket rules
- spawn budgets
- seed support
- validation for playable paths
- hazard, pickup, enemy, and reward placement hooks

## Slice 6: Turns, Phases, And Menu Selection

Status: core rules foundation started.

Goal:

- support non-realtime game loops and menu-driven commands using the action core

Initial deliverables added:

- `TurnRuntimeState`
- `ITurnOrderService`
- `PhaseDefinition`
- `TurnOrderDefinition`
- `GameModeDefinition.turnOrderDefinition`
- validation tests for seat and phase authoring

Not included yet:

- selectable action menus
- validation and execution hooks for AI or local players

## Slice 7: Board, Card, And Tabletop Foundations

Status: board foundation started.

Goal:

- let Pyralis support games where the participant controls a board seat, hand, cursor, or faction rather than a pawn

Initial deliverables added:

- `BoardCoordinate`
- `BoardSpaceState`
- `BoardPieceState`
- `BoardRuntimeState`
- `IBoardStateService`
- `BoardMoveShape`
- `BoardMovePolicyContext`
- `IBoardMovePolicy`
- `BoardMovePolicy`
- `BoardTerminalConditionKind`
- `BoardTerminalEvaluationResult`
- `IBoardTerminalCondition`
- `BoardTerminalCondition`
- `BoardPieceDefinition`
- `BoardMovePolicyDefinition`
- `BoardMoveOffset`
- `BoardTerminalConditionDefinition`
- `BoardDefinition`
- `BoardStartingPiece`
- `GameModeDefinition.boardDefinition`
- `GameModeDefinition.boardTerminalConditions`
- queued board move resolver
- queued turn-advance resolver
- optional policy validation on queued board moves
- exact offset/jump move policies for knight-style and custom tile-jump movement
- terminal condition validation for side-eliminated and objective-occupied outcomes
- runtime tests for move and capture state
- editor tests for board, move-policy, and terminal-condition authoring validation and runtime-state creation

Not included yet:

- decks, hands, discard, and zones
- composable board-rule contracts for directional token movement without bundled route generators
- expanded terminal conditions such as no-legal-moves, score threshold, and round limit
- card cost and resource hooks
- action-stack or action-queue resolution
- sample card or board setup path

## Slice 8: Golden Samples

Goal:

- prove the platform through polished setup paths

Expected samples:

- La Cucarachacha-style 2D hazard and pickup loop
- side-scrolling action prototype
- projectile combat prototype
- turn or menu combat prototype
- simple board or card prototype

## RPG Systems Platform Program

Status: dedicated roadmap added in `RPG_SYSTEMS_ROADMAP.md`; Phases 1-7 have foundational runtime/data/editor code started, Phase 6 includes the first native RPG Narrative Editor window, and Phase 7 includes the first hub interaction spine, first HUD prompt surface, scene-side hub interaction controller, routed RPG panel shells, native dialogue panel body, native quest board panel body, inventory-backed vendor panel body, equipment-backed loadout panel body, progression-backed skill tree/trainer panel body, and package proof route covering dialogue, quest, vendor, loadout, trainer, and portal scene-request flow.

Goal:

- add reusable RPG systems that can serve side-scrolling brawlers, tabletop tactics, survival loops, hub-launched minigames, action RPGs, and open-zone prototypes

Why as a program:

- inventory, equipment, skill trees, quests, NPC hooks, hubs, persistence, and open-zone state all depend on a shared participant-owned RPG state spine
- these systems should enrich existing Pyralis routes instead of replacing the action, tabletop, scoring, setup, scene-flow, and networking architecture

Build order:

1. RPG Identity, Stats, And Progression
2. Inventory And Item Catalog
3. Equipment And Effects
4. Skill Trees - foundational code added
5. Quests And Objectives - foundational code added
6. NPC And Dialogue Hooks - foundational code and native editor window added
7. Hub Framework - foundational code added
8. Persistence - foundational code added
9. Open-Zone Readiness - foundational code added
10. Golden RPG Sample

The implementation history has been folded into current package docs and runtime tests. Treat this roadmap, `FEATURE_DEVELOPMENT_SCOPE.md`, `RPG_SYSTEMS_ROADMAP.md`, and the active source contracts as the durable RPG direction instead of dated external plan files.

## Current Priority

Use `CORE_PACKAGE_READINESS_CHECKPOINTS.md` as the near-term product gate.

1. Make the Authoring Window the route auditor and tutor: guided checklists, first proofs, exact issue cards, scene surface evidence, common mistake detection, and safe repair actions.
2. Finish Rules-Driven Tabletop MVP: expanded named move policies, richer capture/occupancy policies, additional terminal conditions, generic setup guidance, first proof, and beginner setup docs.
3. Prove Local Two-Player Side-Scrolling Shooter MVP: local participant setup, 2D projectile launcher setup path, scene readiness validation, proof scene path, and first proof.
4. Keep Unity-Only Authoring UX current: guided inspectors, Create Asset menu coverage, cookbook facts, optional route contracts, and setup validation for every new creator-facing asset.
5. Preserve Runtime Parity Hardening: update matrix, inventory, tests, and docs whenever a lane changes status.
6. Start RPG Systems Platform only as coherent tested slices that preserve participant-owned, actor-agnostic architecture.

Do not start deeper scene building until the relevant checkpoint has a runtime path, authoring path, validation path, and proof path.
