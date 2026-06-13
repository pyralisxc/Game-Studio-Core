# Pyralis Canonical Setup

This is the technical contract for new Pyralis gameplay scenes after the platform realignment and deferred-cleanup pass.

If you are wiring your first scene, use the Pyralis Authoring Window for route setup and use Inspector Field Guides only for the asset or component you are editing. Read `START_HERE.md` when you need the written first-scene path. Use this file as the technical contract once the basic setup flow makes sense. The manual, prefab, and subsystem setup docs support it; they should not contradict it.

For feature-driven authoring contracts, add contracts in the feature package and let `ResolvedAuthoringContractRegistry` discover them reflectively. New package scripts/assets must carry `.meta` files and should be refreshed in Unity before relying on CLI build gates.

## 1. Core Scene Root

For a first playable scene, create one empty GameObject named `Gameplay Root`.

Attach these components to `Gameplay Root`:

- `GameplaySessionBootstrap`
- `PyralisGameplayLifetimeScope`

`GameplaySessionBootstrap` will add `PyralisGameplayLifetimeScope` automatically if it is missing, but adding both up front makes the scene easier to inspect.

For the movement-first proof pass, stop after these links:

- `GameplaySessionBootstrap` with `Session Definition`
- at least one pawn-backed participant path (`ParticipantDefinition` + `PawnDefinition` + prefab)
- `Spawn Points` with at least one transform
- a known input route for that participant (`InputProfile` or local join flow)
- a Cinemachine-backed `Camera Root` assigned to `GameplaySessionBootstrap > Camera Rig Controller` for 2D pawn movement bounds and framing
- auto-created core services (left enabled)

Delay scoring, HUD, combat, scene-flow, pickup/hazard, and network extras until movement proof is confirmed in Play mode.

On `GameplaySessionBootstrap`, assign:

- `Session Definition` - your `SessionDefinition` asset
- `Dont Destroy On Load` - on for persistent bootstrap scenes, off for isolated test scenes
- `Auto Create Core Services` - on for first-scene proofs that use the standard bootstrap-owned service path
- `Inject Loaded Scenes On Build` - on unless you have a custom injection flow
- `Spawn Points` - optional Transforms where pawn-backed participants should appear
- `Player Input Manager` - optional, only when using local join or Unity Input System player joining
- `Camera Rig Controller` - optional, assign the `Camera Root` when using Pyralis camera control or camera-aware 2D bounds
- `Camera Bounds Source` - optional, use only for specialized custom `ICameraBoundsProvider` services; the `CinemachineCameraRigController` provides bounds when assigned as the camera rig

With the standard bootstrap-owned service path enabled, the bootstrap creates these child objects at runtime when they are not assigned:

- `SceneLoader`
- `TimeManager`
- `CameraShake`
- `SessionStateService`
- `ParticipantRosterService`
- `ParticipantSpawnService`
- `ParticipantInputRouter`

You usually do not need to create those service objects by hand for a first scene.

Create additional root objects only when the selected runtime patterns need them:

| Root object | Attach | Use when |
|---|---|---|
| `Camera Root` | `CinemachineCameraRigController` plus your Cinemachine camera component, `CameraRigProfile`, Target Camera assignment, and Cinemachine Brain verified on the physical Target Camera. The normal route keeps or creates one physical Unity Camera, usually the default Main Camera, and adds separate Cinemachine Camera GameObjects that control it. Unity usually adds the Brain when you create a Cinemachine Camera; add it manually only if missing. For 2D movement or bounded views, the physical Target Camera or assigned CameraRigProfile must be orthographic. | The setup uses shared camera, split screen, camera/cursor control, board view, camera profiles, or 2D visible bounds |
| `Input Root` | Unity `PlayerInputManager` | The setup supports multiple local players joining during play |
| `UI Root` | Canvas, EventSystem, UI presenters such as `UIManager`, HUD binders, board/card/action presenters, or menu screens | The setup has HUD, menus, cards, board UI, turn UI, action selection, prompts, or settings UI |
| `Settings Root` | `SettingsManager` | The setup needs reusable volume, deadzone, fullscreen, or settings persistence |
| `Scene Flow Root` | `SceneFader` | The setup uses fade transitions or central scene loading from menus |
| `Scoring Root` | `ParticipantScoreService` | The setup tracks points, timers, victory points, resources, or round results |
| `Playfield Root` | authored bounds, spawn zones, board anchors, card zones, encounter zones, pickup/hazard spawners | The setup needs placed gameplay surfaces |

`GameplaySessionBootstrap` is the supported composition root. New scenes should not build their own global service wiring.

Beginner rule: start with only `Gameplay Root`, then add the optional roots that match the capability ingredients in your `GameSetupProfile`.

## 2. Required Authoring Chain

Create these authored assets for every new setup:

- `GameSetupProfile`
- `SessionDefinition`
- `GameModeDefinition`
- at least one `ParticipantDefinition`

Select one or more runtime capability families in `GameSetupProfile.runtimeCapabilities`. Optional `RuntimePatternDefinition` contracts are advanced metadata only; create or assign them when a capability needs reusable route detail that the generic family cannot express.

Create pawn assets only when a participant needs an actor body in the scene:

- `PawnDefinition`
- `PawnPresentationProfile`
- `PawnMovementProfile` when movement is data-authored
- `InputProfile` when participant input drives the pawn directly. Assign your Unity Input Action Asset, set the primary action map, then map Pyralis gameplay roles to your project's action names.
- `PawnAnimationProfile` and `ActorAnimationDefinition` when the pawn has Animator-driven visuals
- supporting pawn profiles as needed: combat, traversal, pickups, feedback, interaction, status

Create non-pawn setup assets only when the selected capabilities need them:

- camera, cursor, UI, board, card, scoring, settings, scene-flow, or playfield profiles and scene roots

Template or scaffold tooling is not the active first-test path. The primary learning and validation path is the Authoring Window guiding native Unity setup: create assets in the selected project folder, add components through the Inspector, and assign references through Inspector fields. Future scaffolds must be downstream of a manually proven route and must not count as authoring-validation evidence.

Before wiring scenes or prefabs, use the `GameSetupProfile` inspector to confirm:

- the setup selects the intended overlapping runtime capabilities
- pawn-backed games include a pawn-compatible capability
- board, card, tabletop, camera, cursor, or menu games include non-pawn control surfaces
- projectile-heavy games include projectile or combat capabilities
- turn/menu games include action or targeting capabilities
- validation issues are resolved or intentionally deferred

Assign the setup profile to `GameModeDefinition.setupProfile` so game-mode validation can surface setup problems early.

## 3. Participant And Pawn Wiring

Every game has participants. A participant can be a player, AI, seat, hand, faction, camera controller, or turn owner.

For non-pawn games, a participant can stop at:

- `ParticipantDefinition`
- input, UI, board, card, cursor, or turn-control surfaces

For pawn-backed games, each spawned participant should resolve through:

- `ParticipantDefinition`
- `PawnDefinition`
- a pawn prefab that includes `PawnRoot`

`PawnRoot` is the runtime composition root for a pawn. It applies authored profiles and installs feature modules from `FeatureModuleDefinition`.

A pawn prefab is normal Unity content, not a Pyralis preset. Create it in the Hierarchy or Prefab Mode, add the lane components your intent requires, wire visible Inspector fields, then drag the finished prefab into `PawnDefinition > Pawn Prefab`. The authoring system should explain and validate that shape; it should not silently choose the user's art, map, animation controller, combat feel, or local-multiplayer ownership.

Supported presentation targets:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

## 4. Pawn Runtime Stacks

### 2D

The current supported 2D stack is:

- `PawnRoot`
- `Motor2D`
- `Motor2DInputAdapter`
- `Pawn2DMovementComponent`
- `Pawn2DPresentationComponent`
- `SpriteRenderer`
- `Animator`
- `PawnCombatBehaviour2D` when combat is required

`Motor2DInputAdapter` is the preferred player-input bridge for this stack. `PlayerInputHandler` remains the lower-level keyboard, gamepad, and touch input reader used by that route when direct input handling is needed.

For new 2D player pawns, do not add both `Motor2DInputAdapter` and a separate `PlayerInputHandler` to the same prefab. `Motor2DInputAdapter` already provides the supported input-handler bridge for the first movement proof, and duplicate handlers make input ownership harder to reason about.

For a beginner 2D movement proof, the clean prefab route is:

1. Create a Hierarchy GameObject and name it for the pawn.
2. Add `SpriteRenderer` and assign a visible sprite before Play Mode.
3. Add `Animator` even if the controller will be assigned later; this keeps the animation route explicit.
4. Add `PawnRoot`, then assign the matching `PawnDefinition`.
5. Add `Motor2D`; Unity adds `Pawn2DMovementComponent` and `Pawn2DPresentationComponent`.
6. Add `Motor2DInputAdapter`.
7. Add Unity `PlayerInput` only when the proof needs explicit local keyboard/gamepad ownership, and assign the same Input Actions asset used by the `InputProfile`.
8. Save the GameObject as a prefab and assign it to `PawnDefinition > Pawn Prefab`.

`Motor2D` is the shared 2D pawn motor surface. Movement, presentation, and input live in focused sibling components so the stack stays inspectable and profile-driven.

The 2D input stack reads movement, dash/jump, attack, secondary attack, interact, and block action names from the effective `InputProfile`. This lets project Input Actions keep custom names while Pyralis still knows which gameplay role each action fills.

### 3D / 2.5D

The current supported 3D stack is:

- `Motor3D`
- `Pawn3DInputModule`
- `Pawn3DMovementComponent`
- `Pawn3DTraversalComponent`
- `Pawn3DPresentationComponent`

This is the canonical direct module-composition path for `Billboard2_5D` and `Rigged3D`.

The 3D input stack also reads action names from the effective `InputProfile`, including move, look, jump, attack, secondary attack, interact, sprint, crouch, roll, block, weapon cycle, and look-around roles.

## 5. Feature Modules

Feature modules are authored through `FeatureModuleDefinition` and installed through `PawnDefinition.featureModules`.

Important rules:

- every reusable feature module should provide an explicit `[AuthoringContract]` on the owning feature type
- `ResolvedAuthoringContractRegistry` discovers contracts reflectively; do not add central hardcoded module-id registries
- the contract must declare required profile type, dependency interfaces, physical Unity component placement requirements, supported lanes, unsupported lanes, action roles, native setup actions, assignment fields, customization moments, developer first-proof guidance, and `SetupNodeId` when the contract enriches a stable resolved setup graph node
- every declared `FirstProofTargetId` must map to a real `PyralisAuthoringRouteProof` fact
- every feature module must declare network intent
- runtime prefabs must expose the required feature runtime interfaces, while actor roots, scene roots, UI roots, and other authored objects must expose only the physical component requirements declared for that placement
- feature-owned authored profiles should live with the feature whenever practical

The Authoring Window reads these contracts for setup guidance, proof target guidance, dependency surfaces, physical Unity placement requirements, and unsupported lane cautions. The feature module Inspector and contract validator use the same contract data for profile, runtime-interface, and lane validation. Keep feature-specific authoring rules in the feature contract; central definitions should only keep generic `FeatureModuleDefinition` rules.

When adding a module, add the contract metadata, asmdef references, `.meta` files, registry tests, validation tests, proof-target tests, and docs update in the same slice. Do not patch generated `.csproj` files; refresh Unity so project files are regenerated from the package assets.

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
- set `SessionDefinition.networkMode` to an NGO mode only when the scene owns network authority and spawn rules
- use `NetworkManager` + `UnityTransport` for the supported MVP backend
- add `NetworkObject` to pawn prefabs and register them in Network Prefabs before using networked participant spawning
- use `FeatureModuleDefinition.networkRole` and authority/prediction metadata to describe feature behavior before claiming it is network-ready

Supported MVP routes:

- `GameplaySessionBootstrap` selects networked session, roster, spawn, ownership, and authority services when `networkMode` is `NetcodeHost`, `NetcodeClient`, or `NetcodeServer`
- networked participant spawning calls NGO spawn/despawn on the server path and assigns participant-specific ownership
- local authority checks compare the resolved participant owner to the local NGO client id
- setup validation catches missing `NetworkManager`, `UnityTransport`, `NetworkObject`, and Network Prefab registration

Enhancement lanes not claimed by the MVP:

- rollback, movement reconciliation, projectile reconciliation, remote input streams, lobby/matchmaking, or replicated animation state

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

1. `Docs/Authoring/START_HERE.md`
2. `Docs/Authoring/AUTHORING_MODEL.md`
3. `Docs/Authoring/Prefabs/Bootstrap_Example_Setup.md`
4. `Docs/Authoring/RUNTIME_PATTERN_COOKBOOK.md` when choosing or authoring setup patterns
5. `Docs/Authoring/Prefabs/Pawn_Setup.md` only for pawn-backed games
6. feature-specific setup docs as needed
7. `Docs/Authoring/Systems/Architecture_Overview.md` when changing architecture

## 9. Rules For New Docs

When writing or updating setup docs:

- prefer canonical type names over historical aliases
- describe the current DI/composition path and avoid global lookup language
- point back to this doc when introducing subsystem-specific setup
