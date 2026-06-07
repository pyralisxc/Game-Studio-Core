# Bootstrap Example Setup

This is the fastest clean setup path for a new scene.

## Before You Wire This

Start with the setup intent:

- create or select a `GameSetupProfile`
- assign one or more `RuntimePatternDefinition` assets
- assign the setup profile to `GameModeDefinition.setupProfile`
- resolve setup-profile validation before placing scene services

Use the example authoring pack when you want a ready starter chain that already includes runtime patterns and a setup profile.

## 1. Create authoring assets

Use:

- `Assets/Create/NeonBlack/Gameplay/Example Authoring Pack`

Or manually create:

- `SessionDefinition`
- `GameSetupProfile`
- one or more `RuntimePatternDefinition` assets
- `GameModeDefinition`
- `ParticipantDefinition`
- `PawnDefinition`
- `PawnPresentationProfile`
- `PawnAnimationProfile`
- `ActorAnimationDefinition`

## 2. Add session bootstrap

Create an empty root object and add:

- `GameplaySessionBootstrap`

Assign:

- `Session Definition`
- spawn points if used
- `PlayerInputManager` if using local join
- `CinemachineCameraRigController` if using shared 3D camera flow

## 3. Build the pawn prefab

Every new pawn prefab should include:

- `PawnRoot`
- `ActorAnimationDriver`

Then choose one supported pawn stack:

- 2D: `Motor2D` + `Motor2DInputAdapter`
- 2.5D: `Motor3D` + `Pawn3DInputModule` + `Pawn3DMovementComponent` + `Pawn3DTraversalComponent` + `Pawn3DPresentationComponent` + `PawnCombatBehaviour`
- rigged 3D: same as 2.5D, but `PawnPresentationProfile` uses `Rigged3D`

## 4. Wire animation

On the pawn's `PawnDefinition`, assign:

- `Presentation Profile`
- `Animation Profile`

On the animation profile:

- assign the base Animator controller
- bind the supported gameplay signals used by the pawn

On the presentation profile:

- choose `Sprite2D`, `Billboard2_5D`, or `Rigged3D`
- configure sprite facing or billboard mode as needed

## 5. Assign participants

On each `ParticipantDefinition`:

- assign the pawn definition
- assign input profile if used
- set the preferred seat index if needed

On the `SessionDefinition`:

- assign the participant definitions
- assign the default game mode
- assign settings profile if used

## 6. Play

At runtime:

- `GameplaySessionBootstrap` sets up session services
- participants are registered
- pawns are spawned
- `PawnRoot` applies movement, combat, traversal, presentation, and animation data
- `ActorAnimationDriver` responds to gameplay signals from the pawn systems
