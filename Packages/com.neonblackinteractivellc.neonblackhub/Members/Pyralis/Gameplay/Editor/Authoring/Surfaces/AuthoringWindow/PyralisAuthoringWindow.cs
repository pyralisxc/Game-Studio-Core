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

        private const string SelectedAuthoringContextLabel = "Selected Authoring Context";

        private static readonly string[] ModeLabels = { "Overview", "Intent", "Guide", "Map", "Validate", "Facts" };
        private static readonly Dictionary<string, bool> ServiceStepFoldouts = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> IntentRowFoldouts = new Dictionary<string, bool>();
        private const double InspectorRepaintIntervalSeconds = 0.35d;

        private AuthoringWindowMode _mode = AuthoringWindowMode.Overview;
        [SerializeField] private Object _pinnedActiveSetup;
        [SerializeField] private Object _lastActiveSetup;
        [SerializeField] private bool _showBeginnerLocationTags = true;
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
        [SerializeField] private Vector2 _hygieneScroll;
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
                "tabHygiene" => AuthoringWindowMode.Validate,
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
            else if (_mode == AuthoringWindowMode.Validate)
            {
                PyralisAuthoringToolkitTabRenderer.DrawHygiene(
                    _contentRoot,
                    activeSetup,
                    new PyralisAuthoringIntentSelection(_intentLane, _intentCapabilities, _intentAxioms));
            }
            else if (_mode == AuthoringWindowMode.Map)
            {
                PyralisAuthoringToolkitTabRenderer.DrawMap(
                    _contentRoot,
                    activeSetup,
                    GetCachedRouteReport(activeSetup, true));
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
                AuthoringWindowMode.Validate => "tabHygiene",
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
                case AuthoringWindowMode.Validate: return ref _hygieneScroll;
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

        private void DrawActiveSetupBar(Object selection, Object activeSetup, Object selectionSetup, Object sceneFallbackSetup)
        {
            EditorGUILayout.LabelField("Active Setup", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope())
            {
                _showBeginnerLocationTags = EditorGUILayout.ToggleLeft("Beginner Location Tags", _showBeginnerLocationTags);
                string pinnedPrefix = _pinnedActiveSetup != null
                    ? "Pinned"
                    : selectionSetup != null
                        ? "Following Selection"
                        : sceneFallbackSetup != null && activeSetup == sceneFallbackSetup
                            ? "Scene Gameplay Root"
                            : activeSetup != null
                            ? "Remembered Setup"
                            : "Following Selection";
                string activeLabel = activeSetup != null ? $"{activeSetup.name} ({activeSetup.GetType().Name})" : "No setup context";
                EditorGUILayout.LabelField(pinnedPrefix, activeLabel, EditorStyles.wordWrappedLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(!PyralisAuthoringSetupContextResolver.CanUseAsActiveSetup(selection)))
                    {
                        if (GUILayout.Button("Pin Selection As Active Setup"))
                        {
                            _pinnedActiveSetup = PyralisAuthoringSetupContextResolver.GetSetupContext(selection);
                            InvalidateAuthoringCache();
                        }
                    }

                    using (new EditorGUI.DisabledScope(_pinnedActiveSetup == null))
                    {
                        if (GUILayout.Button("Clear Pin"))
                        {
                            _pinnedActiveSetup = null;
                            InvalidateAuthoringCache();
                        }
                    }

                    using (new EditorGUI.DisabledScope(activeSetup == null))
                    {
                        if (GUILayout.Button("Inspect Active Setup"))
                        {
                            Selection.activeObject = activeSetup;
                            EditorGUIUtility.PingObject(activeSetup);
                        }
                    }
                }

                EditorGUILayout.LabelField("Selection keeps the Guide reactive. Overview and Map use the active setup so the setup story stays steady while you inspect parts.", EditorStyles.wordWrappedMiniLabel);
                if (_pinnedActiveSetup == null && selection != null && activeSetup != null && selection != activeSetup && selectionSetup == activeSetup)
                    EditorGUILayout.LabelField("The selected object is already linked into this setup, so Authoring is keeping the scene root as the active setup while you inspect the selected field owner.", EditorStyles.wordWrappedMiniLabel);
                if (_pinnedActiveSetup == null && sceneFallbackSetup != null && activeSetup == sceneFallbackSetup)
                    EditorGUILayout.LabelField("Nothing is selected, so Authoring is using the single GameplaySessionBootstrap found in the open scene as the setup root.", EditorStyles.wordWrappedMiniLabel);
                else if (_pinnedActiveSetup == null && selectionSetup == null && activeSetup != null)
                    EditorGUILayout.LabelField("This route is remembered from the last setup anchor you selected. Use Clear Pin/Pin Selection when you want to change the setup story deliberately.", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private void DrawBeginnerLocationLegend()
        {
            if (!_showBeginnerLocationTags)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Beginner Location Tags", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("Use these colors to tell where a named thing lives while following setup guidance. Click a matching surface beacon when a step names a Unity tab.", EditorStyles.wordWrappedMiniLabel);
                PyralisAuthoringWindowPrimitives.DrawSemanticTagStrip(PyralisAuthoringLabelUtility.BeginnerLegendTags);
            }
        }

        private void DrawModeToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                for (int index = 0; index < ModeLabels.Length; index++)
                    DrawModeToolbarTab((AuthoringWindowMode)index, ModeLabels[index]);
            }

            DrawActiveModeAccent(_mode);
        }

        private void DrawModeToolbarTab(AuthoringWindowMode mode, string label)
        {
            bool selected = _mode == mode;
            string tabLabel = selected ? PyralisAuthoringWindowText.ColorizeModeTabLabel(label, GetModeAccentTag(mode)) : label;
            if (GUILayout.Toggle(selected, tabLabel, GetModeToolbarButtonStyle(selected), GUILayout.MinWidth(64f)))
                _mode = mode;
        }

        private static GUIStyle GetModeToolbarButtonStyle(bool selected)
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton)
            {
                alignment = TextAnchor.MiddleCenter,
                richText = true,
                fontStyle = selected ? FontStyle.Bold : FontStyle.Normal
            };
            return style;
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

        private static void DrawActiveModeAccent(AuthoringWindowMode mode)
        {
            PyralisAuthoringSemanticTag tag = GetModeAccentTag(mode);
            Color color = PyralisAuthoringLabelUtility.GetSemanticTagColor(tag);
            Rect rect = GUILayoutUtility.GetRect(1f, 3f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, color);
            PyralisAuthoringWindowText.DrawSemanticMiniLabel(GetModeOrientationText(mode));
        }

        private static PyralisAuthoringSemanticTag GetModeAccentTag(AuthoringWindowMode mode)
        {
            switch (mode)
            {
                case AuthoringWindowMode.Intent:
                    return PyralisAuthoringSemanticTag.Authoring;
                case AuthoringWindowMode.Guide:
                    return PyralisAuthoringSemanticTag.Inspector;
                case AuthoringWindowMode.Map:
                    return PyralisAuthoringSemanticTag.Hierarchy;
                case AuthoringWindowMode.Validate:
                    return PyralisAuthoringSemanticTag.Component;
                case AuthoringWindowMode.Facts:
                    return PyralisAuthoringSemanticTag.Project;
                default:
                    return PyralisAuthoringSemanticTag.PlayMode;
            }
        }

        private static string GetModeOrientationText(AuthoringWindowMode mode)
        {
            switch (mode)
            {
                case AuthoringWindowMode.Intent:
                    return "Intent names the game shape before Project assets, Hierarchy objects, Inspector fields, and Play Mode proof compete for attention.";
                case AuthoringWindowMode.Guide:
                    return "Guide explains the selected Unity object or asset and points to the next Inspector or Project surface.";
                case AuthoringWindowMode.Map:
                    return "Map shows where the active setup chain lives across Hierarchy roots, Project assets, Prefabs, and Inspector fields.";
                case AuthoringWindowMode.Validate:
                    return "Validate separates visible Evidence from actual Play Mode proof.";
                case AuthoringWindowMode.Facts:
                    return "Facts is the advanced coverage map: read-only contracts, reflection, convention, and proof targets for future route work.";
                default:
                    return "Overview keeps the current proof path calm: Do Now, Proof Enhancers, then Feature Cards.";
            }
        }

        private void DrawOverviewMode(Object activeSetup, Object selection, PyralisAuthoringRouteReport report, PyralisAuthoringRouteReport selectionReport)
        {
            bool selectedSetupProfile = selection is GameSetupProfile;
            Object currentStepSelection = selectedSetupProfile ? selection : activeSetup != null ? activeSetup : selection;
            PyralisAuthoringRouteReport currentStepReport = selectedSetupProfile ? selectionReport : activeSetup != null ? report : selectionReport;
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(activeSetup, report);

            EditorGUILayout.LabelField("Overview Dashboard", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PyralisAuthoringOverviewRenderer.DrawGuidanceCard(model, report);

                EditorGUILayout.LabelField("Route", model.RouteName);
                EditorGUILayout.LabelField("Blocking Setup Clear", model.ReadyToPressPlay ? "Yes - selected intent's Do Now setup is clear. Proof Enhancers are optional." : "No - selected intent still has Do Now setup to finish.");
                PyralisAuthoringOverviewRenderer.DrawActionButtons(model, OpenIntentFromOverview, OpenMapFromOverview, OpenValidateFromOverview);
                PyralisAuthoringOverviewRenderer.DrawFirstProofCard(model);
                PyralisAuthoringOverviewRenderer.DrawPlayModeChecklist(model);
                PyralisAuthoringOverviewRenderer.DrawContractProofGuidance(activeSetup, report);
                EditorGUILayout.LabelField("Active Setup", activeSetup != null ? $"{activeSetup.name} ({activeSetup.GetType().Name})" : "Nothing pinned or inferred");
                EditorGUILayout.LabelField("Selected Context", selection != null ? $"{selection.name} ({selection.GetType().Name})" : "Nothing selected");

                PyralisAuthoringOverviewRenderer.DrawLane("Do Now", "Intent-required missing or blocked work only.", model.DoNow);
                PyralisAuthoringOverviewRenderer.DrawLane("Proof Enhancers", "Recommended by this intent once Do Now is clear. Wire only what the first proof depends on.", model.DoSoon);
                PyralisAuthoringOverviewRenderer.DrawLane("Feature Cards", "Optional next capabilities, polish, advanced systems, and setup that can safely wait.", model.Later);
            }

            EditorGUILayout.Space(12f);
            DrawCurrentStepPanel(currentStepSelection, currentStepReport);

            PyralisFeatureAdvisorRenderer.Draw(PyralisAuthoringSetupContextResolver.GetSelectedSetupProfile(activeSetup, PyralisAuthoringSetupContextResolver.GetSelectedMode(activeSetup, PyralisAuthoringSetupContextResolver.GetSelectedSession(activeSetup, PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(activeSetup)))));
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
            _hygieneScroll = Vector2.zero;
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
