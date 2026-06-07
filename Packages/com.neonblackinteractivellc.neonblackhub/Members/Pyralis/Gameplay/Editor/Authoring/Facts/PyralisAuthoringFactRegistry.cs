using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringContractFacts
    {
        public static IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts()
        {
            List<PyralisAuthoringFact> facts = new List<PyralisAuthoringFact>();
            IReadOnlyList<PyralisAuthoringContract> contracts = PyralisAuthoringContractRegistry.All;
            for (int i = 0; i < contracts.Count; i++)
            {
                PyralisAuthoringContract contract = contracts[i];
                if (contract != null)
                    facts.Add(CreateContractFact(contract));
            }

            return facts;
        }

        private static PyralisAuthoringFact CreateContractFact(PyralisAuthoringContract contract)
        {
            return new PyralisAuthoringFact(
                contract.StableId,
                contract.DisplayName,
                PyralisAuthoringFactKind.FeatureContract,
                PyralisAuthoringFactSourceKind.FeatureContract,
                contract.Confidence,
                $"Feature contract for {contract.AuthoringCategory} module setup and lane compatibility.",
                $"Feature contract for {contract.AuthoringCategory} lane coverage and compatibility.",
                contract.FirstProofTargetId,
                requiredProfiles: GetRequiredProfiles(contract.RequiredProfileType),
                requiredPrefabComponents: NormalizeTypeNames(contract.RequiredRuntimeInterfaceNames),
                laneTags: ToStringArray(contract.SupportedPresentationModes),
                unsupportedLaneTags: ToStringArray(contract.UnsupportedPresentationModes),
                assignmentFields: contract.AssignmentFields,
                customizationMoments: contract.CustomizationMoments,
                nativeActions: BuildNativeActions(contract.NativeSetup),
                relatedStableIds: BuildRelatedStableIds(contract.FirstProofTargetId));
        }

        private static string[] GetRequiredProfiles(Type requiredProfileType)
        {
            if (requiredProfileType == null)
                return Array.Empty<string>();

            return new[] { requiredProfileType.Name };
        }

        private static string[] NormalizeTypeNames(string[] typeNames)
        {
            if (typeNames == null || typeNames.Length == 0)
                return Array.Empty<string>();

            string[] normalized = new string[typeNames.Length];
            for (int i = 0; i < typeNames.Length; i++)
                normalized[i] = SimplifyTypeName(typeNames[i]);

            return normalized;
        }

        private static string SimplifyTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return string.Empty;

            int lastDot = typeName.LastIndexOf('.');
            if (lastDot < 0 || lastDot >= typeName.Length - 1)
                return typeName;

            return typeName.Substring(lastDot + 1);
        }

        private static string[] BuildRelatedStableIds(string firstProofTargetId)
        {
            if (string.IsNullOrWhiteSpace(firstProofTargetId))
                return Array.Empty<string>();

            return new[] { firstProofTargetId };
        }

        private static string[] ToStringArray(Presentation.Animation.ActorPresentationMode[] presentationModes)
        {
            if (presentationModes == null || presentationModes.Length == 0)
                return Array.Empty<string>();

            string[] values = new string[presentationModes.Length];
            for (int i = 0; i < presentationModes.Length; i++)
                values[i] = presentationModes[i].ToString();

            return values;
        }

        private static PyralisAuthoringNativeAction[] BuildNativeActions(string[] nativeSetup)
        {
            if (nativeSetup == null || nativeSetup.Length == 0)
                return Array.Empty<PyralisAuthoringNativeAction>();

            List<PyralisAuthoringNativeAction> actions = new List<PyralisAuthoringNativeAction>();
            for (int i = 0; i < nativeSetup.Length; i++)
            {
                string step = nativeSetup[i];
                if (string.IsNullOrWhiteSpace(step))
                    continue;

                actions.Add(BuildNativeActionFromStep(step));
            }

            return actions.ToArray();
        }

        private static PyralisAuthoringNativeAction BuildNativeActionFromStep(string step)
        {
            string action = step.Trim();
            string lowered = action.ToLowerInvariant();

            string verb = "Inspect";
            PyralisAuthoringActionSurface surface = PyralisAuthoringActionSurface.Inspector;
            string target = "feature setup";
            string field = action;
            string success = "the setup step is captured and verified in the editor";

            if (lowered.StartsWith("create", StringComparison.Ordinal))
            {
                verb = "Create";
                surface = PyralisAuthoringActionSurface.ProjectWindow;
                success = "the referenced item exists in the chosen project folder";
            }
            else if (lowered.StartsWith("assign ", StringComparison.Ordinal) || lowered.StartsWith("add ", StringComparison.Ordinal))
            {
                verb = "Assign";
                target = "Feature module setup";
                success = "the value is assigned in the authored definition";
            }
            else if (lowered.StartsWith("bind ", StringComparison.Ordinal))
            {
                verb = "Bind";
                target = "Action binding";
                success = "the action role is connected in the authored action graph";
            }

            return new PyralisAuthoringNativeAction(verb, surface, target, field, success);
        }
    }

    public static class PyralisAuthoringFactRegistry
    {
        private static readonly Lazy<IReadOnlyList<PyralisAuthoringFact>> _allFacts =
            new Lazy<IReadOnlyList<PyralisAuthoringFact>>(BuildFacts);

        public static IReadOnlyList<PyralisAuthoringFact> AllFacts => _allFacts.Value;

        public static List<PyralisAuthoringFact> GetFacts(PyralisAuthoringFactKind kind)
        {
            IReadOnlyList<PyralisAuthoringFact> facts = AllFacts;
            List<PyralisAuthoringFact> matches = new List<PyralisAuthoringFact>();
            for (int i = 0; i < facts.Count; i++)
            {
                if (facts[i].Kind == kind)
                    matches.Add(facts[i]);
            }

            return matches;
        }

        public static PyralisAuthoringFact Find(string stableId)
        {
            IReadOnlyList<PyralisAuthoringFact> facts = AllFacts;
            for (int i = 0; i < facts.Count; i++)
            {
                if (facts[i].MatchesStableId(stableId))
                    return facts[i];
            }

            return null;
        }

        public static bool HasDuplicateStableIds(out string duplicateStableId)
        {
            IReadOnlyList<PyralisAuthoringFact> facts = AllFacts;
            HashSet<string> ids = new HashSet<string>();
            for (int i = 0; i < facts.Count; i++)
            {
                string stableId = facts[i].StableId;
                if (string.IsNullOrWhiteSpace(stableId))
                    continue;

                if (!ids.Add(stableId))
                {
                    duplicateStableId = stableId;
                    return true;
                }
            }

            duplicateStableId = string.Empty;
            return false;
        }

        private static IReadOnlyList<PyralisAuthoringFact> BuildFacts()
        {
            List<PyralisAuthoringFact> facts = new List<PyralisAuthoringFact>();
            IReadOnlyList<RuntimeCapabilityCard> cards = PyralisRuntimeCapabilityCatalog.All;
            for (int i = 0; i < cards.Count; i++)
                facts.Add(cards[i].Fact);

            facts.AddRange(PyralisAuthoringContractFacts.GetAuthoringFacts());
            facts.AddRange(Inspectors.PyralisSetupFlowGuidance.GetAuthoringFacts());
            facts.AddRange(PyralisAuthoringRouteProof.GetAuthoringFacts());
            facts.AddRange(PyralisRouteCoverageFacts.GetAuthoringFacts());
            facts.AddRange(PyralisInspectorHandoffFacts.GetAuthoringFacts());
            facts.AddRange(PyralisAuthoringConventionFactRegistry.AllFacts);
            facts.AddRange(PyralisSceneSurfaceEvidenceFacts.GetAuthoringFacts());

            return facts;
        }
    }
}
