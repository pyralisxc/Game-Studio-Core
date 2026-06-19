using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Characters;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
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
            AddSetupFact(facts, PyralisSetupFlowStepId.ResolveRouteCapabilities, "Resolve Route Capabilities", "Reflect capability families from intent, contracts, serialized gameplay references, feature modules, participants, pawns, and scene evidence.", "Capability setup");
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
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignSettingsManager, "Assign Settings Manager", "Create or assign a SettingsManager to handle global volume, deadzones, and control swaps.", "Game Shell and UX");
            AddSetupFact(facts, PyralisSetupFlowStepId.SceneAndPrefabReadiness, "Scene And Prefab Readiness", "Block Play Mode proof guidance until required scene objects, prefab modules, and inspector handoffs are clear.", "First-proof gate", new[] { "proof.1p-pawn-movement" });
            return facts;
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

        public static PyralisSetupFlowWorkIntent GetDefaultWorkIntent(PyralisSetupFlowStepId stepId)
        {
            switch (stepId)
            {
                case PyralisSetupFlowStepId.SelectGameplaySessionBootstrap:
                case PyralisSetupFlowStepId.GameplayRoot:
                case PyralisSetupFlowStepId.RuntimeServiceOwnership:
                case PyralisSetupFlowStepId.AssignSessionDefinition:
                case PyralisSetupFlowStepId.AssignDefaultGameMode:
                case PyralisSetupFlowStepId.ResolveRouteCapabilities:
                case PyralisSetupFlowStepId.AssignDefaultParticipants:
                case PyralisSetupFlowStepId.AssignParticipantPawn:
                case PyralisSetupFlowStepId.AssignSpawnPoints:
                    return PyralisSetupFlowWorkIntent.Foundation;
                case PyralisSetupFlowStepId.AddHudOrMenuSurface:
                case PyralisSetupFlowStepId.AddProjectileLauncher:
                    return PyralisSetupFlowWorkIntent.FeatureCard;
                case PyralisSetupFlowStepId.TuneCameraFraming:
                case PyralisSetupFlowStepId.TunePawnVisualsAndCollision:
                case PyralisSetupFlowStepId.TuneMovementAndInputFeel:
                    return PyralisSetupFlowWorkIntent.ProofEnhancer;
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
                case PyralisSetupFlowStepId.ResolveRouteCapabilities: return "setup.resolve-route-capabilities";
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
                case PyralisSetupFlowStepId.AssignSettingsManager: return "setup.assign-settings-manager";
                case PyralisSetupFlowStepId.SceneAndPrefabReadiness: return "setup.scene-prefab-readiness";
                default: return string.Empty;
            }
        }

        public static PyralisAuthoringNativeAction? GetNativeAction(PyralisSetupFlowStepId stepId, string message)
        {
            switch (stepId)
            {
                case PyralisSetupFlowStepId.SelectGameplaySessionBootstrap:
                    return PyralisAuthoringNativeActionFactory.CreateSceneObjectAction(
                        "Gameplay Root",
                        "GameplaySessionBootstrap",
                        "Overview shows Gameplay Root as the active setup",
                        "name it Gameplay Root");
                case PyralisSetupFlowStepId.AssignSessionDefinition:
                    return PyralisAuthoringNativeActionFactory.CreateAssetAction(
                        "SessionDefinition",
                        "NeonBlack -> Definitions -> Session Definition",
                        "the Session row is ready",
                        "assign it to GameplaySessionBootstrap.sessionDefinition");
                case PyralisSetupFlowStepId.AssignDefaultGameMode:
                    return PyralisAuthoringNativeActionFactory.CreateAssetAction(
                        "GameModeDefinition",
                        "NeonBlack -> Definitions -> Game Mode Definition",
                        "the Game Mode row is ready",
                        "assign it to SessionDefinition.defaultGameMode");
                case PyralisSetupFlowStepId.ResolveRouteCapabilities:
                    return new PyralisAuthoringNativeAction(
                        "Choose and wire",
                        PyralisAuthoringActionSurface.AuthoringWindow,
                        "Intent",
                        "set DNA axioms, choose the presentation lane, toggle the capability ingredients that describe the route, then create or wire the matching SessionDefinition, GameModeDefinition, participants, pawns, feature modules, board/turn assets, and scene objects so the graph can reflect them",
                        "route capabilities are reflected from real setup");
                case PyralisSetupFlowStepId.AssignDefaultParticipants:
                    return PyralisAuthoringNativeActionFactory.CreateAssetAction(
                        "ParticipantDefinition",
                        "NeonBlack -> Definitions -> Participant Definition",
                        "Players / Seats is ready",
                        "configure seat/player meaning, then assign it to SessionDefinition.defaultParticipants");
                case PyralisSetupFlowStepId.AssignParticipantPawn:
                    return GetPawnNativeAction(message);
                case PyralisSetupFlowStepId.AssignInputProfile:
                    return PyralisAuthoringNativeActionFactory.CreateAssignmentAction(
                        "Create or assign",
                        "ParticipantDefinition.inputProfile",
                        "InputProfile",
                        "assign Actions, keep or confirm Primary Action Map as Player, then scroll to Input Actions Sync and click Sync Action Names From Asset before customizing gameplay action rows",
                        "InputProfile actions can reach the pawn input module");
                case PyralisSetupFlowStepId.AssignSpawnPoints:
                    return PyralisAuthoringNativeActionFactory.CreateSceneObjectAction(
                        "Gameplay Root or a Playfield Root",
                        string.Empty,
                        "the pawn route has one spawn point per default participant",
                        "create and position SpawnPoint_1, then assign it to ParticipantSpawnService.spawnPoints");
                case PyralisSetupFlowStepId.AssignPlayerInputManager:
                    return new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "Input Root",
                        "create an Input Root, add Unity PlayerInputManager, set Join Behavior for your proof, assign Input Actions, then set Player Prefab to the same pawn prefab shape used by the participant route: it must contain PlayerInput and PawnRoot/IPawnParticipantInitializer so each controller owns one spawned participant pawn. Drag the PlayerInputManager component into GameplaySessionBootstrap > Player Input Manager.",
                        "each local controller is paired to one PlayerInput, one participant, and one pawn");
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
                        "the same CinemachineCameraRigController in GameplaySessionBootstrap > Camera Rig Controller; camera-aware runtime systems consume that single camera bounds provider",
                        "camera-aware spawners, hazards, pickups, and framing can read visible bounds; pawn movement uses PlayfieldProfile unless camera-visible bounds are explicitly enabled on the pawn");
                case PyralisSetupFlowStepId.AssignPlayfieldProfile:
                    return PyralisAuthoringNativeActionFactory.CreateAssignmentAction(
                        "Create or assign",
                        "GameModeDefinition.playfieldProfile",
                        "PlayfieldProfile",
                        "create one from NeonBlack -> Profiles -> Playfield Profile when needed; tune bounds and lane rules after assignment",
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
                    return PyralisAuthoringNativeActionFactory.AddComponentAction(
                        "the pawn, weapon mount, trap, turret, or encounter object that fires",
                        "ProjectileLauncher2D or ProjectileLauncher3D",
                        "one authored shot can be fired from a user-owned object",
                        "assign a ProjectileDefinition and tune launcher origin, range, and layers");
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
                case PyralisSetupFlowStepId.AssignSettingsManager:
                    return PyralisAuthoringNativeActionFactory.CreateSceneObjectAction(
                        "Gameplay Root or a persistent Shell object",
                        "SettingsManager",
                        "global volume and control settings are persistent and accessible",
                        "name it Settings Manager and assign a SettingsProfile asset");
                case PyralisSetupFlowStepId.TuneCameraFraming:
                    return new PyralisAuthoringNativeAction(
                        "Customize",
                        PyralisAuthoringActionSurface.Inspector,
                        "Camera Root, CameraRigProfile, and the Cinemachine camera",
                        "for 2D proofs set CameraRigProfile projection values or Target Camera Projection to Orthographic; tune Orthographic Size for zoom, but check Camera Root > 2D Bounds Framing because Enforce Minimum Visible Area 2D can raise the effective size; tune Follow Damping (0 means no lag), Follow Offset, and View Euler Angles for pitch/yaw/roll; disable Use Profile Transform only when you want to hand-place and hand-rotate the Cinemachine camera directly",
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
                        "input and movement feel intentional instead of accidental defaults");
                case PyralisSetupFlowStepId.VisibleLifetimeScope:
                    return PyralisAuthoringNativeActionFactory.AddComponentAction(
                        "Gameplay Root",
                        "PyralisGameplayLifetimeScope",
                        "the composition root is visible before Play Mode");
                case PyralisSetupFlowStepId.FirstSceneDefaults:
                    return PyralisAuthoringNativeActionFactory.ConfigureInspectorAction(
                        "GameplaySessionBootstrap",
                        "bootstrap startup ownership and Inject Loaded Scenes On Build",
                        string.Empty,
                        "first-scene runtime services are owned predictably");
                case PyralisSetupFlowStepId.SceneAndPrefabReadiness:
                    return new PyralisAuthoringNativeAction(
                        "Inspect",
                        PyralisAuthoringActionSurface.Inspector,
                        "the object or asset named by the readiness issue",
                        "clear required scene/prefab readiness issues before entering Play Mode; use Map for scene/setup repair and Inspector Add Component or object picker for the named handoff",
                        "Play Mode is only testing a fully wired proof path");
                default:
                    return null;
            }
        }

        public static PyralisAuthoringNativeAction GetPawnNativeAction(PyralisParticipantPawnIssueKind issueKind)
        {
            return GetPawnNativeAction(issueKind, RuntimeCapabilityLaneTag.Mixed);
        }

        public static PyralisAuthoringNativeAction GetPawnNativeAction(
            PyralisParticipantPawnIssueKind issueKind,
            RuntimeCapabilityLaneTag laneTag)
        {
            switch (issueKind)
            {
                case PyralisParticipantPawnIssueKind.MissingPawnDefinition:
                    return PyralisAuthoringNativeActionFactory.CreateAssetAction(
                        "PawnDefinition",
                        "NeonBlack -> Definitions -> Pawn Definition",
                        "the participant points at a PawnDefinition",
                        "assign it to ParticipantDefinition.defaultPawn");
                case PyralisParticipantPawnIssueKind.MissingPawnPrefab:
                    return new PyralisAuthoringNativeAction(
                        "Create or select",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "the pawn prefab root",
                        GetPawnPrefabSetupInstruction(laneTag),
                        "the PawnDefinition has a prefab");
                case PyralisParticipantPawnIssueKind.MissingPawnRoot:
                    return PyralisAuthoringNativeActionFactory.AddComponentAction(
                        "the pawn prefab root",
                        "PawnRoot",
                        "Pyralis recognizes the prefab as a pawn actor");
                case PyralisParticipantPawnIssueKind.MissingMotor:
                    return PyralisAuthoringNativeActionFactory.AddComponentAction(
                        "the pawn prefab root",
                        GetPawnMotorComponentLabel(laneTag),
                        "movement profiles have a runtime motor to drive");
                case PyralisParticipantPawnIssueKind.MissingPresentation:
                    return PyralisAuthoringNativeActionFactory.AddComponentAction(
                        "the pawn prefab root or visual child",
                        GetPawnPresentationComponentLabel(laneTag),
                        "the pawn has visible presentation",
                        "assign a project-owned sprite, prefab visual, or renderer in the presentation fields");
                case PyralisParticipantPawnIssueKind.MissingInputModule:
                    return PyralisAuthoringNativeActionFactory.AddComponentAction(
                        "the pawn prefab root",
                        GetPawnInputComponentLabel(laneTag),
                        "InputProfile actions can reach movement");
                default:
                    return new PyralisAuthoringNativeAction(
                        "Inspect",
                        PyralisAuthoringActionSurface.Inspector,
                        "the participant, PawnDefinition, or pawn prefab",
                        "the field or component named by the validation message",
                        "Assign Participant Pawn is ready");
            }
        }

        private static PyralisAuthoringNativeAction GetPawnNativeAction(string message)
        {
            return GetPawnNativeAction(InferPawnIssueKind(message));
        }

        public static PyralisParticipantPawnIssueKind InferPawnIssueKind(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return PyralisParticipantPawnIssueKind.None;

            if (message.Contains("PawnDefinition before participants can spawn"))
                return PyralisParticipantPawnIssueKind.MissingPawnDefinition;

            if (message.Contains("point at a pawn prefab"))
                return PyralisParticipantPawnIssueKind.MissingPawnPrefab;

            if (message.Contains("missing PawnRoot"))
                return PyralisParticipantPawnIssueKind.MissingPawnRoot;

            if (message.Contains("missing a lane motor component") || message.Contains("IPawnMotor"))
                return PyralisParticipantPawnIssueKind.MissingMotor;

            if (message.Contains("missing a presentation component") || message.Contains("IPawnPresentationModule"))
                return PyralisParticipantPawnIssueKind.MissingPresentation;

            if (message.Contains("missing an input adapter") || message.Contains("IPawnInputModule"))
                return PyralisParticipantPawnIssueKind.MissingInputModule;

            return PyralisParticipantPawnIssueKind.PawnValidation;
        }

        private static string GetPawnPrefabSetupInstruction(RuntimeCapabilityLaneTag laneTag)
        {
            switch (laneTag)
            {
                case RuntimeCapabilityLaneTag.Sprite2D:
                    return "name the GameObject, add PawnRoot, Motor2D, Motor2DInputAdapter, SpriteRenderer, and Animator, save it as a prefab, then drag the prefab into PawnDefinition > Pawn Prefab. Motor2D adds the required Pawn2DMovementComponent and Pawn2DPresentationComponent siblings. Add Unity PlayerInput only when you want explicit local keyboard/gamepad ownership, and assign the same Input Actions asset used by the InputProfile";
                case RuntimeCapabilityLaneTag.ThirdPerson3D:
                case RuntimeCapabilityLaneTag.Billboard2_5D:
                    return "name the GameObject, add PawnRoot, Pawn3DMovementComponent, Pawn3DInputModule, Pawn3DPresentationComponent, and CharacterController, save it as a prefab, then drag the prefab into PawnDefinition > Pawn Prefab. Add Unity PlayerInput only when you want explicit local keyboard/gamepad ownership, and assign the same Input Actions asset used by the InputProfile";
                default:
                    return "name the GameObject, add PawnRoot plus the lane motor, input, and presentation components, save it as a prefab, then drag the prefab into PawnDefinition > Pawn Prefab. Add Unity PlayerInput only when you want explicit local keyboard/gamepad ownership, and assign the same Input Actions asset used by the InputProfile";
            }
        }

        private static string GetPawnMotorComponentLabel(RuntimeCapabilityLaneTag laneTag)
        {
            switch (laneTag)
            {
                case RuntimeCapabilityLaneTag.Sprite2D:
                    return "Motor2D";
                case RuntimeCapabilityLaneTag.ThirdPerson3D:
                case RuntimeCapabilityLaneTag.Billboard2_5D:
                    return "Pawn3DMovementComponent";
                default:
                    return "the lane motor component";
            }
        }

        private static string GetPawnInputComponentLabel(RuntimeCapabilityLaneTag laneTag)
        {
            switch (laneTag)
            {
                case RuntimeCapabilityLaneTag.Sprite2D:
                    return "Motor2DInputAdapter";
                case RuntimeCapabilityLaneTag.ThirdPerson3D:
                case RuntimeCapabilityLaneTag.Billboard2_5D:
                    return "Pawn3DInputModule";
                default:
                    return "the lane input module";
            }
        }

        private static string GetPawnPresentationComponentLabel(RuntimeCapabilityLaneTag laneTag)
        {
            switch (laneTag)
            {
                case RuntimeCapabilityLaneTag.Sprite2D:
                    return "Pawn2DPresentationComponent";
                case RuntimeCapabilityLaneTag.ThirdPerson3D:
                case RuntimeCapabilityLaneTag.Billboard2_5D:
                    return "Pawn3DPresentationComponent";
                default:
                    return "the lane presentation module";
            }
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
            SetBool(serializedBootstrap, "injectLoadedScenesOnBuild", true);
            serializedBootstrap.ApplyModifiedProperties();
        }

        public static void CreateMissingProfile(PyralisSetupFlowStep step)
        {
            if (step == null || step.ReferencedType == null)
                return;

            string path = EditorUtility.SaveFilePanelInProject("Create Profile", step.Label.Replace(" ", ""), "asset", "Choose a location for the new profile asset.");
            if (string.IsNullOrEmpty(path))
                return;

            ScriptableObject asset = ScriptableObject.CreateInstance(step.ReferencedType);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            
            EditorGUIUtility.PingObject(asset);
        }

        private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
                property.boolValue = value;
        }
    }
}
