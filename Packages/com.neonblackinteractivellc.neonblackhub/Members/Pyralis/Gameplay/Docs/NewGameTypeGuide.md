# Adding a New Game Type to Pyralis

This guide describes the current game-type pattern after the runtime-pattern and setup-profile pass.

## Core rule

New game types should start from:

- `GameSetupProfile`
- one or more `RuntimePatternDefinition` assets
- `GameModeDefinition.setupProfile`
- `SessionDefinition`
- the prefab and scene setup guides under `Docs/Authoring/`

Use pawns only when the selected runtime patterns need authored actor behavior. Camera-only, board, card, tabletop, puzzle, and turn/menu games can be valid Pyralis games without a character controller.

Do not build new game types by making Animator behavior, scene-specific globals, or a genre folder the primary gameplay owner.

## Choosing runtime patterns first

Pick the smallest set of patterns that describes how the game is actually played:

- Realtime Character for player-controlled actors, brawlers, platformers, action RPGs, or arcade character loops
- Combat for melee, brawler, fighter, health, damage, reactions, and defeat rules
- Projectile Combat for guns, spells, thrown objects, hitscan, turrets, ranged enemies, or projectile hazards
- Board/Card/Tabletop for board, card, tactics, resource, seat, hand, piece, marker, and zone play
- Turn/Menu Action for command menus, card selection, tactics turns, queued actions, and menu-driven combat
- Camera/Cursor Control when the controlled surface is a camera, cursor, menu focus, pointer, or selection ray
- Scoring/Objectives for points, resources, victory points, timers, end conditions, or round results
- Procedural Generation when chunks, rooms, pickups, hazards, encounters, or boards are generated rather than placed by hand

Games can overlap patterns. A tactics card battler might use `Board/Card/Tabletop`, `Turn/Menu Action`, `Projectile Combat`, `Scoring/Objectives`, and `Camera/Cursor Control`. A brawler with guns might use `Realtime Character`, `Combat`, `Projectile Combat`, `Animation/Presentation`, and `Scoring/Objectives`.

Resolve setup-profile validation before wiring scene objects or prefab components.

## Pawn-backed game types

Pawn-backed game types should be built from:

- `PawnRoot`
- `PawnDefinition`
- profile-driven runtime modules
- shared animation signals
- `ActorAnimationDriver`

## Pawn composition contract

`PawnRoot` still discovers runtime modules by interface:

- `IPawnMotor`
- `IPawnCombatModule`
- `IPawnTraversalModule`
- `IPawnPresentationModule`

Animation is no longer a separate special-case bridge. The presentation module should apply the presentation and animation profiles to `ActorAnimationDriver`, while the gameplay modules emit signals into that driver.

## How to add a new game type

1. Create or select the `RuntimePatternDefinition` assets that describe the game.
2. Create a `GameSetupProfile` and assign those patterns.
3. Assign the setup profile to `GameModeDefinition.setupProfile`.
4. Choose whether the game is pawn-backed, camera/cursor-backed, board/card-backed, menu-backed, or a hybrid.
5. For pawn-backed games, create the runtime controller components you need.
6. Implement only the pawn interfaces that make sense for that game type.
7. Reuse `ActorAnimationDriver` instead of embedding fresh Animator logic in each controller.
8. Extend `ActorAnimationDefinition` and `PawnAnimationProfile` mappings when the new game type needs additional supported signals or custom animation behavior.

## Non-pawn game types

For board, card, tabletop, puzzle board, camera-only viewer, or menu-driven games:

1. Keep `GameplaySessionBootstrap`, `SessionDefinition`, `GameModeDefinition`, and `GameSetupProfile`.
2. Select non-pawn runtime patterns such as `Board/Card/Tabletop`, `Turn/Menu Action`, and `Camera/Cursor Control`.
3. Use scene services for UI, scoring/objectives, turn state, scene flow, settings, and camera control.
4. Add pawn or actor prefabs only when pieces need authored movement, animation, combat, or feature modules.
5. Keep input routed through the participant/session model rather than assuming one global player object.

## Official presentation targets

Your new game type should choose one of these:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

## Animation authoring expectation

Gameplay code should emit shared signals such as:

- `Move`
- `Jump`
- `Land`
- `Dash`
- `AttackPrimary`
- `AttackSecondary`
- `AttackAerial`
- `BlockLoop`
- `Hang`
- `Shimmy`
- `Interact`
- `Death`

The animation profile is responsible for translating those signals into Unity Animator parameters and triggers.
