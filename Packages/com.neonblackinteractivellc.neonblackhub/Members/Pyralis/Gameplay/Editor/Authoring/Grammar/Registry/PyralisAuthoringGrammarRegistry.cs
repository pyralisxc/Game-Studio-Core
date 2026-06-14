using NeonBlack.Gameplay.Core.Contracts;
using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringGrammarRegistry
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
            HashSet<string> seenIds = new HashSet<string>(StringComparer.Ordinal);

            void AddRange(IEnumerable<PyralisAuthoringFact> source)
            {
                if (source == null) return;
                foreach (var fact in source)
                {
                    if (fact != null && (string.IsNullOrEmpty(fact.StableId) || seenIds.Add(fact.StableId)))
                    {
                        facts.Add(fact);
                    }
                }
            }

            IReadOnlyList<PyralisAuthoringFact> capabilityFacts = PyralisCapabilityVocabulary.GetAuthoringFacts();
            for (int i = 0; i < capabilityFacts.Count; i++)
            {
                PyralisAuthoringFact fact = capabilityFacts[i];
                if (fact != null && seenIds.Add(fact.StableId))
                    facts.Add(fact);
            }

            AddRange(PyralisReflectiveFactScanner.ScanProject());
            AddRange(Inspectors.PyralisSetupFlowGuidance.GetAuthoringFacts());
            IReadOnlyList<PyralisAuthoringFact> proofTemplateFacts = PyralisProofFamilyVocabulary.GetAuthoringFacts();
            AddRange(proofTemplateFacts);
            AddRange(PyralisContractProofFactProjector.GetAuthoringFacts(GetStableIds(proofTemplateFacts)));
            AddRange(PyralisInspectorHandoffFacts.GetAuthoringFacts());
            AddRange(PyralisConventionAuthoringFacts.GetAuthoringFacts());
            AddRange(PyralisIntentVocabulary.GetAuthoringFacts());
            AddRange(PyralisSceneSurfaceEvidenceFacts.GetAuthoringFacts());

            return facts;
        }

        private static IReadOnlyCollection<string> GetStableIds(IReadOnlyList<PyralisAuthoringFact> facts)
        {
            HashSet<string> stableIds = new HashSet<string>(StringComparer.Ordinal);
            if (facts == null)
                return stableIds;

            for (int i = 0; i < facts.Count; i++)
            {
                if (facts[i] != null && !string.IsNullOrWhiteSpace(facts[i].StableId))
                    stableIds.Add(facts[i].StableId);
            }

            return stableIds;
        }
    }
}
