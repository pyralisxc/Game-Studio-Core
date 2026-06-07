using System.Collections.Generic;
using System.Text;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Tabletop;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public enum PyralisSetupFlowStepStatus
    {
        Ready,
        Missing,
        Blocked,
        Recommended,
        Optional
    }

    public enum PyralisSetupFlowActionKind
    {
        None,
        SelectObject,
        PingObject,
        AddLifetimeScope,
        RestoreFirstSceneDefaults
    }

    public enum PyralisSetupFlowStepId
    {
        Unknown,
        SelectGameplaySessionBootstrap,
        GameplayRoot,
        VisibleLifetimeScope,
        FirstSceneDefaults,
        RuntimeServiceOwnership,
        AssignSessionDefinition,
        AssignDefaultGameMode,
        AssignSetupProfile,
        AddRuntimePatterns,
        AssignDefaultParticipants,
        AssignParticipantPawn,
        AssignInputProfile,
        AssignSpawnPoints,
        AssignCameraRig,
        AssignPlayerInputManager,
        TuneCameraFraming,
        TunePawnVisualsAndCollision,
        TuneMovementAndInputFeel,
        AssignPlayfieldProfile,
        EnableScoringRoute,
        AssignGameplayStateService,
        AssignCameraBoundsService,
        AssignScoreService,
        AddHudOrMenuSurface,
        AddProjectileLauncher,
        TabletopRuntimeContract,
        TabletopSelectionSurface,
        SceneAndPrefabReadiness
    }

    public enum PyralisSetupFlowWorkIntent
    {
        Foundation,
        RequiredSetup,
        ProofEnhancer,
        FeatureCard
    }

    public sealed class PyralisSetupFlowStep
    {
        public PyralisSetupFlowStep(
            string label,
            PyralisSetupFlowStepStatus status,
            string message,
            Object referencedObject = null,
            PyralisSetupFlowActionKind actionKind = PyralisSetupFlowActionKind.None,
            PyralisSetupFlowStepId stepId = PyralisSetupFlowStepId.Unknown,
            PyralisSetupFlowWorkIntent workIntent = PyralisSetupFlowWorkIntent.RequiredSetup,
            PyralisAuthoringNativeAction? nativeAction = null)
        {
            Label = label;
            Status = status;
            Message = message;
            ReferencedObject = referencedObject;
            ActionKind = actionKind;
            StepId = stepId;
            WorkIntent = workIntent == PyralisSetupFlowWorkIntent.RequiredSetup && stepId != PyralisSetupFlowStepId.Unknown
                ? PyralisSetupFlowGuidance.GetDefaultWorkIntent(stepId)
                : workIntent;
            NativeAction = nativeAction ?? PyralisSetupFlowGuidance.GetNativeAction(stepId, message);
        }

        public PyralisSetupFlowStepId StepId { get; }
        public string Label { get; }
        public PyralisSetupFlowStepStatus Status { get; }
        public string Message { get; }
        public Object ReferencedObject { get; }
        public PyralisSetupFlowActionKind ActionKind { get; }
        public PyralisSetupFlowWorkIntent WorkIntent { get; }
        public PyralisAuthoringNativeAction? NativeAction { get; }

        public bool IsRequiredIssue => Status == PyralisSetupFlowStepStatus.Missing || Status == PyralisSetupFlowStepStatus.Blocked;
    }

    public static class PyralisSetupFlowGuidance
    {
        public static IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            List<PyralisAuthoringFact> facts = new List<PyralisAuthoringFact>();
            AddSetupFact(facts, PyralisSetupFlowStepId.SelectGameplaySessionBootstrap, "Select Gameplay Session Bootstrap", "Choose the scene object that anchors the active Pyralis setup.", "Core setup selection");
            AddSetupFact(facts, PyralisSetupFlowStepId.GameplayRoot, "Gameplay Root", "Keep the scene setup anchored on one visible GameplaySessionBootstrap object.", "Core setup chain");
            AddSetupFact(facts, PyralisSetupFlowStepId.VisibleLifetimeScope, "Visible Lifetime Scope", "Show the VContainer composition root on the gameplay object before Play Mode.", "Core setup chain");
            AddSetupFact(facts, PyralisSetupFlowStepId.FirstSceneDefaults, "First Scene Defaults", "Use first-scene defaults so core services and scene injection are predictable while authoring.", "Core setup chain");
            AddSetupFact(facts, PyralisSetupFlowStepId.RuntimeServiceOwnership, "Runtime Service Ownership", "Keep runtime services owned by GameplaySessionBootstrap and PyralisGameplayLifetimeScope instead of hidden singleton lookups.", "Core setup chain");
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignSessionDefinition, "Assign Session Definition", "Create or assign the session asset that owns game mode and default participants.", "Core setup chain");
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignDefaultGameMode, "Assign Default Game Mode", "Create or assign the game-rules asset for the session.", "Core setup chain");
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignSetupProfile, "Assign Setup Profile", "Create or assign the setup recipe that lists runtime patterns.", "Core setup chain");
            AddSetupFact(facts, PyralisSetupFlowStepId.AddRuntimePatterns, "Add Runtime Patterns", "Assign runtime pattern definitions that describe the current route.", "Capability setup");
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignDefaultParticipants, "Assign Default Participants", "Create or assign participant definitions for players, seats, factions, or command owners.", "Participant setup");
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignParticipantPawn, "Assign Participant Pawn", "Assign a PawnDefinition and prefab only when the selected route is pawn-backed.", "Pawn-backed movement route", new[] { "capability.2d-pawn-movement", "capability.3d-pawn-movement", "proof.1p-pawn-movement" });
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignInputProfile, "Assign Input Profile", "Assign input mapping when participant input drives pawn movement or actions.", "Pawn-backed movement route", new[] { "capability.2d-pawn-movement", "capability.3d-pawn-movement", "proof.1p-pawn-movement" });
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignSpawnPoints, "Assign Spawn Points", "Place spawn Transforms so pawn-backed participants can enter the scene predictably.", "Pawn-backed movement route", new[] { "capability.2d-pawn-movement", "capability.3d-pawn-movement", "proof.1p-pawn-movement" });
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignCameraRig, "Assign Camera Rig", "Create or assign a camera rig that can frame the first proof.", "Camera and first-proof visibility", new[] { "capability.camera-follow-bounds", "capability.2d-pawn-movement", "capability.3d-pawn-movement", "proof.1p-pawn-movement" });
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignPlayerInputManager, "Assign Player Input Manager", "Use PlayerInputManager only when local join or explicit multi-player input ownership is part of the proof.", "Input and local join");
            AddSetupFact(facts, PyralisSetupFlowStepId.TuneCameraFraming, "Tune Camera Framing", "Customize camera framing and bounds for the selected route.", "Camera and first-proof visibility", new[] { "capability.camera-follow-bounds" });
            AddSetupFact(facts, PyralisSetupFlowStepId.TunePawnVisualsAndCollision, "Tune Pawn Visuals And Collision", "Customize sprite/model, collider or CharacterController fit, pivot, sorting, billboard/rigged presentation, and visible pawn presentation.", "Pawn-backed movement route", new[] { "capability.2d-pawn-movement", "capability.3d-pawn-movement", "proof.1p-pawn-movement" });
            AddSetupFact(facts, PyralisSetupFlowStepId.TuneMovementAndInputFeel, "Tune Movement And Input Feel", "Customize movement profile, CharacterController or Rigidbody feel, and input names so the proof feels intentional.", "Pawn-backed movement route", new[] { "capability.2d-pawn-movement", "capability.3d-pawn-movement", "proof.1p-pawn-movement" });
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignPlayfieldProfile, "Assign Playfield Profile", "Create or assign authored playfield bounds and lane rules when the route needs them.", "World and camera support");
            AddSetupFact(facts, PyralisSetupFlowStepId.EnableScoringRoute, "Enable Scoring Route", "Declare score or objective ownership before UI or services try to display it.", "Scoring route", new[] { "capability.ui-scoring-feedback" });
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignGameplayStateService, "Assign Gameplay State Service", "Assign a scene or composition service when gameplay state is route-owned.", "State route");
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignCameraBoundsService, "Assign Camera Bounds Service", "Connect camera bounds to the active setup when 2D framing, spawners, hazards, pickups, or world limits rely on them.", "Camera and world support", new[] { "capability.camera-follow-bounds" });
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignScoreService, "Assign Score Service", "Create or assign a concrete session score service when scoring is part of the route.", "Scoring route", new[] { "capability.ui-scoring-feedback" });
            AddSetupFact(facts, PyralisSetupFlowStepId.AddHudOrMenuSurface, "Add HUD / UI Surface", "Create or assign visible UI surfaces for prompts, feedback, health, score, menus, or route panels.", "UI route", new[] { "capability.ui-scoring-feedback", "capability.interaction-action-selection" });
            AddSetupFact(facts, PyralisSetupFlowStepId.AddProjectileLauncher, "Assign Projectile Launcher Or Hitbox Source", "Create or assign a hitbox, projectile launcher, enemy attack, weapon mount, trap, turret, or encounter source.", "Projectile/combat/enemy route", new[] { "capability.combat-projectile-proof", "capability.npc-enemy-setup" });
            AddSetupFact(facts, PyralisSetupFlowStepId.TabletopRuntimeContract, "Tabletop Runtime Contract", "Use board, piece, move-policy, turn-order, and action data without requiring pawn fields.", "Tabletop/no-pawn route", new[] { "capability.interaction-action-selection", "proof.board-card-action" });
            AddSetupFact(facts, PyralisSetupFlowStepId.TabletopSelectionSurface, "Assign Tabletop Selection Surface", "Create or assign the board, card, cursor, or action-selection surface that makes one no-pawn proof selectable in Play Mode.", "Tabletop/no-pawn route", new[] { "capability.interaction-action-selection", "proof.board-card-action" });
            AddSetupFact(facts, PyralisSetupFlowStepId.SceneAndPrefabReadiness, "Scene And Prefab Readiness", "Block Play Mode proof guidance until required scene objects, prefab modules, and inspector handoffs are clear.", "First-proof gate", new[] { "proof.1p-pawn-movement" });
            return facts;
        }

        public static PyralisSetupFlowWorkIntent GetDefaultWorkIntent(PyralisSetupFlowStepId stepId)
        {
            switch (stepId)
            {
                case PyralisSetupFlowStepId.GameplayRoot:
                case PyralisSetupFlowStepId.VisibleLifetimeScope:
                case PyralisSetupFlowStepId.FirstSceneDefaults:
                case PyralisSetupFlowStepId.RuntimeServiceOwnership:
                    return PyralisSetupFlowWorkIntent.Foundation;
                case PyralisSetupFlowStepId.TuneCameraFraming:
                case PyralisSetupFlowStepId.TunePawnVisualsAndCollision:
                case PyralisSetupFlowStepId.TuneMovementAndInputFeel:
                case PyralisSetupFlowStepId.AssignPlayfieldProfile:
                case PyralisSetupFlowStepId.AssignCameraBoundsService:
                case PyralisSetupFlowStepId.TabletopSelectionSurface:
                    return PyralisSetupFlowWorkIntent.ProofEnhancer;
                case PyralisSetupFlowStepId.EnableScoringRoute:
                case PyralisSetupFlowStepId.AssignGameplayStateService:
                case PyralisSetupFlowStepId.AssignScoreService:
                case PyralisSetupFlowStepId.AddHudOrMenuSurface:
                case PyralisSetupFlowStepId.AddProjectileLauncher:
                    return PyralisSetupFlowWorkIntent.FeatureCard;
                default:
                    return PyralisSetupFlowWorkIntent.RequiredSetup;
            }
        }

        public static string GetStableId(PyralisSetupFlowStepId stepId)
        {
            switch (stepId)
            {
                case PyralisSetupFlowStepId.SelectGameplaySessionBootstrap: return "setup.select-gameplay-session-bootstrap";
                case PyralisSetupFlowStepId.GameplayRoot: return "setup.gameplay-root";
                case PyralisSetupFlowStepId.VisibleLifetimeScope: return "setup.visible-lifetime-scope";
                case PyralisSetupFlowStepId.FirstSceneDefaults: return "setup.first-scene-defaults";
                case PyralisSetupFlowStepId.RuntimeServiceOwnership: return "setup.runtime-service-ownership";
                case PyralisSetupFlowStepId.AssignSessionDefinition: return "setup.assign-session-definition";
                case PyralisSetupFlowStepId.AssignDefaultGameMode: return "setup.assign-default-game-mode";
                case PyralisSetupFlowStepId.AssignSetupProfile: return "setup.assign-setup-profile";
                case PyralisSetupFlowStepId.AddRuntimePatterns: return "setup.add-runtime-patterns";
                case PyralisSetupFlowStepId.AssignDefaultParticipants: return "setup.assign-default-participants";
                case PyralisSetupFlowStepId.AssignParticipantPawn: return "setup.assign-participant-pawn";
                case PyralisSetupFlowStepId.AssignInputProfile: return "setup.assign-input-profile";
                case PyralisSetupFlowStepId.AssignSpawnPoints: return "setup.assign-spawn-points";
                case PyralisSetupFlowStepId.AssignCameraRig: return "setup.assign-camera-rig";
                case PyralisSetupFlowStepId.AssignPlayerInputManager: return "setup.assign-player-input-manager";
                case PyralisSetupFlowStepId.TuneCameraFraming: return "setup.tune-camera-framing";
                case PyralisSetupFlowStepId.TunePawnVisualsAndCollision: return "setup.tune-pawn-visuals-and-collision";
                case PyralisSetupFlowStepId.TuneMovementAndInputFeel: return "setup.tune-movement-and-input-feel";
                case PyralisSetupFlowStepId.AssignPlayfieldProfile: return "setup.assign-playfield-profile";
                case PyralisSetupFlowStepId.EnableScoringRoute: return "setup.enable-scoring-route";
                case PyralisSetupFlowStepId.AssignGameplayStateService: return "setup.assign-gameplay-state-service";
                case PyralisSetupFlowStepId.AssignCameraBoundsService: return "setup.assign-camera-bounds-service";
                case PyralisSetupFlowStepId.AssignScoreService: return "setup.assign-score-service";
                case PyralisSetupFlowStepId.AddHudOrMenuSurface: return "setup.add-hud-or-menu-surface";
                case PyralisSetupFlowStepId.AddProjectileLauncher: return "setup.add-projectile-launcher";
                case PyralisSetupFlowStepId.TabletopRuntimeContract: return "setup.tabletop-runtime-contract";
                case PyralisSetupFlowStepId.TabletopSelectionSurface: return "setup.tabletop-selection-surface";
                case PyralisSetupFlowStepId.SceneAndPrefabReadiness: return "setup.scene-prefab-readiness";
                default: return string.Empty;
            }
        }

        private static void AddSetupFact(
            List<PyralisAuthoringFact> facts,
            PyralisSetupFlowStepId stepId,
            string displayName,
            string summary,
            string routeRelevance,
            string[] relatedStableIds = null)
        {
            PyralisAuthoringNativeAction? nativeAction = GetNativeAction(stepId, string.Empty);
            PyralisAuthoringNativeAction[] nativeActions = nativeAction.HasValue
                ? new[] { nativeAction.Value }
                : System.Array.Empty<PyralisAuthoringNativeAction>();

            facts.Add(new PyralisAuthoringFact(
                GetStableId(stepId),
                displayName,
                PyralisAuthoringFactKind.SetupNode,
                PyralisAuthoringFactSourceKind.SetupFlow,
                PyralisAuthoringConfidence.Explicit,
                summary,
                routeRelevance,
                string.Empty,
                nativeActions: nativeActions,
                workIntent: GetDefaultWorkIntent(stepId).ToString(),
                relatedStableIds: relatedStableIds));
        }

        public static PyralisAuthoringNativeAction? GetNativeAction(PyralisSetupFlowStepId stepId, string message)
        {
            switch (stepId)
            {
                case PyralisSetupFlowStepId.SelectGameplaySessionBootstrap:
                    return new PyralisAuthoringNativeAction(
                        "Create or select",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "Gameplay Root",
                        "right-click -> Create Empty, name it Gameplay Root, then use Inspector -> Add Component -> GameplaySessionBootstrap",
                        "Overview shows Gameplay Root as the active setup");
                case PyralisSetupFlowStepId.AssignSessionDefinition:
                    return new PyralisAuthoringNativeAction(
                        "Create",
                        PyralisAuthoringActionSurface.ProjectWindow,
                        "the opened setup folder",
                        "choose or create a project-owned setup folder for this proof, keep imported art folders separate, then right-click inside it -> Create -> NeonBlack -> Definitions -> Session Definition, then drag it into GameplaySessionBootstrap > Session Definition or use the field's object picker circle",
                        "the Session row is ready");
                case PyralisSetupFlowStepId.AssignDefaultGameMode:
                    return new PyralisAuthoringNativeAction(
                        "Create",
                        PyralisAuthoringActionSurface.ProjectWindow,
                        "the opened setup folder",
                        "choose or create the proof setup folder in the Project content pane first, keep imported art folders separate, then right-click inside it -> Create -> NeonBlack -> Definitions -> Game Mode Definition, then select/open the SessionDefinition asset and assign Default Game Mode by drag/drop or the field's object picker circle",
                        "the Game Rules row is ready");
                case PyralisSetupFlowStepId.AssignSetupProfile:
                    return new PyralisAuthoringNativeAction(
                        "Create",
                        PyralisAuthoringActionSurface.ProjectWindow,
                        "the opened setup folder",
                        "choose or create the proof setup folder in the Project content pane first, keep imported art folders separate, then right-click inside it -> Create -> NeonBlack -> Profiles -> Game Setup Profile, then select/open the GameModeDefinition asset and assign Setup Profile by drag/drop or the field's object picker circle",
                        "the Setup Recipe row is ready");
                case PyralisSetupFlowStepId.AddRuntimePatterns:
                    return new PyralisAuthoringNativeAction(
                        "Assign",
                        PyralisAuthoringActionSurface.Inspector,
                        "the GameSetupProfile",
                        "Runtime Capabilities; choose the route family in Capability To Add, click Add Capability, then use the generated or assigned runtime pattern only when its metadata matches the route",
                        "Capability Patterns are ready");
                case PyralisSetupFlowStepId.AssignDefaultParticipants:
                    return new PyralisAuthoringNativeAction(
                        "Create",
                        PyralisAuthoringActionSurface.ProjectWindow,
                        "the opened setup folder",
                        "choose or create the proof setup folder in the Project content pane first, keep imported art folders separate, then right-click inside it -> Create -> NeonBlack -> Definitions -> Participant Definition, configure player/seat/input intent, then select/open the SessionDefinition asset, add a Default Participants slot, and assign it by drag/drop or the slot's object picker circle",
                        "Players / Seats is ready");
                case PyralisSetupFlowStepId.AssignParticipantPawn:
                    return GetPawnNativeAction(message);
                case PyralisSetupFlowStepId.AssignInputProfile:
                    return new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.Inspector,
                        "SessionDefinition or ParticipantDefinition",
                        "InputProfile; for the beginner path set Actions to Assets/InputSystem_Actions.inputactions, keep Primary Action Map as Player, then add/remove Gameplay Action rows for the features this pawn uses",
                        "InputProfile actions can reach the pawn input module");
                case PyralisSetupFlowStepId.AssignSpawnPoints:
                    return new PyralisAuthoringNativeAction(
                        "Create",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "Gameplay Root or a Playfield Root",
                        "right-click -> Create Empty, name it SpawnPoint_1, position it, then drag its Transform into GameplaySessionBootstrap > Spawn Points",
                        "the pawn route has one spawn point per default participant");
                case PyralisSetupFlowStepId.AssignCameraRig:
                    return new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "Camera Root",
                        "keep or create exactly one enabled physical Unity Camera for the shared proof, usually the default Main Camera; right-click -> Create Empty, name it Camera Root; add CinemachineCameraRigController; create GameObject -> Cinemachine -> Cinemachine Camera under Camera Root or elsewhere if assigned explicitly; Unity usually adds Cinemachine Brain to the physical Main Camera when this first Cinemachine Camera is created; assign that Cinemachine Camera as Shared Camera Behaviour; verify the physical Main Camera keeps the MainCamera tag and Cinemachine Brain, then assign it as Target Camera; disable or remove accidental extra physical Camera objects only when they were created by mistake; keep intentional overlay, split-screen, minimap, or render-texture cameras; then drag the Camera Root object into GameplaySessionBootstrap > Camera Rig Controller",
                        "the Pyralis camera route is the single camera setup path");
                case PyralisSetupFlowStepId.AssignCameraBoundsService:
                    return new PyralisAuthoringNativeAction(
                        "Assign",
                        PyralisAuthoringActionSurface.Inspector,
                        "GameplaySessionBootstrap",
                        "the same CinemachineCameraRigController in Camera Rig Controller; only use Camera Bounds Source for a specialized custom ICameraBoundsProvider",
                        "2D movement, spawners, hazards, pickups, and framing share Cinemachine-backed visible bounds");
                case PyralisSetupFlowStepId.AssignPlayfieldProfile:
                    return new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.ProjectWindow,
                        "the opened setup folder and GameModeDefinition",
                        "Create -> NeonBlack -> Profiles -> Playfield Profile, tune bounds/lane rules, then select/open the GameModeDefinition asset and assign Playfield Profile",
                        "the route has authored world bounds instead of relying on scene defaults");
                case PyralisSetupFlowStepId.EnableScoringRoute:
                    return new PyralisAuthoringNativeAction(
                        "Enable",
                        PyralisAuthoringActionSurface.Inspector,
                        "GameModeDefinition",
                        "Enable Score when the selected capability pattern expects score, objectives, timers, resources, or result tracking",
                        "the scoring route is declared before services or HUD try to display it");
                case PyralisSetupFlowStepId.AssignScoreService:
                    return new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "Gameplay Root or a Score Services child",
                        "right-click -> Create Empty, name it Score Service, add ParticipantScoreService or another ISessionScoreService, then keep it in the same scene as the bootstrap",
                        "the scoring route has a concrete service object");
                case PyralisSetupFlowStepId.AddHudOrMenuSurface:
                    return new PyralisAuthoringNativeAction(
                        "Create",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "UI Root",
                        "right-click -> UI -> Canvas or Create Empty named UI Root with Canvas and EventSystem, then add the HUD/menu presenter that matches the route such as ParticipantHealthHudBinder, ParticipantFeedbackHudPresenter, UIManager, or an RPG/board/action presenter",
                        "the scene has visible prompts, health, score, action buttons, or route-specific panels in Play Mode");
                case PyralisSetupFlowStepId.AddProjectileLauncher:
                    return new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "the pawn, weapon mount, trap, turret, or encounter object that fires",
                        "Add Component -> ProjectileLauncher2D or ProjectileLauncher3D, then assign a ProjectileDefinition created from the Project window and tune launcher origin/range/layers in the Inspector",
                        "one authored shot can be fired from a user-owned object");
                case PyralisSetupFlowStepId.TabletopRuntimeContract:
                    return new PyralisAuthoringNativeAction(
                        "Create and assign",
                        PyralisAuthoringActionSurface.ProjectWindow,
                        "the opened proof setup folder",
                        "create Board Definition, Board Piece Definition, Board Move Policy Definition, and Turn Order Definition assets in a project-owned setup folder; use generic tokens, cards, tiles, or imported marker prefabs for BoardPieceDefinition > Visual Prefab; then assign the board and turn assets to the GameModeDefinition or the scene board presenter fields",
                        "the no-pawn tabletop route has authored board state, pieces, movement policy, and turn order");
                case PyralisSetupFlowStepId.TabletopSelectionSurface:
                    return new PyralisAuthoringNativeAction(
                        "Add",
                        PyralisAuthoringActionSurface.Inspector,
                        "a project-owned board, card, cursor, or action-selection GameObject",
                        "Add Component -> TabletopBoardGridPresenter for a generic board proof, assign Board Definition, Move Policy Definition, and Turn Order Definition, then optionally add TabletopTurnStatusPresenter to a TextMeshPro label so the first Play Mode pass shows the active seat",
                        "one selectable tabletop surface can resolve a proof action in Play Mode");
                case PyralisSetupFlowStepId.TuneCameraFraming:
                    return new PyralisAuthoringNativeAction(
                        "Customize",
                        PyralisAuthoringActionSurface.Inspector,
                        "Camera Root, CameraRigProfile, and the Cinemachine camera",
                        "for 2D proofs set the CameraRigProfile preset or Target Camera Projection to Orthographic; tune Orthographic Size for zoom, but check Camera Root > 2D Bounds Framing because Enforce Minimum Visible Area 2D can raise the effective size; tune Follow Damping (0 means no lag), Follow Offset, and View Euler Angles for pitch/yaw/roll; disable Use Profile Transform only when you want to hand-place and hand-rotate the Cinemachine camera directly",
                        "the first proof is judged through the right camera setup");
                case PyralisSetupFlowStepId.TunePawnVisualsAndCollision:
                    return new PyralisAuthoringNativeAction(
                        "Customize",
                        PyralisAuthoringActionSurface.Inspector,
                        "the pawn prefab",
                        "SpriteRenderer/art placement, visual child offset, Collider2D shape/size, Rigidbody2D settings, sorting, and pivot/feet alignment",
                        "the spawned pawn looks and collides like the intended actor");
                case PyralisSetupFlowStepId.TuneMovementAndInputFeel:
                    return new PyralisAuthoringNativeAction(
                        "Customize",
                        PyralisAuthoringActionSurface.Inspector,
                        "PawnMovementProfile and effective InputProfile",
                        "movement speed, acceleration, jump/dash feel, gameplay action names, action map, and device assumptions",
                        "input and movement feel intentional instead of starter-default");
                case PyralisSetupFlowStepId.VisibleLifetimeScope:
                    return new PyralisAuthoringNativeAction(
                        "Add",
                        PyralisAuthoringActionSurface.Inspector,
                        "Gameplay Root",
                        "Add Component -> PyralisGameplayLifetimeScope",
                        "the composition root is visible before Play Mode");
                case PyralisSetupFlowStepId.FirstSceneDefaults:
                    return new PyralisAuthoringNativeAction(
                        "Enable",
                        PyralisAuthoringActionSurface.Inspector,
                        "GameplaySessionBootstrap",
                        "Auto Create Core Services and Inject Loaded Scenes On Build",
                        "first-scene runtime services are created predictably");
                case PyralisSetupFlowStepId.SceneAndPrefabReadiness:
                    return new PyralisAuthoringNativeAction(
                        "Inspect",
                        PyralisAuthoringActionSurface.Inspector,
                        "the object or asset named by the readiness issue",
                        "clear required scene/prefab readiness issues before entering Play Mode; use Validate for the detailed list and Inspector Add Component or object picker for the named handoff",
                        "Play Mode is only testing a fully wired proof path");
                default:
                    return null;
            }
        }

        private static PyralisAuthoringNativeAction GetPawnNativeAction(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return new PyralisAuthoringNativeAction(
                    "Inspect",
                    PyralisAuthoringActionSurface.Inspector,
                    "the participant, PawnDefinition, or pawn prefab named by the current step",
                    "the missing pawn field or component",
                    "Assign Participant Pawn is ready");
            }

            if (message.Contains("needs a PawnDefinition"))
            {
                return new PyralisAuthoringNativeAction(
                    "Create",
                    PyralisAuthoringActionSurface.ProjectWindow,
                    "the opened setup folder",
                    "choose or create the proof setup folder in the Project content pane first, keep imported art folders separate, then right-click inside it -> Create -> NeonBlack -> Definitions -> Pawn Definition, then assign it into ParticipantDefinition > Default Pawn by drag/drop or the field's object picker circle",
                    "the participant points at a PawnDefinition");
            }

            if (message.Contains("needs a pawn prefab"))
            {
                return new PyralisAuthoringNativeAction(
                    "Create or select",
                    PyralisAuthoringActionSurface.Hierarchy,
                    "the pawn prefab root",
                    "name the GameObject, add PawnRoot plus lane movement/presentation/input components, then drag the prefab into PawnDefinition > Pawn Prefab",
                    "the PawnDefinition has a prefab");
            }

            if (message.Contains("needs PawnRoot"))
            {
                return new PyralisAuthoringNativeAction(
                    "Add",
                    PyralisAuthoringActionSurface.Inspector,
                    "the pawn prefab root",
                    "Add Component -> PawnRoot",
                    "Pyralis recognizes the prefab as a pawn actor");
            }

            if (message.Contains("needs a component that implements IPawnMotor"))
            {
                return new PyralisAuthoringNativeAction(
                    "Add",
                    PyralisAuthoringActionSurface.Inspector,
                    "the pawn prefab root",
                    "Add Component -> Motor2D for a 2D pawn, or the lane motor that implements IPawnMotor",
                    "movement profiles have a runtime motor to drive");
            }

            if (message.Contains("needs a component that implements IPawnPresentationModule"))
            {
                return new PyralisAuthoringNativeAction(
                    "Add",
                    PyralisAuthoringActionSurface.Inspector,
                    "the pawn prefab root or visual child",
                    "Add Component -> Pawn2DPresentationComponent or the lane presentation module, then assign a project-owned sprite, prefab visual, or renderer in the presentation fields",
                    "the pawn has visible presentation");
            }

            if (message.Contains("needs a component that implements IPawnInputModule"))
            {
                return new PyralisAuthoringNativeAction(
                    "Add",
                    PyralisAuthoringActionSurface.Inspector,
                    "the pawn prefab root",
                    "Add Component -> Motor2DInputAdapter for a 2D pawn, or the lane input module that implements IPawnInputModule",
                    "InputProfile actions can reach movement");
            }

            return new PyralisAuthoringNativeAction(
                "Inspect",
                PyralisAuthoringActionSurface.Inspector,
                "the participant, PawnDefinition, or pawn prefab",
                "the field or component named by the validation message",
                "Assign Participant Pawn is ready");
        }
    }

    public sealed class PyralisSetupFlowReport
    {
        private readonly List<PyralisSetupFlowStep> _steps;
        private readonly List<PyralisSetupFlowStep> _guidedDisplaySteps;

        public PyralisSetupFlowReport(IEnumerable<PyralisSetupFlowStep> steps)
        {
            _steps = new List<PyralisSetupFlowStep>(steps ?? System.Array.Empty<PyralisSetupFlowStep>());
            _guidedDisplaySteps = BuildGuidedDisplaySteps(_steps);
        }

        public IReadOnlyList<PyralisSetupFlowStep> Steps => _steps;
        public IReadOnlyList<PyralisSetupFlowStep> GuidedDisplaySteps => _guidedDisplaySteps;

        public PyralisSetupFlowStep FirstBlockingStep
        {
            get
            {
                for (int i = 0; i < _steps.Count; i++)
                {
                    if (_steps[i].Status == PyralisSetupFlowStepStatus.Missing || _steps[i].Status == PyralisSetupFlowStepStatus.Blocked)
                        return _steps[i];
                }

                return null;
            }
        }

        public int RequiredIssueCount => Count(PyralisSetupFlowStepStatus.Missing) + Count(PyralisSetupFlowStepStatus.Blocked);
        public int MissingCount => Count(PyralisSetupFlowStepStatus.Missing);
        public int BlockedCount => Count(PyralisSetupFlowStepStatus.Blocked);
        public int RecommendedIssueCount => Count(PyralisSetupFlowStepStatus.Recommended);
        public int OptionalCount => Count(PyralisSetupFlowStepStatus.Optional);
        public int ReadyCount => Count(PyralisSetupFlowStepStatus.Ready);

        public PyralisSetupFlowStep GetStep(string label)
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                if (string.Equals(_steps[i].Label, label, System.StringComparison.Ordinal))
                    return _steps[i];
            }

            return null;
        }

        public string BuildChecklistText()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Pyralis Setup Flow");

            for (int i = 0; i < _guidedDisplaySteps.Count; i++)
            {
                PyralisSetupFlowStep step = _guidedDisplaySteps[i];
                builder.Append("- [");
                builder.Append(step.Status == PyralisSetupFlowStepStatus.Ready ? "x" : " ");
                builder.Append("] ");
                builder.Append(step.Label);
                builder.Append(" - ");
                builder.Append(step.Status);
                if (!string.IsNullOrWhiteSpace(step.Message))
                {
                    builder.Append(": ");
                    builder.Append(step.Message);
                }

                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        private static List<PyralisSetupFlowStep> BuildGuidedDisplaySteps(IReadOnlyList<PyralisSetupFlowStep> steps)
        {
            List<PyralisSetupFlowStep> ordered = new List<PyralisSetupFlowStep>();
            PyralisSetupFlowStep firstBlocking = null;

            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i].IsRequiredIssue)
                {
                    firstBlocking = steps[i];
                    ordered.Add(steps[i]);
                    break;
                }
            }

            AddByStatus(ordered, steps, PyralisSetupFlowStepStatus.Missing, firstBlocking);
            AddByStatus(ordered, steps, PyralisSetupFlowStepStatus.Recommended, firstBlocking);
            AddByStatus(ordered, steps, PyralisSetupFlowStepStatus.Ready, firstBlocking);
            AddByStatus(ordered, steps, PyralisSetupFlowStepStatus.Optional, firstBlocking);
            AddByStatus(ordered, steps, PyralisSetupFlowStepStatus.Blocked, firstBlocking);

            return ordered;
        }

        private static void AddByStatus(List<PyralisSetupFlowStep> ordered, IReadOnlyList<PyralisSetupFlowStep> steps, PyralisSetupFlowStepStatus status, PyralisSetupFlowStep skip)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                PyralisSetupFlowStep step = steps[i];
                if (step == skip || step.Status != status)
                    continue;

                ordered.Add(step);
            }
        }

        private int Count(PyralisSetupFlowStepStatus status)
        {
            int count = 0;
            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i].Status == status)
                    count++;
            }

            return count;
        }
    }

    public static class PyralisSetupFlowValidator
    {
        public static PyralisSetupFlowReport BuildReport(GameplaySessionBootstrap bootstrap)
        {
            List<PyralisSetupFlowStep> steps = new List<PyralisSetupFlowStep>();

            if (bootstrap == null)
            {
                steps.Add(new PyralisSetupFlowStep(
                    "Select Gameplay Session Bootstrap",
                    PyralisSetupFlowStepStatus.Missing,
                    "Select a scene object with GameplaySessionBootstrap to inspect setup flow.",
                    stepId: PyralisSetupFlowStepId.SelectGameplaySessionBootstrap));
                return new PyralisSetupFlowReport(steps);
            }

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SessionDefinition session = GetObjectReference<SessionDefinition>(serializedBootstrap, "sessionDefinition");
            bool autoCreateCoreServices = GetBool(serializedBootstrap, "autoCreateCoreServices");
            bool injectLoadedScenesOnBuild = GetBool(serializedBootstrap, "injectLoadedScenesOnBuild");
            int spawnPointCount = GetArraySize(serializedBootstrap, "spawnPoints");
            CinemachineCameraRigController cameraRig = GetObjectReference<CinemachineCameraRigController>(serializedBootstrap, "cameraRigController");
            bool hasCameraRig = cameraRig != null;
            bool hasPlayerInputManager = GetObjectReference<Object>(serializedBootstrap, "playerInputManager") != null;
            bool hasLifetimeScope = bootstrap.GetComponent<PyralisGameplayLifetimeScope>() != null;

            PyralisSetupRouteAnalysis route = PyralisSetupRouteAnalysis.Build(session);
            GameModeDefinition mode = route.Mode;
            GameSetupProfile setupProfile = route.SetupProfile;
            bool hasValidPatterns = route.HasValidPatterns;
            bool requiresPawn = route.RequiresPawn;
            bool hasParticipants = route.HasParticipants;
            bool hasParticipantPawn = route.HasAnyDefaultPawn;
            string participantPawnIssue = route.ParticipantPawnIssue;
            PawnDefinition firstPawn = GetFirstPawnDefinition(session);
            bool hasParticipantInputProfile = HasAnyParticipantInputProfile(session);
            string participantInputProfileIssue = GetParticipantInputIssue(session);
            bool hasUsableParticipantInputProfile = hasParticipantInputProfile && string.IsNullOrWhiteSpace(participantInputProfileIssue);
            bool setupRouteReady = setupProfile != null && hasValidPatterns;
            bool needsCameraRigForFirstProof = setupRouteReady && route.UsesPawnGameplay();
            bool needs2DCameraBounds = setupRouteReady && route.Requires2DCameraBounds();
            bool has2DCameraBounds = !needs2DCameraBounds || HasUsable2DCameraBounds(cameraRig, mode);
            bool hasGameplayStateService = HasSceneService<IGameplayStateReader>(bootstrap, out MonoBehaviour gameplayStateService);
            bool hasCameraBoundsService = HasSceneService<ICameraBoundsProvider>(bootstrap, out MonoBehaviour cameraBoundsService);
            bool hasScoreService = HasSceneService<ISessionScoreService>(bootstrap, out MonoBehaviour scoreService);
            bool hasProjectileLauncher = HasSceneComponent<ProjectileLauncherBase>(bootstrap, out ProjectileLauncherBase projectileLauncher);
            bool hasTabletopGridPresenter = HasSceneComponent<TabletopBoardGridPresenter>(bootstrap, out TabletopBoardGridPresenter tabletopGridPresenter);
            bool hasTabletopSelectionBridge = HasSceneComponent<TabletopBoardSelectionBridge>(bootstrap, out TabletopBoardSelectionBridge tabletopSelectionBridge);
            bool hasTabletopContract = HasTabletopRuntimeContract(mode, tabletopGridPresenter, out Object tabletopContractReference);
            bool hasTabletopSelectionSurface = hasTabletopGridPresenter || hasTabletopSelectionBridge;
            Object tabletopSelectionReference = tabletopGridPresenter != null
                ? tabletopGridPresenter
                : tabletopSelectionBridge != null
                    ? tabletopSelectionBridge
                    : setupProfile;
            bool hasCanvas = HasSceneComponent<Canvas>(bootstrap, out Canvas canvas);
            bool hasUiManager = HasSceneComponent<UIManager>(bootstrap, out UIManager uiManager);
            bool hasFeedbackHud = HasSceneComponent<ParticipantFeedbackHudPresenter>(bootstrap, out ParticipantFeedbackHudPresenter feedbackHud);
            bool hasHealthHud = HasSceneComponent<ParticipantHealthHudBinder>(bootstrap, out ParticipantHealthHudBinder healthHud);
            bool hasHudSurface = hasUiManager || hasFeedbackHud || hasHealthHud;
            Object hudReference = uiManager != null
                ? uiManager
                : feedbackHud != null
                    ? feedbackHud
                    : healthHud != null
                        ? healthHud
                        : canvas != null
                            ? canvas
                            : bootstrap;
            PyralisRuntimeSystemClaimReport runtimeSystemClaimReport = PyralisRuntimeSystemClaimResolver.BuildReport(
                route.RequiredRuntimeSystems,
                new PyralisRuntimeSystemClaimContext(
                    participantPawnIssue,
                    hasProjectileLauncher,
                    hasScoreService,
                    mode != null && mode.enableScore));
            PyralisSceneReadinessReport sceneReadinessReport = PyralisSceneReadinessValidator.BuildReport(bootstrap);

            steps.Add(new PyralisSetupFlowStep(
                "Gameplay Root",
                PyralisSetupFlowStepStatus.Ready,
                "Selected object has GameplaySessionBootstrap.",
                bootstrap,
                stepId: PyralisSetupFlowStepId.GameplayRoot));

            steps.Add(new PyralisSetupFlowStep(
                "Visible Lifetime Scope",
                hasLifetimeScope ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended,
                hasLifetimeScope
                    ? "PyralisGameplayLifetimeScope is visible on this root."
                    : "Runtime can create this automatically, but adding it now makes the supported composition root easier to inspect.",
                hasLifetimeScope ? (Object)bootstrap.GetComponent<PyralisGameplayLifetimeScope>() : bootstrap.gameObject,
                hasLifetimeScope ? PyralisSetupFlowActionKind.SelectObject : PyralisSetupFlowActionKind.AddLifetimeScope,
                stepId: PyralisSetupFlowStepId.VisibleLifetimeScope));

            steps.Add(new PyralisSetupFlowStep(
                "First-Scene Defaults",
                autoCreateCoreServices && injectLoadedScenesOnBuild ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended,
                autoCreateCoreServices && injectLoadedScenesOnBuild
                    ? "Auto Create Core Services and Inject Loaded Scenes On Build are enabled."
                    : "First-scene proofs should keep Auto Create Core Services and Inject Loaded Scenes On Build enabled.",
                bootstrap,
                autoCreateCoreServices && injectLoadedScenesOnBuild ? PyralisSetupFlowActionKind.SelectObject : PyralisSetupFlowActionKind.RestoreFirstSceneDefaults,
                stepId: PyralisSetupFlowStepId.FirstSceneDefaults));

            steps.Add(new PyralisSetupFlowStep(
                "Runtime Service Ownership",
                PyralisSetupFlowStepStatus.Ready,
                "GameplaySessionBootstrap owns PlatformServiceRegistry setup and builds PyralisGameplayLifetimeScope. New setup should depend on session services and platform context, not hidden global lookups.",
                bootstrap,
                PyralisSetupFlowActionKind.SelectObject,
                stepId: PyralisSetupFlowStepId.RuntimeServiceOwnership));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Session Definition",
                session != null ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing,
                session != null ? "Bootstrap can read a SessionDefinition." : "Assign the SessionDefinition this scene should start.",
                session,
                stepId: PyralisSetupFlowStepId.AssignSessionDefinition));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Default Game Mode",
                GetDependentStatus(session != null, mode != null),
                session == null
                    ? "Assign Session Definition first."
                    : mode != null ? "Session has a default GameModeDefinition." : "Assign SessionDefinition > Default Game Mode.",
                mode,
                stepId: PyralisSetupFlowStepId.AssignDefaultGameMode));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Setup Profile",
                GetDependentStatus(mode != null, setupProfile != null),
                mode == null
                    ? "Assign Default Game Mode first."
                    : setupProfile != null ? "Game mode has a GameSetupProfile." : "Assign GameModeDefinition > Setup Profile.",
                setupProfile,
                stepId: PyralisSetupFlowStepId.AssignSetupProfile));

            steps.Add(new PyralisSetupFlowStep(
                "Add Runtime Patterns",
                GetDependentStatus(setupProfile != null, hasValidPatterns),
                setupProfile == null
                    ? "Assign Setup Profile first."
                    : !route.HasAssignedPatterns ? "Add one or more runtime capabilities to the setup profile through Capability To Add -> Add Capability." : hasValidPatterns ? "Setup profile has runtime pattern intent." : "Fix RuntimePatternDefinition validation issues before continuing.",
                setupProfile,
                stepId: PyralisSetupFlowStepId.AddRuntimePatterns));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Default Participants",
                GetDependentStatus(session != null, hasParticipants),
                session == null
                    ? "Assign Session Definition first."
                    : hasParticipants ? "Session has default participants." : "Assign at least one default participant, seat, hand, faction, AI, or player.",
                session,
                stepId: PyralisSetupFlowStepId.AssignDefaultParticipants));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Participant Pawn",
                GetParticipantPawnStatus(setupProfile != null && hasValidPatterns, requiresPawn, hasParticipantPawn, participantPawnIssue),
                GetParticipantPawnMessage(setupProfile != null && hasValidPatterns, requiresPawn, hasParticipantPawn, participantPawnIssue),
                session,
                stepId: PyralisSetupFlowStepId.AssignParticipantPawn));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Input Profile",
                GetParticipantInputProfileStatus(setupProfile != null && hasValidPatterns, requiresPawn, hasParticipants, hasUsableParticipantInputProfile),
                GetParticipantInputProfileMessage(setupProfile != null && hasValidPatterns, requiresPawn, hasParticipants, session, hasParticipantInputProfile, participantInputProfileIssue),
                GetInputProfileReference(session),
                stepId: PyralisSetupFlowStepId.AssignInputProfile));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Spawn Points",
                GetSpawnPointStatus(setupProfile != null && hasValidPatterns, requiresPawn, spawnPointCount),
                GetSpawnPointMessage(setupProfile != null && hasValidPatterns, requiresPawn, spawnPointCount),
                bootstrap,
                stepId: PyralisSetupFlowStepId.AssignSpawnPoints));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Camera Rig",
                GetCameraRigStatus(setupProfile != null && hasValidPatterns, needsCameraRigForFirstProof, route.UsesCamera(), hasCameraRig, has2DCameraBounds),
                GetCameraRigMessage(setupProfile != null && hasValidPatterns, needsCameraRigForFirstProof, needs2DCameraBounds, route.UsesCamera(), hasCameraRig, has2DCameraBounds),
                cameraRig,
                stepId: PyralisSetupFlowStepId.AssignCameraRig));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Player Input Manager",
                GetRequiredRouteServiceStatus(setupProfile != null && hasValidPatterns, route.LikelyUsesInputManager(), hasPlayerInputManager),
                GetPlayerInputMessage(setupProfile != null && hasValidPatterns, route.LikelyUsesInputManager(), hasPlayerInputManager),
                GetObjectReference<Object>(serializedBootstrap, "playerInputManager"),
                stepId: PyralisSetupFlowStepId.AssignPlayerInputManager));

            steps.Add(new PyralisSetupFlowStep(
                "Tune Camera Framing",
                GetCustomizationStatus(setupRouteReady, route.UsesPawnGameplay() || route.UsesCamera() || route.UsesPlayfield(), hasCameraRig),
                GetCameraCustomizationMessage(setupRouteReady, route.UsesPawnGameplay() || route.UsesCamera() || route.UsesPlayfield(), hasCameraRig),
                cameraRig != null ? cameraRig : mode != null ? mode.cameraRigProfile : null,
                stepId: PyralisSetupFlowStepId.TuneCameraFraming));

            steps.Add(new PyralisSetupFlowStep(
                "Tune Pawn Visuals And Collision",
                GetCustomizationStatus(setupRouteReady, route.UsesPawnGameplay(), firstPawn != null && firstPawn.pawnPrefab != null),
                GetPawnCustomizationMessage(setupRouteReady, route.UsesPawnGameplay(), firstPawn),
                firstPawn != null && firstPawn.pawnPrefab != null ? firstPawn.pawnPrefab : firstPawn,
                stepId: PyralisSetupFlowStepId.TunePawnVisualsAndCollision));

            steps.Add(new PyralisSetupFlowStep(
                "Tune Movement And Input Feel",
                GetCustomizationStatus(setupRouteReady, route.UsesPawnGameplay(), firstPawn != null),
                GetMovementCustomizationMessage(setupRouteReady, route.UsesPawnGameplay(), firstPawn),
                GetMovementCustomizationReference(firstPawn, session),
                stepId: PyralisSetupFlowStepId.TuneMovementAndInputFeel));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Playfield Profile",
                GetRecommendationStatus(setupProfile != null && hasValidPatterns, route.UsesPlayfield(), mode != null && mode.playfieldProfile != null),
                GetPlayfieldMessage(setupProfile != null && hasValidPatterns, route.UsesPlayfield(), mode != null && mode.playfieldProfile != null),
                mode != null ? mode.playfieldProfile : null,
                stepId: PyralisSetupFlowStepId.AssignPlayfieldProfile));

            steps.Add(new PyralisSetupFlowStep(
                "Enable Scoring Route",
                GetRequiredRouteServiceStatus(setupRouteReady, route.UsesScoring(), mode != null && mode.enableScore),
                GetScoringMessage(setupRouteReady, route.UsesScoring(), mode != null && mode.enableScore),
                mode,
                stepId: PyralisSetupFlowStepId.EnableScoringRoute));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Gameplay State Service",
                GetGameplayStateServiceStatus(
                    setupRouteReady,
                    route.UsesPawnGameplay() || route.UsesScoring(),
                    autoCreateCoreServices,
                    hasGameplayStateService),
                GetGameplayStateServiceMessage(
                    setupRouteReady,
                    route.UsesPawnGameplay() || route.UsesScoring(),
                    autoCreateCoreServices,
                    hasGameplayStateService),
                gameplayStateService,
                stepId: PyralisSetupFlowStepId.AssignGameplayStateService));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Camera Bounds Service",
                GetRecommendationStatus(setupRouteReady, route.UsesCamera() || route.UsesPlayfield(), hasCameraBoundsService),
                GetCameraBoundsServiceMessage(setupRouteReady, route.UsesCamera() || route.UsesPlayfield(), hasCameraBoundsService),
                cameraBoundsService,
                stepId: PyralisSetupFlowStepId.AssignCameraBoundsService));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Score Service",
                GetRequiredRouteServiceStatus(setupRouteReady, route.UsesScoring(), hasScoreService),
                GetScoreServiceMessage(setupRouteReady, route.UsesScoring(), hasScoreService),
                scoreService,
                stepId: PyralisSetupFlowStepId.AssignScoreService));

            steps.Add(new PyralisSetupFlowStep(
                "Assign HUD / UI Surface",
                GetHudSurfaceStatus(setupRouteReady, route, hasCanvas, hasHudSurface),
                GetHudSurfaceMessage(setupRouteReady, route, hasCanvas, hasHudSurface),
                hudReference,
                stepId: PyralisSetupFlowStepId.AddHudOrMenuSurface));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Projectile Launcher",
                GetRecommendationStatus(setupRouteReady, route.UsesProjectileCombat(), hasProjectileLauncher),
                GetProjectileLauncherMessage(setupRouteReady, route.UsesProjectileCombat(), hasProjectileLauncher),
                projectileLauncher,
                stepId: PyralisSetupFlowStepId.AddProjectileLauncher));

            steps.Add(new PyralisSetupFlowStep(
                "Tabletop Runtime Contract",
                GetTabletopContractStatus(setupRouteReady, route.UsesTabletopContract(), hasTabletopContract),
                GetTabletopContractMessage(setupRouteReady, route.UsesTabletopContract(), hasTabletopContract),
                tabletopContractReference != null ? tabletopContractReference : setupProfile,
                stepId: PyralisSetupFlowStepId.TabletopRuntimeContract));

            steps.Add(new PyralisSetupFlowStep(
                "Assign Tabletop Selection Surface",
                GetTabletopSelectionSurfaceStatus(setupRouteReady, route.UsesTabletopContract(), hasTabletopSelectionSurface),
                GetTabletopSelectionSurfaceMessage(setupRouteReady, route.UsesTabletopContract(), hasTabletopSelectionSurface),
                tabletopSelectionReference,
                stepId: PyralisSetupFlowStepId.TabletopSelectionSurface));

            steps.Add(new PyralisSetupFlowStep(
                "Resolve Runtime System Claims",
                GetRuntimeSystemClaimsStatus(setupRouteReady, runtimeSystemClaimReport),
                GetRuntimeSystemClaimsMessage(setupRouteReady, runtimeSystemClaimReport),
                setupProfile));

            steps.Add(new PyralisSetupFlowStep(
                "Scene And Prefab Readiness",
                GetSceneReadinessStatus(setupRouteReady, sceneReadinessReport),
                GetSceneReadinessMessage(setupRouteReady, sceneReadinessReport),
                sceneReadinessReport != null && !sceneReadinessReport.IsReady ? bootstrap : session,
                stepId: PyralisSetupFlowStepId.SceneAndPrefabReadiness));

            return new PyralisSetupFlowReport(steps);
        }

        private static PyralisSetupFlowStepStatus GetDependentStatus(bool dependencyReady, bool ready)
        {
            if (!dependencyReady)
                return PyralisSetupFlowStepStatus.Blocked;

            return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetParticipantPawnStatus(bool setupReady, bool requiresPawn, bool hasParticipantPawn, string participantPawnIssue)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!requiresPawn)
            {
                if (!hasParticipantPawn)
                    return PyralisSetupFlowStepStatus.Optional;

                return string.IsNullOrWhiteSpace(participantPawnIssue)
                    ? PyralisSetupFlowStepStatus.Ready
                    : PyralisSetupFlowStepStatus.Recommended;
            }

            return hasParticipantPawn && string.IsNullOrWhiteSpace(participantPawnIssue)
                ? PyralisSetupFlowStepStatus.Ready
                : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetParticipantInputProfileStatus(
            bool setupReady,
            bool requiresPawn,
            bool hasParticipants,
            bool hasInputProfile)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!requiresPawn)
                return PyralisSetupFlowStepStatus.Optional;

            if (!hasParticipants)
                return PyralisSetupFlowStepStatus.Blocked;

            return hasInputProfile
                ? PyralisSetupFlowStepStatus.Ready
                : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetSpawnPointStatus(bool setupReady, bool requiresPawn, int spawnPointCount)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!requiresPawn)
                return spawnPointCount > 0 ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return spawnPointCount > 0 ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetRecommendationStatus(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!recommended)
                return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended;
        }

        private static PyralisSetupFlowStepStatus GetCustomizationStatus(bool setupReady, bool relevant, bool hasTarget)
        {
            if (!setupReady || !relevant)
                return PyralisSetupFlowStepStatus.Optional;

            return hasTarget ? PyralisSetupFlowStepStatus.Recommended : PyralisSetupFlowStepStatus.Optional;
        }

        private static PyralisSetupFlowStepStatus GetCameraRigStatus(bool setupReady, bool requiredForFirstProof, bool recommended, bool ready, bool usable2DBounds)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (requiredForFirstProof)
                return ready && usable2DBounds ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;

            if (!recommended)
                return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended;
        }

        private static PyralisSetupFlowStepStatus GetRequiredRouteServiceStatus(bool setupReady, bool required, bool ready)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!required)
                return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetTabletopContractStatus(bool setupReady, bool usesTabletopContract, bool hasTabletopContract)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!usesTabletopContract)
                return PyralisSetupFlowStepStatus.Optional;

            return hasTabletopContract ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Missing;
        }

        private static PyralisSetupFlowStepStatus GetTabletopSelectionSurfaceStatus(bool setupReady, bool usesTabletopContract, bool hasTabletopSelectionSurface)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!usesTabletopContract)
                return PyralisSetupFlowStepStatus.Optional;

            return hasTabletopSelectionSurface ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended;
        }

        private static PyralisSetupFlowStepStatus GetRuntimeSystemClaimsStatus(bool setupReady, PyralisRuntimeSystemClaimReport report)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (report == null || !report.HasDeclaredClaims)
                return PyralisSetupFlowStepStatus.Optional;

            return report.HasUnverifiedClaims
                ? PyralisSetupFlowStepStatus.Recommended
                : PyralisSetupFlowStepStatus.Ready;
        }

        private static string GetRuntimeSystemClaimsMessage(bool setupReady, PyralisRuntimeSystemClaimReport report)
        {
            if (!setupReady)
                return "Choose runtime patterns before resolving declared runtime system claims.";

            if (report == null || !report.HasDeclaredClaims)
                return "No explicit Required Runtime Systems are declared by the selected patterns.";

            if (!report.HasUnverifiedClaims)
                return "Declared Required Runtime Systems are covered by bootstrap services, pawn validation, or concrete scene-service checks.";

            return "These declared Required Runtime Systems still need project verification or deeper prefab checks: " + report.UnverifiedSummary + ".";
        }

        private static PyralisSetupFlowStepStatus GetSceneReadinessStatus(bool setupReady, PyralisSceneReadinessReport report)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (report == null || !report.IsReady)
                return PyralisSetupFlowStepStatus.Missing;

            return report.HasRecommendations
                ? PyralisSetupFlowStepStatus.Recommended
                : PyralisSetupFlowStepStatus.Ready;
        }

        private static string GetSceneReadinessMessage(bool setupReady, PyralisSceneReadinessReport report)
        {
            if (!setupReady)
                return "Choose a valid setup profile and runtime pattern before checking scene and prefab readiness.";

            if (report == null)
                return "Scene and prefab readiness could not be evaluated.";

            if (!report.IsReady)
                return "Do not enter Play Mode yet. Fix required scene/prefab issue: " + report.RequiredSummary + ".";

            if (report.HasRecommendations)
                return "Required scene/prefab checks are clear for a narrow proof. Recommended follow-up: " + report.RecommendedSummary + ".";

            return "Scene and prefab readiness checks are clear. Play Mode can now test the proof instead of revealing missing setup.";
        }

        private static string GetParticipantPawnMessage(bool setupReady, bool requiresPawn, bool hasParticipantPawn, string participantPawnIssue)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding whether participants need pawns.";

            if (!requiresPawn)
            {
                if (!hasParticipantPawn)
                    return "No participant pawn is required for this setup route.";

                if (!string.IsNullOrWhiteSpace(participantPawnIssue))
                    return participantPawnIssue;

                return hasParticipantPawn
                    ? "A participant has a pawn, which is allowed for this setup."
                    : "No participant pawn is required for this setup route.";
            }

            if (!string.IsNullOrWhiteSpace(participantPawnIssue))
                return participantPawnIssue;

            return hasParticipantPawn
                ? "At least one default participant has a pawn."
                : "Selected setup requires pawn-backed participants. Assign a PawnDefinition to a default participant.";
        }

        private static string GetParticipantInputProfileMessage(
            bool setupReady,
            bool requiresPawn,
            bool hasParticipants,
            SessionDefinition session,
            bool hasInputProfile,
            string inputProfileIssue)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding whether input profiles are required.";

            if (!hasParticipants)
                return "Assign participants first, then assign input profiles.";

            if (!hasInputProfile)
            {
                return requiresPawn
                    ? "Assign InputProfile on `SessionDefinition.defaultParticipants[0]` (or set `SessionDefinition.defaultInputProfile`) in Inspector before routing movement."
                    : "Input profile is optional for this route unless a built-in player/input surface is used.";
            }

            if (!string.IsNullOrWhiteSpace(inputProfileIssue))
                return inputProfileIssue;

            if (session == null || session.defaultInputProfile == null)
                return "A participant InputProfile is assigned. Pawn/input readers can now bind control signals.";

            return "InputProfile is assigned. Participant values are used before SessionDefinition.defaultInputProfile fallback.";
        }

        private static Object GetInputProfileReference(SessionDefinition session)
        {
            if (session == null)
                return null;

            if (session.defaultInputProfile != null)
                return session.defaultInputProfile;

            if (session.defaultParticipants == null)
                return session;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant != null && participant.inputProfile != null)
                    return participant;
            }

            return session;
        }

        private static Object GetMovementCustomizationReference(PawnDefinition pawn, SessionDefinition session)
        {
            if (pawn != null && pawn.movementProfile != null)
                return pawn.movementProfile;

            Object inputProfileReference = GetInputProfileReference(session);
            return inputProfileReference != null ? inputProfileReference : pawn;
        }

        private static PawnDefinition GetFirstPawnDefinition(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null)
                return null;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant != null && participant.defaultPawn != null)
                    return participant.defaultPawn;
            }

            return null;
        }

        private static string GetParticipantInputIssue(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return "Assign default participants first.";

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                    continue;

                PawnDefinition pawn = participant.defaultPawn;
                InputProfile effectiveProfile = ParticipantInputProfileUtility.ResolveEffectiveInputProfile(
                    participant,
                    pawn,
                    session.defaultInputProfile);

                if (effectiveProfile == null)
                    return "Add InputProfile to one participant, its PawnDefinition, or SessionDefinition.defaultInputProfile before trying movement in Play Mode.";

                string bindingIssue = GetInputProfileBindingIssue(effectiveProfile);
                if (!string.IsNullOrWhiteSpace(bindingIssue))
                    return $"Participant `{participant.displayName}` effective InputProfile `{effectiveProfile.name}`: {bindingIssue}";
            }

            return string.Empty;
        }

        private static string GetInputProfileBindingIssue(InputProfile profile)
        {
            if (profile == null)
                return "Assign an InputProfile before trying movement in Play Mode.";

            profile.Sanitize();

            if (profile.actions == null)
                return "assign Actions to the stock Assets/InputSystem_Actions.inputactions asset, or choose a custom Unity Input Action Asset for an advanced input layout.";

            InputActionMap actionMap = ParticipantInputProfileUtility.FindGameplayActionMap(profile.actions, profile);
            if (actionMap == null)
            {
                string mapName = !string.IsNullOrWhiteSpace(profile.primaryActionMap)
                    ? profile.primaryActionMap
                    : "Player";
                return $"Primary Action Map `{mapName}` was not found in Actions.";
            }

            GameplayInputActionBinding moveBinding = profile.FindBinding(GameplayInputActionRole.Move);
            if (moveBinding == null)
                return "add a required Move row to Gameplay Actions.";

            if (string.IsNullOrWhiteSpace(moveBinding.actionName))
                return "set the Move row Unity Action Name to the action that drives movement.";

            InputActionMap moveMap = actionMap;
            string moveMapName = moveBinding.GetActionMap(profile.primaryActionMap);
            if (!string.Equals(moveMapName, actionMap.name, System.StringComparison.OrdinalIgnoreCase))
                moveMap = profile.actions.FindActionMap(moveMapName, throwIfNotFound: false);

            if (moveMap == null)
                return $"Move row Action Map `{moveMapName}` was not found in Actions.";

            if (ParticipantInputProfileUtility.FindAction(moveMap, moveBinding.actionName) == null)
                return $"Move row Unity Action Name `{moveBinding.actionName}` was not found in Action Map `{moveMap.name}`.";

            if (!profile.supportsGamepad && !profile.supportsKeyboardMouse && !profile.touchFriendly)
                return "enable at least one supported input surface such as keyboard/mouse, gamepad, or touch.";

            return string.Empty;
        }

        private static string GetSpawnPointMessage(bool setupReady, bool requiresPawn, int spawnPointCount)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding whether spawn points are required.";

            if (!requiresPawn)
                return spawnPointCount > 0
                    ? "Spawn points are assigned, which is allowed when this setup spawns actor bodies."
                    : "Spawn points can stay empty for no-pawn board/card/menu/camera routes.";

            return spawnPointCount > 0
                ? "Spawn points are assigned for pawn-backed participants."
                : "Selected setup requires pawns. Add spawn point transforms to the bootstrap.";
        }

        private static string GetCameraRigMessage(bool setupReady, bool requiredForFirstProof, bool requires2DBounds, bool recommended, bool ready, bool usable2DBounds)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding camera rig wiring.";

            if (requiredForFirstProof)
            {
                if (!ready)
                    return "Pawn movement needs camera bounds before the first Play Mode proof. Keep or create one physical Unity Camera, usually the default Main Camera; do not delete it for the normal Cinemachine route. Create Camera Root, add CinemachineCameraRigController, create or choose a separate Cinemachine Camera for Shared Camera Behaviour, verify the physical Main Camera is tagged MainCamera with Cinemachine Brain, assign that physical camera as Target Camera, then drag Camera Root from Hierarchy into Bootstrap > Camera Rig Controller.";

                if (!requires2DBounds)
                    return "Camera rig is assigned for the pawn movement proof. This route uses a 3D, 2.5D, or non-orthographic pawn lane, so 2D orthographic bounds are not required before Play Mode.";

                return usable2DBounds
                    ? "Camera rig is assigned with usable 2D bounds for the pawn movement proof."
                    : "Camera rig is assigned, but the 2D movement proof still needs orthographic bounds. Select Camera Root and assign an orthographic CameraRigProfile, or select the physical Target Camera and set Camera > Projection to Orthographic. If using a profile, also assign it to GameModeDefinition > Camera Rig Profile.";
            }

            if (!recommended)
                return ready
                    ? "Camera rig is assigned."
                    : "Camera rig is optional for this setup route. Add it later if the player controls a view, cursor, selector, board camera, or follow camera.";

            return ready
                ? "Camera rig is assigned for camera/cursor flow."
                : "Selected setup uses camera/cursor flow. Create or choose a Camera Rig Profile in your project folderbase. In the Hierarchy, keep or create one physical Unity Camera, usually the default Main Camera, then create Camera Root, add CinemachineCameraRigController, and create or choose a separate Cinemachine Camera for Shared Camera Behaviour. Verify the physical Main Camera keeps the MainCamera tag and Cinemachine Brain, then assign Camera Rig Profile, Shared Camera Behaviour, and Target Camera before dragging Camera Root into Bootstrap > Camera Rig Controller. For 2D proofs, set the physical Target Camera Projection to Orthographic or use an orthographic CameraRigProfile, then tune 2D Bounds Framing on the rig.";
        }

        private static string GetCameraCustomizationMessage(bool setupReady, bool relevant, bool hasCameraRig)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding camera customization.";

            if (!relevant)
                return "Camera framing can wait until this route uses a pawn, camera, cursor, board view, playfield, or follow camera.";

            return hasCameraRig
                ? "Before judging Play Mode, select Camera Root and CameraRigProfile and tune framing for the scene: physical Target Camera assignment, MainCamera tag/Brain on that physical camera, orthographic size, 2D Bounds Framing minimum visible area, Follow Damping (0 means no lag), Follow Offset, View Euler Angles for pitch/yaw/roll, and how much room the player needs around the pawn. In orthographic mode, CameraRigProfile > Orthographic Size controls zoom only until Camera Root > Enforce Minimum Visible Area 2D raises it to fit the authored min world size. Keep Use Profile Transform on for profile-driven framing, or turn it off when you want direct Cinemachine transform authoring. In Play Mode, Cinemachine follows a runtime GameplaySharedCameraFocus driven from participants; prove follow by moving the pawn, then verifying the Game view follows that shared focus."
                : "Tune camera framing after the Camera Root exists. The Authoring Window should keep this visible so the proof is judged against the intended view, not a default camera accident.";
        }

        private static string GetPawnCustomizationMessage(bool setupReady, bool relevant, PawnDefinition pawn)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding pawn customization.";

            if (!relevant)
                return "Pawn visuals and colliders can wait because this route does not currently need actor bodies.";

            if (pawn == null || pawn.pawnPrefab == null)
                return "Tune pawn visuals and colliders after the PawnDefinition points to a prefab.";

            return "Before judging Play Mode, open the pawn prefab and check the obvious Unity-owned fit: SpriteRenderer/art placement, visual child offset, Collider2D or Collider shape/size, Rigidbody2D settings, sorting, and whether the pivot matches the intended feet/body position.";
        }

        private static string GetMovementCustomizationMessage(bool setupReady, bool relevant, PawnDefinition pawn)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding movement customization.";

            if (!relevant)
                return "Movement and input tuning can wait because this route does not currently need pawn control.";

            if (pawn == null)
                return "Tune movement and input after a ParticipantDefinition references a PawnDefinition.";

            return "Before judging Play Mode, inspect the PawnMovementProfile, effective InputProfile, and installed FeatureModuleDefinition assets. Use top-down 2D defaults for free X/Y movement, add a top-down hop feature when Jump should lift the visual while staying map-plane grounded, or use side-view 2D settings when Jump should drive Rigidbody2D vertical motion. The InputProfile maps Unity Input Actions into semantic roles; the pawn prefab still needs an input module such as Motor2DInputAdapter to dispatch those roles.";
        }

        private static bool HasUsable2DCameraBounds(CinemachineCameraRigController rig, GameModeDefinition mode)
        {
            if (rig == null)
                return false;

            SerializedObject serializedRig = new SerializedObject(rig);
            CameraRigProfile rigProfile = serializedRig.FindProperty("cameraRigProfile")?.objectReferenceValue as CameraRigProfile;
            if (rigProfile != null)
                return rigProfile.orthographic;

            if (mode != null && mode.cameraRigProfile != null && mode.cameraRigProfile.orthographic)
                return true;

            Camera targetCamera = serializedRig.FindProperty("targetCamera")?.objectReferenceValue as Camera;
            if (targetCamera != null)
                return targetCamera.orthographic;

            Camera childCamera = rig.GetComponentInChildren<Camera>(true);
            return childCamera != null && childCamera.orthographic;
        }

        private static string GetPlayerInputMessage(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding local join wiring.";

            if (!recommended)
                return ready
                    ? "PlayerInputManager is assigned."
                    : "PlayerInputManager is optional for single-player, AI-only, menu-only, and no-join prototypes. Add it only when multiple local players can join.";

            return ready
                ? "PlayerInputManager is assigned for local join, and ParticipantInputRouter will subscribe to join/leave events."
                : "Selected setup looks like multi-participant local join. For a 1P proof, select/open the SessionDefinition asset and set Max Participants to 1. For local join, create an Input Root, add Unity PlayerInputManager, assign a dedicated PlayerInput prefab, configure Join Behavior/Input Actions, then drag the component into Bootstrap > Player Input Manager.";
        }

        private static string GetPlayfieldMessage(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding playfield wiring.";

            if (!recommended)
                return ready
                    ? "Playfield profile is assigned."
                    : "Playfield profile is optional until the route needs authored bounds, board spaces, lanes, zones, or generated areas.";

            return ready
                ? "Playfield profile is assigned."
                : "Add a playfield profile when this setup needs bounds, board spaces, lanes, zones, or generated areas. Put the authored playfield reference on GameModeDefinition > Playfield Profile, then create matching scene anchors or presenters under a Playfield Root.";
        }

        private static string GetScoringMessage(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding scoring wiring.";

            if (!recommended)
                return ready ? "Scoring is enabled." : "Scoring can stay disabled for this setup route.";

            return ready ? "Scoring route is enabled." : "Selected setup uses scoring/objectives. Enable scoring when score systems are part of the first playable loop.";
        }

        private static PyralisSetupFlowStepStatus GetGameplayStateServiceStatus(
            bool setupReady,
            bool required,
            bool autoCreateCoreServices,
            bool ready)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            if (!required)
                return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return autoCreateCoreServices || ready
                ? PyralisSetupFlowStepStatus.Ready
                : PyralisSetupFlowStepStatus.Recommended;
        }

        private static string GetGameplayStateServiceMessage(
            bool setupReady,
            bool required,
            bool autoCreateCoreServices,
            bool ready)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding gameplay state service wiring.";

            if (!required)
            {
                return ready ? "Gameplay state service is present." : "Gameplay state service is optional for this setup route.";
            }

            if (autoCreateCoreServices)
            {
                return "Auto Create Core Services is enabled, so required session-state services are provisioned at startup for the first proof.";
            }

            return ready
                ? "Scene has an IGameplayStateReader for active/dead/game-over aware systems."
                : "Enable Auto Create Core Services for first-scene proofs, or assign an IGameplayStateReader before running your proof.";

        }

        private static string GetCameraBoundsServiceMessage(bool setupReady, bool recommended, bool ready)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding camera bounds service wiring.";

            if (!recommended)
                return ready ? "Camera bounds provider is present." : "Camera bounds provider is optional for this setup route.";

            return ready
                ? "Scene has an ICameraBoundsProvider for playfield-aware systems."
                : "Assign the CinemachineCameraRigController to Bootstrap > Camera Rig Controller. The rig provides ICameraBoundsProvider for 2D movement, spawning, hazards, pickups, and framing; use Camera Bounds Source only for specialized custom bounds providers.";
        }

        private static string GetScoreServiceMessage(bool setupReady, bool required, bool ready)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding score service wiring.";

            if (!required)
                return ready ? "Score service is present." : "Score service is optional for this setup route.";

            return ready
                ? "Scene has an ISessionScoreService for score/objective runtime."
                : "Selected setup claims scoring/objectives. Add ParticipantScoreService or another ISessionScoreService before treating this route as playable.";
        }

        private static PyralisSetupFlowStepStatus GetHudSurfaceStatus(bool setupReady, PyralisSetupRouteAnalysis route, bool hasCanvas, bool ready)
        {
            if (!setupReady)
                return PyralisSetupFlowStepStatus.Blocked;

            bool recommended = route != null
                && (route.UsesScoring()
                    || route.UsesPawnGameplay()
                    || route.UsesTabletopContract()
                    || route.UsesActionSelection()
                    || route.RequiresRuntimeSystem("HUD")
                    || route.RequiresRuntimeSystem("UI"));

            if (!recommended)
                return ready || hasCanvas ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Optional;

            return ready ? PyralisSetupFlowStepStatus.Ready : PyralisSetupFlowStepStatus.Recommended;
        }

        private static string GetHudSurfaceMessage(bool setupReady, PyralisSetupRouteAnalysis route, bool hasCanvas, bool ready)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding HUD or menu surfaces.";

            if (ready)
                return "Scene has a Pyralis HUD/UI surface. Verify its Canvas, EventSystem, labels, panels, buttons, and service references in the Inspector.";

            if (hasCanvas)
                return "Scene has a Canvas, but no known Pyralis HUD/menu presenter yet. Add ParticipantHealthHudBinder for pawn health, ParticipantFeedbackHudPresenter for combat/score/status messages, UIManager for score/time/game-over flow, or a project-owned presenter that reads the same services.";

            if (route != null && route.UsesScoring())
                return "Selected setup uses scoring/objectives. Create a UI Root with Canvas and EventSystem, then add UIManager for score/time/game-over flow or ParticipantFeedbackHudPresenter for score feedback. Link score UI to ParticipantScoreService or another ISessionScoreService after score changes work in Play Mode.";

            if (route != null && route.UsesTabletopContract())
                return "Selected setup uses board/card/tabletop flow. Create a UI Root with Canvas and EventSystem for turn prompts, action menus, card hands, board selection, or routed interaction panels; connect presenters to the board/action/turn services the scene owns.";

            if (route != null && route.UsesActionSelection())
                return "Selected setup uses action selection. Create a UI Root with Canvas and EventSystem, then add buttons, panels, or cursor/selection presenters that call the chosen action, menu, turn, card, or command runtime. Start with one selectable action before expanding the whole menu.";

            if (route != null && route.UsesPawnGameplay())
                return "Pawn-backed setups usually need visible health, feedback, or menus. Create a UI Root with Canvas and EventSystem, then add ParticipantHealthHudBinder for health, ParticipantFeedbackHudPresenter for combat/status/score messages, UIManager for game-over flow, or project-owned presenters as needed.";

            return "HUD or menu surfaces are optional for this route. Add a Canvas and EventSystem when the game needs visible state, buttons, prompts, settings, or action selection.";
        }

        private static string GetProjectileLauncherMessage(bool setupReady, bool required, bool ready)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding projectile launcher wiring.";

            if (!required)
                return ready ? "Projectile launcher is present." : "Projectile launcher is optional for this setup route.";

            return ready
                ? "Scene has a ProjectileLauncherBase implementation for projectile/hitscan runtime."
                : "Projectile combat is selected, but the first movement proof can run before combat wiring. Add ProjectileLauncher2D or ProjectileLauncher3D before treating the full projectile route as wired.";
        }

        private static string GetTabletopContractMessage(bool setupReady, bool usesTabletopContract, bool hasTabletopContract)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding tabletop runtime contract wiring.";

            if (!usesTabletopContract)
                return "Tabletop runtime contract is optional for this setup route.";

            return hasTabletopContract
                ? "Tabletop route has authored board and turn data. Use the selection surface row to make one visible Play Mode proof."
                : "Create and assign BoardDefinition plus TurnOrderDefinition before calling the no-pawn tabletop route ready. BoardMovePolicyDefinition and BoardPieceDefinition assets make the first proof selectable and readable.";
        }

        private static string GetTabletopSelectionSurfaceMessage(bool setupReady, bool usesTabletopContract, bool hasTabletopSelectionSurface)
        {
            if (!setupReady)
                return "Choose runtime patterns before deciding tabletop selection wiring.";

            if (!usesTabletopContract)
                return "Tabletop selection/input surfaces are optional for this setup route.";

            return hasTabletopSelectionSurface
                ? "Scene has a tabletop selection surface. Enter Play Mode and prove one generic board, card, cursor, or menu selection changes board, turn, score, or UI state."
                : "Add TabletopBoardGridPresenter for a generic board proof, or connect TabletopBoardSelectionBridge to a project-owned selection/input bridge, card-hand presenter, cursor, or menu action surface.";
        }

        private static bool HasAnyParticipantInputProfile(SessionDefinition session)
        {
            if (session == null)
                return false;

            if (session.defaultInputProfile != null)
                return true;

            if (session.defaultParticipants == null)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                    continue;

                if (participant.inputProfile != null)
                    return true;

                if (participant.defaultPawn != null && participant.defaultPawn.defaultInputProfile != null)
                    return true;
            }

            return false;
        }

        private static T GetObjectReference<T>(SerializedObject serializedObject, string propertyName) where T : Object
        {
            return serializedObject.FindProperty(propertyName)?.objectReferenceValue as T;
        }

        private static bool GetBool(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return property != null && property.boolValue;
        }

        private static int GetArraySize(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return property != null && property.isArray ? property.arraySize : 0;
        }

        private static bool HasTabletopRuntimeContract(GameModeDefinition mode, TabletopBoardGridPresenter presenter, out Object reference)
        {
            reference = null;
            if (mode != null && mode.boardDefinition != null && mode.turnOrderDefinition != null)
            {
                reference = mode;
                return true;
            }

            if (presenter == null)
                return false;

            SerializedObject serializedPresenter = new SerializedObject(presenter);
            bool hasBoard = GetObjectReference<Object>(serializedPresenter, "boardDefinition") != null;
            bool hasTurnOrder = GetObjectReference<Object>(serializedPresenter, "turnOrderDefinition") != null;
            if (!hasBoard || !hasTurnOrder)
                return false;

            reference = presenter;
            return true;
        }

        private static bool HasSceneService<T>(GameplaySessionBootstrap bootstrap, out MonoBehaviour service) where T : class
        {
            service = null;
            if (bootstrap == null)
                return false;

            UnityEngine.SceneManagement.Scene scene = bootstrap.gameObject.scene;
            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour.gameObject.scene == scene && behaviour is T)
                {
                    service = behaviour;
                    return true;
                }
            }

            return false;
        }

        private static bool HasSceneComponent<T>(GameplaySessionBootstrap bootstrap, out T component) where T : Component
        {
            component = null;
            if (bootstrap == null)
                return false;

            UnityEngine.SceneManagement.Scene scene = bootstrap.gameObject.scene;
            T[] components = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
            for (int i = 0; i < components.Length; i++)
            {
                T candidate = components[i];
                if (candidate != null && candidate.gameObject.scene == scene)
                {
                    component = candidate;
                    return true;
                }
            }

            return false;
        }
    }

    public static class PyralisSetupFlowActions
    {
        public static void AddMissingLifetimeScope(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null || bootstrap.GetComponent<PyralisGameplayLifetimeScope>() != null)
                return;

            Undo.AddComponent<PyralisGameplayLifetimeScope>(bootstrap.gameObject);
        }

        public static void RestoreFirstSceneDefaults(SerializedObject serializedBootstrap)
        {
            if (serializedBootstrap == null)
                return;

            Undo.RecordObject(serializedBootstrap.targetObject, "Restore Pyralis First-Scene Defaults");
            SetBool(serializedBootstrap, "autoCreateCoreServices", true);
            SetBool(serializedBootstrap, "injectLoadedScenesOnBuild", true);
            serializedBootstrap.ApplyModifiedProperties();
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
                property.boolValue = value;
        }
    }

    public static class PyralisSetupFlowDrawer
    {
        public static void Draw(GameplaySessionBootstrap bootstrap, SerializedObject serializedBootstrap, PyralisSetupFlowReport report)
        {
            if (report == null)
                return;

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Setup Flow", EditorStyles.boldLabel);
                DrawContextSummary(bootstrap, report);
                DrawSummary(report);
                DrawToolbar(report);

                for (int i = 0; i < report.GuidedDisplaySteps.Count; i++)
                    DrawStep(bootstrap, serializedBootstrap, report.GuidedDisplaySteps[i]);
            }
        }

        private static void DrawContextSummary(GameplaySessionBootstrap bootstrap, PyralisSetupFlowReport report)
        {
            string sessionName = "None";
            string setupName = "None";
            string routeName = "Not selected";

            if (bootstrap != null)
            {
                SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
                SessionDefinition session = serializedBootstrap.FindProperty("sessionDefinition")?.objectReferenceValue as SessionDefinition;
                GameModeDefinition mode = session != null ? session.defaultGameMode : null;
                GameSetupProfile setupProfile = mode != null ? mode.setupProfile : null;

                sessionName = session != null ? session.name : "None";
                setupName = setupProfile != null ? setupProfile.setupName : "None";
                routeName = setupProfile != null ? PyralisSetupRouteAnalysis.Build(setupProfile).RouteName : "Not selected";
            }

            PyralisSetupFlowStep firstBlocking = report.FirstBlockingStep;
            string firstFix = firstBlocking != null ? firstBlocking.Label : "No required fixes";

            EditorGUILayout.LabelField("Session", sessionName);
            EditorGUILayout.LabelField("Setup Profile", setupName);
            EditorGUILayout.LabelField("Route", routeName);
            EditorGUILayout.LabelField("First Required Fix", firstFix);
        }

        private static void DrawSummary(PyralisSetupFlowReport report)
        {
            PyralisSetupFlowStep firstBlocking = report.FirstBlockingStep;
            if (firstBlocking != null)
            {
                EditorGUILayout.HelpBox("Next setup step: " + firstBlocking.Label + "\n" + firstBlocking.Message, MessageType.Warning);
                return;
            }

            if (report.RecommendedIssueCount > 0)
            {
                EditorGUILayout.HelpBox("Required setup is clear. Run your minimal first proof first, then handle recommended items while you grow into the next route feature.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox("Required setup is clear. Optional items can stay empty until this scene needs them.", MessageType.Info);
        }

        private static void DrawToolbar(PyralisSetupFlowReport report)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy Checklist"))
                    EditorGUIUtility.systemCopyBuffer = report.BuildChecklistText();

                if (GUILayout.Button("Open Authoring Window"))
                    PyralisAuthoringWindow.Open();
            }
        }

        private static void DrawStep(GameplaySessionBootstrap bootstrap, SerializedObject serializedBootstrap, PyralisSetupFlowStep step)
        {
            if (step == null)
                return;

            EditorGUILayout.Space(2f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(GetStatusPrefix(step.Status) + " " + step.Label, EditorStyles.boldLabel);
                    DrawObjectActions(step);
                    DrawRepairAction(bootstrap, serializedBootstrap, step);
                }

                if (!string.IsNullOrWhiteSpace(step.Message))
                    EditorGUILayout.LabelField(step.Message, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static void DrawObjectActions(PyralisSetupFlowStep step)
        {
            using (new EditorGUI.DisabledScope(step.ReferencedObject == null))
            {
                if (GUILayout.Button("Select", GUILayout.Width(56f)))
                    Selection.activeObject = step.ReferencedObject;

                if (GUILayout.Button("Ping", GUILayout.Width(44f)))
                    EditorGUIUtility.PingObject(step.ReferencedObject);
            }
        }

        private static void DrawRepairAction(GameplaySessionBootstrap bootstrap, SerializedObject serializedBootstrap, PyralisSetupFlowStep step)
        {
            switch (step.ActionKind)
            {
                case PyralisSetupFlowActionKind.AddLifetimeScope:
                    if (GUILayout.Button("Add", GUILayout.Width(44f)))
                    {
                        PyralisSetupFlowActions.AddMissingLifetimeScope(bootstrap);
                        GUIUtility.ExitGUI();
                    }
                    break;
                case PyralisSetupFlowActionKind.RestoreFirstSceneDefaults:
                    if (GUILayout.Button("Restore", GUILayout.Width(64f)))
                    {
                        PyralisSetupFlowActions.RestoreFirstSceneDefaults(serializedBootstrap);
                        GUIUtility.ExitGUI();
                    }
                    break;
            }
        }

        private static string GetStatusPrefix(PyralisSetupFlowStepStatus status)
        {
            switch (status)
            {
                case PyralisSetupFlowStepStatus.Ready:
                    return "[Ready]";
                case PyralisSetupFlowStepStatus.Missing:
                    return "[Missing]";
                case PyralisSetupFlowStepStatus.Blocked:
                    return "[Blocked]";
                case PyralisSetupFlowStepStatus.Recommended:
                    return "[Recommended]";
                case PyralisSetupFlowStepStatus.Optional:
                    return "[Optional]";
                default:
                    return "[Setup]";
            }
        }

    }
}
