using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Exports;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Window
{
    public sealed partial class PysAuthoringWindow
    {
        private void DrawSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Settings defines the observation scope. PYS reads evidence inside this folder, then each tab renders its own projection.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                string nextScriptsRoot = EditorGUILayout.TextField("Scripts Folder", scriptsRoot);
                if (nextScriptsRoot != scriptsRoot)
                {
                    scriptsRoot = nextScriptsRoot;
                    MarkScanStale();
                }

                if (GUILayout.Button("Choose", GUILayout.Width(80)))
                    ChooseScriptsRoot();
            }

            DrawScanState();

            if (GUILayout.Button(lastGraph == null ? "Scan Now" : "Refresh Scan"))
                Scan();

            DrawFirstRunReadiness();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Exports", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(AuthoringGraphJsonExporter.DefaultExportFolder, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (GUILayout.Button("Open Export Folder"))
                AuthoringGraphJsonExporter.OpenExportFolder();
        }

        private void DrawScanState()
        {
            if (lastGraph == null)
            {
                EditorGUILayout.HelpBox("No scan has run in this window session.", MessageType.Info);
                return;
            }

            if (scanStale)
                EditorGUILayout.HelpBox("Scan stale. Refresh to rebuild all projections from the selected folder.", MessageType.Warning);
            else
                EditorGUILayout.HelpBox("Scan current. Projection tabs and exports are using the same compiled graph.", MessageType.Info);
        }

        private void DrawFirstRunReadiness()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
            AuthoringMode mode = CurrentMode();
            EditorGUILayout.LabelField("Current", ModeLabel(mode));
            EditorGUILayout.HelpBox(ModeDescription(mode), ModeMessageType(mode));

            if (lastGraph == null)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Observed Evidence", EditorStyles.boldLabel);
            DrawInlineCounts(
                "Target Contracts: " + CountTargetContracts(),
                "Unity Setup Guides: " + CountBuiltInUnitySetupContracts(),
                "Intent Candidates: " + CountTargetGoalContracts(),
                "Validators: " + CountGraphNodes(AuthoringGraphNodeKind.Validator),
                "Scene Objects: " + CountGraphNodes(AuthoringGraphNodeKind.SceneObject),
                "Prefabs: " + CountGraphNodes(AuthoringGraphNodeKind.Prefab),
                "Assets: " + CountGraphNodes(AuthoringGraphNodeKind.Asset),
                "Issues: " + CountGraphNodes(AuthoringGraphNodeKind.Issue),
                "Nodes: " + observedNodeCount,
                "Edges: " + observedEdgeCount);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Authoring Readiness", EditorStyles.boldLabel);
            if (CountTargetGoalContracts() == 0)
                EditorGUILayout.HelpBox("Target-project Intent is not available yet. Facts, Hygiene, and Map are still useful, and Unity Setup Guides can cover native Unity workflows.", MessageType.Info);
            else if (string.IsNullOrWhiteSpace(selectedIntentContractId))
                EditorGUILayout.HelpBox("Target Intent candidates were observed. Select an Intent to build the Guide readiness path.", MessageType.Info);
            else
                EditorGUILayout.HelpBox("A selected Intent is active. Guide owns the readiness path; Map remains current scene/setup reality.", MessageType.Info);

            if (CountGraphNodes(AuthoringGraphNodeKind.Validator) == 0)
                EditorGUILayout.HelpBox("No runtime validation methods were observed. Target projects can expose public GetRuntimeValidationIssues methods for current setup readiness.", MessageType.Info);
            if (!GraphHasRouteMetadata())
                EditorGUILayout.HelpBox("No route metadata was observed. Add prerequisite stable IDs and route stage/order metadata for richer Guide ordering.", MessageType.Info);
        }

        private static string ModeLabel(AuthoringMode mode)
        {
            switch (mode)
            {
                case AuthoringMode.WaitingForScan:
                    return "Waiting For Scan";
                case AuthoringMode.Observer:
                    return "Observer";
                case AuthoringMode.UnitySetupAvailable:
                    return "Unity Setup Available";
                case AuthoringMode.TargetIntentReady:
                    return "Target Intent Ready";
                case AuthoringMode.IntentSelected:
                    return "Intent Selected";
                default:
                    return mode.ToString();
            }
        }

        private static string ModeDescription(AuthoringMode mode)
        {
            switch (mode)
            {
                case AuthoringMode.WaitingForScan:
                    return "Choose a scripts folder and scan. Fresh projects can use Facts, Hygiene, and Map before adding contracts.";
                case AuthoringMode.Observer:
                    return "PYS is observing evidence, but no target Intent or built-in setup guide is available in the current graph.";
                case AuthoringMode.UnitySetupAvailable:
                    return "No target Intent is available. Built-in Unity setup guides can help with native Unity workflows when enabled in Intent.";
                case AuthoringMode.TargetIntentReady:
                    return "Target Intent candidates were observed. Select one in Intent to build Overview and Guide.";
                case AuthoringMode.IntentSelected:
                    return "A selected Intent is steering Overview and Guide. Exports mirror the rendered projection packets.";
                default:
                    return string.Empty;
            }
        }

        private static MessageType ModeMessageType(AuthoringMode mode)
        {
            switch (mode)
            {
                case AuthoringMode.TargetIntentReady:
                case AuthoringMode.IntentSelected:
                    return MessageType.Info;
                case AuthoringMode.UnitySetupAvailable:
                    return MessageType.Info;
                default:
                    return MessageType.None;
            }
        }
    }
}
