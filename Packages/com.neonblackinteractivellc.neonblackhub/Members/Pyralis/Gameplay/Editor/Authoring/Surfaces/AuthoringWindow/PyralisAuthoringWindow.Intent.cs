using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
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
        private readonly Dictionary<string, bool> _capabilityGroupFoldouts = new Dictionary<string, bool>(StringComparer.Ordinal);

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

            var participantContainer = new VisualElement() { name = "participantIntentContainer" };
            participantContainer.AddToClassList("section");
            var participantTitle = new Label("PARTICIPANTS");
            participantTitle.AddToClassList("section-title");
            participantContainer.Add(participantTitle);
            PopulateParticipantRoute(participantContainer);

            var capabilityContainer = new VisualElement() { name = "capabilityContainer" };
            capabilityContainer.AddToClassList("section");
            var capTitle = new Label("CAPABILITY INGREDIENTS");
            capTitle.AddToClassList("section-title");
            capabilityContainer.Add(capTitle);
            var capHelp = new Label("Toggle the gameplay ingredients this route needs. These are not presets; Intent filters the graph while gameplay setup stays in native Unity assets and scene objects.");
            capHelp.style.whiteSpace = WhiteSpace.Normal;
            capHelp.style.opacity = 0.75f;
            capHelp.style.marginBottom = 6f;
            capabilityContainer.Add(capHelp);
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

            var intentSummary = new Label("Project DNA is defined by... capability ingredients: ...") { name = "intentSummary" };
            intentSummary.AddToClassList("intent-card-summary");
            advisorContainer.Add(intentSummary);

            var routeShape = new Label(string.Empty) { name = "intentRouteShape" };
            routeShape.style.whiteSpace = WhiteSpace.Normal;
            routeShape.style.marginTop = 6f;
            routeShape.style.marginBottom = 2f;
            advisorContainer.Add(routeShape);

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
            var exportButton = new Button(() =>
            {
                PyralisAuthoringGraphJsonExportControl.ExportIntentSnapshot(
                    GetCurrentIntentSelection(),
                    GetCachedIntentModel(),
                    PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(_intentLane, _intentAxioms));
            })
            { text = "Export Intent JSON" };
            exportButton.tooltip = "Write the Intent tab steering snapshot: DNA axioms, presentation lane, participant route, capability descriptors, selected ingredients, and advisor rows. It does not export scene/setup reality.";
            actionRow.Add(guideButton);
            actionRow.Add(overviewButton);
            actionRow.Add(exportButton);
            advisorContainer.Add(actionRow);

            var sidebar = new VisualElement() { name = "sidebar" };
            sidebar.AddToClassList("intent-sidebar");
            sidebar.Add(axiomContainer);
            sidebar.Add(laneContainer);
            sidebar.Add(participantContainer);

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

            IReadOnlyList<PyralisAuthoringAxiomGroup> groups = PyralisAuthoringVocabulary.GetAxiomGroups();
            for (int i = 0; i < groups.Count; i++)
            {
                PyralisAuthoringAxiomGroup group = groups[i];
                AddAxiomDropdown(container, group.DisplayName, group.Mask, group.Options);
            }
        }

        private void AddAxiomDropdown(VisualElement container, string label, AuthoringWorldAxiom mask, params AuthoringWorldAxiom[] options)
        {
            List<string> choices = new List<string> { "None" };
            int selectedIndex = 0;
            AuthoringWorldAxiom current = _intentAxioms & mask;

            for (int i = 0; i < options.Length; i++)
            {
                choices.Add(PyralisAuthoringVocabulary.GetAxiomDisplayName(options[i]));
                if (current == options[i])
                    selectedIndex = i + 1;
            }

            var dropdown = new DropdownField(label, choices, selectedIndex);
            dropdown.tooltip = selectedIndex > 0
                ? PyralisAuthoringVocabulary.GetAxiomTooltip(options[selectedIndex - 1])
                : "No mechanical axiom selected for this category.";

            dropdown.RegisterValueChangedCallback(evt =>
            {
                int index = dropdown.index;
                _intentAxioms &= ~mask;
                if (index > 0)
                {
                    AuthoringWorldAxiom selected = options[index - 1];
                    _intentAxioms |= selected;
                    dropdown.tooltip = PyralisAuthoringVocabulary.GetAxiomTooltip(selected);
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

        private void PopulateParticipantRoute(VisualElement container)
        {
            if (container == null)
                return;

            PyralisIntentParticipantRoute[] options =
            {
                PyralisIntentParticipantRoute.InferFromSetup,
                PyralisIntentParticipantRoute.SoloLocal,
                PyralisIntentParticipantRoute.TwoLocalPlayers,
                PyralisIntentParticipantRoute.ThreeLocalPlayers,
                PyralisIntentParticipantRoute.FourLocalPlayers,
                PyralisIntentParticipantRoute.Networked,
                PyralisIntentParticipantRoute.HybridLocalNetworked
            };
            List<string> choices = new List<string>();
            int selectedIndex = 0;
            for (int i = 0; i < options.Length; i++)
            {
                choices.Add(GetParticipantRouteDisplayName(options[i]));
                if (_intentParticipantRoute == options[i])
                    selectedIndex = i;
            }

            var dropdown = new DropdownField("Route Shape", choices, selectedIndex);
            dropdown.tooltip = GetParticipantRouteTooltip(_intentParticipantRoute);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                _intentParticipantRoute = options[Math.Max(0, dropdown.index)];
                dropdown.tooltip = GetParticipantRouteTooltip(_intentParticipantRoute);
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

            Dictionary<string, List<PyralisAuthoringCapabilityDescriptor>> groups = BuildIntentDescriptorGroups();
            HashSet<string> selectedIds = new HashSet<string>(GetSelectedIntentDescriptorIds(), StringComparer.Ordinal);
            foreach (var group in groups)
            {
                var foldout = new Foldout
                {
                    text = group.Key,
                    value = GetCapabilityGroupFoldout(group.Key)
                };
                foldout.AddToClassList("capability-group-foldout");

                string key = group.Key;
                foldout.RegisterValueChangedCallback(evt =>
                {
                    SetCapabilityGroupFoldout(key, evt.newValue);
                });

                Dictionary<string, List<PyralisAuthoringCapabilityDescriptor>> subgroups =
                    BuildIntentDescriptorSubgroups(group.Value);
                foreach (var subgroup in subgroups)
                {
                    if (string.IsNullOrWhiteSpace(subgroup.Key))
                    {
                        AddIntentDescriptorToggles(foldout, subgroup.Value, selectedIds);
                        continue;
                    }

                    var subgroupFoldout = new Foldout
                    {
                        text = subgroup.Key,
                        value = GetCapabilityGroupFoldout(group.Key + "/" + subgroup.Key)
                    };
                    subgroupFoldout.AddToClassList("capability-subgroup-foldout");
                    string subgroupKey = group.Key + "/" + subgroup.Key;
                    subgroupFoldout.RegisterValueChangedCallback(evt =>
                    {
                        SetCapabilityGroupFoldout(subgroupKey, evt.newValue);
                    });
                    AddIntentDescriptorToggles(subgroupFoldout, subgroup.Value, selectedIds);
                    foldout.Add(subgroupFoldout);
                }

                grid.Add(foldout);
            }
        }

        private void AddIntentDescriptorToggles(
            VisualElement parent,
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors,
            HashSet<string> selectedIds)
        {
            if (parent == null || descriptors == null)
                return;

            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = descriptors[i];
                if (descriptor == null)
                    continue;

                var toggle = new Toggle(GetIntentDescriptorLeafLabel(descriptor));
                toggle.name = "cap_" + descriptor.StableId.Replace('.', '_').Replace('/', '_');
                toggle.value = selectedIds.Contains(descriptor.StableId);
                toggle.tooltip = GetIntentDescriptorTooltip(descriptor);
                toggle.RegisterValueChangedCallback(evt =>
                {
                    UpdateSelectedIntentDescriptor(descriptor.StableId, evt.newValue);
                    MarkIntentSetupChangesPending();
                    InvalidateAuthoringCache();
                    UpdateAdvisor(rootVisualElement);
                });
                parent.Add(toggle);
            }
        }

        private Dictionary<string, List<PyralisAuthoringCapabilityDescriptor>> BuildIntentDescriptorGroups()
        {
            Dictionary<string, List<PyralisAuthoringCapabilityDescriptor>> groups =
                new Dictionary<string, List<PyralisAuthoringCapabilityDescriptor>>(StringComparer.Ordinal);
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(_intentLane, _intentAxioms);

            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = descriptors[i];
                if (descriptor == null)
                    continue;

                string group = GetIntentDescriptorGroup(descriptor);
                if (!groups.TryGetValue(group, out List<PyralisAuthoringCapabilityDescriptor> groupDescriptors))
                {
                    groupDescriptors = new List<PyralisAuthoringCapabilityDescriptor>();
                    groups.Add(group, groupDescriptors);
                }

                groupDescriptors.Add(descriptor);
            }

            foreach (List<PyralisAuthoringCapabilityDescriptor> groupDescriptors in groups.Values)
            {
                groupDescriptors.Sort((left, right) =>
                {
                    int orderCompare = left.SortOrder.CompareTo(right.SortOrder);
                    return orderCompare != 0
                        ? orderCompare
                        : string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal);
                });
            }

            return groups;
        }

        private static string GetIntentDescriptorGroup(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return "Shared Ingredients";

            if (!string.IsNullOrWhiteSpace(descriptor.CapabilityPath))
            {
                string[] parts = descriptor.CapabilityPath.Split('/');
                if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[0]))
                    return parts[0].Trim();
            }

            return string.IsNullOrWhiteSpace(descriptor.Group) ? "Shared Ingredients" : descriptor.Group;
        }

        private static Dictionary<string, List<PyralisAuthoringCapabilityDescriptor>> BuildIntentDescriptorSubgroups(
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors)
        {
            Dictionary<string, List<PyralisAuthoringCapabilityDescriptor>> subgroups =
                new Dictionary<string, List<PyralisAuthoringCapabilityDescriptor>>(StringComparer.Ordinal);
            if (descriptors == null)
                return subgroups;

            for (int i = 0; i < descriptors.Count; i++)
            {
                PyralisAuthoringCapabilityDescriptor descriptor = descriptors[i];
                if (descriptor == null)
                    continue;

                string subgroup = GetIntentDescriptorSubgroup(descriptor);
                if (!subgroups.TryGetValue(subgroup, out List<PyralisAuthoringCapabilityDescriptor> subgroupDescriptors))
                {
                    subgroupDescriptors = new List<PyralisAuthoringCapabilityDescriptor>();
                    subgroups.Add(subgroup, subgroupDescriptors);
                }

                subgroupDescriptors.Add(descriptor);
            }

            return subgroups;
        }

        private static string GetIntentDescriptorSubgroup(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null || string.IsNullOrWhiteSpace(descriptor.CapabilityPath))
                return string.Empty;

            string[] parts = descriptor.CapabilityPath.Split('/');
            return parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[1])
                ? parts[1].Trim()
                : string.Empty;
        }

        private static string GetIntentDescriptorLabel(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return "Unknown";

            if (string.IsNullOrWhiteSpace(descriptor.CapabilityPath))
                return descriptor.DisplayName;

            string[] parts = descriptor.CapabilityPath.Split('/');
            if (parts.Length <= 1)
                return descriptor.DisplayName;

            List<string> labels = new List<string>();
            for (int i = 1; i < parts.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(parts[i]))
                    labels.Add(parts[i].Trim());
            }

            return labels.Count > 0
                ? string.Join(" / ", labels)
                : descriptor.DisplayName;
        }

        private static string GetIntentDescriptorLeafLabel(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return "Unknown";

            if (string.IsNullOrWhiteSpace(descriptor.CapabilityPath))
                return descriptor.DisplayName;

            string[] parts = descriptor.CapabilityPath.Split('/');
            if (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]))
                return string.Join(" / ", GetNonEmptyPathParts(parts, 2));
            if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                return parts[1].Trim();

            return descriptor.DisplayName;
        }

        private static string[] GetNonEmptyPathParts(string[] parts, int startIndex)
        {
            List<string> labels = new List<string>();
            if (parts == null)
                return labels.ToArray();

            for (int i = startIndex; i < parts.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(parts[i]))
                    labels.Add(parts[i].Trim());
            }

            return labels.ToArray();
        }

        private string GetIntentDescriptorTooltip(PyralisAuthoringCapabilityDescriptor descriptor)
        {
            if (descriptor == null)
                return string.Empty;

            string baseTooltip = PyralisAuthoringVocabulary.GetCapabilityTooltip(descriptor.Capability);
            if (!string.IsNullOrWhiteSpace(descriptor.Summary))
                return baseTooltip + "\n\nGraph match: " + descriptor.DisplayName + "\n" + descriptor.Summary;

            return baseTooltip + "\n\nGraph match: " + descriptor.DisplayName + ".";
        }

        private string[] GetSelectedIntentDescriptorIds()
        {
            if (string.IsNullOrWhiteSpace(_intentDescriptorIdsValue))
                return Array.Empty<string>();

            string[] values = _intentDescriptorIdsValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> cleaned = new List<string>();
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i]?.Trim();
                if (!string.IsNullOrWhiteSpace(value) && !cleaned.Contains(value))
                    cleaned.Add(value);
            }

            return cleaned.ToArray();
        }

        private void UpdateSelectedIntentDescriptor(string descriptorId, bool selected)
        {
            if (string.IsNullOrWhiteSpace(descriptorId))
                return;

            HashSet<string> ids = new HashSet<string>(GetSelectedIntentDescriptorIds(), StringComparer.Ordinal);
            if (selected)
                ids.Add(descriptorId);
            else
                ids.Remove(descriptorId);

            List<string> ordered = new List<string>(ids);
            ordered.Sort(StringComparer.Ordinal);
            _intentDescriptorIdsValue = string.Join(";", ordered);
            _intentCapabilities = PyralisAuthoringCapabilityDescriptorRegistry.BuildCapabilitiesForDescriptors(ordered);
        }

        private bool GetCapabilityGroupFoldout(string group)
        {
            return string.IsNullOrWhiteSpace(group)
                || !_capabilityGroupFoldouts.TryGetValue(group, out bool expanded)
                || expanded;
        }

        private void SetCapabilityGroupFoldout(string group, bool value)
        {
            if (!string.IsNullOrWhiteSpace(group))
                _capabilityGroupFoldouts[group] = value;
        }

        private static string GetParticipantRouteDisplayName(PyralisIntentParticipantRoute route)
        {
            switch (route)
            {
                case PyralisIntentParticipantRoute.SoloLocal:
                    return "Solo Local";
                case PyralisIntentParticipantRoute.TwoLocalPlayers:
                    return "2 Local Players";
                case PyralisIntentParticipantRoute.ThreeLocalPlayers:
                    return "3 Local Players";
                case PyralisIntentParticipantRoute.FourLocalPlayers:
                    return "4 Local Players";
                case PyralisIntentParticipantRoute.Networked:
                    return "Networked";
                case PyralisIntentParticipantRoute.HybridLocalNetworked:
                    return "Hybrid Local + Network";
                default:
                    return "Infer From Setup";
            }
        }

        private static string GetParticipantRouteTooltip(PyralisIntentParticipantRoute route)
        {
            switch (route)
            {
                case PyralisIntentParticipantRoute.SoloLocal:
                    return "Preview a one-participant local route. Actual setup still comes from SessionDefinition and ParticipantDefinition.";
                case PyralisIntentParticipantRoute.TwoLocalPlayers:
                case PyralisIntentParticipantRoute.ThreeLocalPlayers:
                case PyralisIntentParticipantRoute.FourLocalPlayers:
                    return "Preview a local join route. Guide will expect ParticipantDefinition seats plus Unity PlayerInputManager pairing.";
                case PyralisIntentParticipantRoute.Networked:
                    return "Preview a network-authority route without assuming local PlayerInputManager pairing.";
                case PyralisIntentParticipantRoute.HybridLocalNetworked:
                    return "Preview a route that needs both local device pairing and network authority validation.";
                default:
                    return "Let the graph infer participant topology from the authored session, participants, input router, and spawn service.";
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

                int visibleToggles = FilterCapabilityElement(foldout.contentContainer, filter, hasFilter);

                foldout.style.display = visibleToggles > 0 || !hasFilter ? DisplayStyle.Flex : DisplayStyle.None;
                if (hasFilter && visibleToggles > 0)
                    foldout.value = true;
            }
        }

        private static int FilterCapabilityElement(VisualElement parent, string filter, bool hasFilter)
        {
            if (parent == null)
                return 0;

            int visibleToggles = 0;
            foreach (VisualElement child in parent.Children())
            {
                if (child is Toggle toggle)
                {
                    bool matches = !hasFilter || toggle.label.ToLowerInvariant().Contains(filter);
                    toggle.style.display = matches ? DisplayStyle.Flex : DisplayStyle.None;
                    if (matches)
                        visibleToggles++;
                    continue;
                }

                if (child is Foldout foldout)
                {
                    int nestedVisible = FilterCapabilityElement(foldout.contentContainer, filter, hasFilter);
                    foldout.style.display = nestedVisible > 0 || !hasFilter ? DisplayStyle.Flex : DisplayStyle.None;
                    if (hasFilter && nestedVisible > 0)
                        foldout.value = true;
                    visibleToggles += nestedVisible;
                }
            }

            return visibleToggles;
        }

        private void UpdateAdvisor(VisualElement root)
        {
            Label summaryLabel = root.Q<Label>("intentSummary");
            if (summaryLabel == null) return;

            PyralisAuthoringIntentModel model = GetCachedIntentModel();

            summaryLabel.text = model.Summary;
            Label routeShapeLabel = root.Q<Label>("intentRouteShape");
            if (routeShapeLabel != null)
                routeShapeLabel.text = PyralisAuthoringSetupGraphProjection.BuildRouteShapeSummary(GetCurrentIntentSelection());

            Label nextLabel = root.Q<Label>("intentNext");
            if (nextLabel != null)
                nextLabel.text = GetIntentReadinessMessage();

        }

        private string GetIntentReadinessMessage()
        {
            if (!HasCompleteCoreAxioms())
                return "Choose the DNA axioms first: dimensionality, physics gravity, sequence timeline, and spatial topology. Then choose the capability ingredients for this proof.";

            if (_intentCapabilities == AuthoringCapability.None)
                return "Choose capability ingredients that describe the game. Intent filters the graph; gameplay setup stays in native Unity assets and scene objects.";

            if (_intentHasUnappliedSetupChanges)
                return "Intent is shaped. Open Guide for the graph-filtered route path, then create or wire the Unity assets the graph marks missing.";

            return "Intent is shaped. Open Guide for the graph-filtered route path, then use Project, Hierarchy, and Inspector to create and wire your own setup.";
        }

        private bool HasCompleteCoreAxioms()
        {
            return PyralisAuthoringVocabulary.HasCompleteCoreAxioms(_intentAxioms);
        }

        private void MarkIntentSetupChangesPending()
        {
            _intentHasUnappliedSetupChanges = true;
        }
    }
}
