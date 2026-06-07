# Pyralis Feature Development Scope

This document defines the intended expansion scope for Pyralis before deeper feature development continues.

Pyralis should grow as a modular gameplay composition platform, not only as a character-controller framework. Character-driven action games remain a major supported path, but the platform should also support games where the participant controls a camera, cursor, hand, board seat, faction, menu selection, or other non-pawn runtime surface.

## Scope Intention

Pyralis should make it easy to start many kinds of games from authored parts:

- 2D arcade score loops
- side-scrolling action games
- brawlers and arena combat games
- projectile and gun-driven games
- tactics and menu-driven combat games
- turn-based board games
- tabletop and card games
- hybrid games that mix realtime action, rules systems, and authored encounters

The goal is not to make every genre a separate framework. The goal is to identify reusable capability families and compose them through definitions, profiles, feature modules, editor tooling, and samples.

## Core Product Promise

Pyralis should answer this question for a developer:

> Can I create a working game loop by selecting a game setup pattern, assigning authored assets, and composing reusable gameplay modules instead of rebuilding the framework?

Every new feature should make the canonical authored path easier, faster, or more capable.

## Runtime Pattern Setup Model

Runtime setup patterns are composable recipes, not exclusive genres.

`RuntimePatternDefinition` describes one reusable setup expectation, such as realtime character control, projectile combat, turn/menu actions, board/card/tabletop play, camera/cursor control, scoring, animation mapping, or procedural segments.

`GameSetupProfile` describes an actual game loop by selecting multiple runtime patterns. A game can therefore be:

- realtime character + projectile combat + side-scrolling playfield
- board/card/tabletop + turn/menu action + camera/cursor control
- camera/cursor control + projectile combat + scoring
- realtime character + brawler combat + guns/projectiles + animation mapping

This model keeps overlap explicit and inspectable. It also protects non-pawn games: a participant can be embodied as a pawn, camera, cursor, board seat, card hand, faction, menu selection, or other authored control surface.

## Capability Families

Future feature development should be grouped into capability families.

### Platform Core

Core systems should be actor-agnostic where practical.

Important concepts:

- participants
- seats
- teams or factions
- session state
- game phases
- turn order
- action selection
- targeting
- action resolution
- rules validation
- save/state contracts
- networking and authority contracts

`Participant` should remain central. `PawnRoot` should be treated as one possible participant embodiment, not the only supported embodiment.

### Character And Pawn Gameplay

This is the current strongest surface.

Supported and future work includes:

- 2D pawns
- 2.5D or billboard pawns
- rigged 3D pawns
- movement and traversal
- brawler combat
- platforming
- pickups
- hazards
- encounters
- respawn
- camera follow and presentation

Pawn-focused work should keep using `PawnRoot`, pawn profiles, and pawn-module interfaces.

### Action And Targeting

Action and targeting should become the bridge between realtime, turn-based, tactical, card, and menu-driven games.

Potential concepts:

- `ActionDefinition`
- `ActionCost`
- `ActionTargetRule`
- `ActionResolutionContext`
- `ActionResult`
- area targeting
- line targeting
- entity targeting
- board-space targeting
- card-zone targeting
- queued or immediate execution

Combat, cards, board moves, menu commands, and scripted interactables should be able to share this layer where it is useful.

### Combat

Combat should support multiple avenues of approach instead of assuming one controller style.

Supported and target approaches:

- brawler: realtime hitboxes, combo windows, movement locks, block/parry
- fighter: strict move timing, cancel windows, frame-like reactions, guard states
- shooter: guns, projectiles, hitscan, ammo, reload, spread, recoil
- tactics: select actor, select ability, choose target or tile, resolve action
- menu RPG: choose command, choose target, resolve effects
- card combat: play card, pay cost, choose target, resolve effect stack

The shared combat layer should own reusable outcomes:

- damage
- healing
- status effects
- teams and factions
- targeting filters
- hit reactions
- feedback events
- health and defeat

Input style and delivery style should be adapters on top of those outcomes.

### Guns And Projectiles

Projectile and weapon work should be reusable beyond player controllers.

Target support:

- projectile prefabs
- hitscan fire
- ammo and reload
- burst, spread, charge, cooldown, and fire-rate rules
- 2D and 3D projectile adapters
- pooling
- faction filtering
- impact effects
- area effects
- projectile lifetime and despawn policy
- usable by pawns, enemies, traps, turrets, cards, board pieces, scripted events, and menu actions

Projectile and gun systems should not assume a humanoid pawn. They should depend on action context, ownership, faction, source transform, and target rules.

### Procedural Generation

Procedural generation should start with authored, controllable generation rather than raw algorithmic terrain.

Recommended first scope:

- side-scrolling 2D segment generation
- authored chunks or rooms
- sockets/connectors
- spawn budgets
- hazard, pickup, enemy, and reward rules
- biome or theme profiles
- seeded generation
- validation passes for playable paths

Future expansion can include:

- encounter decks
- board layouts
- tile-based tactical maps
- card pools and draft packs
- room graphs
- roguelike progression

Procedural systems should produce inspectable results and expose enough validation that designers can trust the generated content.

### Board, Card, And Tabletop Systems

Pyralis should eventually support games with no character controller.

Target concepts:

- board spaces
- pieces
- decks
- hands
- discard and exile zones
- card zones
- resources and costs
- turn phases
- action stacks or queues
- legal move validation
- selection and targeting
- camera or cursor as the participant control surface
- AI decision hooks

This should be a sibling capability family to pawn gameplay, not a forced extension of `PawnRoot`.

### Animation And Presentation

Animation should remain data-driven and compatible with common Unity Animator workflows.

Target expectations:

- `PawnAnimationProfile` maps Pyralis signals to existing Animator parameters, triggers, states, or blend-tree values.
- A pawn can use a prebuilt Animator Controller with partial Pyralis mappings.
- Missing mappings should fail gracefully and produce useful validation messages.
- Gameplay code should emit animation signals instead of owning Animator-specific logic.
- Editor tooling should eventually inspect an Animator Controller and suggest mappings.

The animation layer should support common controller patterns:

- bool locomotion
- float blend trees
- trigger-based attacks
- integer state IDs
- layered upper-body actions
- generic rigs
- humanoid rigs
- sprite Animator controllers

## Feature Development Rules

Before starting a major feature, write down:

- the capability family it belongs to
- whether it is platform core, feature runtime, presentation, integration, sample, or game-specific content
- which existing systems it should reuse
- whether a Unity package or commercial-ready open source library should be used first
- which authoring assets or profiles a developer will touch
- what a minimal sample scene or prefab should prove
- what tests or validation rules protect the behavior

Prefer this order:

1. Extend an existing capability when the fit is natural.
2. Add a small contract or profile when it unlocks reuse.
3. Add a feature module when the capability is reusable across game types.
4. Add an integration when the problem is solved well by an external package.
5. Add a game-specific sample only when the behavior is not reusable yet.

Avoid:

- one-off genre forks
- controller-first assumptions in systems that could be actor-agnostic
- hardwired single-player lookups
- direct dependencies on one input style
- Animator-controller-specific gameplay logic
- procedural generation that cannot be inspected, seeded, or validated

## Recommended Development Order

The highest-leverage expansion order is:

1. Action and targeting core.
2. Guns and projectiles as a reusable delivery layer.
3. Animation mapping and Animator compatibility improvements.
4. Canonical side-scrolling 2D procedural segment generation.
5. Turn, phase, and menu-selection runtime.
6. Board/card/tabletop foundations.
7. Polished sample packs proving each path.

This order lets realtime action, shooter, tactical, card, and board-game work share vocabulary instead of growing separate frameworks.

See `FEATURE_DEVELOPMENT_ROADMAP.md` for the executable slice order.

## Active Implementation Slices

The runtime pattern setup slice creates the authored setup spine.

Initial available concepts:

- `RuntimeControlSurface`
- `ParticipantEmbodimentRequirement`
- `RuntimeCapabilityFamily`
- `RuntimePatternDefinition`
- `GameSetupProfile`

This slice lets authored setup assets describe composable game-loop expectations without forcing one exclusive game type. It intentionally stops short of scene generation, board/card rules, procedural generation, turn queues, and full setup wizards.

The first implementation slice is Action + Targeting core.

Initial available concepts:

- `ActionTargetKind`
- `ActionExecutionTiming`
- `ActionResolutionStatus`
- `ActionTargetRule`
- `ActionTargetDescriptor`
- `ActionExecutionContext`
- `ActionValidationResult`
- `ActionResolutionResult`
- `IActionResolver`
- `ActionDefinition`

This slice intentionally avoids visual targeting, turn queues, projectile spawning, board rules, and card rules. Those should build on top of the shared action vocabulary once it is stable.

The second implementation slice is Guns + Projectiles foundation.

Initial available concepts:

- `ProjectileDeliveryMode`
- `ProjectileDefinition`
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

This slice defines authoring data, deterministic spawn-command planning, reusable 2D/3D command execution, optional launcher-owned prefab pooling, plain runtime magazine state, impact effect routing, and sample projectile/fire-mode/impact authoring assets. It intentionally stops short of charge/recoil policy, inventory-level ammo ownership, trail/material presets, and sample weapon prefabs so those can be added cleanly on top of the shared command layer.

## Golden Path Definition

A feature is "golden" when:

- it is usable through authored assets and components
- it has clear setup docs
- it has validation messages for common setup mistakes
- it has tests for core behavior or architecture boundaries
- it can be reused in at least two game styles or is clearly labeled as sample-specific
- it avoids single-player and character-controller assumptions unless that is its explicit purpose
- it has a small working sample or prefab path

The long-term Pyralis offer is not "we have many scripts." The offer is "we have reusable gameplay parts that make new game setup clear, fast, and reliable."
