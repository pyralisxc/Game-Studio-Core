# Pyralis Core Rules And Runtime Parity Design

Date: 2026-05-24

## Purpose

Pyralis should be hardened before scene-building begins. The current package has a strong authored setup spine, participant model, pawn composition path, projectile layer, guided inspectors, and validation tooling. The gap is runtime parity: some advertised lanes, especially board/card/tabletop and turn/menu action, are still setup contracts rather than playable reusable systems.

This design defines the next platform phase: a core rules spine plus an active runtime parity matrix. The goal is to make Pyralis feel like a modular gameplay composition platform, not only a character-controller toolkit.

## Product Promise

Pyralis should let a creator start a normal game loop from Unity by composing authored assets and scene services.

For regular tabletop games, "tabletop support" should mean more than naming a runtime pattern. Pyralis should include reusable baseline rule systems for common board/tabletop mechanics. Custom rules should still be easy to add through code, ScriptableObject rule assets, or future Unity authoring tools, but custom code should not be required just to prove ordinary board-game flow.

## AAA-Style Quality Bar

This phase should follow professional Unity package standards:

- clear runtime contracts before concrete managers
- small focused files with one reason to change
- ScriptableObject definitions and profiles for designer-facing choices
- scene services owned through the established bootstrap/lifetime-scope path
- no hidden singleton reads in new gameplay systems
- no pawn requirement for non-pawn control surfaces
- validation before Play Mode when setup can be checked statically
- tests for pure rules, service behavior, editor validation, and package contracts
- docs that describe the supported path, not aspirational behavior
- sample/starter assets only after the reusable runtime path exists

The implementation should prefer boring, explicit architecture over clever generic engines. The creator experience should stay understandable in Unity Inspector.

## Scope

This phase combines two directions:

1. Core Rules Spine
2. Runtime Parity Hardening

The first implementation checkpoint should build enough platform rules infrastructure to make tabletop and turn/menu routes real. The parity matrix keeps that work honest across 2D shooter, 3D/FPS, brawler, projectile, turn/menu, tabletop, card, scoring, camera/cursor, and local multiplayer lanes.

## Non-Goals

This phase should not build a complete AAA chess AI, online matchmaking, rollback netcode, a visual node editor, or a full scene generator.

This phase should not make tabletop systems depend on `PawnRoot`, `Motor2D`, `Motor3D`, or any one camera/input model.

This phase should not import a large external board-game framework as the main product surface. External libraries can be revisited later for optional adapters or AI/search helpers, but Pyralis's core value is Unity-native authoring and modular composition.

## Current Foundation To Reuse

The design should build on these existing systems:

- `SessionDefinition`, `ParticipantDefinition`, `ParticipantHandle`
- `GameModeDefinition`, `GameSetupProfile`, `RuntimePatternDefinition`
- `RuntimeControlSurface`, `ParticipantEmbodimentRequirement`, `RuntimeCapabilityFamily`
- `GameplaySessionBootstrap`, `PyralisGameplayLifetimeScope`
- `ParticipantRosterService`, `SessionStateService`, `ParticipantInputRouter`
- `ActionDefinition`, `ActionTargetRule`, `ActionTargetDescriptor`, `ActionExecutionContext`
- `IActionResolver`, `ActionValidationResult`, `ActionResolutionResult`
- `ParticipantScoreService`
- `ProjectileDefinition`, `ProjectileFirePlanner`, `ProjectileLauncher2D`, `ProjectileLauncher3D`
- guided inspector helpers and setup-flow validators

The new rules layer should extend the action and targeting vocabulary instead of creating a parallel command system.

## Core Rules Architecture

The core rules spine should introduce reusable concepts for games that are turn-based, phase-based, board-based, card-based, or menu/action-driven.

### Turn And Phase Runtime

Add an actor-agnostic turn/phase layer:

- `TurnOrderDefinition`
- `PhaseDefinition`
- `TurnRuntimeState`
- `PhaseRuntimeState`
- `TurnOrderService`
- `ITurnOrderService`
- `IPhaseRule`

The first concrete runtime should support:

- ordered participant turns
- active participant lookup
- advancing to next participant
- round count
- optional phase list per turn
- phase enter/exit hooks
- skip/eliminate inactive participant hooks

This should serve board games, tactics games, menu RPG combat, card battlers, and any future turn-based hybrid.

### Board Runtime

Add a reusable board layer:

- `BoardDefinition`
- `BoardSpaceDefinition`
- `BoardPieceDefinition`
- `BoardRuntimeState`
- `BoardSpaceState`
- `BoardPieceState`
- `BoardRuntimeService`
- `IBoardStateService`

The first concrete board model should support:

- rectangular grids
- named spaces
- occupancy by piece id
- piece owner/side/faction
- piece tags or families
- active/inactive captured state
- board-space target descriptors
- querying legal spaces by rule

The implementation should be data-first and testable without scene objects. Scene objects can represent spaces and pieces later, but the rules should not require scene transforms.

### Action Queue And Resolution

Add an action-resolution service that uses existing `ActionDefinition` and `ActionExecutionContext`:

- `QueuedAction`
- `ActionQueueService`
- `IActionQueueService`
- `ActionResolverRegistry`
- `IRuleActionResolver`

The first concrete runtime should support:

- immediate action validation
- queued action submission
- resolver lookup by action id or action family
- deterministic resolve order
- failure results that explain why an action was rejected
- optional participant/turn gating

This is the bridge between tabletop moves, card plays, menu commands, projectile actions, and future tactical abilities.

### Rule Definitions

Rules should be authored through focused ScriptableObject assets:

- `BoardMoveRuleDefinition`
- `CaptureRuleDefinition`
- `OccupancyRuleDefinition`
- `WinConditionDefinition`
- `ResourceCostRuleDefinition`
- `TurnGateRuleDefinition`

The first set should cover common tabletop behavior:

- move a piece from one board space to another
- require source ownership by active participant
- require empty destination or capturable destination
- capture an opposing piece
- alternate turns after a resolved move
- win when a side has no pieces, no legal moves, or a named objective state

Rules should be composable and small. For example, chess-like movement should be built from movement-pattern rules plus state rules, not one giant `ChessManager`.

## Baseline Tabletop Support

Pyralis should support regular tabletop setup without requiring custom project code for the first playable loop.

### Baseline Board Games

The first baseline should support:

- checkers-like movement
- chess-like movement primitives
- simple grid tactics movement
- piece capture
- turn alternation
- win/loss conditions

Chess-like movement primitives should include:

- orthogonal rays
- diagonal rays
- single-step king-style movement
- knight offsets
- forward pawn/checker-style movement
- capture-only movement patterns
- blocked-path handling

The first implementation does not need full official chess with castling, en passant, promotion UI, check/checkmate, stalemate, clocks, notation, or AI. It should provide enough legal-move primitives that chess variants can be assembled and official chess can be added deliberately.

### Baseline Card And Hand Support

The first tabletop pass can define the card/hand contracts, but full card gameplay may be a follow-up unless implementation scope remains small.

Minimum contracts:

- `CardDefinition`
- `CardZoneDefinition`
- `CardRuntimeState`
- `CardZoneRuntimeState`
- `ICardZoneService`

Minimum baseline behavior:

- deck, hand, discard, play zone names
- draw card
- move card between zones
- play card as an `ActionDefinition`
- resource-cost validation hook

Card runtime should not block board-game foundations if it would make the first implementation too broad.

## Runtime Parity Matrix

Create and maintain a parity matrix that grades each runtime lane against the same standards.

Initial lanes:

- 2D side-scrolling shooter
- 2D arcade pickup/hazard loop
- 3D/FPS-style pawn
- 3D brawler/fighter
- projectile/guns
- turn/menu action
- board/tabletop
- card/hand/deck
- scoring/objectives
- camera/cursor control
- local multiplayer
- optional networking

Each lane should track:

- setup pattern exists
- starter pack or sample exists
- core runtime services exist
- authoring assets exist
- guided inspectors exist
- setup-flow validation exists
- scene/prefab readiness validation exists
- EditMode tests exist
- PlayMode tests exist where runtime behavior needs Unity
- docs are current
- known limits are explicit

This matrix should live in durable docs and be updated as implementation slices land.

## Authoring And Inspector UX

Every new `MonoBehaviour` or `ScriptableObject` intended for creators should receive guided authoring.

Inspector guidance should answer:

- what this asset/component is for
- when to use it
- what to create first
- what fields can stay empty
- what validation means
- what common mistakes to avoid

Setup Flow should grow route-specific checks for:

- selected tabletop pattern but missing board service
- selected turn/menu pattern but missing turn/order/action queue service
- board setup with no board definition
- pieces with duplicate ids
- pieces placed on missing spaces
- action definitions with board targets but no board service
- win condition selected but no resolver/service can evaluate it

Repair buttons should stay conservative. They can add obvious local services or create starter assets, but should not make full design choices silently.

## Data Flow

Expected tabletop flow:

```text
GameSetupProfile
  -> selects Board/Card/Tabletop + Turn/Menu Action + Camera/Cursor + Scoring if needed

SessionDefinition
  -> defines participants as seats/sides/players

GameModeDefinition
  -> references setup profile and optional board/turn/scoring profiles

GameplaySessionBootstrap
  -> creates participant/session services and registers rule services

BoardRuntimeService
  -> creates board state from BoardDefinition

TurnOrderService
  -> tracks active participant and phase

ActionQueueService
  -> validates and resolves ActionDefinition requests through registered rule resolvers

Rule definitions/resolvers
  -> mutate board/card/score/turn state and report results
```

Scene objects should present this state. They should not be the only source of truth for rules.

## Testing Strategy

Core rule behavior should be tested mostly in EditMode or pure NUnit-style tests:

- board definition validation
- board state initialization
- occupancy rules
- legal move generation
- capture rules
- active participant/turn gate rules
- action queue validation and resolve order
- win-condition checks
- card zone state movement if included

Unity PlayMode tests should cover:

- service lifecycle through `GameplaySessionBootstrap`
- setup-flow services created or assigned correctly
- scene-facing board/piece presenters receive state updates
- local 2-player no-pawn tabletop session can register participants and advance turns

Editor/source contract tests should cover:

- guided inspector coverage
- Add Component / Create Asset menu discoverability
- setup docs mentioning real supported tabletop path
- parity matrix staying present and current

## Documentation Updates

Update:

- `FEATURE_DEVELOPMENT_SCOPE.md`
- `FEATURE_DEVELOPMENT_ROADMAP.md`
- `FEATURE_INVENTORY.md`
- `CURRENT_STATE_AUDIT.md`
- `Docs/Setup/START_HERE.md`
- `Docs/Setup/RUNTIME_PATTERN_COOKBOOK.md`
- `Docs/Setup/Prefabs/Board_Card_Tabletop_Setup.md`
- a new `Docs/RUNTIME_PARITY_MATRIX.md` or equivalent package-local doc

Docs should clearly separate:

- supported baseline tabletop rules
- supported extension hooks
- not-yet-supported official game completeness
- custom project rules

## Implementation Slices

### Slice A: Parity Matrix And Contracts

Create the parity matrix doc and add first core interfaces/definitions for turns, phases, board state, and action queue. Add validation tests and docs. No scene/sample work yet.

### Slice B: Board Runtime Service

Implement board state creation, space lookup, occupancy, piece ownership, move/capture mutation, and board target descriptors. Add tests and guided inspectors.

### Slice C: Turn And Action Queue Runtime

Implement turn order, active participant gating, action queue submission, resolver registration, and deterministic action resolution. Add tests and setup-flow validation.

### Slice D: Baseline Rule Packs

Implement common movement/capture/win condition rule definitions. Include checkers-style and chess-like movement primitives without overclaiming official chess completeness.

### Slice E: Starter Pack And Sample Proof

Add a tabletop starter pack that creates a playable baseline board session with two participants, a board definition, pieces, turn order, action definitions, and rule definitions. Add a small sample/proof scene only after the reusable runtime path is ready.

### Slice F: Cross-Lane Parity Follow-Ups

Use the matrix to prioritize parity gaps across 2D shooter, FPS, brawler, projectile, card, scoring, camera/cursor, and local multiplayer lanes.

## Open Decisions

These can be decided during implementation planning:

- whether card/hand/deck runtime is included in the first implementation plan or split after board/turn
- whether official chess rules are a later named rule pack or part of the first baseline
- how much scene presentation should be included before the first tabletop sample
- whether rule definitions should be polymorphic ScriptableObjects, data-only definitions plus resolver classes, or a hybrid

Recommended defaults:

- split card/hand/deck after board/turn if scope gets large
- implement chess-like movement primitives first, official chess later
- keep first scene presentation minimal
- use a hybrid rule model: data-only definitions for common rules and resolver interfaces for custom coded behavior

## Success Criteria

This phase is successful when:

- tabletop routes have real reusable runtime services, not only docs
- a normal no-pawn board game can be assembled from package assets and services
- custom rules can be added through focused resolver interfaces without editing core systems
- Setup Flow can distinguish "tabletop contract only" from "tabletop runtime ready"
- core rules tests protect legal moves, capture, turns, action queue, and win checks
- parity matrix makes weak runtime lanes visible before scene work starts
- docs accurately state what Pyralis supports today

