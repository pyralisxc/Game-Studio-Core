using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Exports;
using Pys.Authoring.Editor.Hygiene;
using Pys.Authoring.Editor.Projections;
using Pys.Authoring.Editor.Scanning;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Window
{
    public sealed class PysAuthoringWindow : EditorWindow
    {
        private enum AuthoringTab
        {
            Settings,
            Intent,
            Overview,
            Guide,
            Map,
            Hygiene,
            Facts
        }

        private const string DefaultScriptsRoot = "Assets";
        private const string SelectedIntentPrefsKey = "Pys.Authoring.SelectedIntentContractId";

        private string scriptsRoot = DefaultScriptsRoot;
        private Vector2 hygieneScroll;
        private Vector2 intentScroll;
        private Vector2 guideScroll;
        private Vector2 mapScroll;
        private Vector2 factsScroll;
        private AuthoringTab activeTab = AuthoringTab.Settings;
        private int observedTypeCount;
        private int observedAssemblyCount;
        private int observedSourceFileCount;
        private int observedNodeCount;
        private int observedEdgeCount;
        private int hygieneReviewCount;
        private int hygieneWarningCount;
        private int hygieneErrorCount;
        private int activeHygieneLensIndex;
        private string selectedIntentContractId = string.Empty;
        private AuthoringGraph lastGraph;
        private HygieneProjection lastHygiene;
        private IntentProjection lastIntent;
        private FactsProjection lastFacts;
        private MapProjection lastMap;
        private OverviewProjection lastOverview;
        private GuideProjection lastGuide;

        [MenuItem("Tools/PYS/Authoring")]
        public static void Open()
        {
            GetWindow<PysAuthoringWindow>("PYS Authoring");
        }

        private void OnEnable()
        {
            selectedIntentContractId = EditorPrefs.GetString(SelectedIntentPrefsKey, string.Empty);
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(SelectedIntentPrefsKey, selectedIntentContractId ?? string.Empty);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("PYS Authoring", EditorStyles.boldLabel);
            activeTab = (AuthoringTab)GUILayout.Toolbar((int)activeTab, new[] { "Settings", "Intent", "Overview", "Guide", "Map", "Hygiene", "Facts" });

            switch (activeTab)
            {
                case AuthoringTab.Settings:
                    DrawSettings();
                    break;
                case AuthoringTab.Intent:
                    DrawIntent();
                    break;
                case AuthoringTab.Overview:
                    DrawOverview();
                    break;
                case AuthoringTab.Guide:
                    DrawGuide();
                    break;
                case AuthoringTab.Map:
                    DrawMap();
                    break;
                case AuthoringTab.Hygiene:
                    DrawHygiene();
                    break;
                case AuthoringTab.Facts:
                    DrawFacts();
                    break;
            }
        }

        private void DrawSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Settings only defines the observation scope. Contracts are read only from scripts inside this folder.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                scriptsRoot = EditorGUILayout.TextField("Scripts Folder", scriptsRoot);
                if (GUILayout.Button("Choose", GUILayout.Width(80)))
                    ChooseScriptsRoot();
            }

            if (GUILayout.Button("Scan Now"))
                Scan();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Exports", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(AuthoringGraphJsonExporter.DefaultExportFolder, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (GUILayout.Button("Open Export Folder"))
                AuthoringGraphJsonExporter.OpenExportFolder();
        }

        private void DrawHygiene()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Hygiene", EditorStyles.boldLabel);

            hygieneScroll = EditorGUILayout.BeginScrollView(hygieneScroll);
            EditorGUILayout.LabelField("Scripts Folder", string.IsNullOrWhiteSpace(scriptsRoot) ? DefaultScriptsRoot : scriptsRoot);
            EditorGUILayout.LabelField("Observed Types", observedTypeCount.ToString());
            EditorGUILayout.LabelField("Observed Assembly Definitions", observedAssemblyCount.ToString());
            EditorGUILayout.LabelField("Observed Source Files", observedSourceFileCount.ToString());
            EditorGUILayout.LabelField("Observed Graph Nodes", observedNodeCount.ToString());
            EditorGUILayout.LabelField("Observed Graph Edges", observedEdgeCount.ToString());
            EditorGUILayout.LabelField("Review Rows", hygieneReviewCount.ToString());
            EditorGUILayout.LabelField("Warnings", hygieneWarningCount.ToString());
            EditorGUILayout.LabelField("Errors", hygieneErrorCount.ToString());

            if (lastHygiene != null && lastHygiene.Lenses.Count > 0)
            {
                if (activeHygieneLensIndex < 0 || activeHygieneLensIndex >= lastHygiene.Lenses.Count)
                    activeHygieneLensIndex = 0;

                string[] lensLabels = new string[lastHygiene.Lenses.Count];
                for (int i = 0; i < lastHygiene.Lenses.Count; i++)
                    lensLabels[i] = lastHygiene.Lenses[i].Title;

                activeHygieneLensIndex = GUILayout.Toolbar(activeHygieneLensIndex, lensLabels);
                HygieneLensProjection lens = lastHygiene.Lenses[activeHygieneLensIndex];
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(lens.Question, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Rows", lens.Rows.Count.ToString());
                EditorGUILayout.LabelField("Review Rows", lens.ReviewCount.ToString());
                EditorGUILayout.LabelField("Warnings", lens.WarningCount.ToString());
                EditorGUILayout.LabelField("Errors", lens.ErrorCount.ToString());

                for (int i = 0; i < lens.Rows.Count; i++)
                {
                    HygieneRow row = lens.Rows[i];
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(row.Title, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Lens", row.Lens.ToString());
                    EditorGUILayout.LabelField("Issue", row.IssueCode);
                    EditorGUILayout.LabelField("Severity", row.Severity.ToString());
                    EditorGUILayout.LabelField("Owner", row.OwnerId);
                    EditorGUILayout.LabelField("Detail", row.Detail);
                }
            }

            EditorGUILayout.EndScrollView();

            using (new EditorGUI.DisabledScope(lastGraph == null))
            {
                if (GUILayout.Button("Export Graph JSON"))
                    AuthoringGraphJsonExporter.Export(lastGraph, scriptsRoot);
            }

            using (new EditorGUI.DisabledScope(lastHygiene == null))
            {
                if (GUILayout.Button("Export Hygiene JSON"))
                    HygieneJsonExporter.Export(lastHygiene, scriptsRoot);
            }
        }

        private void DrawOverview()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
            if (lastOverview == null)
            {
                EditorGUILayout.HelpBox("Open Settings and scan a scripts folder.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Summary", lastOverview.Summary);
            EditorGUILayout.LabelField("Selected Intent", lastOverview.SelectedIntent);
            EditorGUILayout.LabelField("Proof Target", lastOverview.ProofTarget);
            EditorGUILayout.LabelField("Readiness", lastOverview.Readiness);
            EditorGUILayout.LabelField("Next", lastOverview.NextAction);
            EditorGUILayout.LabelField("Reason", lastOverview.Reason);
            EditorGUILayout.LabelField("Issues", lastOverview.IssueCount.ToString());

            if (GUILayout.Button("Export Overview JSON"))
                ProjectionJsonExporter.ExportOverview(lastOverview, scriptsRoot);
        }

        private void DrawIntent()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Intent", EditorStyles.boldLabel);
            if (lastIntent == null)
            {
                EditorGUILayout.HelpBox("Open Settings and scan a scripts folder.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Selectable Contracts", lastIntent.SelectableCount.ToString());
            if (!string.IsNullOrWhiteSpace(lastIntent.SelectedDisplayName))
                EditorGUILayout.LabelField("Selected", lastIntent.SelectedDisplayName);
            if (!string.IsNullOrWhiteSpace(lastIntent.SelectedDisabledReason))
                EditorGUILayout.HelpBox(lastIntent.SelectedDisabledReason, MessageType.Warning);

            intentScroll = EditorGUILayout.BeginScrollView(intentScroll);
            for (int i = 0; i < lastIntent.Rows.Count; i++)
            {
                IntentRow row = lastIntent.Rows[i];
                EditorGUILayout.Space();
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(!row.Selectable || !string.IsNullOrWhiteSpace(row.DisabledReason)))
                    {
                        bool selected = selectedIntentContractId == row.ContractId;
                        bool nextSelected = EditorGUILayout.Toggle(selected, GUILayout.Width(18));
                        if (nextSelected && !selected)
                            SelectIntent(row.ContractId);
                    }

                    EditorGUILayout.LabelField(row.DisplayName, EditorStyles.boldLabel);
                }
                EditorGUILayout.LabelField("Contract", row.ContractId);
                if (!string.IsNullOrWhiteSpace(row.StableId))
                    EditorGUILayout.LabelField("StableId", row.StableId);
                if (!string.IsNullOrWhiteSpace(row.SourceType))
                    EditorGUILayout.LabelField("Source Type", row.SourceType);
                if (!string.IsNullOrWhiteSpace(row.SourcePath))
                    EditorGUILayout.LabelField("Source File", row.SourcePath);
                EditorGUILayout.LabelField("Category", row.Category);
                EditorGUILayout.LabelField("Capability", row.CapabilityPath);
                EditorGUILayout.LabelField("Surface", row.Surface);
                EditorGUILayout.LabelField("Selectable", row.Selectable.ToString());
                if (!string.IsNullOrWhiteSpace(row.DisabledReason))
                    EditorGUILayout.LabelField("Disabled", row.DisabledReason);
                if (!string.IsNullOrWhiteSpace(row.Summary))
                    EditorGUILayout.LabelField(row.Summary, EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Export Intent JSON"))
                ProjectionJsonExporter.ExportIntent(lastIntent, scriptsRoot);
        }

        private void DrawGuide()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Guide", EditorStyles.boldLabel);
            if (lastGuide == null)
            {
                EditorGUILayout.HelpBox("Open Settings and scan a scripts folder.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Selected Intent", lastGuide.SelectedDisplayName);
            EditorGUILayout.LabelField("Proof Target", lastGuide.ProofTarget);
            EditorGUILayout.LabelField("Proof Ready", lastGuide.ProofReady.ToString());
            guideScroll = EditorGUILayout.BeginScrollView(guideScroll);
            for (int i = 0; i < lastGuide.Rows.Count; i++)
            {
                GuideRow row = lastGuide.Rows[i];
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(row.Order + ". " + row.Title, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Role", row.Role);
                EditorGUILayout.LabelField("Owner", row.OwnerId);
                EditorGUILayout.LabelField("Detail", row.Detail);
                EditorGUILayout.LabelField("Action Kind", row.ActionKind);
                EditorGUILayout.LabelField("Action", row.ActionLabel);
                EditorGUILayout.LabelField("Native Action", row.NativeAction);
                EditorGUILayout.LabelField("Success", row.SuccessCheck);
                EditorGUILayout.LabelField("Blocks Proof", row.BlocksProof.ToString());
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Export Guide JSON"))
                ProjectionJsonExporter.ExportGuide(lastGuide, scriptsRoot);
        }

        private void DrawMap()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Map", EditorStyles.boldLabel);
            if (lastMap == null)
            {
                EditorGUILayout.HelpBox("Open Settings and scan a scripts folder.", MessageType.Info);
                return;
            }

            mapScroll = EditorGUILayout.BeginScrollView(mapScroll);
            for (int i = 0; i < lastMap.Rows.Count; i++)
            {
                MapRow row = lastMap.Rows[i];
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(row.Label, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Kind", row.Kind);
                EditorGUILayout.LabelField("Source", row.SourcePath);
                EditorGUILayout.LabelField("Components", row.ComponentCount.ToString());
                EditorGUILayout.LabelField("Issues", row.IssueCount.ToString());
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Export Map JSON"))
                ProjectionJsonExporter.ExportMap(lastMap, scriptsRoot);
        }

        private void DrawFacts()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Facts", EditorStyles.boldLabel);
            if (lastFacts == null)
            {
                EditorGUILayout.HelpBox("Open Settings and scan a scripts folder.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Assemblies", lastFacts.AssemblyCount.ToString());
            EditorGUILayout.LabelField("Namespaces", lastFacts.NamespaceCount.ToString());
            EditorGUILayout.LabelField("Types", lastFacts.TypeCount.ToString());
            EditorGUILayout.LabelField("Scripts", lastFacts.ScriptCount.ToString());
            EditorGUILayout.LabelField("Fields", lastFacts.FieldCount.ToString());
            EditorGUILayout.LabelField("Contracts", lastFacts.ContractCount.ToString());
            EditorGUILayout.LabelField("Validators", lastFacts.ValidatorCount.ToString());
            EditorGUILayout.LabelField("Scene Objects", lastFacts.SceneObjectCount.ToString());
            EditorGUILayout.LabelField("Prefabs", lastFacts.PrefabCount.ToString());
            EditorGUILayout.LabelField("Assets", lastFacts.AssetCount.ToString());
            EditorGUILayout.LabelField("Issues", lastFacts.IssueCount.ToString());

            factsScroll = EditorGUILayout.BeginScrollView(factsScroll);
            for (int i = 0; i < lastFacts.Rows.Count; i++)
            {
                FactRow row = lastFacts.Rows[i];
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(row.Label, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Kind", row.Kind);
                EditorGUILayout.LabelField("Detail", row.Detail);
                EditorGUILayout.LabelField("Source", row.SourcePath);
                EditorGUILayout.LabelField("Source Count", row.SourceCount.ToString());
                EditorGUILayout.LabelField("Confidence", row.Confidence);
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Export Facts JSON"))
                ProjectionJsonExporter.ExportFacts(lastFacts, scriptsRoot);
        }

        private void ChooseScriptsRoot()
        {
            string projectRoot = System.IO.Path.GetFullPath(".");
            string start = System.IO.Path.GetFullPath(string.IsNullOrWhiteSpace(scriptsRoot) ? DefaultScriptsRoot : scriptsRoot);
            string selected = EditorUtility.OpenFolderPanel("Choose Scripts Folder", start, string.Empty);
            if (string.IsNullOrWhiteSpace(selected))
                return;

            string normalizedProjectRoot = projectRoot.Replace('\\', '/').TrimEnd('/');
            string normalizedSelected = selected.Replace('\\', '/').TrimEnd('/');
            if (normalizedSelected.StartsWith(normalizedProjectRoot, System.StringComparison.OrdinalIgnoreCase))
            {
                scriptsRoot = normalizedSelected.Substring(normalizedProjectRoot.Length).TrimStart('/');
                if (string.IsNullOrWhiteSpace(scriptsRoot))
                    scriptsRoot = DefaultScriptsRoot;
                return;
            }

            scriptsRoot = normalizedSelected;
        }

        private void Scan()
        {
            UnityCodebaseScanResult result = UnityCodebaseScanner.Scan(new UnityCodebaseScanRequest(scriptsRoot));
            lastGraph = DependencyGraphProjection.Build(result);
            lastHygiene = HygieneProjectionBuilder.Build(lastGraph);
            EnsureSelectedIntentStillExists();
            lastIntent = AuthoringProjectionBuilder.BuildIntent(lastGraph, selectedIntentContractId);
            lastFacts = AuthoringProjectionBuilder.BuildFacts(lastGraph);
            lastMap = AuthoringProjectionBuilder.BuildMap(lastGraph);
            lastGuide = AuthoringProjectionBuilder.BuildGuide(lastGraph, selectedIntentContractId);
            lastOverview = AuthoringProjectionBuilder.BuildOverview(lastGraph, lastGuide);
            observedTypeCount = result.Types.Count;
            observedAssemblyCount = result.AssemblyDefinitions.Count;
            observedSourceFileCount = result.SourceDependencies.Count;
            observedNodeCount = lastGraph.Nodes.Count;
            observedEdgeCount = lastGraph.Edges.Count;
            hygieneReviewCount = lastHygiene.ReviewCount;
            hygieneWarningCount = lastHygiene.WarningCount;
            hygieneErrorCount = lastHygiene.ErrorCount;
        }

        private void SelectIntent(string contractId)
        {
            selectedIntentContractId = contractId ?? string.Empty;
            EditorPrefs.SetString(SelectedIntentPrefsKey, selectedIntentContractId);
            lastIntent = AuthoringProjectionBuilder.BuildIntent(lastGraph, selectedIntentContractId);
            lastGuide = AuthoringProjectionBuilder.BuildGuide(lastGraph, selectedIntentContractId);
            lastOverview = AuthoringProjectionBuilder.BuildOverview(lastGraph, lastGuide);
        }

        private void EnsureSelectedIntentStillExists()
        {
            if (lastGraph == null || string.IsNullOrWhiteSpace(selectedIntentContractId))
                return;

            for (int i = 0; i < lastGraph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = lastGraph.Nodes[i];
                if (node.Kind == AuthoringGraphNodeKind.Contract && node.Id == selectedIntentContractId)
                    return;
            }

            selectedIntentContractId = string.Empty;
            EditorPrefs.DeleteKey(SelectedIntentPrefsKey);
        }
    }
}
