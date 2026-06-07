using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringCapabilitySelection
    {
        public static RuntimePatternDefinition[] SetCapabilityPattern(RuntimePatternDefinition[] currentPatterns, RuntimePatternDefinition selectedPattern)
        {
            if (selectedPattern == null)
                return currentPatterns ?? System.Array.Empty<RuntimePatternDefinition>();

            RuntimePatternDefinition[] current = currentPatterns ?? System.Array.Empty<RuntimePatternDefinition>();
            List<RuntimePatternDefinition> next = new List<RuntimePatternDefinition>();
            for (int i = 0; i < current.Length; i++)
            {
                RuntimePatternDefinition pattern = current[i];
                if (pattern == null || pattern.capabilityFamily == selectedPattern.capabilityFamily)
                    continue;

                next.Add(pattern);
            }

            next.Add(selectedPattern);
            return next.ToArray();
        }

        public static RuntimePatternDefinition[] RemoveCapabilityFamily(RuntimePatternDefinition[] currentPatterns, RuntimeCapabilityFamily family)
        {
            RuntimePatternDefinition[] current = currentPatterns ?? System.Array.Empty<RuntimePatternDefinition>();
            List<RuntimePatternDefinition> next = new List<RuntimePatternDefinition>();
            for (int i = 0; i < current.Length; i++)
            {
                RuntimePatternDefinition pattern = current[i];
                if (pattern == null || pattern.capabilityFamily == family)
                    continue;

                next.Add(pattern);
            }

            return next.ToArray();
        }

        public static RuntimePatternDefinition GetSelectedPattern(RuntimePatternDefinition[] currentPatterns, RuntimeCapabilityFamily family)
        {
            if (currentPatterns == null)
                return null;

            for (int i = 0; i < currentPatterns.Length; i++)
            {
                RuntimePatternDefinition pattern = currentPatterns[i];
                if (pattern != null && pattern.capabilityFamily == family)
                    return pattern;
            }

            return null;
        }
    }
}
