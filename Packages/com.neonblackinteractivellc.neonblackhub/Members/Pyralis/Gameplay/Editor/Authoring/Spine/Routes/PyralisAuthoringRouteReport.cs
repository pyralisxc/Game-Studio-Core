using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringRouteReport
    {
        private PyralisAuthoringRouteReport(string routeName, string nextStep, string routeGuidance, List<string> validationIssues)
        {
            RouteName = routeName;
            NextStep = nextStep;
            RouteGuidance = routeGuidance;
            ValidationIssues = validationIssues ?? new List<string>();
        }

        public string RouteName { get; }
        public string NextStep { get; }
        public string RouteGuidance { get; }
        public IReadOnlyList<string> ValidationIssues { get; }

        public static PyralisAuthoringRouteReport Build(Object selection)
        {
            if (selection == null)
            {
                return new PyralisAuthoringRouteReport(
                    "No selection",
                    "Start in Unity: right-click Hierarchy -> Create Empty, name it Gameplay Root, then use Inspector -> Add Component to add GameplaySessionBootstrap.",
                    "The Authoring Window guides the route, but the first scene object is still made with normal Unity workflow. After adding GameplaySessionBootstrap, select it here and wire the SessionDefinition in the Inspector.",
                    new List<string>());
            }

            if (selection is GameplaySessionBootstrap bootstrap)
                return BuildFromAnalysis(PyralisSetupRouteAnalysis.Build(bootstrap), null, bootstrap);

            if (selection is GameObject gameObject && gameObject.GetComponent<GameplaySessionBootstrap>() == null)
            {
                if (!IsSceneSetupRootCandidate(gameObject))
                {
                    return new PyralisAuthoringRouteReport(
                        "Scene support object selected",
                        "Create or select a Gameplay Root first, then add GameplaySessionBootstrap there. Keep camera, lights, art, and playfield objects as scene support objects.",
                        "This object can support the route, but it should not become the composition root. Use Hierarchy -> Create Empty for Gameplay Root, add GameplaySessionBootstrap in the Inspector, then return to camera or playfield setup once the active setup exists.",
                        GetIssues(selection));
                }

                return new PyralisAuthoringRouteReport(
                    "Scene object selected",
                    $"Use Inspector -> Add Component search for GameplaySessionBootstrap on `{gameObject.name}`.",
                    "This is the right native Unity flow: create and name the scene root in Hierarchy first, then add Pyralis components through the Inspector so the setup object remains visible and editable.",
                    GetIssues(selection));
            }

            if (selection is SessionDefinition session)
                return BuildFromAnalysis(PyralisSetupRouteAnalysis.Build(session), session.GetValidationIssues());

            if (selection is GameModeDefinition mode)
                return BuildFromAnalysis(PyralisSetupRouteAnalysis.Build(mode), mode.GetValidationIssues());

            if (selection is GameSetupProfile setupProfile)
                return BuildFromAnalysis(PyralisSetupRouteAnalysis.Build(setupProfile), setupProfile.GetValidationIssues());

            if (selection is ParticipantDefinition participant)
                return BuildFromParticipant(participant);

            if (selection is PawnDefinition pawn)
                return BuildFromPawn(pawn);

            return new PyralisAuthoringRouteReport(
                "Selected Context",
                "Inspect this asset in the context of the active setup route.",
                "This selected object may be part of setup, but route guidance should come from the active bootstrap, session, game mode, or setup profile. Pin or select one of those route anchors when you want whole-game validation.",
                GetIssues(selection));
        }

        private static bool IsSceneSetupRootCandidate(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            if (gameObject.GetComponent<Camera>() != null || gameObject.GetComponent<Light>() != null)
                return false;

            Component[] components = gameObject.GetComponents<Component>();
            return components.Length <= 1 || gameObject.name.Contains("Gameplay");
        }

        private static PyralisAuthoringRouteReport BuildFromAnalysis(PyralisSetupRouteAnalysis analysis, List<string> validationIssues = null, GameplaySessionBootstrap bootstrap = null)
        {
            if (analysis == null || analysis.Session == null && analysis.Mode == null && analysis.SetupProfile == null)
            {
                return new PyralisAuthoringRouteReport(
                    "No setup route selected",
                    "Create a Session Definition asset in the Project window, then assign it to GameplaySessionBootstrap > Session Definition by drag/drop or the field's object picker circle.",
                    "Use normal Unity workflow first: Project window -> open the target folder so its contents are visible, then right-click inside that content pane -> Create -> NeonBlack -> Definitions -> Session Definition. The Inspector remains the source of truth for assigning it to the scene bootstrap.",
                    validationIssues ?? new List<string>());
            }

            List<string> issues = validationIssues ?? BuildValidationIssues(analysis);

            if (bootstrap != null)
            {
                PyralisSetupFlowReport reflectiveReport = PyralisReflectiveContractSolver.BuildReport(bootstrap);
                for (int i = 0; i < reflectiveReport.Steps.Count; i++)
                {
                    PyralisSetupFlowStep step = reflectiveReport.Steps[i];
                    if (step.Status != PyralisSetupFlowStepStatus.Ready)
                    {
                        if (!issues.Contains(step.Message))
                            issues.Add(step.Message);
                    }
                }
            }
PyralisAuthoringRouteDescriptor descriptor = PyralisAuthoringRouteDescriptor.Build(analysis);
            PyralisAuthoringRouteProof proof = PyralisAuthoringRouteProof.Build(descriptor);
            if (analysis.Session != null && analysis.Mode == null)
            {
                return new PyralisAuthoringRouteReport(
                    "No setup route selected",
                    "Create a Game Mode Definition asset, then select/open the SessionDefinition asset and assign Default Game Mode by drag/drop or the field's object picker circle.",
                    "Use the SessionDefinition Inspector to wire the game rules asset. Pyralis explains the route, while native Unity creation and Inspector fields show which folder and field own the connection.",
                    issues);
            }

            if (analysis.SetupProfile == null)
            {
                return new PyralisAuthoringRouteReport(
                    "No setup route selected",
                    "Create or choose a Game Setup Profile asset, then select/open the GameModeDefinition asset and assign Setup Profile by drag/drop or the field's object picker circle.",
                    "The setup profile is the editable route intent. Create it from the Project window when wiring manually, then choose capability ingredients before adding scene extras.",
                    issues);
            }

            if (!analysis.HasAssignedPatterns)
            {
                return new PyralisAuthoringRouteReport(
                    "No setup route selected",
                    "Select/open the GameSetupProfile asset, then use Authoring Window -> Intent to set DNA axioms, presentation lane, and Engine Spine capabilities.",
                    "Capability families are enough for first-proof guidance. Add an optional RuntimePatternDefinition contract only when the generic capability language cannot describe this route.",
                    issues);
            }

            if (!analysis.HasValidPatterns)
            {
                return new PyralisAuthoringRouteReport(
                    "Incomplete setup capability",
                    "Open the GameSetupProfile and fix its runtime capability rows before adding participants.",
                    "A setup capability or optional RuntimePatternDefinition contract is invalid. Capability rows are the route source of truth; optional contracts can enrich guidance after their metadata is valid.",
                    issues);
            }

            if (analysis.Session == null)
            {
                return new PyralisAuthoringRouteReport(
                    analysis.RouteName,
                    "Assign this setup profile through GameModeDefinition, then SessionDefinition, then check the Bootstrap Setup Flow.",
                    analysis.RequiresPawn
                        ? proof.Guidance
                        : "Pawn prefab can stay empty. " + proof.Guidance,
                    issues);
            }

            if (!analysis.HasParticipants)
            {
                return new PyralisAuthoringRouteReport(
                    analysis.RouteName,
                    "Add at least one player, seat, AI, cursor, hand, or other participant to the session.",
                    "Participants can be players, AI, seats, hands, factions, cameras, cursors, or turn owners.",
                    issues);
            }

            if (analysis.RequiresPawn)
            {
                string pawnIssue = analysis.ParticipantPawnIssue;
                if (!string.IsNullOrWhiteSpace(pawnIssue))
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        pawnIssue,
                        "Pawn-backed routes need ParticipantDefinition > PawnDefinition > pawn prefab before they can spawn actors. For a 2D proof, the prefab root should carry PawnRoot, Motor2D, Motor2DInputAdapter, SpriteRenderer, and Animator, with art and input assigned in visible Inspector fields.",
                        issues);
                }

                if (bootstrap == null)
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        "Select the GameplaySessionBootstrap and assign Spawn Points before running the first proof.",
                        proof.Guidance,
                        issues);
                }

                int assignedSpawnPointCount = CountAssignedSpawnPoints(bootstrap);
                int assignedParticipantCount = CountAssignedParticipants(analysis.Session);
                if (assignedSpawnPointCount == 0)
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        "Create SpawnPoint_1 in the Hierarchy, then select Gameplay Root, expand GameplaySessionBootstrap > Spawn Points, click +, and drag SpawnPoint_1 into Element 0 before the first proof.",
                        "Unity list fields need an element slot before a drag can land. Keep this route manual: the spawn Transform is scene-authored placement, not a preset.",
                        issues);
                }

                if (assignedParticipantCount > assignedSpawnPointCount)
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        $"This scene has {assignedSpawnPointCount} assigned spawn point(s) for {assignedParticipantCount} default participant(s). Add one spawn point per participant, or remove extra participants for a clean 1P proof.",
                        "For the first movement proof, keep the route boring on purpose: one participant, one pawn, one assigned spawn point, then Play Mode.",
                        issues);
                }

                if (!HasAssignedCameraRig(bootstrap))
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        "Create and assign a Camera Root before Play Mode.",
                        "The 2D pawn movement stack needs camera bounds for clamping and framing. Native path: keep or create exactly one enabled physical Unity Camera for this shared proof, usually the default Main Camera; do not delete it for the normal Cinemachine route. Create Camera Root, add CinemachineCameraRigController, create or choose a separate Cinemachine Camera for Shared Camera Behaviour, verify the physical Main Camera is tagged MainCamera with Cinemachine Brain, assign that physical camera as Target Camera, disable or remove accidental extra physical Camera objects only when they were created by mistake, then drag the Camera Root object from Hierarchy into GameplaySessionBootstrap > Camera Rig Controller.",
                        issues);
                }

                if (analysis.Requires2DCameraBounds() && !HasUsable2DCameraBounds(bootstrap, analysis.Mode))
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        "Make the camera rig usable for 2D bounds before Play Mode.",
                        "The 2D movement stack uses the assigned CinemachineCameraRigController as its bounds provider. Select/open the Camera Root and either assign an orthographic CameraRigProfile, or select the physical Target Camera and set Camera > Projection to Orthographic. If using a profile, also assign it to GameModeDefinition > Camera Rig Profile so the rig applies the same intent at runtime.",
                        issues);
                }

                if (analysis.LikelyUsesInputManager())
                {
                    if (!TryGetAssignedPlayerInputManager(bootstrap, out PlayerInputManager playerInputManager))
                    {
                        return new PyralisAuthoringRouteReport(
                            "Pawn-backed local-join route",
                            "For a 1P proof, set SessionDefinition > Max Participants to 1; use PlayerInputManager only for local join.",
                            "Multi-participant local join uses Unity PlayerInputManager to receive join/leave events. Single-player pawn movement should leave Bootstrap > Player Input Manager empty unless the route is intentionally testing local join.",
                            issues);
                    }

                    if (playerInputManager.playerPrefab == null)
                    {
                        return new PyralisAuthoringRouteReport(
                            "Pawn-backed local-join route",
                            "Configure PlayerInputManager > Player Prefab before Play Mode, or disable local join for this proof.",
                            "Unity PlayerInputManager logs runtime errors when join is enabled without a Player Prefab. Use a dedicated PlayerInput prefab for local join; do not use the spawned pawn prefab unless that prefab is intentionally the input-join prefab.",
                            issues);
                    }
                }

                PyralisSetupFlowReport setupFlowReport = PyralisSetupFlowValidator.BuildReport(bootstrap);
                PyralisSetupFlowStep inputProfileStep = setupFlowReport.GetStep("Assign Input Profile");
                if (inputProfileStep != null && inputProfileStep.IsRequiredIssue)
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        inputProfileStep.Message,
                        "Before Play Mode, select/open the effective InputProfile from the participant, PawnDefinition, or SessionDefinition fallback. Assign Actions, verify Primary Action Map, and make Move Action match a movement action in the Input Action Asset.",
                        issues);
                }

                PyralisAuthoringSceneSurfaceRow playfieldSurface = GetSceneSurfaceRow(bootstrap, PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield);
                if (playfieldSurface != null && playfieldSurface.Recommended && !playfieldSurface.Present)
                {
                    return new PyralisAuthoringRouteReport(
                        "Pawn-backed route",
                        "Create a playable Environment / Playfield surface before Play Mode.",
                        "A spawn point only says where the pawn appears; it is not a movement proof surface. In the Hierarchy, create a Ground, Platform, Tilemap, Zone, or Playfield Root that matches the route, then use the Inspector to add an intentional Collider2D, TilemapCollider2D, bounds provider, or project-owned surface component before judging movement, jump, camera follow, or combat spacing in Play Mode.",
                        issues);
                }

                return new PyralisAuthoringRouteReport(
                    "Pawn-backed route",
                    "Enter Play Mode and confirm the first pawn spawns, receives input, and moves.",
                    proof.Guidance,
                    issues);
            }

            return new PyralisAuthoringRouteReport(
                analysis.RouteName,
                proof.FirstUnityFocus,
                "Pawn prefab can stay empty unless this setup later introduces actor bodies. " + proof.Guidance,
                issues);
        }

        private static PyralisAuthoringRouteReport BuildFromParticipant(ParticipantDefinition participant)
        {
            if (participant == null)
                return Build(null);

            string guidance = participant.defaultPawn == null
                ? "Default Pawn can stay empty for no-pawn routes. Assign one only when this participant owns an actor body."
                : "This participant points to a pawn definition. Validate the pawn next if this is a pawn-backed route.";

            return new PyralisAuthoringRouteReport(
                "Participant asset",
                participant.defaultPawn == null ? "Assign this participant to a SessionDefinition." : "Validate the assigned PawnDefinition.",
                guidance,
                new List<string>());
        }

        private static PyralisAuthoringRouteReport BuildFromPawn(PawnDefinition pawn)
        {
            if (pawn == null)
                return Build(null);

            string nextStep = pawn.pawnPrefab == null
                ? "Assign PawnDefinition > Pawn Prefab."
                : "Assign this PawnDefinition to pawn-backed participants.";

            string guidance = pawn.pawnPrefab == null
                ? "A pawn-backed route cannot spawn this pawn until it points to a prefab with PawnRoot on the root GameObject."
                : "Use this only for participants that need spawned or placed actor bodies.";

            return new PyralisAuthoringRouteReport("Pawn-backed asset", nextStep, guidance, PyralisAuthoringValidationModel.BuildPawnRouteValidationIssues(pawn));
        }

        private static PyralisAuthoringSceneSurfaceRow GetSceneSurfaceRow(GameplaySessionBootstrap bootstrap, string surface)
        {
            if (bootstrap == null || string.IsNullOrWhiteSpace(surface))
                return null;

            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(bootstrap);
            for (int i = 0; i < snapshot.Rows.Count; i++)
            {
                PyralisAuthoringSceneSurfaceRow row = snapshot.Rows[i];
                if (row != null && row.Surface == surface)
                    return row;
            }

            return null;
        }

        private static List<string> BuildValidationIssues(PyralisSetupRouteAnalysis analysis)
        {
            if (analysis.Session != null)
                return analysis.Session.GetValidationIssues();

            if (analysis.Mode != null)
                return analysis.Mode.GetValidationIssues();

            if (analysis.SetupProfile != null)
                return analysis.SetupProfile.GetValidationIssues();

            return new List<string>();
        }

        private static List<string> GetIssues(Object selection)
        {
            return selection switch
            {
                SessionDefinition session => session.GetValidationIssues(),
                PawnDefinition pawn => pawn.GetValidationIssues(),
                GameModeDefinition mode => mode.GetValidationIssues(),
                FeatureModuleDefinition module => module.GetValidationIssues(),
                RuntimePatternDefinition pattern => pattern.GetValidationIssues(),
                GameSetupProfile setup => setup.GetValidationIssues(),
                _ => new List<string>()
            };
        }

        private static int CountAssignedSpawnPoints(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return 0;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SerializedProperty spawnPoints = serializedBootstrap.FindProperty("spawnPoints");
            if (spawnPoints == null || !spawnPoints.isArray)
                return 0;

            int count = 0;
            for (int i = 0; i < spawnPoints.arraySize; i++)
            {
                if (spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    count++;
            }

            return count;
        }

        private static int CountAssignedParticipants(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null)
                return 0;

            int count = 0;
            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                if (session.defaultParticipants[i] != null)
                    count++;
            }

            return count;
        }

        private static bool HasAssignedCameraRig(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return false;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            return serializedBootstrap.FindProperty("cameraRigController")?.objectReferenceValue != null;
        }

        private static bool HasUsable2DCameraBounds(GameplaySessionBootstrap bootstrap, GameModeDefinition mode)
        {
            if (!TryGetAssignedCameraRig(bootstrap, out CinemachineCameraRigController rig))
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

        private static bool TryGetAssignedCameraRig(GameplaySessionBootstrap bootstrap, out CinemachineCameraRigController rig)
        {
            rig = null;
            if (bootstrap == null)
                return false;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            rig = serializedBootstrap.FindProperty("cameraRigController")?.objectReferenceValue as CinemachineCameraRigController;
            return rig != null;
        }

        private static bool TryGetAssignedPlayerInputManager(GameplaySessionBootstrap bootstrap, out PlayerInputManager playerInputManager)
        {
            playerInputManager = null;
            if (bootstrap == null)
                return false;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            playerInputManager = serializedBootstrap.FindProperty("playerInputManager")?.objectReferenceValue as PlayerInputManager;
            return playerInputManager != null;
        }
    }
}
