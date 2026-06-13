using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisIntentCapabilityProjection
    {
        public static RuntimeCapabilityFamily[] BuildRuntimeFamilies(
            AuthoringCapability capabilities,
            RuntimeCapabilityLaneTag lane,
            AuthoringWorldAxiom axioms)
        {
            RuntimeCapabilityFamily[] reflected = PyralisReflectiveCapabilityDependencyProjection.BuildRuntimeFamilies(
                capabilities,
                lane,
                axioms);
            RuntimeCapabilityFamily[] fallback = PyralisRuntimeCapabilityFamilyMap.GetFamilies(capabilities, lane, axioms);
            return MergeDistinct(reflected, fallback);
        }

        public static RuntimePatternDefinition[] FilterRuntimePatternsToFamilies(
            RuntimePatternDefinition[] patterns,
            IReadOnlyCollection<RuntimeCapabilityFamily> families)
        {
            if (patterns == null || patterns.Length == 0 || families == null || families.Count == 0)
                return Array.Empty<RuntimePatternDefinition>();

            return patterns
                .Where(pattern => pattern != null && families.Contains(pattern.capabilityFamily))
                .ToArray();
        }

        private static RuntimeCapabilityFamily[] MergeDistinct(
            RuntimeCapabilityFamily[] reflected,
            RuntimeCapabilityFamily[] fallback)
        {
            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();
            AddRangeDistinct(families, reflected);
            AddRangeDistinct(families, fallback);
            return families.ToArray();
        }

        private static void AddRangeDistinct(List<RuntimeCapabilityFamily> target, RuntimeCapabilityFamily[] source)
        {
            if (source == null)
                return;

            for (int i = 0; i < source.Length; i++)
            {
                if (!target.Contains(source[i]))
                    target.Add(source[i]);
            }
        }
    }
}
