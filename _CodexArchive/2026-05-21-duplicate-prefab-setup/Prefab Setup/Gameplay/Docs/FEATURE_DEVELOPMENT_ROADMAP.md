# Pyralis Feature Development Roadmap

This roadmap turns the broader feature scope into executable platform slices.

## Roadmap Rule

Each slice should produce reusable platform value, compile cleanly, update docs, and leave a small test or validation signal behind.

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
- runtime and editor tests

Not included yet:

- visual target selection
- action queues
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
- example authoring pack assets for a sample hitscan projectile, fire mode, and impact definition

Not included yet:

- charge and recoil policies
- sample weapon prefab path
- richer pool prewarm/editor setup tools
- inventory-level ammo ownership across multiple fire modes
- projectile trail/impact material presets

## Slice 3: Runtime Pattern Setup

Status: foundational code added.

Goal:

- create composable setup recipes that describe overlapping game-loop expectations without making game type an exclusive dropdown

Initial deliverables added:

- `RuntimeControlSurface`
- `ParticipantEmbodimentRequirement`
- `RuntimeCapabilityFamily`
- `RuntimePatternDefinition`
- `GameSetupProfile`
- validation for missing identity, missing control surfaces, pawn/non-pawn mismatches, duplicate setup patterns, and cautionary pattern combinations
- example authoring pack assets for realtime character, projectile combat, turn/menu action, board/card/tabletop, camera/cursor control, and a composed brawler-with-projectiles setup
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

Goal:

- support non-realtime game loops and menu-driven commands using the action core

Expected deliverables:

- turn order service
- phase definitions
- action queue or command queue
- selectable action menus
- validation and execution hooks for AI or local players

## Slice 7: Board, Card, And Tabletop Foundations

Goal:

- let Pyralis support games where the participant controls a board seat, hand, cursor, or faction rather than a pawn

Expected deliverables:

- board spaces and pieces
- decks, hands, discard, and zones
- legal move validation hooks
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

## Current Priority

Use the new runtime pattern setup spine to keep future work composable. Near-term options are finishing projectile authoring polish, adding animation compatibility tooling, or building the first scene-readiness validator; each should declare which `RuntimePatternDefinition` and `GameSetupProfile` path it strengthens.
