using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisAuthoringFactExplorerRenderer
    {
        private static readonly Dictionary<string, bool> Foldouts = new Dictionary<string, bool>();

        public static void Draw(Object activeSetup)
        {
            EditorGUILayout.LabelField("Fact Explorer", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("Read-only coverage view. Facts explain what Pyralis knows about capabilities, setup nodes, proof paths, Inspector handoffs, validation vocabulary, and future convention-derived guidance. Use native Unity surfaces for creation, assignment, customization, and Play Mode proof.", MessageType.Info);

            IReadOnlyList<PyralisAuthoringFact> facts = PyralisAuthoringFactRegistry.AllFacts;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Active Setup", activeSetup != null ? $"{activeSetup.name} ({activeSetup.GetType().Name})" : "No active setup selected");
                EditorGUILayout.LabelField("Total Facts", facts.Count.ToString());
                DrawFactCoverageSummary(facts);
            }

            DrawFeatureContractSetupProfiles();

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
            DrawFactGroup(PyralisAuthoringFactKind.UnitySurface, facts);
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

        private static void DrawFeatureContractSetupProfiles()
        {
            IReadOnlyList<ResolvedAuthoringContract> contracts = ResolvedAuthoringContractRegistry.All;
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Contract-Backed Feature Module Setup", EditorStyles.boldLabel);
            PyralisAuthoringWindowText.DrawSemanticHelpBox("Read-only setup guidance generated from feature-owned authoring contracts. Use native Unity surfaces for asset creation, Prefab/Component composition, Inspector assignment, object picking, and Play Mode proof.", MessageType.Info);

            if (contracts == null || contracts.Count == 0)
            {
                EditorGUILayout.LabelField("No feature contracts discovered. Tag interfaces with [AuthoringContract(ModuleId=\"...\")] for reflective discovery.", EditorStyles.wordWrappedMiniLabel);
                return;
            }

            Dictionary<string, List<ResolvedAuthoringContract>> contractsByCategory = BuildContractsByCategory(contracts);
            List<string> categories = new List<string>(contractsByCategory.Keys);
            categories.Sort(System.StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < categories.Count; i++)
            {
                string category = categories[i];
                List<ResolvedAuthoringContract> categoryContracts = contractsByCategory[category];
                string key = "Pyralis.AuthoringWindow.ContractSetup." + category;
                bool isOpen = Foldouts.TryGetValue(key, out bool value) && value;
                isOpen = EditorGUILayout.Foldout(isOpen, $"{category} Contracts ({categoryContracts.Count})", true);
                Foldouts[key] = isOpen;

                if (!isOpen)
                    continue;

                EditorGUI.indentLevel++;
                for (int contractIndex = 0; contractIndex < categoryContracts.Count; contractIndex++)
                    DrawFeatureContractSetupProfile(categoryContracts[contractIndex]);
                EditorGUI.indentLevel--;
            }
        }

        private static Dictionary<string, List<ResolvedAuthoringContract>> BuildContractsByCategory(IReadOnlyList<ResolvedAuthoringContract> contracts)
        {
            Dictionary<string, List<ResolvedAuthoringContract>> contractsByCategory = new Dictionary<string, List<ResolvedAuthoringContract>>();
            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                if (contract == null)
                    continue;

                string category = string.IsNullOrWhiteSpace(contract.AuthoringCategory) ? "General" : contract.AuthoringCategory;
                if (!contractsByCategory.TryGetValue(category, out List<ResolvedAuthoringContract> categoryContracts))
                {
                    categoryContracts = new List<ResolvedAuthoringContract>();
                    contractsByCategory.Add(category, categoryContracts);
                }

                categoryContracts.Add(contract);
            }

            foreach (List<ResolvedAuthoringContract> categoryContracts in contractsByCategory.Values)
            {
                categoryContracts.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, System.StringComparison.OrdinalIgnoreCase));
            }

            return contractsByCategory;
        }

        private static void DrawFeatureContractSetupProfile(ResolvedAuthoringContract contract)
        {
            if (contract == null)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(contract.DisplayName, contract.StableId, EditorStyles.boldLabel);
                PyralisAuthoringWindowPrimitives.DrawSemanticTagStrip(GetFeatureContractSetupTags(contract));
                PyralisAuthoringWindowPrimitives.DrawMiniField("Feature Contract", contract.StableId);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Required Profile", contract.RequiredProfileType != null ? contract.RequiredProfileType.Name : "None for this module.");
                PyralisAuthoringWindowPrimitives.DrawMiniList("Runtime Interfaces", contract.RequiredRuntimeInterfaceNames);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Required Unity Components", contract.RequiredComponentNames);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Supported Lanes", ToPresentationModeNames(contract.SupportedPresentationModes));
                PyralisAuthoringWindowPrimitives.DrawMiniList("Unsupported / Caution Lanes", ToPresentationModeNames(contract.UnsupportedPresentationModes));
                if (!string.IsNullOrWhiteSpace(contract.UnsupportedLaneMessage))
                    PyralisAuthoringWindowPrimitives.DrawMiniField("Unsupported Lane Message", contract.UnsupportedLaneMessage);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Consumed Actions", contract.ConsumedActionRoles);
                DrawContractNativeSetupActions(contract.NativeSetup);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Assignment Fields", contract.AssignmentFields);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Customization Moments", contract.CustomizationMoments);
                PyralisAuthoringWindowPrimitives.DrawMiniField("First Proof Target", string.IsNullOrWhiteSpace(contract.FirstProofTargetId) ? "None recorded yet." : contract.FirstProofTargetId);
            }
        }

        private static List<PyralisAuthoringSemanticTag> GetFeatureContractSetupTags(ResolvedAuthoringContract contract)
        {
            List<PyralisAuthoringSemanticTag> tags = new List<PyralisAuthoringSemanticTag>();
            AddSemanticTag(PyralisAuthoringSemanticTag.Authoring, tags);
            AddSemanticTag(PyralisAuthoringSemanticTag.Project, tags);
            AddSemanticTag(PyralisAuthoringSemanticTag.Inspector, tags);
            AddSemanticTagIfAny(contract.RequiredRuntimeInterfaceNames, PyralisAuthoringSemanticTag.Prefab, tags);
            AddSemanticTagIfAny(contract.RequiredComponentNames, PyralisAuthoringSemanticTag.Prefab, tags);
            AddSemanticTagIfAny(contract.ConsumedActionRoles, PyralisAuthoringSemanticTag.Input, tags);
            if (!string.IsNullOrWhiteSpace(contract.FirstProofTargetId))
                AddSemanticTag(PyralisAuthoringSemanticTag.PlayMode, tags);
            return tags;
        }

        public static string[] ToPresentationModeNames(ActorPresentationMode[] modes)
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
                PyralisAuthoringWindowPrimitives.DrawMiniField("Native Setup Actions", "None recorded yet.");
                return;
            }

            EditorGUILayout.LabelField("Native Setup Actions", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < nativeSetup.Count; i++)
            {
                string setupStep = nativeSetup[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    PyralisAuthoringWindowPrimitives.DrawSemanticTagBadge(GetContractSetupSemanticTag(setupStep));
                    EditorGUILayout.LabelField(GetContractSetupSurfaceLabel(setupStep), GUILayout.Width(150f));
                    PyralisAuthoringWindowText.DrawSemanticMiniLabel(setupStep);
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
            bool isOpen = Foldouts.TryGetValue(key, out bool value) && value;
            isOpen = EditorGUILayout.Foldout(isOpen, $"{kind} ({count})", true);
            Foldouts[key] = isOpen;

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
                PyralisAuthoringWindowPrimitives.DrawMiniField("Source", fact.SourceKind + " / " + fact.Confidence);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Work Intent", fact.WorkIntent);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Route Relevance", fact.RouteRelevance);
                PyralisAuthoringWindowPrimitives.DrawMiniField("Summary", fact.Summary);
                PyralisAuthoringWindowPrimitives.DrawMiniField("First Proof", fact.FirstProof);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Goal Tags", fact.GoalTags);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Supported Lanes", fact.LaneTags);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Unsupported / Caution Lanes", fact.UnsupportedLaneTags);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Required Definitions", fact.RequiredDefinitions);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Required Profiles", fact.RequiredProfiles);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Required Scene Components", fact.RequiredSceneComponents);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Required Unity Surfaces", fact.RequiredUnitySurfaces);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Assignment Fields", fact.AssignmentFields);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Customization Moments", fact.CustomizationMoments);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Can Wait", fact.CanWait);
                DrawFactNativeActions(fact.NativeActions);
                PyralisAuthoringWindowPrimitives.DrawMiniList("Related Stable Ids", fact.RelatedStableIds);
            }
        }

        private static void DrawFactNativeActions(IReadOnlyList<PyralisAuthoringNativeAction> actions)
        {
            if (actions == null || actions.Count == 0)
            {
                PyralisAuthoringWindowPrimitives.DrawMiniField("Native Unity Actions", "None recorded yet.");
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

        public static void DrawFactSemanticTags(PyralisAuthoringFact fact)
        {
            List<PyralisAuthoringSemanticTag> tags = GetFactSemanticTags(fact);
            if (tags.Count == 0)
                return;

            PyralisAuthoringWindowPrimitives.DrawSemanticTagStrip(tags);
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
            AddSemanticTagIfAny(fact.RequiredUnitySurfaces, PyralisAuthoringSemanticTag.Prefab, tags);
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
            AddSemanticTagForText(string.Join(" ", fact.RequiredUnitySurfaces), tags);
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
                case PyralisAuthoringFactKind.UnitySurface:
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

    }
}
