# Bootstrap Example Setup

This is the fastest clean setup path for a new scene.

The first scene object is `GameplaySessionBootstrap`.

The first asset it reads is `SessionDefinition`.

The first place to check progress is the `GameplaySessionBootstrap` Inspector's **Setup Flow** monitor.

## Before You Wire This

Start with the setup intent:

- select or create a `GameSetupProfile`
- choose runtime capability ingredients in the Authoring Window Intent tab or in `GameSetupProfile.runtimeCapabilities`
- add existing `RuntimePatternDefinition` assets only when the generic capability ingredients need advanced reusable metadata
- assign the setup profile to `GameModeDefinition.setupProfile`
- resolve setup-profile validation before placing scene services

Use manual native authoring for validation passes. Future scaffold tooling can capture a proven route later as editable project assets, but it should not replace the test of whether Setup Flow and the Authoring Window can guide asset creation and field wiring.

## 1. Create authoring assets

Manually create the always-needed assets:

- `SessionDefinition`
- `GameSetupProfile`
- `GameModeDefinition`
- `ParticipantDefinition`

Optional enrichment:

- existing `RuntimePatternDefinition` assets only when a capability needs reusable route detail that the generic family cannot express

For pawn-backed games, also create:

- `PawnDefinition`
- `PawnPresentationProfile`
- `PawnMovementProfile` if the pawn moves through Pyralis movement modules
- `InputProfile` if participant input drives the pawn directly
- `PawnAnimationProfile` and `ActorAnimationDefinition` if the pawn uses Animator-driven visuals

## 2. Add session bootstrap

Create an empty root object named `Gameplay Root`.

Select it and add these via Inspector **Add Component** search:

- `GameplaySessionBootstrap`
- `PyralisGameplayLifetimeScope` (optional for first-pass visibility; bootstrap can auto-create it)

`PyralisGameplayLifetimeScope` is created automatically by `GameplaySessionBootstrap` if missing, but adding it yourself makes the scene easier to inspect.

On `GameplaySessionBootstrap`, assign `Session Definition` first. This is the handoff from the scene to your authored setup.

Keep the bootstrap selected after this assignment. The **Setup Flow** monitor will read the assigned session chain and show the next missing setup item first:

- missing `SessionDefinition`
- missing default `GameModeDefinition`
- missing `GameSetupProfile`
- missing or invalid runtime capability ingredients
- optional runtime pattern metadata only when the selected setup uses it
- missing participants
- pawn and spawn-point requirements only when selected capabilities require pawns
- camera, input, playfield, and scoring recommendations only when selected capabilities imply them

The monitor is a safe checklist, not a scene generator. It can select and ping existing objects, copy the checklist, open the authoring window, add a visible `PyralisGameplayLifetimeScope` to this root, and restore first-scene bootstrap defaults. It does not create a whole scene, edit prefabs, or automatically wire referenced assets.

The authoring window mirrors this flow by showing the selected bootstrap/session/setup route, readiness, native next steps, and validation. Create assets from the Project window and wire references in the Inspector so the folderbase and ownership stay visible.

For a 1P movement proof, treat these as the route-required setup items:

- `Session Definition` (in the bootstrap inspector)
- one `ParticipantDefinition` and a route-appropriate input path (`InputProfile` on participant or `Player Input Manager` if local join)
- pawn routes: at least one participant `Default Pawn` plus `Spawn Points` transforms
- no-pawn routes: board/cursor/action control-surface setup first (board, camera/cursor, action UI, or menu route)

Optional in this first pass:

- `Camera Rig Controller` only when using shared camera flow now
- `CinemachineCameraRigController` as the camera rig and bounds provider when validating camera-bound proof now
- scoring/UI/scene flow roots until movement proof is stable

Leave these enabled for first-scene proofs:

- `Auto Create Core Services`
- `Inject Loaded Scenes On Build`

At runtime, the bootstrap creates the core child services it needs when they are not assigned manually: `SceneLoader`, `TimeManager`, `CameraShake`, `SessionStateService`, `ParticipantRosterService`, `ParticipantSpawnService`, and `ParticipantInputRouter`.

## 2A. Add optional scene roots

Only add the roots your `GameSetupProfile` actually needs.

| Root object | Attach | Use when |
|---|---|---|
| `Camera Root` | `CinemachineCameraRigController` and your Cinemachine camera component | Follow cameras, split screen, camera/cursor control, or camera profiles |
| `UI Root` | Canvas, EventSystem, HUD/menu/card/turn UI components | HUD, menus, cards, board UI, turn UI, or settings panels |
| `Settings Root` | `SettingsManager` | Volume, deadzone, fullscreen, or persistent settings |
| `Scene Flow Root` | `SceneFader` | Fade transitions, menu-to-game flow, restart, or return-to-menu |
| `Scoring Root` | `ParticipantScoreService` | Points, timers, victory points, resources, or round results |
| `Playfield Root` | Bounds, board anchors, card zones, encounter zones, spawners | Any placed gameplay surface or generated spawn area |

For non-pawn games such as board, card, camera-only, or turn/menu games, `Gameplay Root`, `UI Root`, `Camera Root`, and `Playfield Root` may be enough. Do not build a pawn prefab unless the game needs actor bodies.

## 3. Build the pawn prefab

Skip this section for non-pawn games.

Every pawn prefab should include:

- `PawnRoot`
- `ActorAnimationDriver` if it has Animator-driven visuals

Then choose one supported movement/presentation stack:

- 2D: `Motor2D` + `Motor2DInputAdapter` + `Pawn2DMovementComponent` + `Pawn2DPresentationComponent` (+ optional `PawnCombatBehaviour2D`)
- 2.5D: `Motor3D` + `Pawn3DInputModule` + `Pawn3DMovementComponent` + `Pawn3DTraversalComponent` + `Pawn3DPresentationComponent`
- rigged 3D: same as 2.5D, with `PawnPresentationProfile` driving rigged output

## 4. Wire animation

Skip this section when the pawn has no Animator-driven visuals.

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

- assign the pawn definition only when this participant should spawn or own an actor body
- assign input profile if participant input is used
- set the preferred seat index if needed

On the `SessionDefinition`:

- assign the participant definitions
- assign the default game mode
- assign settings profile if used

## 6. Play

At runtime:

- `GameplaySessionBootstrap` sets up session services
- participants are registered
- pawns are spawned only for participants with pawn definitions and spawn surfaces
- `PawnRoot` applies movement, combat, traversal, presentation, and animation data
- `ActorAnimationDriver` responds to gameplay signals from the pawn systems
