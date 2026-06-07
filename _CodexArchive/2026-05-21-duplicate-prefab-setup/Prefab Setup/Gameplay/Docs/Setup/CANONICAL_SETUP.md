# Pyralis Canonical Setup

This is the canonical setup path for new Pyralis gameplay scenes after the platform realignment and deferred-cleanup pass.

Use this guide first. The prefab and subsystem setup docs support it; they should not contradict it.

## 1. Core Scene Root

Create or verify these root objects:

- `GameplaySessionBootstrap`
- optional camera root using `CinemachineCameraRigController`
- optional UI root
- optional settings root
- any authored scene helpers such as playfield bounds, encounter zones, hazards, or pickup spawners

`GameplaySessionBootstrap` is the supported composition root. New scenes should not build their own global service wiring.

## 2. Required Authoring Chain

Create these authored assets first:

- `GameSetupProfile`
- one or more `RuntimePatternDefinition` assets
- `SessionDefinition`
- `GameModeDefinition`
- at least one `ParticipantDefinition`
- at least one `PawnDefinition`
- `PawnPresentationProfile`
- optional `PawnAnimationProfile`
- optional `ActorAnimationDefinition`
- supporting profiles as needed: movement, combat, traversal, camera, settings, input, pickups

If you want a faster start, use the example authoring pack generator and then replace the starter values with project-specific content.

Before wiring scenes or prefabs, use the `GameSetupProfile` inspector to confirm:

- the setup selects the intended overlapping runtime patterns
- pawn-backed games include a pawn-compatible pattern
- board, card, tabletop, camera, cursor, or menu games include non-pawn control surfaces
- projectile-heavy games include a projectile combat pattern
- turn/menu games include an action or targeting pattern
- validation issues are resolved or intentionally deferred

Assign the setup profile to `GameModeDefinition.setupProfile` so game-mode validation can surface setup problems early.

## 3. Participant And Pawn Wiring

Each participant should resolve through:

- `ParticipantDefinition`
- `PawnDefinition`
- a pawn prefab that includes `PawnRoot`

`PawnRoot` is the runtime composition root for the pawn. It applies authored profiles and installs feature modules from `FeatureModuleDefinition`.

Supported presentation targets:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

## 4. Pawn Runtime Stacks

### 2D

The current supported 2D stack is:

- `Motor2D`
- `Pawn2DMovementComponent`
- `Pawn2DPresentationComponent`
- `PawnCombatBehaviour2D`
- `PlayerInputHandler`

`Motor2D` is now a compatibility facade over the direct 2D component stack.

### 3D / 2.5D

The current supported 3D stack is:

- `Motor3D`
- `Pawn3DInputModule`
- `Pawn3DMovementComponent`
- `Pawn3DTraversalComponent`
- `Pawn3DPresentationComponent`

This is the canonical direct module-composition path for `Billboard2_5D` and `Rigged3D`.

## 5. Feature Modules

Feature modules are authored through `FeatureModuleDefinition` and installed through `PawnDefinition.featureModules`.

Important rules:

- every feature module must declare network intent
- runtime prefabs must expose the required feature runtime contracts
- feature-owned authored profiles should live with the feature whenever practical

Current extracted runtime domains include:

- `Combat`
- `Traversal`
- `Interaction`
- `Feedback`
- `Scoring`
- `Pickups`

Cross-feature camera, animation, and visual utilities live under the package-level `Presentation` domain rather than inside a feature folder.

## 6. Networking

Networking is a first-class concern, but gameplay code should target Pyralis contracts rather than backend APIs directly.

Current rules:

- use Pyralis participant/session ownership concepts
- keep backend-specific logic in `Networking/` or `Integrations/`
- do not build new gameplay systems directly on NGO types

## 7. Scoring, Pickups, And Feedback

### Pickups

Canonical runtime collector path:

- `ActorPickupCollectorFeature2D`
- `ActorPickupCollectorFeature3D`
- `IPickupCollectible`

Canonical concrete 2D authored path:

- `Collectible2D`
- `CollectibleSpawner2D`
- `CollectibleFeedback2D`

### Scoring

Canonical scoring runtime core:

- `ParticipantScoreService`

Leaderboard and stillness-bonus behavior now sit behind smaller seams:

- `ILeaderboardService`
- `LeaderboardEntry`
- `IGameplayStateReader`

### Feedback

Canonical participant feedback seam:

- `ParticipantFeedbackService`
- `IParticipantFeedbackStream`
- `IParticipantFeedbackPublisher`
- `ParticipantFeedbackMessage`
- `ParticipantFeedbackKind`

HUD is now split into:

- `ParticipantFeedbackHudPresenter`
- `ParticipantHealthHudBinder`
- `ParticipantHealthPanel`
- `ParticipantTimedTextPanel`

## 8. Recommended Reading Order

1. `Docs/Setup/CANONICAL_SETUP.md`
2. `Docs/Setup/README.md`
3. `Docs/Setup/RUNTIME_PATTERN_COOKBOOK.md`
4. `Docs/Setup/Systems/Architecture_Overview.md`
5. `Docs/Setup/Prefabs/Bootstrap_Example_Setup.md`
6. `Docs/Setup/Prefabs/Pawn_Setup.md`
7. `Docs/Setup/Prefabs/Feature_Module_Framework_Setup.md`
7. feature-specific setup docs as needed

## 9. Rules For New Docs

When writing or updating setup docs:

- prefer canonical type names over historical aliases
- describe the current DI/composition path and avoid global lookup language
- point back to this doc when introducing subsystem-specific setup
