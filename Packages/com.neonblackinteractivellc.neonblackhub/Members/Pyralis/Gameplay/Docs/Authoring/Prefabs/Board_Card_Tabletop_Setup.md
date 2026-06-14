# Board, Card, And Tabletop Setup

Use this chapter when the game does not start with a character controller.

Good fits:

- board-space interaction
- card hands, decks, piles, or zones
- tactical boards
- tabletop-style turn games
- seat, side, faction, hand, or marker ownership
- camera-only board inspection
- menu-driven combat with no spawned pawn body

The important rule is simple: a player does not require a pawn. In Pyralis, a player is usually a `ParticipantDefinition`. A pawn is only needed when that participant owns an actor GameObject in the scene.

## Non-Pawn Tabletop MVP quick path

Create the no-pawn route manually from `Create -> NeonBlack`: `Session Definition`, `Game Mode Definition`, one or more `Participant Definition` assets for seats, hands, sides, or factions, `Board Definition`, `Board Piece Definition`, `Board Move Policy`, `Turn Order Definition`, and optional `Board Terminal Condition` assets. Use Intent to filter the reflected graph toward board/card/tabletop, action-selection, camera/cursor, UI, and scoring concerns. Keep assets in a project-owned setup folder so the route is visible and editable.

First proof loop:

1. Add `GameplaySessionBootstrap` and `PyralisGameplayLifetimeScope` to a `Gameplay Root`.
2. Assign the authored `SessionDefinition` to `GameplaySessionBootstrap`.
3. On each participant, leave `Default Pawn` empty.
4. On `GameplaySessionBootstrap`, leave `Spawn Points` empty.
5. Add a board root with `TabletopBoardGridPresenter`.
6. Assign the authored `BoardDefinition`, `BoardMovePolicyDefinition`, `TurnOrderDefinition`, and optional project-owned space or piece prefabs.
7. If the proof uses turns, add a simple UI label and `TabletopTurnStatusPresenter` so the active seat is visible.
8. Press Play, select one generic token, card, marker, or board piece, select a valid destination or action target, and confirm that board, turn, score, or UI state changes.
9. Keep the authored `TurnOrderDefinition` and `BoardTerminalConditionDefinition` assets assigned on the tabletop `GameModeDefinition` when the prototype needs turn flow and win/loss checks.

This path proves that participants can be seats or sides, not pawn owners. Add pawns later only when the game design needs visible actor bodies.

## What This Sets Up

This setup gives you a no-pawn session:

1. `GameplaySessionBootstrap` starts the scene.
2. `SessionDefinition` says which mode starts and which participants exist.
3. `SessionDefinition.defaultGameMode` points to the `GameModeDefinition`.
4. `GameModeDefinition`, participants, board/turn definitions, feature modules, scene evidence, or grammar vocabulary and reflected contracts expose board, card, turn, action, camera, cursor, UI, or scoring route evidence.
5. Scene roots provide the visible board, cards, cursor, UI, scoring, and turn flow.

## Before You Start

Read:

- `START_HERE.md`
- `AUTHORING_MODEL.md`
- `ROUTE_CAPABILITY_COOKBOOK.md`
- `Prefabs/Bootstrap_Example_Setup.md`

You do not need:

- `PawnDefinition`
- pawn prefab
- spawn points
- `PawnRoot`
- pawn movement profiles
- pawn animation profiles

Add those later only if the tabletop game grows pieces that need actor bodies, animation, combat reactions, or feature modules.

## Assets To Create

Create these assets:

1. `SessionDefinition`
2. `GameModeDefinition`
3. one `ParticipantDefinition` per seat, player, hand, faction, or side
4. `ActionDefinition` assets for legal actions such as move piece, play card, end turn, draw, discard, pass, confirm, or cancel
5. graph vocabulary, contracts/reflection, and scene evidence for route metadata

In Unity, create these from the `Assets/Create/NeonBlack/...` menus where available. Keep them in a folder such as:

- `Assets/Game/Setup/Tabletop`

## How To Wire The Assets

In the Authoring Window:

- use Intent to filter guidance toward board/card/tabletop route evidence
- include action-selection, camera/cursor, UI, and scoring filters only if the game uses them
- add optional capabilities only when the current proof needs them
- use notes and asset names to explain the exact game shape

On `GameModeDefinition`:

- assign a `PlayfieldProfile` only if you want authored board bounds, lanes, spaces, or zones
- assign a `CameraRigProfile` only if the mode uses the Pyralis camera flow
- enable scoring only if this mode uses the shared scoring service
- leave combat, pickups, hazards, and respawn off unless the tabletop game uses them

On `SessionDefinition`:

- assign the `GameModeDefinition` to `SessionDefinition.defaultGameMode`

- assign the `GameModeDefinition` as the default game mode
- assign the participant definitions
- assign default input/settings profiles only if they apply to the session

On each `ParticipantDefinition`:

- leave `Default Pawn` empty
- assign a seat index, player id, team, hand, faction, or side using the fields your game uses
- assign input only if that participant is controlled directly by local input

## Scene Objects To Create

Always create:

- `Gameplay Root`

Attach:

- `GameplaySessionBootstrap`
- `PyralisGameplayLifetimeScope`

On `GameplaySessionBootstrap`:

- assign `Session Definition`
- keep `Auto Create Core Services` enabled
- keep `Inject Loaded Scenes On Build` enabled
- leave `Spawn Points` empty
- leave `Player Input Manager` empty unless using local join
- assign `Camera Rig Controller` only if you created a Pyralis camera root

Usually create:

- `UI Root`
- `Playfield Root`
- `Camera Root`

Optional roots:

- `Scoring Root` when using `ParticipantScoreService`
- `Scene Flow Root` when using fades or menu transitions
- `Settings Root` when using shared settings

## Selection And Action Bridge

Add `TabletopBoardSelectionBridge` when the scene needs board-space selection to become a queued rule action.

For the first scene proof, add `TabletopBoardGridPresenter` to a board root. Assign its `BoardDefinition`, optionally assign a `BoardMovePolicyDefinition`, and press Play. The presenter builds selectable board-space objects, creates piece views from starting pieces, initializes `TabletopBoardSelectionBridge`, and routes selected spaces into `ActionQueueService`.

The bridge does not draw the board and does not decide legal moves. Your project-owned board presenter, card-hand UI, cursor, or menu surface should own visuals and call:

- `Initialize(BoardRuntimeState, ActionQueueService)` after creating the board runtime state and action queue
- `TrySelectPieceAt(BoardCoordinate)` or `TrySelectPiece(string)` when the player picks a piece
- `TrySelectDestination(BoardCoordinate)` when the player picks a target space

The bridge creates a `BoardMoveActionPayload` and sends it through `ActionQueueService`, where `BoardMoveActionResolver` and the authored move policy decide whether the move is legal.

Keep `Resolve Queued Move Immediately` off when a turn runner, animation queue, command log, or network layer owns action resolution. Turn it on only for simple prototypes that need the selected move to resolve as soon as it is queued.

## Board And Card Surfaces

Pyralis does not force one board implementation. A no-pawn game can expose control through several surfaces:

- board-space GameObjects with colliders
- UI buttons for spaces, cards, hands, decks, discard piles, and actions
- raycast targets under a camera
- generated grid or card-zone anchors
- menu selection lists
- custom rule systems that consume `ActionDefinition.actionId`

Keep the rule engine separate from the setup surface when possible. The setup should tell Pyralis who is playing, what mode is active, which actions exist, and which scene roots own UI/camera/board behavior.

## Action Examples

Create `ActionDefinition` assets for the moves the player can request:

| Action | Target rule idea |
|---|---|
| `action.turn.end` | no target |
| `action.piece.move` | one board space |
| `action.card.play` | one card, optional board target |
| `action.card.draw` | no target or deck target |
| `action.piece.capture` | one opposing piece or space |
| `action.choice.confirm` | current selection |
| `action.choice.cancel` | no target |

The runtime system that owns the board or card rules decides whether the selected action is legal. The `ActionDefinition` is the shared authoring contract that UI, input, and rules can point at.

## Press Play Validation

After pressing Play, check:

- `GameplaySessionBootstrap` has a `SessionDefinition`.
- `SessionDefinition` has a default `GameModeDefinition`.
- `SessionDefinition.defaultGameMode` has a `GameModeDefinition`.
- participant definitions exist even though `Default Pawn` is empty.
- no spawn points are required.
- the scene has the UI, camera, board, card, or cursor surface that players use.
- Console has no missing script or duplicate assembly errors.

It is normal for no-pawn tabletop scenes to have no `PawnDefinition`, no `PawnRoot`, and no spawned character.

## Common Mistakes

- Creating a pawn prefab just because the game has a player.
- Treating empty spawn points as a bug in no-pawn games.
- Turning on combat, pickups, hazards, or respawn before the game needs them.
- Putting legal-move rules inside UI buttons instead of a board/card rules system.
- Forgetting that participants can represent seats, hands, factions, or sides.

## What To Read Next

- `Prefabs/UI_HUD_Setup.md` for UI roots, HUDs, menus, card UI, and turn UI.
- `Prefabs/Scoring_Setup.md` for points, victory points, resources, timers, or round results.
- `Prefabs/Camera_Setup.md` for camera/cursor control.
- `Prefabs/Scene_Flow_Setup.md` for menu-to-game flow.
- `Prefabs/Pawn_Setup.md` only if you later add actor bodies.
