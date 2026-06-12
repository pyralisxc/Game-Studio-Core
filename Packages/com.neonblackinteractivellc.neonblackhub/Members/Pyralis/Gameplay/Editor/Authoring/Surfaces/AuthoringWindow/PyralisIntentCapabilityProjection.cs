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
            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();

            AddFamilyIf(
                families,
                HasAnyCapability(
                    capabilities,
                    AuthoringCapability.Participants,
                    AuthoringCapability.Movement,
                    AuthoringCapability.KineticMotor2D,
                    AuthoringCapability.KineticMotor3D,
                    AuthoringCapability.Steering2D,
                    AuthoringCapability.Steering3D,
                    AuthoringCapability.Traversal),
                RuntimeCapabilityFamily.CharacterPawnGameplay);

            AddFamilyIf(
                families,
                HasAnyCapability(
                    capabilities,
                    AuthoringCapability.Combat,
                    AuthoringCapability.CombatState,
                    AuthoringCapability.CombatSensors,
                    AuthoringCapability.MeleeFlow,
                    AuthoringCapability.TacticsAggressive,
                    AuthoringCapability.TacticsDefensive),
                RuntimeCapabilityFamily.Combat);

            AddFamilyIf(
                families,
                HasAnyCapability(capabilities, AuthoringCapability.RangedFlow),
                RuntimeCapabilityFamily.GunsProjectiles);

            AddFamilyIf(
                families,
                HasAnyCapability(capabilities, AuthoringCapability.Rules, AuthoringCapability.UI, AuthoringCapability.TurnBased, AuthoringCapability.Puzzle),
                RuntimeCapabilityFamily.ActionTargeting);

            AddFamilyIf(
                families,
                HasAnyCapability(capabilities, AuthoringCapability.Tabletop, AuthoringCapability.Grid)
                    || lane == RuntimeCapabilityLaneTag.TabletopBoard,
                RuntimeCapabilityFamily.BoardCardTabletop);

            AddFamilyIf(
                families,
                HasAnyCapability(capabilities, AuthoringCapability.Camera, AuthoringCapability.Input)
                    || lane == RuntimeCapabilityLaneTag.CameraCursor,
                RuntimeCapabilityFamily.CameraInput);

            AddFamilyIf(
                families,
                HasAnyCapability(capabilities, AuthoringCapability.Animation, AuthoringCapability.VFX),
                RuntimeCapabilityFamily.AnimationPresentation);

            AddFamilyIf(
                families,
                HasAnyCapability(capabilities, AuthoringCapability.Scoring),
                RuntimeCapabilityFamily.ScoringObjectives);

            AddFamilyIf(
                families,
                HasAnyCapability(capabilities, AuthoringCapability.Environment)
                    && (axioms & AuthoringWorldAxiom.InfiniteSpace) != 0,
                RuntimeCapabilityFamily.ProceduralGeneration);

            AddFamilyIf(
                families,
                HasAnyCapability(capabilities, AuthoringCapability.Networking)
                    || (axioms & AuthoringWorldAxiom.Networked) != 0,
                RuntimeCapabilityFamily.Networking);

            return families.ToArray();
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

        private static bool HasAnyCapability(AuthoringCapability selected, params AuthoringCapability[] candidates)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                if ((selected & candidates[i]) != 0)
                    return true;
            }

            return false;
        }

        private static void AddFamilyIf(List<RuntimeCapabilityFamily> families, bool condition, RuntimeCapabilityFamily family)
        {
            if (condition && !families.Contains(family))
                families.Add(family);
        }
    }
}
