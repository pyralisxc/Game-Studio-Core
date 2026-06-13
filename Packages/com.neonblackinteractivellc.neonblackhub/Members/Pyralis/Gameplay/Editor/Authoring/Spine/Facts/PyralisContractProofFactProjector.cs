using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisContractProofFactProjector
    {
        public static IReadOnlyList<PyralisAuthoringFact> EnrichRouteProofFacts(IReadOnlyList<PyralisAuthoringFact> routeProofFacts)
        {
            if (routeProofFacts == null || routeProofFacts.Count == 0)
                return Array.Empty<PyralisAuthoringFact>();

            Dictionary<string, List<ResolvedAuthoringContract>> contractsByProofId =
                BuildContractsByProofTargetId();
            List<PyralisAuthoringFact> enriched = new List<PyralisAuthoringFact>(routeProofFacts.Count);

            for (int i = 0; i < routeProofFacts.Count; i++)
            {
                PyralisAuthoringFact routeProof = routeProofFacts[i];
                if (routeProof == null || string.IsNullOrWhiteSpace(routeProof.StableId))
                {
                    enriched.Add(routeProof);
                    continue;
                }

                if (!contractsByProofId.TryGetValue(routeProof.StableId, out List<ResolvedAuthoringContract> contracts)
                    || contracts == null
                    || contracts.Count == 0)
                {
                    enriched.Add(routeProof);
                    continue;
                }

                enriched.Add(EnrichRouteProofFact(routeProof, contracts));
            }

            return enriched;
        }

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

        private static Dictionary<string, List<ResolvedAuthoringContract>> BuildContractsByProofTargetId()
        {
            Dictionary<string, List<ResolvedAuthoringContract>> contractsByProofId =
                new Dictionary<string, List<ResolvedAuthoringContract>>(StringComparer.Ordinal);
            IReadOnlyList<ResolvedAuthoringContract> contracts = ResolvedAuthoringContractRegistry.All;

            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                if (contract == null || string.IsNullOrWhiteSpace(contract.FirstProofTargetId))
                    continue;

                if (!contractsByProofId.TryGetValue(contract.FirstProofTargetId, out List<ResolvedAuthoringContract> matches))
                {
                    matches = new List<ResolvedAuthoringContract>();
                    contractsByProofId.Add(contract.FirstProofTargetId, matches);
                }

                matches.Add(contract);
            }

            return contractsByProofId;
        }

        private static PyralisAuthoringFact EnrichRouteProofFact(PyralisAuthoringFact routeProof, List<ResolvedAuthoringContract> contracts)
        {
            List<string> contractStableIds = new List<string>();
            List<string> generatedGoalTags = new List<string>();
            List<string> generatedLaneTags = new List<string>();
            List<string> generatedUnsupportedLaneTags = new List<string>();
            List<string> generatedRequiredProfiles = new List<string>();
            List<string> generatedRequiredUnitySurfaces = new List<string>();
            List<string> generatedAssignmentFields = new List<string>();
            List<string> generatedCustomizationMoments = new List<string>();
            List<string> generatedRelatedStableIds = new List<string>();
            List<PyralisAuthoringNativeAction> generatedNativeActions = new List<PyralisAuthoringNativeAction>();
            AuthoringWorldAxiom axioms = routeProof.Axioms;
            AuthoringCapability capability = routeProof.Capability;

            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                if (contract == null)
                    continue;

                AddDistinct(contractStableIds, contract.StableId);
                AddRangeDistinct(generatedGoalTags, BuildGoalTags(contract));
                AddRangeDistinct(generatedLaneTags, PyralisReflectiveFactScanner.ToStringArray(contract.SupportedPresentationModes));
                AddRangeDistinct(generatedUnsupportedLaneTags, PyralisReflectiveFactScanner.ToStringArray(contract.UnsupportedPresentationModes));
                AddRangeDistinct(generatedRequiredProfiles, PyralisReflectiveFactScanner.GetRequiredProfiles(contract.RequiredProfileType));
                AddRangeDistinct(generatedRequiredUnitySurfaces, SimplifyTypeNames(contract.RequiredRuntimeInterfaceNames, contract.RequiredComponentNames));
                AddRangeDistinct(generatedAssignmentFields, contract.AssignmentFields);
                AddRangeDistinct(generatedCustomizationMoments, contract.CustomizationMoments);
                AddRangeDistinct(generatedRelatedStableIds, BuildRelatedStableIds(contract));
                AddRangeDistinct(generatedNativeActions, BuildNativeActions(contract));
                axioms |= contract.Axioms;
                capability |= contract.Capability;
            }

            string routeRelevance = routeProof.RouteRelevance;
            if (contractStableIds.Count > 0)
            {
                routeRelevance = string.IsNullOrWhiteSpace(routeRelevance)
                    ? "Reflective contract inputs: " + string.Join(", ", contractStableIds)
                    : routeRelevance + " Reflective contract inputs: " + string.Join(", ", contractStableIds);
            }

            return new PyralisAuthoringFact(
                routeProof.StableId,
                routeProof.DisplayName,
                routeProof.Kind,
                routeProof.SourceKind,
                routeProof.Confidence,
                routeProof.Summary,
                routeRelevance,
                routeProof.FirstProof,
                goalTags: MergeDistinct(routeProof.GoalTags, generatedGoalTags),
                laneTags: MergeDistinct(routeProof.LaneTags, generatedLaneTags),
                unsupportedLaneTags: MergeDistinct(routeProof.UnsupportedLaneTags, generatedUnsupportedLaneTags),
                requiredDefinitions: routeProof.RequiredDefinitions,
                requiredProfiles: MergeDistinct(routeProof.RequiredProfiles, generatedRequiredProfiles),
                requiredSceneComponents: routeProof.RequiredSceneComponents,
                requiredUnitySurfaces: MergeDistinct(routeProof.RequiredUnitySurfaces, generatedRequiredUnitySurfaces),
                assignmentFields: MergeDistinct(routeProof.AssignmentFields, generatedAssignmentFields),
                customizationMoments: MergeDistinct(routeProof.CustomizationMoments, generatedCustomizationMoments),
                canWait: routeProof.CanWait,
                nativeActions: MergeDistinct(routeProof.NativeActions, generatedNativeActions),
                workIntent: routeProof.WorkIntent,
                relatedStableIds: MergeDistinct(routeProof.RelatedStableIds, generatedRelatedStableIds),
                axioms: axioms,
                capability: capability,
                priority: (AuthoringPriority)routeProof.Priority,
                priorityValueOverride: routeProof.PriorityValueOverride,
                deprecatedInVersion: routeProof.DeprecatedInVersion,
                removableInVersion: routeProof.RemovableInVersion,
                documentationURL: routeProof.DocumentationURL,
                expertAdvice: routeProof.ExpertAdvice);
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
                requiredUnitySurfaces: SimplifyTypeNames(contract.RequiredRuntimeInterfaceNames, contract.RequiredComponentNames),
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
            if (!string.IsNullOrWhiteSpace(contract.AuthoringLane) && !tags.Contains(contract.AuthoringLane))
                tags.Add(contract.AuthoringLane);

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

        private static void AddRangeDistinct(List<string> target, string[] values)
        {
            if (values == null)
                return;

            for (int i = 0; i < values.Length; i++)
                AddDistinct(target, values[i]);
        }

        private static void AddDistinct(List<string> target, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || target.Contains(value))
                return;

            target.Add(value);
        }

        private static string[] MergeDistinct(string[] first, List<string> second)
        {
            List<string> merged = new List<string>();
            AddRangeDistinct(merged, first);
            if (second != null)
            {
                for (int i = 0; i < second.Count; i++)
                    AddDistinct(merged, second[i]);
            }

            return merged.ToArray();
        }

        private static void AddRangeDistinct(List<PyralisAuthoringNativeAction> target, PyralisAuthoringNativeAction[] values)
        {
            if (values == null)
                return;

            for (int i = 0; i < values.Length; i++)
                AddDistinct(target, values[i]);
        }

        private static void AddDistinct(List<PyralisAuthoringNativeAction> target, PyralisAuthoringNativeAction value)
        {
            for (int i = 0; i < target.Count; i++)
            {
                PyralisAuthoringNativeAction existing = target[i];
                if (existing.Surface == value.Surface
                    && string.Equals(existing.Verb, value.Verb, StringComparison.Ordinal)
                    && string.Equals(existing.Target, value.Target, StringComparison.Ordinal)
                    && string.Equals(existing.FieldOrComponent, value.FieldOrComponent, StringComparison.Ordinal)
                    && string.Equals(existing.SuccessCheck, value.SuccessCheck, StringComparison.Ordinal))
                {
                    return;
                }
            }

            target.Add(value);
        }

        private static PyralisAuthoringNativeAction[] MergeDistinct(
            PyralisAuthoringNativeAction[] first,
            List<PyralisAuthoringNativeAction> second)
        {
            List<PyralisAuthoringNativeAction> merged = new List<PyralisAuthoringNativeAction>();
            AddRangeDistinct(merged, first);
            if (second != null)
            {
                for (int i = 0; i < second.Count; i++)
                    AddDistinct(merged, second[i]);
            }

            return merged.ToArray();
        }
    }
}
