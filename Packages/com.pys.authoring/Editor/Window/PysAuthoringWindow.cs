using System.Collections.Generic;
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
        private bool intentShowDisabledCandidates = true;
        private bool guideShowBlockingRowsOnly;
        private bool mapShowSceneObjects = true;
        private bool mapShowPrefabs = true;
        private bool mapShowAssets = true;
        private bool mapShowIssuesOnly;
        private int factsKindFilterIndex;
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

            DrawObservedEvidenceReadiness();

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

                HygieneProjection renderedHygiene = RenderedHygieneProjection();
                for (int i = 0; i < renderedHygiene.Rows.Count; i++)
                {
                    HygieneRow row = renderedHygiene.Rows[i];
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
                    HygieneJsonExporter.Export(RenderedHygieneProjection(), scriptsRoot);
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
            intentShowDisabledCandidates = EditorGUILayout.Toggle("Show Disabled Candidates", intentShowDisabledCandidates);
            if (!string.IsNullOrWhiteSpace(lastIntent.SelectedDisplayName))
                EditorGUILayout.LabelField("Selected", lastIntent.SelectedDisplayName);
            if (!string.IsNullOrWhiteSpace(lastIntent.SelectedDisabledReason))
                EditorGUILayout.HelpBox(lastIntent.SelectedDisabledReason, MessageType.Warning);

            string[] optionLabels = BuildIntentOptionLabels(lastIntent);
            int currentIndex = IntentPopupIndex(lastIntent);
            int nextIndex = EditorGUILayout.Popup("Intent", currentIndex, optionLabels);
            if (nextIndex != currentIndex)
                SelectIntentFromPopup(lastIntent, nextIndex);

            IntentRow selectedRow = FindIntentRow(lastIntent, selectedIntentContractId);
            if (selectedRow != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(selectedRow.DisplayName, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Capability", selectedRow.CapabilityPath);
                EditorGUILayout.LabelField("Category", selectedRow.Category);
                EditorGUILayout.LabelField("Surface", selectedRow.Surface);
                EditorGUILayout.LabelField("Pattern", selectedRow.OrganizationPattern);
                EditorGUILayout.LabelField("Dependencies", selectedRow.DependencyCount.ToString());
                if (!string.IsNullOrWhiteSpace(selectedRow.DisabledReason))
                    EditorGUILayout.HelpBox(selectedRow.DisabledReason, MessageType.Warning);
                if (!string.IsNullOrWhiteSpace(selectedRow.Summary))
                    EditorGUILayout.LabelField(selectedRow.Summary, EditorStyles.wordWrappedLabel);
            }
            else
            {
                EditorGUILayout.HelpBox("Select one authoring goal to build the Guide path.", MessageType.Info);
            }

            if (GUILayout.Button("Export Intent JSON"))
                ProjectionJsonExporter.ExportIntent(RenderedIntentProjection(), scriptsRoot);
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
            guideShowBlockingRowsOnly = EditorGUILayout.Toggle("Show Blocking Rows Only", guideShowBlockingRowsOnly);
            GuideProjection renderedGuide = RenderedGuideProjection();
            guideScroll = EditorGUILayout.BeginScrollView(guideScroll);
            for (int i = 0; i < renderedGuide.Rows.Count; i++)
            {
                GuideRow row = renderedGuide.Rows[i];
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(row.Order + ". " + row.Title, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Role", row.Role);
                EditorGUILayout.LabelField("Owner", row.OwnerId);
                EditorGUILayout.LabelField("StableId", row.StableId);
                EditorGUILayout.LabelField("Route Stage", row.RouteStage);
                EditorGUILayout.LabelField("Route Order", row.RouteOrder.ToString());
                EditorGUILayout.LabelField("Setup Domain", row.SetupDomain);
                EditorGUILayout.LabelField("Detail", row.Detail);
                EditorGUILayout.LabelField("Action Kind", row.ActionKind);
                EditorGUILayout.LabelField("Action", row.ActionLabel);
                EditorGUILayout.LabelField("Native Action", row.NativeAction);
                EditorGUILayout.LabelField("Success", row.SuccessCheck);
                EditorGUILayout.LabelField("Blocks Proof", row.BlocksProof.ToString());
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Export Guide JSON"))
                ProjectionJsonExporter.ExportGuide(renderedGuide, scriptsRoot);
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

            mapShowSceneObjects = EditorGUILayout.Toggle("Scene Objects", mapShowSceneObjects);
            mapShowPrefabs = EditorGUILayout.Toggle("Prefabs", mapShowPrefabs);
            mapShowAssets = EditorGUILayout.Toggle("Assets", mapShowAssets);
            mapShowIssuesOnly = EditorGUILayout.Toggle("Rows With Issues Only", mapShowIssuesOnly);
            MapProjection renderedMap = RenderedMapProjection();

            mapScroll = EditorGUILayout.BeginScrollView(mapScroll);
            for (int i = 0; i < renderedMap.Rows.Count; i++)
            {
                MapRow row = renderedMap.Rows[i];
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(row.Label, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Kind", row.Kind);
                EditorGUILayout.LabelField("Source", row.SourcePath);
                EditorGUILayout.LabelField("Components", row.ComponentCount.ToString());
                EditorGUILayout.LabelField("Issues", row.IssueCount.ToString());
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Export Map JSON"))
                ProjectionJsonExporter.ExportMap(renderedMap, scriptsRoot);
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

            string[] factKinds = BuildFactKindFilterOptions(lastFacts);
            if (factsKindFilterIndex < 0 || factsKindFilterIndex >= factKinds.Length)
                factsKindFilterIndex = 0;
            factsKindFilterIndex = EditorGUILayout.Popup("Kind Filter", factsKindFilterIndex, factKinds);
            FactsProjection renderedFacts = RenderedFactsProjection();

            factsScroll = EditorGUILayout.BeginScrollView(factsScroll);
            for (int i = 0; i < renderedFacts.Rows.Count; i++)
            {
                FactRow row = renderedFacts.Rows[i];
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
                ProjectionJsonExporter.ExportFacts(renderedFacts, scriptsRoot);
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
            lastIntent = AuthoringProjectionBuilder.BuildIntent(lastGraph, selectedIntentContractId);
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

        private static string[] BuildIntentOptionLabels(IntentProjection intent)
        {
            int selectableCount = intent != null ? intent.SelectableCount : 0;
            string[] labels = new string[selectableCount + 1];
            labels[0] = "No intent selected";
            if (intent == null)
                return labels;

            int labelIndex = 1;
            for (int i = 0; i < intent.Rows.Count; i++)
            {
                IntentRow row = intent.Rows[i];
                if (!IsSelectableIntent(row))
                    continue;

                labels[labelIndex] = IntentOptionLabel(row);
                labelIndex++;
            }

            return labels;
        }

        private int IntentPopupIndex(IntentProjection intent)
        {
            if (intent == null || string.IsNullOrWhiteSpace(selectedIntentContractId))
                return 0;

            int optionIndex = 1;
            for (int i = 0; i < intent.Rows.Count; i++)
            {
                IntentRow row = intent.Rows[i];
                if (!IsSelectableIntent(row))
                    continue;

                if (row.ContractId == selectedIntentContractId)
                    return optionIndex;

                optionIndex++;
            }

            return 0;
        }

        private void SelectIntentFromPopup(IntentProjection intent, int popupIndex)
        {
            if (popupIndex <= 0 || intent == null)
            {
                SelectIntent(string.Empty);
                return;
            }

            int optionIndex = 1;
            for (int i = 0; i < intent.Rows.Count; i++)
            {
                IntentRow row = intent.Rows[i];
                if (!IsSelectableIntent(row))
                    continue;

                if (optionIndex == popupIndex)
                {
                    SelectIntent(row.ContractId);
                    return;
                }

                optionIndex++;
            }
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

            return string.IsNullOrWhiteSpace(row.CapabilityPath)
                ? row.DisplayName
                : row.DisplayName + " - " + row.CapabilityPath;
        }

        private void EnsureSelectedIntentStillExists()
        {
            if (string.IsNullOrWhiteSpace(selectedIntentContractId))
                return;

            if (lastIntent != null)
            {
                for (int i = 0; i < lastIntent.Rows.Count; i++)
                {
                    IntentRow row = lastIntent.Rows[i];
                    if (row.ContractId == selectedIntentContractId && IsSelectableIntent(row))
                        return;
                }
            }

            selectedIntentContractId = string.Empty;
            EditorPrefs.DeleteKey(SelectedIntentPrefsKey);
        }

        private void DrawObservedEvidenceReadiness()
        {
            if (lastGraph == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Observer mode is waiting for a scan. PYS can inspect Facts, Hygiene, and Map before a project adds authoring contracts.", MessageType.Info);
                return;
            }

            int contractCount = CountGraphNodes(AuthoringGraphNodeKind.Contract);
            int goalCount = CountGoalContracts();
            int validatorCount = CountGraphNodes(AuthoringGraphNodeKind.Validator);
            int sceneRealityCount = CountGraphNodes(AuthoringGraphNodeKind.SceneObject) + CountGraphNodes(AuthoringGraphNodeKind.Prefab) + CountGraphNodes(AuthoringGraphNodeKind.Asset);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Observed Evidence", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Contracts", contractCount.ToString());
            EditorGUILayout.LabelField("Goal Contracts", goalCount.ToString());
            EditorGUILayout.LabelField("Runtime Validation Methods", validatorCount.ToString());
            EditorGUILayout.LabelField("Scene/Prefab/Asset Rows", sceneRealityCount.ToString());
            EditorGUILayout.LabelField("Issues", CountGraphNodes(AuthoringGraphNodeKind.Issue).ToString());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Authoring Guide Readiness", EditorStyles.boldLabel);
            if (goalCount == 0)
                EditorGUILayout.HelpBox("Observer mode active. Add goal contracts to unlock Intent as selected-goal steering.", MessageType.Info);
            else if (string.IsNullOrWhiteSpace(selectedIntentContractId))
                EditorGUILayout.HelpBox("Goal contracts were observed. Select an Intent to build the Guide proof path.", MessageType.Info);
            else
                EditorGUILayout.HelpBox("Authoring guide mode active for the selected Intent. Guide owns the proof path; Map remains current scene/setup reality.", MessageType.Info);

            if (validatorCount == 0)
                EditorGUILayout.HelpBox("Add target-owned runtime validation methods to surface current setup readiness.", MessageType.Info);
            if (!GraphHasRouteMetadata())
                EditorGUILayout.HelpBox("Add prerequisite stable IDs and route metadata to order the Guide path.", MessageType.Info);
        }

        private IntentProjection RenderedIntentProjection()
        {
            if (lastIntent == null || intentShowDisabledCandidates)
                return lastIntent;

            IntentProjection projection = new IntentProjection
            {
                SelectedContractId = lastIntent.SelectedContractId,
                SelectedDisplayName = lastIntent.SelectedDisplayName,
                SelectedDisabledReason = lastIntent.SelectedDisabledReason
            };

            for (int i = 0; i < lastIntent.Rows.Count; i++)
            {
                IntentRow row = lastIntent.Rows[i];
                if (!IsSelectableIntent(row))
                    continue;

                projection.Rows.Add(row);
                projection.SelectableCount++;
            }

            return projection;
        }

        private GuideProjection RenderedGuideProjection()
        {
            if (lastGuide == null || !guideShowBlockingRowsOnly)
                return lastGuide;

            GuideProjection projection = new GuideProjection
            {
                SelectedContractId = lastGuide.SelectedContractId,
                SelectedDisplayName = lastGuide.SelectedDisplayName,
                ProofTarget = lastGuide.ProofTarget,
                ProofReady = lastGuide.ProofReady
            };

            for (int i = 0; i < lastGuide.Rows.Count; i++)
            {
                if (lastGuide.Rows[i].BlocksProof)
                    projection.Rows.Add(lastGuide.Rows[i]);
            }

            return projection;
        }

        private MapProjection RenderedMapProjection()
        {
            if (lastMap == null)
                return null;

            MapProjection projection = new MapProjection();
            for (int i = 0; i < lastMap.Rows.Count; i++)
            {
                MapRow row = lastMap.Rows[i];
                if (mapShowIssuesOnly && row.IssueCount == 0)
                    continue;

                if (row.Kind == AuthoringGraphNodeKind.SceneObject.ToString() && !mapShowSceneObjects)
                    continue;
                if (row.Kind == AuthoringGraphNodeKind.Prefab.ToString() && !mapShowPrefabs)
                    continue;
                if (row.Kind == AuthoringGraphNodeKind.Asset.ToString() && !mapShowAssets)
                    continue;

                projection.Rows.Add(row);
            }

            return projection;
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

                projection.Rows.Add(row);
            }

            return projection;
        }

        private HygieneProjection RenderedHygieneProjection()
        {
            if (lastHygiene == null)
                return null;

            HygieneProjection projection = new HygieneProjection
            {
                ReviewCount = 0,
                WarningCount = 0,
                ErrorCount = 0
            };

            if (lastHygiene.Lenses.Count == 0)
                return projection;

            if (activeHygieneLensIndex < 0 || activeHygieneLensIndex >= lastHygiene.Lenses.Count)
                activeHygieneLensIndex = 0;

            HygieneLensProjection sourceLens = lastHygiene.Lenses[activeHygieneLensIndex];
            HygieneLensProjection lens = new HygieneLensProjection(sourceLens.Kind, sourceLens.Title, sourceLens.Question)
            {
                ReviewCount = sourceLens.ReviewCount,
                WarningCount = sourceLens.WarningCount,
                ErrorCount = sourceLens.ErrorCount
            };
            projection.Lenses.Add(lens);
            projection.ReviewCount = sourceLens.ReviewCount;
            projection.WarningCount = sourceLens.WarningCount;
            projection.ErrorCount = sourceLens.ErrorCount;

            for (int i = 0; i < sourceLens.Rows.Count; i++)
            {
                projection.Rows.Add(sourceLens.Rows[i]);
                lens.Rows.Add(sourceLens.Rows[i]);
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

        private int CountGoalContracts()
        {
            if (lastGraph == null)
                return 0;

            int count = 0;
            for (int i = 0; i < lastGraph.Nodes.Count; i++)
            {
                AuthoringGraphNode node = lastGraph.Nodes[i];
                if (node.Kind != AuthoringGraphNodeKind.Contract)
                    continue;

                string surface = Metadata(node, "surface");
                if (surface == AuthoringSurface.Goal.ToString()
                    || !string.IsNullOrWhiteSpace(Metadata(node, "proofTarget"))
                    || !string.IsNullOrWhiteSpace(Metadata(node, "successChecks")))
                {
                    count++;
                }
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

        private static string Metadata(AuthoringGraphNode node, string key)
        {
            if (node == null || string.IsNullOrWhiteSpace(key))
                return string.Empty;

            return node.Metadata.TryGetValue(key, out string value) ? value ?? string.Empty : string.Empty;
        }
    }
}
