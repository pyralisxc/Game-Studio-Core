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
            AddSetupFact(facts, PyralisSetupFlowStepId.AssignSetupProfile, "Assign Setup Profile", "Create or assign the setup profile that lists selected capability ingredients.", "Core setup chain");
            AddSetupFact(facts, PyralisSetupFlowStepId.AddRuntimePatterns, "Choose Capabilities", "Select capability families that describe the current route. Optional runtime pattern assets can add advanced metadata later.", "Capability setup");
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
                case PyralisSetupFlowStepId.AssignSetupProfile:
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
                        "the Setup Profile row is ready");
                case PyralisSetupFlowStepId.AddRuntimePatterns:
                    return new PyralisAuthoringNativeAction(
                        "Choose",
                        PyralisAuthoringActionSurface.AuthoringWindow,
                        "Intent",
                        "set DNA axioms, choose the presentation lane, and toggle the Engine Spine capabilities that describe this route while the GameSetupProfile is active; leave RuntimePatternDefinition empty unless this route needs an advanced reusable contract",
                        "Capability ingredients are selected");
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
                        "right-click -> Create Empty, name it SpawnPoint_1, position it, select Gameplay Root, expand GameplaySessionBootstrap > Spawn Points, click + to create Element 0, then drag SpawnPoint_1 from the Hierarchy into that Transform slot",
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
                case PyralisSetupFlowStepId.AssignSettingsManager:
                    return new PyralisAuthoringNativeAction(
                        "Create or assign",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "Gameplay Root or a persistent Shell object",
                        "right-click -> Create Empty, name it Settings Manager, add SettingsManager, and assign a SettingsProfile asset",
                        "global volume and control settings are persistent and accessible");
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
                        "bootstrap startup ownership and Inject Loaded Scenes On Build",
                        "first-scene runtime services are owned predictably");
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

        public static PyralisAuthoringNativeAction GetPawnNativeAction(PyralisParticipantPawnIssueKind issueKind)
        {
            switch (issueKind)
            {
                case PyralisParticipantPawnIssueKind.MissingPawnDefinition:
                    return new PyralisAuthoringNativeAction(
                        "Create",
                        PyralisAuthoringActionSurface.ProjectWindow,
                        "the opened setup folder",
                        "choose or create the proof setup folder in the Project content pane first, keep imported art folders separate, then right-click inside it -> Create -> NeonBlack -> Definitions -> Pawn Definition, then assign it into ParticipantDefinition > Default Pawn by drag/drop or the field's object picker circle",
                        "the participant points at a PawnDefinition");
                case PyralisParticipantPawnIssueKind.MissingPawnPrefab:
                    return new PyralisAuthoringNativeAction(
                        "Create or select",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "the pawn prefab root",
                        "name the GameObject, add the lane stack, save it as a prefab, then drag the prefab into PawnDefinition > Pawn Prefab. For a 2D proof, add PawnRoot, Motor2D, Motor2DInputAdapter, SpriteRenderer, and Animator; Motor2D adds the required movement and presentation siblings. Add Unity PlayerInput only when you want explicit local keyboard/gamepad ownership, and assign the same Input Actions asset used by the InputProfile",
                        "the PawnDefinition has a prefab");
                case PyralisParticipantPawnIssueKind.MissingPawnRoot:
                    return new PyralisAuthoringNativeAction(
                        "Add",
                        PyralisAuthoringActionSurface.Inspector,
                        "the pawn prefab root",
                        "Add Component -> PawnRoot",
                        "Pyralis recognizes the prefab as a pawn actor");
                case PyralisParticipantPawnIssueKind.MissingMotor:
                    return new PyralisAuthoringNativeAction(
                        "Add",
                        PyralisAuthoringActionSurface.Inspector,
                        "the pawn prefab root",
                        "Add Component -> Motor2D for a 2D pawn, or the lane motor that implements IPawnMotor",
                        "movement profiles have a runtime motor to drive");
                case PyralisParticipantPawnIssueKind.MissingPresentation:
                    return new PyralisAuthoringNativeAction(
                        "Add",
                        PyralisAuthoringActionSurface.Inspector,
                        "the pawn prefab root or visual child",
                        "Add Component -> Pawn2DPresentationComponent or the lane presentation module, then assign a project-owned sprite, prefab visual, or renderer in the presentation fields",
                        "the pawn has visible presentation");
                case PyralisParticipantPawnIssueKind.MissingInputModule:
                    return new PyralisAuthoringNativeAction(
                        "Add",
                        PyralisAuthoringActionSurface.Inspector,
                        "the pawn prefab root",
                        "Add Component -> Motor2DInputAdapter for a 2D pawn, or the lane input module that implements IPawnInputModule",
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

        private static PyralisParticipantPawnIssueKind InferPawnIssueKind(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return PyralisParticipantPawnIssueKind.None;

            if (message.Contains("PawnDefinition before participants can spawn"))
                return PyralisParticipantPawnIssueKind.MissingPawnDefinition;

            if (message.Contains("point at a pawn prefab"))
                return PyralisParticipantPawnIssueKind.MissingPawnPrefab;

            if (message.Contains("missing PawnRoot"))
                return PyralisParticipantPawnIssueKind.MissingPawnRoot;

            if (message.Contains("missing a lane motor component"))
                return PyralisParticipantPawnIssueKind.MissingMotor;

            if (message.Contains("missing a presentation component"))
                return PyralisParticipantPawnIssueKind.MissingPresentation;

            if (message.Contains("missing an input adapter"))
                return PyralisParticipantPawnIssueKind.MissingInputModule;

            return PyralisParticipantPawnIssueKind.PawnValidation;
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
