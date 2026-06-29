using System.Collections.Generic;
using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Projections;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Window
{
    public sealed partial class PysAuthoringWindow
    {
        private IntentProjection RenderedIntentProjection()
        {
            if (lastIntent == null)
                return null;

            IntentProjection projection = new IntentProjection
            {
                SelectedContractId = lastIntent.SelectedContractId,
                SelectedDisplayName = lastIntent.SelectedDisplayName,
                SelectedDisabledReason = lastIntent.SelectedDisabledReason,
                SelectedFeatureToggles = SelectedIntentFeatureTogglesForExport(),
                SelectedLane = SelectedIntentLaneForExport(),
                SelectedCompositionSummary = SelectedIntentCompositionSummaryForExport()
            };

            for (int i = 0; i < lastIntent.Rows.Count; i++)
            {
                IntentRow row = lastIntent.Rows[i];
                if (!intentShowDisabledCandidates && !IsSelectableIntent(row))
                    continue;

                projection.Rows.Add(row);
                if (IsSelectableIntent(row))
                    projection.SelectableCount++;
            }

            return projection;
        }

        private int CountGraphNodes(AuthoringGraphNodeKind kind)
        {
            if (lastGraph == null)
                return 0;

            int count = 0;
            for (int i = 0; i < lastGraph.Nodes.Count; i++)
            {
                if (lastGraph.Nodes[i].Kind == kind)
                    count++;
            }

            return count;
        }

        private int CountTargetContracts()
        {
            return CountContracts(false, false);
        }

        private int CountBuiltInUnitySetupContracts()
        {
            return CountContracts(true, true);
        }

        private int CountTargetGoalContracts()
        {
            if (lastGraph == null)
                return 0;

            int count = 0;
            for (int i = 0; i < lastGraph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = lastGraph.Nodes[i];
                if (node.Kind != AuthoringGraphNodeKind.Contract || IsBuiltInUnitySetup(node))
                    continue;

                if (IsGoalContract(node))
                    count++;
            }

            return count;
        }

        private int CountContracts(bool builtInOnly, bool includeBuiltIn)
        {
            if (lastGraph == null)
                return 0;

            int count = 0;
            for (int i = 0; i < lastGraph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = lastGraph.Nodes[i];
                if (node.Kind != AuthoringGraphNodeKind.Contract)
                    continue;

                bool builtIn = IsBuiltInUnitySetup(node);
                if (builtInOnly && !builtIn)
                    continue;
                if (!includeBuiltIn && builtIn)
                    continue;

                count++;
            }

            return count;
        }

        private bool GraphHasRouteMetadata()
        {
            if (lastGraph == null)
                return false;

            for (int i = 0; i < lastGraph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = lastGraph.Nodes[i];
                if (node.Kind != AuthoringGraphNodeKind.Contract)
                    continue;

                if (!string.IsNullOrWhiteSpace(Metadata(node, "prerequisiteStableIds"))
                    || !string.IsNullOrWhiteSpace(Metadata(node, "routeStage"))
                    || !string.IsNullOrWhiteSpace(Metadata(node, "setupDomain")))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsGoalContract(AuthoringGraphNode node)
        {
            string surface = Metadata(node, "surface");
            return surface == AuthoringSurface.Goal.ToString()
                || !string.IsNullOrWhiteSpace(Metadata(node, "proofTarget"))
                || !string.IsNullOrWhiteSpace(Metadata(node, "successDescription"))
                || !string.IsNullOrWhiteSpace(Metadata(node, "expectedEvidence"))
                || !string.IsNullOrWhiteSpace(Metadata(node, "completionSignals"))
                || !string.IsNullOrWhiteSpace(Metadata(node, "successChecks"));
        }

        private static bool IsBuiltInUnitySetup(AuthoringGraphNode node)
        {
            return Metadata(node, "sourceKind") == "BuiltInUnitySetup"
                || Metadata(node, "intentSource") == "BuiltInUnitySetup"
                || Metadata(node, "setupGuideKind") == "UnitySetupGuide";
        }

        private static string Metadata(AuthoringGraphNode node, string key)
        {
            if (node == null || string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return node.Metadata.TryGetValue(key, out string value) ? value ?? string.Empty : string.Empty;
        }

        private static void DrawCountRowCompact(string label, int count)
        {
            EditorGUILayout.LabelField(label + ": " + count, GUILayout.MinWidth(120));
        }

        private static void DrawSectionHeader(string label)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        private static void DrawWrappedRow(string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(value, EditorStyles.wordWrappedLabel);
        }

        private static void DrawCompactRow(string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (EditorGUIUtility.currentViewWidth < 520f)
            {
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(value, EditorStyles.wordWrappedLabel);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(120));
                EditorGUILayout.LabelField(value, EditorStyles.wordWrappedLabel);
            }
        }

        private static void DrawInlineCounts(params string[] labelValues)
        {
            if (labelValues == null || labelValues.Length == 0)
                return;

            int index = 0;
            while (index < labelValues.Length)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    int columns = EditorGUIUtility.currentViewWidth < 560f ? 2 : 4;
                    for (int column = 0; column < columns && index < labelValues.Length; column++, index++)
                        EditorGUILayout.LabelField(labelValues[index], GUILayout.MinWidth(120));
                }
            }
        }

        private static string ShortPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            if (value.Length <= 42)
                return value;

            return "..." + value.Substring(value.Length - 39);
        }

        private static void DrawOptionalLabel(string label, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                DrawCompactRow(label, value);
        }

        private static void DrawOptionalHelp(string value, MessageType type)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (type == MessageType.None)
                EditorGUILayout.LabelField(value, EditorStyles.wordWrappedLabel);
            else
                EditorGUILayout.HelpBox(value, type);
        }

        private static string[] SplitMetadataLines(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new string[0];

            string[] raw = value.Split(new[] { '\n', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> lines = new List<string>();
            for (int i = 0; i < raw.Length; i++)
            {
                string line = raw[i].Trim();
                if (!string.IsNullOrWhiteSpace(line))
                    lines.Add(line);
            }

            return lines.ToArray();
        }

        private static string CompactLines(string value)
        {
            string[] lines = SplitMetadataLines(value);
            return string.Join(", ", lines);
        }

        private static bool ContainsText(string value, string query)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !string.IsNullOrWhiteSpace(query)
                && value.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
