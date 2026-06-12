using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisSelectedContextRenderer
    {
        public static void Draw(
            Object selection,
            PyralisAuthoringRouteReport report,
            PyralisAuthoringSetupGraph graph,
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
                DrawGraphContext(PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow(graph, selection));

                if (GUILayout.Button("Open In Inspector"))
                {
                    Selection.activeObject = selection;
                    EditorGUIUtility.PingObject(selection);
                }
            }

            if (selection is GameObject gameObject)
            {
                DrawGameObjectContext(gameObject);
                return;
            }

            if (selection is Component component)
            {
                DrawComponentContext(component);
                return;
            }

            if (selection is RuntimePatternDefinition pattern)
            {
                DrawRuntimePatternContext(pattern, fillMissingRuntimePatternText);
                return;
            }

            if (selection is GameSetupProfile setupProfile)
            {
                DrawSetupProfileContext(setupProfile);
                return;
            }

            if (selection is SessionDefinition or GameModeDefinition or ParticipantDefinition or PawnDefinition or FeatureModuleDefinition)
                EditorGUILayout.HelpBox(report.RouteGuidance, MessageType.Info);
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
            EditorGUI.indentLevel--;
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

        private static void DrawComponentContext(Component component)
        {
            if (component == null)
                return;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Component Context", EditorStyles.boldLabel);
            DrawComponentRow(component);

            if (component is GameplaySessionBootstrap bootstrap)
            {
                PyralisSetupFlowReport setupFlowReport = PyralisSetupFlowValidator.BuildReport(bootstrap);
                PyralisSetupFlowStep firstBlockingStep = setupFlowReport.FirstBlockingStep;
                if (firstBlockingStep != null)
                    EditorGUILayout.HelpBox(firstBlockingStep.Message, GetMessageType(firstBlockingStep.Status));
                else
                    EditorGUILayout.HelpBox("Required setup is clear. Run the first proof pass first, then handle recommended items while the route grows.", MessageType.Info);
            }
        }

        private static void DrawRuntimePatternContext(
            RuntimePatternDefinition pattern,
            Action<RuntimePatternDefinition> fillMissingRuntimePatternText)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Runtime Pattern Guidance", EditorStyles.boldLabel);

            string description = !string.IsNullOrWhiteSpace(pattern.description)
                ? pattern.description
                : RuntimePatternAuthoringText.GetSuggestedDescription(pattern);
            string setupNotes = !string.IsNullOrWhiteSpace(pattern.setupNotes)
                ? pattern.setupNotes
                : RuntimePatternAuthoringText.GetSuggestedSetupNotes(pattern);

            EditorGUILayout.HelpBox(description, MessageType.Info);
            EditorGUILayout.LabelField("Presentation Lanes", FormatPresentationLanes(pattern.presentationLanes), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("First Proof Requirements", pattern.firstProofRequirements.ToString(), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.HelpBox("Setup notes:\n" + setupNotes, MessageType.None);

            using (new EditorGUILayout.HorizontalScope())
            {
                bool hasMissingText = string.IsNullOrWhiteSpace(pattern.description) || string.IsNullOrWhiteSpace(pattern.setupNotes);
                using (new EditorGUI.DisabledScope(!hasMissingText))
                {
                    if (GUILayout.Button(new GUIContent("Fill Missing Text From Fields", "Fills only empty Description and Setup Notes from the selected pattern fields. It does not choose a route, assign requirements, or create setup content.")))
                        fillMissingRuntimePatternText?.Invoke(pattern);
                }

                if (GUILayout.Button("Copy Guidance"))
                    EditorGUIUtility.systemCopyBuffer = description + "\n\nSetup notes:\n" + setupNotes;
            }
        }

        private static void DrawSetupProfileContext(GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(4f);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("Open Intent to choose or revise setup profile capability ingredients. Guide keeps this selected-profile view read-only so route shaping stays in one place.", MessageType.Info);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Optional Route Contracts", EditorStyles.boldLabel);

            if (setupProfile.runtimePatterns == null || setupProfile.runtimePatterns.Length == 0)
            {
                EditorGUILayout.HelpBox("No optional runtime pattern assets are assigned. That is fine for generic capability-first setup; add one only when a route needs reusable advanced metadata.", MessageType.Info);
                return;
            }

            for (int i = 0; i < setupProfile.runtimePatterns.Length; i++)
            {
                RuntimePatternDefinition pattern = setupProfile.runtimePatterns[i];
                if (pattern == null)
                {
                    EditorGUILayout.HelpBox($"Pattern slot {i} is empty.", MessageType.Warning);
                    continue;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(GetRuntimePatternLabel(pattern), $"{pattern.capabilityFamily} / {pattern.participantEmbodiment}");
                    if (GUILayout.Button("Select", GUILayout.Width(72f)))
                        Selection.activeObject = pattern;
                }
            }
        }

        private static string GetRuntimePatternLabel(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return "Missing pattern";

            if (!string.IsNullOrWhiteSpace(pattern.displayName))
                return pattern.displayName;

            if (!string.IsNullOrWhiteSpace(pattern.patternId))
                return pattern.patternId;

            return pattern.name;
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

        private static MessageType GetMessageType(PyralisSetupFlowStepStatus status)
        {
            return status == PyralisSetupFlowStepStatus.Missing || status == PyralisSetupFlowStepStatus.Blocked
                ? MessageType.Warning
                : MessageType.Info;
        }

        private static string FormatPresentationLanes(RuntimePatternPresentationLane[] lanes)
        {
            if (lanes == null || lanes.Length == 0)
                return "None assigned";

            string[] labels = new string[lanes.Length];
            for (int i = 0; i < lanes.Length; i++)
                labels[i] = lanes[i].ToString();

            return string.Join(", ", labels);
        }
    }
}
