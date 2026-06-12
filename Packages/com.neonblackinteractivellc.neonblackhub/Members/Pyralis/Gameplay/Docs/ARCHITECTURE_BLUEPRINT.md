# NeonBlack Gameplay Architecture Blueprint

This document describes the target architecture for NeonBlack Gameplay as it evolves from mode-separated scripts into a shared, modular, N-player-ready gameplay package.

This is both a planning document and a reference for the currently implemented shared-core direction.

## Goals

- Support a wide variety of game types from one shared gameplay toolkit.
- Make one-player, two-player, and future larger multiplayer modes variations of the same architecture.
- Prefer Inspector-authorable systems over hardcoded behavior forks.
- Keep "arcade", "brawler", and future modes as compositions of shared features and data profiles.
- Minimize custom framework code when stable Unity or commercial-ready open source solutions already exist.
- Support non-character games where a participant controls a camera, cursor, hand, board seat, faction, or menu selection instead of a pawn.

## Current Implemented Foundation

The current refactor pass now includes these concrete shared-core building blocks:

- `GameplaySessionBootstrap`
- `SessionStateService`
- `ParticipantRosterService`
- `ParticipantSpawnService`
- `ParticipantInputRouter`
- `PawnRoot`
- `CinemachineCameraRigController`
- `SessionDefinition`
- `ParticipantDefinition`
- `PawnDefinition`
- `GameModeDefinition`
- `PlayfieldProfile`
- `CameraRigProfile`
- `InputProfile`
- `SettingsProfile`
- feature module definitions and pawn-module interfaces
- `ResolvedAuthoringContractRegistry` discovery for `IAuthoringContractProvider` contract providers

This means the package has a real Inspector-driven shared-core startup path. New gameplay and authoring work should extend this path directly instead of preserving abandoned setup routes.

## Core Architectural Principles

### 1. N-Participant Core

NeonBlack Gameplay should be built for N participants.

`1P` and `2P` are not special architectures. They are common configurations of the same participant model.

### 2. Shared Capability, Data-Driven Identity

Shared systems should provide reusable capability.

Game identity should mostly come from:

- data assets,
- enabled modules,
- profile assignments,
- authored prefabs,
- mode configuration.

### 3. Inspector Is The Daily Surface

Most day-to-day design iteration should happen in:

- prefabs,
- `ScriptableObject` assets,
- custom inspectors,
- scene setup,
- package examples.

The Inspector path should be treated as product surface, not secondary tooling. If the preferred setup requires knowing hidden code rules, the authoring model is not done yet.

The maintainable path is:

- one obvious scene entrypoint,
- one obvious top-level session asset,
- visible links from mode intent to setup profile to runtime patterns,
- validation messages near the fields that caused them,
- feature-owned authoring contracts that feed setup guidance, validation, facts, and proof targets.

### 3.5. Feature Contracts Own Feature Setup Truth

Reusable feature modules declare their authoring requirements beside the owning feature through `IAuthoringContractProvider`. `ResolvedAuthoringContractRegistry` discovers those providers reflectively. Central authoring code aggregates and displays contracts; it should not maintain parallel switch statements or manual module-id lists for feature-specific profile, lane, action, runtime-interface, or first-proof rules.

A complete feature contract names:

- `FeatureModuleDefinition.moduleId`
- required profile type
- required runtime prefab interfaces
- supported and unsupported presentation lanes
- consumed action roles
- native setup actions
- assignment fields
- customization moments
- first route proof target

This keeps feature ownership local while allowing the Authoring Window, inspectors, validators, facts, and proof workflow to agree on the same data.

### 4. Pawn Composition Uses Direct Module Ownership

The supported pawn stacks now implement pawn interfaces directly on focused components.

For the 3D brawler, `Motor3D` is the composition root. It coordinates focused sibling components: `Pawn3DInputModule`, `Pawn3DMovementComponent`, Traversal-owned `Pawn3DTraversalComponent`, and `Pawn3DPresentationComponent`. Each implements the corresponding pawn interface directly.

For the 2D stack, `Motor2D` is the shared 2D pawn motor surface. Focused ownership lives in `Motor2DInputAdapter`, `PlayerInputHandler`, `PawnCombatBehaviour2D`, `Pawn2DMovementComponent`, and `Pawn2DPresentationComponent`.

### 5. Runtime Is Transport-Agnostic

Gameplay architecture should not assume local-only or network-only ownership.

The runtime model should keep participant identity, pawn identity, and input ownership separate so networking can be added cleanly later.

### 6. Networking Is An Optional Extension Layer

`NeonBlack.Gameplay` does not reference the optional `NeonBlack.Gameplay.Networking` assembly. NGO-dependent session, authority, spawn, validation, and NetworkManager behaviour lives in the separate Networking assembly. One current shared character seam, `MovementStateSnapshot`, still uses NGO serialization types so movement replication can share DTOs with networked adapters; keep that exception narrow or move it behind a dedicated networking contract before expanding prediction/reconciliation work.

The three participant services expose protected virtual override points:

- `SessionStateService.TryStartHostIfNeeded()` - overridden by `NetworkedSessionStateService` to call `NetworkManager.StartHost()`
- `ParticipantRosterService.ResolveOwnerClientId()` - overridden by `NetworkedParticipantRosterService` to return `NetworkManager.LocalClientId`
- `ParticipantSpawnService.SpawnParticipantPawn()` / `DestroyPawnInstance()` - overridden by `NetworkedParticipantSpawnService` to call `NetworkObject.Spawn()` / `NetworkObject.Despawn()`

For local games register the base classes at bootstrap. For online games register the `Networked*` variants instead.

## Target Runtime Vocabulary

### Participant

A gameplay seat or actor in the session.

A participant may have:

- an input owner,
- a team or faction,
- a pawn,
- a camera or cursor,
- a hand, deck, board seat, or selected entity,
- score or lives,
- UI ownership.

### Pawn

The runtime entity being controlled in the world.

A pawn is configured by data and composed from shared modules.

Important: a pawn is one possible participant embodiment, not the only one. Board games, card games, menu-driven tactics, and camera-as-player games should be able to use the participant/session model without creating fake character controllers.

### Actor

A runtime entity that can receive features, actions, targeting, feedback, or ownership.

Examples:

- character pawns
- enemies
- turrets
- board pieces
- cards
- interactable scene objects
- traps
- scripted encounter objects

Use `Actor` vocabulary when a system does not require movement-controller behavior.

### Action

A player, AI, rule, or system-driven intent that can be validated and resolved.

Examples:

- punch
- fire weapon
- cast ability
- play card
- move board piece
- select menu command
- trigger trap

Actions should be able to resolve through realtime delivery, turn-based menus, board/card rules, or scripted systems.

### Session

The rules around how participants join and how the game loop runs.

Examples:

- solo local,
- couch co-op,
- versus local,
- future online co-op,
- future networked versus.

### Game Mode

The scoring, win/loss, progression, respawn, and phase logic of the experience.

Examples:

- survival pickup mode,
- arena brawler mode,
- stage clear mode,
- score attack mode.

### Playfield

The movement and spatial rules of the playable space.

Examples:

- free 2D screen bounds,
- 2.5D lane depth,
- top-down arena,
- screen wrap,
- arena lock,
- stage progression rails.

### Feature Module

A reusable behavior package that can be enabled or disabled by composition.

Examples:

- pickups,
- hazards,
- combo combat,
- climb,
- dodge,
- inventory,
- respawn,
- shared camera rules.

### Control Surface

The thing a participant directly manipulates.

Examples:

- character controller
- camera
- cursor
- selected board piece
- card hand
- menu selection
- faction command layer

Control surfaces should route through participant ownership and input/action contracts rather than forcing all games through pawn movement.

### Runtime Pattern

A reusable optional route contract that describes a capability family and its participant/control-surface expectations.

Examples:

- realtime character
- projectile combat
- turn/menu action
- board/card/tabletop
- camera/cursor control
- scoring
- procedural segments
- animation mapping

Runtime patterns are composable. A game mode should be able to combine realtime character, projectile combat, side-scrolling playfield, scoring, and animation mapping without pretending those are separate frameworks.

### Game Setup Profile

An authored profile that selects multiple runtime patterns to describe one game loop.

Examples:

- brawler with projectiles
- side-scrolling shooter
- card battler
- tactics prototype
- camera/cursor tabletop game

This profile is the bridge between product intent and future setup tooling. It is not a runtime manager and should not execute game logic directly.

## Target Data Model

The exact class names can change, but this is the preferred shape.

### SessionDefinition

Defines:

- local or network-ready session mode,
- participant limit,
- join policy,
- shared or split presentation rules,
- authority assumptions.

### ParticipantDefinition

Defines:

- participant role,
- team or faction defaults,
- HUD ownership,
- spawn policy,
- default pawn assignment rules.

### PawnDefinition

Defines:

- pawn prefab,
- movement profile,
- combat profile,
- traversal profile,
- presentation profile,
- default feature modules.

### GameModeDefinition

Defines:

- score and objective rules,
- respawn rules,
- phase flow,
- hazard or pickup enablement,
- victory and failure conditions,
- required services.

Future mode definitions may also reference turn, action, board, card, or procedural generation profiles when the game is not pawn-controller-first.

### RuntimePatternDefinition

Defines:

- stable pattern id,
- display name and setup notes,
- capability family,
- supported control surfaces,
- participant embodiment requirement,
- required and optional runtime systems,
- recommended companion patterns,
- cautionary companion patterns.

Patterns should be used as reusable setup vocabulary, not as exclusive game-type labels.

### GameSetupProfile

Defines:

- setup name and summary,
- selected runtime patterns,
- setup notes,
- validation for missing pattern metadata, duplicate pattern ids, pawn/non-pawn mismatch, and cautionary combinations.

Game setup profiles should become the object future wizards, sample generators, and setup validators read before creating or inspecting scene content.

### PlayfieldProfile

Defines:

- movement space model,
- depth or lane rules,
- screen bounds or wrap,
- arena lock rules,
- spawn regions,
- camera boundary relationship.

### CameraRigProfile

Defines:

- follow style,
- composition,
- zoom behavior,
- shake tuning,
- multi-target behavior,
- split or shared camera preferences.

### InputProfile

Defines:

- action asset reference,
- control scheme expectations,
- rebinding rules,
- touch or gamepad presentation hints.

### FeatureModuleDefinition

Defines:

- module enablement,
- module-specific tunables,
- data references for a reusable capability.

### Future ActionDefinition

Defines:

- action id and display name,
- cost rules,
- targeting rules,
- execution timing,
- delivery style,
- resolution effects,
- animation and feedback signals.

This is the likely shared bridge for guns, projectiles, brawler moves, turn-based commands, tactical abilities, cards, board moves, and scripted interactions.

## Controller Direction

The 3D brawler pawn is fully decomposed. `Motor3D` is the composition root - it coordinates four focused sibling components with zero gameplay logic of its own:

- `Pawn3DInputModule` - all Input System binding; produces a `FrameInput` snapshot each frame
- `Pawn3DMovementComponent` - owns `BrawlerMovementModel`, drives `CharacterController`, implements `IPawnMotor` and `IMovementModule`
- `Pawn3DTraversalComponent` - ledge detection, climb, hang, shimmy; implements `IPawnTraversalModule`
- `Pawn3DPresentationComponent` - Animator, billboard, land squash, debug HUD; implements `IPawnPresentationModule`

Mode-specific differences are applied through profiles and optional modules.

Current implementation note:

- the direct 2D and 3D pawn stacks are the supported authoring path.
- movement, input, presentation, traversal, combat, interaction, feedback, pickups, and status should expose feature-owned contracts when they are reusable modules.

Examples:

- a phone brawler may use touch input presentation plus lane-depth movement plus combo combat plus pickups,
- a survival arcade mode may use free-bounds movement plus dash plus pickup scoring plus hazards,
- a technical arena mode may use free movement plus richer combat plus score attack rules.

## Playfield Versus Camera

Do not treat movement bounds as a camera-only concern.

Preferred split:

- `PlayfieldProfile` owns playable-space rules,
- `CameraRigProfile` owns framing and follow,
- the camera may read playfield data when useful,
- movement modules may read the same playfield data directly.

This avoids forcing "aspect-bound movement" to mean "camera profile."

## Shared Features Versus Compatibility-Specific Layers

### Likely Shared

- participant roster,
- spawning and respawning,
- turn, phase, and action-selection primitives,
- targeting and action-resolution primitives,
- health and damage primitives,
- knockback,
- hitboxes and projectiles,
- guns, ammo, reload, spread, hitscan, and reusable projectile delivery,
- inventory and equipment foundations,
- card, deck, hand, board-space, and piece primitives where reusable,
- procedural generation contracts for authored chunks, sockets, budgets, seeds, and validation,
- pickup and score primitives,
- hazard foundations,
- camera service abstractions,
- animation signal mapping and Animator compatibility tooling,
- settings and save-backed configuration,
- input ownership and routing.

### Likely Mode-Specific Or Preset-Oriented

- exact combo grammar,
- exact fighting-game move list,
- exact turn order rules for one card game,
- exact board rules for one tabletop game,
- exact touch layout presentation,
- exact scoring formula,
- exact arena progression rules,
- exact hazard themes and authored content.

If a feature appears in more than one mode, treat that as a signal it should move toward shared.

## Preferred Unity And Ecosystem Foundations

Use engine-supported systems first where they fit well.

Preferred starting points:

- Unity Input System for participant input ownership and local multiplayer,
- `PlayerInput` and `PlayerInputManager` for local join and pairing flows,
- `ScriptableObject` assets for gameplay definitions and profiles,
- editor tooling for authoring surfaces,
- Cinemachine for higher-level camera authoring if camera complexity keeps growing.

Current implementation note:

- the package manifest declares Cinemachine, Netcode for GameObjects, and Unity Transport as package dependencies; NGO gameplay behaviour remains an opt-in route isolated behind `NeonBlack.Gameplay.Networking.asmdef`, with only narrow shared serialization DTOs allowed outside the Networking folder.

Avoid building custom replacements for these unless there is a clear documented limitation.

## Unity Authoring Maintainability Rules

Unity authoring stays maintainable when scene objects are thin runtime readers and authored assets own most design intent.

Prefer:

- `ScriptableObject` definitions for identity, relationships, and reusable setup choices
- `ScriptableObject` profiles for tuning, numbers, curves, effects, and presentation choices
- prefabs for reusable runtime object composition
- one bootstrap root per playable scene
- custom inspectors that explain what to assign next
- validation that catches missing references before Play Mode

Avoid:

- hidden global state as the main authoring contract
- tag searches as the preferred player/participant lookup
- scene-only wiring that cannot be recreated from definitions and profiles
- expanding singleton managers when the session or participant model should own the behavior
- adding fields to large MonoBehaviours when a profile or feature module would make the choice reusable

The practical test is simple: a designer or future developer should be able to inspect a scene root, follow the assigned assets, and understand why the runtime behaves the way it does. If they must inspect several static singletons or search for objects by tag to understand the scene, the authoring path is carrying maintenance debt.

## Current Risk Areas

The architecture is coherent enough for active route development, but several areas still need disciplined cleanup as new content arrives.

Highest-risk areas:

- runtime services still include narrow static compatibility/query surfaces, especially participant lookup helpers, that should keep shrinking toward explicit lifetime-scope ownership and participant/session references
- some older scene-facing flows still need participant-native proof in Play Mode
- `CameraOcclusionFader` and a few polling/ticking systems need hot-path allocation cleanup before content density grows
- several large MonoBehaviours and editor classes remain change hotspots
- the aggregate `NeonBlack.Gameplay` assembly can still hide accidental cross-domain coupling
- deeper lane validation still needs to grow beyond the first route/service layer

These are not reasons to restart the architecture. They are the next cleanup checkpoints that make the existing direction cheaper to maintain.

## Current Refactor Targets

### Target 1: Participant Model

Replace single-active-player assumptions with a participant roster model.

This includes removing design dependence on:

- one active player registry,
- one input receiver,
- one player prefab per mode,
- tag-based player discovery as the primary path.

### Target 2: Input Ownership

Move from single-player input handling toward per-participant input ownership.

This should support:

- one local participant,
- multiple local participants,
- future network-backed participant ownership.

### Target 3: Controller Decomposition

Break large pawn scripts into modules with clear responsibilities.

**Resolved for 3D and 2D.** `PlayerActions` is gone, `Motor3D` coordinates focused components, and `Motor2D` now delegates to dedicated 2D movement and presentation components instead of owning those concerns directly.

### Target 4: Mode As Data

Move mode identity out of folders and into authored definitions.

Arcade and brawler should remain example assemblies of shared parts, with reusable learning captured as capability facts, validation rules, optional route contracts, and generic setup guidance rather than presets.

### Target 5: Documentation As Source Of Truth

Keep architecture, standards, and migration docs current as code changes land.

Docs should describe the supported path directly. Keep compatibility notes only when they protect active content, a supported public contract, or a still-open cleanup task.

### Target 5.5: Single Runtime Composition Path

The long-term runtime service ownership model should have one primary composition root.

`GameplaySessionBootstrap` remains the supported scene entrypoint, but it should feed a clear service graph rather than becoming a second service container. `PyralisGameplayLifetimeScope` should be the durable owner for dependency registration. Static singleton accessors and compatibility query helpers such as participant lookup should shrink toward narrow facades rather than becoming a second service-location model.

This matters because Unity scenes already have enough implicit state. The gameplay platform should not add several hidden service-resolution models on top of that.

### Target 6: Actor-Agnostic Expansion

Expand the platform so participant-owned play does not require a character controller.

This includes future support for:

- camera-as-player games,
- cursor/selector-driven games,
- tactics and menu selection,
- board pieces,
- cards, hands, decks, and zones,
- turn and phase systems,
- action/targeting resolution shared by realtime and rules-driven games.

### Target 7: Capability Families Before Genre Forks

New feature work should start from reusable capability families rather than genre folders.

Examples:

- action and targeting before "RPG combat",
- projectile delivery before "shooter mode",
- board spaces and legal moves before one named board game,
- card zones and action resolution before one specific card game,
- authored segment generation before one procedural side-scroller.

## Anti-Goals

Do not:

- create a second monolithic framework under a new name,
- fork every shared system into mode-specific copies,
- keep adding singletons that imply one active player forever,
- force every game type through a pawn or character-controller model,
- hide board/card/turn/action rules inside one-off UI scripts,
- let docs drift behind the current design,
- replace mature engine packages with custom code for style reasons alone.

## Migration Mindset

This architecture should be reached incrementally.

The preferred path is:

- stabilize language and standards,
- define target shapes,
- refactor seams with high leverage,
- keep active setup guidance focused on the supported path,
- retire old paths once they no longer protect active content or a public contract.
