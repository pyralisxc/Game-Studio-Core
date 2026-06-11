# Pyralis Engine Capabilities & Authoring Contracts

Generated on: 2026-06-10 10:29

This document is auto-generated from the [AuthoringContract] attributes and the central Authoring Registries. It serves as the singular source of truth for the engine's capabilities.

## Capability: Setup
> Foundational scene and bootstrap configuration.

### Actor Feature Host
- **Summary**: Feature contract for Setup, Session module setup and lane compatibility.

### Actor Feature Host (Setup, Session)
- **Summary**: Installs runtime feature prefabs declared by actor definitions or profiles.
- **Expert Advice**: The ActorFeatureHost is the central manager for dynamic actor capabilities. It handles dependency injection (VContainer) for newly instantiated feature prefabs.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/composition)

### Feature Module Definition
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Feature Module Definition (Setup)
- **Summary**: Authoring container for attachable runtime logic, used to extend Pawns or Game Modes with modular functionality.

### Feature Module Runtime
- **Summary**: Feature contract for Setup, Session module setup and lane compatibility.

### Feature Module Runtime (Setup, Session)
- **Summary**: The runtime entry point for custom game features and modular logic.
- **Expert Advice**: Implement this interface on any component that needs to participate in the feature-host lifecycle. It provides access to shared services via the InitializationContext.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/composition)

### Game Config
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Game Config (Setup)
- **Summary**: The master wiring point for the game project; defines the entry session and service prefabs.
- **Expert Advice**: Use service prefabs only if you need custom logic for core services like Time or Scene Loading.

### Game Manager
- **Summary**: Feature contract for Setup, Session module setup and lane compatibility.

### Game Manager (Setup, Session)
- **Summary**: Central game orchestrator; coordinates scoring, difficulty, spawning, and high-level game flow.
- **Expert Advice**: The GameManager is the 2D arcade orchestrator. It manages the lifecycle from Splash to Game Over. Use the inspector events to hook up UI and audio transitions.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/gameflow)
- **Axioms**: `Dimensions2D`

### Game Setup Profile
- **Summary**: Feature contract for Setup, Session module setup and lane compatibility.

### Game Setup Profile (Setup, Session)
- **Summary**: Project-window creation path for the runtime pattern setup profile.
- **Expert Advice**: GameSetupProfile is your 'Recipe' for a specific route. Use it to select high-level capability families and the specific RuntimePatternDefinitions that implement them.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/game-setup)

### Gameplay Platform Context
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Gameplay Platform Context (Setup)
- **Summary**: Current platform runtime composition context for the active gameplay session.
- **Expert Advice**: Use TryGetCurrent to safely resolve the platform context without hard singleton dependencies.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/platform-context)

### Gameplay Session Bootstrap
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Gameplay Session Bootstrap (Setup)
- **Summary**: Primary entry point for gameplay sessions; orchestrates participant spawn, camera setup, and core services.
- **Expert Advice**: The Bootstrap is the heart of the Pyralis session. Ensure your SessionDefinition has at least one Participant defined. The Bootstrap auto-creates required services (Roster, Spawn, State) if they are missing from its children.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/bootstrap)

### Level Data
- **Summary**: Feature contract for Setup, Environment module setup and lane compatibility.

### Level Data (Setup, Environment)
- **Summary**: Data container for level configuration, including display names and scene references.
- **Expert Advice**: LevelData assets are primarily used by the LevelRegistry to build the world-select UI. Ensure the SceneName exactly matches the entry in File -> Build Settings.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/navigation)

### Level Registry
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Level Registry (Setup)
- **Summary**: Ordered list of all playable worlds. Referenced by menu and session flow.
- **Expert Advice**: The Registry is the source of truth for the level selector UI. Use it to centralize world definitions across the project.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/navigation)

### Level Session
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Level Session (Setup)
- **Summary**: Lightweight static cross-scene contract for level selection metadata.
- **Expert Advice**: Set ChosenSceneName before triggering a SceneManager.LoadScene call.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/level-session)

### Loading Screen Controller
- **Summary**: Feature contract for Setup, UI module setup and lane compatibility.

### Loading Screen Controller (Setup, UI)
- **Summary**: LoadingScreenController reads SceneFader.PendingScene and shows optional progress UI.
- **Expert Advice**: Do not open the loading scene directly unless falling back to MainMenu is acceptable. Do not put gameplay-only startup logic here; this scene should remain transitional.

### Main Menu Manager
- **Summary**: Feature contract for Setup, UI module setup and lane compatibility.

### Main Menu Manager (Setup, UI)
- **Summary**: Main menu controller for a panel-driven gameplay menu scene; handles play/load/exit navigation.
- **Expert Advice**: Do not leave New Game, Settings, or Exit buttons empty unless disabled. Navigation services cannot load an unnamed scene. Ensure the Scene Navigator Source is present in the menu scene.

### Networked Participant Spawn Service (Setup, Session)
- **Summary**: Orchestrates participant spawning at designated spawn points during session initialization.
- **Expert Advice**: Spawns pawns based on ParticipantDefinitions. If your game doesn't use physical pawns (e.g., pure UI or Card games), you can leave Spawn Points empty or disable 'Spawn On Register'.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/participants)

### Participant Input Router
- **Summary**: Feature contract for Setup, Input module setup and lane compatibility.

### Participant Input Router (Setup, Input)
- **Summary**: Routes physical input device events to the correct participant and their pawn.
- **Expert Advice**: The Input Router watches for new 'PlayerJoined' events and automatically registers them into the Roster. It ensures that InputProfiles are correctly applied to the newly created PlayerInput instances.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/input)

### Participant Spawn Service
- **Summary**: Feature contract for Setup, Session module setup and lane compatibility.

### Participant Spawn Service (Setup, Session)
- **Summary**: Orchestrates participant spawning at designated spawn points during session initialization.
- **Expert Advice**: Spawns pawns based on ParticipantDefinitions. If your game doesn't use physical pawns (e.g., pure UI or Card games), you can leave Spawn Points empty or disable 'Spawn On Register'.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/participants)

### Platform Service Registry
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Platform Service Registry (Setup)
- **Summary**: Small runtime service registry used by the platform composition layer during the VContainer transition.
- **Expert Advice**: Legacy service locator. New features should prefer direct VContainer injection via IObjectResolver.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/service-registry)

### Playfield Profile
- **Summary**: Feature contract for Setup, Camera module setup and lane compatibility.

### Playfield Profile (Setup, Camera)
- **Summary**: Project-window creation path for movement space, bounds, wrap, and arena-depth rules.

### Pyralis Gameplay Lifetime Scope
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Pyralis Gameplay Lifetime Scope (Setup)
- **Summary**: Inspector Add Component path for the visible Pyralis runtime composition scope.

### Pyralis Proof Validator
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Pyralis Proof Validator (Setup)
- **Summary**: Automated scene-readiness checker for Proof of Concept scenes.
- **Expert Advice**: Add this component to any proof scene to verify the bootstrap and core services are healthy.

### Runtime Pattern Definition
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Runtime Pattern Definition (Setup)
- **Summary**: Defines a reusable recipe for runtime system expectations and presentation requirements.

### Scene Fader
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Scene Fader (Setup)
- **Summary**: Persistent ISceneNavigator that fades to black, optionally routes through the loading screen, and restores time scale.
- **Expert Advice**: Do not load multiple SceneFaders; Awake keeps one active transition service and destroys duplicates. Do not use FadeToSceneViaLoader unless SceneNames.LoadingScreen is in Build Settings.

### Scene Guard
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Scene Guard (Setup)
- **Summary**: Lightweight scene-transition cleanup helper that destroys duplicate active EventSystems and AudioListeners at Awake.
- **Expert Advice**: Keep one active EventSystem and one active AudioListener as the expected final state. Use it as cleanup support, not as a substitute for clean scene ownership.

### Scene Loader
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Scene Loader (Setup)
- **Summary**: Handles all scene transitions with a fade by creating its own fade canvas at runtime.
- **Expert Advice**: Inject ISceneNavigator to trigger transitions. Keep Fade Duration non-negative; zero gives an instant cut with the generated fade canvas.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/navigation)

### Scene Navigator
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Scene Navigator (Setup)
- **Summary**: Lightweight static helper for simple scene loads. Prefers ISceneNavigator when available.
- **Expert Advice**: SceneNavigator is a static bypass for logic scripts. For production flows with UI faders, prefer injecting ISceneNavigator (SceneLoader) into your systems.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/navigation)

### Settings Menu
- **Summary**: Feature contract for Setup, UI module setup and lane compatibility.

### Settings Menu (Setup, UI)
- **Summary**: Drives the 3D main-menu settings panel: volume sliders, fullscreen state, and resolution selection.
- **Expert Advice**: Do not leave every control empty; the panel will open but cannot change settings. Wire the resolution dropdown On Value Changed event only if another script needs to observe it; this script adds its own listener on enable.

### Settings Screen
- **Summary**: Feature contract for Setup, UI module setup and lane compatibility.

### Settings Screen (Setup, UI)
- **Summary**: Swaps between a main menu page and a settings page, forwards slider/toggle values to a settings service, and can pause gameplay.
- **Expert Advice**: Do not assign child controls as page roots; page swapping should hide whole panels. Sliders will not save unless Settings Source is assigned.

### Spawn Tracker
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Spawn Tracker (Setup)
- **Summary**: Tracks destruction of spawned objects for Spawner accounting.
- **Expert Advice**: Do not add this component manually to scene objects unless you are implementing a custom spawn tracker logic.

### Spawner
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Spawner (Setup)
- **Summary**: General-purpose utility for spawning prefabs and sprites with optional patrol movement.
- **Expert Advice**: Do not enable Patrol with zero distance. Ensure at least one prefab or sprite is assigned.

### Splash Screen Controller
- **Summary**: Feature contract for Setup, UI module setup and lane compatibility.

### Splash Screen Controller (Setup, UI)
- **Summary**: Drives the optional intro scene, plays a video or static fallback, preloads the next scene, and fades out.
- **Expert Advice**: Do not leave Next Scene Name blank; async loading will fail. Do not assign a video display without a video clip unless a static image is intentionally shown.

### Test Reflective Contract
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Test Reflective Contract (Setup)
- **Summary**: This is a test contract to verify the reflective authoring UI.

### Time Manager
- **Summary**: Feature contract for Setup module setup and lane compatibility.

### Time Manager (Setup)
- **Summary**: Manages global time scale effects such as hit-pause and game freeze.
- **Expert Advice**: Use TimeManager to create dramatic pauses during combat or UI events. It manages the global Unity Time.timeScale safely and resets it on disable.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/time)
- **Axioms**: `Realtime`

---

## Capability: Session
> High-level session orchestration and network authority.

### Action Definition
- **Summary**: Feature contract for Session, TurnBased module setup and lane compatibility.

### Action Definition (Session, TurnBased)
- **Summary**: Project-window creation path for one selectable command or resolver-backed action.

### Action Queue Service
- **Summary**: Feature contract for Session module setup and lane compatibility.

### Action Queue Service (Session)
- **Summary**: Processes action execution requests and resolves them via registered resolvers.
- **Expert Advice**: Register custom IActionResolvers to extend the engine's action vocabulary. The queue handles FIFO execution and validation.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/actions)
- **Axioms**: `Realtime, TurnBased`

### Action Resolver
- **Summary**: Feature contract for Session module setup and lane compatibility.

### Action Resolver (Session)
- **Summary**: Resolves gameplay actions like movement, attacks, or logic triggers.
- **Expert Advice**: Implement this interface to handle specific ActionId strings within the ActionQueueService.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/action-resolvers)

### Actor Feature Host
- **Summary**: Feature contract for Setup, Session module setup and lane compatibility.

### Actor Feature Host (Setup, Session)
- **Summary**: Installs runtime feature prefabs declared by actor definitions or profiles.
- **Expert Advice**: The ActorFeatureHost is the central manager for dynamic actor capabilities. It handles dependency injection (VContainer) for newly instantiated feature prefabs.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/composition)

### Actor Gameplay Action Receiver
- **Summary**: Feature contract for Session, Input module setup and lane compatibility.

### Actor Gameplay Action Receiver (Session, Input)
- **Summary**: Receives and handles gameplay actions on an actor.

### Collectible Spawner2 D
- **Summary**: Feature contract for Session module setup and lane compatibility.

### Collectible Spawner2 D (Session)
- **Summary**: Manages the collectible object pool and spawn logic for 2D sessions.
- **Expert Advice**: Use a pool size that covers the maximum expected collectibles. Higher minimum on-screen counts ensure the player always has something to collect.
- **Axioms**: `Dimensions2D`

### Enemy Spawner
- **Summary**: Feature contract for Session, Combat module setup and lane compatibility.

### Enemy Spawner (Session, Combat)
- **Summary**: Inspector Add Component path for scene-authored enemy spawning.

### Feature Module Runtime
- **Summary**: Feature contract for Setup, Session module setup and lane compatibility.

### Feature Module Runtime (Setup, Session)
- **Summary**: The runtime entry point for custom game features and modular logic.
- **Expert Advice**: Implement this interface on any component that needs to participate in the feature-host lifecycle. It provides access to shared services via the InitializationContext.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/composition)

### Game Manager
- **Summary**: Feature contract for Setup, Session module setup and lane compatibility.

### Game Manager (Setup, Session)
- **Summary**: Central game orchestrator; coordinates scoring, difficulty, spawning, and high-level game flow.
- **Expert Advice**: The GameManager is the 2D arcade orchestrator. It manages the lifecycle from Splash to Game Over. Use the inspector events to hook up UI and audio transitions.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/gameflow)
- **Axioms**: `Dimensions2D`

### Game Setup Profile
- **Summary**: Feature contract for Setup, Session module setup and lane compatibility.

### Game Setup Profile (Setup, Session)
- **Summary**: Project-window creation path for the runtime pattern setup profile.
- **Expert Advice**: GameSetupProfile is your 'Recipe' for a specific route. Use it to select high-level capability families and the specific RuntimePatternDefinitions that implement them.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/game-setup)

### Hazard Spawner
- **Summary**: Feature contract for Session, Combat module setup and lane compatibility.

### Hazard Spawner (Session, Combat)
- **Summary**: Orchestrates pooling and spawning of 2D hazards based on difficulty pacing.
- **Axioms**: `Dimensions2D`

### Hub Definition
- **Summary**: Feature contract for Session module setup and lane compatibility.

### Interaction Feature Profile
- **Summary**: Feature contract for Session, Puzzle module setup and lane compatibility.

### Interaction Feature Profile (Session, Puzzle)
- **Summary**: Defines how an actor interacts with world objects.
- **Expert Advice**: Use interactionCooldown to prevent rapid-fire interaction spamming.

### Leaderboard Manager
- **Summary**: Feature contract for Session module setup and lane compatibility.

### Leaderboard Manager (Session)
- **Summary**: No-op bridge for leaderboard services. Replace with a package-specific manager for online scores.
- **Expert Advice**: Use this bridge to keep code compiling without backend dependencies. Ensure the Leaderboard ID matches your online configuration.

### Networked Participant Roster Service (Session)
- **Summary**: Authoritative local roster of participants. Bridges compatibility for single-player lookups.
- **Expert Advice**: Source of truth for all active participants. Bridges Unity's PlayerInput system to the Pyralis 'Participant' model. Use it to iterate over players or find specific client authority.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/participants)

### Networked Participant Spawn Service (Setup, Session)
- **Summary**: Orchestrates participant spawning at designated spawn points during session initialization.
- **Expert Advice**: Spawns pawns based on ParticipantDefinitions. If your game doesn't use physical pawns (e.g., pure UI or Card games), you can leave Spawn Points empty or disable 'Spawn On Register'.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/participants)

### Networked Session State Service (Session)
- **Summary**: Global service for tracking high-level gameplay session states (Playing, Paused, Lobby).
- **Expert Advice**: SessionStateService tracks the high-level flow (Lobby -> Gameplay). Inject IGameplayStateReader to listen for phase changes in your UI or Game Logic scripts.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/session)

### Participant Roster Service
- **Summary**: Feature contract for Session module setup and lane compatibility.

### Participant Roster Service (Session)
- **Summary**: Authoritative local roster of participants. Bridges compatibility for single-player lookups.
- **Expert Advice**: Source of truth for all active participants. Bridges Unity's PlayerInput system to the Pyralis 'Participant' model. Use it to iterate over players or find specific client authority.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/participants)

### Participant Spawn Service
- **Summary**: Feature contract for Setup, Session module setup and lane compatibility.

### Participant Spawn Service (Setup, Session)
- **Summary**: Orchestrates participant spawning at designated spawn points during session initialization.
- **Expert Advice**: Spawns pawns based on ParticipantDefinitions. If your game doesn't use physical pawns (e.g., pure UI or Card games), you can leave Spawn Points empty or disable 'Spawn On Register'.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/participants)

### Pawn Root
- **Summary**: Feature contract for Session, Movement module setup and lane compatibility.

### Pawn Root (Session, Movement)
- **Summary**: The root coordinator for participant-owned pawns. Handles profile application and feature installation.
- **Expert Advice**: The PawnRoot is the composition root. It reads the PawnDefinition and installs requested feature modules (Combat, Traversal, etc.) at runtime. Pawn prefabs should not carry their own scene cameras.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/pawns)

### Rpg Open Zone Service
- **Summary**: Feature contract for Session, Environment module setup and lane compatibility.

### Session Definition
- **Summary**: Feature contract for Session module setup and lane compatibility.

### Session Definition (Session)
- **Summary**: Root configuration for a gameplay session. Defines the boundary of your game world and network authority.
- **Expert Advice**: SessionDefinition is your session's 'Law'. For local-only prototypes, keep 'Local First' checked to bypass networking overhead. Assign a Default Input Profile here to save time on per-pawn setup.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/session)

### Session State Service
- **Summary**: Feature contract for Session module setup and lane compatibility.

### Session State Service (Session)
- **Summary**: Global service for tracking high-level gameplay session states (Playing, Paused, Lobby).
- **Expert Advice**: SessionStateService tracks the high-level flow (Lobby -> Gameplay). Inject IGameplayStateReader to listen for phase changes in your UI or Game Logic scripts.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/session)

### Stillness Bonus2 D
- **Summary**: Feature contract for Session module setup and lane compatibility.

### Stillness Bonus2 D (Session)
- **Summary**: Awards points to the player for remaining stationary.
- **Expert Advice**: Set the stillness threshold high enough to ignore micro-movement or drift. Ensure the score award source is correctly wired.

---

## Capability: Input
> Human and AI control schemes and event routing.

### Actor Gameplay Action Receiver
- **Summary**: Feature contract for Session, Input module setup and lane compatibility.

### Actor Gameplay Action Receiver (Session, Input)
- **Summary**: Receives and handles gameplay actions on an actor.

### Actor Guard Input Bridge2 D
- **Summary**: Feature contract for Input, Combat module setup and lane compatibility.

### Actor Guard Input Bridge2 D (Input, Combat)
- **Summary**: Forwards 2D guard input into an installed Actor Guard feature on ActorFeatureHost.
- **Expert Advice**: Bridge only forwards input; it does not block damage by itself. Ensure the Guard feature is installed in PawnDefinition.

### Actor Interaction Feature
- **Summary**: Feature contract for Input, Puzzle module setup and lane compatibility.

### Actor Interaction Feature Runtime
- **Summary**: Feature contract for Input, Puzzle module setup and lane compatibility.

### Actor Interaction Input Bridge2 D
- **Summary**: Feature contract for Input, Puzzle module setup and lane compatibility.

### Actor Interaction Input Bridge2 D (Input, Puzzle)
- **Summary**: Forwards interact input into an installed Actor Interaction feature on ActorFeatureHost.
- **Expert Advice**: Bridge only forwards input. Ensure the Interaction feature is installed in PawnDefinition.

### Input Config
- **Summary**: Feature contract for Input module setup and lane compatibility.

### Input Config (Input)
- **Summary**: Defines participant-specific input overrides (e.g., custom controller bindings).
- **Expert Advice**: Assign an InputActionAsset that defines the gameplay controls for this lane. Use unique InputConfigs for players vs AI if specific binding overrides are needed.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/input)

### Input Profile
- **Summary**: Feature contract for Input module setup and lane compatibility.

### Input Profile (Input)
- **Summary**: Maps high-level gameplay actions (Move, Jump, Interact) to Unity Input System actions.
- **Expert Advice**: InputProfile decouples gameplay logic from physical keys. Use the action role to map common verbs (Jump, Dash) across different control schemes.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/input)

### Motor2 D Input Adapter
- **Summary**: Feature contract for Input, Movement module setup and lane compatibility.

### Motor2 D Input Adapter (Input, Movement)
- **Summary**: Primary input module for 2D characters. Translates participant input into Motor2D movement.
- **Axioms**: `Dimensions2D`

### Participant Input Router
- **Summary**: Feature contract for Setup, Input module setup and lane compatibility.

### Participant Input Router (Setup, Input)
- **Summary**: Routes physical input device events to the correct participant and their pawn.
- **Expert Advice**: The Input Router watches for new 'PlayerJoined' events and automatically registers them into the Roster. It ensures that InputProfiles are correctly applied to the newly created PlayerInput instances.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/input)

### Pawn3 D Input Module
- **Summary**: Feature contract for Input module setup and lane compatibility.

### Pawn3 D Input Module (Input)
- **Summary**: Translates Unity Input System actions into Pawn-readable FrameInput data.
- **Expert Advice**: Converts hardware signals into FrameInput. It uses the InputProfile to find action names. Ensure your InputActionAsset has the 'Player' map (or as defined in your profile).
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/input)
- **Axioms**: `Dimensions3D, Realtime`

---

## Capability: UI
> User interface, menus, and HUD presentation.

### Actor Feedback Profile
- **Summary**: Feature contract for UI, VFX module setup and lane compatibility.

### Actor Feedback Profile (UI, VFX)
- **Summary**: Configures which gameplay events (damage, death, score) trigger visual feedback or HUD notifications.
- **Expert Advice**: Use these toggles to silence feedback for specific actor archetypes (e.g., destructible props vs. bosses).
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/visuals)

### Hazard Feedback Profile
- **Summary**: Feature contract for UI, VFX module setup and lane compatibility.

### Hazard Feedback Profile (UI, VFX)
- **Summary**: Defines the visual feedback (flashes, popups) for hazard activation and explosion.
- **Expert Advice**: Use popupFontSize to ensure warnings are visible at the game's camera distance.

### Leaderboard Screen
- **Summary**: Feature contract for UI module setup and lane compatibility.

### Leaderboard Screen (UI)
- **Summary**: UI screen for displaying top scores from a leaderboard service.
- **Expert Advice**: Ensure the Row Prefab has exactly three TMP labels in order: Rank, Name, Score.

### Loading Screen Controller
- **Summary**: Feature contract for Setup, UI module setup and lane compatibility.

### Loading Screen Controller (Setup, UI)
- **Summary**: LoadingScreenController reads SceneFader.PendingScene and shows optional progress UI.
- **Expert Advice**: Do not open the loading scene directly unless falling back to MainMenu is acceptable. Do not put gameplay-only startup logic here; this scene should remain transitional.

### Main Menu Manager
- **Summary**: Feature contract for Setup, UI module setup and lane compatibility.

### Main Menu Manager (Setup, UI)
- **Summary**: Main menu controller for a panel-driven gameplay menu scene; handles play/load/exit navigation.
- **Expert Advice**: Do not leave New Game, Settings, or Exit buttons empty unless disabled. Navigation services cannot load an unnamed scene. Ensure the Scene Navigator Source is present in the menu scene.

### Participant Feedback Hud Presenter
- **Summary**: Feature contract for UI module setup and lane compatibility.

### Participant Feedback Hud Presenter (UI)
- **Summary**: Displays participant-specific feedback (combos, status, scores) in the HUD.

### Participant Health Hud Binder
- **Summary**: Feature contract for UI module setup and lane compatibility.

### Participant Health Hud Binder (UI)
- **Summary**: Binds participant health state to UI elements like labels and progress bars.

### Participant Timed Text Panel
- **Summary**: Feature contract for UI module setup and lane compatibility.

### Participant Timed Text Panel (UI)
- **Summary**: Displays temporary text messages (e.g., 'Level Up', 'K.O.') on the HUD.

### Settings Manager
- **Summary**: Feature contract for UI, Audio module setup and lane compatibility.

### Settings Manager (UI, Audio)
- **Summary**: Manages global audio volume levels and mixer integration. Connects settings profiles to the active Unity AudioMixer.
- **Expert Advice**: Ensure your AudioMixer has exposed parameters named 'MusicVolume' and 'SFXVolume' (case sensitive) for the manager to drive. This component persists across scenes if placed on a DontDestroyOnLoad root.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/settings)

### Settings Menu
- **Summary**: Feature contract for Setup, UI module setup and lane compatibility.

### Settings Menu (Setup, UI)
- **Summary**: Drives the 3D main-menu settings panel: volume sliders, fullscreen state, and resolution selection.
- **Expert Advice**: Do not leave every control empty; the panel will open but cannot change settings. Wire the resolution dropdown On Value Changed event only if another script needs to observe it; this script adds its own listener on enable.

### Settings Profile
- **Summary**: Feature contract for UI module setup and lane compatibility.

### Settings Profile (UI)
- **Summary**: Project-window creation path for settings and menu defaults.

### Settings Screen
- **Summary**: Feature contract for Setup, UI module setup and lane compatibility.

### Settings Screen (Setup, UI)
- **Summary**: Swaps between a main menu page and a settings page, forwards slider/toggle values to a settings service, and can pause gameplay.
- **Expert Advice**: Do not assign child controls as page roots; page swapping should hide whole panels. Sliders will not save unless Settings Source is assigned.

### Splash Screen Controller
- **Summary**: Feature contract for Setup, UI module setup and lane compatibility.

### Splash Screen Controller (Setup, UI)
- **Summary**: Drives the optional intro scene, plays a video or static fallback, preloads the next scene, and fades out.
- **Expert Advice**: Do not leave Next Scene Name blank; async loading will fail. Do not assign a video display without a video clip unless a static image is intentionally shown.

### U I Manager
- **Summary**: Feature contract for UI module setup and lane compatibility.

### U I Manager (UI)
- **Summary**: Manages gameplay UI: HUD, game over screen, and settings navigation.
- **Expert Advice**: The UIManager is a high-level presentation layer. It listens to the IGameplaySessionFlow to toggle panels based on game state.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/ui)

### U I Orientation Handler
- **Summary**: Feature contract for UI module setup and lane compatibility.

### U I Orientation Handler (UI)
- **Summary**: Maintains UI layout integrity across portrait and landscape device orientations.
- **Expert Advice**: Captures the current RectTransform state. Position your UI for one orientation, hit Capture, then repeat for the other. Use with CanvasScaler in 'Match' mode for best results.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/ui)

---

## Capability: Movement
> General movement archetype and intent definitions.

### Character Motor State
- **Summary**: Feature contract for Movement module setup and lane compatibility.

### Character Motor State (Movement)
- **Summary**: Exposes the internal state of a character motor (velocity, ground state, direction).
- **Expert Advice**: Exposes motor snapshots. Other features (like Combat or UI) should query this for movement state instead of concrete motors to maintain architecture decoupling.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/movement)
- **Axioms**: `Dimensions2D, Dimensions3D`

### Motor2 D Input Adapter
- **Summary**: Feature contract for Input, Movement module setup and lane compatibility.

### Motor2 D Input Adapter (Input, Movement)
- **Summary**: Primary input module for 2D characters. Translates participant input into Motor2D movement.
- **Axioms**: `Dimensions2D`

### Movement Module
- **Summary**: Feature contract for Movement module setup and lane compatibility.

### Movement Module (Movement)
- **Summary**: Calculates actor translation and velocity based on input and physical rules.
- **Expert Advice**: The universal runtime contract for movement. High-level systems (AI/Network) use this to drive the actor without knowing if it's 2D or 3D. Implement this to provide 'Velocity' and 'Grounded' data.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/movement)
- **Axioms**: `Dimensions2D, Dimensions3D`

### Pawn Definition
- **Summary**: Feature contract for Movement, Combat module setup and lane compatibility.

### Pawn Definition (Movement, Combat)
- **Summary**: Core definition for a controllable entity, linking its prefab to movement, combat, and animation profiles.
- **Expert Advice**: PawnDefinition is the glue for your characters. Ensure you assign a PawnPrefab and appropriate profiles for Movement and Combat. Check 'First Proof' by spawning it in a test scene.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/pawn)

### Pawn Movement Profile
- **Summary**: Feature contract for Movement module setup and lane compatibility.

### Pawn Movement Profile (Movement)
- **Summary**: Defines the movement feel, speed, acceleration, and damping for a pawn archetype.
- **Expert Advice**: The movement profile is your 'steering wheel'. It defines the responsiveness and agility of your actor. For 2D games, set 'Use 2D Physics' to enable Rigidbody2D interaction.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/movement)

### Pawn Root
- **Summary**: Feature contract for Session, Movement module setup and lane compatibility.

### Pawn Root (Session, Movement)
- **Summary**: The root coordinator for participant-owned pawns. Handles profile application and feature installation.
- **Expert Advice**: The PawnRoot is the composition root. It reads the PawnDefinition and installs requested feature modules (Combat, Traversal, etc.) at runtime. Pawn prefabs should not carry their own scene cameras.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/pawns)

### Pawn Traversal Profile
- **Summary**: Feature contract for Movement module setup and lane compatibility.

### Pawn Traversal Profile (Movement)
- **Summary**: Defines the jumping, dodging, and climbing capabilities of a pawn.
- **Expert Advice**: Use jumpHeight and gravity to tune the arc of the jump. If 'allowJump' is off, the actor will be grounded unless a separate 'Hop' feature is installed.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/traversal)

### Pawn2 D Movement Component
- **Summary**: Feature contract for Movement module setup and lane compatibility.

### Pawn2 D Movement Component (Movement)
- **Summary**: Tunable 2D movement module supporting top-down and platformer (side-view) modes.
- **Expert Advice**: Top-down route: leave Jump Enabled off and set Gravity Scale to 0. Side-view route: enable Jump, set Ground Layer, and ensure Rigidbody2D 'Collision Detection' is set to Continuous for high-speed dashes.
- **Axioms**: `Dimensions2D`

### Pawn3 D Movement Component
- **Summary**: Feature contract for Movement module setup and lane compatibility.

### Pawn3 D Movement Component (Movement)
- **Summary**: Core 3D movement motor; handles walking, jumping, gravity, and ground detection.
- **Expert Advice**: Movement Component uses CharacterController.Move(). Ensure the Ground Layer mask does not include the 'Player' layer to prevent the pawn from trying to ground itself on its own collider.
- **Axioms**: `Dimensions3D, Realtime`

---

## Capability: Combat
> General combat systems and weapon logic.

### Actor Combat Reaction Feature Runtime
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Actor Combat Reaction Profile
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Actor Combat Reaction Profile (Combat)
- **Summary**: Defines how an actor reacts to combat events (guard, parry, block, shield break).
- **Expert Advice**: Use parryReactionLockDuration to stun the attacker when a parry is successful.

### Actor Guard Feature
- **Summary**: Feature contract for Combat module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`

### Actor Guard Input Bridge2 D
- **Summary**: Feature contract for Input, Combat module setup and lane compatibility.

### Actor Guard Input Bridge2 D (Input, Combat)
- **Summary**: Forwards 2D guard input into an installed Actor Guard feature on ActorFeatureHost.
- **Expert Advice**: Bridge only forwards input; it does not block damage by itself. Ensure the Guard feature is installed in PawnDefinition.

### Actor Status Effect Feature Runtime
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Actor Status Effect Profile
- **Summary**: Feature contract for Combat, Stats module setup and lane compatibility.

### Actor Status Effect Profile (Combat, Stats)
- **Summary**: Defines common status effect vulnerabilities and immunities for an actor.
- **Expert Advice**: Use defaultShieldDamageReduction to scale incoming damage when the actor has an active shield effect.

### Actor Status Effect Receiver
- **Summary**: Feature contract for Combat, Stats module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`

### Battle Manager
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Battle Manager (Combat)
- **Summary**: Manages attack tokens to prevent all enemies from attacking the player simultaneously.
- **Axioms**: `Realtime`

### Combat Action Definition
- **Summary**: Feature contract for Combat, Animation module setup and lane compatibility.

### Combat Action Definition (Combat, Animation)
- **Summary**: Project-window creation path for one combat action.
- **Expert Advice**: Use comboStep to sequence multi-hit attacks. Use cooldownOverride if this move should be slower or faster than the weapon default.

### Combat Sequence Definition
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Combat Sequence Definition (Combat)
- **Summary**: Defines a sequence of combat actions (combos) triggered by a specific input type.
- **Expert Advice**: Use sequences to build multi-hit brawler combos. Each action in the list must correspond to the correct combo step.

### Damage Zone
- **Summary**: Feature contract for Combat, Puzzle module setup and lane compatibility.

### Damage Zone (Combat, Puzzle)
- **Summary**: 3D trigger volume that repeatedly damages overlapping actors.
- **Expert Advice**: Do not set Tick Interval too low. Ensure target actors have a HealthComponent. Use Hazard Impact Profile if you want this hazard to behave identically to others.
- **Axioms**: `Dimensions3D`

### Damage Zone2 D
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Damage Zone2 D (Combat)
- **Summary**: Inspector Add Component path for a 2D hazard or damage trigger.
- **Axioms**: `Dimensions2D`

### Enemy Ambient Feature Profile
- **Summary**: Feature contract for Combat, Animation module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `ThirdPerson3D`

### Enemy Ambient Feature Runtime
- **Summary**: Feature contract for Combat, Animation module setup and lane compatibility.

### Enemy Attack
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Enemy Attack (Combat)
- **Summary**: Defines the selection criteria and execution of a specific AI attack.
- **Expert Advice**: Set Priority higher for 'punish' or 'finisher' moves. Use weight for random selection within the same priority.

### Enemy Combat Profile
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Enemy Combat Profile (Combat)
- **Summary**: Defines how an AI enemy chooses and sequences its attacks.
- **Expert Advice**: Use Sequential mode for boss phases or predictable combos. Use Priority or Weighted for dynamic combat behavior.

### Enemy Feature Profile
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Enemy Feature Profile (Combat)
- **Summary**: The central configuration for an enemy; binds combat and reaction profiles together.
- **Expert Advice**: Use modular profiles to share behaviors across multiple enemy types while keeping the root profile unique per archetype.

### Enemy Reaction Feature Runtime
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Enemy Reaction Profile
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Enemy Reaction Profile (Combat)
- **Summary**: Configures stagger, hit-stun, and death reaction timing for enemies.
- **Expert Advice**: Balance hit stun to prevent 'infinite' combos by the player while still providing satisfying weight to attacks.

### Enemy Reaction State
- **Summary**: Feature contract for Combat module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `ThirdPerson3D`

### Enemy Spawner
- **Summary**: Feature contract for Session, Combat module setup and lane compatibility.

### Enemy Spawner (Session, Combat)
- **Summary**: Inspector Add Component path for scene-authored enemy spawning.

### Fire Mode Definition
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Fire Mode Definition (Combat)
- **Summary**: Project-window creation path for firing cadence, burst, and spread behavior.

### Hazard
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Hazard (Combat)
- **Summary**: Primary controller for 2D hazards, handling movement, targeting, and impact sequences.
- **Expert Advice**: Ensure a Kinematic Rigidbody2D is on the root for explosive hazards. Keep Shadow and Outline renderers on separate child objects.
- **Axioms**: `Dimensions2D`

### Hazard Feedback Runtime
- **Summary**: Feature contract for Combat, VFX module setup and lane compatibility.

### Hazard Impact Profile
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Hazard Impact Profile (Combat)
- **Summary**: Defines the damage, knockback, and status effects applied by a hazard on contact.
- **Expert Advice**: Use destroyCollectiblesOnContact for obstacle hazards that should 'eat' powerups.

### Hazard Preset Library
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Hazard Preset Library (Combat)
- **Summary**: A designer-facing catalogue of hazard presets for quick assignment and lookup.
- **Expert Advice**: Use this to manage a large variety of hazards without cluttering scene references.

### Hazard Spawner
- **Summary**: Feature contract for Session, Combat module setup and lane compatibility.

### Hazard Spawner (Session, Combat)
- **Summary**: Orchestrates pooling and spawning of 2D hazards based on difficulty pacing.
- **Axioms**: `Dimensions2D`

### Pawn Combat Behaviour2 D
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Pawn Combat Behaviour2 D (Combat)
- **Summary**: 2D pawn combat; receives attack input, resolves combos, activates HitBox2D, and fires projectiles.
- **Expert Advice**: For 2D-only combat, prefer PawnCombatBehaviour2D. Do not leave hitbox zone names mismatched with WeaponData fallback zones.
- **Axioms**: `Dimensions2D`

### Pawn Combat Module
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Pawn Combat Module (Combat)
- **Summary**: Handles pawn-specific combat logic, weapon state, and targeting.

### Pawn Combat Profile
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Pawn Combat Profile (Combat)
- **Summary**: Defines the core combat parameters for a pawn archetype.
- **Expert Advice**: Use comboResetTime to control the window for continuing a combo. Assign a WeaponData asset to define the hitboxes and visual effects of the attack.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/combat)

### Pawn Definition
- **Summary**: Feature contract for Movement, Combat module setup and lane compatibility.

### Pawn Definition (Movement, Combat)
- **Summary**: Core definition for a controllable entity, linking its prefab to movement, combat, and animation profiles.
- **Expert Advice**: PawnDefinition is the glue for your characters. Ensure you assign a PawnPrefab and appropriate profiles for Movement and Combat. Check 'First Proof' by spawning it in a test scene.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/pawn)

### Projectile Definition
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Projectile Definition (Combat)
- **Summary**: Project-window creation path for projectile behavior.

### Projectile Impact Definition
- **Summary**: Feature contract for Combat, VFX module setup and lane compatibility.

### Projectile Impact Definition (Combat, VFX)
- **Summary**: Controls hit/miss VFX, audio, and impact feel for projectiles.
- **Expert Advice**: Use small hitPauseDuration to add 'weight' to physical impacts.

### Projectile Launcher2 D
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Projectile Launcher2 D (Combat)
- **Summary**: 2D projectile launcher; supports 2D physics projectiles and circlecast/raycast hitscan.
- **Expert Advice**: Set Hit Mask to exclude the shooter's layer. Use Hitscan for instant weapons (bullets) and Prefab for traveling projectiles (missiles, fireballs).
- **Axioms**: `Dimensions2D, Realtime`

### Projectile Launcher3 D
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Projectile Launcher3 D (Combat)
- **Summary**: 3D projectile launcher; supports physics-based projectiles and raycast hitscan.
- **Expert Advice**: Set Hit Mask to exclude the shooter's layer. Ensure projectile prefabs have a Rigidbody or IProjectileRuntimeBody for movement.
- **Axioms**: `Dimensions3D, Realtime`

### Status Effect Definition
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Status Effect Definition (Combat)
- **Summary**: Defines a status effect (buff or debuff) that can be applied to actors.
- **Expert Advice**: Use tickInterval for effects that apply over time (e.g., Poison, Heal).

### Weapon Data
- **Summary**: Feature contract for Combat module setup and lane compatibility.

### Weapon Data (Combat)
- **Summary**: The primary definition for an actor's weapon; defines damage, timing, range, and presentation.
- **Expert Advice**: Use overrideController to change actor animations when this weapon is equipped.

---

## Capability: Animation
> Visual state machines and skeletal deformation.

### Actor Animation Controller
- **Summary**: Feature contract for Animation module setup and lane compatibility.

### Actor Animation Controller (Animation)
- **Summary**: Controls actor animation states, transitions, and parameter syncing.
- **Expert Advice**: Provides a logic-only abstraction for animations. Other systems (Movement, Combat) should call these methods instead of driving the Animator directly to maintain decoupling.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/animation)

### Actor Animation Definition
- **Summary**: Feature contract for Animation module setup and lane compatibility.

### Actor Animation Definition (Animation)
- **Summary**: Defines the animation signal contract supported by an actor setup.
- **Expert Advice**: Leave Supported Signals empty to accept all standard signals. Use specific signals only if the animator is restricted.

### Actor Animation Driver
- **Summary**: Feature contract for Animation module setup and lane compatibility.

### Actor Animation Driver (Animation)
- **Summary**: Bridges Pyralis signals (Move, Jump, Attack) to Animator parameters, plus sprite/billboard facing.
- **Expert Advice**: The Animation Driver is a 'Signal Bridge'. It decouples logic from visual state. If using sprites, ensure 'Visual Root' is assigned so the driver can flip the localScale for facing direction logic.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/animation)

### Actor Shadow Driver
- **Summary**: Feature contract for Animation module setup and lane compatibility.

### Actor Shadow Driver (Animation)
- **Summary**: Applies shadow presentation (blob or renderer) based on PawnPresentationProfile.
- **Expert Advice**: Use Blob mode for 2D and 2.5D games. Renderer mode is only supported for Rigged3D actors.

### Billboard Facing3 D
- **Summary**: Feature contract for Animation module setup and lane compatibility.

### Billboard Facing3 D (Animation)
- **Summary**: Forces a 3D object to face the camera and supports left/right mirroring.
- **Expert Advice**: Use Y-Axis only for ground-based actors. Use Full Facing for projectiles or floating items.
- **Axioms**: `Dimensions3D`

### Combat Action Definition
- **Summary**: Feature contract for Combat, Animation module setup and lane compatibility.

### Combat Action Definition (Combat, Animation)
- **Summary**: Project-window creation path for one combat action.
- **Expert Advice**: Use comboStep to sequence multi-hit attacks. Use cooldownOverride if this move should be slower or faster than the weapon default.

### Enemy Ambient Feature Profile
- **Summary**: Feature contract for Combat, Animation module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `ThirdPerson3D`

### Enemy Ambient Feature Runtime
- **Summary**: Feature contract for Combat, Animation module setup and lane compatibility.

### Pawn Animation Profile
- **Summary**: Feature contract for Animation module setup and lane compatibility.

### Pawn Animation Profile (Animation)
- **Summary**: Maps high-level gameplay signals to Unity Animator parameters for a specific character visual.
- **Expert Advice**: Use the Controller Mapping Wizard in the custom inspector to quickly align your animator with Pyralis signals. This profile acts as the bridge between gameplay logic and visual feedback.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/animation)

### Pawn Presentation Profile
- **Summary**: Feature contract for Animation, VFX module setup and lane compatibility.

### Pawn Presentation Profile (Animation, VFX)
- **Summary**: Project-window creation path for pawn presentation lane and visual setup choices.

### Pawn2 D Presentation Component
- **Summary**: Feature contract for Animation, VFX module setup and lane compatibility.

### Pawn2 D Presentation Component (Animation, VFX)
- **Summary**: Maps movement and dash/death state to sprite tinting, facing, squash/stretch, and animation signals.
- **Expert Advice**: Do not leave both moving and idle tint invisible. Do not enable tilt or squash/stretch until the visual pivot is correct.
- **Axioms**: `Dimensions2D`

### Pawn3 D Presentation Component
- **Summary**: Feature contract for Animation module setup and lane compatibility.

### Pawn3 D Presentation Component (Animation)
- **Summary**: 3D presentation module; maps movement state to Animator signals and handles billboarding.
- **Expert Advice**: Presentation logic should be visual-only. It reads from movement/combat state and writes to the Animator. Use Billboarding settings if your 3D pawn uses 2D sprites.
- **Axioms**: `Dimensions3D`

---

## Capability: VFX
> Particle systems, post-processing, and shader effects.

### Actor Feedback Feature Runtime
- **Summary**: Feature contract for VFX module setup and lane compatibility.

### Actor Feedback Profile
- **Summary**: Feature contract for UI, VFX module setup and lane compatibility.

### Actor Feedback Profile (UI, VFX)
- **Summary**: Configures which gameplay events (damage, death, score) trigger visual feedback or HUD notifications.
- **Expert Advice**: Use these toggles to silence feedback for specific actor archetypes (e.g., destructible props vs. bosses).
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/visuals)

### Actor Feedback Publisher
- **Summary**: Feature contract for VFX module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`

### Actor Floating Feedback Receiver
- **Summary**: Feature contract for VFX module setup and lane compatibility.

### Actor Floating Feedback Receiver (VFX)
- **Summary**: Renders world-space damage, heal, score, combo, status, parry, stagger, guard-break, and finisher popups from actor feedback events.
- **Expert Advice**: Enable at least one feedback category. Use shorter popup lifetimes for actors that take frequent damage. For HUD-only games, prefer participant HUD presenters over world-space popups.

### Camera Shake
- **Summary**: Feature contract for VFX module setup and lane compatibility.

### Camera Shake (VFX)
- **Summary**: Canonical camera shake service for gameplay impact feedback.
- **Expert Advice**: 2D path: use Planar2D and mostly position influence. 3D path: use Spatial3D or PositionAndRotation with lower intensity.
- **Axioms**: `Dimensions2D, Dimensions3D`

### Collectible Feedback2 D
- **Summary**: Feature contract for VFX, Audio module setup and lane compatibility.

### Collectible Feedback2 D (VFX, Audio)
- **Summary**: Manages audio and visual feedback (particles/sounds) for collectible actions.
- **Expert Advice**: Set SFX spatial blend to 0 (2D) for consistent UI-style feedback. Ensure Particle Systems have 'Stop Action' set to 'Disable' or 'None' for pooling.

### Hazard Feedback Profile
- **Summary**: Feature contract for UI, VFX module setup and lane compatibility.

### Hazard Feedback Profile (UI, VFX)
- **Summary**: Defines the visual feedback (flashes, popups) for hazard activation and explosion.
- **Expert Advice**: Use popupFontSize to ensure warnings are visible at the game's camera distance.

### Hazard Feedback Runtime
- **Summary**: Feature contract for Combat, VFX module setup and lane compatibility.

### Participant Feedback Service
- **Summary**: Feature contract for VFX module setup and lane compatibility.

### Participant Feedback Service (VFX)
- **Summary**: Global service for streaming feedback events (scoring, health) to participants for UI/SFX triggers.

### Pawn Presentation Profile
- **Summary**: Feature contract for Animation, VFX module setup and lane compatibility.

### Pawn Presentation Profile (Animation, VFX)
- **Summary**: Project-window creation path for pawn presentation lane and visual setup choices.

### Pawn2 D Presentation Component
- **Summary**: Feature contract for Animation, VFX module setup and lane compatibility.

### Pawn2 D Presentation Component (Animation, VFX)
- **Summary**: Maps movement and dash/death state to sprite tinting, facing, squash/stretch, and animation signals.
- **Expert Advice**: Do not leave both moving and idle tint invisible. Do not enable tilt or squash/stretch until the visual pivot is correct.
- **Axioms**: `Dimensions2D`

### Projectile Impact Definition
- **Summary**: Feature contract for Combat, VFX module setup and lane compatibility.

### Projectile Impact Definition (Combat, VFX)
- **Summary**: Controls hit/miss VFX, audio, and impact feel for projectiles.
- **Expert Advice**: Use small hitPauseDuration to add 'weight' to physical impacts.

### Sprite Flasher
- **Summary**: Feature contract for VFX module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`

### Sprite Flasher (VFX)
- **Summary**: Coroutine-driven color flash effects on SpriteRenderers.
- **Lanes**: `Sprite2D`, `Billboard2_5D`

---

## Capability: Tabletop
> Board game logic, piece management, and move policies.

### Board Definition
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Board Definition (Tabletop, Grid)
- **Summary**: Project-window creation path for tabletop board layouts and starting pieces.

### Board Move Policy Definition
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Board Move Policy Definition (Tabletop, Grid)
- **Summary**: Project-window creation path for tabletop legal-move policy.

### Board Piece Definition
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Board Piece Definition (Tabletop, Grid)
- **Summary**: Project-window creation path for tabletop board pieces.

### Board Runtime State
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Board Runtime State (Tabletop, Grid)
- **Summary**: Authoritative logical board state for tabletop-style games.

### Board Terminal Condition Definition
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Board Terminal Condition Definition (Tabletop, Grid)
- **Summary**: Project-window creation path for tabletop round or game-end conditions.

### Phase Definition
- **Summary**: Feature contract for Tabletop, TurnBased module setup and lane compatibility.

### Phase Definition (Tabletop, TurnBased)
- **Summary**: Project-window creation path for turn phase rules.

### Tabletop Board Grid Presenter
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Tabletop Board Grid Presenter (Tabletop, Grid)
- **Summary**: Inspector Add Component path for a board presenter that can build selectable tabletop spaces.
- **Expert Advice**: Bridges the abstract BoardDefinition to scene objects. It handles coordinate mapping (X,Y) to world positions. Ensure your cell size matches your visual assets.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/tabletop)

### Tabletop Board Selection Bridge
- **Summary**: Feature contract for Tabletop module setup and lane compatibility.

### Tabletop Board Selection Bridge (Tabletop)
- **Summary**: Bridge between a board presenter and queued board actions (e.g. piece selection and movement).

### Tabletop Turn Status Presenter
- **Summary**: Feature contract for Tabletop module setup and lane compatibility.

### Tabletop Turn Status Presenter (Tabletop)
- **Summary**: LIGHTWEIGHT UI binding that shows which tabletop seat acts next.

### Turn Order Definition
- **Summary**: Feature contract for Tabletop, TurnBased module setup and lane compatibility.

### Turn Order Definition (Tabletop, TurnBased)
- **Summary**: Project-window creation path for tabletop and turn/menu action order.

### Turn Order Service
- **Summary**: Feature contract for Tabletop, TurnBased module setup and lane compatibility.

### Turn Order Service (Tabletop, TurnBased)
- **Summary**: Manages turn sequence and active participant in turn-based games.
- **Expert Advice**: Central authority for whose turn it is. Use this to gate player input and trigger AI decision phases in Tabletop modes.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/tabletop)
- **Axioms**: `TurnBased`

---

## Capability: Grid
> Coordinate systems, cell properties, and spatial queries.

### Board Definition
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Board Definition (Tabletop, Grid)
- **Summary**: Project-window creation path for tabletop board layouts and starting pieces.

### Board Move Policy Definition
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Board Move Policy Definition (Tabletop, Grid)
- **Summary**: Project-window creation path for tabletop legal-move policy.

### Board Piece Definition
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Board Piece Definition (Tabletop, Grid)
- **Summary**: Project-window creation path for tabletop board pieces.

### Board Runtime State
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Board Runtime State (Tabletop, Grid)
- **Summary**: Authoritative logical board state for tabletop-style games.

### Board Terminal Condition Definition
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Board Terminal Condition Definition (Tabletop, Grid)
- **Summary**: Project-window creation path for tabletop round or game-end conditions.

### Tabletop Board Grid Presenter
- **Summary**: Feature contract for Tabletop, Grid module setup and lane compatibility.

### Tabletop Board Grid Presenter (Tabletop, Grid)
- **Summary**: Inspector Add Component path for a board presenter that can build selectable tabletop spaces.
- **Expert Advice**: Bridges the abstract BoardDefinition to scene objects. It handles coordinate mapping (X,Y) to world positions. Ensure your cell size matches your visual assets.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/tabletop)

---

## Capability: Turn Based
> Phase management, action queues, and initiative.

### Action Definition
- **Summary**: Feature contract for Session, TurnBased module setup and lane compatibility.

### Action Definition (Session, TurnBased)
- **Summary**: Project-window creation path for one selectable command or resolver-backed action.

### Phase Definition
- **Summary**: Feature contract for Tabletop, TurnBased module setup and lane compatibility.

### Phase Definition (Tabletop, TurnBased)
- **Summary**: Project-window creation path for turn phase rules.

### Turn Order Definition
- **Summary**: Feature contract for Tabletop, TurnBased module setup and lane compatibility.

### Turn Order Definition (Tabletop, TurnBased)
- **Summary**: Project-window creation path for tabletop and turn/menu action order.

### Turn Order Service
- **Summary**: Feature contract for Tabletop, TurnBased module setup and lane compatibility.

### Turn Order Service (Tabletop, TurnBased)
- **Summary**: Manages turn sequence and active participant in turn-based games.
- **Expert Advice**: Central authority for whose turn it is. Use this to gate player input and trigger AI decision phases in Tabletop modes.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/tabletop)
- **Axioms**: `TurnBased`

### Turn Runtime State
- **Summary**: Feature contract for Turn Based module setup and lane compatibility.

### Turn Runtime State (Turn Based)
- **Summary**: Runtime cursor for seat-based turn order tracking.
- **Expert Advice**: Pure data class representing the cursor in a round. It handles the wrap-around logic from the last participant back to the first.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/tabletop)

---

## Capability: Stats
> Attributes, modifiers, and character progression systems.

### Actor Status Effect Profile
- **Summary**: Feature contract for Combat, Stats module setup and lane compatibility.

### Actor Status Effect Profile (Combat, Stats)
- **Summary**: Defines common status effect vulnerabilities and immunities for an actor.
- **Expert Advice**: Use defaultShieldDamageReduction to scale incoming damage when the actor has an active shield effect.

### Actor Status Effect Receiver
- **Summary**: Feature contract for Combat, Stats module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`

### Equippable Item Definition
- **Summary**: Feature contract for Stats, Inventory module setup and lane compatibility.

### Equippable Item Definition (Stats, Inventory)
- **Summary**: Extends standard items with equipment slots and stat modifiers.
- **Expert Advice**: Use stat modifiers to provide meaningful progression and customization through equipment.

### Progression Curve Definition
- **Summary**: Feature contract for Stats module setup and lane compatibility.

### Skill Tree Definition
- **Summary**: Feature contract for Stats module setup and lane compatibility.

### Stat Definition
- **Summary**: Feature contract for Stats module setup and lane compatibility.

### Stat Definition (Stats)
- **Summary**: Defines a reusable RPG stat (e.g., Strength, Wisdom, Health).
- **Expert Advice**: Use categories to group related stats (e.g., 'Primary', 'Combat', 'Social') in UI and tools.

---

## Capability: Inventory
> Item storage, equipment, and resource management.

### Actor Interaction Handler
- **Summary**: Feature contract for Inventory module setup and lane compatibility.
- **Lanes**: `Sprite2D`

### Actor Pickup Collector Feature2 D
- **Summary**: Feature contract for Inventory module setup and lane compatibility.
- **Lanes**: `Sprite2D`

### Actor Pickup Collector Feature3 D
- **Summary**: Feature contract for Inventory module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `ThirdPerson3D`

### Collectible2 D
- **Summary**: Feature contract for Inventory module setup and lane compatibility.

### Collectible2 D (Inventory)
- **Summary**: 2D collectible item that awards points and triggers feedback on collection.
- **Expert Advice**: Use CircleCollider2D for optimal performance. Collectibles bob vertically based on local time to avoid visual synchronization.
- **Axioms**: `Dimensions2D`

### Collectible3 D
- **Summary**: Feature contract for Inventory module setup and lane compatibility.

### Collectible3 D (Inventory)
- **Summary**: 3D collectible item that awards points and bobs in world space.
- **Axioms**: `Dimensions3D`

### Equipment Service
- **Summary**: Feature contract for Inventory module setup and lane compatibility.

### Equipment Slot Definition
- **Summary**: Feature contract for Inventory module setup and lane compatibility.

### Equippable Item Definition
- **Summary**: Feature contract for Stats, Inventory module setup and lane compatibility.

### Equippable Item Definition (Stats, Inventory)
- **Summary**: Extends standard items with equipment slots and stat modifiers.
- **Expert Advice**: Use stat modifiers to provide meaningful progression and customization through equipment.

### Inventory Service
- **Summary**: Feature contract for Inventory module setup and lane compatibility.

### Item Catalog Definition
- **Summary**: Feature contract for Inventory module setup and lane compatibility.

### Item Definition
- **Summary**: Feature contract for Inventory module setup and lane compatibility.

### Pickup Feature Profile
- **Summary**: Feature contract for Inventory, Puzzle module setup and lane compatibility.

### Pickup Feature Profile (Inventory, Puzzle)
- **Summary**: Project-window creation path for pickup feature setup.

### Rpg Vendor Panel Presenter
- **Summary**: Feature contract for Inventory module setup and lane compatibility.

### Vendor Definition
- **Summary**: Feature contract for Inventory module setup and lane compatibility.

---

## Capability: Dialogue
> Narrative flow, branching conversations, and event nodes.

### Dialogue Graph Definition
- **Summary**: Feature contract for Dialogue module setup and lane compatibility.

### Dialogue Service
- **Summary**: Feature contract for Dialogue module setup and lane compatibility.

### Dialogue Service (Dialogue)
- **Summary**: Manages narrative flow and branching dialogue sessions with condition and effect support.
- **Expert Advice**: Dialogue graphs can trigger world effects via IDialogueEffectSink. Mention event hooks: Graphs can trigger quest updates or item grants through effects. Use the custom condition resolver for complex narrative gates.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/rpg/dialogue)
- **Axioms**: `Realtime, TurnBased`

### Hub Interaction Service
- **Summary**: Feature contract for Dialogue, Puzzle module setup and lane compatibility.

### Npc Definition
- **Summary**: Feature contract for Dialogue module setup and lane compatibility.

### Quest Definition
- **Summary**: Feature contract for Dialogue module setup and lane compatibility.

### Rpg Dialogue Panel Presenter
- **Summary**: Feature contract for Dialogue module setup and lane compatibility.

### Rpg Quest Board Panel Presenter
- **Summary**: Feature contract for Dialogue module setup and lane compatibility.

---

## Capability: Puzzle
> Logic gates, triggers, and state-based world interactions.

### Actor Interaction Feature
- **Summary**: Feature contract for Input, Puzzle module setup and lane compatibility.

### Actor Interaction Feature Runtime
- **Summary**: Feature contract for Input, Puzzle module setup and lane compatibility.

### Actor Interaction Input Bridge2 D
- **Summary**: Feature contract for Input, Puzzle module setup and lane compatibility.

### Actor Interaction Input Bridge2 D (Input, Puzzle)
- **Summary**: Forwards interact input into an installed Actor Interaction feature on ActorFeatureHost.
- **Expert Advice**: Bridge only forwards input. Ensure the Interaction feature is installed in PawnDefinition.

### Camera Zone
- **Summary**: Feature contract for Puzzle, Camera module setup and lane compatibility.

### Camera Zone (Puzzle, Camera)
- **Summary**: 3D trigger volume that switches CameraRigProfile when the player enters.
- **Expert Advice**: Combat arena path: enter a tighter profile and exit back to the default profile. Cutscene path: enable One Shot and leave On Exit Profile empty. Exploration path: use wider profiles for overlooks or large platforming spaces.

### Damage Zone
- **Summary**: Feature contract for Combat, Puzzle module setup and lane compatibility.

### Damage Zone (Combat, Puzzle)
- **Summary**: 3D trigger volume that repeatedly damages overlapping actors.
- **Expert Advice**: Do not set Tick Interval too low. Ensure target actors have a HealthComponent. Use Hazard Impact Profile if you want this hazard to behave identically to others.
- **Axioms**: `Dimensions3D`

### Hub Interaction Service
- **Summary**: Feature contract for Dialogue, Puzzle module setup and lane compatibility.

### Interaction Feature Profile
- **Summary**: Feature contract for Session, Puzzle module setup and lane compatibility.

### Interaction Feature Profile (Session, Puzzle)
- **Summary**: Defines how an actor interacts with world objects.
- **Expert Advice**: Use interactionCooldown to prevent rapid-fire interaction spamming.

### Pickup Feature Profile
- **Summary**: Feature contract for Inventory, Puzzle module setup and lane compatibility.

### Pickup Feature Profile (Inventory, Puzzle)
- **Summary**: Project-window creation path for pickup feature setup.

---

## Capability: Camera
> Framing, following, and world containment boundaries.

### Camera Bounds Provider
- **Summary**: Feature contract for Camera module setup and lane compatibility.

### Camera Bounds Provider (Camera)
- **Summary**: Provides world-space boundaries for camera framing and containment.
- **Axioms**: `BoundedSpace`

### Camera Occlusion Fader
- **Summary**: Feature contract for Camera module setup and lane compatibility.

### Camera Occlusion Fader (Camera)
- **Summary**: Fades renderers that block the line of sight between the camera and a tracked target.
- **Expert Advice**: Keep the player layer out of Occlusion Mask. Use this for 3D line-of-sight fading; 2D sprite visibility is usually handled via sorting layers.

### Camera Rig Profile
- **Summary**: Feature contract for Camera module setup and lane compatibility.

### Camera Rig Profile (Camera)
- **Summary**: Project-window creation path for camera framing, follow, zoom, and 2D orthographic route choices.
- **Expert Advice**: CameraRigProfile defines how the world is seen. For 2D games, check 'Orthographic'. Use 'Follow Offset' to position the camera relative to the pawn or group focus.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/camera)

### Camera Zone
- **Summary**: Feature contract for Puzzle, Camera module setup and lane compatibility.

### Camera Zone (Puzzle, Camera)
- **Summary**: 3D trigger volume that switches CameraRigProfile when the player enters.
- **Expert Advice**: Combat arena path: enter a tighter profile and exit back to the default profile. Cutscene path: enable One Shot and leave On Exit Profile empty. Exploration path: use wider profiles for overlooks or large platforming spaces.

### Cinemachine Camera Rig Controller
- **Summary**: Feature contract for Camera module setup and lane compatibility.

### Cinemachine Camera Rig Controller (Camera)
- **Summary**: Cinemachine Camera Rig Controller is the Pyralis scene camera runtime. Use this Inspector for assigned references and tuning values.
- **Expert Advice**: The Camera Rig handles multi-user framing. For 2D projects, ensure your 'Target Camera' is set to Orthographic. Use '2D Bounds Framing' on the rig root to keep the camera within the designed level boundaries.

### Playfield Profile
- **Summary**: Feature contract for Setup, Camera module setup and lane compatibility.

### Playfield Profile (Setup, Camera)
- **Summary**: Project-window creation path for movement space, bounds, wrap, and arena-depth rules.

---

## Capability: Environment
> World geometry, lighting, and static decoration.

### Level Data
- **Summary**: Feature contract for Setup, Environment module setup and lane compatibility.

### Level Data (Setup, Environment)
- **Summary**: Data container for level configuration, including display names and scene references.
- **Expert Advice**: LevelData assets are primarily used by the LevelRegistry to build the world-select UI. Ensure the SceneName exactly matches the entry in File -> Build Settings.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/navigation)

### Rpg Open Zone Service
- **Summary**: Feature contract for Session, Environment module setup and lane compatibility.

---

## Capability: Audio
> Soundscapes, spatial audio, and music management.

### Collectible Feedback2 D
- **Summary**: Feature contract for VFX, Audio module setup and lane compatibility.

### Collectible Feedback2 D (VFX, Audio)
- **Summary**: Manages audio and visual feedback (particles/sounds) for collectible actions.
- **Expert Advice**: Set SFX spatial blend to 0 (2D) for consistent UI-style feedback. Ensure Particle Systems have 'Stop Action' set to 'Disable' or 'None' for pooling.

### Settings Manager
- **Summary**: Feature contract for UI, Audio module setup and lane compatibility.

### Settings Manager (UI, Audio)
- **Summary**: Manages global audio volume levels and mixer integration. Connects settings profiles to the active Unity AudioMixer.
- **Expert Advice**: Ensure your AudioMixer has exposed parameters named 'MusicVolume' and 'SFXVolume' (case sensitive) for the manager to drive. This component persists across scenes if placed on a DontDestroyOnLoad root.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/settings)

---

## Capability: Networking
> State synchronization, authority, and multiplayer connectivity.

### Local Participant Authority Service
- **Summary**: Feature contract for Networking module setup and lane compatibility.

### Local Participant Authority Service (Networking)
- **Summary**: Provides the local-only authority model for participants.
- **Expert Advice**: The Local Authority service is a 'dumb' pass-through for same-machine play. It identifies all inputs as local. Use the Networked variant for online multiplayer projects.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/authority)

### Local Session Ownership Service
- **Summary**: Feature contract for Networking module setup and lane compatibility.

### Local Session Ownership Service (Networking)
- **Summary**: Provides the local-only ownership model for game sessions, used in offline modes.
- **Expert Advice**: Enforces that the local machine owns the game world. Use this for single-player or local split-screen prototypes. It effectively bypasses network synchronization checks.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/session)

### Networked Participant Authority Service
- **Summary**: Feature contract for Networking module setup and lane compatibility.

### Networked Participant Authority Service (Networking)
- **Summary**: Resolves participant authority from the active Netcode for GameObjects (NGO) local client.
- **Expert Advice**: Use this service when you want to bridge Unity Input System seating to NGO Client IDs automatically.

### Networked Participant Roster Service
- **Summary**: Feature contract for Networking module setup and lane compatibility.

### Networked Participant Roster Service (Networking)
- **Summary**: Drop-in replacement for ParticipantRosterService in online sessions. Resolves NGO Client IDs.

### Networked Participant Spawn Service
- **Summary**: Feature contract for Networking module setup and lane compatibility.

### Networked Participant Spawn Service (Networking)
- **Summary**: Drop-in replacement for ParticipantSpawnService in online sessions. Registers pawns with NGO.

### Networked Session Ownership Service
- **Summary**: Feature contract for Networking module setup and lane compatibility.

### Networked Session Ownership Service (Networking)
- **Summary**: NGO-backed session ownership policy used by networked Pyralis sessions.
- **Expert Advice**: This service enforces server-authoritative logic for the session lifecycle.

### Networked Session State Service
- **Summary**: Feature contract for Networking module setup and lane compatibility.

### Networked Session State Service (Networking)
- **Summary**: Drop-in replacement for SessionStateService in online sessions. Handles NGO role startup.

### Pyralis Network Setup Validator
- **Summary**: Feature contract for Networking module setup and lane compatibility.

### Pyralis Network Setup Validator (Networking)
- **Summary**: Shared validation for the NGO-backed Pyralis runtime lane.

---

## Capability: Quests
> Quest tracking, objective management, and reward systems.

### Quest Service
- **Summary**: Feature contract for Quests module setup and lane compatibility.

---

## Capability: Vendors
> Shop logic, trading interfaces, and currency exchange.

### Vendor Service
- **Summary**: Feature contract for Vendors module setup and lane compatibility.

### Vendor Service (Vendors)
- **Summary**: Facilitates item transactions (buying and selling) between characters and vendors using currency.
- **Expert Advice**: Vendor offers require valid Item IDs from the catalog. Ensure currency items are configured in the inventory service and catalog.
- **Axioms**: `Realtime, TurnBased`

---

## Capability: Skill Tree
> Abilities, unlock paths, and specialized talent trees.

### Skill Tree Service
- **Summary**: Feature contract for Skill Tree module setup and lane compatibility.

---

## Capability: Progression
> Experience points, leveling, and milestone tracking.

### Progression Service
- **Summary**: Feature contract for Progression module setup and lane compatibility.

---

## Capability: Combat State
> Health, damage tracking, and actor life-cycle state.

### Actor Health State
- **Summary**: Feature contract for Combat State module setup and lane compatibility.

### Actor Health State (Combat State)
- **Summary**: Provides the current health and life state of an actor.
- **Expert Advice**: Use Damaged and Died events to trigger UI updates and death sequences. Ensure Faction is correctly set for friendly-fire filtering.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/combat/health)

### Health Component
- **Summary**: Feature contract for Combat State module setup and lane compatibility.

### Health Component (Combat State)
- **Summary**: Universal health component for players, enemies, and destructible props.
- **Expert Advice**: HealthComponent is a neutral actor. Use Faction to prevent friendly fire. Attach HitFlash or HitPause listeners to the OnDamaged event for standard combat feel. Use Faction.Neutral for props that should be destructible by everyone.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/health)
- **Axioms**: `Realtime`

---

## Capability: Combat Sensors
> Hitboxes, hurtboxes, and collision-based event triggers.

### Hit Box
- **Summary**: Feature contract for Combat Sensors module setup and lane compatibility.

### Hit Box (Combat Sensors)
- **Summary**: One-shot overlap query hitbox for melee and projectile impacts.
- **Expert Advice**: HitBoxes are disabled colliders used only for overlap queries. Ensure the owner is set so knockback direction is calculated correctly from the attacker's root. Use hitPauseSink for juicy combat feel.
- **Axioms**: `Dimensions3D, Realtime`

### Hit Box2 D
- **Summary**: Feature contract for Combat Sensors module setup and lane compatibility.

### Hit Box2 D (Combat Sensors)
- **Summary**: Trigger-based 2D hitbox for melee attacks in Tilemap or 2D physics scenes.
- **Expert Advice**: HitBox2D uses OnTriggerEnter2D. Ensure the root actor has a Rigidbody2D and correct LayerMasks to detect the intended targets. Use 'Freeze Frame Duration' for impact weight.
- **Axioms**: `Dimensions2D, Realtime`

---

## Capability: Rules
> Game-mode specific rulesets, win/loss conditions, and timers.

### Game Mode Definition
- **Summary**: Feature contract for Rules module setup and lane compatibility.

### Game Mode Definition (Rules)
- **Summary**: Defines the specific game rules, required features, and scene setup for a gameplay session.
- **Expert Advice**: GameModeDefinition is your 'Ruleset' bridge. Use 'Required Feature Modules' to inject global systems like Scoring or Weather. Ensure the 'Gameplay Scene' matches the level design intended for this mode.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/game-mode)

---

## Capability: Scoring
> Points, resources, leaderboards, and objective tracking.

### Participant Score Service
- **Summary**: Feature contract for Scoring module setup and lane compatibility.

### Participant Score Service (Scoring)
- **Summary**: Canonical scoring service; tracks participant scores, session points, survival time, and high-score persistence.
- **Expert Advice**: The Scoring service stores high scores in PlayerPrefs. Use ISessionScoreService for cross-feature point collection and IParticipantRoster for multiplayer leaderboards.

---

## Capability: Participants
> Player seats, AI slots, and input ownership.

### Participant Definition
- **Summary**: Feature contract for Participants module setup and lane compatibility.

### Participant Definition (Participants)
- **Summary**: Defines a player or NPC seat within a session, including their default pawn and input configuration.
- **Expert Advice**: ParticipantDefinitions represent 'Seats' at the table. For AI, leave 'Auto Join' on and set an AI-compatible Input Profile. For local multiplayer, ensure unique Input Profiles or shared schemes are configured.

---

## Capability: 2D Kinetic Motor
> Low-level 2D physical motor implementation (Rigidbody2D).

### Motor2 D
- **Summary**: Feature contract for 2D Kinetic Motor module setup and lane compatibility.

### Motor2 D (2D Kinetic Motor)
- **Summary**: Canonical 2D pawn motor; coordinates movement, animations, and reactions.
- **Expert Advice**: Ensure your Rigidbody2D is set to 'Interpolate' for smooth camera follow. If using Top-Down, set Gravity Scale to 0. Motor2D delegates to Movement and Presentation modules.
- **Axioms**: `Dimensions2D, Realtime`

---

## Capability: 3D Kinetic Motor
> Low-level 3D physical motor implementation (CharacterController).

### Motor3 D
- **Summary**: Feature contract for 3D Kinetic Motor module setup and lane compatibility.

### Motor3 D (3D Kinetic Motor)
- **Summary**: Canonical 3D pawn motor; sequences input, movement, traversal, and presentation sibling modules.
- **Expert Advice**: Motor3D is a high-level coordinator. It does not move the pawn directly but Ticks its sibling modules in a deterministic order. Ensure CharacterController 'Skin Width' is at least 10% of the radius to prevent jitter on slopes.
- **Axioms**: `Dimensions3D, Realtime`

---

## Capability: 3D Steering
> Pathfinding and navigation for 3D actors.

### Enemy A I
- **Summary**: Feature contract for Steering3D, TacticsAggressive module setup and lane compatibility.

### Enemy A I (Steering3D, TacticsAggressive)
- **Summary**: Canonical 3D Enemy AI controller; handles patrol, detection, and attack states.
- **Expert Advice**: EnemyAI separates 'Tactics' from 'Steering'. Use 'EnemyFeatureProfile' to define shared stats like Aggro Range and Attack Cooldowns across multiple prefabs.
- **Axioms**: `Dimensions3D, Realtime`

---

## Capability: Traversal
> World interaction features like ledge-climb, ladders, and jumping.

### Actor Traversal Feature
- **Summary**: Feature contract for Traversal module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `ThirdPerson3D`

### Pawn Traversal Feature Runtime3 D
- **Summary**: Feature contract for Traversal module setup and lane compatibility.
- **Lanes**: `Billboard2_5D`, `ThirdPerson3D`

### Pawn3 D Traversal Component
- **Summary**: Feature contract for Traversal module setup and lane compatibility.

### Pawn3 D Traversal Component (Traversal)
- **Summary**: 3D traversal module; handles ledge climbing, hanging, and shimmying.
- **Expert Advice**: Traversal logic is separated from base movement. Ensure your Animator has 'Climb' and 'Hang' signals wired to valid animations.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/traversal)
- **Axioms**: `Dimensions3D`

### Top Down Hop Feature Runtime
- **Summary**: Feature contract for Traversal module setup and lane compatibility.
- **Lanes**: `Sprite2D`, `Billboard2_5D`

---

## Capability: Melee Flow
> Attack sequencing, combos, and melee state management.

### Pawn Combat Behaviour
- **Summary**: Feature contract for Melee Flow module setup and lane compatibility.

### Pawn Combat Behaviour (Melee Flow)
- **Summary**: Primary pawn combat controller; handles sequences, combos, and delegates to modules.
- **Expert Advice**: PawnCombatBehaviour is sequence-driven. If attacks feel floaty or don't land, check that your Animation Sequence assets have the 'FireHitBox' event timed precisely with the swing frame.
- **Axioms**: `Realtime`

---

## Capability: Ranged Flow
> Projectile sequencing, reloading, and targeting logic.

### Projectile Fire Planner
- **Summary**: Feature contract for Ranged Flow module setup and lane compatibility.

### Projectile Fire Planner (Ranged Flow)
- **Summary**: Logic for planning projectile trajectories based on fire modes and spread rules.
- **Expert Advice**: This class produces ProjectileSpawnCommands but does not execute them. Use it in conjunction with a Launcher to decouple firing logic from physical spawning.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/combat/projectiles)

### Projectile Launcher Base
- **Summary**: Feature contract for Ranged Flow module setup and lane compatibility.

### Projectile Launcher Base (Ranged Flow)
- **Summary**: Base component for firing projectiles and hitscan attacks with built-in pooling and feedback routing.
- **Expert Advice**: Extend this class to create custom 2D or 3D launchers. It handles the low-level spawning and impact feedback routing via IHitPauseSink and ICameraShakeSink.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/combat/projectiles)

### Projectile Launcher2 D (Ranged Flow)
- **Summary**: Base component for firing projectiles and hitscan attacks with built-in pooling and feedback routing.
- **Expert Advice**: Extend this class to create custom 2D or 3D launchers. It handles the low-level spawning and impact feedback routing via IHitPauseSink and ICameraShakeSink.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/combat/projectiles)

### Projectile Launcher3 D (Ranged Flow)
- **Summary**: Base component for firing projectiles and hitscan attacks with built-in pooling and feedback routing.
- **Expert Advice**: Extend this class to create custom 2D or 3D launchers. It handles the low-level spawning and impact feedback routing via IHitPauseSink and ICameraShakeSink.
- **Docs**: [Technical Specification](https://docs.neonblack.com/pyralis/combat/projectiles)

---

## Capability: Aggressive Tactics
> AI decision trees for charging, flanking, and attacking.

### Enemy A I
- **Summary**: Feature contract for Steering3D, TacticsAggressive module setup and lane compatibility.

### Enemy A I (Steering3D, TacticsAggressive)
- **Summary**: Canonical 3D Enemy AI controller; handles patrol, detection, and attack states.
- **Expert Advice**: EnemyAI separates 'Tactics' from 'Steering'. Use 'EnemyFeatureProfile' to define shared stats like Aggro Range and Attack Cooldowns across multiple prefabs.
- **Axioms**: `Dimensions3D, Realtime`

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
- **Lanes**: `Billboard2_5D`, `ThirdPerson3D`

### 3D Space Action
- **Summary**: A Rigged3D route where actors, cameras, interaction, enemies, projectiles, or traversal operate in 3D world space.
- **Lanes**: `Rigged3D`

### Action Selection Proof
- **Summary**: Run one selected command before expanding menus, cards, ability lists, animation polish, or AI.
- **Lanes**: `UiMenuOnly`, `TabletopBoard`, `CameraCursor`, `Sprite2D`, `ThirdPerson3D`

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

### Assign Settings Manager
- **Summary**: Create or assign a SettingsManager to handle global volume, deadzones, and control swaps.

### Assign Setup Profile
- **Summary**: Create or assign the setup recipe that lists runtime patterns.

### Assign Spawn Points
- **Summary**: Place spawn Transforms so pawn-backed participants can enter the scene predictably.

### Assign Tabletop Selection Surface
- **Summary**: Create or assign the board, card, cursor, or action-selection surface that makes one no-pawn proof selectable in Play Mode.

### Board / Action Selection
- **Summary**: Board grid presenter, selection bridge, card hand, action/menu presenter, UI button, cursor bridge, or collider/raycast target evidence.
- **Lanes**: `TabletopBoard`, `UiMenuOnly`, `CameraCursor`, `Sprite2D`, `ThirdPerson3D`

### Board Card Action Proof
- **Summary**: Run one rules-backed tabletop selection before adding card UX, AI turns, campaign flow, or networking.
- **Lanes**: `TabletopBoard`, `UiMenuOnly`, `CameraCursor`

### BoardPieceDefinition Visual Prefab
- **Summary**: Assign creator-owned art to each board piece definition so the proof uses imported content without hardcoding a game.

### Camera / Bounds
- **Summary**: Camera root, Cinemachine controller, physical target camera, camera profile, or bounds evidence.
- **Lanes**: `CameraCursor`, `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `TabletopBoard`

### Camera Cursor World Proof
- **Summary**: Run one camera, cursor, bounds, or world-surface proof before adding multi-target framing or cinematic polish.
- **Lanes**: `CameraCursor`, `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `TabletopBoard`

### Camera Follow And Bounds
- **Summary**: A camera or cursor control surface that makes the current route visible and keeps 2D proofs framed.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `CameraCursor`

### Camera Or Cursor Command
- **Summary**: A non-pawn or mixed route where the player first controls a camera, cursor, selector, board surface, or command UI.
- **Lanes**: `CameraCursor`, `TabletopNoPawn`, `UiMenu`

### CameraRigProfile Framing Fields
- **Summary**: Customize orthographic, zoom, damping, follow, and shake values to make the first route proof readable.

### CinemachineCameraRigController Camera Fields
- **Summary**: Assign camera rig, playfield, target camera, and Cinemachine references on the scene camera root.

### Combat Attack Proof
- **Summary**: A smallest attack, hit, shot, damage, or reaction path for a pawn, NPC/enemy, trap, turret, or command source.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `CameraCursor`

### Custom Object Effect Proof
- **Summary**: Run one custom object, feature, trigger, pickup, hazard, turret, trap, or service effect before treating it as a full system.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `TabletopBoard`

### Custom Object Or Feature Route
- **Summary**: Scene objects, feature modules, hazards, pickups, triggers, turrets, traps, or custom systems that are not the primary pawn.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `TabletopBoard`

### Enable Scoring Route
- **Summary**: Declare score or objective ownership before UI or services try to display it.

### Environment / Playfield
- **Summary**: World, board, arena, backdrop, collider, bounds, zone, spawn, or selectable playfield evidence.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `TabletopBoard`

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
- **Summary**: Assign the setup recipe that lists runtime patterns for the route.

### Gameplay Root
- **Summary**: Keep the scene setup anchored on one visible GameplaySessionBootstrap object.

### GameplaySessionBootstrap Session Definition
- **Summary**: Assign the session asset on the scene bootstrap so the active scene knows which route to start.

### GameSetupProfile Runtime Patterns
- **Summary**: Assign runtime pattern definitions that describe the supported route capabilities.

### Generated Content Proof
- **Summary**: Generate one inspectable output before making generated content required for progression.
- **Lanes**: `Sprite2D`, `TabletopBoard`, `CameraCursor`

### Hybrid / Custom Project
- **Summary**: A project-wide intent that combines ingredients across world, actor, action, UI, rules, or runtime lanes without accepting a named preset.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`, `TabletopNoPawn`, `UiMenu`, `CameraCursor`

### InputProfile Gameplay Action Names
- **Summary**: Customize action-name fields to match the project's Input Action Asset without renaming the whole input asset for Pyralis.

### Interaction Or Action Selection
- **Summary**: One selectable command surface such as an interact prompt, menu command, board space, card, cursor target, or pawn action.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `TabletopBoard`, `UiMenuOnly`, `CameraCursor`

### Network Ownership Proof
- **Summary**: Confirm the local proof first, then prove one host/client ownership path before expanding replication.
- **Lanes**: `Mixed`, `Sprite2D`, `ThirdPerson3D`

### Networking Authority Route
- **Summary**: Host/client/server, participant ownership, authority, network spawn, replicated state, and network-ready route proof.
- **Lanes**: `Mixed`, `Sprite2D`, `ThirdPerson3D`

### NPC / Enemy Actor Setup
- **Summary**: An authored enemy or NPC actor that can appear in the scene, detect or patrol, take damage, attack, and participate in an encounter.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`

### NPC And Enemy Actor Route
- **Summary**: Pawn or actor routes driven by AI, encounter, ambient, dialogue, vendor, quest, or enemy combat definitions.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`

### NPC Enemy Behavior Proof
- **Summary**: Run one NPC or enemy behavior proof before building encounter waves, boss phases, vendors, or broad AI systems.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`

### ParticipantDefinition Default Pawn
- **Summary**: Assign a PawnDefinition when this participant should spawn or control a pawn-backed body.

### Pawn Actor Route
- **Summary**: Participant-backed player, NPC, or simulated actor routes that spawn or control a pawn body.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`

### Pawn Brawler
- **Summary**: A pawn-backed action route where movement and close-range attacks are the first playable loop.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `Rigged3D`

### PawnDefinition Movement And Presentation Profiles
- **Summary**: Assign movement and presentation profiles so pawn feel and visuals stay designer-customizable.

### PawnDefinition Pawn Prefab
- **Summary**: Assign the prefab that contains PawnRoot and lane-specific runtime components.

### Pickups / Hazards / Enemies
- **Summary**: Pickup, hazard, enemy, encounter, arena, spawner, or custom feature object evidence.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `TabletopBoard`

### Runtime Service Ownership
- **Summary**: Keep runtime services owned by GameplaySessionBootstrap and PyralisGameplayLifetimeScope instead of hidden singleton lookups.

### Scene And Prefab Readiness
- **Summary**: Block Play Mode proof guidance until required scene objects, prefab modules, and inspector handoffs are clear.

### Scoring / Objectives
- **Summary**: Score, objective, timer, resource, result, win/loss service, or visible output evidence.
- **Lanes**: `UiMenuOnly`, `Sprite2D`, `ThirdPerson3D`, `TabletopBoard`

### Select Gameplay Session Bootstrap
- **Summary**: Choose the scene object that anchors the active Pyralis setup.

### SessionDefinition Default Game Mode
- **Summary**: Assign the game-rules asset that controls the playable loop for the session.

### Tabletop Board Card Route
- **Summary**: Board seats, board pieces, card hands, faction surfaces, turn order, phases, terminal conditions, and board action selection.
- **Lanes**: `TabletopBoard`, `UiMenuOnly`, `CameraCursor`

### Tabletop Runtime Contract
- **Summary**: Use board, piece, move-policy, turn-order, and action data without requiring pawn fields.

### Tabletop, Board, Or Card Project
- **Summary**: A no-pawn or mixed route where board state, card hands, action selection, turns, seats, factions, or rules resolution are the project center.
- **Lanes**: `TabletopNoPawn`, `UiMenu`, `CameraCursor`, `Sprite2D`

### TabletopBoardGridPresenter Board Fields
- **Summary**: Assign board, move policy, turn order, selection bridge, and board-space/piece prefabs for the smallest tabletop proof.

### TabletopTurnStatusPresenter Fields
- **Summary**: Assign the board presenter and TextMeshPro label that should show the active local seat during a tabletop proof.

### Tune Camera Framing
- **Summary**: Customize camera framing and bounds for the selected route.

### Tune Movement And Input Feel
- **Summary**: Customize movement profile, CharacterController or Rigidbody feel, and input names so the proof feels intentional.

### Tune Pawn Visuals And Collision
- **Summary**: Customize sprite/model, collider or CharacterController fit, pivot, sorting, billboard/rigged presentation, and visible pawn presentation.

### UI / HUD / Menus
- **Summary**: Canvas, EventSystem, HUD presenter, menu presenter, prompt, card hand, action buttons, or score/feedback panel evidence.
- **Lanes**: `UiMenuOnly`, `TabletopBoard`, `Sprite2D`, `ThirdPerson3D`

### UI / Menu First Project
- **Summary**: A route where menu commands, HUD, action selection, settings, results, dialogue, or UI-presented game state are the first authored surface.
- **Lanes**: `UiMenu`, `TabletopNoPawn`, `CameraCursor`

### UI And Scoring Feedback
- **Summary**: Visible route state such as score, health, prompt text, feedback, objective state, or menu/action labels.
- **Lanes**: `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `TabletopBoard`, `UiMenuOnly`

### UI HUD Menu Proof
- **Summary**: Run one UI, HUD, prompt, score, health, feedback, or menu event before building full navigation or result screens.
- **Lanes**: `UiMenuOnly`, `TabletopBoard`, `Sprite2D`, `ThirdPerson3D`

### UI HUD Or Menu Route
- **Summary**: Canvas, HUD, menu, prompt, health, score, feedback, inventory, dialogue, card hand, or route-selection surfaces.
- **Lanes**: `UiMenuOnly`, `TabletopBoard`, `Sprite2D`, `ThirdPerson3D`

### Visible Lifetime Scope
- **Summary**: Show the VContainer composition root on the gameplay object before Play Mode.

### World Camera And Scene Surface Route
- **Summary**: Camera, bounds, scene service, world trigger, board view, cursor view, or environmental route surfaces.
- **Lanes**: `CameraCursor`, `Sprite2D`, `Billboard2_5D`, `ThirdPerson3D`, `TabletopBoard`

---

