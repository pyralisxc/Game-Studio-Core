using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisContractProofFactProjector
    {
        public static IReadOnlyList<PyralisAuthoringFact> GetAuthoringFacts(IReadOnlyCollection<string> existingProofIds = null)
        {
            HashSet<string> seenProofIds = new HashSet<string>(StringComparer.Ordinal);
            if (existingProofIds != null)
            {
                foreach (string proofId in existingProofIds)
                {
                    if (!string.IsNullOrWhiteSpace(proofId))
                        seenProofIds.Add(proofId);
                }
            }

            List<PyralisAuthoringFact> facts = new List<PyralisAuthoringFact>();
            IReadOnlyList<ResolvedAuthoringContract> contracts = ResolvedAuthoringContractRegistry.All;
            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                if (contract == null || string.IsNullOrWhiteSpace(contract.FirstProofTargetId))
                    continue;

                if (!seenProofIds.Add(contract.FirstProofTargetId))
                    continue;

                facts.Add(CreateProofFact(contract));
            }

            return facts;
        }

        public static PyralisAuthoringFact FindProofFact(string stableId, IReadOnlyCollection<string> existingProofIds = null)
        {
            if (string.IsNullOrWhiteSpace(stableId))
                return null;

            IReadOnlyList<PyralisAuthoringFact> facts = GetAuthoringFacts(existingProofIds);
            for (int i = 0; i < facts.Count; i++)
            {
                if (facts[i] != null && facts[i].MatchesStableId(stableId))
                    return facts[i];
            }

            return null;
        }

        private static PyralisAuthoringFact CreateProofFact(ResolvedAuthoringContract contract)
        {
            string displayName = !string.IsNullOrWhiteSpace(contract.DisplayName)
                ? contract.DisplayName + " Proof"
                : contract.FirstProofTargetId;
            string category = !string.IsNullOrWhiteSpace(contract.AuthoringCategory)
                ? contract.AuthoringCategory
                : AuthoringCapabilityRegistry.GetDisplayName(contract.Capability);
            string routeRelevance = !string.IsNullOrWhiteSpace(contract.Relevance)
                ? contract.Relevance
                : "Feature-owned authoring contract proof target.";
            string firstProof = !string.IsNullOrWhiteSpace(contract.FirstProofGuidance)
                ? contract.FirstProofGuidance
                : "Run one focused Play Mode proof for this feature after required setup evidence is clear.";

            return new PyralisAuthoringFact(
                contract.FirstProofTargetId,
                displayName,
                PyralisAuthoringFactKind.Proof,
                PyralisAuthoringFactSourceKind.FeatureContract,
                contract.Confidence,
                "Feature-owned proof target generated from " + contract.StableId + ".",
                routeRelevance,
                firstProof,
                goalTags: BuildGoalTags(contract),
                laneTags: PyralisReflectiveFactScanner.ToStringArray(contract.SupportedPresentationModes),
                unsupportedLaneTags: PyralisReflectiveFactScanner.ToStringArray(contract.UnsupportedPresentationModes),
                requiredProfiles: PyralisReflectiveFactScanner.GetRequiredProfiles(contract.RequiredProfileType),
                requiredPrefabComponents: SimplifyTypeNames(contract.RequiredRuntimeInterfaceNames, contract.RequiredComponentNames),
                assignmentFields: contract.AssignmentFields,
                customizationMoments: contract.CustomizationMoments,
                canWait: contract.UnsupportedPresentationModes.Length > 0 ? new[] { contract.UnsupportedLaneMessage } : null,
                nativeActions: BuildNativeActions(contract),
                workIntent: "FirstProof",
                relatedStableIds: BuildRelatedStableIds(contract),
                axioms: contract.Axioms,
                capability: contract.Capability,
                priority: (AuthoringPriority)contract.Priority,
                priorityValueOverride: contract.PriorityValueOverride,
                documentationURL: contract.DocumentationURL,
                expertAdvice: contract.ExpertAdvice);
        }

        private static string[] BuildGoalTags(ResolvedAuthoringContract contract)
        {
            List<string> tags = new List<string>();
            foreach (AuthoringCapability capability in AuthoringCapabilityRegistry.GetAllIndividualCapabilities())
            {
                if ((contract.Capability & capability) != 0)
                    tags.Add(AuthoringCapabilityRegistry.GetDisplayName(capability));
            }

            if (!string.IsNullOrWhiteSpace(contract.AuthoringCategory) && !tags.Contains(contract.AuthoringCategory))
                tags.Add(contract.AuthoringCategory);

            return tags.ToArray();
        }

        private static string[] SimplifyTypeNames(string[] interfaceNames, string[] componentNames)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            List<string> names = new List<string>();
            AddNames(interfaceNames);
            AddNames(componentNames);
            return names.ToArray();

            void AddNames(string[] source)
            {
                if (source == null)
                    return;

                for (int i = 0; i < source.Length; i++)
                {
                    string name = SimplifyTypeName(source[i]);
                    if (!string.IsNullOrWhiteSpace(name) && seen.Add(name))
                        names.Add(name);
                }
            }
        }

        private static string SimplifyTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return string.Empty;

            int lastDot = typeName.LastIndexOf('.');
            return lastDot >= 0 && lastDot < typeName.Length - 1
                ? typeName.Substring(lastDot + 1)
                : typeName;
        }

        private static PyralisAuthoringNativeAction[] BuildNativeActions(ResolvedAuthoringContract contract)
        {
            List<PyralisAuthoringNativeAction> actions = new List<PyralisAuthoringNativeAction>();
            if (contract.SourceType != null)
                actions.AddRange(PyralisReflectiveFactScanner.BuildNativeActions(contract.SourceType, contract.NativeSetup));

            if (actions.Count == 0)
            {
                actions.Add(new PyralisAuthoringNativeAction(
                    "Inspect",
                    PyralisAuthoringActionSurface.Inspector,
                    !string.IsNullOrWhiteSpace(contract.DisplayName) ? contract.DisplayName : "feature contract",
                    "review the contract-owned setup fields and evidence",
                    "the feature proof target has visible setup evidence"));
            }

            return actions.ToArray();
        }

        private static string[] BuildRelatedStableIds(ResolvedAuthoringContract contract)
        {
            List<string> related = new List<string> { contract.StableId };
            if (!string.IsNullOrWhiteSpace(contract.SetupNodeId))
                related.Add(contract.SetupNodeId);
            if (!string.IsNullOrWhiteSpace(contract.ModuleId))
                related.Add("feature." + contract.ModuleId);

            return related.ToArray();
        }
    }
}
