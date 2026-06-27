# NeonBlack Gameplay

This folder contains the active Pyralis gameplay framework for Neon Black Hub.

The current source of truth is a shared gameplay stack built around:

- `GameplaySessionBootstrap`
- `PyralisGameplayLifetimeScope`
- authored `SessionDefinition`, `ParticipantDefinition`, `PawnDefinition`, and `GameModeDefinition`
- participant-owned pawns through `PawnRoot`
- participant-owned input through `ParticipantDefinition.inputProfile`
- graph-inferred participant topology for solo local, local join, networked, and hybrid routes
- pawn camera targets through `PawnCameraTarget`
- scene-owned Cinemachine routing through `CameraRigProfile.focusMode`
- explicit pawn sibling components for physical identity
- `ActorFeatureHost` for optional/swappable gameplay capabilities
- contracts, reflection, dependency-tree discovery, validators, and the resolved authoring graph

Core motto:

- Unity owns engine behavior.
- Pyralis owns gameplay meaning.
- Reflection discovers structure.
- Dependency analysis discovers setup relationships.
- Validators witness local semantic readiness.
- The graph compiles understanding.
- Authoring renders guidance.

## Supported pawn presentation targets

NeonBlack Gameplay supports three official presentation modes:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

Rigged 3D support is Animator-driven and intended for both `Generic` and `Humanoid` rigs.

## Runtime rule of thumb

- Siblings describe what a pawn **is**: root, motor, movement, presentation, input receiver, collider/rigidbody/controller, and core physical identity.
- Features describe what a pawn **can do**: interaction, traversal extras, combat styles, pickups, feedback, status, and route-specific capabilities.
- Participants describe who is **driving**: human, AI, network authority, seat, team, or non-pawn actor.
- Participant topology describes how control enters the session: solo auto-start, Unity `PlayerInputManager` local join, network authority, or hybrid local/networked join.
- Camera profiles describe what is **watched**: participant pawn socket, participant group, playfield center, explicit scene anchor, or manual Cinemachine.
- Scenes own scene-scale direction: bootstrap, lifetime scope, camera rig, spawners, scene services, and proof-specific objects.
- Authoring reads contracts/reflection/dependency evidence and projects the resolved graph; it does not create hidden presets or duplicate setup truth.

## Verification rule of thumb

- Manual Unity play proofs test gameplay feel and whether the authored route actually works.
- Automated tests protect data transfer, ownership, routing, contracts, graph projections, exports, and refactor seams.
- Authoring JSON exports show what the system currently understands and where the next cleanup pressure lives.

Do not keep broad optional domain matrices or documentation audits in the active suite. The default gate should catch broken seams without pretending to replace manual proof play.

## Layout

- `Core/`: stable contracts plus tiny shared type vocabulary such as movement modes, presentation lanes, animation signals, action value language, and narrow optional sinks
- `Data/`: ScriptableObject definitions, profiles, and config assets such as `GameConfig`; authored combat data lives here, not in the Combat runtime namespace
- `Editor/`: shared authoring helpers and custom inspectors
- `Glue/`: bootstrap, lifetime scope, session services, participant services, input routing, participant spawning, route composition, and service-registration wiring that makes authored modules run
- `Modules/`: reusable gameplay capability families such as character, combat, traversal, pickups, hazards, enemies, encounters, environment, interaction, feedback, scoring, tabletop, settings, input, spawning, RPG, and actor feature composition
- `Networking/`: ownership, authority, and backend-facing runtime contracts
- `Presentation/`: cross-feature visual and camera infrastructure
- `Tests/`: package-level validation infrastructure
- `Docs/`: setup and architecture notes

Core is intentionally not an implementation home. Authored input setup is owned by `InputProfile`, gameplay config assets live under `Data/Config`, runtime session context lives under `Glue/Lifetime`, and optional feature domains live under `Modules`.

Core contracts should stay neutral and compile-time focused. For example, `IDamageNumberSink` is a shared optional sink, but `DamageNumber` and `DamageNumberSpawner` belong to the Feedback module. `IActorCombatCommandReceiver` and `IActorGuardController` are tiny runtime seams that let motors, presentation, and input bridges talk to combat posture without importing concrete combat behavior. Combat definitions such as `WeaponData`, `CombatSequenceDefinition`, projectile definitions, and status effects are authored data under `Data.Definitions.Combat`; Combat runtime scripts consume them instead of owning their namespace.

Module folders own their local runtime composition, lane-specific implementation, scene-facing presenters, and feature-specific editor tooling when the logic is not part of the universal authoring spine. Module code should be covered by feature-owned runtime, UI, and editor asmdefs instead of falling into the aggregate root assembly. A module's runtime asmdef should sit high enough to cover runtime-owned code without pulling in feature UI/view code that depends back on authored `Data`; feature UI/view asmdefs may sit beside runtime to keep those cycles explicit and acyclic. Nested `Editor` asmdefs own editor-only tools. Lane folders use `Sprite2D`, `Billboard2_5D`, and `Rigged3D` instead of shorthand folder names. Character owns pawn identity, movement, input receiver contracts, and pawn-facing contracts; Combat owns pawn combat behavior, hitbox/damage/projectile/block/weapon modules, combat action state, and its feature service installer. For example, RPG runtime domain/contracts/services compile through the RPG runtime module while RPG UI presenters compile through `Modules/Rpg/UI`; tabletop board/turn rules compile through the Tabletop runtime module while tabletop scene-facing views compile through `Modules/Tabletop/UI`; queued action execution lives under `Modules/Actions/Runtime`; and the RPG narrative editor lives under `Modules/Rpg/Editor/Tools`. Cross-feature display helpers live under `Presentation`, such as generic HUD orientation code in `Presentation/HUD/UI`, settings panels in `Presentation/HUD/Settings`, and camera trigger zones in `Presentation/Camera/Zones`. Route-specific mode orchestration and scene loading belong under `Glue/SceneFlow`.

## Current pawn animation architecture

Pawn animation is data-driven and Unity-authored:

1. `PawnDefinition` points to presentation and animation assets.
2. `PawnPresentationProfile` declares whether the pawn is 2D, 2.5D, or rigged 3D.
3. `PawnAnimationProfile` maps gameplay signals to Animator behavior.
4. `ActorAnimationDriver` applies those mappings at runtime after `PawnRoot` receives the participant-owned pawn setup.
5. movement, combat, and traversal systems emit shared animation signals instead of owning Animator logic directly.

For participant-spawned pawns, profile fields belong on `PawnDefinition`; prefab components should carry the Unity-native objects they own, such as the visual `Animator`. Component profile fields are for direct scene actors or advanced overrides.

## Recommended reading

- `Docs/CURRENT_STATE_AUDIT.md` for current health, risks, Hygiene baseline, and verification posture.
- `Docs/ARCHITECTURE_BLUEPRINT.md` for runtime ownership, folderbase, and system boundaries.
- `Docs/Authoring/START_HERE.md` for the first human setup path.
- `Docs/Authoring/AUTHORING_BLUEPRINT.md` for Authoring Window behavior.
- `Docs/Authoring/AUTHORING_MODEL.md` for asset/profile/runtime relationships.
- `Docs/Authoring/CANONICAL_SETUP.md` for the technical setup contract.
- `Docs/FEATURE_DEVELOPMENT_ROADMAP.md` for current expansion priorities.
