using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Hygiene;
using Pys.Authoring.Editor.Projections;
using Pys.Authoring.Editor.Scanning;
using UnityEditor;
using UnityEngine;

namespace Pys.Authoring.Editor.Window
{
    public sealed partial class PysAuthoringWindow : EditorWindow
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

        private enum AuthoringMode
        {
            WaitingForScan,
            Observer,
            UnitySetupAvailable,
            TargetIntentReady,
            IntentSelected
        }

        private const string DefaultScriptsRoot = "Assets";
        private const string SelectedIntentPrefsKey = "Pys.Authoring.SelectedIntentContractId";
        private const string ScriptsRootPrefsKey = "Pys.Authoring.ScriptsRoot";
        private const string ActiveTabPrefsKey = "Pys.Authoring.ActiveTab";
        private const string IntentShowDisabledPrefsKey = "Pys.Authoring.Intent.ShowDisabledCandidates";
        private const string IntentShowUnitySetupGuidesPrefsKey = "Pys.Authoring.Intent.ShowUnitySetupGuides";
        private const string IntentSelectedFeaturesPrefsKey = "Pys.Authoring.Intent.SelectedFeatureToggles";
        private const string IntentSelectedLanePrefsKey = "Pys.Authoring.Intent.SelectedLane";
        private const string GuideShowBlockingOnlyPrefsKey = "Pys.Authoring.Guide.ShowBlockingRowsOnly";
        private const string MapShowSceneObjectsPrefsKey = "Pys.Authoring.Map.ShowSceneObjects";
        private const string MapShowPrefabsPrefsKey = "Pys.Authoring.Map.ShowPrefabs";
        private const string MapShowAssetsPrefsKey = "Pys.Authoring.Map.ShowAssets";
        private const string MapShowIssuesOnlyPrefsKey = "Pys.Authoring.Map.ShowIssuesOnly";
        private const string FactsKindFilterPrefsKey = "Pys.Authoring.Facts.KindFilterIndex";
        private const string FactsSearchPrefsKey = "Pys.Authoring.Facts.SearchText";
        private const string HygieneSeverityFilterPrefsKey = "Pys.Authoring.Hygiene.SeverityFilterIndex";
        private static readonly string[] TabLabels = { "Settings", "Intent", "Overview", "Guide", "Map", "Hygiene", "Facts" };

        private string scriptsRoot = DefaultScriptsRoot;
        private Vector2 settingsScroll;
        private Vector2 intentScroll;
        private Vector2 overviewScroll;
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
        private bool intentShowUnitySetupGuides;
        private string selectedIntentFeatureToggles = string.Empty;
        private string selectedIntentLane = string.Empty;
        private bool guideShowBlockingOnly;
        private bool mapShowSceneObjects = true;
        private bool mapShowPrefabs = true;
        private bool mapShowAssets = true;
        private bool mapShowIssuesOnly;
        private int factsKindFilterIndex;
        private string factsSearchText = string.Empty;
        private int hygieneSeverityFilterIndex;
        private bool scanStale = true;
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
            scriptsRoot = EditorPrefs.GetString(ScriptsRootPrefsKey, DefaultScriptsRoot);
            selectedIntentContractId = EditorPrefs.GetString(SelectedIntentPrefsKey, string.Empty);
            activeTab = (AuthoringTab)EditorPrefs.GetInt(ActiveTabPrefsKey, (int)AuthoringTab.Settings);
            intentShowDisabledCandidates = EditorPrefs.GetBool(IntentShowDisabledPrefsKey, true);
            intentShowUnitySetupGuides = EditorPrefs.GetBool(IntentShowUnitySetupGuidesPrefsKey, false);
            selectedIntentFeatureToggles = EditorPrefs.GetString(IntentSelectedFeaturesPrefsKey, string.Empty);
            selectedIntentLane = EditorPrefs.GetString(IntentSelectedLanePrefsKey, string.Empty);
            guideShowBlockingOnly = EditorPrefs.GetBool(GuideShowBlockingOnlyPrefsKey, false);
            mapShowSceneObjects = EditorPrefs.GetBool(MapShowSceneObjectsPrefsKey, true);
            mapShowPrefabs = EditorPrefs.GetBool(MapShowPrefabsPrefsKey, true);
            mapShowAssets = EditorPrefs.GetBool(MapShowAssetsPrefsKey, true);
            mapShowIssuesOnly = EditorPrefs.GetBool(MapShowIssuesOnlyPrefsKey, false);
            factsKindFilterIndex = EditorPrefs.GetInt(FactsKindFilterPrefsKey, 0);
            factsSearchText = EditorPrefs.GetString(FactsSearchPrefsKey, string.Empty);
            hygieneSeverityFilterIndex = EditorPrefs.GetInt(HygieneSeverityFilterPrefsKey, 0);
            scanStale = true;
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(ScriptsRootPrefsKey, scriptsRoot ?? DefaultScriptsRoot);
            EditorPrefs.SetString(SelectedIntentPrefsKey, selectedIntentContractId ?? string.Empty);
            EditorPrefs.SetInt(ActiveTabPrefsKey, (int)activeTab);
            EditorPrefs.SetBool(IntentShowDisabledPrefsKey, intentShowDisabledCandidates);
            EditorPrefs.SetBool(IntentShowUnitySetupGuidesPrefsKey, intentShowUnitySetupGuides);
            EditorPrefs.SetString(IntentSelectedFeaturesPrefsKey, selectedIntentFeatureToggles ?? string.Empty);
            EditorPrefs.SetString(IntentSelectedLanePrefsKey, selectedIntentLane ?? string.Empty);
            EditorPrefs.SetBool(GuideShowBlockingOnlyPrefsKey, guideShowBlockingOnly);
            EditorPrefs.SetBool(MapShowSceneObjectsPrefsKey, mapShowSceneObjects);
            EditorPrefs.SetBool(MapShowPrefabsPrefsKey, mapShowPrefabs);
            EditorPrefs.SetBool(MapShowAssetsPrefsKey, mapShowAssets);
            EditorPrefs.SetBool(MapShowIssuesOnlyPrefsKey, mapShowIssuesOnly);
            EditorPrefs.SetInt(FactsKindFilterPrefsKey, factsKindFilterIndex);
            EditorPrefs.SetString(FactsSearchPrefsKey, factsSearchText ?? string.Empty);
            EditorPrefs.SetInt(HygieneSeverityFilterPrefsKey, hygieneSeverityFilterIndex);
        }

        private void OnGUI()
        {
            DrawWindowHeader();
            DrawTabNavigation();
            BeginActiveTabScroll();

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

            EditorGUILayout.EndScrollView();
        }

        private void BeginActiveTabScroll()
        {
            switch (activeTab)
            {
                case AuthoringTab.Settings:
                    settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll);
                    break;
                case AuthoringTab.Intent:
                    intentScroll = EditorGUILayout.BeginScrollView(intentScroll);
                    break;
                case AuthoringTab.Overview:
                    overviewScroll = EditorGUILayout.BeginScrollView(overviewScroll);
                    break;
                case AuthoringTab.Guide:
                    guideScroll = EditorGUILayout.BeginScrollView(guideScroll);
                    break;
                case AuthoringTab.Map:
                    mapScroll = EditorGUILayout.BeginScrollView(mapScroll);
                    break;
                case AuthoringTab.Hygiene:
                    hygieneScroll = EditorGUILayout.BeginScrollView(hygieneScroll);
                    break;
                case AuthoringTab.Facts:
                    factsScroll = EditorGUILayout.BeginScrollView(factsScroll);
                    break;
            }
        }

        private void DrawWindowHeader()
        {
            EditorGUILayout.LabelField("PYS Authoring", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Root", ShortPath(scriptsRoot), GUILayout.MinWidth(120));
                EditorGUILayout.LabelField("Mode", ModeLabel(CurrentMode()), GUILayout.MinWidth(120));
                EditorGUILayout.LabelField("Scan", lastGraph == null ? "Not Run" : scanStale ? "Stale" : "Current", GUILayout.MinWidth(90));
            }
        }

        private void DrawTabNavigation()
        {
            int nextTab = position.width < 560f
                ? GUILayout.SelectionGrid((int)activeTab, TabLabels, 4)
                : GUILayout.Toolbar((int)activeTab, TabLabels);

            if (nextTab != (int)activeTab)
            {
                activeTab = (AuthoringTab)nextTab;
                EditorPrefs.SetInt(ActiveTabPrefsKey, (int)activeTab);
            }
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
                MarkScanStale();
                return;
            }

            scriptsRoot = normalizedSelected;
            MarkScanStale();
        }

        private void MarkScanStale()
        {
            scanStale = true;
            EditorPrefs.SetString(ScriptsRootPrefsKey, scriptsRoot ?? DefaultScriptsRoot);
        }

        private void Scan()
        {
            UnityCodebaseScanResult result = UnityCodebaseScanner.Scan(new UnityCodebaseScanRequest(scriptsRoot));
            lastGraph = DependencyGraphProjection.Build(result);
            lastHygiene = HygieneProjectionBuilder.Build(lastGraph);
            lastIntent = AuthoringProjectionBuilder.BuildIntent(lastGraph, selectedIntentContractId, intentShowUnitySetupGuides);
            EnsureSelectedIntentStillExists();
            lastIntent = AuthoringProjectionBuilder.BuildIntent(lastGraph, selectedIntentContractId, intentShowUnitySetupGuides);
            lastFacts = AuthoringProjectionBuilder.BuildFacts(lastGraph);
            lastMap = AuthoringProjectionBuilder.BuildMap(lastGraph);
            lastGuide = AuthoringProjectionBuilder.BuildGuide(lastGraph, selectedIntentContractId, intentShowUnitySetupGuides);
            lastOverview = AuthoringProjectionBuilder.BuildOverview(lastGraph, lastGuide);
            observedTypeCount = result.Types.Count;
            observedAssemblyCount = result.AssemblyDefinitions.Count;
            observedSourceFileCount = result.SourceDependencies.Count;
            observedNodeCount = lastGraph.Nodes.Count;
            observedEdgeCount = lastGraph.Edges.Count;
            hygieneReviewCount = lastHygiene.ReviewCount;
            hygieneWarningCount = lastHygiene.WarningCount;
            hygieneErrorCount = lastHygiene.ErrorCount;
            scanStale = false;
        }

        private void SelectIntent(string contractId)
        {
            if (selectedIntentContractId != (contractId ?? string.Empty))
                ResetIntentComposition();

            selectedIntentContractId = contractId ?? string.Empty;
            EditorPrefs.SetString(SelectedIntentPrefsKey, selectedIntentContractId);
            RebuildSelectedPathProjections();
        }

        private void ResetIntentComposition()
        {
            selectedIntentFeatureToggles = string.Empty;
            selectedIntentLane = string.Empty;
            EditorPrefs.DeleteKey(IntentSelectedFeaturesPrefsKey);
            EditorPrefs.DeleteKey(IntentSelectedLanePrefsKey);
        }

        private void RebuildSelectedPathProjections()
        {
            lastIntent = AuthoringProjectionBuilder.BuildIntent(lastGraph, selectedIntentContractId, intentShowUnitySetupGuides);
            EnsureSelectedIntentStillExists();
            lastIntent = AuthoringProjectionBuilder.BuildIntent(lastGraph, selectedIntentContractId, intentShowUnitySetupGuides);
            lastGuide = AuthoringProjectionBuilder.BuildGuide(lastGraph, selectedIntentContractId, intentShowUnitySetupGuides);
            lastOverview = AuthoringProjectionBuilder.BuildOverview(lastGraph, lastGuide);
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
            ResetIntentComposition();
        }

        private AuthoringMode CurrentMode()
        {
            if (lastGraph == null)
                return AuthoringMode.WaitingForScan;

            if (!string.IsNullOrWhiteSpace(selectedIntentContractId) && lastGuide != null && !string.IsNullOrWhiteSpace(lastGuide.SelectedContractId))
                return AuthoringMode.IntentSelected;

            if (CountTargetGoalContracts() > 0)
                return AuthoringMode.TargetIntentReady;

            if (CountBuiltInUnitySetupContracts() > 0)
                return AuthoringMode.UnitySetupAvailable;

            return AuthoringMode.Observer;
        }
    }
}
