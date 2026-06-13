using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    public partial class PyralisAuthoringWindow
    {
        private bool _intentHasUnappliedSetupChanges;

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

            var intentContract = new Label("Intent shapes the game you want. It does not apply presets, create assets, wire scenes, or choose art/feel for you.");
            intentContract.style.whiteSpace = WhiteSpace.Normal;
            intentContract.style.opacity = 0.75f;
            intentContract.style.marginBottom = 6f;
            advisorContainer.Add(intentContract);

            var intentSummary = new Label("Project DNA is defined by... Engine Spine capabilities: ...") { name = "intentSummary" };
            intentSummary.AddToClassList("intent-card-summary");
            advisorContainer.Add(intentSummary);

            var intentNext = new Label(string.Empty) { name = "intentNext" };
            intentNext.style.whiteSpace = WhiteSpace.Normal;
            intentNext.style.marginTop = 6f;
            intentNext.style.marginBottom = 6f;
            advisorContainer.Add(intentNext);

            var actionRow = new VisualElement { name = "intentActionRow" };
            actionRow.style.flexDirection = FlexDirection.Row;
            actionRow.style.marginTop = 4f;
            var guideButton = new Button(() => SwitchMode(AuthoringWindowMode.Guide)) { text = "Open Guide" };
            guideButton.tooltip = "Show the graph-filtered route guide for this intent without applying a preset.";
            var overviewButton = new Button(() => SwitchMode(AuthoringWindowMode.Overview)) { text = "Open Overview" };
            overviewButton.tooltip = "Return to the current setup route once a scene root or setup asset exists.";
            var applyButton = new Button(ApplyIntentToActiveSetupProfile) { text = "Apply To Setup Profile", name = "applyIntentToSetupProfile" };
            applyButton.tooltip = "Write the selected intent capability families into the active GameSetupProfile runtime capability rows.";
            applyButton.SetEnabled(GetActiveIntentSetupProfile() != null && _intentCapabilities != AuthoringCapability.None);
            actionRow.Add(applyButton);
            actionRow.Add(guideButton);
            actionRow.Add(overviewButton);
            advisorContainer.Add(actionRow);

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
            dropdown.tooltip = selectedIndex > 0
                ? AuthoringWorldAxiomRegistry.GetTooltip(options[selectedIndex - 1])
                : "No mechanical axiom selected for this category.";

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

                MarkIntentSetupChangesPending();
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
                MarkIntentSetupChangesPending();
                InvalidateAuthoringCache();
                UpdateAdvisor(rootVisualElement);
            });
            container.Add(dropdown);
        }

        private void PopulateCapabilities(VisualElement container)
        {
            if (container == null) return;

            var searchField = new ToolbarSearchField();
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
                var foldout = new Foldout
                {
                    text = group.Key,
                    value = group.Value.foldout
                };
                foldout.AddToClassList("capability-group-foldout");

                string key = group.Key;
                foldout.RegisterValueChangedCallback(evt =>
                {
                    if (key == "Core & Shell") _coreFoldout = evt.newValue;
                    else if (key == "Actor & Action") _actorFoldout = evt.newValue;
                    else if (key == "Strategy & Progression") _strategyFoldout = evt.newValue;
                    else if (key == "World & Meta") _worldFoldout = evt.newValue;
                });

                foreach (AuthoringCapability cap in group.Value.caps)
                {
                    var toggle = new Toggle(AuthoringCapabilityRegistry.GetDisplayName(cap));
                    toggle.name = "cap_" + cap.ToString();
                    toggle.value = (_intentCapabilities & cap) != 0;
                    toggle.tooltip = AuthoringCapabilityRegistry.GetTooltip(cap);
                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        if (evt.newValue) _intentCapabilities |= cap;
                        else _intentCapabilities &= ~cap;
                        MarkIntentSetupChangesPending();
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

            foreach (VisualElement element in grid.Children())
            {
                if (element is not Foldout foldout)
                    continue;

                int visibleToggles = 0;
                foreach (VisualElement child in foldout.contentContainer.Children())
                {
                    if (child is not Toggle toggle)
                        continue;

                    bool matches = !hasFilter || toggle.label.ToLowerInvariant().Contains(filter);
                    toggle.style.display = matches ? DisplayStyle.Flex : DisplayStyle.None;
                    if (matches)
                        visibleToggles++;
                }

                foldout.style.display = visibleToggles > 0 || !hasFilter ? DisplayStyle.Flex : DisplayStyle.None;
                if (hasFilter && visibleToggles > 0)
                    foldout.value = true;
            }
        }

        private void UpdateAdvisor(VisualElement root)
        {
            Label summaryLabel = root.Q<Label>("intentSummary");
            if (summaryLabel == null) return;

            var selection = new PyralisAuthoringIntentSelection(_intentLane, _intentCapabilities, _intentAxioms);
            PyralisAuthoringIntentModel model = PyralisAuthoringIntentAdvisor.Build(selection);

            summaryLabel.text = model.Summary;
            Label nextLabel = root.Q<Label>("intentNext");
            if (nextLabel != null)
                nextLabel.text = GetIntentReadinessMessage();

            Button applyButton = root.Q<Button>("applyIntentToSetupProfile");
            if (applyButton != null)
                applyButton.SetEnabled(GetActiveIntentSetupProfile() != null && _intentCapabilities != AuthoringCapability.None);
        }

        private string GetIntentReadinessMessage()
        {
            if (!HasCompleteCoreAxioms())
                return "Choose the DNA axioms first: dimensionality, physics gravity, sequence timeline, and spatial topology. Then choose the Engine Spine capabilities for this proof.";

            if (_intentCapabilities == AuthoringCapability.None)
                return "Choose Engine Spine capabilities that describe the game. The active GameSetupProfile will store matching runtime capability rows for Overview, Guide, Map, and Validate.";

            GameSetupProfile setupProfile = GetActiveIntentSetupProfile();
            if (setupProfile == null)
                return "Intent is shaped. Create or select a GameSetupProfile so these choices become the route contract read by Overview, Guide, Map, and Validate.";

            if (_intentHasUnappliedSetupChanges)
                return $"Intent is shaped. Click Apply To Setup Profile to update `{setupProfile.name}` runtime capability rows, then open Guide for the graph-filtered route path.";

            return $"Intent matches the active GameSetupProfile `{setupProfile.name}`. Open Guide for the graph-filtered route path, then use Project, Hierarchy, and Inspector to create and wire your own setup.";
        }

        private bool HasCompleteCoreAxioms()
        {
            return (_intentAxioms & (AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Dimensions3D)) != 0
                && (_intentAxioms & (AuthoringWorldAxiom.GravityVertical | AuthoringWorldAxiom.GravityRadial | AuthoringWorldAxiom.GravityNone)) != 0
                && (_intentAxioms & (AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.TurnBased)) != 0
                && (_intentAxioms & (AuthoringWorldAxiom.BoundedSpace | AuthoringWorldAxiom.WrappedSpace | AuthoringWorldAxiom.InfiniteSpace)) != 0;
        }

        private GameSetupProfile GetActiveIntentSetupProfile()
        {
            Object selection = Selection.activeObject;
            Object selectionSetup = PyralisAuthoringSetupContextResolver.GetSetupContext(selection);
            Object sceneFallbackSetup = PyralisAuthoringSetupContextResolver.GetSceneFallbackSetup(selection, selectionSetup);
            Object activeSetup = PyralisAuthoringSetupContextResolver.ResolveActiveSetup(selection, selectionSetup, sceneFallbackSetup, _pinnedActiveSetup, _lastActiveSetup);
            return selection as GameSetupProfile
                ?? PyralisAuthoringSetupContextResolver.GetSelectedSetupProfile(activeSetup, PyralisAuthoringSetupContextResolver.GetSelectedMode(activeSetup, PyralisAuthoringSetupContextResolver.GetSelectedSession(activeSetup, PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(activeSetup))));
        }

        private void MarkIntentSetupChangesPending()
        {
            _intentHasUnappliedSetupChanges = true;
        }

        private void ApplyIntentToActiveSetupProfile()
        {
            if (_intentCapabilities == AuthoringCapability.None)
                return;

            GameSetupProfile setupProfile = GetActiveIntentSetupProfile();
            if (setupProfile == null)
                return;

            RuntimeCapabilityFamily[] families = PyralisIntentCapabilityProjection.BuildRuntimeFamilies(_intentCapabilities, _intentLane, _intentAxioms);
            if (families.Length == 0)
                return;

            Undo.RecordObject(setupProfile, "Sync Intent To Setup Profile");
            List<RuntimeCapabilitySelection> next = new List<RuntimeCapabilitySelection>();

            for (int i = 0; i < families.Length; i++)
            {
                RuntimeCapabilityFamily family = families[i];
                RuntimeCapabilitySelection existing = PyralisRuntimeCapabilityCatalogRenderer.GetCapabilitySelection(setupProfile, family);
                next.Add(new RuntimeCapabilitySelection
                {
                    capabilityFamily = family,
                    patternDefinition = existing?.patternDefinition,
                    requiredForFirstProof = existing?.requiredForFirstProof ?? true
                });
            }

            setupProfile.runtimeCapabilities = next.ToArray();
            setupProfile.runtimePatterns = PyralisIntentCapabilityProjection.FilterRuntimePatternsToFamilies(setupProfile.runtimePatterns, families);
            EditorUtility.SetDirty(setupProfile);
            _intentHasUnappliedSetupChanges = false;
            InvalidateAuthoringCache();
            UpdateAdvisor(rootVisualElement);
        }
    }
}
