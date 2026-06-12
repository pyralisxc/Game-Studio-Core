using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    public partial class PyralisAuthoringWindow : EditorWindow
    {
        private enum AuthoringWindowMode
        {
            Overview,
            Intent,
            Guide,
            Map,
            Validate,
            Facts
        }

        private static readonly Dictionary<string, bool> ServiceStepFoldouts = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> IntentRowFoldouts = new Dictionary<string, bool>();
        private const double InspectorRepaintIntervalSeconds = 0.35d;

        private AuthoringWindowMode _mode = AuthoringWindowMode.Overview;
        [SerializeField] private Object _pinnedActiveSetup;
        [SerializeField] private Object _lastActiveSetup;
        [SerializeField] private bool _emptySceneIntentStartApplied;
        [SerializeField] private RuntimeCapabilityLaneTag _intentLane = RuntimeCapabilityLaneTag.Sprite2D;
        [SerializeField] private AuthoringWorldAxiom _intentAxioms = AuthoringWorldAxiom.None;
        [SerializeField] private long _intentCapabilitiesValue = 0;
        private AuthoringCapability _intentCapabilities 
        { 
            get => (AuthoringCapability)_intentCapabilitiesValue; 
            set => _intentCapabilitiesValue = (long)value; 
        }
        [SerializeField] private string _intentGoalFilter = "";
        [SerializeField] private Vector2 _overviewScroll;
        [SerializeField] private Vector2 _intentScroll;
        [SerializeField] private Vector2 _intentCapabilityScroll;
        [SerializeField] private Vector2 _mapScroll;
        [SerializeField] private Vector2 _validateScroll;
        [SerializeField] private Vector2 _guideScroll;
        [SerializeField] private Vector2 _factsScroll;
        [SerializeField] private bool _coreFoldout = true;
        [SerializeField] private bool _actorFoldout = true;
        [SerializeField] private bool _strategyFoldout = true;
        [SerializeField] private bool _worldFoldout = true;
        private double _lastInspectorRepaintTime;
        private int _authoringCacheVersion;
        private string _cachedIntentModelKey;
        private PyralisAuthoringIntentModel _cachedIntentModel;
        private int _cachedActiveReportVersion = -1;
        private int _cachedSelectionReportVersion = -1;
        private Object _cachedActiveReportTarget;
        private Object _cachedSelectionReportTarget;
        private PyralisAuthoringRouteReport _cachedActiveReport;
        private PyralisAuthoringRouteReport _cachedSelectionReport;

        private VisualElement _contentRoot;

        [MenuItem("NeonBlack/Gameplay/Pyralis Authoring Window")]
        public static void Open()
        {
            GetWindow<PyralisAuthoringWindow>("Pyralis Authoring");
        }

        public void CreateGUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Surfaces/AuthoringWindow/UI/PyralisAuthoringWindow.uxml");
            if (uxml == null)
            {
                rootVisualElement.Add(new Label("Failed to load UXML. Check path: Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Surfaces/AuthoringWindow/UI/PyralisAuthoringWindow.uxml"));
                return;
            }

            uxml.CloneTree(rootVisualElement);
            _contentRoot = rootVisualElement.Q<VisualElement>("content");

            SetupTabs();
            RefreshActiveTab();
        }

        private void SetupTabs()
        {
            var toolbar = rootVisualElement.Q<VisualElement>("toolbar");
            if (toolbar == null) return;

            foreach (var tab in toolbar.Children())
            {
                tab.RegisterCallback<PointerDownEvent>(evt =>
                {
                    UpdateTabSelection(tab);
                });
            }
        }

        private void UpdateTabSelection(VisualElement selectedTab)
        {
            var toolbar = rootVisualElement.Q<VisualElement>("toolbar");
            foreach (var tab in toolbar.Children())
                tab.RemoveFromClassList("mode-tab--active");

            selectedTab.AddToClassList("mode-tab--active");

            _mode = selectedTab.name switch
            {
                "tabOverview" => AuthoringWindowMode.Overview,
                "tabIntent" => AuthoringWindowMode.Intent,
                "tabMap" => AuthoringWindowMode.Map,
                "tabValidate" => AuthoringWindowMode.Validate,
                "tabGuide" => AuthoringWindowMode.Guide,
                "tabFacts" => AuthoringWindowMode.Facts,
                _ => _mode
            };

            RefreshActiveTab();
        }

        private void RefreshActiveTab()
        {
            if (_contentRoot == null) return;
            _contentRoot.Clear();

            Object selection = Selection.activeObject;
            Object selectionSetup = PyralisAuthoringSetupContextResolver.GetSetupContext(selection);
            Object sceneFallbackSetup = PyralisAuthoringSetupContextResolver.GetSceneFallbackSetup(selection, selectionSetup);
            Object activeSetup = PyralisAuthoringSetupContextResolver.ResolveActiveSetup(selection, selectionSetup, sceneFallbackSetup, _pinnedActiveSetup, _lastActiveSetup);
            if (ShouldStartInIntent(activeSetup, selectionSetup, sceneFallbackSetup, _mode)
                && !_emptySceneIntentStartApplied)
            {
                _emptySceneIntentStartApplied = true;
                _mode = AuthoringWindowMode.Intent;
                UpdateToolbarSelection();
            }
            else if (!HasNoSetupContext(activeSetup, selectionSetup, sceneFallbackSetup))
            {
                _emptySceneIntentStartApplied = false;
            }

            if (_mode == AuthoringWindowMode.Intent)
            {
                RefreshIntentTab();
            }
            else
            {
                _contentRoot.Add(new IMGUIContainer(() =>
                {
                    // We use the same logic as the old OnGUI but skip the layout headers
                    Object currentSelection = Selection.activeObject;
                    Object currentSelectionSetup = PyralisAuthoringSetupContextResolver.GetSetupContext(currentSelection);
                    Object currentSceneFallbackSetup = PyralisAuthoringSetupContextResolver.GetSceneFallbackSetup(currentSelection, currentSelectionSetup);
                    Object currentActiveSetup = PyralisAuthoringSetupContextResolver.ResolveActiveSetup(currentSelection, currentSelectionSetup, currentSceneFallbackSetup, _pinnedActiveSetup, _lastActiveSetup);
                    
                    ref Vector2 scroll = ref GetCurrentScroll();
                    scroll = EditorGUILayout.BeginScrollView(scroll);
                    DrawModeContent(currentActiveSetup, currentSelection);
                    EditorGUILayout.EndScrollView();
                }));
            }
        }

        private void SwitchMode(AuthoringWindowMode mode)
        {
            _mode = mode;
            UpdateToolbarSelection();
            EditorApplication.delayCall += () =>
            {
                if (this == null)
                    return;

                RefreshActiveTab();
                Repaint();
            };
        }

        private void UpdateToolbarSelection()
        {
            var toolbar = rootVisualElement.Q<VisualElement>("toolbar");
            if (toolbar == null)
                return;

            foreach (var tab in toolbar.Children())
                tab.RemoveFromClassList("mode-tab--active");

            string tabName = _mode switch
            {
                AuthoringWindowMode.Overview => "tabOverview",
                AuthoringWindowMode.Intent => "tabIntent",
                AuthoringWindowMode.Guide => "tabGuide",
                AuthoringWindowMode.Map => "tabMap",
                AuthoringWindowMode.Validate => "tabValidate",
                AuthoringWindowMode.Facts => "tabFacts",
                _ => "tabOverview"
            };

            rootVisualElement.Q<VisualElement>(tabName)?.AddToClassList("mode-tab--active");
        }

        private ref Vector2 GetCurrentScroll()
        {
            switch (_mode)
            {
                case AuthoringWindowMode.Overview: return ref _overviewScroll;
                case AuthoringWindowMode.Intent: return ref _intentScroll;
                case AuthoringWindowMode.Map: return ref _mapScroll;
                case AuthoringWindowMode.Validate: return ref _validateScroll;
                case AuthoringWindowMode.Guide: return ref _guideScroll;
                case AuthoringWindowMode.Facts: return ref _factsScroll;
                default: return ref _overviewScroll;
            }
        }

        private void DrawModeContent(Object activeSetup, Object selection)
        {
            switch (_mode)
            {
                case AuthoringWindowMode.Overview:
                    DrawOverviewMode(
                        activeSetup,
                        selection,
                        GetCachedRouteReport(activeSetup, true),
                        GetCachedRouteReport(selection, false));
                    break;
                case AuthoringWindowMode.Guide:
                    DrawGuideMode(
                        selection,
                        GetCachedRouteReport(selection, false),
                        activeSetup,
                        GetCachedRouteReport(activeSetup, true));
                    break;
                case AuthoringWindowMode.Map:
                    PyralisAuthoringMapRenderer.Draw(activeSetup, selection, GetCachedRouteReport(activeSetup, true));
                    break;
                case AuthoringWindowMode.Validate:
                    PyralisAuthoringValidateRenderer.Draw(activeSetup, GetCachedRouteReport(activeSetup, true), TryRunGuidanceAction);
                    break;
                case AuthoringWindowMode.Facts:
                    PyralisAuthoringFactExplorerRenderer.Draw(activeSetup);
                    break;
            }
        }

        private Object ResolveCurrentActiveSetup(Object selection)
        {
            Object selectionSetup = PyralisAuthoringSetupContextResolver.GetSetupContext(selection);
            Object sceneFallbackSetup = PyralisAuthoringSetupContextResolver.GetSceneFallbackSetup(selection, selectionSetup);
            return PyralisAuthoringSetupContextResolver.ResolveActiveSetup(
                selection,
                selectionSetup,
                sceneFallbackSetup,
                _pinnedActiveSetup,
                _lastActiveSetup);
        }

        private void OnSelectionChange()
        {
            InvalidateAuthoringCache();
            Object selection = Selection.activeObject;
            if (_mode == AuthoringWindowMode.Intent
                && selection is GameObject selectedGameObject
                && selectedGameObject.GetComponent<GameplaySessionBootstrap>() == null
                && PyralisAuthoringSetupContextResolver.GetSetupContext(selection) == null)
            {
                _mode = AuthoringWindowMode.Guide;
                _guideScroll = Vector2.zero;
            }

            Repaint();
            RefreshActiveTab();
        }

        private void OnHierarchyChange()
        {
            InvalidateAuthoringCache();
            Repaint();
            RefreshActiveTab();
        }

        private void OnProjectChange()
        {
            InvalidateAuthoringCache();
            Repaint();
            RefreshActiveTab();
        }

        private void OnInspectorUpdate()
        {
            double now = EditorApplication.timeSinceStartup;
            if (now - _lastInspectorRepaintTime < InspectorRepaintIntervalSeconds)
                return;

            _lastInspectorRepaintTime = now;
            Repaint();
        }


        private void InvalidateAuthoringCache()
        {
            _authoringCacheVersion++;
            _cachedIntentModelKey = null;
            _cachedIntentModel = null;
        }

        private PyralisAuthoringRouteReport GetCachedRouteReport(Object target, bool activeSetupReport)
        {
            if (activeSetupReport)
            {
                if (_cachedActiveReportVersion == _authoringCacheVersion && _cachedActiveReportTarget == target)
                    return _cachedActiveReport;

                _cachedActiveReportVersion = _authoringCacheVersion;
                _cachedActiveReportTarget = target;
                _cachedActiveReport = PyralisAuthoringRouteReport.Build(target);
                return _cachedActiveReport;
            }

            if (_cachedSelectionReportVersion == _authoringCacheVersion && _cachedSelectionReportTarget == target)
                return _cachedSelectionReport;

            _cachedSelectionReportVersion = _authoringCacheVersion;
            _cachedSelectionReportTarget = target;
            _cachedSelectionReport = PyralisAuthoringRouteReport.Build(target);
            return _cachedSelectionReport;
        }

        private static bool ShouldStartInIntent(Object activeSetup, Object selectionSetup, Object sceneFallbackSetup, AuthoringWindowMode mode)
        {
            return mode == AuthoringWindowMode.Overview
                && HasNoSetupContext(activeSetup, selectionSetup, sceneFallbackSetup);
        }

        private static bool HasNoSetupContext(Object activeSetup, Object selectionSetup, Object sceneFallbackSetup)
        {
            return activeSetup == null
                && selectionSetup == null
                && sceneFallbackSetup == null;
        }

        private void DrawOverviewMode(Object activeSetup, Object selection, PyralisAuthoringRouteReport report, PyralisAuthoringRouteReport selectionReport)
        {
            bool selectedSetupProfile = selection is GameSetupProfile;
            Object currentStepSelection = selectedSetupProfile ? selection : activeSetup != null ? activeSetup : selection;
            PyralisAuthoringRouteReport currentStepReport = selectedSetupProfile ? selectionReport : activeSetup != null ? report : selectionReport;
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(activeSetup);
            PyralisAuthoringCurrentStepGraphRow currentStep = PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(graph, currentStepReport);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(activeSetup, report, graph);

            EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PyralisAuthoringOverviewRenderer.DrawGuidanceCard(model, report, graph);
                PyralisAuthoringOverviewRenderer.DrawActionButtons(model, OpenIntentFromOverview, OpenMapFromOverview, OpenValidateFromOverview);
            }

            EditorGUILayout.Space(12f);
            PyralisAuthoringOverviewRenderer.DrawFirstProofCard(model, graph);
            PyralisAuthoringOverviewRenderer.DrawPlayModeChecklist(model);
            PyralisAuthoringOverviewRenderer.DrawLane("Do Now", "Only intent-required missing or blocked work appears here.", model.DoNow);
            PyralisAuthoringOverviewRenderer.DrawLane("Proof Enhancers", "Useful before Play Mode when they make the first proof clearer.", model.DoSoon);
            PyralisAuthoringOverviewRenderer.DrawContractProofGuidance(activeSetup, report);
            DrawCurrentStepPanel(currentStepSelection, currentStepReport, currentStep);
        }

        private void OpenIntentFromOverview()
        {
            _intentScroll = Vector2.zero;
            SwitchMode(AuthoringWindowMode.Intent);
        }

        private void OpenMapFromOverview()
        {
            _mapScroll = Vector2.zero;
            SwitchMode(AuthoringWindowMode.Map);
        }

        private void OpenValidateFromOverview()
        {
            _validateScroll = Vector2.zero;
            SwitchMode(AuthoringWindowMode.Validate);
        }

        private PyralisAuthoringIntentModel GetCachedIntentModel()
        {
            string key = $"{_intentLane}_{_intentAxioms}_{_intentCapabilities}_{_authoringCacheVersion}";
            if (_cachedIntentModelKey == key)
                return _cachedIntentModel;

            _cachedIntentModelKey = key;
            _cachedIntentModel = PyralisAuthoringIntentAdvisor.Build(
                new PyralisAuthoringIntentSelection(_intentLane, _intentCapabilities, _intentAxioms));
            return _cachedIntentModel;
        }

        private static void DrawIntentRows(string title, string description, IReadOnlyList<PyralisAuthoringIntentRow> rows, string tooltip, int collapsedLimit = 0)
{
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(new GUIContent(title, tooltip), new GUIContent(rows != null ? $"{rows.Count} items" : "0 items", description), EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            if (rows == null || rows.Count == 0)
            {
                EditorGUILayout.LabelField("No matching facts yet. Add route intent facts or capability contracts when the studio spine learns a new path.", EditorStyles.wordWrappedMiniLabel);
                return;
            }

            int count = collapsedLimit > 0 && rows.Count > collapsedLimit ? collapsedLimit : rows.Count;
            for (int i = 0; i < count; i++)
                DrawIntentRow(rows[i]);

            if (count < rows.Count)
                EditorGUILayout.LabelField($"{rows.Count - count} more reflected rows are available in Facts.", EditorStyles.wordWrappedMiniLabel);
        }

        private static void DrawIntentRow(PyralisAuthoringIntentRow row)
        {
            if (row == null || row.Fact == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string state = row.State == PyralisAuthoringIntentRowState.Caution ? "Caution" : row.State.ToString();
                string foldoutKey = "Pyralis.AuthoringWindow.Intent." + row.Fact.StableId;
                bool expanded = GetFoldout(IntentRowFoldouts, foldoutKey, row.State == PyralisAuthoringIntentRowState.Caution);
                using (new EditorGUILayout.HorizontalScope())
                {
                    expanded = EditorGUILayout.Foldout(expanded, new GUIContent(row.Fact.DisplayName, row.Fact.Summary), true);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(new GUIContent(GetIntentTierLabel(row.Tier), "Guide priority from lane, goals, related intent ids, and cautions. Score " + row.Score + "."), GUILayout.Width(96f));
                    EditorGUILayout.LabelField(new GUIContent(state, row.Reason), GUILayout.Width(84f));
                }

                SetFoldout(IntentRowFoldouts, foldoutKey, expanded);
                PyralisAuthoringFactExplorerRenderer.DrawFactSemanticTags(row.Fact);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Why", row.Reason, "Why this row is currently visible for the selected project intent.");
                PyralisAuthoringWindowPrimitives.DrawMiniField("Priority", GetIntentTierLabel(row.Tier), "Guide priority. Numeric score is intentionally hidden by default so the user reads guidance, not a leaderboard.");
                PyralisAuthoringWindowPrimitives.DrawMiniField("First Proof", row.Fact.FirstProof, "The smallest native Unity proof this row is trying to help you reach.");

                if (!expanded)
                {
                    PyralisAuthoringWindowPrimitives.DrawMiniList("Customization", row.Fact.CustomizationMoments, "Creator-owned choices to make after the route skeleton is understood.", 2);
                    return;
                }

                PyralisAuthoringWindowPrimitives.DrawMiniField("What It Means", row.Fact.Summary, "The short descriptor provided by the reflective fact.");
                PyralisAuthoringWindowPrimitives.DrawMiniField("Route Relevance", row.Fact.RouteRelevance, "Why this fact matters to the route shape.");
                PyralisAuthoringWindowPrimitives.DrawMiniList("Supported Lanes", row.Fact.LaneTags, "Lanes where this fact is expected to fit cleanly.");
                PyralisAuthoringWindowPrimitives.DrawMiniList("Unsupported / Caution Lanes", row.Fact.UnsupportedLaneTags, "Lanes where this fact is usually not the clean fit.");
                PyralisAuthoringWindowPrimitives.DrawMiniList("Assignment Fields", row.Fact.AssignmentFields, "Unity fields or objects the creator may need to inspect or assign.");
                PyralisAuthoringWindowPrimitives.DrawMiniList("Customization", row.Fact.CustomizationMoments, "Creator-owned choices. Authoring guides these choices; it does not pick them.");
                PyralisAuthoringWindowPrimitives.DrawMiniList("Can Wait", row.Fact.CanWait, "Useful work to defer until the route's first proof is readable.");
            }
        }

        private bool TryRunGuidanceAction(PyralisAuthoringValidationIssue issue)
        {
            if (issue == null || issue.Target == null)
                return false;

            if (issue.IssueCode != null
                && (issue.IssueCode.StartsWith("sceneSurface.", System.StringComparison.Ordinal)
                    || issue.IssueCode.StartsWith("prefabReadiness.", System.StringComparison.Ordinal)))
            {
                return OpenMapForTarget(issue.Target);
            }

            switch (issue.IssueCode)
            {
                case "session.defaultGameMode.missing":
                case "session.defaultParticipants.missing":
                case "session.defaultParticipants.slot.empty":
                case "gameMode.setupProfile.missing":
                case "setupProfile.runtimePatterns.missing":
                case "setupProfile.runtimePatterns.slot.empty":
                case "setupProfile.runtimePatterns.duplicate":
                case "pawn.pawnPrefab.missing":
                    return OpenGuideForTarget(issue.Target);

                default:
                    return false;
            }
        }

        private bool OpenMapForTarget(Object target)
        {
            if (target == null)
                return false;

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);

            if (PyralisAuthoringSetupContextResolver.CanUseAsActiveSetup(target))
            {
                _pinnedActiveSetup = PyralisAuthoringSetupContextResolver.GetSetupContext(target);
                InvalidateAuthoringCache();
            }

            _mode = AuthoringWindowMode.Map;
            _mapScroll = Vector2.zero;
            Repaint();
            return true;
        }

        private bool OpenGuideForTarget(Object target)
        {
            if (target == null)
                return false;

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);

            if (PyralisAuthoringSetupContextResolver.CanUseAsActiveSetup(target))
            {
                _pinnedActiveSetup = PyralisAuthoringSetupContextResolver.GetSetupContext(target);
                InvalidateAuthoringCache();
            }

            _mode = AuthoringWindowMode.Guide;
            _guideScroll = Vector2.zero;
            Repaint();
            return true;
        }

        private static void FillMissingRuntimePatternText(RuntimePatternDefinition pattern)
        {
            Undo.RecordObject(pattern, "Fill Runtime Pattern Guidance Text");

            if (string.IsNullOrWhiteSpace(pattern.description))
                pattern.description = RuntimePatternAuthoringText.GetSuggestedDescription(pattern);

            if (string.IsNullOrWhiteSpace(pattern.setupNotes))
                pattern.setupNotes = RuntimePatternAuthoringText.GetSuggestedSetupNotes(pattern);

            pattern.Sanitize();
            EditorUtility.SetDirty(pattern);
        }

        private static string GetIntentTierLabel(PyralisAuthoringIntentGuideTier tier)
{
            return tier switch
            {
                PyralisAuthoringIntentGuideTier.Primary => "Strong match",
                PyralisAuthoringIntentGuideTier.SuggestedNext => "Suggested",
                PyralisAuthoringIntentGuideTier.OptionalEnhancer => "Can wait",
                PyralisAuthoringIntentGuideTier.Caution => "Caution",
                _ => tier.ToString()
            };
        }

        private static bool GetFoldout(Dictionary<string, bool> foldouts, string key, bool defaultValue)
        {
            return foldouts != null && !string.IsNullOrWhiteSpace(key) && foldouts.TryGetValue(key, out bool value)
                ? value
                : defaultValue;
        }

        private static void SetFoldout(Dictionary<string, bool> foldouts, string key, bool value)
        {
            if (foldouts == null || string.IsNullOrWhiteSpace(key))
                return;

            foldouts[key] = value;
        }

    }
}
