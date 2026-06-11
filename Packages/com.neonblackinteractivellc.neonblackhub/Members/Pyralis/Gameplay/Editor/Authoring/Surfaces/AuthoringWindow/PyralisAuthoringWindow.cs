using System;
using System.Linq;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

using NeonBlack.Gameplay.Editor.Authoring;

namespace NeonBlack.Gameplay.Editor
{
    public class PyralisAuthoringWindow : EditorWindow
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

        private const string GuidedSetupRouteLabel = "Guided Setup Route";
        private const string SelectedAuthoringContextLabel = "Selected Authoring Context";
        private const string SetupChainLabel = "Setup Chain";

        private static readonly string[] ModeLabels = { "Overview", "Intent", "Guide", "Map", "Validate", "Facts" };
        private static readonly RuntimeCapabilityFamily[] GuidedCapabilityFamilies =
        {
            RuntimeCapabilityFamily.CharacterPawnGameplay,
            RuntimeCapabilityFamily.Combat,
            RuntimeCapabilityFamily.GunsProjectiles,
            RuntimeCapabilityFamily.ActionTargeting,
            RuntimeCapabilityFamily.BoardCardTabletop,
            RuntimeCapabilityFamily.CameraInput,
            RuntimeCapabilityFamily.AnimationPresentation,
            RuntimeCapabilityFamily.ScoringObjectives,
            RuntimeCapabilityFamily.ProceduralGeneration,
            RuntimeCapabilityFamily.Networking
        };
        private static readonly RuntimeCapabilityLaneTag[] GuidedCapabilityLaneTags =
        {
            RuntimeCapabilityLaneTag.Sprite2D,
            RuntimeCapabilityLaneTag.Billboard2_5D,
            RuntimeCapabilityLaneTag.ThirdPerson3D,
            RuntimeCapabilityLaneTag.TabletopBoard,
            RuntimeCapabilityLaneTag.UiMenuOnly,
            RuntimeCapabilityLaneTag.CameraCursor,
            RuntimeCapabilityLaneTag.Mixed
        };
private static readonly SemanticTokenRule[] SemanticTokenRules =
        {
            new SemanticTokenRule(PyralisAuthoringSemanticTag.PlayMode, "Play Mode proof"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.PlayMode, "Play Mode"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.PlayMode, "proof"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Project, "Project window"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Hierarchy, "Hierarchy window"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Inspector, "Inspector Add Component"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Inspector, "Object Picker"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Input, "Input Action Asset"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Project, "CreateAssetMenu"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Component, "AddComponentMenu"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Component, "RequireComponent"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Definition, "Definition"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Profile, "Profile"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Prefab, "Prefab"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Component, "Component"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Project, "Project"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Hierarchy, "Hierarchy"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Inspector, "Inspector"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Input, "Input"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.UI, "Canvas"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.UI, "HUD"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.UI, "UI"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Animation, "Animator"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Animation, "Animation"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Audio, "Audio"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Audio, "Sound"),
            new SemanticTokenRule(PyralisAuthoringSemanticTag.Authoring, "Authoring")
        };
        private static readonly Dictionary<string, bool> ServiceStepFoldouts = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> IntentRowFoldouts = new Dictionary<string, bool>();
        private static readonly Dictionary<string, bool> GoalCategoryFoldouts = new Dictionary<string, bool>();
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

        private readonly struct SemanticTokenRule
        {
            public SemanticTokenRule(PyralisAuthoringSemanticTag tag, string token)
            {
                Tag = tag;
                Token = token ?? string.Empty;
            }

            public PyralisAuthoringSemanticTag Tag { get; }
            public string Token { get; }
        }

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

            if (_mode == AuthoringWindowMode.Intent)
            {
                RefreshIntentTab();
            }
            else if (_mode == AuthoringWindowMode.Validate)
            {
                RefreshHygieneTab();
            }
            else if (_mode == AuthoringWindowMode.Map)
            {
                RefreshMapTab();
            }
            else
            {
                _contentRoot.Add(new IMGUIContainer(() =>
                {
                    // We use the same logic as the old OnGUI but skip the layout headers
                    Object selection = Selection.activeObject;
                    Object selectionSetup = GetSetupContext(selection);
                    Object sceneFallbackSetup = GetSceneFallbackSetup(selection, selectionSetup);
                    Object activeSetup = ResolveActiveSetup(selection, selectionSetup, sceneFallbackSetup, _pinnedActiveSetup, _lastActiveSetup);
                    
                    ref Vector2 scroll = ref GetCurrentScroll();
                    scroll = EditorGUILayout.BeginScrollView(scroll);
                    DrawModeContent(activeSetup, selection);
                    EditorGUILayout.EndScrollView();
                }));
            }
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

        private void RefreshIntentTab()
        {
            var axiomContainer = new VisualElement() { name = "axiomContainer" };
            axiomContainer.AddToClassList("section");
            var axiomTitle = new Label("DNA AXIOMS");
            axiomTitle.AddToClassList("section-title");
            axiomContainer.Add(axiomTitle);
            var axiomToggles = new VisualElement() { name = "axiomToggles" };
            axiomContainer.Add(axiomToggles);
            PopulateAxioms(axiomToggles);

            var laneContainer = new VisualElement() { name = "laneContainer" };
            laneContainer.AddToClassList("section");
            var laneTitle = new Label("PRESENTATION LANE");
            laneTitle.AddToClassList("section-title");
            laneContainer.Add(laneTitle);
            PopulateLanes(laneContainer);

            var capabilityContainer = new VisualElement() { name = "capabilityContainer" };
            capabilityContainer.AddToClassList("section");
            var capTitle = new Label("ENGINE SPINE CAPABILITIES");
            capTitle.AddToClassList("section-title");
            capabilityContainer.Add(capTitle);
            PopulateCapabilities(capabilityContainer);

            var advisorContainer = new VisualElement() { name = "advisorContainer" };
            advisorContainer.AddToClassList("section");
            var advisorTitle = new Label("INTENT ADVISOR");
            advisorTitle.AddToClassList("section-title");
            advisorContainer.Add(advisorTitle);
            
            var intentSummary = new Label("Project DNA is defined by... Engine Spine capabilities: ...") { name = "intentSummary" };
            intentSummary.AddToClassList("intent-card-summary");
            advisorContainer.Add(intentSummary);

            var sidebar = new VisualElement() { name = "sidebar" };
            sidebar.AddToClassList("intent-sidebar");
            sidebar.Add(axiomContainer);
            sidebar.Add(laneContainer);

            var main = new VisualElement() { name = "main" };
            main.AddToClassList("intent-main");
            main.Add(capabilityContainer);
            main.Add(advisorContainer);

            var intentView = new VisualElement() { name = "intentView" };
            intentView.AddToClassList("intent-container");
            intentView.Add(sidebar);
            intentView.Add(main);

            _contentRoot.Add(intentView);
            UpdateAdvisor(_contentRoot);
        }

        private void PopulateAxioms(VisualElement container)
        {
            if (container == null) return;

            AddAxiomDropdown(container, "Dimensionality", 
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Dimensions3D,
                AuthoringWorldAxiom.Dimensions2D, AuthoringWorldAxiom.Dimensions3D);

            AddAxiomDropdown(container, "Physics Gravity", 
                AuthoringWorldAxiom.GravityVertical | AuthoringWorldAxiom.GravityRadial | AuthoringWorldAxiom.GravityNone,
                AuthoringWorldAxiom.GravityVertical, AuthoringWorldAxiom.GravityRadial, AuthoringWorldAxiom.GravityNone);

            AddAxiomDropdown(container, "Sequence Timeline", 
                AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.TurnBased,
                AuthoringWorldAxiom.Realtime, AuthoringWorldAxiom.TurnBased);

            AddAxiomDropdown(container, "Spatial Topology", 
                AuthoringWorldAxiom.BoundedSpace | AuthoringWorldAxiom.WrappedSpace | AuthoringWorldAxiom.InfiniteSpace,
                AuthoringWorldAxiom.BoundedSpace, AuthoringWorldAxiom.WrappedSpace, AuthoringWorldAxiom.InfiniteSpace);

            AddAxiomDropdown(container, "Networking", 
                AuthoringWorldAxiom.Networked,
                AuthoringWorldAxiom.Networked);
        }

        private void AddAxiomDropdown(VisualElement container, string label, AuthoringWorldAxiom mask, params AuthoringWorldAxiom[] options)
        {
            List<string> choices = new List<string> { "None" };
            int selectedIndex = 0;
            AuthoringWorldAxiom current = _intentAxioms & mask;

            for (int i = 0; i < options.Length; i++)
            {
                choices.Add(AuthoringWorldAxiomRegistry.GetDisplayName(options[i]));
                if (current == options[i])
                    selectedIndex = i + 1;
            }

            var dropdown = new DropdownField(label, choices, selectedIndex);
            
            // Initial tooltip
            if (selectedIndex > 0)
                dropdown.tooltip = AuthoringWorldAxiomRegistry.GetTooltip(options[selectedIndex - 1]);
            else
                dropdown.tooltip = "No mechanical axiom selected for this category.";

            dropdown.RegisterValueChangedCallback(evt =>
            {
                int index = dropdown.index;
                _intentAxioms &= ~mask;
                if (index > 0)
                {
                    AuthoringWorldAxiom selected = options[index - 1];
                    _intentAxioms |= selected;
                    dropdown.tooltip = AuthoringWorldAxiomRegistry.GetTooltip(selected);
                }
                else
                {
                    dropdown.tooltip = "No mechanical axiom selected for this category.";
                }
                InvalidateAuthoringCache();
                UpdateAdvisor(rootVisualElement);
            });
            container.Add(dropdown);
        }

        private void PopulateLanes(VisualElement container)
        {
            if (container == null) return;
            
            var options = (RuntimeCapabilityLaneTag[])System.Enum.GetValues(typeof(RuntimeCapabilityLaneTag));
            List<string> choices = new List<string>();
            int selectedIndex = 0;
            
            for (int i = 0; i < options.Length; i++)
            {
                choices.Add(options[i].ToString());
                if (_intentLane == options[i])
                    selectedIndex = i;
            }

            var dropdown = new DropdownField("Active Lane", choices, selectedIndex);
            dropdown.tooltip = RuntimeCapabilityLaneRegistry.GetTooltip(_intentLane);
            
            dropdown.RegisterValueChangedCallback(evt =>
            {
                _intentLane = options[dropdown.index];
                dropdown.tooltip = RuntimeCapabilityLaneRegistry.GetTooltip(_intentLane);
                InvalidateAuthoringCache();
                UpdateAdvisor(rootVisualElement);
            });
            container.Add(dropdown);
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
                    DrawMapMode(activeSetup, selection, GetCachedRouteReport(activeSetup, true));
                    break;
                case AuthoringWindowMode.Validate:
                    DrawValidateMode(activeSetup, GetCachedRouteReport(activeSetup, true));
                    break;
                case AuthoringWindowMode.Facts:
                    DrawFactExplorerMode(activeSetup);
                    break;
            }
        }

        private void PopulateCapabilities(VisualElement container)
        {
            if (container == null) return;

            var searchField = new UnityEditor.UIElements.ToolbarSearchField();
            searchField.value = _intentGoalFilter;
            searchField.style.width = new Length(100, LengthUnit.Percent);
            searchField.RegisterValueChangedCallback(evt =>
            {
                _intentGoalFilter = evt.newValue;
                FilterCapabilities(container, _intentGoalFilter);
            });
            container.Add(searchField);

            var grid = new VisualElement() { name = "capabilityGridInternal" };
            grid.AddToClassList("capability-grid");
            container.Add(grid);
            
            var groups = new Dictionary<string, (AuthoringCapability[] caps, bool foldout)>
            {
                { "Core & Shell", (new[] { AuthoringCapability.Setup, AuthoringCapability.Session, AuthoringCapability.Rules, AuthoringCapability.Participants, AuthoringCapability.Scoring, AuthoringCapability.Input, AuthoringCapability.UI, AuthoringCapability.Audio }, _coreFoldout) },
                { "Actor & Action", (new[] { AuthoringCapability.Movement, AuthoringCapability.KineticMotor2D, AuthoringCapability.KineticMotor3D, AuthoringCapability.Steering2D, AuthoringCapability.Steering3D, AuthoringCapability.Traversal, AuthoringCapability.Combat, AuthoringCapability.CombatState, AuthoringCapability.CombatSensors, AuthoringCapability.MeleeFlow, AuthoringCapability.RangedFlow, AuthoringCapability.TacticsAggressive, AuthoringCapability.TacticsDefensive, AuthoringCapability.Animation, AuthoringCapability.VFX }, _actorFoldout) },
{ "Strategy & Progression", (new[] { AuthoringCapability.Stats, AuthoringCapability.Inventory, AuthoringCapability.Dialogue, AuthoringCapability.Quests, AuthoringCapability.Vendors, AuthoringCapability.SkillTree, AuthoringCapability.Progression, AuthoringCapability.Tabletop, AuthoringCapability.Grid }, _strategyFoldout) },
                { "World & Meta", (new[] { AuthoringCapability.Camera, AuthoringCapability.Environment, AuthoringCapability.Networking, AuthoringCapability.TurnBased, AuthoringCapability.Puzzle }, _worldFoldout) }
            };

            foreach (var group in groups)
            {
                var foldout = new Foldout();
                foldout.text = group.Key;
                foldout.value = group.Value.foldout;
                foldout.AddToClassList("capability-group-foldout");
                
                // Track foldout state
                string key = group.Key;
                foldout.RegisterValueChangedCallback(evt =>
                {
                    if (key == "Core & Shell") _coreFoldout = evt.newValue;
                    else if (key == "Actor & Action") _actorFoldout = evt.newValue;
                    else if (key == "Strategy & Progression") _strategyFoldout = evt.newValue;
                    else if (key == "World & Meta") _worldFoldout = evt.newValue;
                });

                foreach (var cap in group.Value.caps)
                {
                    var toggle = new Toggle(AuthoringCapabilityRegistry.GetDisplayName(cap));
                    toggle.name = "cap_" + cap.ToString();
                    toggle.value = (_intentCapabilities & cap) != 0;
                    toggle.tooltip = AuthoringCapabilityRegistry.GetTooltip(cap);
                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        if (evt.newValue) _intentCapabilities |= cap;
                        else _intentCapabilities &= ~cap;
                        InvalidateAuthoringCache();
                        UpdateAdvisor(rootVisualElement);
                    });
                    foldout.Add(toggle);
                }
                grid.Add(foldout);
            }
        }

        private void FilterCapabilities(VisualElement container, string filter)
        {
            var grid = container.Q<VisualElement>("capabilityGridInternal");
            if (grid == null) return;

            bool hasFilter = !string.IsNullOrWhiteSpace(filter);
            filter = filter?.ToLowerInvariant();

            foreach (var element in grid.Children())
            {
                if (element is Foldout foldout)
                {
                    int visibleToggles = 0;
                    foreach (var child in foldout.contentContainer.Children())
                    {
                        if (child is Toggle toggle)
                        {
                            bool matches = !hasFilter || toggle.label.ToLowerInvariant().Contains(filter);
                            toggle.style.display = matches ? DisplayStyle.Flex : DisplayStyle.None;
                            if (matches) visibleToggles++;
                        }
                    }
                    foldout.style.display = (visibleToggles > 0 || !hasFilter) ? DisplayStyle.Flex : DisplayStyle.None;
                    if (hasFilter && visibleToggles > 0)
                        foldout.value = true;
                }
            }
        }

        private void RefreshHygieneTab()
        {
            if (_contentRoot == null) return;
            _contentRoot.Clear();

            var hygieneView = new VisualElement();
            hygieneView.AddToClassList("hygiene-view");
            
            var title = new Label("PROJECT HYGIENE AUDIT");
            title.AddToClassList("section-title");
            hygieneView.Add(title);

            var list = new VisualElement() { name = "globalHygieneList" };
            hygieneView.Add(list);
            
            _contentRoot.Add(hygieneView);
            UpdateHygiene(list);
        }

        private void RefreshMapTab()
        {
            if (_contentRoot == null) return;
            _contentRoot.Clear();

            Object selection = Selection.activeObject;
            Object activeSetup = ResolveActiveSetup(selection, GetSetupContext(selection), GetSceneFallbackSetup(selection, GetSetupContext(selection)), _pinnedActiveSetup, _lastActiveSetup);
            PyralisAuthoringRouteReport report = GetCachedRouteReport(activeSetup, true);

            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(activeSetup);
            SessionDefinition session = GetSelectedSession(activeSetup, bootstrap);
            GameModeDefinition mode = GetSelectedMode(activeSetup, session);
            GameSetupProfile setupProfile = GetSelectedSetupProfile(activeSetup, mode);

            var mapView = new VisualElement();
            mapView.AddToClassList("map-view");
            
            var title = new Label("SETUP CHAIN MAP");
            title.AddToClassList("section-title");
            mapView.Add(title);

            var info = new Label("Use this map to understand how the active setup is connected. Select an object to see its details in the Inspector.");
            info.style.opacity = 0.7f;
            info.style.marginBottom = 10;
            info.style.whiteSpace = WhiteSpace.Normal;
            mapView.Add(info);

            var chainContainer = new VisualElement() { name = "chainContainer" };
            chainContainer.AddToClassList("setup-chain-container");
            mapView.Add(chainContainer);

            // 1. Bootstrap
            AddChainLink(chainContainer, "BOOTSTRAP", bootstrap, "The scene entry point. Initializes services and starts the session.", "Select a GameplaySessionBootstrap or Gameplay Root object.");
            
            // 2. Session
            AddChainLink(chainContainer, "SESSION", session, "Defines participants, networking, and rules.", "Create or assign the first asset the scene root reads.");

            // 3. Game Mode
            AddChainLink(chainContainer, "GAME MODE", mode, "The specific setup recipe and win/loss rules.", "Create or assign the rules asset for this session.");

            // 4. Setup Recipe
            AddChainLink(chainContainer, "SETUP RECIPE", setupProfile, "Combines capability patterns before prefab or scene wiring starts.", "Create or assign the recipe that combines game capability patterns.");

            if (report != null && !string.IsNullOrEmpty(report.NextStep))
            {
                var recContainer = new VisualElement();
                recContainer.style.marginTop = 15;
                recContainer.style.paddingTop = 10;
                recContainer.style.paddingBottom = 10;
                recContainer.style.paddingLeft = 10;
                recContainer.style.paddingRight = 10;
                recContainer.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
                recContainer.style.borderLeftWidth = 4;
                recContainer.style.borderLeftColor = new Color(1f, 0.8f, 0f);

                var recTitle = new Label("NEXT RECOMMENDED ACTION");
                recTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
                recTitle.style.fontSize = 10;
                recTitle.style.opacity = 0.6f;
                recContainer.Add(recTitle);

                var recText = new Label(report.NextStep);
                recText.style.whiteSpace = WhiteSpace.Normal;
                recContainer.Add(recText);

                mapView.Add(recContainer);
            }
            
            _contentRoot.Add(mapView);
        }

        private void AddChainLink(VisualElement container, string step, Object target, string description, string missingMessage)
        {
            bool isConnected = target != null;
            var link = new VisualElement();
            link.AddToClassList("setup-link");
            link.style.marginBottom = 10;
            link.style.paddingLeft = 10;
            link.style.borderLeftWidth = 4;
            link.style.borderLeftColor = isConnected ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.4f, 0.4f);

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            
            var leftSide = new VisualElement();
            leftSide.style.flexDirection = FlexDirection.Row;

            var stepLabel = new Label(step);
            stepLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            stepLabel.style.width = 100;
            leftSide.Add(stepLabel);

            var statusLabel = new Label(isConnected ? $"CONNECTED: {target.name}" : "MISSING");
            statusLabel.style.color = isConnected ? new Color(0.8f, 0.8f, 0.8f) : new Color(1f, 0.6f, 0.6f);
            leftSide.Add(statusLabel);
            header.Add(leftSide);

            if (isConnected)
            {
                var selectBtn = new Button(() => Selection.activeObject = target) { text = "SELECT" };
                selectBtn.style.height = 18;
                selectBtn.style.fontSize = 9;
                header.Add(selectBtn);
            }

            link.Add(header);

            var descLabel = new Label(isConnected ? description : missingMessage);
            descLabel.style.whiteSpace = WhiteSpace.Normal;
            descLabel.style.fontSize = 11;
            descLabel.style.opacity = isConnected ? 0.7f : 1f;
            link.Add(descLabel);

            container.Add(link);
        }

        private void UpdateHygiene(VisualElement container)
        {
            if (container == null) return;
            container.Clear();

            Object selection = Selection.activeObject;
            Object activeSetup = ResolveActiveSetup(selection, GetSetupContext(selection), GetSceneFallbackSetup(selection, GetSetupContext(selection)), _pinnedActiveSetup, _lastActiveSetup);
            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(activeSetup);
            SessionDefinition session = GetSelectedSession(activeSetup, bootstrap);

            var scanIssues = PyralisAssetHygieneScanner.Scan(session);

            var selectionIntent = new PyralisAuthoringIntentSelection(_intentLane, _intentCapabilities, _intentAxioms);
            var model = PyralisAuthoringIntentAdvisor.Build(selectionIntent);

            if (model.HygieneIssues.Count == 0 && scanIssues.Count == 0)
            {
                var success = new VisualElement();
                success.style.paddingTop = 20;
                success.style.alignItems = Align.Center;
                
                var label = new Label("✓ PROJECT HYGIENE: 100%") { name = "hygieneSuccess" };
                label.style.fontSize = 18;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
                label.style.color = new Color(0.4f, 1f, 0.4f);
                success.Add(label);
                
                var subLabel = new Label("All active authoring contracts have high-fidelity metadata and proofs.") { name = "hygieneSubLabel" };
                subLabel.style.opacity = 0.7f;
                success.Add(subLabel);
                
                container.Add(success);
            }
            else
            {
                var header = new Label($"HYGIENE ALERTS ({model.HygieneIssues.Count + scanIssues.Count})");
                header.style.unityFontStyleAndWeight = FontStyle.Bold;
                header.style.marginBottom = 8;
                container.Add(header);

                foreach (var issue in model.HygieneIssues)
                {
                    var issueBox = new HelpBox(issue.Reason, GetHelpBoxType(issue.Severity));
                    issueBox.style.marginBottom = 4;
                    container.Add(issueBox);
                }

                if (scanIssues.Count > 0)
                {
                    var subHeader = new Label("ASSET CONFIGURATION ISSUES");
                    subHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
                    subHeader.style.marginTop = 8;
                    subHeader.style.marginBottom = 4;
                    subHeader.style.opacity = 0.7f;
                    container.Add(subHeader);

                    foreach (var issue in scanIssues)
                    {
                        var row = new VisualElement();
                        row.style.flexDirection = FlexDirection.Row;
                        row.style.marginBottom = 2;
                        
                        var btn = new Button(() => { Selection.activeObject = issue.Asset; }) { text = "VIEW" };
                        btn.style.width = 50;
                        row.Add(btn);

                        var msg = new Label(issue.Message);
                        msg.style.marginLeft = 5;
                        msg.style.unityTextAlign = TextAnchor.MiddleLeft;
                        row.Add(msg);

                        container.Add(row);
                    }
                }
            }
        }

        private void UpdateAdvisor(VisualElement root)
        {
            var summaryLabel = root.Q<Label>("intentSummary");
            if (summaryLabel == null) return;

            var selection = new PyralisAuthoringIntentSelection(_intentLane, _intentCapabilities, _intentAxioms);
            var model = PyralisAuthoringIntentAdvisor.Build(selection);

            summaryLabel.text = model.Summary;
        }

        private HelpBoxMessageType GetHelpBoxType(PyralisAuthoringIssueSeverity severity)
        {
            return severity switch
            {
                PyralisAuthoringIssueSeverity.Required => HelpBoxMessageType.Error,
                PyralisAuthoringIssueSeverity.Blocked => HelpBoxMessageType.Error,
                PyralisAuthoringIssueSeverity.Bug => HelpBoxMessageType.Error,
                PyralisAuthoringIssueSeverity.Recommended => HelpBoxMessageType.Warning,
                PyralisAuthoringIssueSeverity.Optional => HelpBoxMessageType.Info,
                PyralisAuthoringIssueSeverity.Info => HelpBoxMessageType.Info,
                _ => HelpBoxMessageType.None
            };
        }

        private void OnSelectionChange()
        {
            InvalidateAuthoringCache();
            Object selection = Selection.activeObject;
            if (_mode == AuthoringWindowMode.Intent
                && selection is GameObject selectedGameObject
                && selectedGameObject.GetComponent<GameplaySessionBootstrap>() == null
                && GetSetupContext(selection) == null)
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
                    using (new EditorGUI.DisabledScope(!CanUseAsActiveSetup(selection)))
                    {
                        if (GUILayout.Button("Pin Selection As Active Setup"))
                        {
                            _pinnedActiveSetup = GetSetupContext(selection);
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
                DrawSemanticTagStrip(PyralisAuthoringLabelUtility.BeginnerLegendTags);
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
            string tabLabel = selected ? ColorizeModeTabLabel(label, GetModeAccentTag(mode)) : label;
            if (GUILayout.Toggle(selected, tabLabel, GetModeToolbarButtonStyle(selected), GUILayout.MinWidth(64f)))
                _mode = mode;
        }

        private static string ColorizeModeTabLabel(string label, PyralisAuthoringSemanticTag tag)
        {
            Color color = PyralisAuthoringLabelUtility.GetSemanticTagColor(tag);
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{label}</color>";
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
            DrawSemanticMiniLabel(GetModeOrientationText(mode));
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
            Object currentStepSelection = activeSetup != null ? activeSetup : selection;
            PyralisAuthoringRouteReport currentStepReport = activeSetup != null ? report : selectionReport;
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(activeSetup, report);

            EditorGUILayout.LabelField("Overview Dashboard", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawOverviewGuidanceCard(model, report);

                EditorGUILayout.LabelField("Route", model.RouteName);
                EditorGUILayout.LabelField("Blocking Setup Clear", model.ReadyToPressPlay ? "Yes - required setup is clear. Proof Enhancers are optional." : "No - required setup is missing.");
                DrawOverviewActionButtons(model);
                DrawFirstProofCard(model);
                DrawPlayModeChecklist(model);
                DrawContractProofGuidance(activeSetup, report);
                EditorGUILayout.LabelField("Active Setup", activeSetup != null ? $"{activeSetup.name} ({activeSetup.GetType().Name})" : "Nothing pinned or inferred");
                EditorGUILayout.LabelField("Selected Context", selection != null ? $"{selection.name} ({selection.GetType().Name})" : "Nothing selected");

                DrawOverviewLane("Do Now", "Required missing or blocked work only.", model.DoNow);
                DrawOverviewLane("Proof Enhancers", "Helpful native setup once Do Now is clear. These should improve the first proof, not block it.", model.DoSoon);
                DrawOverviewLane("Feature Cards", "Optional next capabilities, polish, advanced systems, and setup that can safely wait.", model.Later);
            }

            EditorGUILayout.Space(12f);
            DrawCurrentStepPanel(currentStepSelection, currentStepReport);

            DrawFeatureAdvisor(GetSelectedSetupProfile(activeSetup, GetSelectedMode(activeSetup, GetSelectedSession(activeSetup, GetSelectedBootstrap(activeSetup)))));
        }

        private static void DrawOverviewGuidanceCard(PyralisAuthoringOverviewModel model, PyralisAuthoringRouteReport report)
        {
            if (model == null)
                return;

            EditorGUILayout.LabelField("Guidance", EditorStyles.miniBoldLabel);
            string guidance = report != null && !string.IsNullOrWhiteSpace(report.RouteGuidance)
                ? report.RouteGuidance
                : model.FirstProofGuidance;
            DrawSemanticHelpBox(guidance, model.ReadyToPressPlay ? MessageType.Info : MessageType.Warning);
            DrawMiniField("Next", model.BestNextAction);
            DrawMiniField("Proof Status", GetFlowTestStatus(model));
            DrawMiniField("First Proof", model.FirstProofLabel);
            DrawSemanticMiniLabel(model.FirstProofGuidance);
        }

        private void DrawOverviewActionButtons(PyralisAuthoringOverviewModel model)
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Map"))
                {
                    _mode = AuthoringWindowMode.Map;
                    _mapScroll = Vector2.zero;
                }

                if (GUILayout.Button("Open Validate"))
                {
                    _mode = AuthoringWindowMode.Validate;
                    _hygieneScroll = Vector2.zero;
                }

                Object bestTarget = GetBestOverviewTarget(model);
                using (new EditorGUI.DisabledScope(bestTarget == null))
                {
                    if (GUILayout.Button("Inspect Best Target"))
                        SelectAndPing(bestTarget);
                }
            }
        }

        private static Object GetBestOverviewTarget(PyralisAuthoringOverviewModel model)
        {
            if (model == null)
                return null;

            Object target = GetFirstTarget(model.DoNow);
            if (target != null)
                return target;

            target = GetFirstTarget(model.DoSoon);
            if (target != null)
                return target;

            return GetFirstTarget(model.Later);
        }

        private static Object GetFirstTarget(IReadOnlyList<PyralisAuthoringOverviewIssue> issues)
        {
            if (issues == null)
                return null;

            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i] != null && issues[i].Target != null)
                    return issues[i].Target;
            }

            return null;
        }

        private static string GetFlowTestStatus(PyralisAuthoringOverviewModel model)
        {
            if (model == null)
                return "Select an active setup before testing the flow.";

            if (model.DoNow.Count > 0)
                return "Not ready to test yet. Clear Do Now in Edit Mode first, then use Play Mode only as the first proof test.";

            if (model.DoSoon.Count > 0)
                return "Ready for a narrow Play Mode proof. Proof Enhancers can make the first test clearer, but setup edits still belong in Edit Mode.";

            return "Ready for first proof. Run the smallest route pass named below, verify one interaction in Play Mode, stop Play Mode, then add one feature at a time.";
        }

        private static void DrawFirstProofCard(PyralisAuthoringOverviewModel model)
        {
            if (model == null)
                return;

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("First Playable Proof", model.FirstProofLabel, EditorStyles.miniBoldLabel);
                DrawMiniField("Setup Surface", model.FirstProofSetupSurface);
                DrawMiniField("Success Looks Like", model.FirstProofSuccessCriteria);
                DrawMiniField("Proof Chain", model.FirstProofChainSummary);
                DrawMiniField("Defer Until After Proof", model.FirstProofDeferUntilAfter);
            }
        }

        private static void DrawPlayModeChecklist(PyralisAuthoringOverviewModel model)
        {
            if (model == null || model.PlayModeChecklist.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Play Mode Checklist", EditorStyles.miniBoldLabel);
                for (int i = 0; i < model.PlayModeChecklist.Count; i++)
                    DrawPlayModeChecklistItem(model.PlayModeChecklist[i]);
            }
        }

        private static void DrawPlayModeChecklistItem(PyralisAuthoringPlayModeChecklistItem item)
        {
            if (item == null)
                return;

            string status = item.Ready ? "Ready" : "Needs edit";
            EditorGUILayout.LabelField(item.Label, status, EditorStyles.miniBoldLabel);
            if (!string.IsNullOrWhiteSpace(item.Detail))
                DrawSemanticMiniLabel(item.Detail);
        }

        private static void DrawContractProofGuidance(Object activeSetup, PyralisAuthoringRouteReport report)
        {
            IReadOnlyList<PyralisAuthoringContractProofGuidanceRow> rows = PyralisAuthoringContractProofGuidance.Build(activeSetup, report);
            if (rows == null || rows.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Contract Proof Targets", EditorStyles.miniBoldLabel);
                DrawSemanticMiniLabel("Feature modules included in this setup can enhance the first proof, but Play Mode remains the proof pass.");
                EditorGUI.indentLevel++;
                for (int i = 0; i < rows.Count; i++)
                    DrawContractProofGuidanceRow(rows[i]);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawContractProofGuidanceRow(PyralisAuthoringContractProofGuidanceRow row)
        {
            if (row == null || row.Contract == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string proofLabel = row.ProofFact != null ? row.ProofFact.DisplayName : row.Contract.FirstProofTargetId;
                EditorGUILayout.LabelField(row.Contract.DisplayName, proofLabel, EditorStyles.boldLabel);
                DrawMiniField("Feature Module", row.Contract.StableId);
                DrawMiniField("Proof Target", string.IsNullOrWhiteSpace(row.Contract.FirstProofTargetId) ? "None recorded." : row.Contract.FirstProofTargetId);
                DrawMiniField("Proof Target Exists", row.ProofTargetExists ? "Yes - this contract maps to a route proof card." : "No - the contract points at a missing route proof card.");
                DrawMiniField("Proof Status", GetContractProofStatusText(row));

                if (row.ProofFact != null)
                {
                    DrawMiniField("Play Mode Proof", row.ProofFact.FirstProof);
                    DrawMiniList("Proof Setup Fields", row.ProofFact.AssignmentFields);
                }

                if (row.HasUnsupportedLaneCaution)
                    DrawMiniField("Unsupported Lane Cautions", GetUnsupportedLaneCaution(row));
                else
                    DrawMiniList("Unsupported Lane Cautions", ToPresentationModeNames(row.Contract.UnsupportedPresentationModes));
            }
        }

        private static string GetContractProofStatusText(PyralisAuthoringContractProofGuidanceRow row)
        {
            if (row == null)
                return "No proof guidance available.";

            switch (row.State)
            {
                case PyralisAuthoringContractProofState.ProofTargetMissing:
                    return "Blocked: proof target is missing from PyralisAuthoringRouteProof.";
                case PyralisAuthoringContractProofState.ProofBlockedBySetup:
                    return "Proof target exists, but route setup still has blockers. Clear Do Now before Play Mode.";
                default:
                    return "Proof not run in Play Mode. Enter Play only after required setup is clear, then verify this proof target.";
            }
        }

        private static string GetUnsupportedLaneCaution(PyralisAuthoringContractProofGuidanceRow row)
        {
            if (row == null || row.Contract == null || !row.ActiveLane.HasValue)
                return "No active lane caution.";

            if (!string.IsNullOrWhiteSpace(row.Contract.UnsupportedLaneMessage))
                return row.Contract.UnsupportedLaneMessage;

            return $"{row.Contract.DisplayName} does not support {row.ActiveLane.Value}. Choose a supported feature module or change the pawn presentation profile before Play Mode.";
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

        private string GetIntentModelKey()
        {
            return $"{_intentLane}_{_intentAxioms}_{_intentCapabilities}_{_authoringCacheVersion}";
        }

        private string[] GetIntentGoalNames()
        {
            if (_intentCapabilities == AuthoringCapability.None)
                return new[] { "No capabilities selected yet" };

            List<string> names = new List<string>();
            foreach (AuthoringCapability cap in AuthoringCapabilityRegistry.GetAllIndividualCapabilities())
            {
                if ((_intentCapabilities & cap) != 0)
                    names.Add(AuthoringCapabilityRegistry.GetDisplayName(cap));
            }
            return names.ToArray();
        }

        private void DrawHygieneIssues(IReadOnlyList<PyralisAuthoringIssue> issues)
        {
            if (issues == null || issues.Count == 0) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(new GUIContent("Code Hygiene & Duplication", "Warnings and errors related to the engine spine and capability registry."), EditorStyles.miniBoldLabel);
                foreach (var issue in issues)
                {
                    MessageType type = issue.Severity switch
                    {
                        PyralisAuthoringIssueSeverity.Bug => MessageType.Error,
                        PyralisAuthoringIssueSeverity.Required => MessageType.Warning,
                        _ => MessageType.Info
                    };
                    EditorGUILayout.HelpBox($"{issue.IssueCode}: {issue.Reason}", type);
                }
            }
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
                DrawFactSemanticTags(row.Fact);
                DrawMiniField("Why", row.Reason, "Why this row is currently visible for the selected project intent.");
                DrawMiniField("Priority", GetIntentTierLabel(row.Tier), "Guide priority. Numeric score is intentionally hidden by default so the user reads guidance, not a leaderboard.");
                DrawMiniField("First Proof", row.Fact.FirstProof, "The smallest native Unity proof this row is trying to help you reach.");

                if (!expanded)
                {
                    DrawMiniList("Customization", row.Fact.CustomizationMoments, "Creator-owned choices to make after the route skeleton is understood.", 2);
                    return;
                }

                DrawMiniField("What It Means", row.Fact.Summary, "The short descriptor provided by the reflective fact.");
                DrawMiniField("Route Relevance", row.Fact.RouteRelevance, "Why this fact matters to the route shape.");
                DrawMiniList("Supported Lanes", row.Fact.LaneTags, "Lanes where this fact is expected to fit cleanly.");
                DrawMiniList("Unsupported / Caution Lanes", row.Fact.UnsupportedLaneTags, "Lanes where this fact is usually not the clean fit.");
                DrawMiniList("Assignment Fields", row.Fact.AssignmentFields, "Unity fields or objects the creator may need to inspect or assign.");
                DrawMiniList("Customization", row.Fact.CustomizationMoments, "Creator-owned choices. Authoring guides these choices; it does not pick them.");
                DrawMiniList("Can Wait", row.Fact.CanWait, "Useful work to defer until the route's first proof is readable.");
            }
        }

        private static void DrawOverviewLane(string title, string description, IReadOnlyList<PyralisAuthoringOverviewIssue> issues)
        {
            EditorGUILayout.Space(6f);
            int issueCount = issues != null ? issues.Count : 0;
            EditorGUILayout.LabelField(title, GetLaneCountLabel(issueCount), EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedMiniLabel);
            if (issueCount == 0)
            {
                EditorGUILayout.LabelField(GetEmptyLaneText(title), EditorStyles.wordWrappedMiniLabel);
            }
            else
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < issueCount; i++)
                    DrawOverviewIssueCard(issues[i]);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawOverviewIssueCard(PyralisAuthoringOverviewIssue issue)
        {
            if (issue == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(issue.Label, GetStatusLabel(issue.Status), EditorStyles.boldLabel);
                DrawMiniField("Setup Role", GetWorkIntentLabel(issue.WorkIntent));
                DrawSemanticMiniLabel(issue.Message);
                if (!string.IsNullOrWhiteSpace(issue.NativeActionGuidance))
                {
                    EditorGUILayout.Space(2f);
                    DrawMiniField("Native Unity Action", issue.NativeActionGuidance);
                }

                DrawSemanticMiniLabel(issue.Evidence);

                using (new EditorGUI.DisabledScope(issue.Target == null))
                {
                    if (GUILayout.Button("Inspect Target"))
                    {
                        Selection.activeObject = issue.Target;
                        EditorGUIUtility.PingObject(issue.Target);
                    }
                }
            }
        }

        private static void DrawMiniField(string label, string value)
        {
            DrawMiniField(label, value, string.Empty);
        }

        private static void DrawMiniField(string label, string value, string tooltip)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            EditorGUILayout.LabelField(new GUIContent(label, tooltip ?? string.Empty), EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            DrawSemanticMiniLabel(value);
            EditorGUI.indentLevel--;
        }

        private static void DrawSemanticHelpBox(string message, MessageType type)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (type != MessageType.None)
                    EditorGUILayout.LabelField(type.ToString(), EditorStyles.miniBoldLabel);

                DrawSemanticMiniLabel(message);
            }
        }

        private static void DrawSemanticMiniLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            EditorGUILayout.LabelField(ColorizeSemanticTokens(value), GetSemanticMiniLabelStyle());
        }

        private static GUIStyle GetSemanticMiniLabelStyle()
        {
            return new GUIStyle(EditorStyles.wordWrappedMiniLabel)
            {
                richText = true
            };
        }

        private static string ColorizeSemanticTokens(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            System.Text.StringBuilder builder = new System.Text.StringBuilder(value.Length + 32);
            int index = 0;
            while (index < value.Length)
            {
                if (TryGetSemanticTokenAt(value, index, out SemanticTokenRule rule, out int length))
                {
                    string color = ColorUtility.ToHtmlStringRGB(PyralisAuthoringLabelUtility.GetSemanticTagColor(rule.Tag));
                    builder.Append("<color=#");
                    builder.Append(color);
                    builder.Append(">");
                    AppendEscapedRichText(builder, value, index, length);
                    builder.Append("</color>");
                    index += length;
                    continue;
                }

                AppendEscapedRichText(builder, value[index]);
                index++;
            }

            return builder.ToString();
        }

        private static bool TryGetSemanticTokenAt(string value, int index, out SemanticTokenRule match, out int length)
        {
            match = default(SemanticTokenRule);
            length = 0;

            for (int i = 0; i < SemanticTokenRules.Length; i++)
            {
                SemanticTokenRule rule = SemanticTokenRules[i];
                if (string.IsNullOrEmpty(rule.Token) || index + rule.Token.Length > value.Length)
                    continue;

                if (!IsSemanticTokenBoundary(value, index - 1) || !IsSemanticTokenBoundary(value, index + rule.Token.Length))
                    continue;

                if (string.Compare(value, index, rule.Token, 0, rule.Token.Length, System.StringComparison.OrdinalIgnoreCase) != 0)
                    continue;

                match = rule;
                length = rule.Token.Length;
                return true;
            }

            return false;
        }

        private static bool IsSemanticTokenBoundary(string value, int index)
        {
            if (index < 0 || index >= value.Length)
                return true;

            char character = value[index];
            return !char.IsLetterOrDigit(character) && character != '_';
        }

        private static void AppendEscapedRichText(System.Text.StringBuilder builder, string value, int start, int length)
        {
            for (int i = 0; i < length; i++)
                AppendEscapedRichText(builder, value[start + i]);
        }

        private static void AppendEscapedRichText(System.Text.StringBuilder builder, char character)
        {
            builder.Append(character);
        }

        private static string GetLaneCountLabel(int count)
        {
            return count == 1 ? "1 item" : count + " items";
        }

        private static string GetEmptyLaneText(string title)
        {
            switch (title)
            {
                case "Do Now":
                    return "No blockers in this lane.";
                case "Proof Enhancers":
                    return "No route-specific proof helpers are asking for attention right now.";
                case "Feature Cards":
                    return "No optional feature work is competing with this proof.";
                default:
                    return "Nothing in this lane.";
            }
        }

        private static string GetStatusLabel(PyralisSetupFlowStepStatus status)
        {
            switch (status)
            {
                case PyralisSetupFlowStepStatus.Missing:
                    return "Needs setup";
                case PyralisSetupFlowStepStatus.Blocked:
                    return "Blocked";
                case PyralisSetupFlowStepStatus.Recommended:
                    return "Recommended";
                case PyralisSetupFlowStepStatus.Optional:
                    return "Optional";
                default:
                    return "Ready";
            }
        }

        private static string GetWorkIntentLabel(PyralisSetupFlowWorkIntent workIntent)
        {
            switch (workIntent)
            {
                case PyralisSetupFlowWorkIntent.Foundation:
                    return "Foundation setup";
                case PyralisSetupFlowWorkIntent.ProofEnhancer:
                    return "Proof enhancer";
                case PyralisSetupFlowWorkIntent.FeatureCard:
                    return "Feature card";
                default:
                    return "Required setup";
            }
        }

        private static void DrawReadinessSummary(Object selection)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Readiness Summary", EditorStyles.boldLabel);

            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(selection);
            SessionDefinition session = GetSelectedSession(selection, bootstrap);
            GameModeDefinition mode = GetSelectedMode(selection, session);
            GameSetupProfile setupProfile = GetSelectedSetupProfile(selection, mode);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PyralisSetupFlowReport flowReport = bootstrap != null ? PyralisSetupFlowValidator.BuildReport(bootstrap) : null;
                DrawCompactReadinessRow("Scene Root", bootstrap != null, false, bootstrap);
                DrawCompactReadinessRow("Session", session != null, false, session);
                DrawCompactReadinessRow("Game Rules", mode != null, false, mode);
                DrawCompactReadinessRow("Setup Recipe", setupProfile != null, false, setupProfile);
                DrawCompactReadinessRow("Capability Patterns", GetStepReady(flowReport, "Add Runtime Patterns", setupProfile != null && setupProfile.runtimePatterns != null && setupProfile.runtimePatterns.Length > 0), false, setupProfile, GetStepMessage(flowReport, "Add Runtime Patterns"));
                DrawCompactReadinessRow("Players / Seats", GetStepReady(flowReport, "Assign Default Participants", session != null && session.defaultParticipants != null && session.defaultParticipants.Length > 0), false, session, GetStepMessage(flowReport, "Assign Default Participants"));
                DrawCompactReadinessRow("Pawn / No Pawn", GetStepReady(flowReport, "Assign Participant Pawn", HasAnyPawn(session)), true, session, GetStepMessage(flowReport, "Assign Participant Pawn"));
                DrawCompactReadinessRow("Scene Roots", GetStepReady(flowReport, "Scene And Prefab Readiness", bootstrap != null), true, bootstrap, GetStepMessage(flowReport, "Scene And Prefab Readiness"));
            }
        }

        private static void DrawSceneSurfaceSnapshot(Object activeSetup)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Scene Surface Scan", EditorStyles.boldLabel);
            DrawSemanticHelpBox("This reads ordinary Unity scene objects too. A found surface is evidence, not proof: Play Mode still owns the final route proof.", MessageType.Info);

            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(activeSetup);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                for (int i = 0; i < snapshot.Rows.Count; i++)
                    DrawSceneSurfaceRow(snapshot.Rows[i]);
            }
        }

        private static void DrawSceneSurfaceRow(PyralisAuthoringSceneSurfaceRow row)
        {
            if (row == null)
                return;

            string status = GetSceneSurfaceStatus(row);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(row.Surface, status, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                DrawMiniField("Evidence", row.Current);
                DrawMiniField("Next fix", row.NextFix);
                EditorGUI.indentLevel--;
            }
        }

        private static string GetSceneSurfaceStatus(PyralisAuthoringSceneSurfaceRow row)
        {
            return $"[{PyralisAuthoringLabelUtility.GetEvidenceLabel(row.EvidenceState)}]";
        }

        private static void DrawYouAreHereChain(Object activeSetup)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("You Are Here", EditorStyles.boldLabel);

            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(activeSetup);
            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(activeSetup);
            PyralisAuthoringSceneSurfaceSnapshot surfaces = PyralisAuthoringSceneSurfaceSnapshot.Build(activeSetup);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawSetupChainRow("Scene Root", bootstrap, bootstrap != null, false, "Scene object that starts the session.");
                DrawSetupChainRow("Session", route.Session, route.Session != null, false, "Asset that names game rules and participants.");
                DrawSetupChainRow("Game Rules", route.Mode, route.Mode != null, false, "Ruleset that chooses the setup recipe.");
                DrawSetupChainRow("Setup Recipe", route.SetupProfile, route.SetupProfile != null, false, "Capability recipe for this route.");
                DrawSetupChainRow("Capabilities", route.SetupProfile, route.HasValidPatterns, false, GetCapabilityChainMessage(route));
                DrawSetupChainRow("Participants", route.Session, route.HasParticipants, false, route.HasParticipants ? "Players, seats, hands, factions, or command owners are assigned." : "Assign at least one default participant.");
                DrawSetupChainRow("Pawn / No Pawn", GetFirstParticipant(route.Session), GetPawnChainReady(route), !route.RequiresPawn, GetPawnChainMessage(route));
                DrawSetupChainRow("Scene Surfaces", bootstrap, GetRecommendedSceneSurfacesReady(surfaces), false, GetSceneSurfaceChainMessage(surfaces));
            }
        }

        private static void DrawSetupChainRow(string label, Object target, bool isReady, bool isOptional, string message)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string status = GetReadinessBadge(isReady, target, isOptional);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
                    using (new EditorGUI.DisabledScope(target == null))
                    {
                        if (GUILayout.Button("Inspect", GUILayout.Width(72f)))
                        {
                            Selection.activeObject = target;
                            EditorGUIUtility.PingObject(target);
                        }
                    }
                }

                EditorGUI.indentLevel++;
                DrawSemanticMiniLabel($"{status}: {message}");
                EditorGUI.indentLevel--;
            }
        }

        private static string GetCapabilityChainMessage(PyralisAuthoringRouteDescriptor route)
        {
            if (route.SetupProfile == null)
                return "Create or assign the setup recipe before choosing capabilities.";

            if (!route.HasAssignedPatterns)
                return "Choose capability patterns before scene wiring.";

            if (!route.HasValidPatterns)
                return "Fix runtime pattern validation before trusting route guidance.";

            return route.RouteName;
        }

        private static bool GetPawnChainReady(PyralisAuthoringRouteDescriptor route)
        {
            if (!route.RequiresPawn)
                return true;

            return route.HasParticipants && string.IsNullOrWhiteSpace(route.ParticipantPawnIssue);
        }

        private static string GetPawnChainMessage(PyralisAuthoringRouteDescriptor route)
        {
            if (!route.RequiresPawn)
                return "No-pawn route: empty PawnDefinition fields are correct unless you intentionally add actor bodies.";

            if (string.IsNullOrWhiteSpace(route.ParticipantPawnIssue))
                return "Pawn-backed route has participant pawn setup.";

            return route.ParticipantPawnIssue;
        }

        private static Object GetFirstParticipant(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return session;

            return session.defaultParticipants[0] != null ? session.defaultParticipants[0] : session;
        }

        private static bool GetRecommendedSceneSurfacesReady(PyralisAuthoringSceneSurfaceSnapshot snapshot)
        {
            if (snapshot == null || snapshot.Rows.Count == 0)
                return true;

            for (int i = 0; i < snapshot.Rows.Count; i++)
            {
                PyralisAuthoringSceneSurfaceRow row = snapshot.Rows[i];
                if (row != null && !row.SupportsFirstProofAttempt)
                    return false;
            }

            return true;
        }

        private static string GetSceneSurfaceChainMessage(PyralisAuthoringSceneSurfaceSnapshot snapshot)
        {
            if (snapshot == null)
                return "Scene surface scan is unavailable.";

            int missing = 0;
            List<string> missingSurfaces = new List<string>();
            for (int i = 0; i < snapshot.Rows.Count; i++)
            {
                PyralisAuthoringSceneSurfaceRow row = snapshot.Rows[i];
                if (row != null && !row.SupportsFirstProofAttempt)
                {
                    missing++;
                    if (!string.IsNullOrWhiteSpace(row.Surface))
                        missingSurfaces.Add(row.Surface);
                }
            }

            if (missing == 0)
                return "Route-recommended scene surface evidence is present or not needed yet. Play Mode still proves behavior.";

            string missingText = string.Join(", ", missingSurfaces);
            return $"{missing} proof enhancer scene surface(s) are not detected yet: {missingText}. Scroll to Scene Surface Scan or Overview feature cards, then create the needed object through the native Hierarchy, Project, or Inspector path.";
        }

        private static void DrawCompactReadinessRow(string label, bool isReady, bool isOptional, Object target = null, string message = null)
        {
            string targetName = target != null ? $" ({target.name})" : string.Empty;
            EditorGUILayout.LabelField(label, GetReadinessBadge(isReady, target, isOptional) + targetName);
            if (!string.IsNullOrWhiteSpace(message))
            {
                EditorGUI.indentLevel++;
                DrawSemanticMiniLabel(message);
                EditorGUI.indentLevel--;
            }
        }

        private static bool GetStepReady(PyralisSetupFlowReport report, string label, bool fallback)
        {
            PyralisSetupFlowStep step = report != null ? report.GetStep(label) : null;
            return step != null ? step.Status == PyralisSetupFlowStepStatus.Ready || step.Status == PyralisSetupFlowStepStatus.Optional : fallback;
        }

        private static string GetStepMessage(PyralisSetupFlowReport report, string label)
        {
            PyralisSetupFlowStep step = report != null ? report.GetStep(label) : null;
            return step != null ? step.Message : null;
        }

        private void DrawGuideMode(Object selection, PyralisAuthoringRouteReport report, Object activeSetup, PyralisAuthoringRouteReport activeSetupReport)
        {
            if (ShouldShowSelectionFirstGuide(selection, activeSetup))
            {
                EditorGUILayout.LabelField("Selected Object Next Step", EditorStyles.boldLabel);
                DrawCurrentStepPanel(selection, report);

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("What This Selection Does", EditorStyles.boldLabel);
                DrawSelectedContext(selection, report);
                DrawSelectionGuide(selection, report);

                EditorGUILayout.Space(10f);
                DrawCurrentIntentGuide(GetCachedIntentModel());
                
                DrawReflectiveContracts(activeSetup);
            }
            else
            {
                DrawCurrentIntentGuide(GetCachedIntentModel());
                
                DrawReflectiveContracts(activeSetup);

                EditorGUILayout.Space(10f);
                EditorGUILayout.LabelField("What This Selection Does", EditorStyles.boldLabel);
                DrawSelectedContext(selection, report);
                DrawSelectionGuide(selection, report);
            }

            if (activeSetup != null && activeSetup != selection)
            {
                EditorGUILayout.Space(12f);
                EditorGUILayout.LabelField("Steady Setup Context", EditorStyles.boldLabel);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Active Setup", $"{activeSetup.name} ({activeSetup.GetType().Name})", EditorStyles.wordWrappedLabel);
                    if (activeSetupReport != null)
                    {
                        EditorGUILayout.LabelField("Route", activeSetupReport.RouteName, EditorStyles.wordWrappedLabel);
                        EditorGUILayout.LabelField("Next Required Step", activeSetupReport.NextStep, EditorStyles.wordWrappedLabel);
                    }
                }
            }
        }

        private static bool ShouldShowSelectionFirstGuide(Object selection, Object activeSetup)
        {
            return activeSetup == null
                && selection is GameObject selectedGameObject
                && selectedGameObject.GetComponent<GameplaySessionBootstrap>() == null;
        }

        private static void DrawCurrentStepPanel(Object selection, PyralisAuthoringRouteReport report)
        {
            if (report == null)
                return;

            EditorGUILayout.LabelField("Current Step", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(report.RouteName, EditorStyles.miniBoldLabel);
                DrawSemanticHelpBox(report.NextStep, report.ValidationIssues.Count > 0 ? MessageType.Warning : MessageType.Info);

                EditorGUILayout.LabelField("Primary Action", EditorStyles.miniBoldLabel);
                DrawPrimaryAction(selection, report);

                string key = "Pyralis.AuthoringWindow.CurrentStep.Why";
                bool isOpen = ServiceStepFoldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, "Why This Matters", true);
                ServiceStepFoldouts[key] = isOpen;

                if (isOpen)
                    DrawSemanticMiniLabel(report.RouteGuidance);
            }
        }

        private static void DrawCurrentIntentGuide(PyralisAuthoringIntentModel model)
        {
            EditorGUILayout.LabelField("Current Intent Guide", EditorStyles.boldLabel);
            DrawSemanticHelpBox(
                "Ranked cookbook cards for the selected Intent. Use these to decide what to create, inspect, customize, or defer. Facts remains the full dictionary outside the current intent.",
                MessageType.Info);

            if (model == null)
            {
                EditorGUILayout.LabelField("No intent model is available yet.", EditorStyles.wordWrappedMiniLabel);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawMiniField("Intent Summary", model.Summary);
                DrawMiniField("Matched Intent Families", model.MatchingIntents != null && model.MatchingIntents.Count > 0
                    ? JoinFactDisplayNames(model.MatchingIntents)
                    : "No named family matched yet. Toggle intent controls to shape the guide.");
            }

            DrawIntentRows(
                "Recommended Cards",
                "Highest-ranked facts and capabilities for the current intent. Start at the top unless Overview reports a blocking setup issue.",
                model.Recommendations,
                "Cards are sorted by lane, goals, related route intent, and caution fit.");

            DrawIntentRows(
                "Caution Cards",
                "Useful facts that are not a clean fit for the selected lane. Keep them visible as tradeoffs, not primary steps.",
                model.Cautions,
                "Cautions help prevent pawn, combat, UI, board, or networking assumptions from leaking into the wrong route.");
        }

        private void DrawReflectiveContracts(Object activeSetup)
        {
            if (activeSetup == null) return;
            
            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(activeSetup);
            if (bootstrap == null) return;

            PyralisSetupFlowReport flowReport = PyralisSetupFlowValidator.BuildReport(bootstrap);
            if (flowReport == null || flowReport.Steps.Count == 0) return;

            // Filter for reflective steps (StepId == Unknown)
            List<PyralisSetupFlowStep> reflectiveSteps = new List<PyralisSetupFlowStep>();
            foreach (var step in flowReport.Steps)
            {
                if (step.StepId == PyralisSetupFlowStepId.Unknown)
                {
                    reflectiveSteps.Add(step);
                }
            }

            if (reflectiveSteps.Count == 0) return;

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Reflective Design Contracts", EditorStyles.boldLabel);
            DrawSemanticHelpBox("These contracts are discovered reflectively from feature code and attributes. They ensure the scene state matches the design intent.", MessageType.Info);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                foreach (var step in reflectiveSteps)
                {
                    DrawReflectiveContractRow(step);
                }
            }
        }

        private static void DrawReflectiveContractRow(PyralisSetupFlowStep step)
        {
            MessageType msgType = step.Status switch
            {
                PyralisSetupFlowStepStatus.Ready => MessageType.Info,
                PyralisSetupFlowStepStatus.Missing => MessageType.Warning,
                PyralisSetupFlowStepStatus.Blocked => MessageType.Error,
                _ => MessageType.None
            };

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    string statusPrefix = step.Status == PyralisSetupFlowStepStatus.Ready ? "✔" : "❌";
                    EditorGUILayout.LabelField($"{statusPrefix} {step.Label}", EditorStyles.boldLabel);
                    
                    if (step.ReferencedObject != null)
                    {
                        if (GUILayout.Button("Ping", GUILayout.Width(44f)))
                            EditorGUIUtility.PingObject(step.ReferencedObject);
                        
                        if (GUILayout.Button("Select", GUILayout.Width(56f)))
                            Selection.activeObject = step.ReferencedObject;
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(step.Message))
                {
                    EditorGUILayout.HelpBox(step.Message, msgType);
                }
            }
        }

        private static int GetIntentGuideCardCount(PyralisAuthoringIntentModel model)
        {
            if (model == null)
                return 0;

            int count = 0;
            if (model.Recommendations != null)
                count += model.Recommendations.Count;
            if (model.Cautions != null)
                count += model.Cautions.Count;
            return count;
        }

        private static string JoinFactDisplayNames(IReadOnlyList<PyralisAuthoringFact> facts)
        {
            if (facts == null || facts.Count == 0)
                return string.Empty;

            List<string> names = new List<string>();
            for (int i = 0; i < facts.Count; i++)
            {
                if (facts[i] != null && !string.IsNullOrWhiteSpace(facts[i].DisplayName))
                    names.Add(facts[i].DisplayName);
            }

            return names.Count > 0 ? string.Join(", ", names) : string.Empty;
        }

        private static void DrawPrimaryAction(Object selection, PyralisAuthoringRouteReport report)
        {
            if (selection == null)
            {
                DrawSemanticHelpBox(
                    "Native first step: right-click in Hierarchy, choose Create Empty, name it Gameplay Root, then select it and use Inspector -> Add Component search for GameplaySessionBootstrap. Keep this window open while you do it.",
                    MessageType.Info);
                DrawSemanticMiniLabel(
                    "After that, select Gameplay Root so Overview can switch from route discovery to the setup map: SessionDefinition, participants, pawn prefab, spawn points, input, and camera bounds.");
                return;
            }

            if (selection is GameObject selectedGameObject && selectedGameObject.GetComponent<GameplaySessionBootstrap>() == null)
            {
                if (IsSceneSupportObject(selectedGameObject))
                {
                    DrawSemanticHelpBox(
                        $"Native path: keep `{selectedGameObject.name}` as scene support. In the Hierarchy, right-click -> Create Empty, name it Gameplay Root, then select Gameplay Root and use Inspector -> Add Component search for GameplaySessionBootstrap.",
                        MessageType.Info);
                    DrawSemanticMiniLabel(
                        "After Gameplay Root exists, return to camera, art, lights, and playfield objects as guided setup steps. The camera route should be wired through Camera Root with CinemachineCameraRigController, not by deleting Main Camera or turning it into the session root.");
                    return;
                }

                DrawSemanticHelpBox(
                    $"Native path: keep `{selectedGameObject.name}` selected, then use Inspector -> Add Component search for GameplaySessionBootstrap. Add PyralisGameplayLifetimeScope next if you want the composition root visible before Play Mode.",
                    MessageType.Info);
                DrawSemanticMiniLabel(
                    "This is still before asset wiring. Once GameplaySessionBootstrap is on the object, Overview will promote it to the active setup and guide SessionDefinition, participants, pawn prefab, spawn points, input, and camera bounds.");
                return;
            }

            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(selection);
            SessionDefinition session = GetSelectedSession(selection, bootstrap);
            GameModeDefinition mode = GetSelectedMode(selection, session);
            GameSetupProfile setupProfile = GetSelectedSetupProfile(selection, mode);
            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(setupProfile, session, mode);

            if (bootstrap != null && session == null)
            {
                DrawSemanticHelpBox(
                    "Native path: in the Project window, choose or create a project-owned setup folder for this proof, separate from imported art folders, then right-click in that folder and choose Create -> NeonBlack -> Definitions -> Session Definition. Drag that asset into GameplaySessionBootstrap > Session Definition in the Inspector, or click the field's object picker circle and double-click the asset.",
                    MessageType.Info);
                DrawSemanticMiniLabel(
                    "This keeps folderbase and assignment ownership visible: check the Project content pane/breadcrumb first, because Unity creates the asset in the active Project folder. Keep Jim/imported art in its art folder, keep Pyralis setup definitions/profiles in the proof setup folder, and use the Inspector to show exactly which field owns the link.");
                return;
            }

            if (session != null && mode == null)
            {
                DrawSemanticHelpBox(
                    "Native path: in the Project window, create a Game Mode Definition asset. Then select/open the SessionDefinition asset and assign its Default Game Mode field by dragging the asset there or using the field's object picker circle.",
                    MessageType.Info);
                DrawSemanticMiniLabel(
                    "The Authoring Window explains the next link; the SessionDefinition Inspector remains the field-level source of truth.");
                return;
            }

            if (mode != null && setupProfile == null)
            {
                DrawSemanticHelpBox(
                    "Native path: in the Project window, create a Game Setup Profile asset. Then select/open the GameModeDefinition asset and assign its Setup Profile field by dragging the asset there or using the field's object picker circle.",
                    MessageType.Info);
                DrawSemanticMiniLabel(
                    "Create or choose the setup recipe intentionally, then wire it through the GameModeDefinition Inspector.");
                return;
            }

            if (setupProfile != null && !route.HasAssignedPatterns)
            {
                DrawSemanticHelpBox(
                    "Native path: select/open the GameSetupProfile asset, use Runtime Capabilities -> Capability To Add to choose the route family, then click Add Capability. Runtime Patterns is the resolved recipe output; create a new Runtime Pattern Definition only when the existing capability language cannot describe the route.",
                    MessageType.Info);
                DrawSemanticMiniLabel(
                    "Runtime capabilities name the route intent before participant and pawn wiring becomes meaningful. For a 1P movement proof, start with Character Pawn Gameplay.");
                return;
            }

            if (setupProfile != null && !route.HasValidPatterns)
            {
                DrawSemanticHelpBox(
                    "Native path: select the assigned Runtime Pattern Definition and clear its Inspector validation issues before adding participants. Fill Pattern Id, Display Name, Description, Setup Notes, Capability Family, Participant Embodiment, and Supported Control Surfaces.",
                    MessageType.Warning);
                DrawSemanticMiniLabel(
                    "A pattern slot is assigned, but Pyralis cannot trust it as the route source of truth until its metadata is real. Do this in the pattern Inspector, then return to the setup root.");
                return;
            }

            if (session != null && (session.defaultParticipants == null || session.defaultParticipants.Length == 0))
            {
                DrawSemanticHelpBox(
                    "Native path: create a Participant Definition asset in the Project window, configure player/input fields in its Inspector, then select/open the SessionDefinition asset, add a Default Participants slot, and drag the asset there or use the slot's object picker circle.",
                    MessageType.Info);
                DrawSemanticMiniLabel(
                    "Participants are design-owned. Use the Inspector so player, seat, input, hand, faction, camera, cursor, or pawn intent stays explicit.");
                return;
            }

            if (route.RequiresPawn && !string.IsNullOrWhiteSpace(route.ParticipantPawnIssue))
            {
                DrawSemanticHelpBox(GetPawnIssuePrimaryAction(route.ParticipantPawnIssue), MessageType.Info);
                DrawSemanticMiniLabel(
                    "The setup root can still see pawn-required routes. Select the participant, pawn definition, or pawn prefab named in Current Step when you need the exact Inspector field.");
                return;
            }

            if (report.NextStep.Contains("Spawn Points"))
            {
                DrawSemanticHelpBox(
                    "Native path: select Gameplay Root, right-click -> Create Empty, name it SpawnPoint_1, position it, click + on GameplaySessionBootstrap > Spawn Points, then drag SpawnPoint_1 into the new Transform slot.",
                    MessageType.Info);
                DrawSemanticMiniLabel(
                    "Unity list fields usually need an element slot before a drag can land. The guide should keep this explicit so beginners do not bounce off an empty list.");
                return;
            }

            if (report.NextStep.Contains("PlayerInputManager") || report.NextStep.Contains("Player Input Manager"))
            {
                DrawSemanticHelpBox(
                    "Native path: only add Unity PlayerInputManager for local join. For a 1P proof, select/open the SessionDefinition asset, set Max Participants to 1, and leave Bootstrap > Player Input Manager empty. For local join, add PlayerInputManager, assign a dedicated PlayerInput prefab, configure Join Behavior/Input Actions, then drag it into Bootstrap > Player Input Manager.",
                    MessageType.Warning);
                DrawSemanticMiniLabel(
                    "PlayerInputManager is not the pawn spawner for ordinary 1P proofs. It is a local-join input surface, and Unity requires a Player Prefab when joining is enabled.");
                return;
            }

            if (report.NextStep.Contains("Camera Root") || report.NextStep.Contains("Camera Rig"))
            {
                DrawSemanticHelpBox(
                    "Native path:\n1. Keep or create exactly one enabled physical Unity Camera for this shared proof, usually the default Main Camera.\n2. Hierarchy right-click -> Create Empty, name it Camera Root.\n3. Add CinemachineCameraRigController.\n4. Create GameObject -> Cinemachine -> Cinemachine Camera; this creates a separate Cinemachine Camera GameObject and usually adds Cinemachine Brain to the physical Main Camera.\n5. Verify Main Camera still has the MainCamera tag and Cinemachine Brain. Add the Brain manually only if it is missing.\n6. Assign the Cinemachine Camera component to Shared Camera Behaviour and the physical Main Camera to Target Camera.\n7. Disable or remove accidental extra physical Camera objects only when they were created by mistake; keep intentional overlay, split-screen, minimap, or render-texture cameras.\n8. Drag Camera Root into GameplaySessionBootstrap > Camera Rig Controller.",
                    MessageType.Warning);
                DrawSemanticMiniLabel(
                    "For a 2D proof, also set the physical Target Camera > Camera > Projection to Orthographic or use an orthographic CameraRigProfile. This is setup, not polish: Pawn2DMovementComponent needs usable camera bounds before it can move reliably.");
                return;
            }

            if (report.NextStep.Contains("Play Mode"))
            {
                DrawSemanticHelpBox(
                    "Native path: switch to the Game tab, press Play, then use the route input to confirm one pawn spawns, receives input, and moves. If the pawn cannot move or the framing is poor in a 2D proof, return to Edit Mode and make the Target Camera orthographic or assign an orthographic CameraRigProfile on Camera Root.",
                    MessageType.Info);
                DrawSemanticMiniLabel(
                    "Play Mode is the proof, not the setup tool. If a recommended row still affects what you are testing, wire it first.");
                return;
            }

            if (selection is ParticipantDefinition participant && participant.defaultPawn == null)
            {
                if (route.RequiresPawn)
                {
                    DrawSemanticHelpBox(
                        "Native path: create a Pawn Definition asset in the Project window, create or choose a pawn prefab, then assign the Pawn Definition into ParticipantDefinition > Default Pawn by dragging it there or using the field's object picker circle.",
                        MessageType.Info);
                    DrawSemanticMiniLabel(
                        "Keep pawn definition, prefab, art, movement, and presentation choices explicit in Unity instead of letting the guide silently pick defaults.");
                }
                else
                {
                    DrawSemanticMiniLabel("Pawn is optional for this route; leave it empty for seats, hands, factions, camera, cursor, menu, or board-driven participants.");
                }

                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(selection == null))
                {
                    if (GUILayout.Button("Inspect Selection"))
                    {
                        Selection.activeObject = selection;
                        EditorGUIUtility.PingObject(selection);
                    }
                }
            }

            DrawSemanticMiniLabel(report.ValidationIssues.Count > 0 ? "Review validation issues next." : "No one-click setup action is needed for this selection.");
        }

        private static string GetPawnIssuePrimaryAction(string participantPawnIssue)
        {
            if (string.IsNullOrWhiteSpace(participantPawnIssue))
                return "Native path: inspect the pawn route item named in Current Step and clear its Inspector validation issue.";

            if (participantPawnIssue.Contains("needs a PawnDefinition"))
            {
                return "Native path: create a Pawn Definition asset in the Project window, create or choose a pawn prefab, then assign the Pawn Definition to the participant named in Current Step.";
            }

            if (participantPawnIssue.Contains("needs a pawn prefab"))
            {
                return "Native path: create or choose a pawn prefab, add PawnRoot and the movement/presentation components it needs, then assign that prefab to PawnDefinition > Pawn Prefab.";
            }

            if (participantPawnIssue.Contains("needs PawnRoot"))
            {
                return "Native path: select the pawn prefab named in Current Step and use Inspector -> Add Component to add PawnRoot on the root GameObject.";
            }

            if (participantPawnIssue.Contains("needs a component that implements IPawnMotor"))
            {
                return "Native path: select the pawn prefab named in Current Step and use Inspector -> Add Component to add a movement motor component that implements IPawnMotor.";
            }

            if (participantPawnIssue.Contains("needs a component that implements IPawnPresentationModule"))
            {
                return "Native path: select the pawn prefab named in Current Step and use Inspector -> Add Component to add a presentation component that implements IPawnPresentationModule. For 2D visuals, drag the sprite or Aseprite asset from the Project window onto SpriteRenderer > Sprite instead of relying only on the object picker search.";
            }

            if (participantPawnIssue.Contains("needs a component that implements IPawnInputModule"))
            {
                return "Native path: select the pawn prefab named in Current Step and use Inspector -> Add Component to add the input adapter for the lane, such as Motor2DInputAdapter for a 2D pawn, so InputProfile actions reach movement.";
            }

            if (participantPawnIssue.Contains("extra PlayerInputHandler"))
            {
                return "Native path: open the pawn prefab named in Current Step, keep Motor2DInputAdapter as the supported 2D input bridge, remove the duplicate 2D Player Input Handler component, then return here and re-check the movement proof.";
            }

            return "Native path: inspect the pawn route item named in Current Step and clear its Inspector validation issue before entering Play Mode.";
        }

        private static void DrawMapMode(Object activeSetup, Object selection, PyralisAuthoringRouteReport report)
        {
            EditorGUILayout.LabelField("Setup Map", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this page to understand how the active setup is connected. Edit actual fields in the Inspector when a row names a missing link.", MessageType.Info);
            DrawActiveAndSelectedContext(activeSetup, selection);
            DrawYouAreHereChain(activeSetup);
            DrawSetupChain(activeSetup, report, false);
            DrawSceneSurfaceSnapshot(activeSetup);
            DrawReadinessSummary(activeSetup);
        }

        private static void DrawActiveAndSelectedContext(Object activeSetup, Object selection)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField(SelectedAuthoringContextLabel, EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", activeSetup != null ? $"{activeSetup.name} ({activeSetup.GetType().Name})" : "No setup context", EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Current Selection", selection != null ? $"{selection.name} ({selection.GetType().Name})" : "Nothing selected", EditorStyles.wordWrappedLabel);
            }
        }

        private static void DrawSelectionGuide(Object selection, PyralisAuthoringRouteReport report)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Important Values", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(GetImportantValuesText(selection), EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("What To Check First", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(report.NextStep, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Runtime Meaning", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(report.RouteGuidance, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static string GetImportantValuesText(Object selection)
        {
            return selection switch
            {
                GameplaySessionBootstrap => "Session reference, spawn points, and scene-level setup helpers.",
                SessionDefinition => "Game Rules, players/seats, local/network mode, and participant limits.",
                GameModeDefinition => "Setup Recipe plus rule-level defaults for the playable loop.",
                GameSetupProfile => "Capability Patterns that describe what this game route needs before scene wiring starts.",
                RuntimePatternDefinition => "Capability family, participant embodiment, presentation lanes, first-proof requirements, supported control surfaces, required systems, and setup notes.",
                ParticipantDefinition => "Display name, seat index, input ownership, and optional Pawn Actor.",
                PawnDefinition => "Pawn prefab, profiles, feature modules, and presentation setup.",
                Component component => component is PawnRoot
                    ? "PawnRoot marks the prefab root that Pyralis treats as a pawn actor."
                    : "This component participates in the selected GameObject's runtime behavior. Use the Inspector for field values and this window for setup meaning.",
                GameObject => "Pyralis components on this object and the likely authoring root for setup.",
                null => "Select a Pyralis setup asset, scene root, pawn prefab, or component to see its authoring meaning.",
                _ => "Use this asset's Inspector for fields, and use this page to understand how it fits into setup."
            };
        }

        private void DrawValidateMode(Object activeSetup, PyralisAuthoringRouteReport report)
        {
            EditorGUILayout.LabelField("Validate Active Setup", EditorStyles.boldLabel);

            if (activeSetup == null)
            {
                EditorGUILayout.HelpBox("Select a Bootstrap, Session, Game Mode, Setup Profile, Participant, Pawn, Runtime Pattern, or Feature Module asset to validate it here.", MessageType.Info);
                return;
            }

            PyralisAuthoringValidationModel model = PyralisAuthoringValidationModel.Build(activeSetup, report);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", activeSetup.name);
                EditorGUILayout.LabelField("Route", model.RouteName);
                EditorGUILayout.LabelField("Next Step", model.NextStep, EditorStyles.wordWrappedLabel);
                DrawValidateReadinessBuckets(activeSetup);
            }

            if (!model.HasIssues)
            {
                EditorGUILayout.HelpBox("No validation issues found for the selected item.", MessageType.Info);
                return;
            }

            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.SessionSetup, model.Issues);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.GameRules, model.Issues);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.SetupRecipe, model.Issues);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.PlayersSeats, model.Issues);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.PawnsActors, model.Issues);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.SceneObjects, model.Issues);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.CodeContract, model.Issues);
            DrawValidationIssueGroup(PyralisAuthoringValidationCategory.Other, model.Issues);
}

        private static void DrawValidateReadinessBuckets(Object activeSetup)
        {
            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(activeSetup);
            if (bootstrap == null)
                return;

            PyralisSceneReadinessReport readiness = PyralisSceneReadinessValidator.BuildReport(bootstrap);
            if (readiness == null || readiness.Issues.Count == 0)
                return;

            DrawReadinessBucket("Required Before Play", readiness.GetIssues(PyralisSceneReadinessSeverity.RequiredBeforePlay), MessageType.Error);
            DrawReadinessBucket("Recommended Before Play", readiness.GetIssues(PyralisSceneReadinessSeverity.RecommendedBeforePlay), MessageType.Warning);
            DrawReadinessBucket("Proof Enhancers", readiness.GetIssues(PyralisSceneReadinessSeverity.ProofEnhancer), MessageType.Info);
        }

        private static void DrawReadinessBucket(
            string label,
            IReadOnlyList<PyralisSceneReadinessIssue> issues,
            MessageType messageType)
        {
            if (issues == null || issues.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            int visible = Mathf.Min(issues.Count, 4);
            for (int i = 0; i < visible; i++)
            {
                PyralisSceneReadinessIssue issue = issues[i];
                if (issue == null)
                    continue;

                string text = string.IsNullOrWhiteSpace(issue.NativeAction)
                    ? issue.Message
                    : issue.Message + "\nNext native action: " + issue.NativeAction;
                EditorGUILayout.HelpBox(text, messageType);
            }

            if (issues.Count > visible)
                EditorGUILayout.LabelField("+" + (issues.Count - visible) + " more readiness item(s)", EditorStyles.miniLabel);
        }

        private void DrawValidationIssueGroup(
            PyralisAuthoringValidationCategory category,
            IReadOnlyList<PyralisAuthoringValidationIssue> issues)
        {
            bool drewAny = false;

            for (int i = 0; i < issues.Count; i++)
            {
                PyralisAuthoringValidationIssue issue = issues[i];
                if (issue == null || issue.Category != category)
                    continue;

                if (!drewAny)
                {
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField(PyralisAuthoringValidationModel.GetCategoryTitle(category), EditorStyles.miniBoldLabel);
                    drewAny = true;
                }

                DrawValidationIssueCard(issue);
            }
        }

        private void DrawValidationIssueCard(PyralisAuthoringValidationIssue issue)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Issue Code", issue.IssueCode, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Problem", issue.Problem, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Affected Field", issue.AffectedMember, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Why It Matters", issue.WhyItMatters, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("Inspect Next", issue.InspectionHint, EditorStyles.wordWrappedMiniLabel);
                DrawValidationIssueTypedMetadata(issue);
                DrawValidationIssueEvidence(issue);

                if (issue.HasGuidanceAction && GUILayout.Button(issue.GuidanceActionLabel))
                    TryRunGuidanceAction(issue);

                if (issue.CanInspectTarget && GUILayout.Button(issue.PrimaryActionLabel))
                    SelectAndPing(issue.Target);
            }
        }

        private static void DrawValidationIssueTypedMetadata(PyralisAuthoringValidationIssue issue)
        {
            PyralisAuthoringIssue typedIssue = issue?.TypedIssue;
            if (typedIssue == null)
                return;

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Typed Issue", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Severity", typedIssue.Severity.ToString(), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Work Intent", typedIssue.WorkIntent, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("Evidence", PyralisAuthoringLabelUtility.GetEvidenceLabel(typedIssue.EvidenceState), EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrWhiteSpace(typedIssue.FieldOrComponent))
                EditorGUILayout.LabelField("Field / Component", typedIssue.FieldOrComponent, EditorStyles.wordWrappedMiniLabel);
            if (typedIssue.NativeAction.HasValue)
            {
                EditorGUILayout.LabelField("Native Unity Action", EditorStyles.miniBoldLabel);
                PyralisAuthoringSurfaceBeacon.DrawNativeAction(typedIssue.NativeAction.Value, typedIssue.NativeAction.Value.ToGuidanceSentence());
            }
            EditorGUI.indentLevel--;
        }

        private static void DrawValidationIssueEvidence(PyralisAuthoringValidationIssue issue)
        {
            if (issue == null || !issue.HasAuditEvidence)
                return;

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Audit Evidence", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            if (!string.IsNullOrWhiteSpace(issue.Expected))
                EditorGUILayout.LabelField("Expected", issue.Expected, EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrWhiteSpace(issue.Found))
                EditorGUILayout.LabelField("Found", issue.Found, EditorStyles.wordWrappedMiniLabel);
            if (!string.IsNullOrWhiteSpace(issue.SuccessLooksLike))
                EditorGUILayout.LabelField("Success Looks Like", issue.SuccessLooksLike, EditorStyles.wordWrappedMiniLabel);
            EditorGUI.indentLevel--;
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

            if (CanUseAsActiveSetup(target))
            {
                _pinnedActiveSetup = GetSetupContext(target);
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

            if (CanUseAsActiveSetup(target))
            {
                _pinnedActiveSetup = GetSetupContext(target);
                InvalidateAuthoringCache();
            }

            _mode = AuthoringWindowMode.Guide;
            _guideScroll = Vector2.zero;
            Repaint();
            return true;
        }

        private static void DrawFactExplorerMode(Object activeSetup)
        {
            EditorGUILayout.LabelField("Fact Explorer", EditorStyles.boldLabel);
            DrawSemanticHelpBox("Read-only coverage view. Facts explain what Pyralis knows about capabilities, setup nodes, proof paths, Inspector handoffs, validation vocabulary, and future convention-derived guidance. Use native Unity surfaces for creation, assignment, customization, and Play Mode proof.", MessageType.Info);

            IReadOnlyList<PyralisAuthoringFact> facts = PyralisAuthoringFactRegistry.AllFacts;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", activeSetup != null ? $"{activeSetup.name} ({activeSetup.GetType().Name})" : "No active setup selected");
                EditorGUILayout.LabelField("Total Facts", facts.Count.ToString());
                DrawFactCoverageSummary(facts);
            }

            DrawFeatureContractSetupRecipes();

            DrawFactGroup(PyralisAuthoringFactKind.RuntimeCapability, facts);
            DrawFactGroup(PyralisAuthoringFactKind.FeatureContract, facts);
            DrawFactGroup(PyralisAuthoringFactKind.RouteFamily, facts);
            DrawFactGroup(PyralisAuthoringFactKind.RouteIntent, facts);
            DrawFactGroup(PyralisAuthoringFactKind.SetupNode, facts);
            DrawFactGroup(PyralisAuthoringFactKind.Proof, facts);
            DrawFactGroup(PyralisAuthoringFactKind.AssignmentField, facts);
            DrawFactGroup(PyralisAuthoringFactKind.CustomizationMoment, facts);
            DrawFactGroup(PyralisAuthoringFactKind.Issue, facts);
            DrawFactGroup(PyralisAuthoringFactKind.Definition, facts);
            DrawFactGroup(PyralisAuthoringFactKind.Profile, facts);
            DrawFactGroup(PyralisAuthoringFactKind.SceneComponent, facts);
            DrawFactGroup(PyralisAuthoringFactKind.PrefabComponent, facts);
        }

        private static void DrawFactCoverageSummary(IReadOnlyList<PyralisAuthoringFact> facts)
        {
            EditorGUILayout.LabelField("Coverage", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            System.Array kinds = System.Enum.GetValues(typeof(PyralisAuthoringFactKind));
            for (int i = 0; i < kinds.Length; i++)
            {
                PyralisAuthoringFactKind kind = (PyralisAuthoringFactKind)kinds.GetValue(i);
                int count = CountFacts(kind, facts);
                if (count > 0)
                    EditorGUILayout.LabelField(kind.ToString(), count.ToString(), EditorStyles.wordWrappedMiniLabel);
            }
            EditorGUI.indentLevel--;
        }

        private static int CountFacts(PyralisAuthoringFactKind kind, IReadOnlyList<PyralisAuthoringFact> facts)
        {
            if (facts == null)
                return 0;

            int count = 0;
            for (int i = 0; i < facts.Count; i++)
            {
                if (facts[i] != null && facts[i].Kind == kind)
                    count++;
            }

            return count;
        }

        private static void DrawFeatureContractSetupRecipes()
        {
            IReadOnlyList<PyralisAuthoringContract> contracts = PyralisAuthoringContractRegistry.All;
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Contract-Backed Feature Module Setup", EditorStyles.boldLabel);
            DrawSemanticHelpBox("Read-only setup recipes generated from feature-owned authoring contracts. Use native Unity surfaces for asset creation, Prefab/Component composition, Inspector assignment, object picking, and Play Mode proof.", MessageType.Info);

            if (contracts == null || contracts.Count == 0)
            {
                EditorGUILayout.LabelField("No feature contracts discovered. Tag interfaces with [AuthoringContract(ModuleId=\"...\")] for reflective discovery.", EditorStyles.wordWrappedMiniLabel);
                return;
            }

            Dictionary<string, List<PyralisAuthoringContract>> contractsByCategory = BuildContractsByCategory(contracts);
            List<string> categories = new List<string>(contractsByCategory.Keys);
            categories.Sort(System.StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < categories.Count; i++)
            {
                string category = categories[i];
                List<PyralisAuthoringContract> categoryContracts = contractsByCategory[category];
                string key = "Pyralis.AuthoringWindow.ContractSetup." + category;
                bool isOpen = ServiceStepFoldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, $"{category} Contracts ({categoryContracts.Count})", true);
                ServiceStepFoldouts[key] = isOpen;

                if (!isOpen)
                    continue;

                EditorGUI.indentLevel++;
                for (int contractIndex = 0; contractIndex < categoryContracts.Count; contractIndex++)
                    DrawFeatureContractSetupRecipe(categoryContracts[contractIndex]);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawContractBackedFeatureModuleSetup(GameSetupProfile setupProfile)
        {
            DrawFeatureContractSetupRecipes();
        }

        private static Dictionary<string, List<PyralisAuthoringContract>> BuildContractsByCategory(IReadOnlyList<PyralisAuthoringContract> contracts)
        {
            Dictionary<string, List<PyralisAuthoringContract>> contractsByCategory = new Dictionary<string, List<PyralisAuthoringContract>>();
            for (int i = 0; i < contracts.Count; i++)
            {
                PyralisAuthoringContract contract = contracts[i];
                if (contract == null)
                    continue;

                string category = string.IsNullOrWhiteSpace(contract.AuthoringCategory) ? "General" : contract.AuthoringCategory;
                if (!contractsByCategory.TryGetValue(category, out List<PyralisAuthoringContract> categoryContracts))
                {
                    categoryContracts = new List<PyralisAuthoringContract>();
                    contractsByCategory.Add(category, categoryContracts);
                }

                categoryContracts.Add(contract);
            }

            foreach (List<PyralisAuthoringContract> categoryContracts in contractsByCategory.Values)
            {
                categoryContracts.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, System.StringComparison.OrdinalIgnoreCase));
            }

            return contractsByCategory;
        }

        private static void DrawFeatureContractSetupRecipe(PyralisAuthoringContract contract)
        {
            if (contract == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(contract.DisplayName, contract.StableId, EditorStyles.boldLabel);
                DrawSemanticTagStrip(GetFeatureContractSetupTags(contract));
                DrawMiniField("Feature Contract", contract.StableId);
                DrawMiniField("Required Profile", contract.RequiredProfileType != null ? contract.RequiredProfileType.Name : "None for this module.");
                DrawMiniList("Runtime Interfaces", contract.RequiredRuntimeInterfaceNames);
                DrawMiniList("Supported Lanes", ToPresentationModeNames(contract.SupportedPresentationModes));
                DrawMiniList("Unsupported / Caution Lanes", ToPresentationModeNames(contract.UnsupportedPresentationModes));
                if (!string.IsNullOrWhiteSpace(contract.UnsupportedLaneMessage))
                    DrawMiniField("Unsupported Lane Message", contract.UnsupportedLaneMessage);
                DrawMiniList("Consumed Actions", contract.ConsumedActionRoles);
                DrawContractNativeSetupActions(contract.NativeSetup);
                DrawMiniList("Assignment Fields", contract.AssignmentFields);
                DrawMiniList("Customization Moments", contract.CustomizationMoments);
                DrawMiniField("First Proof Target", string.IsNullOrWhiteSpace(contract.FirstProofTargetId) ? "None recorded yet." : contract.FirstProofTargetId);
            }
        }

        private static List<PyralisAuthoringSemanticTag> GetFeatureContractSetupTags(PyralisAuthoringContract contract)
        {
            List<PyralisAuthoringSemanticTag> tags = new List<PyralisAuthoringSemanticTag>();
            AddSemanticTag(PyralisAuthoringSemanticTag.Authoring, tags);
            AddSemanticTag(PyralisAuthoringSemanticTag.Project, tags);
            AddSemanticTag(PyralisAuthoringSemanticTag.Inspector, tags);
            AddSemanticTagIfAny(contract.RequiredRuntimeInterfaceNames, PyralisAuthoringSemanticTag.Prefab, tags);
            AddSemanticTagIfAny(contract.ConsumedActionRoles, PyralisAuthoringSemanticTag.Input, tags);
            if (!string.IsNullOrWhiteSpace(contract.FirstProofTargetId))
                AddSemanticTag(PyralisAuthoringSemanticTag.PlayMode, tags);
            return tags;
        }

        private static string[] ToPresentationModeNames(ActorPresentationMode[] modes)
        {
            if (modes == null || modes.Length == 0)
                return System.Array.Empty<string>();

            string[] names = new string[modes.Length];
            for (int i = 0; i < modes.Length; i++)
                names[i] = modes[i].ToString();

            return names;
        }

        private static void DrawContractNativeSetupActions(IReadOnlyList<string> nativeSetup)
        {
            if (nativeSetup == null || nativeSetup.Count == 0)
            {
                DrawMiniField("Native Setup Actions", "None recorded yet.");
                return;
            }

            EditorGUILayout.LabelField("Native Setup Actions", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < nativeSetup.Count; i++)
            {
                string setupStep = nativeSetup[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawSemanticTagBadge(GetContractSetupSemanticTag(setupStep));
                    EditorGUILayout.LabelField(GetContractSetupSurfaceLabel(setupStep), GUILayout.Width(150f));
                    DrawSemanticMiniLabel(setupStep);
                }
            }
            EditorGUI.indentLevel--;
        }

        private static PyralisAuthoringSemanticTag GetContractSetupSemanticTag(string setupStep)
        {
            if (string.IsNullOrWhiteSpace(setupStep))
                return PyralisAuthoringSemanticTag.Authoring;

            if (ContainsIgnoreCase(setupStep, "Play Mode") || ContainsIgnoreCase(setupStep, "proof"))
                return PyralisAuthoringSemanticTag.PlayMode;

            if (ContainsIgnoreCase(setupStep, "runtime prefab") || ContainsIgnoreCase(setupStep, "component"))
                return PyralisAuthoringSemanticTag.Prefab;

            if (ContainsIgnoreCase(setupStep, "bind ") || ContainsIgnoreCase(setupStep, "InputProfile"))
                return PyralisAuthoringSemanticTag.Input;

            if (ContainsIgnoreCase(setupStep, "assign ") || ContainsIgnoreCase(setupStep, "add module"))
                return PyralisAuthoringSemanticTag.Inspector;

            if (ContainsIgnoreCase(setupStep, "create "))
                return PyralisAuthoringSemanticTag.Project;

            return PyralisAuthoringSemanticTag.Authoring;
        }

        private static string GetContractSetupSurfaceLabel(string setupStep)
        {
            if (ContainsIgnoreCase(setupStep, "runtime prefab") || ContainsIgnoreCase(setupStep, "component"))
                return "Prefab/Add Component";

            if (ContainsIgnoreCase(setupStep, "bind ") || ContainsIgnoreCase(setupStep, "InputProfile"))
                return "Inspector/Input Profile";

            if (ContainsIgnoreCase(setupStep, "assign ") || ContainsIgnoreCase(setupStep, "add module"))
                return "Inspector/Object Picker";

            if (ContainsIgnoreCase(setupStep, "Play Mode") || ContainsIgnoreCase(setupStep, "proof"))
                return "Play Mode Proof";

            if (ContainsIgnoreCase(setupStep, "create "))
                return "Project Window Create";

            return "Native Unity Surface";
        }

        private static bool ContainsIgnoreCase(string value, string match)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !string.IsNullOrWhiteSpace(match)
                && value.IndexOf(match, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void DrawFactGroup(PyralisAuthoringFactKind kind, IReadOnlyList<PyralisAuthoringFact> facts)
        {
            int count = CountFacts(kind, facts);
            string key = "Pyralis.AuthoringWindow.FactExplorer." + kind;
            bool isOpen = ServiceStepFoldouts.TryGetValue(key, out bool value) && value;
            isOpen = EditorGUILayout.Foldout(isOpen, $"{kind} ({count})", true);
            ServiceStepFoldouts[key] = isOpen;

            if (!isOpen)
                return;

            if (count == 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("No facts yet. This is a coverage gap for future Authoring 2.0 work.", EditorStyles.wordWrappedMiniLabel);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < facts.Count; i++)
            {
                PyralisAuthoringFact fact = facts[i];
                if (fact != null && fact.Kind == kind)
                    DrawFactCard(fact);
            }
            EditorGUI.indentLevel--;
        }

        private static void DrawFactCard(PyralisAuthoringFact fact)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(fact.DisplayName, fact.StableId, EditorStyles.boldLabel);
                DrawFactSemanticTags(fact);
                DrawMiniField("Source", fact.SourceKind + " / " + fact.Confidence);
                DrawMiniField("Work Intent", fact.WorkIntent);
                DrawMiniField("Route Relevance", fact.RouteRelevance);
                DrawMiniField("Summary", fact.Summary);
                DrawMiniField("First Proof", fact.FirstProof);
                DrawMiniList("Goal Tags", fact.GoalTags);
                DrawMiniList("Supported Lanes", fact.LaneTags);
                DrawMiniList("Unsupported / Caution Lanes", fact.UnsupportedLaneTags);
                DrawMiniList("Required Definitions", fact.RequiredDefinitions);
                DrawMiniList("Required Profiles", fact.RequiredProfiles);
                DrawMiniList("Required Scene Components", fact.RequiredSceneComponents);
                DrawMiniList("Required Prefab Components", fact.RequiredPrefabComponents);
                DrawMiniList("Assignment Fields", fact.AssignmentFields);
                DrawMiniList("Customization Moments", fact.CustomizationMoments);
                DrawMiniList("Can Wait", fact.CanWait);
                DrawFactNativeActions(fact.NativeActions);
                DrawMiniList("Related Stable Ids", fact.RelatedStableIds);
            }
        }

        private static void DrawFactNativeActions(IReadOnlyList<PyralisAuthoringNativeAction> actions)
        {
            if (actions == null || actions.Count == 0)
            {
                DrawMiniField("Native Unity Actions", "None recorded yet.");
                return;
            }

            EditorGUILayout.LabelField("Native Unity Actions", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < actions.Count; i++)
            {
                PyralisAuthoringSurfaceBeacon.DrawNativeAction(actions[i], actions[i].ToGuidanceSentence());
            }
            EditorGUI.indentLevel--;
        }

        private static void DrawFactSemanticTags(PyralisAuthoringFact fact)
        {
            List<PyralisAuthoringSemanticTag> tags = GetFactSemanticTags(fact);
            if (tags.Count == 0)
                return;

            DrawSemanticTagStrip(tags);
        }

        private static List<PyralisAuthoringSemanticTag> GetFactSemanticTags(PyralisAuthoringFact fact)
        {
            List<PyralisAuthoringSemanticTag> tags = new List<PyralisAuthoringSemanticTag>();
            if (fact == null)
                return tags;

            AddSemanticTagForFactKind(fact.Kind, tags);
            AddSemanticTagIfAny(fact.RequiredDefinitions, PyralisAuthoringSemanticTag.Definition, tags);
            AddSemanticTagIfAny(fact.RequiredProfiles, PyralisAuthoringSemanticTag.Profile, tags);
            AddSemanticTagIfAny(fact.RequiredSceneComponents, PyralisAuthoringSemanticTag.Hierarchy, tags);
            AddSemanticTagIfAny(fact.RequiredPrefabComponents, PyralisAuthoringSemanticTag.Prefab, tags);
            AddSemanticTagIfAny(fact.AssignmentFields, PyralisAuthoringSemanticTag.Inspector, tags);
            AddSemanticTagIfAny(fact.CustomizationMoments, PyralisAuthoringSemanticTag.Inspector, tags);

            for (int i = 0; i < fact.NativeActions.Length; i++)
                AddSemanticTag(PyralisAuthoringLabelUtility.GetSemanticTag(fact.NativeActions[i].Surface), tags);

            AddSemanticTagForText(fact.DisplayName, tags);
            AddSemanticTagForText(fact.Summary, tags);
            AddSemanticTagForText(fact.RouteRelevance, tags);
            AddSemanticTagForText(fact.FirstProof, tags);
            AddSemanticTagForText(string.Join(" ", fact.RequiredDefinitions), tags);
            AddSemanticTagForText(string.Join(" ", fact.RequiredProfiles), tags);
            AddSemanticTagForText(string.Join(" ", fact.RequiredSceneComponents), tags);
            AddSemanticTagForText(string.Join(" ", fact.RequiredPrefabComponents), tags);
            AddSemanticTagForText(string.Join(" ", fact.AssignmentFields), tags);
            AddSemanticTagForText(string.Join(" ", fact.CustomizationMoments), tags);

            return tags;
        }

        private static void AddSemanticTagForFactKind(PyralisAuthoringFactKind kind, List<PyralisAuthoringSemanticTag> tags)
        {
            switch (kind)
            {
                case PyralisAuthoringFactKind.FeatureContract:
                    AddSemanticTag(PyralisAuthoringSemanticTag.Authoring, tags);
                    AddSemanticTag(PyralisAuthoringSemanticTag.Inspector, tags);
                    AddSemanticTag(PyralisAuthoringSemanticTag.Project, tags);
                    break;
                case PyralisAuthoringFactKind.Definition:
                    AddSemanticTag(PyralisAuthoringSemanticTag.Definition, tags);
                    AddSemanticTag(PyralisAuthoringSemanticTag.Project, tags);
                    break;
                case PyralisAuthoringFactKind.Profile:
                    AddSemanticTag(PyralisAuthoringSemanticTag.Profile, tags);
                    AddSemanticTag(PyralisAuthoringSemanticTag.Project, tags);
                    break;
                case PyralisAuthoringFactKind.SceneComponent:
                    AddSemanticTag(PyralisAuthoringSemanticTag.Hierarchy, tags);
                    AddSemanticTag(PyralisAuthoringSemanticTag.Component, tags);
                    break;
                case PyralisAuthoringFactKind.PrefabComponent:
                    AddSemanticTag(PyralisAuthoringSemanticTag.Prefab, tags);
                    AddSemanticTag(PyralisAuthoringSemanticTag.Component, tags);
                    break;
                case PyralisAuthoringFactKind.AssignmentField:
                case PyralisAuthoringFactKind.CustomizationMoment:
                    AddSemanticTag(PyralisAuthoringSemanticTag.Inspector, tags);
                    break;
                case PyralisAuthoringFactKind.Proof:
                    AddSemanticTag(PyralisAuthoringSemanticTag.PlayMode, tags);
                    break;
                default:
                    AddSemanticTag(PyralisAuthoringSemanticTag.Authoring, tags);
                    break;
            }
        }

        private static void AddSemanticTagForText(string text, List<PyralisAuthoringSemanticTag> tags)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text.IndexOf("Project", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("CreateAssetMenu", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.Project, tags);
            if (text.IndexOf("Hierarchy", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.Hierarchy, tags);
            if (text.IndexOf("Inspector", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("Object Picker", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("assign", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.Inspector, tags);
            if (text.IndexOf("Definition", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.Definition, tags);
            if (text.IndexOf("Profile", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.Profile, tags);
            if (text.IndexOf("Prefab", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.Prefab, tags);
            if (text.IndexOf("Component", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("AddComponentMenu", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("RequireComponent", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.Component, tags);
            if (text.IndexOf("Input", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("Action", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.Input, tags);
            if (text.IndexOf("UI", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("HUD", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("Canvas", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.UI, tags);
            if (text.IndexOf("Animation", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("Animator", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.Animation, tags);
            if (text.IndexOf("Audio", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("Sound", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.Audio, tags);
            if (text.IndexOf("Play Mode", System.StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("proof", System.StringComparison.OrdinalIgnoreCase) >= 0)
                AddSemanticTag(PyralisAuthoringSemanticTag.PlayMode, tags);
        }

        private static void AddSemanticTagIfAny(string[] values, PyralisAuthoringSemanticTag tag, List<PyralisAuthoringSemanticTag> tags)
        {
            if (values != null && values.Length > 0)
                AddSemanticTag(tag, tags);
        }

        private static void AddSemanticTag(PyralisAuthoringSemanticTag tag, List<PyralisAuthoringSemanticTag> tags)
        {
            if (!tags.Contains(tag))
                tags.Add(tag);
        }

        private static void DrawSemanticTagStrip(IReadOnlyList<PyralisAuthoringSemanticTag> tags)
        {
            if (tags == null || tags.Count == 0)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < tags.Count; i++)
                    DrawSemanticTagBadge(tags[i]);
            }
        }

        private static void DrawSemanticTagBadge(PyralisAuthoringSemanticTag tag)
        {
            Color color = PyralisAuthoringLabelUtility.GetSemanticTagColor(tag);
            string label = PyralisAuthoringLabelUtility.GetSemanticTagLabel(tag);
            GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                padding = new RectOffset(5, 5, 1, 2)
            };

            Vector2 size = style.CalcSize(new GUIContent(label));
            Rect rect = GUILayoutUtility.GetRect(size.x + 12f, 18f, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(rect, color);
            Rect inner = new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f);
            EditorGUI.DrawRect(inner, new Color(color.r * 0.74f, color.g * 0.74f, color.b * 0.74f, 1f));
            GUI.Label(rect, label, style);
        }

        private static void SelectAndPing(Object target)
        {
            if (target == null)
                return;

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        private static void DrawSelectedContext(Object selection, PyralisAuthoringRouteReport report)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField(SelectedAuthoringContextLabel, EditorStyles.boldLabel);

            if (selection == null)
            {
                EditorGUILayout.HelpBox("Select a Pyralis scene object, component, definition, profile, or starter-pack asset to make this window show the authoring context for that selection.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Selection", selection.name);
                EditorGUILayout.LabelField("Type", selection.GetType().Name);

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
                DrawRuntimePatternContext(pattern);
                return;
            }

            if (selection is GameSetupProfile setupProfile)
            {
                DrawSetupProfileContext(setupProfile);
                DrawFeatureAdvisor(setupProfile);
                return;
            }

            if (selection is SessionDefinition or GameModeDefinition or ParticipantDefinition or PawnDefinition or FeatureModuleDefinition)
                EditorGUILayout.HelpBox(report.RouteGuidance, MessageType.Info);
        }

        private static void DrawSetupChain(Object selection, PyralisAuthoringRouteReport report, bool showOnlyNextAction)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField(SetupChainLabel, EditorStyles.boldLabel);

            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(selection);
            SessionDefinition session = GetSelectedSession(selection, bootstrap);
            GameModeDefinition mode = GetSelectedMode(selection, session);
            GameSetupProfile setupProfile = GetSelectedSetupProfile(selection, mode);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.HelpBox("Use this map to understand the route chain before editing Inspector fields. It diagnoses what is connected, what is missing, and which native Unity step belongs next.", MessageType.Info);

                if (!showOnlyNextAction)
                {
                    DrawServiceStep("Scene Root", bootstrap != null, bootstrap, "Startup object found.", "Select a GameplaySessionBootstrap or Gameplay Root object.", "This is the scene object that starts the Pyralis session when Play begins.");
                    DrawServiceStep("Session", session != null, session, "Session asset is connected.", "Create or assign the first asset the scene root reads.", "The session names the game mode and the players, seats, cursors, or other participants that can join.");
                    DrawServiceStep("Game Rules", mode != null, mode, "Game rules asset is connected.", "Create or assign the rules asset for this session.", "The game mode points at the setup recipe and owns rule-level choices for this playable loop.");
                    DrawServiceStep("Setup Recipe", setupProfile != null, setupProfile, "Setup recipe is connected.", "Create or assign the recipe that combines game capability patterns.", "The setup recipe combines capability patterns before prefab or scene wiring starts.");
                    DrawRuntimePatternServiceSteps(setupProfile);
                    DrawParticipantServiceSteps(session, PyralisAuthoringRouteDescriptor.Build(setupProfile, session, mode));
                }

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Current Recommendation", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(report.NextStep, EditorStyles.wordWrappedLabel);
            }

            DrawMissingCoreActions(selection, bootstrap, session, mode, setupProfile);
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

        private static void DrawRuntimePatternContext(RuntimePatternDefinition pattern)
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
                        FillMissingRuntimePatternText(pattern);
                }

                if (GUILayout.Button("Copy Guidance"))
                    EditorGUIUtility.systemCopyBuffer = description + "\n\nSetup notes:\n" + setupNotes;
            }
        }

        private static void DrawSetupProfileContext(GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(4f);
            DrawCapabilityPicker(setupProfile);

            EditorGUILayout.Space(4f);
            DrawRuntimeCapabilityCatalog(setupProfile);

            EditorGUILayout.Space(4f);
            DrawContractBackedFeatureModuleSetup(setupProfile);

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Setup Profile Patterns", EditorStyles.boldLabel);

            if (setupProfile.runtimePatterns == null || setupProfile.runtimePatterns.Length == 0)
            {
                EditorGUILayout.HelpBox("No runtime patterns assigned yet. Add existing runtime patterns before wiring scene objects.", MessageType.Warning);
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

        private static void DrawCapabilityPicker(GameSetupProfile setupProfile)
        {
            EditorGUILayout.LabelField("Design Capabilities", EditorStyles.boldLabel);

            if (setupProfile == null)
            {
                EditorGUILayout.HelpBox("Select a GameSetupProfile before choosing capabilities.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Pick the capabilities this game route needs. The picker assigns existing RuntimePatternDefinition assets, then the guide explains the setup impact.", EditorStyles.wordWrappedMiniLabel);

                for (int i = 0; i < GuidedCapabilityFamilies.Length; i++)
                    DrawCapabilityPickerRow(setupProfile, GuidedCapabilityFamilies[i]);
            }
        }

        private static void DrawCapabilityPickerRow(GameSetupProfile setupProfile, RuntimeCapabilityFamily family)
        {
            List<RuntimePatternDefinition> availablePatterns = FindRuntimePatternsByFamily(family);
            RuntimePatternDefinition selectedPattern = PyralisAuthoringCapabilitySelection.GetSelectedPattern(setupProfile.runtimePatterns, family);
            if (selectedPattern != null && !availablePatterns.Contains(selectedPattern))
                availablePatterns.Insert(0, selectedPattern);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool hasAvailablePattern = availablePatterns.Count > 0;
                    bool isEnabled = selectedPattern != null;
                    using (new EditorGUI.DisabledScope(!hasAvailablePattern && !isEnabled))
                    {
                        bool nextEnabled = EditorGUILayout.ToggleLeft(GetCapabilityLabel(family), isEnabled, GUILayout.Width(210f));
                        if (nextEnabled != isEnabled)
                        {
                            if (nextEnabled && hasAvailablePattern)
                                ApplyCapabilityPattern(setupProfile, availablePatterns[0]);
                            else
                                RemoveCapabilityPattern(setupProfile, family);
                        }
                    }

                    using (new EditorGUI.DisabledScope(!isEnabled || availablePatterns.Count <= 1))
                    {
                        int selectedIndex = Mathf.Max(0, availablePatterns.IndexOf(selectedPattern));
                        string[] labels = BuildPatternPopupLabels(availablePatterns);
                        int nextIndex = EditorGUILayout.Popup(selectedIndex, labels);
                        if (nextIndex != selectedIndex && nextIndex >= 0 && nextIndex < availablePatterns.Count)
                            ApplyCapabilityPattern(setupProfile, availablePatterns[nextIndex]);
                    }
                }

                EditorGUILayout.LabelField(GetCapabilityDescription(family), EditorStyles.wordWrappedMiniLabel);
                if (availablePatterns.Count == 0)
                    EditorGUILayout.HelpBox("No existing RuntimePatternDefinition asset was found for this capability. Create one only if the existing starter patterns cannot describe this route.", MessageType.None);
            }
        }

        private static void DrawRuntimeCapabilityCatalog(GameSetupProfile setupProfile)
        {
            EditorGUILayout.LabelField("Runtime Capability Catalog", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    ColorizeSemanticTokens("Browse Pyralis-supported runtime setup by game goal or runtime lane. No asset or component creation happens here; each card points back to native Project, Hierarchy, Inspector, Add Component, assignment, customization, and Play Mode proof steps."),
                    GetSemanticMiniLabelStyle());

                DrawRuntimeCapabilityCatalogByGoal(setupProfile);
                DrawRuntimeCapabilityCatalogByLane(setupProfile);
            }
        }

        private static void DrawRuntimeCapabilityCatalogByGoal(GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Browse By Engine Spine Capability", EditorStyles.miniBoldLabel);
            foreach (AuthoringCapability cap in AuthoringCapabilityRegistry.GetAllIndividualCapabilities())
            {
                if (cap == AuthoringCapability.None) continue;
                
                List<PyralisAuthoringFact> facts = PyralisAuthoringFactRegistry.AllFacts
                    .Where(f => f.Kind == PyralisAuthoringFactKind.RuntimeCapability || f.Kind == PyralisAuthoringFactKind.FeatureContract)
                    .Where(f => (f.Capability & cap) != 0)
                    .ToList();

                if (facts.Count > 0)
                {
                    DrawRuntimeCapabilityGroup(
                        AuthoringCapabilityRegistry.GetDisplayName(cap), 
                        "Capability", 
                        facts, 
                        setupProfile, 
                        AuthoringCapabilityRegistry.GetTooltip(cap));
                }
            }
        }

        private static void DrawRuntimeCapabilityCatalogByLane(GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Browse By Runtime Lane", EditorStyles.miniBoldLabel);
            for (int i = 0; i < GuidedCapabilityLaneTags.Length; i++)
            {
                RuntimeCapabilityLaneTag tag = GuidedCapabilityLaneTags[i];
                string laneName = tag.ToString();
                List<PyralisAuthoringFact> facts = PyralisAuthoringFactRegistry.AllFacts
                    .Where(f => f.Kind == PyralisAuthoringFactKind.RuntimeCapability 
                                || f.Kind == PyralisAuthoringFactKind.FeatureContract
                                || f.Kind == PyralisAuthoringFactKind.PrefabComponent
                                || f.Kind == PyralisAuthoringFactKind.Profile
                                || f.Kind == PyralisAuthoringFactKind.Definition)
                    .Where(f => f.HasLane(laneName) || f.IsExplicitlyUnsupported(laneName)).ToList();
DrawRuntimeCapabilityGroup(GetLaneTagLabel(tag), "Lane", facts, setupProfile, laneName, tag);
            }
        }

        private static void DrawRuntimeCapabilityGroup(
            string title,
            string groupKind,
            IReadOnlyList<PyralisAuthoringFact> facts,
            GameSetupProfile setupProfile,
            string keySuffix,
            RuntimeCapabilityLaneTag? laneTag = null)
        {
            string key = "Pyralis.AuthoringWindow.RuntimeCapabilityCatalog." + groupKind + "." + keySuffix;
            bool isOpen = ServiceStepFoldouts.TryGetValue(key, out bool value) && value;
            int count = facts != null ? facts.Count : 0;
            isOpen = EditorGUILayout.Foldout(isOpen, $"{title} ({count})", true);
            ServiceStepFoldouts[key] = isOpen;

            if (!isOpen || facts == null)
                return;

            EditorGUI.indentLevel++;
            for (int i = 0; i < facts.Count; i++)
                DrawRuntimeCapabilityCard(facts[i], setupProfile, keySuffix + "." + i, laneTag);
            EditorGUI.indentLevel--;
        }

        private static void DrawRuntimeCapabilityCard(
            PyralisAuthoringFact fact,
            GameSetupProfile setupProfile,
            string keySuffix,
            RuntimeCapabilityLaneTag? laneContext)
        {
            if (fact == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string status = GetRuntimeCapabilityStatus(fact, setupProfile, laneContext);
                EditorGUILayout.LabelField(fact.DisplayName, status, EditorStyles.boldLabel);
                EditorGUILayout.LabelField(fact.Summary, EditorStyles.wordWrappedMiniLabel);

                string key = "Pyralis.AuthoringWindow.RuntimeCapabilityCard." + fact.StableId + "." + keySuffix;
                bool isOpen = ServiceStepFoldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, "Native Setup Guide", true);
                ServiceStepFoldouts[key] = isOpen;

                if (!isOpen)
                    return;

                EditorGUI.indentLevel++;
                DrawMiniField("Route Relevance", fact.RouteRelevance);
                DrawMiniList("Required Definitions", fact.RequiredDefinitions);
                DrawMiniList("Required Profiles", fact.RequiredProfiles);
                DrawMiniList("Required Scene Components", fact.RequiredSceneComponents);
                DrawMiniList("Required Prefab Components", fact.RequiredPrefabComponents);
                DrawMiniList("Assignment Fields", fact.AssignmentFields);
                DrawMiniList("Customization Moments", fact.CustomizationMoments);
                DrawMiniList("Can Wait", fact.CanWait);
                DrawMiniField("First Proof", fact.FirstProof);
                DrawMiniList("Common Next Capabilities", fact.RelatedStableIds);
                EditorGUI.indentLevel--;
            }
        }

        private static string GetRuntimeCapabilityStatus(PyralisAuthoringFact fact, GameSetupProfile setupProfile, RuntimeCapabilityLaneTag? laneContext)
        {
            if (fact == null)
                return "Unknown";

            if (laneContext.HasValue)
            {
                string laneName = laneContext.Value.ToString();
                if (fact.IsExplicitlyUnsupported(laneName))
                    return "Explicitly unsupported for this lane";
                
                if (!fact.HasLane(laneName))
                    return "Available in Pyralis, but not explicitly relevant to this lane";
            }

            if (PyralisReflectiveContractSolver.IsSatisfied(fact, out string message, out _))
            {
                return "✔ " + message;
            }

            if (fact.RequiredSceneComponents != null && fact.RequiredSceneComponents.Length > 0)
            {
                return "❌ " + message;
            }

            return "Guide-only option";
        }

        private static void DrawMiniList(string label, IReadOnlyList<string> values)
        {
            DrawMiniList(label, values, string.Empty);
        }

        private static void DrawMiniList(string label, IReadOnlyList<string> values, string tooltip, int maxVisibleItems = int.MaxValue)
        {
            if (values == null || values.Count == 0)
            {
                DrawMiniField(label, "None for this first proof.", tooltip);
                return;
            }

            EditorGUILayout.LabelField(new GUIContent(label, tooltip ?? string.Empty), EditorStyles.miniBoldLabel);
            int visibleCount = Mathf.Min(values.Count, maxVisibleItems);
            for (int i = 0; i < visibleCount; i++)
                DrawSemanticMiniLabel("- " + values[i]);

            if (visibleCount < values.Count)
                DrawSemanticMiniLabel($"+ {values.Count - visibleCount} more when expanded");
        }

        private static List<RuntimePatternDefinition> FindRuntimePatternsByFamily(RuntimeCapabilityFamily family)
        {
            List<RuntimePatternDefinition> patterns = new List<RuntimePatternDefinition>();
            string[] guids = AssetDatabase.FindAssets("t:RuntimePatternDefinition");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                RuntimePatternDefinition pattern = AssetDatabase.LoadAssetAtPath<RuntimePatternDefinition>(path);
                if (pattern != null && pattern.capabilityFamily == family)
                    patterns.Add(pattern);
            }

            patterns.Sort((left, right) => string.Compare(GetRuntimePatternLabel(left), GetRuntimePatternLabel(right), System.StringComparison.OrdinalIgnoreCase));
            return patterns;
        }

        private static string[] BuildPatternPopupLabels(List<RuntimePatternDefinition> patterns)
        {
            string[] labels = new string[patterns.Count];
            for (int i = 0; i < patterns.Count; i++)
                labels[i] = GetRuntimePatternLabel(patterns[i]);

            return labels;
        }

        private static void ApplyCapabilityPattern(GameSetupProfile setupProfile, RuntimePatternDefinition pattern)
        {
            if (setupProfile == null || pattern == null)
                return;

            Undo.RecordObject(setupProfile, "Set Setup Capability Pattern");
            setupProfile.runtimePatterns = PyralisAuthoringCapabilitySelection.SetCapabilityPattern(setupProfile.runtimePatterns, pattern);
            EditorUtility.SetDirty(setupProfile);
        }

        private static void RemoveCapabilityPattern(GameSetupProfile setupProfile, RuntimeCapabilityFamily family)
        {
            if (setupProfile == null)
                return;

            Undo.RecordObject(setupProfile, "Remove Setup Capability Pattern");
            setupProfile.runtimePatterns = PyralisAuthoringCapabilitySelection.RemoveCapabilityFamily(setupProfile.runtimePatterns, family);
            EditorUtility.SetDirty(setupProfile);
        }

        private static void DrawFeatureAdvisor(GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Game Type And Feature Guide", EditorStyles.boldLabel);

            PyralisAuthoringFeatureAdvisor advisor = PyralisAuthoringFeatureAdvisor.Build(setupProfile);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Route Intent", advisor.RouteIntent, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("First Playable Proof", advisor.FirstProofLabel, EditorStyles.miniBoldLabel);
                DrawSemanticMiniLabel(advisor.FirstProofGuidance);
                DrawMiniField("First Unity Focus", advisor.FirstUnityFocus);
            }

            if (advisor.DesignPrompts.Count > 0)
            {
                EditorGUILayout.LabelField("Design Before Setup", EditorStyles.miniBoldLabel);
                for (int i = 0; i < advisor.DesignPrompts.Count; i++)
                    DrawDesignPrompt(advisor.DesignPrompts[i], i);
            }

            if (advisor.EnvironmentGuidance.Count > 0)
            {
                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField("World And Environment Contract", EditorStyles.miniBoldLabel);
                for (int i = 0; i < advisor.EnvironmentGuidance.Count; i++)
                    DrawFeatureAdvisorRow(advisor.EnvironmentGuidance[i], "Environment." + i);
            }

            if (advisor.SelectedFeatures.Count == 0)
            {
                EditorGUILayout.HelpBox("No capability patterns are selected yet. Choose existing RuntimePatternDefinition assets before wiring camera, input, HUD, pawns, menus, combat, or board objects.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Selected Capabilities", EditorStyles.miniBoldLabel);
            for (int i = 0; i < advisor.SelectedFeatures.Count; i++)
                DrawFeatureAdvisorRow(advisor.SelectedFeatures[i], "Selected." + i);

            if (advisor.RecommendedFeatures.Count == 0)
                return;

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Recommended Next Options", EditorStyles.miniBoldLabel);
            DrawFeatureAdvisorRowsBySource(advisor.RecommendedFeatures);
        }

        private static void DrawFeatureAdvisorRowsBySource(IReadOnlyList<PyralisAuthoringFeatureRow> rows)
        {
            string currentSource = null;
            for (int i = 0; i < rows.Count; i++)
            {
                PyralisAuthoringFeatureRow row = rows[i];
                if (row == null)
                    continue;

                if (currentSource != row.Source)
                {
                    currentSource = row.Source;
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField(currentSource, EditorStyles.miniBoldLabel);
                }

                DrawFeatureAdvisorRow(row, "Recommended." + i);
            }
        }

        private static void DrawDesignPrompt(PyralisAuthoringDesignPrompt prompt, int index)
        {
            if (prompt == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(prompt.Question, EditorStyles.miniBoldLabel);
                DrawSemanticMiniLabel(prompt.Options);

                string key = "Pyralis.AuthoringWindow.DesignPrompt." + index + "." + prompt.Question;
                bool isOpen = ServiceStepFoldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, "Why And Setup Impact", true);
                ServiceStepFoldouts[key] = isOpen;

                if (!isOpen)
                    return;

                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Why it matters", EditorStyles.miniBoldLabel);
                DrawSemanticMiniLabel(prompt.WhyItMatters);
                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField("Setup impact", EditorStyles.miniBoldLabel);
                DrawSemanticMiniLabel(prompt.SetupImpact);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawFeatureAdvisorRow(PyralisAuthoringFeatureRow row, string keySuffix)
        {
            if (row == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(row.Feature, row.Source, EditorStyles.boldLabel);
                DrawSemanticMiniLabel(row.GameplayEffect);

                string key = "Pyralis.AuthoringWindow.FeatureAdvisor." + keySuffix + "." + row.Feature;
                bool isOpen = ServiceStepFoldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, "Setup And Customization", true);
                ServiceStepFoldouts[key] = isOpen;

                if (!isOpen)
                    return;

                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Unity setup", EditorStyles.miniBoldLabel);
                DrawSemanticMiniLabel(row.UnitySetup);
                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField("Customize here", EditorStyles.miniBoldLabel);
                DrawSemanticMiniLabel(row.Customization);
                EditorGUI.indentLevel--;
            }
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

        private static void DrawServiceStep(string label, bool isReady, Object target, string readyText, string missingText, string detailText = null, bool isOptional = false)
        {
            DrawExpandableServiceStep(label, isReady, target, readyText, missingText, detailText, isOptional);
        }

        private static void DrawExpandableServiceStep(string label, bool isReady, Object target, string readyText, string missingText, string detailText, bool isOptional = false)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string status = GetReadinessBadge(isReady, target, isOptional);
                string detail = isReady ? readyText : missingText;
                string targetName = target != null ? $" ({target.name})" : string.Empty;
                EditorGUILayout.LabelField(label, $"{status}{targetName}: {detail}", EditorStyles.wordWrappedLabel);
                using (new EditorGUI.DisabledScope(target == null))
                {
                    if (GUILayout.Button("Inspect Asset", GUILayout.Width(96f)))
                    {
                        Selection.activeObject = target;
                        EditorGUIUtility.PingObject(target);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(detailText))
                return;

            string key = "Pyralis.AuthoringWindow.ServiceStep." + label;
            bool isOpen = ServiceStepFoldouts.TryGetValue(key, out bool value) && value;
            isOpen = EditorGUILayout.Foldout(isOpen, "Details", true);
            ServiceStepFoldouts[key] = isOpen;

            if (isOpen)
            {
                EditorGUI.indentLevel++;
                DrawSemanticMiniLabel(detailText);
                EditorGUI.indentLevel--;
            }
        }

        private static string GetReadinessBadge(bool isReady, Object target, bool isOptional = false)
        {
            if (isReady)
                return "[Ready]";

            if (isOptional)
                return "[Optional]";

            if (target != null)
                return "[Blocked]";

            return "[Needs Setup]";
        }

        private static void DrawRuntimePatternServiceSteps(GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Capability Patterns", EditorStyles.miniBoldLabel);

            if (setupProfile == null || setupProfile.runtimePatterns == null || setupProfile.runtimePatterns.Length == 0)
            {
                DrawServiceStep("Capability Patterns", false, setupProfile, string.Empty, "Choose existing patterns before scene wiring.", "Patterns describe the kind of game being built: pawn action, tabletop, camera/cursor, scoring, combat, traversal, and similar capability families.");
                return;
            }

            for (int i = 0; i < setupProfile.runtimePatterns.Length; i++)
            {
                RuntimePatternDefinition pattern = setupProfile.runtimePatterns[i];
                string readyText = pattern != null
                    ? $"{GetRuntimePatternLabel(pattern)} tells the Inspector what this setup needs."
                    : string.Empty;
                DrawServiceStep($"Pattern {i}", pattern != null, pattern, readyText, "Empty capability pattern slot; fill it in the Setup Recipe Inspector.", "Use patterns to declare game intent before creating scene objects or prefabs.");
            }
        }

        private static void DrawParticipantServiceSteps(SessionDefinition session, PyralisAuthoringRouteDescriptor route)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Participant Prep", EditorStyles.miniBoldLabel);

            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
            {
                DrawServiceStep("Player / Seat", false, session, string.Empty, "Create or assign participants after the route exists.", "A participant can be a player, AI, board seat, hand, faction, cursor, camera owner, or turn owner depending on the setup recipe.");
                return;
            }

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                DrawServiceStep($"Player / Seat {i}", participant != null, participant, "Participant asset is available for Inspector setup.", "Empty participant slot.", "This is who or what participates in the game session.");
                if (participant != null)
                {
                    bool pawnOptional = route == null || !route.RequiresPawn;
                    string missingText = pawnOptional
                        ? "Leave empty for no-pawn routes; create one only if this participant owns an actor body."
                        : "Create or assign a PawnDefinition for this pawn-backed route.";
                    DrawServiceStep("Pawn Actor", participant.defaultPawn != null, participant.defaultPawn, "Pawn definition is available for Inspector setup.", missingText, "A pawn actor is the spawned or placed body controlled by a participant.", pawnOptional);
                }
            }
        }

        private static bool IsPyralisComponent(Component component)
        {
            if (component == null)
                return true;

            string namespaceName = component.GetType().Namespace ?? string.Empty;
            return namespaceName.StartsWith("NeonBlack.Gameplay", System.StringComparison.Ordinal);
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

        private static MessageType GetMessageType(PyralisSetupFlowStepStatus status)
        {
            return status == PyralisSetupFlowStepStatus.Missing || status == PyralisSetupFlowStepStatus.Blocked
                ? MessageType.Warning
                : MessageType.Info;
        }

        private static void DrawMissingCoreActions(
            Object selection,
            GameplaySessionBootstrap bootstrap,
            SessionDefinition session,
            GameModeDefinition mode,
            GameSetupProfile setupProfile)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Native Next Step", EditorStyles.miniBoldLabel);

            bool drewStep = false;

            if (bootstrap != null && session == null)
            {
                drewStep = true;
                DrawNativeWorkflowStep(
                    "Create a Session Definition",
                    "Project window: choose or create a setup folder for this proof, keep imported art folders separate, then right-click inside that content pane -> Create -> NeonBlack -> Definitions -> Session Definition. Then drag it into GameplaySessionBootstrap > Session Definition, or use the field's object picker circle and double-click the asset.");
            }

            if (session != null && mode == null)
            {
                drewStep = true;
                DrawNativeWorkflowStep(
                    "Create or choose a Game Mode Definition",
                    "Project window: open the setup folder so its contents are visible, then right-click inside that content pane -> Create -> NeonBlack -> Definitions -> Game Mode Definition. Then select/open the SessionDefinition asset and assign Default Game Mode by drag/drop or the field's object picker circle.");
            }

            if (mode != null && setupProfile == null)
            {
                drewStep = true;
                DrawNativeWorkflowStep(
                    "Create or choose a Game Setup Profile",
                    "Project window: open the setup folder so its contents are visible, then right-click inside that content pane -> Create -> NeonBlack -> Profiles -> Game Setup Profile. Then select/open the GameModeDefinition asset and assign Setup Profile by drag/drop or the field's object picker circle.");
            }

            if (session != null && (session.defaultParticipants == null || session.defaultParticipants.Length == 0))
            {
                drewStep = true;
                DrawNativeWorkflowStep(
                    "Create a Participant Definition",
                    "Project window: open the setup folder so its contents are visible, then right-click inside that content pane -> Create -> NeonBlack -> Definitions -> Participant Definition. Configure player/input/seat fields, then add it to SessionDefinition > Default Participants.");
            }

            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(setupProfile, session, mode);
            if (selection is ParticipantDefinition participant && participant.defaultPawn == null)
            {
                if (route.RequiresPawn)
                {
                    drewStep = true;
                    DrawNativeWorkflowStep(
                        "Create a Pawn Definition",
                        "Project window: open the setup folder so its contents are visible, then right-click inside that content pane -> Create -> NeonBlack -> Definitions -> Pawn Definition. Assign its pawn prefab, then assign the Pawn Definition into ParticipantDefinition > Default Pawn by drag/drop or the field's object picker circle.");
                }
            }

            if (!drewStep)
                EditorGUILayout.HelpBox("No obvious missing setup link for this selection. Use Inspect Asset on a service step when you need field-level editing.", MessageType.Info);
        }

        private static void DrawNativeWorkflowStep(string title, string instruction)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
                PyralisAuthoringActionSurface surface = GetWorkflowStepSurface(instruction);
                PyralisAuthoringSurfaceBeacon.DrawNativeAction(
                    new PyralisAuthoringNativeAction("Focus", surface, title, instruction, "the visible Unity surface matches the step"),
                    instruction);
            }
        }

        private static PyralisAuthoringActionSurface GetWorkflowStepSurface(string instruction)
        {
            if (string.IsNullOrWhiteSpace(instruction))
                return PyralisAuthoringActionSurface.AuthoringWindow;

            if (instruction.IndexOf("Project window", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("Create ->", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("Create -> NeonBlack", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return PyralisAuthoringActionSurface.ProjectWindow;

            if (instruction.IndexOf("Hierarchy", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("right-click -> Create Empty", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("GameObject ->", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return PyralisAuthoringActionSurface.Hierarchy;

            if (instruction.IndexOf("Play Mode", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return PyralisAuthoringActionSurface.PlayMode;

            if (instruction.IndexOf("Inspector", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("Add Component", System.StringComparison.OrdinalIgnoreCase) >= 0
                || instruction.IndexOf("object picker", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return PyralisAuthoringActionSurface.Inspector;

            return PyralisAuthoringActionSurface.AuthoringWindow;
        }

        internal static GameplaySessionBootstrap GetSelectedBootstrap(Object selection)
        {
            if (selection is GameplaySessionBootstrap bootstrap)
                return bootstrap;

            if (selection is GameObject gameObject)
                return gameObject.GetComponent<GameplaySessionBootstrap>() ?? gameObject.GetComponentInParent<GameplaySessionBootstrap>();

            if (selection is Component component)
                return component.GetComponent<GameplaySessionBootstrap>() ?? component.GetComponentInParent<GameplaySessionBootstrap>();

            return null;
        }

        private static Object ResolveActiveSetup(Object selection, Object selectionSetup, Object sceneFallbackSetup, Object pinnedActiveSetup, Object rememberedActiveSetup)
        {
            if (CanUseAsActiveSetup(pinnedActiveSetup))
                return GetSetupContext(pinnedActiveSetup);

            Object rememberedSetup = CanUseAsActiveSetup(rememberedActiveSetup)
                ? GetSetupContext(rememberedActiveSetup)
                : null;
            if (rememberedSetup != null && ShouldKeepRememberedSetupForLooseSelection(selection, rememberedSetup))
                return rememberedSetup;

            GameplaySessionBootstrap sceneBootstrap = GetOnlySceneBootstrap();
            if (sceneBootstrap != null && ShouldKeepRememberedSetupForLooseSelection(selection, sceneBootstrap))
                return sceneBootstrap;

            if (selectionSetup != null)
                return selectionSetup;

            if (sceneFallbackSetup != null)
                return sceneFallbackSetup;

            if (rememberedSetup != null)
                return rememberedSetup;

            return sceneBootstrap;
        }

        private static bool ShouldKeepRememberedSetupForLooseSelection(Object selection, Object rememberedSetup)
        {
            if (selection == null || rememberedSetup == null)
                return false;

            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(rememberedSetup);
            SessionDefinition rememberedSession = GetSelectedSession(rememberedSetup, bootstrap);
            if (selection is SessionDefinition && bootstrap != null)
                return rememberedSession == null;

            if (selection is GameModeDefinition && rememberedSession != null)
                return rememberedSession.defaultGameMode == null;

            GameModeDefinition rememberedMode = GetSelectedMode(rememberedSetup, rememberedSession);
            if (selection is GameSetupProfile && rememberedMode != null)
                return rememberedMode.setupProfile == null;

            return false;
        }

        internal static Object GetSceneFallbackSetup(Object selection, Object selectionSetup)
        {
            if (selection != null || selectionSetup != null)
                return null;

            return GetOnlySceneBootstrap();
        }

        private static bool CanUseAsActiveSetup(Object selection)
        {
            return GetSetupContext(selection) != null;
        }

        private static Object GetSetupContext(Object selection)
        {
            if (selection == null)
                return null;

            if (selection is GameplaySessionBootstrap)
                return selection;

            GameplaySessionBootstrap linkedBootstrap = GetBootstrapReferencingSelectedAsset(selection);
            if (linkedBootstrap != null)
                return linkedBootstrap;

            if (selection is SessionDefinition
                || selection is GameModeDefinition
                || selection is GameSetupProfile)
                return selection;

            GameplaySessionBootstrap bootstrap = GetSelectedBootstrap(selection);
            if (bootstrap != null)
                return bootstrap;

            GameplaySessionBootstrap referencedBootstrap = GetBootstrapReferencingSelectedTransform(selection);
            if (referencedBootstrap != null)
                return referencedBootstrap;

            return null;
        }

        private static GameplaySessionBootstrap GetBootstrapReferencingSelectedAsset(Object selection)
        {
            if (selection == null)
                return null;

            foreach (GameplaySessionBootstrap bootstrap in Object.FindObjectsByType<GameplaySessionBootstrap>(FindObjectsInactive.Include))
            {
                if (bootstrap == null || !bootstrap.gameObject.scene.IsValid() || !bootstrap.gameObject.scene.isLoaded)
                    continue;

                SessionDefinition session = GetSelectedSession(null, bootstrap);
                if (SessionReferencesSelection(session, selection))
                    return bootstrap;
            }

            return null;
        }

        private static bool SessionReferencesSelection(SessionDefinition session, Object selection)
        {
            if (session == null)
                return false;

            if (selection == session
                || selection == session.defaultGameMode
                || selection == session.defaultInputProfile
                || selection == session.settingsProfile)
                return true;

            if (GameModeReferencesSelection(session.defaultGameMode, selection))
                return true;

            if (session.defaultParticipants == null)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (ParticipantReferencesSelection(participant, selection))
                    return true;
            }

            return false;
        }

        private static bool GameModeReferencesSelection(GameModeDefinition mode, Object selection)
        {
            if (mode == null)
                return false;

            if (selection == mode
                || selection == mode.setupProfile
                || selection == mode.playfieldProfile
                || selection == mode.cameraRigProfile
                || selection == mode.turnOrderDefinition
                || selection == mode.boardDefinition)
                return true;

            if (SetupProfileReferencesSelection(mode.setupProfile, selection))
                return true;

            if (mode.requiredFeatureModules != null)
            {
                for (int i = 0; i < mode.requiredFeatureModules.Length; i++)
                {
                    if (selection == mode.requiredFeatureModules[i])
                        return true;
                }
            }

            if (mode.boardTerminalConditions != null)
            {
                for (int i = 0; i < mode.boardTerminalConditions.Length; i++)
                {
                    if (selection == mode.boardTerminalConditions[i])
                        return true;
                }
            }

            return false;
        }

        private static bool SetupProfileReferencesSelection(GameSetupProfile setupProfile, Object selection)
        {
            if (setupProfile == null)
                return false;

            if (selection == setupProfile)
                return true;

            if (setupProfile.runtimePatterns == null)
                return false;

            for (int i = 0; i < setupProfile.runtimePatterns.Length; i++)
            {
                if (selection == setupProfile.runtimePatterns[i])
                    return true;
            }

            return false;
        }

        private static bool ParticipantReferencesSelection(ParticipantDefinition participant, Object selection)
        {
            if (participant == null)
                return false;

            if (selection == participant
                || selection == participant.defaultPawn
                || selection == participant.inputProfile)
                return true;

            PawnDefinition pawn = participant.defaultPawn;
            return pawn != null
                && (selection == pawn.pawnPrefab
                    || selection == pawn.defaultInputProfile
                    || selection == pawn.movementProfile
                    || selection == pawn.combatProfile
                    || selection == pawn.traversalProfile
                    || selection == pawn.presentationProfile
                    || selection == pawn.animationProfile
                    || PawnReferencesFeatureModule(pawn, selection));
        }

        private static bool PawnReferencesFeatureModule(PawnDefinition pawn, Object selection)
        {
            if (pawn == null || pawn.featureModules == null)
                return false;

            for (int i = 0; i < pawn.featureModules.Length; i++)
            {
                if (selection == pawn.featureModules[i])
                    return true;
            }

            return false;
        }

        private static GameplaySessionBootstrap GetBootstrapReferencingSelectedTransform(Object selection)
        {
            Transform selectedTransform = GetSelectedTransform(selection);
            if (selectedTransform == null)
                return null;

            foreach (GameplaySessionBootstrap bootstrap in Object.FindObjectsByType<GameplaySessionBootstrap>(FindObjectsInactive.Include))
            {
                SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
                SerializedProperty spawnPoints = serializedBootstrap.FindProperty("spawnPoints");
                if (spawnPoints == null || !spawnPoints.isArray)
                    continue;

                for (int i = 0; i < spawnPoints.arraySize; i++)
                {
                    if (spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue == selectedTransform)
                        return bootstrap;
                }
            }

            return null;
        }

        private static Transform GetSelectedTransform(Object selection)
        {
            if (selection is GameObject gameObject)
                return gameObject.transform;

            if (selection is Component component)
                return component.transform;

            return null;
        }

        private static bool IsSceneSupportObject(GameObject gameObject)
        {
            return gameObject.GetComponent<UnityEngine.Camera>() != null
                || gameObject.GetComponent<Light>() != null
                || gameObject.GetComponentInParent<UnityEngine.Camera>() != null;
        }

        private static GameplaySessionBootstrap GetOnlySceneBootstrap()
        {
            GameplaySessionBootstrap onlyBootstrap = null;
            foreach (GameplaySessionBootstrap bootstrap in Object.FindObjectsByType<GameplaySessionBootstrap>(FindObjectsInactive.Include))
            {
                if (bootstrap == null || !bootstrap.gameObject.scene.IsValid() || !bootstrap.gameObject.scene.isLoaded)
                    continue;

                if (onlyBootstrap != null)
                    return null;

                onlyBootstrap = bootstrap;
            }

            return onlyBootstrap;
        }

        internal static SessionDefinition GetSelectedSession(Object selection, GameplaySessionBootstrap bootstrap)
        {
            if (selection is SessionDefinition session)
                return session;

            if (bootstrap == null)
                return null;

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            return serializedBootstrap.FindProperty("sessionDefinition")?.objectReferenceValue as SessionDefinition;
        }

        internal static GameModeDefinition GetSelectedMode(Object selection, SessionDefinition session)
        {
            if (selection is GameModeDefinition mode)
                return mode;

            return session != null ? session.defaultGameMode : null;
        }

        internal static GameSetupProfile GetSelectedSetupProfile(Object selection, GameModeDefinition mode)
        {
            if (selection is GameSetupProfile setupProfile)
                return setupProfile;

            return mode != null ? mode.setupProfile : null;
        }

        private static bool HasAnyPawn(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant != null && participant.defaultPawn != null)
                    return true;
            }

            return false;
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

        private static string GetRuntimePatternLabel(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return "(Missing)";

            if (!string.IsNullOrWhiteSpace(pattern.displayName))
                return pattern.displayName;

            return !string.IsNullOrWhiteSpace(pattern.patternId) ? pattern.patternId : pattern.name;
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

        private static string GetCapabilityLabel(RuntimeCapabilityFamily family)
        {
            return family switch
            {
                RuntimeCapabilityFamily.CharacterPawnGameplay => "Pawn actor",
                RuntimeCapabilityFamily.Combat => "Combat",
                RuntimeCapabilityFamily.GunsProjectiles => "Projectiles",
                RuntimeCapabilityFamily.ActionTargeting => "Action/menu selection",
                RuntimeCapabilityFamily.BoardCardTabletop => "Board/card/tabletop",
                RuntimeCapabilityFamily.CameraInput => "Camera/cursor",
                RuntimeCapabilityFamily.AnimationPresentation => "Animation/presentation",
                RuntimeCapabilityFamily.ScoringObjectives => "Scoring/objectives",
                RuntimeCapabilityFamily.ProceduralGeneration => "Procedural",
                RuntimeCapabilityFamily.Networking => "Networking",
                _ => family.ToString()
            };
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

        private static string GetLaneTagLabel(RuntimeCapabilityLaneTag tag)
        {
            return tag switch
            {
                RuntimeCapabilityLaneTag.Sprite2D => "Sprite2D",
                RuntimeCapabilityLaneTag.Billboard2_5D => "Billboard2_5D",
                RuntimeCapabilityLaneTag.ThirdPerson3D => "Rigged3D",
                RuntimeCapabilityLaneTag.TabletopBoard => "Tabletop / No Pawn",
                RuntimeCapabilityLaneTag.UiMenuOnly => "UI / Menu",
                RuntimeCapabilityLaneTag.CameraCursor => "Camera / Cursor",
                RuntimeCapabilityLaneTag.Mixed => "Networked",
                _ => tag.ToString()
            };
        }

        private static string GetCapabilityDescription(RuntimeCapabilityFamily family)
        {
            return family switch
            {
                RuntimeCapabilityFamily.CharacterPawnGameplay => "Use when participants need actor bodies, movement, spawn points, pawn definitions, and pawn prefabs.",
                RuntimeCapabilityFamily.Combat => "Use for hitboxes, hurtboxes, damage, health, reactions, brawler moves, fighter moves, or combat sequences.",
                RuntimeCapabilityFamily.GunsProjectiles => "Use for bullets, spells, traps, turrets, hitscan, fire modes, ammo, and impact feedback.",
                RuntimeCapabilityFamily.ActionTargeting => "Use when players choose commands through menus, turns, cards, board spaces, cursors, or queued abilities.",
                RuntimeCapabilityFamily.BoardCardTabletop => "Use for seats, hands, pieces, zones, legal moves, card/board state, turn order, and no-pawn tabletop flow.",
                RuntimeCapabilityFamily.CameraInput => "Use when the player controls a camera, cursor, selector, commander view, or other non-pawn surface.",
                RuntimeCapabilityFamily.AnimationPresentation => "Use for Sprite2D, Billboard2_5D, Rigged3D, Animator signals, facing, shadows, and visual feedback.",
                RuntimeCapabilityFamily.ScoringObjectives => "Use for score, timers, lives, resources, objectives, win/loss state, round results, or victory points.",
                RuntimeCapabilityFamily.ProceduralGeneration => "Use for generated rooms, chunks, lanes, waves, board layouts, encounters, budgets, and seeds.",
                RuntimeCapabilityFamily.Networking => "Use after the local route works and the scene needs ownership, authority, transport, or network prefab readiness.",
                _ => "Use for a custom setup capability. Make sure its pattern description and setup notes explain the Unity wiring."
            };
        }

        private static void DrawRouteReport(PyralisAuthoringRouteReport report)
        {
            EditorGUILayout.LabelField(GuidedSetupRouteLabel, EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Route", report.RouteName);
                EditorGUILayout.LabelField("Next Step", report.NextStep, EditorStyles.wordWrappedLabel);
                EditorGUILayout.HelpBox(report.RouteGuidance, MessageType.Info);
            }
        }
    }
}
