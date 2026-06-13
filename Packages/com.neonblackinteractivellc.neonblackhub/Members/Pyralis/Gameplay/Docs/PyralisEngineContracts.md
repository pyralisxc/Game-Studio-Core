# Pyralis Engine Capabilities & Authoring Contracts

Generated on: 2026-06-08 22:04

This document is auto-generated from `[AuthoringContract]` metadata and describes feature contract truth. Setup/reference truth comes from the dependency tree; compiled readiness/proof truth comes from the authoring setup graph.

## Capability: Setup
> Foundational scene and bootstrap configuration.

### Actor Feature Host
- **Summary**: Inspector Add Component path for installing custom feature modules on an actor.

### Actor Feature Host
- **Summary**: Feature contract for  module setup and lane compatibility.

### Enemy Feature Profile
- **Summary**: Project-window creation path for enemy feature setup.

### Enemy Feature Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Feature Module Definition
- **Summary**: Authoring container for attachable runtime logic, used to extend Pawns or Game Modes with modular functionality.

### Feature Module Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Feature Module Runtime
- **Summary**: The runtime entry point for custom game features and modular logic.

### Feature Module Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Game Manager
- **Summary**: Central game orchestrator; coordinates scoring, difficulty, spawning, and high-level game flow.
- **Axioms**: `Dimensions2D`

### Game Manager
- **Summary**: Feature contract for  module setup and lane compatibility.

### Game Setup Profile
- **Summary**: Project-window creation path for the runtime pattern setup profile.

### Game Setup Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Gameplay Session Bootstrap
- **Summary**: Primary entry point for gameplay sessions; orchestrates participant spawn, camera setup, and core services.

### Gameplay Session Bootstrap
- **Summary**: Feature contract for  module setup and lane compatibility.

### Participant Input Router
- **Summary**: Routes physical input device events to the correct participant and their pawn.

### Participant Input Router
- **Summary**: Feature contract for  module setup and lane compatibility.

### Participant Spawn Service
- **Summary**: Orchestrates participant spawning at designated spawn points during session initialization.

### Participant Spawn Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Playfield Profile
- **Summary**: Project-window creation path for movement space, bounds, wrap, and arena-depth rules.

### Playfield Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pyralis Gameplay Lifetime Scope
- **Summary**: Inspector Add Component path for the visible Pyralis runtime composition scope.

### Pyralis Gameplay Lifetime Scope
- **Summary**: Feature contract for  module setup and lane compatibility.

### Runtime Pattern Definition
- **Summary**: Defines a reusable recipe for runtime system expectations and presentation requirements.

### Runtime Pattern Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Scene Loader
- **Summary**: Manages global scene transitions and screen fades.

### Scene Loader
- **Summary**: Feature contract for  module setup and lane compatibility.

### Spawner
- **Summary**: General-purpose utility for spawning prefabs and sprites with optional patrol movement.

### Spawner
- **Summary**: Feature contract for  module setup and lane compatibility.

### Time Manager
- **Summary**: Manages global time scale effects such as hit-pause and game freeze.
- **Axioms**: `Realtime`

### Time Manager
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Session
> High-level game rules and participant definitions.

### Action Definition
- **Summary**: Project-window creation path for one selectable command or resolver-backed action.

### Action Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Action Queue Service
- **Summary**: Processes action execution requests and resolves them via registered resolvers.
- **Axioms**: `Realtime, TurnBased`

### Action Queue Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Action Resolver
- **Summary**: Resolves gameplay actions like movement, attacks, or logic triggers.

### Action Resolver
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Feature Host
- **Summary**: Inspector Add Component path for installing custom feature modules on an actor.

### Actor Feature Host
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Gameplay Action Receiver
- **Summary**: Receives and handles gameplay actions on an actor.

### Actor Gameplay Action Receiver
- **Summary**: Feature contract for  module setup and lane compatibility.

### Enemy Spawner
- **Summary**: Inspector Add Component path for scene-authored enemy spawning.

### Enemy Spawner
- **Summary**: Feature contract for  module setup and lane compatibility.

### Feature Module Runtime
- **Summary**: The runtime entry point for custom game features and modular logic.

### Feature Module Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Game Manager
- **Summary**: Central game orchestrator; coordinates scoring, difficulty, spawning, and high-level game flow.
- **Axioms**: `Dimensions2D`

### Game Manager
- **Summary**: Feature contract for  module setup and lane compatibility.

### Game Mode Definition
- **Summary**: Defines the specific game rules, required features, and scene setup for a gameplay session.

### Game Mode Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Game Setup Profile
- **Summary**: Project-window creation path for the runtime pattern setup profile.

### Game Setup Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Hazard Spawner
- **Summary**: Orchestrates pooling and spawning of 2D hazards based on difficulty pacing.
- **Axioms**: `Dimensions2D`

### Hazard Spawner
- **Summary**: Feature contract for  module setup and lane compatibility.

### Hub Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Participant Definition
- **Summary**: Defines a player or NPC seat within a session, including their default pawn and input configuration.

### Participant Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Participant Score Service
- **Summary**: Canonical scoring service; tracks participant scores, session points, survival time, and high-score persistence.

### Participant Score Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Participant Spawn Service
- **Summary**: Orchestrates participant spawning at designated spawn points during session initialization.

### Participant Spawn Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Rpg Open Zone Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Session Definition
- **Summary**: Top-level session configuration for local and networked gameplay setup.

### Session Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Session State Service
- **Summary**: Global service for tracking and broadcasting high-level gameplay session states (Playing, Paused, lobby).

### Session State Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Skill Tree Service
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Input
> Human and AI control schemes and event routing.

### Actor Gameplay Action Receiver
- **Summary**: Receives and handles gameplay actions on an actor.

### Actor Gameplay Action Receiver
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Interaction Feature
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Interaction Feature Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Input Config
- **Summary**: Defines participant-specific input overrides (e.g., custom controller bindings).

### Input Config
- **Summary**: Feature contract for  module setup and lane compatibility.

### Input Profile
- **Summary**: Maps high-level gameplay actions (Move, Jump, Interact) to Unity Input System actions.

### Input Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Motor2 D Input Adapter
- **Summary**: Inspector Add Component path for the supported neutral 2D pawn input adapter.

### Motor2 D Input Adapter
- **Summary**: Feature contract for  module setup and lane compatibility.

### Participant Input Router
- **Summary**: Routes physical input device events to the correct participant and their pawn.

### Participant Input Router
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: UI
> User interface, menus, and HUD presentation.

### Actor Feedback Profile
- **Summary**: Project-window creation path for actor feedback and route-readable reaction polish.

### Actor Feedback Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Participant Feedback Hud Presenter
- **Summary**: Displays participant-specific feedback (combos, status, scores) in the HUD.

### Participant Feedback Hud Presenter
- **Summary**: Feature contract for  module setup and lane compatibility.

### Participant Health Hud Binder
- **Summary**: Binds participant health state to UI elements like labels and progress bars.

### Participant Health Hud Binder
- **Summary**: Feature contract for  module setup and lane compatibility.

### Settings Profile
- **Summary**: Project-window creation path for settings and menu defaults.

### Settings Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### U I Manager
- **Summary**: Manages gameplay UI: HUD, game over screen, and settings navigation.

### U I Manager
- **Summary**: Feature contract for  module setup and lane compatibility.

### U I Orientation Handler
- **Summary**: Maintains UI layout integrity across portrait and landscape device orientations.

### U I Orientation Handler
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Movement
> Pawn locomotion, physics, and pathfinding.

### Actor Traversal Feature
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `Rigged3D`

### Character Motor State
- **Summary**: Exposes the internal state of a character motor (velocity, ground state, direction).
- **Axioms**: `Dimensions2D, Dimensions3D`

### Character Motor State
- **Summary**: Feature contract for  module setup and lane compatibility.

### Enemy A I
- **Summary**: Inspector Add Component path for enemy AI behavior.

### Enemy A I
- **Summary**: Feature contract for  module setup and lane compatibility.

### Motor2 D
- **Summary**: Canonical 2D pawn motor; coordinates movement, animations, and reactions.
- **Axioms**: `Dimensions2D`

### Motor2 D
- **Summary**: Feature contract for  module setup and lane compatibility.

### Motor2 D Input Adapter
- **Summary**: Inspector Add Component path for the supported neutral 2D pawn input adapter.

### Motor2 D Input Adapter
- **Summary**: Feature contract for  module setup and lane compatibility.

### Motor3 D
- **Summary**: Canonical 3D pawn motor; coordinates input, movement, traversal, and presentation.
- **Axioms**: `Dimensions3D`

### Motor3 D
- **Summary**: Feature contract for  module setup and lane compatibility.

### Movement Module
- **Summary**: Calculates actor translation and velocity based on input and physical rules.
- **Axioms**: `Dimensions2D, Dimensions3D`

### Movement Module
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pawn Definition
- **Summary**: Core definition for a controllable entity, linking its prefab to movement, combat, and animation profiles.

### Pawn Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pawn Movement Profile
- **Summary**: Project-window creation path for pawn movement feel, speed, acceleration, dash, and jump tuning.

### Pawn Movement Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pawn Root
- **Summary**: The root coordinator for participant-owned pawns. Handles profile application and feature installation.

### Pawn Root
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pawn Traversal Feature Runtime3 D
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `Rigged3D`

### Pawn2 D Movement Component
- **Summary**: Tunable 2D movement module supporting top-down and platformer (side-view) modes.

### Pawn2 D Movement Component
- **Summary**: Feature contract for  module setup and lane compatibility.

### Top Down Hop Feature Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`

---

## Capability: Combat
> Health, damage, weapons, and reaction systems.

### Actor Combat Reaction Feature Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Combat Reaction Profile
- **Summary**: Project-window creation path for combat reaction behavior.

### Actor Combat Reaction Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Guard Feature
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`

### Actor Health State
- **Summary**: Provides the current health and life state of an actor.

### Actor Health State
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Status Effect Feature Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Status Effect Receiver
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`

### Combat Action Definition
- **Summary**: Project-window creation path for one combat action.

### Combat Action Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Damage Zone2 D
- **Summary**: Inspector Add Component path for a 2D hazard or damage trigger.

### Damage Zone2 D
- **Summary**: Feature contract for  module setup and lane compatibility.

### Enemy A I
- **Summary**: Inspector Add Component path for enemy AI behavior.

### Enemy A I
- **Summary**: Feature contract for  module setup and lane compatibility.

### Enemy Ambient Feature Profile
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `Rigged3D`

### Enemy Ambient Feature Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Enemy Feature Profile
- **Summary**: Project-window creation path for enemy feature setup.

### Enemy Feature Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Enemy Reaction Feature Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Enemy Reaction State
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `Rigged3D`

### Enemy Spawner
- **Summary**: Inspector Add Component path for scene-authored enemy spawning.

### Enemy Spawner
- **Summary**: Feature contract for  module setup and lane compatibility.

### Fire Mode Definition
- **Summary**: Project-window creation path for firing cadence, burst, and spread behavior.

### Fire Mode Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Hazard
- **Summary**: Primary controller for 2D hazards, handling movement, targeting, and impact sequences.
- **Axioms**: `Dimensions2D`

### Hazard
- **Summary**: Feature contract for  module setup and lane compatibility.

### Hazard Feedback Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Hazard Spawner
- **Summary**: Orchestrates pooling and spawning of 2D hazards based on difficulty pacing.
- **Axioms**: `Dimensions2D`

### Hazard Spawner
- **Summary**: Feature contract for  module setup and lane compatibility.

### Health Component
- **Summary**: Inspector Add Component path for any damageable player, enemy, prop, or custom object.

### Health Component
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pawn Combat Behaviour
- **Summary**: Primary pawn combat controller; handles sequences, combos, hit detection, and damage modification.
- **Axioms**: `Realtime`

### Pawn Combat Behaviour
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pawn Combat Module
- **Summary**: Handles pawn-specific combat logic, weapon state, and targeting.

### Pawn Combat Module
- **Summary**: Feature contract for  module setup and lane compatibility.

### Projectile Definition
- **Summary**: Project-window creation path for projectile behavior.

### Projectile Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Animation
> Visual state machines and skeletal deformation.

### Actor Animation Controller
- **Summary**: Controls actor animation states, transitions, and parameter syncing.

### Actor Animation Controller
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Animation Driver
- **Summary**: Inspector Add Component path for actor animation and presentation mapping.

### Actor Animation Driver
- **Summary**: Feature contract for  module setup and lane compatibility.

### Combat Action Definition
- **Summary**: Project-window creation path for one combat action.

### Combat Action Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Enemy Ambient Feature Profile
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `Rigged3D`

### Enemy Ambient Feature Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pawn Presentation Profile
- **Summary**: Project-window creation path for pawn presentation lane and visual setup choices.

### Pawn Presentation Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pawn2 D Presentation Component
- **Summary**: Inspector Add Component path for the 2D pawn visual and presentation module.

### Pawn2 D Presentation Component
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: VFX
> Particle systems, post-processing, and shader effects.

### Actor Feedback Feature Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Feedback Profile
- **Summary**: Project-window creation path for actor feedback and route-readable reaction polish.

### Actor Feedback Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Feedback Publisher
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`

### Camera Shake
- **Summary**: Canonical camera shake service for gameplay impact feedback.
- **Axioms**: `Dimensions2D, Dimensions3D`

### Camera Shake
- **Summary**: Feature contract for  module setup and lane compatibility.

### Hazard Feedback Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Participant Feedback Service
- **Summary**: Global service for streaming feedback events (scoring, health) to participants for UI/SFX triggers.

### Participant Feedback Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pawn Presentation Profile
- **Summary**: Project-window creation path for pawn presentation lane and visual setup choices.

### Pawn Presentation Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pawn2 D Presentation Component
- **Summary**: Inspector Add Component path for the 2D pawn visual and presentation module.

### Pawn2 D Presentation Component
- **Summary**: Feature contract for  module setup and lane compatibility.

### Sprite Flasher
- **Summary**: Coroutine-driven color flash effects on SpriteRenderers.
- **Axioms**: `SpriteVisuals, BillboardVisuals`

### Sprite Flasher
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Tabletop
> Board game logic, piece management, and move policies.

### Board Definition
- **Summary**: Project-window creation path for tabletop board layouts and starting pieces.

### Board Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Board Move Policy Definition
- **Summary**: Project-window creation path for tabletop legal-move policy.

### Board Move Policy Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Board Piece Definition
- **Summary**: Project-window creation path for tabletop board pieces.

### Board Piece Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Phase Definition
- **Summary**: Project-window creation path for turn phase rules.

### Phase Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Tabletop Board Grid Presenter
- **Summary**: Inspector Add Component path for a board presenter that can build selectable tabletop spaces.

### Tabletop Board Grid Presenter
- **Summary**: Feature contract for  module setup and lane compatibility.

### Tabletop Board Selection Bridge
- **Summary**: Bridge between a board presenter and queued board actions (e.g. piece selection and movement).

### Tabletop Board Selection Bridge
- **Summary**: Feature contract for  module setup and lane compatibility.

### Tabletop Turn Status Presenter
- **Summary**: LIGHTWEIGHT UI binding that shows which tabletop seat acts next.

### Tabletop Turn Status Presenter
- **Summary**: Feature contract for  module setup and lane compatibility.

### Turn Order Definition
- **Summary**: Project-window creation path for tabletop and turn/menu action order.

### Turn Order Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Turn Order Service
- **Summary**: Manages turn sequence and active participant in turn-based games.
- **Axioms**: `TurnBased`

### Turn Order Service
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Grid
> Coordinate systems, cell properties, and spatial queries.

### Board Definition
- **Summary**: Project-window creation path for tabletop board layouts and starting pieces.

### Board Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Board Move Policy Definition
- **Summary**: Project-window creation path for tabletop legal-move policy.

### Board Move Policy Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Board Piece Definition
- **Summary**: Project-window creation path for tabletop board pieces.

### Board Piece Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Tabletop Board Grid Presenter
- **Summary**: Inspector Add Component path for a board presenter that can build selectable tabletop spaces.

### Tabletop Board Grid Presenter
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Turn Based
> Phase management, action queues, and initiative.

### Action Definition
- **Summary**: Project-window creation path for one selectable command or resolver-backed action.

### Action Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Phase Definition
- **Summary**: Project-window creation path for turn phase rules.

### Phase Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Turn Order Definition
- **Summary**: Project-window creation path for tabletop and turn/menu action order.

### Turn Order Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Turn Order Service
- **Summary**: Manages turn sequence and active participant in turn-based games.
- **Axioms**: `TurnBased`

### Turn Order Service
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Stats
> Attributes, modifiers, and character progression systems.

### Actor Status Effect Receiver
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`

### Progression Curve Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Progression Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Skill Tree Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Skill Tree Service
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Inventory
> Item storage, equipment, and resource management.

### Actor Interaction Handler
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Sprite2D`

### Actor Pickup Collector Feature2 D
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Sprite2D`

### Actor Pickup Collector Feature3 D
- **Summary**: Feature contract for  module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `Rigged3D`

### Equipment Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Equipment Slot Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Inventory Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Item Catalog Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Item Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pickup Feature Profile
- **Summary**: Project-window creation path for pickup feature setup.

### Pickup Feature Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Rpg Vendor Panel Presenter
- **Summary**: Feature contract for  module setup and lane compatibility.

### Vendor Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Vendor Service
- **Summary**: Reflective authoring contract discovered for VendorService.

### Vendor Service
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Dialogue
> Narrative flow, branching conversations, and event nodes.

### Dialogue Graph Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Dialogue Service
- **Summary**: Reflective authoring contract discovered for DialogueService.

### Dialogue Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Hub Interaction Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Npc Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Quest Definition
- **Summary**: Feature contract for  module setup and lane compatibility.

### Quest Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Rpg Dialogue Panel Presenter
- **Summary**: Feature contract for  module setup and lane compatibility.

### Rpg Quest Board Panel Presenter
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Puzzle
> Logic gates, triggers, and state-based world interactions.

### Actor Interaction Feature
- **Summary**: Feature contract for  module setup and lane compatibility.

### Actor Interaction Feature Runtime
- **Summary**: Feature contract for  module setup and lane compatibility.

### Hub Interaction Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Pickup Feature Profile
- **Summary**: Project-window creation path for pickup feature setup.

### Pickup Feature Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Camera
> Framing, following, and world containment boundaries.

### Camera Bounds Provider
- **Summary**: Provides world-space boundaries for camera framing and containment.
- **Axioms**: `BoundedSpace`

### Camera Bounds Provider
- **Summary**: Feature contract for  module setup and lane compatibility.

### Camera Rig Profile
- **Summary**: Project-window creation path for camera framing, follow, zoom, and 2D orthographic route choices.

### Camera Rig Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

### Cinemachine Camera Rig Controller
- **Summary**: Canonical shared camera rig controller. Handles participant framing, 2D/3D bounds, and profile switching.

### Cinemachine Camera Rig Controller
- **Summary**: Feature contract for  module setup and lane compatibility.

### Playfield Profile
- **Summary**: Project-window creation path for movement space, bounds, wrap, and arena-depth rules.

### Playfield Profile
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Environment
> World geometry, lighting, and static decoration.

### Rpg Open Zone Service
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## Capability: Networking
> State synchronization, authority, and multiplayer connectivity.

### Local Participant Authority Service
- **Summary**: Provides the local-only authority model for participants.

### Local Participant Authority Service
- **Summary**: Feature contract for  module setup and lane compatibility.

### Local Session Ownership Service
- **Summary**: Provides the local-only ownership model for game sessions, used in offline modes.

### Local Session Ownership Service
- **Summary**: Feature contract for  module setup and lane compatibility.

---

## General & Legacy Contracts
> Contracts that have not yet been migrated to the typed Spine Capability system or represent general engine utilities.

### 1P Pawn Movement Proof
- **Summary**: Run one local pawn-backed movement proof before adding combat, HUD, enemies, scoring, or networking.
- **Lanes**: `Sprite2D`

### 2.5D Lane / Arena Action
- **Summary**: A lane, depth, or arena route where actors use 2.5D presentation and route-specific movement, camera, combat, or enemy behavior.
- **Lanes**: `Billboard2_5D`

### 2D Pawn Movement
- **Summary**: A participant-owned Sprite2D pawn that can spawn into the scene and move through input.
- **Lanes**: `Sprite2D`

### 2D Side-View Action
- **Summary**: A Sprite2D route where authored pawns move on a side-view playfield with gravity, ledges, platforms, or jumping.
- **Lanes**: `Sprite2D`

### 2D Top-Down / Free Movement
- **Summary**: A Sprite2D route where pawns, cursors, pickups, projectiles, scoring, HUD, or hazards act on a top-down 2D plane without requiring side-view gravity.
- **Lanes**: `Sprite2D`, `CameraCursor`

### 3D / 2.5D Pawn Movement
- **Summary**: A participant-owned 3D CharacterController pawn that can move on X/Z, present as a billboard sprite or rigged model, and receive gameplay input.
- **Lanes**: `Billboard2_5D`, `Rigged3D`

### 3D Space Action
- **Summary**: A Rigged3D route where actors, cameras, interaction, enemies, projectiles, or traversal operate in 3D world space.
- **Lanes**: `Rigged3D`

### Action Selection Proof
- **Summary**: Run one selected command before expanding menus, cards, ability lists, animation polish, or AI.
- **Lanes**: `UiMenu`, `TabletopNoPawn`, `CameraCursor`, `Sprite2D`, `Rigged3D`

### Add HUD / UI Surface
- **Summary**: Create or assign visible UI surfaces for prompts, feedback, health, score, menus, or route panels.

### Add Runtime Patterns
- **Summary**: Assign runtime pattern definitions that describe the current route.

### Assign Camera Bounds Service
- **Summary**: Connect camera bounds to the active setup when 2D framing, spawners, hazards, pickups, or world limits rely on them.

### Assign Camera Rig
- **Summary**: Create or assign a camera rig that can frame the first proof.

### Assign Default Game Mode
- **Summary**: Create or assign the game-rules asset for the session.

### Assign Default Participants
- **Summary**: Create or assign participant definitions for players, seats, factions, or command owners.

### Assign Gameplay State Service
- **Summary**: Assign a scene or composition service when gameplay state is route-owned.

### Assign Input Profile
- **Summary**: Assign input mapping when participant input drives pawn movement or actions.

### Assign Participant Pawn
- **Summary**: Assign a PawnDefinition and prefab only when the selected route is pawn-backed.

### Assign Player Input Manager
- **Summary**: Use PlayerInputManager only when local join or explicit multi-player input ownership is part of the proof.

### Assign Playfield Profile
- **Summary**: Create or assign authored playfield bounds and lane rules when the route needs them.

### Assign Projectile Launcher Or Hitbox Source
- **Summary**: Create or assign a hitbox, projectile launcher, enemy attack, weapon mount, trap, turret, or encounter source.

### Assign Score Service
- **Summary**: Create or assign a concrete session score service when scoring is part of the route.

### Assign Session Definition
- **Summary**: Create or assign the session asset that owns game mode and default participants.

### Assign Setup Profile
- **Summary**: Create or assign the setup profile that lists runtime capability ingredients.

### Assign Spawn Points
- **Summary**: Place spawn Transforms so pawn-backed participants can enter the scene predictably.

### Assign Tabletop Selection Surface
- **Summary**: Create or assign the board, card, cursor, or action-selection surface that makes one no-pawn proof selectable in Play Mode.

### Board / Action Selection
- **Summary**: Board grid presenter, selection bridge, card hand, action/menu presenter, UI button, cursor bridge, or collider/raycast target evidence.
- **Lanes**: `TabletopNoPawn`, `UiMenu`, `CameraCursor`, `Sprite2D`, `Rigged3D`

### Board Card Action Proof
- **Summary**: Run one rules-backed tabletop selection before adding card UX, AI turns, campaign flow, or networking.
- **Lanes**: `TabletopNoPawn`, `UiMenu`, `CameraCursor`

### BoardPieceDefinition Visual Prefab
- **Summary**: Assign creator-owned art to each board piece definition so the proof uses imported content without hardcoding a game.

### Camera / Bounds
- **Summary**: Camera root, Cinemachine controller, physical target camera, camera profile, or bounds evidence.
- **Lanes**: `CameraCursor`, `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`

### Camera Cursor World Proof
- **Summary**: Run one camera, cursor, bounds, or world-surface proof before adding multi-target framing or cinematic polish.
- **Lanes**: `CameraCursor`, `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`

### Camera Follow And Bounds
- **Summary**: A camera or cursor control surface that makes the current route visible and keeps 2D proofs framed.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `CameraCursor`

### Camera Or Cursor Command
- **Summary**: A non-pawn or mixed route where the player first controls a camera, cursor, selector, board surface, or command UI.
- **Lanes**: `CameraCursor`, `TabletopNoPawn`, `UiMenu`

### CameraRigProfile Framing Fields
- **Summary**: Customize orthographic, zoom, damping, follow, and shake values to make the first route proof readable.

### CinemachineCameraRigController Camera Fields
- **Summary**: Assign camera rig, playfield, target camera, and Cinemachine references on the scene camera root.

### Combat Attack Proof
- **Summary**: A smallest attack, hit, shot, damage, or reaction path for a pawn, NPC/enemy, trap, turret, or command source.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `CameraCursor`

### Custom Object Effect Proof
- **Summary**: Run one custom object, feature, trigger, pickup, hazard, turret, trap, or service effect before treating it as a full system.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`

### Custom Object Or Feature Route
- **Summary**: Scene objects, feature modules, hazards, pickups, triggers, turrets, traps, or custom systems that are not the primary pawn.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`

### Enable Scoring Route
- **Summary**: Declare score or objective ownership before UI or services try to display it.

### Environment / Playfield
- **Summary**: World, board, arena, backdrop, collider, bounds, zone, spawn, or selectable playfield evidence.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`

### FeatureModuleDefinition Profile Runtime And Network Fields
- **Summary**: Assign a feature profile, optional runtime prefab, and network role when a custom feature should install predictable runtime behavior.

### First Scene Defaults
- **Summary**: Use first-scene defaults so core services and scene injection are predictable while authoring.

### GameModeDefinition Board And Turn Rules
- **Summary**: Assign board and turn-rule assets when the route is tabletop, board, card, tactics, or turn/menu driven.

### GameModeDefinition Camera And Playfield Profiles
- **Summary**: Assign camera and playfield profiles when route visibility, bounds, orthographic framing, or arena space matters.

### GameModeDefinition Required Feature Modules
- **Summary**: Assign feature modules when the route depends on custom objects, hazards, pickups, NPC behaviors, UI feedback, or route-specific runtime modules.

### GameModeDefinition Setup Profile
- **Summary**: Assign the setup profile that lists runtime capability ingredients for the route.

### Gameplay Root
- **Summary**: Keep the scene setup anchored on one visible GameplaySessionBootstrap object.

### GameplaySessionBootstrap Session Definition
- **Summary**: Assign the session asset on the scene bootstrap so the active scene knows which route to start.

### GameSetupProfile Runtime Patterns
- **Summary**: Assign runtime pattern definitions that describe the supported route capabilities.

### Generated Content Proof
- **Summary**: Generate one inspectable output before making generated content required for progression.
- **Lanes**: `Sprite2D`, `TabletopNoPawn`, `CameraCursor`

### Hybrid / Custom Project
- **Summary**: A project-wide intent that combines ingredients across world, actor, action, UI, rules, or runtime lanes without accepting a named preset.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`, `UiMenu`, `CameraCursor`

### InputProfile Gameplay Action Names
- **Summary**: Customize action-name fields to match the project's Input Action Asset without renaming the whole input asset for Pyralis.

### Interaction Or Action Selection
- **Summary**: One selectable command surface such as an interact prompt, menu command, board space, card, cursor target, or pawn action.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`, `UiMenu`, `CameraCursor`

### Network Ownership Proof
- **Summary**: Confirm the local proof first, then prove one host/client ownership path before expanding replication.
- **Lanes**: `Networked`, `Sprite2D`, `Rigged3D`

### Networking Authority Route
- **Summary**: Host/client/server, participant ownership, authority, network spawn, replicated state, and network-ready route proof.
- **Lanes**: `Networked`, `Sprite2D`, `Rigged3D`

### NPC / Enemy Actor Setup
- **Summary**: An authored enemy or NPC actor that can appear in the scene, detect or patrol, take damage, attack, and participate in an encounter.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`

### NPC And Enemy Actor Route
- **Summary**: Pawn or actor routes driven by AI, encounter, ambient, dialogue, vendor, quest, or enemy combat definitions.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`

### NPC Enemy Behavior Proof
- **Summary**: Run one NPC or enemy behavior proof before building encounter waves, boss phases, vendors, or broad AI systems.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`

### ParticipantDefinition Default Pawn
- **Summary**: Assign a PawnDefinition when this participant should spawn or control a pawn-backed body.

### Pawn Actor Route
- **Summary**: Participant-backed player, NPC, or simulated actor routes that spawn or control a pawn body.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`

### Pawn Brawler
- **Summary**: A pawn-backed action route where movement and close-range attacks are the first playable loop.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`

### PawnDefinition Movement And Presentation Profiles
- **Summary**: Assign movement and presentation profiles so pawn feel and visuals stay designer-customizable.

### PawnDefinition Pawn Prefab
- **Summary**: Assign the prefab that contains PawnRoot and lane-specific runtime components.

### Pickups / Hazards / Enemies
- **Summary**: Pickup, hazard, enemy, encounter, arena, spawner, or custom feature object evidence.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`

### Runtime Service Ownership
- **Summary**: Keep runtime services owned by GameplaySessionBootstrap and PyralisGameplayLifetimeScope instead of hidden singleton lookups.

### Scene And Prefab Readiness
- **Summary**: Block Play Mode proof guidance until required scene objects, prefab modules, and inspector handoffs are clear.

### Scoring / Objectives
- **Summary**: Score, objective, timer, resource, result, win/loss service, or visible output evidence.
- **Lanes**: `UiMenu`, `Sprite2D`, `Rigged3D`, `TabletopNoPawn`

### Select Gameplay Session Bootstrap
- **Summary**: Choose the scene object that anchors the active Pyralis setup.

### SessionDefinition Default Game Mode
- **Summary**: Assign the game-rules asset that controls the playable loop for the session.

### Tabletop Board Card Route
- **Summary**: Board seats, board pieces, card hands, faction surfaces, turn order, phases, terminal conditions, and board action selection.
- **Lanes**: `TabletopNoPawn`, `UiMenu`, `CameraCursor`

### Tabletop Runtime Contract
- **Summary**: Use board, piece, move-policy, turn-order, and action data without requiring pawn fields.

### Tabletop, Board, Or Card Project
- **Summary**: A no-pawn or mixed route where board state, card hands, action selection, turns, seats, factions, or rules resolution are the project center.
- **Lanes**: `TabletopNoPawn`, `UiMenu`, `CameraCursor`, `Sprite2D`

### TabletopBoardGridPresenter Board Fields
- **Summary**: Assign board, move policy, turn order, selection bridge, and board-space/piece prefabs for the smallest tabletop proof.

### TabletopTurnStatusPresenter Fields
- **Summary**: Assign the board presenter and TextMeshPro label that should show the active local seat during a tabletop proof.

### Test Reflective Contract
- **Summary**: Feature contract for Test Goal module setup and lane compatibility.

### Test Reflective Contract (Test Goal)
- **Summary**: This is a test contract to verify the reflective authoring UI.

### Tune Camera Framing
- **Summary**: Customize camera framing and bounds for the selected route.

### Tune Movement And Input Feel
- **Summary**: Customize movement profile, CharacterController or Rigidbody feel, and input names so the proof feels intentional.

### Tune Pawn Visuals And Collision
- **Summary**: Customize sprite/model, collider or CharacterController fit, pivot, sorting, billboard/rigged presentation, and visible pawn presentation.

### UI / HUD / Menus
- **Summary**: Canvas, EventSystem, HUD presenter, menu presenter, prompt, card hand, action buttons, or score/feedback panel evidence.
- **Lanes**: `UiMenu`, `TabletopNoPawn`, `Sprite2D`, `Rigged3D`

### UI / Menu First Project
- **Summary**: A route where menu commands, HUD, action selection, settings, results, dialogue, or UI-presented game state are the first authored surface.
- **Lanes**: `UiMenu`, `TabletopNoPawn`, `CameraCursor`

### UI And Scoring Feedback
- **Summary**: Visible route state such as score, health, prompt text, feedback, objective state, or menu/action labels.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`, `UiMenu`

### UI HUD Menu Proof
- **Summary**: Run one UI, HUD, prompt, score, health, feedback, or menu event before building full navigation or result screens.
- **Lanes**: `UiMenu`, `TabletopNoPawn`, `Sprite2D`, `Rigged3D`

### UI HUD Or Menu Route
- **Summary**: Canvas, HUD, menu, prompt, health, score, feedback, inventory, dialogue, card hand, or route-selection surfaces.
- **Lanes**: `UiMenu`, `TabletopNoPawn`, `Sprite2D`, `Rigged3D`

### Visible Lifetime Scope
- **Summary**: Show the VContainer composition root on the gameplay object before Play Mode.

### World Camera And Scene Surface Route
- **Summary**: Camera, bounds, scene service, world trigger, board view, cursor view, or environmental route surfaces.
- **Lanes**: `CameraCursor`, `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`

---

