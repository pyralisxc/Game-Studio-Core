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
            Hygiene,
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
        [SerializeField] private Vector2 _hygieneScroll;
        [SerializeField] private Vector2 _guideScroll;
        [SerializeField] private Vector2 _factsScroll;
        private double _lastInspectorRepaintTime;
        private int _authoringCacheVersion;
        private string _cachedIntentModelKey;
        private PyralisAuthoringIntentModel _cachedIntentModel;
        private string _cachedCurrentSetupGraphKey;
        private PyralisAuthoringSetupGraph _cachedCurrentSetupGraph;
        private string _cachedIntentProjectedSetupGraphKey;
        private PyralisAuthoringSetupGraph _cachedIntentProjectedSetupGraph;

        private VisualElement _contentRoot;

        [MenuItem("NeonBlack/Gameplay/Pyralis Authoring Window")]
        [MenuItem("Window/Pyralis Authoring")]
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
                "tabHygiene" => AuthoringWindowMode.Hygiene,
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
                    try
                    {
                        DrawModeContent(currentActiveSetup, currentSelection);
                    }
                    finally
                    {
                        EditorGUILayout.EndScrollView();
                    }
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
                AuthoringWindowMode.Hygiene => "tabHygiene",
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
                case AuthoringWindowMode.Hygiene: return ref _hygieneScroll;
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
                        selection);
                    break;
                case AuthoringWindowMode.Guide:
                    DrawGuideMode(
                        selection,
                        activeSetup,
                        GetCachedCurrentSetupGraph(activeSetup != null ? activeSetup : selection));
                    break;
                case AuthoringWindowMode.Map:
                    PyralisAuthoringMapRenderer.Draw(activeSetup, selection, GetCachedCurrentSetupGraph(activeSetup));
                    break;
                case AuthoringWindowMode.Hygiene:
                    PyralisAuthoringHygieneRenderer.Draw(
                        activeSetup,
                        GetCachedCurrentSetupGraph(activeSetup),
                        GetCachedIntentProjectedSetupGraph(activeSetup));
                    break;
                case AuthoringWindowMode.Facts:
                    PyralisAuthoringFactExplorerRenderer.Draw(activeSetup, GetCachedCurrentSetupGraph(activeSetup));
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
            _cachedCurrentSetupGraphKey = null;
            _cachedCurrentSetupGraph = null;
            _cachedIntentProjectedSetupGraphKey = null;
            _cachedIntentProjectedSetupGraph = null;
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

        private void DrawOverviewMode(Object activeSetup, Object selection)
        {
            Object graphSource = activeSetup != null ? activeSetup : null;
            PyralisAuthoringSetupGraph graph = GetCachedCurrentSetupGraph(graphSource);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(activeSetup, graph);

            EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PyralisAuthoringOverviewRenderer.DrawGuidanceCard(model, graph);
                PyralisAuthoringOverviewRenderer.DrawActionButtons(model, OpenIntentFromOverview, OpenGuideFromOverview, OpenMapFromOverview);
            }

            EditorGUILayout.Space(12f);
            PyralisAuthoringOverviewRenderer.DrawFirstProofCard(model, graph);
            PyralisAuthoringOverviewRenderer.DrawLane("Do Now", "Only route-required missing or blocked work appears here.", model.DoNow);
            PyralisAuthoringOverviewRenderer.DrawLane("Proof Enhancers", "Useful before Play Mode when they make the first proof clearer.", model.DoSoon);
        }

        private void OpenIntentFromOverview()
        {
            _intentScroll = Vector2.zero;
            SwitchMode(AuthoringWindowMode.Intent);
        }

        private void OpenGuideFromOverview()
        {
            _guideScroll = Vector2.zero;
            SwitchMode(AuthoringWindowMode.Guide);
        }

        private void OpenMapFromOverview()
        {
            _mapScroll = Vector2.zero;
            SwitchMode(AuthoringWindowMode.Map);
        }

        private PyralisAuthoringIntentModel GetCachedIntentModel()
        {
            string key = $"{_intentLane}_{_intentAxioms}_{_intentCapabilities}_{_authoringCacheVersion}";
            if (_cachedIntentModelKey == key && _cachedIntentModel != null)
                return _cachedIntentModel;

            _cachedIntentModelKey = key;
            _cachedIntentModel = PyralisAuthoringSetupGraphProjection.BuildIntentModel(
                GetCurrentIntentSelection());
            return _cachedIntentModel;
        }

        private PyralisAuthoringIntentSelection GetCurrentIntentSelection()
        {
            return new PyralisAuthoringIntentSelection(_intentLane, _intentCapabilities, _intentAxioms);
        }

        private PyralisAuthoringSetupGraph GetCachedCurrentSetupGraph(Object graphSource)
        {
            string key = GetSetupGraphCacheKey(graphSource, includeIntent: false);
            if (string.Equals(_cachedCurrentSetupGraphKey, key, StringComparison.Ordinal) && _cachedCurrentSetupGraph != null)
                return _cachedCurrentSetupGraph;

            _cachedCurrentSetupGraphKey = key;
            _cachedCurrentSetupGraph = PyralisAuthoringSetupGraphBuilder.Build(graphSource);
            return _cachedCurrentSetupGraph;
        }

        private PyralisAuthoringSetupGraph GetCachedIntentProjectedSetupGraph(Object graphSource)
        {
            string key = GetSetupGraphCacheKey(graphSource, includeIntent: true);
            if (string.Equals(_cachedIntentProjectedSetupGraphKey, key, StringComparison.Ordinal) && _cachedIntentProjectedSetupGraph != null)
                return _cachedIntentProjectedSetupGraph;

            _cachedIntentProjectedSetupGraphKey = key;
            _cachedIntentProjectedSetupGraph = PyralisAuthoringSetupGraphBuilder.Build(graphSource, GetCurrentIntentSelection());
            return _cachedIntentProjectedSetupGraph;
        }

        private string GetSetupGraphCacheKey(Object graphSource, bool includeIntent)
        {
            string sourceKey = graphSource != null
                ? GlobalObjectId.GetGlobalObjectIdSlow(graphSource).ToString()
                : "null";
            if (!includeIntent)
                return sourceKey + ":current:" + _authoringCacheVersion;

            return sourceKey
                + ":intent:"
                + _intentLane
                + ":" + _intentAxioms
                + ":" + _intentCapabilities
                + ":" + _authoringCacheVersion;
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
