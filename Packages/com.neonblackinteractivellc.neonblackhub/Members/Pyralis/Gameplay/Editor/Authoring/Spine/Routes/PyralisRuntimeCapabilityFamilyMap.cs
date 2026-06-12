using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisRuntimeCapabilityFamilyMap
    {
        public static RuntimeCapabilityFamily[] GetFamilies(
            AuthoringCapability capability,
            RuntimeCapabilityLaneTag lane = RuntimeCapabilityLaneTag.Mixed,
            AuthoringWorldAxiom axioms = AuthoringWorldAxiom.None)
        {
            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();

            AddFamilyForCapability(families, capability);

            if (lane == RuntimeCapabilityLaneTag.TabletopBoard)
                AddFamily(families, RuntimeCapabilityFamily.BoardCardTabletop);

            if (lane == RuntimeCapabilityLaneTag.CameraCursor)
                AddFamily(families, RuntimeCapabilityFamily.CameraInput);

            if ((axioms & AuthoringWorldAxiom.InfiniteSpace) != 0 &&
                HasAnyCapability(capability, AuthoringCapability.Environment))
                AddFamily(families, RuntimeCapabilityFamily.ProceduralGeneration);

            if ((axioms & AuthoringWorldAxiom.Networked) != 0)
                AddFamily(families, RuntimeCapabilityFamily.Networking);

            return families.ToArray();
        }

        public static bool CapabilityMatchesFamily(AuthoringCapability capability, RuntimeCapabilityFamily family)
        {
            RuntimeCapabilityFamily[] families = GetFamilies(capability);
            for (int i = 0; i < families.Length; i++)
            {
                if (families[i] == family)
                    return true;
            }

            return false;
        }

        private static void AddFamilyForCapability(List<RuntimeCapabilityFamily> families, AuthoringCapability capability)
        {
            if (HasAnyCapability(
                    capability,
                    AuthoringCapability.Setup,
                    AuthoringCapability.Session,
                    AuthoringCapability.Participants))
            {
                AddFamily(families, RuntimeCapabilityFamily.PlatformCore);
                AddFamily(families, RuntimeCapabilityFamily.CharacterPawnGameplay);
            }

            if (HasAnyCapability(
                    capability,
                    AuthoringCapability.Movement,
                    AuthoringCapability.KineticMotor2D,
                    AuthoringCapability.KineticMotor3D,
                    AuthoringCapability.Steering2D,
                    AuthoringCapability.Steering3D,
                    AuthoringCapability.Traversal))
            {
                AddFamily(families, RuntimeCapabilityFamily.CharacterPawnGameplay);
            }

            if (HasAnyCapability(
                    capability,
                    AuthoringCapability.Combat,
                    AuthoringCapability.CombatState,
                    AuthoringCapability.CombatSensors,
                    AuthoringCapability.MeleeFlow,
                    AuthoringCapability.TacticsAggressive,
                    AuthoringCapability.TacticsDefensive))
            {
                AddFamily(families, RuntimeCapabilityFamily.Combat);
            }

            if (HasAnyCapability(capability, AuthoringCapability.RangedFlow))
            {
                AddFamily(families, RuntimeCapabilityFamily.GunsProjectiles);
                AddFamily(families, RuntimeCapabilityFamily.Combat);
            }

            if (HasAnyCapability(
                    capability,
                    AuthoringCapability.TurnBased,
                    AuthoringCapability.Rules,
                    AuthoringCapability.Puzzle))
            {
                AddFamily(families, RuntimeCapabilityFamily.ActionTargeting);
            }

            if (HasAnyCapability(capability, AuthoringCapability.Tabletop, AuthoringCapability.Grid))
                AddFamily(families, RuntimeCapabilityFamily.BoardCardTabletop);

            if (HasAnyCapability(capability, AuthoringCapability.Input))
            {
                AddFamily(families, RuntimeCapabilityFamily.CameraInput);
                AddFamily(families, RuntimeCapabilityFamily.ActionTargeting);
                AddFamily(families, RuntimeCapabilityFamily.CharacterPawnGameplay);
            }

            if (HasAnyCapability(capability, AuthoringCapability.Camera))
                AddFamily(families, RuntimeCapabilityFamily.CameraInput);

            if (HasAnyCapability(capability, AuthoringCapability.Animation, AuthoringCapability.VFX))
                AddFamily(families, RuntimeCapabilityFamily.AnimationPresentation);

            if (HasAnyCapability(capability, AuthoringCapability.UI))
            {
                AddFamily(families, RuntimeCapabilityFamily.ActionTargeting);
                AddFamily(families, RuntimeCapabilityFamily.ScoringObjectives);
            }

            if (HasAnyCapability(capability, AuthoringCapability.Scoring))
                AddFamily(families, RuntimeCapabilityFamily.ScoringObjectives);

            if (HasAnyCapability(capability, AuthoringCapability.Environment))
                AddFamily(families, RuntimeCapabilityFamily.ProceduralGeneration);

            if (HasAnyCapability(capability, AuthoringCapability.Networking))
                AddFamily(families, RuntimeCapabilityFamily.Networking);
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

        private static void AddFamily(List<RuntimeCapabilityFamily> families, RuntimeCapabilityFamily family)
        {
            if (!families.Contains(family))
                families.Add(family);
        }
    }
}
