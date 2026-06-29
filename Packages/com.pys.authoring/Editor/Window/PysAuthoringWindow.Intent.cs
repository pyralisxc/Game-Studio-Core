using System.Collections.Generic;
using Pys.Authoring.Editor.Exports;
using Pys.Authoring.Editor.Projections;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Window
{
    public sealed partial class PysAuthoringWindow
    {
        private void DrawIntent()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Intent", EditorStyles.boldLabel);
            if (lastIntent == null)
            {
                EditorGUILayout.HelpBox("Open Settings and scan a scripts folder.", MessageType.Info);
                return;
            }

            DrawIntentControls();

            DrawIntentCandidateToggles();

            IntentRow selectedRow = FindIntentRow(lastIntent, selectedIntentContractId);
            if (selectedRow == null)
            {
                EditorGUILayout.HelpBox("No intent selected. Select a target Intent or enable Unity Setup Guides when no target Intent exists.", MessageType.Info);
                DrawIntentCandidateSummary();
            }
            else
            {
                DrawSelectedIntentWorkspace(selectedRow);
            }

            if (GUILayout.Button("Export Intent JSON"))
                ProjectionJsonExporter.ExportIntent(RenderedIntentProjection(), scriptsRoot);
        }

        private void DrawIntentControls()
        {
            DrawInlineCounts(
                "Selectable: " + lastIntent.SelectableCount,
                "Source: " + IntentSourceSummary());

            intentShowDisabledCandidates = EditorGUILayout.Toggle("Show Disabled Candidates", intentShowDisabledCandidates);
            bool nextShowUnitySetupGuides = EditorGUILayout.Toggle("Show Unity Setup Guides", intentShowUnitySetupGuides);
            if (nextShowUnitySetupGuides != intentShowUnitySetupGuides)
            {
                intentShowUnitySetupGuides = nextShowUnitySetupGuides;
                EditorPrefs.SetBool(IntentShowUnitySetupGuidesPrefsKey, intentShowUnitySetupGuides);
                RebuildSelectedPathProjections();
            }

            if (!string.IsNullOrWhiteSpace(lastIntent.SelectedDisabledReason))
                EditorGUILayout.HelpBox(lastIntent.SelectedDisabledReason, MessageType.Warning);
        }

        private void DrawSelectedIntentWorkspace(IntentRow row)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(row.DisplayName, EditorStyles.boldLabel);
            DrawTagLine(row);
            DrawOptionalHelp(row.Summary, MessageType.None);
            DrawOptionalHelp(BuildIntentSummary(row), MessageType.Info);
            DrawOptionalHelp(row.DisabledReason, MessageType.Warning);

            DrawIntentToggleRows(row);
            DrawIntentLaneRows(row);

            DrawIntentMetadataBlock("Readiness", row.SuccessDescription, row.ReadinessHint, row.ExpectedEvidence, row.CompletionSignals);
            DrawIntentMetadataBlock("Relationships", row.CompatibleStableIds, row.SupportingStableIds, row.ValidationOwnerStableId, string.Empty);
            DrawIntentMetadataBlock("Hover Explanations", row.HoverExplanations, string.Empty, string.Empty, string.Empty);
        }

        private void DrawTagLine(IntentRow row)
        {
            DrawInlineCounts(
                "Source: " + (string.IsNullOrWhiteSpace(row.IntentSource) ? "TargetContract" : row.IntentSource),
                "Category: " + row.Category,
                "Surface: " + row.Surface,
                "Dependencies: " + row.DependencyCount);
            DrawWrappedRow("Capability", row.CapabilityPath);
            DrawCompactRow("Pattern", row.OrganizationPattern);
        }

        private void DrawIntentToggleRows(IntentRow row)
        {
            string[] toggles = SplitMetadataLines(row.IntentToggles);
            if (toggles.Length == 0)
                return;

            EnsureIntentCompositionDefaults(row);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Feature Toggles", EditorStyles.boldLabel);
            List<string> selected = SelectedFeatureList();
            for (int i = 0; i < toggles.Length; i++)
            {
                string toggle = toggles[i];
                bool current = selected.Contains(toggle);
                bool next = EditorGUILayout.ToggleLeft(new GUIContent(toggle, TooltipFor(row, i)), current);
                if (next == current)
                    continue;

                if (next)
                    selected.Add(toggle);
                else
                    selected.Remove(toggle);

                selectedIntentFeatureToggles = string.Join("\n", selected.ToArray());
                EditorPrefs.SetString(IntentSelectedFeaturesPrefsKey, selectedIntentFeatureToggles);
            }
        }

        private void DrawIntentLaneRows(IntentRow row)
        {
            string[] lanes = SplitMetadataLines(row.IntentLanes);
            if (lanes.Length == 0)
                return;

            EnsureIntentCompositionDefaults(row);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Lanes", EditorStyles.boldLabel);
            int selectedLane = LaneIndex(lanes, selectedIntentLane);
            int nextLane = EditorGUILayout.Popup("Lane", selectedLane, lanes);
            if (nextLane != selectedLane)
            {
                selectedIntentLane = lanes[nextLane];
                EditorPrefs.SetString(IntentSelectedLanePrefsKey, selectedIntentLane);
            }
        }

        private void DrawIntentCandidateToggles()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Intent Candidates", EditorStyles.boldLabel);
            bool anyVisible = false;
            for (int i = 0; i < lastIntent.Rows.Count; i++)
            {
                IntentRow row = lastIntent.Rows[i];
                if (row == null)
                    continue;
                if (!intentShowDisabledCandidates && !IsSelectableIntent(row))
                    continue;

                anyVisible = true;
                bool selected = row.ContractId == selectedIntentContractId;
                using (new EditorGUI.DisabledScope(!IsSelectableIntent(row)))
                {
                    bool next = EditorGUILayout.ToggleLeft(new GUIContent(IntentOptionLabel(row), row.Summary), selected);
                    if (next && !selected)
                        SelectIntent(row.ContractId);
                    else if (!next && selected)
                        SelectIntent(string.Empty);
                }

                if (!string.IsNullOrWhiteSpace(row.DisabledReason))
                    DrawWrappedRow("Disabled", row.DisabledReason);
            }

            if (!anyVisible)
                EditorGUILayout.HelpBox("No selectable Intent candidates are visible for the current filters.", MessageType.Info);
        }

        private void DrawIntentMetadataBlock(string label, params string[] values)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < values.Length; i++)
            {
                string[] split = SplitMetadataLines(values[i]);
                for (int splitIndex = 0; splitIndex < split.Length; splitIndex++)
                    lines.Add(split[splitIndex]);
            }

            if (lines.Count == 0)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            for (int i = 0; i < lines.Count; i++)
                EditorGUILayout.LabelField(lines[i], EditorStyles.wordWrappedLabel);
        }

        private void DrawIntentCandidateSummary()
        {
            int targetCount = CountIntentRowsBySource("TargetContract");
            int builtInCount = CountIntentRowsBySource("BuiltInUnitySetup");
            DrawInlineCounts(
                "Target Intent Rows: " + targetCount,
                "Unity Setup Guide Rows: " + builtInCount);
        }

        private string IntentSourceSummary()
        {
            int targetCount = CountIntentRowsBySource("TargetContract");
            int builtInCount = CountIntentRowsBySource("BuiltInUnitySetup");
            return targetCount + " target / " + builtInCount + " Unity setup";
        }

        private int CountIntentRowsBySource(string source)
        {
            if (lastIntent == null)
                return 0;

            int count = 0;
            for (int i = 0; i < lastIntent.Rows.Count; i++)
            {
                string rowSource = string.IsNullOrWhiteSpace(lastIntent.Rows[i].IntentSource) ? "TargetContract" : lastIntent.Rows[i].IntentSource;
                if (rowSource == source)
                    count++;
            }

            return count;
        }

        private static IntentRow FindIntentRow(IntentProjection intent, string contractId)
        {
            if (intent == null || string.IsNullOrWhiteSpace(contractId))
                return null;

            for (int i = 0; i < intent.Rows.Count; i++)
            {
                if (intent.Rows[i].ContractId == contractId)
                    return intent.Rows[i];
            }

            return null;
        }

        private static bool IsSelectableIntent(IntentRow row)
        {
            return row != null && row.Selectable && string.IsNullOrWhiteSpace(row.DisabledReason);
        }

        private static string IntentOptionLabel(IntentRow row)
        {
            if (row == null)
                return string.Empty;

            string source = row.IntentSource == "BuiltInUnitySetup" ? "Unity" : "Target";
            string label = string.IsNullOrWhiteSpace(row.CapabilityPath)
                ? row.DisplayName
                : row.DisplayName + " - " + row.CapabilityPath;
            return "[" + source + "] " + label;
        }

        private string BuildIntentSummary(IntentRow row)
        {
            if (row == null)
                return string.Empty;

            EnsureIntentCompositionDefaults(row);
            List<string> parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(selectedIntentLane))
                parts.Add("lane: " + selectedIntentLane);
            if (!string.IsNullOrWhiteSpace(selectedIntentFeatureToggles))
                parts.Add("features: " + CompactLines(selectedIntentFeatureToggles));
            if (!string.IsNullOrWhiteSpace(row.SupportingStableIds))
                parts.Add("supporting: " + CompactLines(row.SupportingStableIds));

            if (parts.Count == 0)
                return string.Empty;

            return "Selected intent composition uses " + string.Join("; ", parts) + ".";
        }

        private void EnsureIntentCompositionDefaults(IntentRow row)
        {
            if (row == null)
                return;

            string[] toggles = SplitMetadataLines(row.IntentToggles);
            if (string.IsNullOrWhiteSpace(selectedIntentFeatureToggles) && toggles.Length > 0)
            {
                selectedIntentFeatureToggles = string.Join("\n", toggles);
                EditorPrefs.SetString(IntentSelectedFeaturesPrefsKey, selectedIntentFeatureToggles);
            }
            else if (toggles.Length > 0)
            {
                List<string> allowed = new List<string>(toggles);
                List<string> selected = SelectedFeatureList();
                bool changed = false;
                for (int i = selected.Count - 1; i >= 0; i--)
                {
                    if (allowed.Contains(selected[i]))
                        continue;

                    selected.RemoveAt(i);
                    changed = true;
                }

                if (changed)
                {
                    selectedIntentFeatureToggles = string.Join("\n", selected.ToArray());
                    EditorPrefs.SetString(IntentSelectedFeaturesPrefsKey, selectedIntentFeatureToggles);
                }
            }

            string[] lanes = SplitMetadataLines(row.IntentLanes);
            if (string.IsNullOrWhiteSpace(selectedIntentLane) && lanes.Length > 0)
            {
                selectedIntentLane = lanes[0];
                EditorPrefs.SetString(IntentSelectedLanePrefsKey, selectedIntentLane);
            }
            else if (lanes.Length > 0 && LaneIndex(lanes, selectedIntentLane) == 0 && selectedIntentLane != lanes[0])
            {
                selectedIntentLane = lanes[0];
                EditorPrefs.SetString(IntentSelectedLanePrefsKey, selectedIntentLane);
            }
        }

        private List<string> SelectedFeatureList()
        {
            return new List<string>(SplitMetadataLines(selectedIntentFeatureToggles));
        }

        private static int LaneIndex(string[] lanes, string selectedLane)
        {
            if (lanes == null || lanes.Length == 0)
                return 0;

            for (int i = 0; i < lanes.Length; i++)
            {
                if (lanes[i] == selectedLane)
                    return i;
            }

            return 0;
        }

        private string SelectedIntentFeatureTogglesForExport()
        {
            IntentRow row = FindIntentRow(lastIntent, selectedIntentContractId);
            if (row == null)
                return string.Empty;

            EnsureIntentCompositionDefaults(row);
            return selectedIntentFeatureToggles ?? string.Empty;
        }

        private string SelectedIntentLaneForExport()
        {
            IntentRow row = FindIntentRow(lastIntent, selectedIntentContractId);
            if (row == null)
                return string.Empty;

            EnsureIntentCompositionDefaults(row);
            return selectedIntentLane ?? string.Empty;
        }

        private string SelectedIntentCompositionSummaryForExport()
        {
            IntentRow row = FindIntentRow(lastIntent, selectedIntentContractId);
            return row == null ? string.Empty : BuildIntentSummary(row);
        }

        private static string TooltipFor(IntentRow row, int index)
        {
            string[] explanations = SplitMetadataLines(row.HoverExplanations);
            if (index >= 0 && index < explanations.Length)
                return explanations[index];

            return string.Empty;
        }
    }
}
