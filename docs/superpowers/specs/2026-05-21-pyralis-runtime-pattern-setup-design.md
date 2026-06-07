# Pyralis Runtime Pattern Setup Design

Date: 2026-05-21

## Purpose

Pyralis needs a developer-facing way to describe what kind of game is being assembled without forcing that game into one rigid genre lane. The next platform slice should introduce composable runtime setup patterns so a game can declare that it uses realtime character control, projectile combat, turn/menu actions, board/card systems, scoring, hazards, procedural generation, or camera/cursor control in any useful combination.

The goal is to make Pyralis feel like a gameplay composition platform. A developer should be able to start from a setup profile, inspect what systems are expected, and receive useful validation when required pieces are missing.

## Product Principle

Runtime patterns are setup recipes, not exclusive game types.

A game may combine several patterns:

- side-scrolling shooter: realtime character + projectile combat + side-scrolling playfield + scoring
- tactics RPG: board/grid + turn/menu actions + combat + projectile delivery
- card battler with animated actors: board/card/tabletop + turn/menu actions + combat resolution + character presentation
- tower defense: camera/cursor control + projectile combat + wave spawning + scoring
- brawler with guns: realtime character + brawler combat + projectile combat + animation mapping

Overlap is expected and supported.

## Target User Experience

A developer creates or selects a game setup profile, assigns one or more runtime patterns, and uses that profile as the project-level explanation of the game loop. Pyralis uses those patterns to communicate:

- whether participants require pawns
- what control surfaces are valid
- what capability families are expected
- which systems are required or optional
- which other patterns combine well
- which combinations may need warnings
- what setup docs or samples are relevant

The first slice does not need to generate full scenes. It should create the data model and validation surface that a future setup wizard can safely build on.

## Core Concepts

### Runtime Pattern Definition

`RuntimePatternDefinition` describes one reusable setup recipe.

Examples:

- Realtime Character
- Projectile Combat
- Turn/Menu Action
- Board/Card/Tabletop
- Camera/Cursor Control
- Side-Scrolling Playfield
- Scoring
- Procedural Segments
- Animation Mapping

Each pattern should describe:

- stable id
- display name
- description
- capability family
- control surfaces it supports
- participant embodiment requirements
- required runtime systems
- optional runtime systems
- recommended companion patterns
- conflicting or cautionary patterns
- setup notes

This asset should live in the data/definition layer because it describes authoring intent rather than executing gameplay.

### Game Setup Profile

`GameSetupProfile` describes a particular game loop by referencing multiple runtime patterns.

Examples:

- `Setup_SideScrollingShooter`
- `Setup_BrawlerWithGuns`
- `Setup_CardBattler`
- `Setup_TacticsPrototype`

The profile should expose:

- setup name
- summary
- selected runtime patterns
- optional setup notes
- validation method that aggregates pattern expectations

This profile is the object future editor tooling, setup wizards, documentation, and sample generation should read.

### Control Surface

Control surfaces describe what a participant manipulates.

Initial values should include:

- Pawn
- Camera
- Cursor
- Menu Selection
- Board Seat
- Board Piece
- Card Hand
- Faction
- System/AI
- Custom

This keeps the platform honest: a participant does not need to be a character controller.

### Participant Embodiment

Participant embodiment describes whether the pattern expects a pawn.

Initial values should include:

- None Required
- Optional Pawn
- Required Pawn
- Non-Pawn Surface Required
- Custom

This gives validation a simple way to distinguish a brawler from a card game without hardcoding genre logic.

### Capability Family

Capability family groups patterns into understandable product areas.

Initial values should include:

- Platform Core
- Character/Pawn Gameplay
- Action/Targeting
- Combat
- Guns/Projectiles
- Procedural Generation
- Board/Card/Tabletop
- Animation/Presentation
- Scoring/Objectives
- Camera/Input
- Networking
- Custom

## Validation Behavior

The first implementation should provide lightweight validation, not a full scene analyzer.

Pattern validation should catch:

- missing stable id
- missing display name
- no supported control surfaces
- required pawn pattern with no pawn control surface
- non-pawn required pattern that only declares pawn control
- duplicate patterns in a game setup profile
- conflicting pattern pairs
- empty setup profile

Validation should be plain C# so it can be tested without opening Unity scenes.

Future validation can inspect:

- scene services
- session definitions
- participant definitions
- pawn definitions
- action definitions
- projectile launchers
- board/card zones
- animator mappings
- sample prefab completeness

## Data Flow

The intended first-slice flow is:

```text
RuntimePatternDefinition
  -> describes one reusable setup recipe

GameSetupProfile
  -> selects multiple RuntimePatternDefinitions
  -> validates overlap, cautions, and missing metadata

SessionDefinition / GameModeDefinition
  -> may later reference GameSetupProfile
  -> uses the profile as setup intent

Future setup wizard
  -> reads GameSetupProfile
  -> creates or validates scene systems, assets, and prefabs
```

The first slice may stop before wiring `GameSetupProfile` into `SessionDefinition` if doing so keeps the change safer. If wired immediately, it should be optional and backward-compatible.

## Editor And Authoring

The first slice should add enough authoring support to make the assets usable:

- CreateAssetMenu entries for runtime patterns and game setup profiles
- validation methods on the assets
- editor tests that protect validation behavior
- sample authoring pack entries for a few canonical patterns, if the existing example factory can support them cleanly

Full custom inspectors and setup wizard generation are future work.

## Initial Canonical Patterns

The implementation should include or support authoring these canonical pattern examples:

### Realtime Character

For arcade, platformer, brawler, side-scroller, and action games.

Expected control surfaces:

- Pawn

Participant embodiment:

- Required Pawn

Recommended companion patterns:

- Projectile Combat
- Scoring
- Animation Mapping
- Side-Scrolling Playfield

### Projectile Combat

For guns, turrets, spells, traps, enemy attacks, and projectile-heavy games.

Expected control surfaces:

- Pawn
- Camera
- Cursor
- Board Piece
- Card Hand
- System/AI

Participant embodiment:

- Optional Pawn

Recommended companion patterns:

- Realtime Character
- Turn/Menu Action
- Board/Card/Tabletop
- Camera/Cursor Control

### Turn/Menu Action

For tactics, menu RPG, command selection, card actions, and action queues.

Expected control surfaces:

- Menu Selection
- Cursor
- Board Seat
- Card Hand
- Pawn

Participant embodiment:

- Optional Pawn

Recommended companion patterns:

- Board/Card/Tabletop
- Projectile Combat
- Combat

### Board/Card/Tabletop

For games with seats, spaces, pieces, cards, hands, decks, zones, turns, and legal moves.

Expected control surfaces:

- Board Seat
- Board Piece
- Card Hand
- Cursor

Participant embodiment:

- Non-Pawn Surface Required

Recommended companion patterns:

- Turn/Menu Action
- Camera/Cursor Control
- Scoring

### Camera/Cursor Control

For strategy, builder, board, card, tabletop, puzzle, inspection, and commander-style games.

Expected control surfaces:

- Camera
- Cursor
- Menu Selection
- Faction

Participant embodiment:

- Non-Pawn Surface Required

Recommended companion patterns:

- Board/Card/Tabletop
- Projectile Combat
- Procedural Segments

## Testing

Tests should cover:

- runtime pattern validation passes for a well-formed pattern
- validation fails for missing id or display name
- validation fails for impossible pawn/non-pawn control-surface combinations
- game setup profile validation fails for duplicate pattern references
- game setup profile validation surfaces conflicts
- game setup profile validation accepts overlapping compatible patterns

If the example authoring pack is updated, an editor source test should verify that it creates or references the intended pattern/profile assets.

## Documentation

Update durable docs to explain:

- runtime patterns are composable, not exclusive
- overlap is expected
- `GameSetupProfile` is the bridge between product intent and future setup tooling
- participant does not imply pawn
- future samples and setup wizards should build on this model

Likely docs to update:

- `FEATURE_DEVELOPMENT_SCOPE.md`
- `FEATURE_DEVELOPMENT_ROADMAP.md`
- `FEATURE_INVENTORY.md`
- possibly `ARCHITECTURE_BLUEPRINT.md`

## Non-Goals

This slice should not build:

- a full scene generation wizard
- board/card runtime rules
- procedural segment generation
- action queues
- turn order
- weapon inventory
- animation controller inspection
- network authority changes

Those systems should consume this setup model later.

## Implementation Boundaries

Prefer a small data-first slice:

- data definitions in the existing data assembly
- validation in plain C# methods on definitions/profiles
- optional references from session or game mode only if backward-compatible
- editor tests before or alongside implementation
- docs updated as part of the same checkpoint

Avoid creating runtime managers that do not yet execute behavior. The point of this slice is to name and validate setup intent so later systems have a shared source of truth.

## Success Criteria

The slice is successful when:

- Pyralis has authored runtime pattern assets
- a game setup profile can combine multiple patterns
- overlapping patterns validate cleanly when compatible
- impossible combinations produce readable validation issues
- docs explain the setup model clearly
- tests protect the core validation behavior
- the implementation does not require every game to have a pawn

