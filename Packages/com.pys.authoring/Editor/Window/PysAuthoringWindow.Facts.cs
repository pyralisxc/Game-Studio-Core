using System.Collections.Generic;
using Pys.Authoring.Editor.Exports;
using Pys.Authoring.Editor.Projections;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Window
{
    public sealed partial class PysAuthoringWindow
    {
        private void DrawFacts()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Facts", EditorStyles.boldLabel);
            if (lastFacts == null)
            {
                EditorGUILayout.HelpBox("Open Settings and scan a scripts folder.", MessageType.Info);
                return;
            }

            DrawInlineCounts(
                "Assemblies: " + lastFacts.AssemblyCount,
                "Namespaces: " + lastFacts.NamespaceCount,
                "Types: " + lastFacts.TypeCount,
                "Scripts: " + lastFacts.ScriptCount,
                "Fields: " + lastFacts.FieldCount,
                "Contracts: " + lastFacts.ContractCount,
                "Validators: " + lastFacts.ValidatorCount,
                "Scene: " + lastFacts.SceneObjectCount,
                "Prefabs: " + lastFacts.PrefabCount,
                "Assets: " + lastFacts.AssetCount,
                "Issues: " + lastFacts.IssueCount);

            string[] factKinds = BuildFactKindFilterOptions(lastFacts);
            if (factsKindFilterIndex < 0 || factsKindFilterIndex >= factKinds.Length)
                factsKindFilterIndex = 0;
            factsKindFilterIndex = EditorGUILayout.Popup("Kind Filter", factsKindFilterIndex, factKinds);
            string nextSearch = EditorGUILayout.TextField("Search", factsSearchText ?? string.Empty);
            if (nextSearch != factsSearchText)
            {
                factsSearchText = nextSearch;
                EditorPrefs.SetString(FactsSearchPrefsKey, factsSearchText ?? string.Empty);
            }

            FactsProjection renderedFacts = RenderedFactsProjection();
            DrawCompactRow("Rows", renderedFacts.Rows.Count.ToString());

            string currentKind = string.Empty;
            for (int i = 0; i < renderedFacts.Rows.Count; i++)
            {
                FactRow row = renderedFacts.Rows[i];
                if (row.Kind != currentKind)
                {
                    currentKind = row.Kind;
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(currentKind) ? "Unknown" : currentKind, EditorStyles.boldLabel);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(row.Label, EditorStyles.boldLabel);
                DrawInlineCounts(
                    "Kind: " + row.Kind,
                    "Sources: " + row.SourceCount,
                    "Confidence: " + row.Confidence);

                DrawWrappedRow("Detail", row.Detail);
                DrawWrappedRow("Source", row.SourcePath);
            }

            if (GUILayout.Button("Export Facts JSON"))
                ProjectionJsonExporter.ExportFacts(renderedFacts, scriptsRoot);
        }

        private FactsProjection RenderedFactsProjection()
        {
            if (lastFacts == null)
                return null;

            string selectedKind = SelectedFactKind();
            FactsProjection projection = new FactsProjection
            {
                AssemblyCount = lastFacts.AssemblyCount,
                NamespaceCount = lastFacts.NamespaceCount,
                TypeCount = lastFacts.TypeCount,
                ScriptCount = lastFacts.ScriptCount,
                FieldCount = lastFacts.FieldCount,
                ContractCount = lastFacts.ContractCount,
                ValidatorCount = lastFacts.ValidatorCount,
                SceneObjectCount = lastFacts.SceneObjectCount,
                PrefabCount = lastFacts.PrefabCount,
                AssetCount = lastFacts.AssetCount,
                IssueCount = lastFacts.IssueCount
            };

            for (int i = 0; i < lastFacts.Rows.Count; i++)
            {
                FactRow row = lastFacts.Rows[i];
                if (!string.IsNullOrWhiteSpace(selectedKind) && row.Kind != selectedKind)
                    continue;
                if (!MatchesFactsSearch(row))
                    continue;

                projection.Rows.Add(row);
            }

            return projection;
        }

        private string[] BuildFactKindFilterOptions(FactsProjection facts)
        {
            List<string> kinds = new List<string> { "All" };
            if (facts == null)
                return kinds.ToArray();

            for (int i = 0; i < facts.Rows.Count; i++)
            {
                string kind = facts.Rows[i].Kind;
                if (!string.IsNullOrWhiteSpace(kind) && !kinds.Contains(kind))
                    kinds.Add(kind);
            }

            return kinds.ToArray();
        }

        private string SelectedFactKind()
        {
            string[] options = BuildFactKindFilterOptions(lastFacts);
            if (factsKindFilterIndex <= 0 || factsKindFilterIndex >= options.Length)
                return string.Empty;

            return options[factsKindFilterIndex];
        }

        private bool MatchesFactsSearch(FactRow row)
        {
            if (row == null)
                return false;
            if (string.IsNullOrWhiteSpace(factsSearchText))
                return true;

            string query = factsSearchText.Trim();
            return ContainsText(row.Kind, query)
                || ContainsText(row.Label, query)
                || ContainsText(row.Detail, query)
                || ContainsText(row.SourcePath, query)
                || ContainsText(row.Confidence, query);
        }
    }
}
