using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisInspectorHandoffFacts
    {
        public static IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            return new[]
            {
                CreateAssignmentFact(
                    "inspector.gameplay-session-bootstrap.session-definition",
                    "GameplaySessionBootstrap Session Definition",
                    "Assign the session asset on the scene bootstrap so the active scene knows which route to start.",
                    "Core setup chain",
                    "GameplaySessionBootstrap.sessionDefinition -> SessionDefinition",
                    "GameplaySessionBootstrap",
                    "Session Definition",
                    "the Authoring Window can read the selected session route",
                    new[] { "setup.assign-session-definition" }),

                CreateAssignmentFact(
                    "inspector.session-definition.default-game-mode",
                    "SessionDefinition Default Game Mode",
                    "Assign the game-rules asset that controls the playable loop for the session.",
                    "Core setup chain",
                    "SessionDefinition.defaultGameMode -> GameModeDefinition",
                    "SessionDefinition",
                    "Default Game Mode",
                    "the setup flow advances to setup profile selection",
                    new[] { "setup.assign-default-game-mode" }),

                CreateAssignmentFact(
                    "inspector.game-mode-definition.setup-profile",
                    "GameModeDefinition Setup Profile",
                    "Assign the setup profile that lists capability ingredients for the route.",
                    "Core setup chain",
                    "GameModeDefinition.setupProfile -> GameSetupProfile",
                    "GameModeDefinition",
                    "Setup Profile",
                    "capability selection becomes route-aware",
                    new[] { "setup.assign-setup-profile" }),

                CreateAssignmentFact(
                    "inspector.game-setup-profile.runtime-capabilities",
                    "GameSetupProfile Capability Ingredients",
                    "Runtime Capabilities stores the capability ingredients chosen in Intent so the graph can explain route readiness.",
                    "Capability setup",
                    "GameSetupProfile.runtimeCapabilities -> RuntimeCapabilitySelection[]",
                    "GameSetupProfile",
                    "Runtime Capabilities",
                    "the graph can distinguish pawn, no-pawn, combat, UI, camera, scoring, tabletop, and network needs",
                    new[] { "setup.add-runtime-patterns" }),

                CreateAssignmentFact(
                    "inspector.participant-definition.default-pawn",
                    "ParticipantDefinition Default Pawn",
                    "Assign a PawnDefinition when this participant should spawn or control a pawn-backed body.",
                    "2D pawn movement route",
                    "ParticipantDefinition.defaultPawn -> PawnDefinition",
                    "ParticipantDefinition",
                    "Default Pawn",
                    "the participant can resolve its pawn definition for the first proof",
                    new[] { "setup.assign-participant-pawn", "capability.2d-pawn-movement", "proof.1p-pawn-movement" }),

                CreateAssignmentFact(
                    "inspector.pawn-definition.pawn-prefab",
                    "PawnDefinition Pawn Prefab",
                    "Assign the prefab that contains PawnRoot and lane-specific runtime components.",
                    "2D pawn movement route",
                    "PawnDefinition.pawnPrefab -> pawn prefab",
                    "PawnDefinition",
                    "Pawn Prefab",
                    "Play Mode can spawn one pawn from the selected definition",
                    new[] { "setup.assign-participant-pawn", "capability.2d-pawn-movement", "proof.1p-pawn-movement" }),

                CreateAssignmentFact(
                    "inspector.pawn-definition.movement-and-presentation-profiles",
                    "PawnDefinition Movement And Presentation Profiles",
                    "Assign movement and presentation profiles so pawn feel and visuals stay designer-customizable.",
                    "2D pawn movement route",
                    "PawnDefinition movement/presentation profile fields -> PawnMovementProfile and PawnPresentationProfile",
                    "PawnDefinition",
                    "Movement Profile and Presentation Profile",
                    "the pawn route has tunable feel and visible presentation",
                    new[] { "setup.tune-pawn-visuals-and-collision", "setup.tune-movement-and-input-feel", "capability.2d-pawn-movement", "proof.1p-pawn-movement" }),

                CreateCustomizationFact(
                    "inspector.input-profile.gameplay-action-names",
                    "InputProfile Gameplay Action Names",
                    "Customize action-name fields to match the project's Input Action Asset without renaming the whole input asset for Pyralis.",
                    "2D pawn movement route",
                    "InputProfile Gameplay Action Names -> move, jump, dash, attack, interact, look, and related action names",
                    "InputProfile",
                    "Gameplay Action Names",
                    "input reaches the pawn through the selected InputProfile",
                    new[] { "setup.assign-input-profile", "setup.tune-movement-and-input-feel", "capability.2d-pawn-movement", "proof.1p-pawn-movement" }),

                CreateAssignmentFact(
                    "inspector.game-mode-definition.board-and-turn-rules",
                    "GameModeDefinition Board And Turn Rules",
                    "Assign board and turn-rule assets when the route is tabletop, board, card, tactics, or turn/menu driven.",
                    "tabletop board/card route",
                    "GameModeDefinition.boardDefinition, turnOrderDefinition, boardTerminalConditions -> tabletop rules",
                    "GameModeDefinition",
                    "Board Definition, Turn Order Definition, Board Terminal Conditions",
                    "the tabletop route has authored board, turn, and terminal rule references",
                    new[] { "route.tabletop-card", "proof.board-card-action", "reflection.create-asset-menu.board-definition", "reflection.create-asset-menu.turn-order-definition" }),

                CreateAssignmentFact(
                    "inspector.game-mode-definition.camera-and-playfield",
                    "GameModeDefinition Camera And Playfield Profiles",
                    "Assign camera and playfield profiles when route visibility, bounds, orthographic framing, or arena space matters.",
                    "world/camera route",
                    "GameModeDefinition.cameraRigProfile and playfieldProfile -> CameraRigProfile and PlayfieldProfile",
                    "GameModeDefinition",
                    "Camera Rig Profile and Playfield Profile",
                    "the route has authored visibility, bounds, and framing defaults",
                    new[] { "route.world-camera", "proof.camera-cursor-world", "capability.camera-follow-bounds", "reflection.create-asset-menu.camera-rig-profile", "reflection.create-asset-menu.playfield-profile" }),

                CreateAssignmentFact(
                    "inspector.game-mode-definition.required-feature-modules",
                    "GameModeDefinition Required Feature Modules",
                    "Assign feature modules when the route depends on custom objects, hazards, pickups, NPC behaviors, UI feedback, or route-specific runtime modules.",
                    "custom object/feature route",
                    "GameModeDefinition.requiredFeatureModules -> FeatureModuleDefinition[]",
                    "GameModeDefinition",
                    "Required Feature Modules",
                    "the mode declares which route-owned modules must install before proof",
                    new[] { "route.custom-object-feature", "proof.custom-object-effect", "reflection.create-asset-menu.feature-module-definition" }),

                CreateAssignmentFact(
                    "inspector.feature-module-definition.profile-runtime-network",
                    "FeatureModuleDefinition Profile Runtime And Network Fields",
                    "Assign a feature profile, optional runtime prefab, and network role when a custom feature should install predictable runtime behavior.",
                    "custom object/feature route",
                    "FeatureModuleDefinition.profileAsset, runtimePrefab, networkRole, and authority flags -> feature runtime setup",
                    "FeatureModuleDefinition",
                    "Profile Asset, Runtime Prefab, Network Role, Authority Flags",
                    "the feature module has its profile, runtime prefab, and network intent visible in one Inspector",
                    new[] { "route.custom-object-feature", "route.networking", "proof.custom-object-effect", "proof.network-ownership", "reflection.create-asset-menu.feature-module-definition" }),

                CreateAssignmentFact(
                    "inspector.cinemachine-camera-rig-controller.camera-fields",
                    "CinemachineCameraRigController Camera Fields",
                    "Assign camera rig, playfield, target camera, and Cinemachine references on the scene camera root.",
                    "world/camera route",
                    "CinemachineCameraRigController.cameraRigProfile, playfieldProfile, targetCamera, sharedCameraBehaviour, splitScreenCameraBehaviours -> camera proof setup",
                    "CinemachineCameraRigController",
                    "Camera Rig Profile, Playfield Profile, Target Camera, Cinemachine References",
                    "Play Mode camera proof can frame the authored route surface",
                    new[] { "route.world-camera", "proof.camera-cursor-world", "capability.camera-follow-bounds", "reflection.add-component-menu.cinemachine-camera-rig-controller" }),

                CreateAssignmentFact(
                    "inspector.tabletop-board-grid-presenter.board-fields",
                    "TabletopBoardGridPresenter Board Fields",
                    "Assign board, move policy, turn order, selection bridge, and board-space/piece prefabs for the smallest tabletop proof.",
                    "tabletop board/card route",
                    "TabletopBoardGridPresenter.boardDefinition, movePolicyDefinition, turnOrderDefinition, selectionBridge, spacePrefab, piecePrefab -> tabletop scene surface",
                    "TabletopBoardGridPresenter",
                    "Board Definition, Move Policy Definition, Turn Order Definition, Selection Bridge, Space Prefab, Piece Prefab",
                    "one board/card selection surface can build, gate active seats, and resolve a proof selection",
                    new[] { "route.tabletop-card", "proof.board-card-action", "reflection.add-component-menu.tabletop-board-grid-presenter", "reflection.add-component-menu.tabletop-board-selection-bridge", "reflection.add-component-menu.tabletop-turn-status-presenter" }),

                CreateAssignmentFact(
                    "inspector.tabletop-turn-status-presenter.fields",
                    "TabletopTurnStatusPresenter Fields",
                    "Assign the board presenter and TextMeshPro label that should show the active local seat during a tabletop proof.",
                    "tabletop board/card route",
                    "TabletopTurnStatusPresenter.boardPresenter, label, seat names -> visible turn feedback",
                    "TabletopTurnStatusPresenter",
                    "Board Presenter, Label, Seat Names",
                    "the first tabletop proof visibly communicates whose turn it is",
                    new[] { "route.tabletop-card", "proof.board-card-action", "reflection.add-component-menu.tabletop-turn-status-presenter" }),

                CreateCustomizationFact(
                    "inspector.board-piece-definition.visual-prefab",
                    "BoardPieceDefinition Visual Prefab",
                    "Assign creator-owned art to each board piece definition so the proof uses imported content without hardcoding a game.",
                    "tabletop board/card route",
                    "BoardPieceDefinition.visualPrefab -> imported token, card, tile, piece, or marker prefab",
                    "BoardPieceDefinition",
                    "Visual Prefab",
                    "the board presenter instantiates the author's visible token art for each piece type",
                    new[] { "route.tabletop-card", "proof.board-card-action", "reflection.create-asset-menu.board-piece-definition" }),

                CreateCustomizationFact(
                    "inspector.camera-rig-profile.framing-fields",
                    "CameraRigProfile Framing Fields",
                    "Customize orthographic, zoom, damping, follow, and shake values to make the first route proof readable.",
                    "world/camera route",
                    "CameraRigProfile orthographic/defaultDistance/minZoom/maxZoom/followDamping/orthographicSize/shakeAmplitude -> route framing feel",
                    "CameraRigProfile",
                    "Framing, Zoom, Damping, Orthographic, Shake",
                    "the route remains visible while preserving the user's camera feel",
                    new[] { "route.world-camera", "proof.camera-cursor-world", "capability.camera-follow-bounds", "reflection.create-asset-menu.camera-rig-profile" })
            };
        }

        private static PyralisAuthoringFact CreateAssignmentFact(
            string stableId,
            string displayName,
            string summary,
            string routeRelevance,
            string assignmentField,
            string target,
            string fieldOrComponent,
            string success,
            string[] relatedStableIds)
        {
            return CreateInspectorFact(
                stableId,
                displayName,
                PyralisAuthoringFactKind.AssignmentField,
                summary,
                routeRelevance,
                assignmentField,
                target,
                fieldOrComponent,
                success,
                "RequiredSetup",
                relatedStableIds);
        }

        private static PyralisAuthoringFact CreateCustomizationFact(
            string stableId,
            string displayName,
            string summary,
            string routeRelevance,
            string customizationMoment,
            string target,
            string fieldOrComponent,
            string success,
            string[] relatedStableIds)
        {
            return CreateInspectorFact(
                stableId,
                displayName,
                PyralisAuthoringFactKind.CustomizationMoment,
                summary,
                routeRelevance,
                customizationMoment,
                target,
                fieldOrComponent,
                success,
                "ProofEnhancer",
                relatedStableIds);
        }

        private static PyralisAuthoringFact CreateInspectorFact(
            string stableId,
            string displayName,
            PyralisAuthoringFactKind kind,
            string summary,
            string routeRelevance,
            string fieldDescription,
            string target,
            string fieldOrComponent,
            string success,
            string workIntent,
            string[] relatedStableIds)
        {
            PyralisAuthoringNativeAction action = new PyralisAuthoringNativeAction(
                kind == PyralisAuthoringFactKind.CustomizationMoment ? "Customize" : "Assign",
                PyralisAuthoringActionSurface.Inspector,
                target,
                fieldOrComponent,
                success);

            return new PyralisAuthoringFact(
                stableId,
                displayName,
                kind,
                PyralisAuthoringFactSourceKind.InspectorGuide,
                PyralisAuthoringConfidence.Explicit,
                summary,
                routeRelevance,
                string.Empty,
                assignmentFields: kind == PyralisAuthoringFactKind.AssignmentField ? new[] { fieldDescription } : null,
                customizationMoments: kind == PyralisAuthoringFactKind.CustomizationMoment ? new[] { fieldDescription } : null,
                nativeActions: new[] { action },
                workIntent: workIntent,
                relatedStableIds: relatedStableIds);
        }
    }
}
