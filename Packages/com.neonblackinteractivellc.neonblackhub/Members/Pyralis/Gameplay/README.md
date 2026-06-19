# NeonBlack Gameplay

This folder contains the active Pyralis gameplay framework for Neon Black Hub.

The current source of truth is a shared gameplay stack built around:

- `GameplaySessionBootstrap`
- `PyralisGameplayLifetimeScope`
- authored `SessionDefinition`, `ParticipantDefinition`, `PawnDefinition`, and `GameModeDefinition`
- participant-owned pawns through `PawnRoot`
- participant-owned input through `ParticipantDefinition.inputProfile`
- pawn camera targets through `PawnCameraTarget`
- scene-owned Cinemachine routing through `CameraRigProfile.focusMode`
- explicit pawn sibling components for physical identity
- `ActorFeatureHost` for optional/swappable gameplay capabilities
- contracts, reflection, dependency-tree discovery, validators, and the resolved authoring graph

## Supported pawn presentation targets

NeonBlack Gameplay now supports three official presentation modes:

- `Sprite2D`
- `Billboard2_5D`
- `Rigged3D`

Rigged 3D support is Animator-driven and intended for both `Generic` and `Humanoid` rigs.

## Runtime rule of thumb

- Siblings describe what a pawn **is**: root, motor, movement, presentation, input receiver, collider/rigidbody/controller, and core physical identity.
- Features describe what a pawn **can do**: interaction, traversal extras, combat styles, pickups, feedback, status, and route-specific capabilities.
- Participants describe who is **driving**: human, AI, network authority, seat, team, or non-pawn actor.
- Camera profiles describe what is **watched**: participant pawn socket, participant group, playfield center, explicit scene anchor, or manual Cinemachine.
- Scenes own scene-scale direction: bootstrap, lifetime scope, camera rig, spawners, scene services, and proof-specific objects.
- Authoring reads contracts/reflection/dependency evidence and projects the resolved graph; it does not create hidden presets or duplicate setup truth.

## Verification rule of thumb

- Manual Unity play proofs test gameplay feel and whether the authored route actually works.
- Automated tests protect data transfer, ownership, routing, contracts, graph projections, exports, and refactor seams.
- Authoring JSON exports show what the system currently understands and where the next cleanup pressure lives.

Do not keep broad optional domain matrices or documentation audits in the active suite. The default gate should catch broken seams without pretending to replace manual proof play.

## Layout

- `Core/`: engine spine, shared runtime services, runtime contracts, and shared contract metadata consumed by `Editor/Authoring`
- `Data/`: ScriptableObject definitions and profiles
- `Editor/`: shared authoring helpers and custom inspectors
- `Features/`: optional runtime systems, gameplay modules, feature-owned composition, feature UI, and feature-specific editor tools
- `Networking/`: ownership, authority, and backend-facing runtime contracts
- `Presentation/`: cross-feature visual and camera infrastructure
- `Tests/`: package-level validation infrastructure
- `Docs/`: setup and architecture notes

Feature folders should own their local composition and tooling when the logic is not part of the universal authoring spine. For example, RPG service registration lives under `Features/Rpg/Runtime/Composition`, while the RPG narrative editor lives under `Features/Rpg/Editor/Tools`.

## Current pawn animation architecture

Pawn animation is data-driven and Unity-authored:

1. `PawnDefinition` points to presentation and animation assets.
2. `PawnPresentationProfile` declares whether the pawn is 2D, 2.5D, or rigged 3D.
3. `PawnAnimationProfile` maps gameplay signals to Animator behavior.
4. `ActorAnimationDriver` applies those mappings at runtime after `PawnRoot` receives the participant-owned pawn setup.
5. movement, combat, and traversal systems emit shared animation signals instead of owning Animator logic directly.

For participant-spawned pawns, profile fields belong on `PawnDefinition`; prefab components should carry the Unity-native objects they own, such as the visual `Animator`. Component profile fields are for direct scene actors or advanced overrides.

## Recommended reading

- `Docs/CURRENT_STATE_AUDIT.md` for current health, risks, and cleanup focus.
- `Docs/ARCHITECTURE_BLUEPRINT.md` for runtime ownership, folderbase, and system boundaries.
- `Docs/Authoring/START_HERE.md` for the first human setup path.
- `Docs/Authoring/AUTHORING_BLUEPRINT.md` for Authoring Window behavior.
- `Docs/Authoring/AUTHORING_MODEL.md` for asset/profile/runtime relationships.
- `Docs/Authoring/CANONICAL_SETUP.md` for the technical setup contract.
- `Docs/FEATURE_DEVELOPMENT_ROADMAP.md` for current expansion priorities.
