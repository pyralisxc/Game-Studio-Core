# NeonBlack Gameplay

This folder contains the active gameplay framework for Neon Black Hub.

The current source of truth is a shared gameplay stack built around:

- `GameplaySessionBootstrap`
- `GameplayLifetimeScope`
- authored `SessionDefinition`, `ParticipantDefinition`, `PawnDefinition`, and `GameModeDefinition`
- participant-owned pawns through `PawnRoot`
- participant-owned input through `ParticipantDefinition.inputProfile`
- graph-inferred participant topology for solo local, local join, networked, and hybrid routes
- pawn camera targets through `PawnCameraTarget`
- scene-owned Cinemachine routing through `CameraRigProfile.focusMode` and reflected camera recipe values
- explicit pawn sibling components for physical identity
- direct module-owned components and profiles for optional gameplay capabilities
- PYS contract metadata on gameplay scripts that own setup meaning

Core motto:

- Unity owns engine behavior.
- NeonBlack Gameplay owns gameplay meaning.
- Reflection discovers structure.
- Dependency analysis discovers setup relationships.
- Validators witness local semantic readiness.
- The graph compiles understanding.
- PYS Authoring observes gameplay contract evidence from its own package lane.

## Supported pawn presentation targets

NeonBlack Gameplay supports three official presentation modes:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

Rigged 3D support is Animator-driven and intended for both `Generic` and `Humanoid` rigs.

## Runtime rule of thumb

- Siblings describe what a pawn **is**: root, motor, movement, presentation, input receiver, collider/rigidbody/controller, and core physical identity.
- Features describe what a pawn **can do**: interaction, collectible handling, traversal extras, combat styles, feedback, status, and route-specific capabilities.
- Participants describe who is **driving**: human, AI, network authority, seat, team, or non-pawn actor.
- Participant topology describes how control enters the session: solo auto-start, Unity `PlayerInputManager` local join, network authority, or hybrid local/networked join.
- Camera profiles describe what is **watched** and which saved Cinemachine recipe values should apply. Cinemachine owns composition, damping, offsets, lens behavior, and blend mechanics.
- Scenes own scene-scale direction: bootstrap, lifetime scope, camera rig, spawners, scene services, and proof-specific objects.
- Gameplay scripts expose setup evidence through gameplay contracts; PYS Authoring reads that evidence from its own package.
- Gameplay state that advances over time uses `GameplayTickBehaviour` and `GameplayTickContext` in the owning domain. Presentation, HUD effects, camera smoothing, and local input polling may use Unity `Update`/`LateUpdate` directly when they only display or collect state.

## Verification rule of thumb

- Manual Unity play proofs test gameplay feel and whether the authored route actually works.
- Automated tests protect data transfer, ownership, routing, PYS contract consumption, and refactor seams.
- PYS Authoring JSON exports can be used as external diagnostics for contract evidence.

Do not keep broad optional domain matrices or documentation audits in the active suite. The default gate should catch broken seams without pretending to replace manual proof play.

## Layout

- `Core/`: stable contracts plus tiny shared type vocabulary such as movement modes, presentation lanes, animation signals, action value language, and narrow optional sinks/publishers; runtime contracts are grouped by seam family such as combat, movement, interaction, session, feedback, time, and scene
- `Data/`: ScriptableObject definitions, profiles, config assets such as `GameConfig`, and data-backed handoff contracts such as participant and interaction dispatch context; authored combat data lives here, not in the Combat runtime namespace
- `Editor/`: tactical custom inspectors for local gameplay fields
- `Glue/`: bootstrap, lifetime scope, session services, participant services, input routing, participant spawning, route composition, and service-registration wiring that makes authored modules run
- `Modules/`: reusable gameplay capability families such as character, combat, traversal, hazards, enemies, encounters, environment, interaction/collectibles, feedback, scoring, tabletop, settings, input, spawning, and RPG; broad modules should expose child folders for their real subsystems instead of keeping feature pieces at the module root
- `Networking/`: ownership, authority, and backend-facing runtime contracts
- `Presentation/`: cross-feature visual and camera infrastructure
- `Tests/`: package-level validation infrastructure
- `Docs/`: setup and architecture notes

Core is intentionally not an implementation home. Authored input setup is owned by `InputProfile`, gameplay config assets live under `Data/Config`, runtime session context lives under `Glue/Lifetime`, and optional feature domains live under `Modules`. The root `NeonBlack.Gameplay` assembly is a compatibility facade over `NeonBlack.Gameplay.Glue`; new composition code should reference or live in Glue directly instead of expanding the facade.

Core contracts should stay neutral and compile-time focused. For example, `IDamageNumberSink` and `IActorFeedbackPublisher` are shared optional feedback seams, but `DamageNumber`, `DamageNumberSpawner`, `ActorFeedbackComponent`, and feedback event payloads belong to the Feedback module. `IActorAnimationController`, `IBillboardFacingController`, `IActorCombatMovementState`, `IActorCombatMovementInfluence`, `IActorCombatRequestReceiver`, `ActorCombatCommand`, `IActorCombatResultReceiver`, `ActorCombatResult`, `IActorGuardController`, `IActorGuardInputReceiver2D`, `IActorHazardImpactTarget`, `IActorInteractionInputReceiver2D`, `IActorInteractionRequestReceiver`, `IActorMotionStateReader`, `IActorMovementInputReceiver2D`, and `IEncounterSpawnSource` are tiny runtime seams that let player input, enemy AI, combat modules, motors, hazards, scoring, posture systems, interaction, encounters, spawning, and presentation communicate without importing concrete opposite-lane behavior. Data-backed presentation seams such as `IVisualFlashPlayer`, `ICameraRigProfileSwitcher`, and `IPawnAnimationProfileReceiver` live under `Data.Presentation` because they operate on authored presentation profiles/entries while Presentation owns the concrete Unity components. Combat definitions such as `WeaponData`, `CombatSequenceDefinition`, projectile definitions, and status effects are authored data under `Data.Definitions.Combat`; Combat runtime scripts consume them instead of owning their namespace.

Participant-facing pawn handoff contracts such as `IPawnParticipantInitializer`, `IPawnParticipantStateReader`, `IPawnRuntimeServicesReceiver`, `PawnRuntimeServicesContext`, `IPawnInputModule`, and `IPawnCombatModule` live under `Data.Participants` because they express authored participant/profile ownership rather than Character behavior. Interaction handler contracts that need participant context live under `Data.Interactions`, so Traversal, RPG, and Interaction-owned components can handle interactions without importing the concrete Interaction module. The Input module consumes those Data contracts and Core command seams; it should not import Character, Combat, or Interaction just to push move, jump, dash, attack, guard, or interact requests. The Combat module consumes Data combat profiles and Core combat/movement seams; it should not import Character just to read facing, airborne, or action-lock state from a pawn motor. When Character movement needs combat timing or movement multipliers, it reads `IActorCombatMovementInfluence` rather than importing Combat.

3D traversal is also seam-owned instead of Character-owned. `FrameInput` lives in `Core.Types.Input` as neutral per-frame command data. `IPawnTraversalModule`, `IPawnTraversalMovementController`, and `IActorTraversalFeature` live under `Data.Participants` because they are pawn/profile handoff surfaces. Traversal may name `Motor3D` or `Pawn3DMovementComponent` in authoring setup strings, but its runtime assembly should not reference the Character module.

Enemy AI follows the same command seam as participant input. Enemy decisions issue `ActorCombatCommand` through `IActorCombatRequestReceiver`; enemy reaction reads health, knockback, feedback, hit pause, and camera shake through Core contracts and sinks. Enemy-owned inspectors should edit detection, movement, patrol, targeting, and enemy feature profiles only. Combat-owned enemy attack setup belongs on `EnemyCombatModule` and its Combat editor, not inside `EnemyAIEditor`.

Module folders own lane-specific implementation, scene-facing presenters, and feature-specific editor tooling when the logic is not part of the universal authoring spine. Module code should be covered by feature-owned runtime, UI, and editor asmdefs instead of falling into the aggregate root assembly. A module's runtime asmdef should sit high enough to cover runtime-owned behavior without pulling in feature UI/view code that depends back on authored `Data`; feature UI/view asmdefs may sit beside runtime to keep those cycles explicit and acyclic. Nested `Editor` asmdefs own editor-only tools. Lane folders use `Sprite2D`, `Billboard2_5D`, and `Rigged3D` instead of shorthand folder names. Character owns pawn identity, movement, and pawn-facing behavior contracts; Combat owns pawn combat behavior, hitbox/damage/projectile/block/weapon modules, and combat action state. Feature service installers live under `Glue/ServiceRegistration/FeatureInstallers` because VContainer registration is composition, not module behavior. For example, RPG value/domain models and definition-facing contracts live under `Data/Rpg`, RPG runtime services live under `Modules/Rpg/Runtime`, RPG service registration lives under Glue, and RPG UI presenters compile through `Modules/Rpg/UI`. Tabletop board/turn value rules live under `Data/Tabletop/Rules`, while tabletop runtime action resolvers, action queue implementation, selection bridges, and turn-order service seams live under `Modules/Tabletop/Runtime`; tabletop scene-facing views compile through `Modules/Tabletop/UI`. Core keeps the shared action contracts and value language under `Core/Types/Actions`; feature modules own concrete queue/resolver behavior. The RPG narrative editor lives under `Modules/Rpg/Editor/Tools`. Cross-feature display helpers live under `Presentation`, such as generic HUD orientation code in `Presentation/HUD/UI`, settings panels in `Presentation/HUD/Settings`, and camera trigger zones in `Presentation/Camera/Zones`. Ordinary module runtime asmdefs should not reference `Presentation`; modules consume Core/Data seams while Presentation implements the concrete Unity visual/camera/animation components. Route-specific mode orchestration and scene loading belong under `Glue/SceneFlow`.

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
- `Packages/com.pys.authoring/Docs` for PYS Authoring behavior, projections, exports, and target-project integration.
- `Docs/FEATURE_DEVELOPMENT_ROADMAP.md` for current expansion priorities.
