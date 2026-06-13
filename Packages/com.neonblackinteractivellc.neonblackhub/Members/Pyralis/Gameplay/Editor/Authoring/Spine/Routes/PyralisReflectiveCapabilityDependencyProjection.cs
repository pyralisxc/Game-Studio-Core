using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisReflectiveCapabilityDependencyProjection
    {
        public static RuntimeCapabilityFamily[] BuildRuntimeFamilies(
            AuthoringCapability capabilities,
            RuntimeCapabilityLaneTag lane,
            AuthoringWorldAxiom axioms)
        {
            if (capabilities == AuthoringCapability.None)
                return Array.Empty<RuntimeCapabilityFamily>();

            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();
            Array values = Enum.GetValues(typeof(RuntimeCapabilityFamily));
            for (int i = 0; i < values.Length; i++)
            {
                RuntimeCapabilityFamily family = (RuntimeCapabilityFamily)values.GetValue(i);
                PyralisAuthoringFact fact = PyralisRuntimeCapabilityCatalog.FindPrimaryFactByFamily(family);
                if (!ReflectiveFactMatchesIntent(fact, capabilities, lane, axioms))
                    continue;

                AddDistinct(families, family);
            }

            return families.ToArray();
        }

        private static bool ReflectiveFactMatchesIntent(
            PyralisAuthoringFact fact,
            AuthoringCapability capabilities,
            RuntimeCapabilityLaneTag lane,
            AuthoringWorldAxiom axioms)
        {
            if (fact == null || fact.Capability == AuthoringCapability.None)
                return false;

            if ((fact.Capability & capabilities) == 0)
                return false;

            string laneName = lane.ToString();
            if (fact.IsExplicitlyUnsupported(laneName))
                return false;

            if (fact.LaneTags.Length > 0 && !fact.HasLane(laneName))
                return false;

            return fact.Axioms == AuthoringWorldAxiom.None
                || axioms == AuthoringWorldAxiom.None
                || (fact.Axioms & axioms) != 0;
        }

        private static void AddDistinct(List<RuntimeCapabilityFamily> families, RuntimeCapabilityFamily family)
        {
            if (!families.Contains(family))
                families.Add(family);
        }
    }
}
