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
            return PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(capabilities, lane, axioms);
        }
    }
}
