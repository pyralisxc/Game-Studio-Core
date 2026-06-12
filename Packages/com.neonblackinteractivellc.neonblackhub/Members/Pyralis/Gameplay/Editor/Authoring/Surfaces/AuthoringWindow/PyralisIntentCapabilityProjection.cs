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
            return PyralisRuntimeCapabilityFamilyMap.GetFamilies(capabilities, lane, axioms);
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

    }
}
