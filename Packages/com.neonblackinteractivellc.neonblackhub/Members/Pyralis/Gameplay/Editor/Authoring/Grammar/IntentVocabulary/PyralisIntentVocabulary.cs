using NeonBlack.Gameplay.Core.Contracts;
using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisIntentVocabulary
    {
        public static IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            return new[]
            {
                new PyralisAuthoringFact(
                    "intent.2d-side-view-action",
                    "2D Side-View Action",
                    PyralisAuthoringFactKind.RouteIntent,
                    PyralisAuthoringFactSourceKind.Convention,
                    PyralisAuthoringConfidence.Explicit,
                    "A Sprite2D route where authored pawns move on a side-view playfield with gravity, ledges, platforms, or jumping.",
                    "Use this when the creator wants a 2D side-scroller, platformer, brawler, runner, or action prototype without applying a preset.",
                    "One Sprite2D pawn spawns, receives input, moves horizontally, and proves the side-view movement surface. Verify by checking that the pawn lands on a platform and can jump successfully.",
                    goalTags: new[]
                    {
                        "Movement",
                        "JumpTraversal",
                        "Input",
                        "AnimationPresentation",
                        "Camera"
                    },
                    laneTags: new[] { RuntimeCapabilityLaneTag.Sprite2D.ToString() },
                    assignmentFields: new[]
                    {
                        "Intent capability filter -> 2D movement ingredients",
                        "PawnDefinition.pawnPrefab -> Sprite2D pawn prefab",
                        "PawnMovementProfile -> side-view movement and jump feel"
                    },
                    customizationMoments: new[]
                    {
                        "Choose side-view movement feel, jump height, gravity, and collider fit.",
                        "Map imported art and animation deliberately after the pawn route exists."
                    },
                    canWait: new[] { "HUD", "scoring", "enemies", "networking" },
                    relatedStableIds: new[]
                    {
                        "capability.2d-pawn-movement",
                        "capability.combat-projectile-proof",
                        "capability.camera-follow-bounds"
                    },
                    axioms: AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityVertical,
                    capability: AuthoringCapability.Movement | AuthoringCapability.Combat | AuthoringCapability.Input | AuthoringCapability.Animation | AuthoringCapability.Camera,
                    priority: AuthoringPriority.Primary,
                    expertAdvice: "Ensure your level geometry uses layers defined in the PawnMovementProfile's Ground Layer mask."),
                new PyralisAuthoringFact(
                    "intent.2d-top-down-plane",
                    "2D Top-Down / Free Movement",
                    PyralisAuthoringFactKind.RouteIntent,
                    PyralisAuthoringFactSourceKind.Convention,
                    PyralisAuthoringConfidence.Explicit,
                    "A Sprite2D route for top-down action, arena movement, or cursor-driven interaction without side-view gravity.",
                    "Use this for top-down free X/Y movement, twin-stick shooters, or arena-style interaction proofs.",
                    "One controlled body, cursor, or action surface moves on the 2D plane and proves camera/world bounds; arena-style proofs can then add one projectile or pickup event, one score change, and one HUD readout without treating jump gravity as required.",
                    goalTags: new[]
                    {
                        "Movement",
                        "Input",
                        "Camera",
                        "Interaction",
                        "Projectiles",
                        "Scoring",
                        "UiHud"
                    },
                    laneTags: new[] { RuntimeCapabilityLaneTag.Sprite2D.ToString(), RuntimeCapabilityLaneTag.CameraCursor.ToString() },
                    assignmentFields: new[]
                    {
                        "PawnMovementProfile -> top-down/free movement feel",
                        "InputProfile -> Move, Aim, Attack, Interact, or Custom rows",
                        "PlayfieldProfile -> legal movement bounds",
                        "CameraRigProfile -> top-down framing"
                    },
                    customizationMoments: new[]
                    {
                        "Choose whether Jump means nothing, dash, top-down hop, or a custom ability.",
                        "Tune free-movement speed, collider fit, targeting direction, playfield bounds, camera framing, and obstacle surfaces."
                    },
                    canWait: new[] { "side-view gravity ground", "platform jump tuning", "full enemy waves", "networking", "leaderboards", "full HUD polish" },
                    relatedStableIds: new[]
                    {
                        "capability.2d-pawn-movement",
                        "feature.actor.traversal.topdown-hop",
                        "capability.camera-follow-bounds",
                        "capability.combat-projectile-proof",
                        "capability.ui-scoring-feedback",
                        "proof.ui-hud-menu-event",
                        "route.custom-object-feature"
                    },
                    axioms: AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone,
                    capability: AuthoringCapability.Movement | AuthoringCapability.Input | AuthoringCapability.Combat | AuthoringCapability.Camera,
                    priority: AuthoringPriority.Primary,
                    expertAdvice: "Use PlayfieldProfile to define legal movement bounds. Use CameraRigProfile to decide how that playfield is framed on screen."),
                new PyralisAuthoringFact(
                    "intent.pawn-brawler",
                    "Pawn Brawler",
                    PyralisAuthoringFactKind.RouteIntent,
                    PyralisAuthoringFactSourceKind.Convention,
                    PyralisAuthoringConfidence.Explicit,
                    "A pawn-backed action route where movement and close-range attacks are the first playable loop.",
                    "Use this when the creator wants a brawler, fighter, arena action route, or enemy-driven pawn combat proof.",
                    "One pawn moves, triggers one attack signal, and proves visible cause/effect before expanding enemies, scoring, or combos.",
                    goalTags: new[]
                    {
                        "Movement",
                        "JumpTraversal",
                        "Combat",
                        "Input",
                        "AnimationPresentation"
                    },
                    laneTags: new[]
                    {
                        "Sprite2D",
                        "Billboard2_5D",
                        "Rigged3D"
                    },
                    requiredDefinitions: new[] { "ParticipantDefinition", "PawnDefinition", "CombatActionDefinition" },
                    requiredProfiles: new[] { "PawnMovementProfile", "PawnCombatProfile", "PawnPresentationProfile" },
                    assignmentFields: new[]
                    {
                        "SessionDefinition.defaultParticipants -> ParticipantDefinition",
                        "ParticipantDefinition.defaultPawn -> PawnDefinition",
                        "PawnDefinition.combatProfile -> PawnCombatProfile"
                    },
                    customizationMoments: new[]
                    {
                        "Tune movement speed, jump/air control where the lane supports it, attack timing, hit windows, and animation signals.",
                        "Creator validates movement feel, route taste, animation fit, and attack readability."
                    },
                    canWait: new[] { "full combo list", "enemy waves", "HUD polish", "score loop", "networking" },
                    relatedStableIds: new[]
                    {
                        "capability.2d-pawn-movement",
                        "capability.3d-pawn-movement",
                        "capability.combat-projectile-proof",
                        "capability.npc-enemy-setup",
                        "capability.ui-scoring-feedback"
                    },
                    axioms: AuthoringWorldAxiom.None,
                    capability: AuthoringCapability.Movement | AuthoringCapability.Combat | AuthoringCapability.Input | AuthoringCapability.Animation,
                    priority: AuthoringPriority.Primary,
                    expertAdvice: "Leverage PawnCombatProfile to define hitboxes and attack sequences efficiently."),
                new PyralisAuthoringFact(
                    "intent.2_5d-lane-arena",
                    "2.5D Lane / Arena Action",
                    PyralisAuthoringFactKind.RouteIntent,
                    PyralisAuthoringFactSourceKind.Convention,
                    PyralisAuthoringConfidence.Explicit,
                    "A lane, depth, or arena route where actors use 2.5D presentation and route-specific movement, camera, combat, or enemy behavior.",
                    "Use this when the project wants beat-em-up lane depth, arena positioning, billboard actors, or a hybrid 2D/3D camera relationship.",
                    "One actor moves through the intended lane or arena space and proves camera framing before deeper combat or encounter work. Verify by checking billboard rotation and depth-based sorting.",
                    goalTags: new[]
                    {
                        "Movement",
                        "Combat",
                        "Camera",
                        "NpcsEnemies",
                        "AnimationPresentation"
                    },
                    laneTags: new[] { "Billboard2_5D" },
                    assignmentFields: new[]
                    {
                        "PawnPresentationProfile -> Billboard2_5D",
                        "PawnMovementProfile -> lane/depth movement feel",
                        "PlayfieldProfile -> arena or lane movement bounds",
                        "CameraRigProfile -> arena or lane framing"
                    },
                    customizationMoments: new[]
                    {
                        "Choose lane depth, camera angle, actor scale, enemy spacing, and whether combat uses melee, projectiles, or both.",
                        "Creator validates route taste, readable depth, and movement/combat feel."
                    },
                    canWait: new[] { "full encounter gates", "boss phases", "networking", "shipping export gate" },
                    relatedStableIds: new[]
                    {
                        "capability.3d-pawn-movement",
                        "capability.combat-projectile-proof",
                        "capability.npc-enemy-setup",
                        "feature.actor.traversal.3d",
                        "feature.enemy.reaction",
                        "capability.camera-follow-bounds"
                    },
                    expertAdvice: "Use BillboardFacing3D for actors and ensure Sorting Group or depth-based Z-offsetting is used for correct visual layering."),
                new PyralisAuthoringFact(
                    "intent.3d-space-action",
                    "3D Space Action",
                    PyralisAuthoringFactKind.RouteIntent,
                    PyralisAuthoringFactSourceKind.Convention,
                    PyralisAuthoringConfidence.Explicit,
                    "A Rigged3D route where actors, cameras, interaction, enemies, projectiles, or traversal operate in 3D world space.",
                    "Use this when the project needs 3D movement, rigged presentation, 3D camera composition, or 3D-friendly feature modules.",
                    "One rigged or placeholder actor exists in 3D space, receives control or AI intent, and proves movement/camera/interaction before deeper systems.",
                    goalTags: new[]
                    {
                        "Movement",
                        "Camera",
                        "Interaction",
                        "NpcsEnemies",
                        "AnimationPresentation"
                    },
                    laneTags: new[] { "Rigged3D" },
                    assignmentFields: new[]
                    {
                        "PawnPresentationProfile -> Rigged3D",
                        "PawnTraversalProfile or movement profile -> 3D traversal feel",
                        "CameraRigProfile -> 3D follow/framing"
                    },
                    customizationMoments: new[]
                    {
                        "Choose controller feel, camera behavior, navigation space, animation rig fit, and interaction targeting.",
                        "Creator validates movement feel, route taste, and asset/design fit before promotion."
                    },
                    canWait: new[] { "network authority", "full AI behavior", "save/progression", "shipping export gate" },
                    relatedStableIds: new[]
                    {
                        "capability.3d-pawn-movement",
                        "capability.combat-projectile-proof",
                        "capability.npc-enemy-setup",
                        "feature.actor.traversal.3d",
                        "feature.actor.interaction",
                        "feature.enemy.reaction"
                    },
                    expertAdvice: "Choose between CharacterController or Rigidbody for actors; ensure ProBuilder or level meshes have correct collision layers."),
                new PyralisAuthoringFact(
                    "intent.camera-cursor-command",
                    "Camera Or Cursor Command",
                    PyralisAuthoringFactKind.RouteIntent,
                    PyralisAuthoringFactSourceKind.Convention,
                    PyralisAuthoringConfidence.Explicit,
                    "A non-pawn or mixed route where the player first controls a camera, cursor, selector, board surface, or command UI.",
                    "Use this when the game idea is tactical, tabletop, menu-command, camera-driven, or cursor-first instead of actor-body-first.",
                    "One selection surface drives one visible command, cursor movement, camera action, or board/card choice.",
                    goalTags: new[] { "Camera", "Tabletop" },
                    laneTags: new[]
                    {
                        "CameraCursor",
                        RuntimeCapabilityLaneTag.TabletopBoard.ToString(),
                        "UiMenu"
                    },
                    relatedStableIds: new[]
                    {
                        "capability.camera-follow-bounds",
                        "capability.interaction-action-selection"
                    },
                    expertAdvice: "Use PlayfieldProfile for playable bounds and CameraRigProfile for the view. Cursor hit-testing should use explicit interaction layers or surfaces."),
                new PyralisAuthoringFact(
                    "intent.tabletop-board-card",
                    "Tabletop, Board, Or Card Project",
                    PyralisAuthoringFactKind.RouteIntent,
                    PyralisAuthoringFactSourceKind.Convention,
                    PyralisAuthoringConfidence.Explicit,
                    "A no-pawn or mixed route where board state, card hands, action selection, turns, seats, factions, or rules resolution are the project center.",
                    "Use this when the project starts from a board/grid/table surface, card hand, tower-defense-like placement surface, tactics action, or turn command.",
                    "One selectable surface accepts or rejects one rules-backed action and shows a visible or inspectable state change.",
                    goalTags: new[]
                    {
                        "Tabletop",
                        "UiHud",
                        "Camera",
                        "Interaction",
                        "Projectiles"
                    },
                    laneTags: new[]
                    {
                        "TabletopNoPawn",
                        "UiMenu",
                        "CameraCursor",
                        "Sprite2D"
                    },
                    assignmentFields: new[]
                    {
                        "Intent capability filter -> tabletop/action ingredients",
                        "SessionDefinition.defaultParticipants -> seats, factions, hands, or command owners",
                        "ActionDefinition -> legal action or card command"
                    },
                    customizationMoments: new[]
                    {
                        "Choose board/grid shape, selectable surfaces, turn timing, card/command vocabulary, targeting rules, and visual state feedback.",
                        "Hybrid choices such as projectiles, towers, enemies, or pawns stay optional ingredients rather than bundled route generators."
                    },
                    canWait: new[] { "pawn actors", "full card UX", "campaign map", "networking" },
                    relatedStableIds: new[]
                    {
                        "proof.board-card-action",
                        "proof.action-selection",
                        "capability.interaction-action-selection",
                        "capability.ui-scoring-feedback"
                    },
                    axioms: AuthoringWorldAxiom.TurnBased,
                    capability: AuthoringCapability.Tabletop | AuthoringCapability.Camera | AuthoringCapability.Input | AuthoringCapability.TurnBased,
                    priority: AuthoringPriority.Primary,
                    expertAdvice: "Use LevelRegistry to manage different board/level configurations and SessionDefinition for rule overrides."),
                new PyralisAuthoringFact(
                    "intent.ui-menu-first",
                    "UI / Menu First Project",
                    PyralisAuthoringFactKind.RouteIntent,
                    PyralisAuthoringFactSourceKind.Convention,
                    PyralisAuthoringConfidence.Explicit,
                    "A route where menu commands, HUD, action selection, settings, results, dialogue, or UI-presented game state are the first authored surface.",
                    "Use this when the project begins with command choice, UI state, card/menu interaction, HUD feedback, or a non-scene-object first proof.",
                    "One UI or menu action changes visible game/session state without requiring a pawn body. Verify by clicking a button and seeing a 'Score Changed' or 'State Updated' HUD response.",
                    goalTags: new[]
                    {
                        "UiHud",
                        "Tabletop",
                        "Scoring",
                        "Interaction"
                    },
                    laneTags: new[] { "UiMenu", "TabletopNoPawn", "CameraCursor" },
                    assignmentFields: new[]
                    {
                        "Canvas / EventSystem -> UI surface",
                        "ActionDefinition -> command/action meaning",
                        "Presenter or binder component -> state display"
                    },
                    customizationMoments: new[]
                    {
                        "Choose UI composition, labels, navigation order, feedback timing, accessibility, and whether UI is screen-space or world-space.",
                        "Keep art/layout in the user's hands while facts describe required binders and proof targets."
                    },
                    canWait: new[] { "pawn prefab", "movement controller", "final art", "networking" },
                    relatedStableIds: new[]
                    {
                        "proof.ui-hud-menu-event",
                        "proof.action-selection",
                        "capability.ui-scoring-feedback"
                    },
                    expertAdvice: "Map UI events to session state changes via Presenter patterns to decouple layout from logic."),
                new PyralisAuthoringFact(
                    "intent.hybrid-custom-project",
                    "Hybrid / Custom Project",
                    PyralisAuthoringFactKind.RouteIntent,
                    PyralisAuthoringFactSourceKind.Convention,
                    PyralisAuthoringConfidence.Explicit,
                    "A project-wide intent that combines ingredients across world, actor, action, UI, rules, or runtime lanes without accepting a named preset.",
                    "Use this when the creator is exploring combinations such as tabletop plus projectiles, brawler plus RPG progression, card plus board, or custom systems.",
                    "One chosen ingredient chain has a small proof target; other selected ingredients remain visible as next options, cautions, or proof enhancers. Verify that your custom logic executes and produces at least one observable side effect.",
                    goalTags: new[]
                    {
                        "Movement",
                        "Combat",
                        "Projectiles",
                        "Tabletop",
                        "UiHud",
                        "Interaction",
                        "NpcsEnemies"
                    },
                    laneTags: new[]
                    {
                        "Sprite2D",
                        "Billboard2_5D",
                        "Rigged3D",
                        "TabletopNoPawn",
                        "UiMenu",
                        "CameraCursor"
                    },
                    assignmentFields: new[]
                    {
                        "Intent capability filter -> selected ingredients",
                        "FeatureModuleDefinition -> custom reusable systems when they graduate into Pyralis",
                        "Proof target facts -> choose the first small observable chain"
                    },
                    customizationMoments: new[]
                    {
                        "Choose which ingredient is the active authoring focus and which ingredients are intentionally deferred.",
                        "Mark custom systems with facts/contracts before calling them reusable Pyralis features."
                    },
                    canWait: new[] { "full route completion", "optional route contracts", "export promotion gate" },
                    relatedStableIds: new[]
                    {
                        "proof.custom-object-effect",
                        "proof.generated-content",
                        "proof.camera-cursor-world"
                    },
                    expertAdvice: "Start by proving one unique interaction loop before expanding into complex systems.")

            };
        }
    }

public readonly struct PyralisAuthoringAxiomGroup
    {
        public PyralisAuthoringAxiomGroup(string displayName, AuthoringWorldAxiom mask, params AuthoringWorldAxiom[] options)
        {
            DisplayName = displayName ?? string.Empty;
            Mask = mask;
            Options = options ?? Array.Empty<AuthoringWorldAxiom>();
        }

        public string DisplayName { get; }
        public AuthoringWorldAxiom Mask { get; }
        public AuthoringWorldAxiom[] Options { get; }
    }

    internal static class PyralisAuthoringVocabulary
    {
        private static readonly PyralisAuthoringAxiomGroup[] AxiomGroups =
        {
            new PyralisAuthoringAxiomGroup(
                "Dimensionality",
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Dimensions3D,
                AuthoringWorldAxiom.Dimensions2D,
                AuthoringWorldAxiom.Dimensions3D),
            new PyralisAuthoringAxiomGroup(
                "Physics Gravity",
                AuthoringWorldAxiom.GravityVertical | AuthoringWorldAxiom.GravityRadial | AuthoringWorldAxiom.GravityNone,
                AuthoringWorldAxiom.GravityVertical,
                AuthoringWorldAxiom.GravityRadial,
                AuthoringWorldAxiom.GravityNone),
            new PyralisAuthoringAxiomGroup(
                "Sequence Timeline",
                AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.TurnBased,
                AuthoringWorldAxiom.Realtime,
                AuthoringWorldAxiom.TurnBased),
            new PyralisAuthoringAxiomGroup(
                "Spatial Topology",
                AuthoringWorldAxiom.BoundedSpace | AuthoringWorldAxiom.WrappedSpace | AuthoringWorldAxiom.InfiniteSpace,
                AuthoringWorldAxiom.BoundedSpace,
                AuthoringWorldAxiom.WrappedSpace,
                AuthoringWorldAxiom.InfiniteSpace),
            new PyralisAuthoringAxiomGroup(
                "Networking",
                AuthoringWorldAxiom.Networked,
                AuthoringWorldAxiom.Networked)
        };

        private static readonly Dictionary<AuthoringWorldAxiom, AxiomMetadata> AxiomMetadataByValue =
            new Dictionary<AuthoringWorldAxiom, AxiomMetadata>
            {
                { AuthoringWorldAxiom.Dimensions2D, new AxiomMetadata("2D Logic", "The world logic operates in a flat 2D plane (XY or XZ).") },
                { AuthoringWorldAxiom.Dimensions3D, new AxiomMetadata("3D Logic", "The world logic operates in a full 3D coordinate space (XYZ).") },
                { AuthoringWorldAxiom.GravityVertical, new AxiomMetadata("Vertical Gravity", "Standard downward gravity vector affecting mechanics.") },
                { AuthoringWorldAxiom.GravityNone, new AxiomMetadata("Zero Gravity", "No inherent gravity vector; movement mechanics are unconstrained.") },
                { AuthoringWorldAxiom.GravityRadial, new AxiomMetadata("Radial Gravity", "Gravity pulls toward a specific point or center in the world.") },
                { AuthoringWorldAxiom.Realtime, new AxiomMetadata("Real-time Sequence", "Continuous time flow; mechanical actions resolve immediately.") },
                { AuthoringWorldAxiom.TurnBased, new AxiomMetadata("Turn-based Sequence", "Discrete time steps; mechanics are phased or queued.") },
                { AuthoringWorldAxiom.BoundedSpace, new AxiomMetadata("Bounded Topology", "The physical world has explicit limits or walls.") },
                { AuthoringWorldAxiom.WrappedSpace, new AxiomMetadata("Wrapped Topology", "Mechanically, moving past one edge teleports to the opposite edge.") },
                { AuthoringWorldAxiom.InfiniteSpace, new AxiomMetadata("Infinite Topology", "The physical world has no inherent mechanical bounds or limits.") },
                { AuthoringWorldAxiom.Networked, new AxiomMetadata("Networked", "Game state and mechanics are replicated across a network.") }
            };

        private static readonly Dictionary<AuthoringCapability, CapabilityMetadata> CapabilityMetadataByValue =
            new Dictionary<AuthoringCapability, CapabilityMetadata>
            {
                { AuthoringCapability.Setup, new CapabilityMetadata("Setup", "Foundational scene and bootstrap configuration.") },
                { AuthoringCapability.Session, new CapabilityMetadata("Session", "High-level session orchestration and network authority.") },
                { AuthoringCapability.Rules, new CapabilityMetadata("Rules", "Game-mode specific rulesets, win/loss conditions, and timers.") },
                { AuthoringCapability.Participants, new CapabilityMetadata("Participants", "Player seats, AI slots, and input ownership.") },
                { AuthoringCapability.Scoring, new CapabilityMetadata("Scoring", "Points, resources, leaderboards, and objective tracking.") },
                { AuthoringCapability.Input, new CapabilityMetadata("Input", "Human and AI control schemes and event routing.") },
                { AuthoringCapability.UI, new CapabilityMetadata("UI", "User interface, menus, and HUD presentation.") },
                { AuthoringCapability.Movement, new CapabilityMetadata("Movement", "General movement archetype and intent definitions.") },
                { AuthoringCapability.KineticMotor2D, new CapabilityMetadata("2D Kinetic Motor", "Low-level 2D physical motor implementation.") },
                { AuthoringCapability.KineticMotor3D, new CapabilityMetadata("3D Kinetic Motor", "Low-level 3D motor implementation.") },
                { AuthoringCapability.Steering2D, new CapabilityMetadata("2D Steering", "Pathfinding and navigation for 2D actors.") },
                { AuthoringCapability.Steering3D, new CapabilityMetadata("3D Steering", "Pathfinding and navigation for 3D actors.") },
                { AuthoringCapability.Traversal, new CapabilityMetadata("Traversal", "World interaction features like ledge-climb, ladders, and jumping.") },
                { AuthoringCapability.Combat, new CapabilityMetadata("Combat", "General combat systems and weapon logic.") },
                { AuthoringCapability.CombatState, new CapabilityMetadata("Combat State", "Health, damage tracking, and actor life-cycle state.") },
                { AuthoringCapability.CombatSensors, new CapabilityMetadata("Hit Detection", "Hitboxes, hurtboxes, overlap checks, and collision-based combat events.") },
                { AuthoringCapability.MeleeFlow, new CapabilityMetadata("Melee Flow", "Attack sequencing, combos, and melee state management.") },
                { AuthoringCapability.RangedFlow, new CapabilityMetadata("Ranged Flow", "Projectile sequencing, reloading, and targeting logic.") },
                { AuthoringCapability.TacticsAggressive, new CapabilityMetadata("Aggressive Tactics", "AI decision trees for charging, flanking, and attacking.") },
                { AuthoringCapability.TacticsDefensive, new CapabilityMetadata("Defensive Tactics", "AI decision trees for guarding, retreating, and kiting.") },
                { AuthoringCapability.Animation, new CapabilityMetadata("Animation", "Visual state machines and skeletal deformation.") },
                { AuthoringCapability.VFX, new CapabilityMetadata("VFX", "Particle systems, post-processing, and shader effects.") },
                { AuthoringCapability.Tabletop, new CapabilityMetadata("Tabletop", "Board game logic, piece management, and move policies.") },
                { AuthoringCapability.Grid, new CapabilityMetadata("Grid", "Coordinate systems, cell properties, and spatial queries.") },
                { AuthoringCapability.TurnBased, new CapabilityMetadata("Turn Based", "Phase management, action queues, and initiative.") },
                { AuthoringCapability.Stats, new CapabilityMetadata("Stats", "Attributes, modifiers, and character progression systems.") },
                { AuthoringCapability.Inventory, new CapabilityMetadata("Inventory", "Item storage, equipment, and resource management.") },
                { AuthoringCapability.Dialogue, new CapabilityMetadata("Dialogue", "Narrative flow, branching conversations, and event nodes.") },
                { AuthoringCapability.Puzzle, new CapabilityMetadata("Puzzle", "Logic gates, triggers, and state-based world interactions.") },
                { AuthoringCapability.Rpg, new CapabilityMetadata("RPG", "General role-playing systems.") },
                { AuthoringCapability.Quests, new CapabilityMetadata("Quests", "Quest tracking, objective management, and reward systems.") },
                { AuthoringCapability.Vendors, new CapabilityMetadata("Vendors", "Shop logic, trading interfaces, and currency exchange.", "https://docs.neonblack.com/pyralis/vendors") },
                { AuthoringCapability.SkillTree, new CapabilityMetadata("Skill Tree", "Abilities, unlock paths, and specialized talent trees.") },
                { AuthoringCapability.Progression, new CapabilityMetadata("Progression", "Experience points, leveling, and milestone tracking.") },
                { AuthoringCapability.Camera, new CapabilityMetadata("Camera", "Framing, following, and world containment boundaries.") },
                { AuthoringCapability.Environment, new CapabilityMetadata("Environment", "World geometry, lighting, and static decoration.") },
                { AuthoringCapability.Audio, new CapabilityMetadata("Audio", "Soundscapes, spatial audio, and music management.") },
                { AuthoringCapability.Networking, new CapabilityMetadata("Networking", "State synchronization, authority, and multiplayer connectivity.") }
            };

        public static IReadOnlyList<PyralisAuthoringAxiomGroup> GetAxiomGroups()
        {
            return AxiomGroups;
        }

        public static bool HasCompleteCoreAxioms(AuthoringWorldAxiom axioms)
        {
            for (int i = 0; i < AxiomGroups.Length; i++)
            {
                PyralisAuthoringAxiomGroup group = AxiomGroups[i];
                if (group.Mask == AuthoringWorldAxiom.Networked)
                    continue;

                if ((axioms & group.Mask) == 0)
                    return false;
            }

            return true;
        }

        public static string GetAxiomDisplayName(AuthoringWorldAxiom axiom)
        {
            if (axiom == AuthoringWorldAxiom.None)
                return "General";

            return AxiomMetadataByValue.TryGetValue(axiom, out AxiomMetadata meta)
                ? meta.DisplayName
                : axiom.ToString();
        }

        public static string GetAxiomTooltip(AuthoringWorldAxiom axiom)
        {
            return AxiomMetadataByValue.TryGetValue(axiom, out AxiomMetadata meta)
                ? meta.Tooltip
                : "A reflective world axiom.";
        }

        public static string GetCapabilityDisplayName(AuthoringCapability capability)
        {
            if (capability == AuthoringCapability.None)
                return "General";

            return CapabilityMetadataByValue.TryGetValue(capability, out CapabilityMetadata meta)
                ? meta.DisplayName
                : AuthoringCapabilityRegistry.PrettifyTypeName(capability.ToString());
        }

        public static string GetCapabilityTooltip(AuthoringCapability capability)
        {
            return CapabilityMetadataByValue.TryGetValue(capability, out CapabilityMetadata meta)
                ? meta.Tooltip
                : "A reflective engine capability.";
        }

        public static string GetCapabilityDocumentationUrl(AuthoringCapability capability)
        {
            return CapabilityMetadataByValue.TryGetValue(capability, out CapabilityMetadata meta)
                ? meta.DocumentationUrl
                : string.Empty;
        }

        public static string GetCapabilityExpertAdvice(AuthoringCapability capability)
        {
            return CapabilityMetadataByValue.TryGetValue(capability, out CapabilityMetadata meta)
                ? meta.ExpertAdvice
                : string.Empty;
        }

        public static string GetCapabilityHygieneAdvice(AuthoringCapability capability)
        {
            return CapabilityMetadataByValue.TryGetValue(capability, out CapabilityMetadata meta) && !string.IsNullOrWhiteSpace(meta.HygieneAdvice)
                ? meta.HygieneAdvice
                : $"Ensure your scripts are tagged with [AuthoringContract(Capability = AuthoringCapability.{capability})].";
        }

        private readonly struct AxiomMetadata
        {
            public AxiomMetadata(string displayName, string tooltip)
            {
                DisplayName = displayName;
                Tooltip = tooltip;
            }

            public string DisplayName { get; }
            public string Tooltip { get; }
        }

        private readonly struct CapabilityMetadata
        {
            public CapabilityMetadata(string displayName, string tooltip, string documentationUrl = "", string expertAdvice = "", string hygieneAdvice = "")
            {
                DisplayName = displayName;
                Tooltip = tooltip;
                DocumentationUrl = documentationUrl;
                ExpertAdvice = expertAdvice;
                HygieneAdvice = hygieneAdvice;
            }

            public string DisplayName { get; }
            public string Tooltip { get; }
            public string DocumentationUrl { get; }
            public string ExpertAdvice { get; }
            public string HygieneAdvice { get; }
        }
    }

}
