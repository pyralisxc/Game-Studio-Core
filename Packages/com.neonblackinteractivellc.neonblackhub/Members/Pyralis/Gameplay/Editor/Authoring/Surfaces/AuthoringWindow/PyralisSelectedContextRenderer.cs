using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisSelectedContextRenderer
    {
        public static void Draw(
            Object selection,
            PyralisAuthoringSetupGraph graph,
            PyralisAuthoringCurrentStepGraphRow currentStep,
            Action<RuntimePatternDefinition> fillMissingRuntimePatternText)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Selected Authoring Context", EditorStyles.boldLabel);

            if (selection == null)
            {
                EditorGUILayout.HelpBox("Select a Pyralis scene object, component, definition, profile, or authored asset to make this window show the authoring context for that selection.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Selection", selection.name);
                EditorGUILayout.LabelField("Type", selection.GetType().Name);
                PyralisAuthoringSelectedContextGraphRow selectedContext = PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow(graph, selection);
                DrawGraphContext(selectedContext);

                if (GUILayout.Button("Open In Inspector"))
                {
                    Selection.activeObject = selection;
                    EditorGUIUtility.PingObject(selection);
                }

                if (selection is RuntimePatternDefinition pattern)
                    DrawRuntimePatternActions(pattern, selectedContext, fillMissingRuntimePatternText);
            }

            if (selection is GameObject gameObject)
            {
                DrawGameObjectContext(gameObject);
                return;
            }

            if (selection is Component component)
            {
                DrawComponentContext(component, currentStep);
                return;
            }

            if (selection is SessionDefinition or GameModeDefinition or ParticipantDefinition or PawnDefinition or FeatureModuleDefinition)
                EditorGUILayout.HelpBox(GetGraphContextGuidance(graph, selection, currentStep), MessageType.Info);
        }

        private static void DrawGraphContext(PyralisAuthoringSelectedContextGraphRow row)
        {
            if (row == null)
                return;

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Resolved Graph Context", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Node", string.IsNullOrWhiteSpace(row.NodeId) ? "No matching graph node yet" : row.NodeId, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Evidence", GetEvidenceLabel(row.EvidenceState), EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrWhiteSpace(row.Role))
                EditorGUILayout.LabelField("Role", row.Role, EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrWhiteSpace(row.NextCheck))
                EditorGUILayout.LabelField("Next Check", row.NextCheck, EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrWhiteSpace(row.NativeSetup))
                EditorGUILayout.LabelField("Native Setup", row.NativeSetup, EditorStyles.wordWrappedMiniLabel);
            DrawSelectedContextDetails(row);
            EditorGUI.indentLevel--;
        }

        private static void DrawSelectedContextDetails(PyralisAuthoringSelectedContextGraphRow row)
        {
            if (row == null || row.Details.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Selection Details", EditorStyles.miniBoldLabel);
            for (int i = 0; i < row.Details.Count; i++)
            {
                PyralisAuthoringSelectedContextDetail detail = row.Details[i];
                if (detail == null)
                    continue;

                if (!detail.CanSelectTarget)
                {
                    EditorGUILayout.LabelField(detail.Label, detail.Value, EditorStyles.wordWrappedMiniLabel);
                    continue;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(detail.Label, detail.Value, EditorStyles.wordWrappedMiniLabel);
                    if (GUILayout.Button("Select", GUILayout.Width(72f)))
                        Selection.activeObject = detail.Target;
                }
            }
        }

        private static string GetGraphContextGuidance(
            PyralisAuthoringSetupGraph graph,
            Object selection,
            PyralisAuthoringCurrentStepGraphRow currentStep)
        {
            PyralisAuthoringSelectedContextGraphRow row = PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow(graph, selection);
            if (row != null && !string.IsNullOrWhiteSpace(row.RuntimeMeaning))
                return row.RuntimeMeaning;

            if (currentStep != null && !string.IsNullOrWhiteSpace(currentStep.Message))
                return currentStep.Message;

            return "Use the Inspector for this selected asset, and use Map or Validate to see how it participates in the resolved setup graph.";
        }

        private static void DrawGameObjectContext(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents<Component>();
            List<Component> pyralisComponents = new List<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (IsPyralisComponent(component))
                    pyralisComponents.Add(component);
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Pyralis components on this GameObject", EditorStyles.boldLabel);

            if (pyralisComponents.Count == 0)
            {
                EditorGUILayout.HelpBox("No Pyralis components were found on this GameObject. Select a Gameplay Root, pawn prefab root, camera root, tabletop presenter, UI presenter, or specific Pyralis component.", MessageType.Info);
                return;
            }

            for (int i = 0; i < pyralisComponents.Count; i++)
                DrawComponentRow(pyralisComponents[i]);

            Component authoringRoot = FindLikelyAuthoringRoot(pyralisComponents);
            if (authoringRoot != null)
            {
                EditorGUILayout.HelpBox($"Likely authoring root: {authoringRoot.GetType().Name}. Select it when you want the most specific Inspector while keeping this window open for route guidance.", MessageType.Info);
                if (GUILayout.Button($"Select {authoringRoot.GetType().Name}"))
                    Selection.activeObject = authoringRoot;
            }
        }

        private static void DrawComponentContext(Component component, PyralisAuthoringCurrentStepGraphRow currentStep)
        {
            if (component == null)
                return;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Component Context", EditorStyles.boldLabel);
            DrawComponentRow(component);

            if (component is GameplaySessionBootstrap)
            {
                if (currentStep != null && currentStep.HasNode)
                    EditorGUILayout.HelpBox(currentStep.Message, GetMessageType(currentStep.EvidenceState));
                else
                    EditorGUILayout.HelpBox("Required setup is clear. Run the first proof pass first, then handle recommended items while the route grows.", MessageType.Info);
            }
        }

        private static void DrawRuntimePatternActions(
            RuntimePatternDefinition pattern,
            PyralisAuthoringSelectedContextGraphRow selectedContext,
            Action<RuntimePatternDefinition> fillMissingRuntimePatternText)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Runtime Pattern Actions", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(selectedContext == null || !selectedContext.HasMissingRuntimePatternText))
                {
                    if (GUILayout.Button(new GUIContent("Fill Missing Text From Fields", "Fills only empty Description and Setup Notes from the selected pattern fields. It does not choose a route, assign requirements, or create setup content.")))
                        fillMissingRuntimePatternText?.Invoke(pattern);
                }

                using (new EditorGUI.DisabledScope(selectedContext == null || string.IsNullOrWhiteSpace(selectedContext.CopyGuidance)))
                {
                    if (GUILayout.Button("Copy Guidance"))
                        EditorGUIUtility.systemCopyBuffer = selectedContext.CopyGuidance;
                }
            }
        }

        private static void DrawComponentRow(Component component)
        {
            if (component == null)
            {
                EditorGUILayout.HelpBox("Missing script on this GameObject.", MessageType.Warning);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(component.GetType().Name, GetComponentRole(component), EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("Select", GUILayout.Width(72f)))
                    Selection.activeObject = component;
            }
        }

        private static bool IsPyralisComponent(Component component)
        {
            if (component == null)
                return true;

            string namespaceName = component.GetType().Namespace ?? string.Empty;
            return namespaceName.StartsWith("NeonBlack.Gameplay", StringComparison.Ordinal);
        }

        private static Component FindLikelyAuthoringRoot(List<Component> components)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is GameplaySessionBootstrap)
                    return components[i];
            }

            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is PawnRoot)
                    return components[i];
            }

            return components.Count > 0 ? components[0] : null;
        }

        private static string GetComponentRole(Component component)
        {
            if (component == null)
                return "Missing script";

            return component switch
            {
                GameplaySessionBootstrap => "scene startup and setup-flow root",
                PawnRoot => "pawn composition root",
                _ => "runtime or authoring component"
            };
        }

        private static string GetEvidenceLabel(PyralisAuthoringGraphEvidenceState state)
        {
            return state switch
            {
                PyralisAuthoringGraphEvidenceState.Ready => "Ready",
                PyralisAuthoringGraphEvidenceState.Optional => "Optional",
                PyralisAuthoringGraphEvidenceState.Missing => "Missing",
                PyralisAuthoringGraphEvidenceState.CandidateDetected => "Candidate detected",
                PyralisAuthoringGraphEvidenceState.Blocked => "Blocked",
                _ => "Unknown"
            };
        }

        private static MessageType GetMessageType(PyralisAuthoringGraphEvidenceState state)
        {
            return state == PyralisAuthoringGraphEvidenceState.Missing || state == PyralisAuthoringGraphEvidenceState.Blocked
                ? MessageType.Warning
                : MessageType.Info;
        }

    }
}
