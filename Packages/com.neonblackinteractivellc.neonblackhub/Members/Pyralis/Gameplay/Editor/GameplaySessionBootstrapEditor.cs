using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Input;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(GameplaySessionBootstrap))]
    public class GameplaySessionBootstrapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Gameplay Session Bootstrap",
                defaultOpen: true,
                new PyralisGuideSection(
                    "What This Is",
                    "GameplaySessionBootstrap is the scene startup root. It reads the session definition, creates or receives core services, spawns participants, injects runtime context, and hands the participant roster to the camera rig.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("CANONICAL_SETUP.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Assign Session Definition for the playable route.",
                        "For pawn-backed tests, add at least one Spawn Point.",
                        "Keep Auto Create Core Services enabled for first proofs unless you intentionally placed custom SessionState, ParticipantRoster, ParticipantSpawn, and InputRouter services in the scene.",
                        "Assign Camera Rig Controller to the scene CinemachineCameraRigController when pawns need shared camera follow or 2D camera bounds.",
                        "Leave Camera Bounds Source empty for the normal route; use it only for a specialized custom ICameraBoundsProvider."
                    }),
                new PyralisGuideSection(
                    "Runtime Wiring",
                    null,
                    new[]
                    {
                        "At Play, Bootstrap creates or resolves ParticipantRosterService, then calls Camera Rig Controller > SetParticipantRoster.",
                        "The pawn prefab should not carry the scene camera. The scene/session owns Main Camera, Cinemachine Camera, Camera Rig Controller, roster, gameplay state, and bounds.",
                        "If the Cinemachine camera still has no tracking target during Play, inspect this Bootstrap, the assigned Camera Rig Controller, and whether participants actually spawned."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(GetBootstrapMessages(serializedObject), "GameplaySessionBootstrap is ready to start the authored scene route.");
            PyralisInspectorHandoff.DrawAuthoringButton();

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetBootstrapMessages(SerializedObject serializedObject)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty sessionDefinition = serializedObject.FindProperty("sessionDefinition");
            SerializedProperty autoCreateCoreServices = serializedObject.FindProperty("autoCreateCoreServices");
            SerializedProperty injectLoadedScenesOnBuild = serializedObject.FindProperty("injectLoadedScenesOnBuild");
            SerializedProperty spawnPoints = serializedObject.FindProperty("spawnPoints");
            SerializedProperty participantRosterService = serializedObject.FindProperty("participantRosterService");
            SerializedProperty cameraRigController = serializedObject.FindProperty("cameraRigController");
            SerializedProperty cameraBoundsSource = serializedObject.FindProperty("cameraBoundsSource");

            if (sessionDefinition != null && sessionDefinition.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Required("Session Definition is empty. Assign the SessionDefinition that owns the selected GameModeDefinition and participants."));

            if (spawnPoints != null && spawnPoints.arraySize == 0)
                messages.Add(PyralisGuideIssue.Recommended("Spawn Points is empty. Pawn-backed routes need at least one Transform spawn point; no-pawn routes can skip this."));

            bool autoServices = autoCreateCoreServices == null || autoCreateCoreServices.boolValue;
            if (!autoServices)
                messages.Add(PyralisGuideIssue.Recommended("Auto Create Core Services is off. Assign Session State, Participant Roster, Participant Spawn, and Participant Input Router services manually before Play Mode."));
            else if (participantRosterService != null && participantRosterService.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Participant Roster Service is empty, but Auto Create Core Services is on. Bootstrap will create it at Play and pass it to the assigned Camera Rig Controller."));

            if (injectLoadedScenesOnBuild != null && !injectLoadedScenesOnBuild.boolValue)
                messages.Add(PyralisGuideIssue.Optional("Inject Loaded Scenes On Build is off. Keep it on for first proofs unless a custom composition route owns scene injection."));

            if (cameraRigController != null && cameraRigController.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Recommended("Camera Rig Controller is empty. Assign the scene CinemachineCameraRigController when the route needs pawn follow, 2D camera bounds, camera profiles, or shared/split camera control."));
            else if (cameraRigController != null && cameraRigController.objectReferenceValue != null)
                messages.Add(PyralisGuideIssue.Optional("Camera route check: Bootstrap will pass the runtime ParticipantRosterService to this Camera Rig Controller at Play so the Cinemachine camera can follow spawned pawns."));

            if (cameraBoundsSource != null && cameraBoundsSource.objectReferenceValue != null)
                messages.Add(PyralisGuideIssue.Optional("Camera Bounds Source is assigned. This is for a custom ICameraBoundsProvider route; the normal Cinemachine route only needs Camera Rig Controller."));

            return messages;
        }
    }

    [CustomEditor(typeof(PawnRoot))]
    public class PawnRootEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            PyralisInspectorHandoff.DrawAuthoringButton();

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetPawnRootMessages(bool hasPawnDefinition)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!hasPawnDefinition)
                messages.Add(PyralisGuideIssue.Optional("Pawn Definition is empty. This is fine for spawned pawns that receive their definition from ParticipantDefinition at runtime."));

            return messages;
        }
    }

    [CustomEditor(typeof(SessionStateService))]
    public class SessionStateServiceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            PyralisInspectorHandoff.DrawAuthoringButton();

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSessionStateMessages(bool hasSessionDefinition)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!hasSessionDefinition)
                messages.Add(PyralisGuideIssue.Optional("Session Definition is empty. This is expected when GameplaySessionBootstrap injects the session at runtime."));

            return messages;
        }
    }

    [CustomEditor(typeof(ParticipantRosterService))]
    public class ParticipantRosterServiceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            PyralisInspectorHandoff.DrawAuthoringButton();

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetRosterMessages(bool hasSessionDefinition)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!hasSessionDefinition)
                messages.Add(PyralisGuideIssue.Optional("Session Definition is empty. This is expected when GameplaySessionBootstrap injects it at runtime."));

            return messages;
        }
    }

    [CustomEditor(typeof(ParticipantSpawnService))]
    public class ParticipantSpawnServiceEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            PyralisInspectorHandoff.DrawAuthoringButton();

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetSpawnMessages(bool hasRosterService, bool hasSessionStateService, int spawnPointCount)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!hasRosterService)
                messages.Add(PyralisGuideIssue.Optional("Roster Service is empty. This is expected when GameplaySessionBootstrap injects it at runtime."));

            if (!hasSessionStateService)
                messages.Add(PyralisGuideIssue.Optional("Session State Service is empty. This is expected when GameplaySessionBootstrap injects it at runtime."));

            if (spawnPointCount == 0)
                messages.Add(PyralisGuideIssue.Recommended("Spawn Points is empty. Add spawn points for pawn-backed games; skip them for no-pawn games."));

            return messages;
        }
    }

    [CustomEditor(typeof(ParticipantInputRouter))]
    public class ParticipantInputRouterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            PyralisInspectorHandoff.DrawAuthoringButton();

            serializedObject.ApplyModifiedProperties();
        }

        private static List<PyralisGuideIssue> GetInputRouterMessages(bool hasSessionDefinition, bool hasRosterService)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (!hasSessionDefinition)
                messages.Add(PyralisGuideIssue.Optional("Session Definition is empty. This is expected when GameplaySessionBootstrap injects it at runtime."));

            if (!hasRosterService)
                messages.Add(PyralisGuideIssue.Optional("Roster Service is empty. This is expected when GameplaySessionBootstrap injects it at runtime."));

            return messages;
        }
    }

    internal static class PyralisInspectorHandoff
    {
        public static void DrawAuthoringButton()
        {
            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Open Pyralis Authoring"))
                NeonBlack.Gameplay.Editor.PyralisAuthoringWindow.Open();
        }
    }
}
