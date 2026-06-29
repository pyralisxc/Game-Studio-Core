using System.Collections.Generic;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public partial class PawnCombatBehaviour2D
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (hitBoxZones == null || hitBoxZones.Length == 0)
                yield return RuntimeValidationIssue.Required("Hit Box Zones is empty. Melee attacks need HitBox2D slots.");
            if (!HasActions(primarySequence))
                yield return RuntimeValidationIssue.Required("Primary Sequence needs at least one CombatActionDefinition. PawnCombatBehaviour2D does not invent local primary attacks.");
            if (!HasActions(secondarySequence))
                yield return RuntimeValidationIssue.Required("Secondary Sequence needs at least one CombatActionDefinition. PawnCombatBehaviour2D does not invent local secondary attacks.");
            if (attackCooldown < 0f)
                yield return RuntimeValidationIssue.Required("Attack Cooldown cannot be negative.");
        }

        private static bool HasActions(NeonBlack.Gameplay.Data.Definitions.Combat.CombatSequenceDefinition sequence)
        {
            return sequence != null && sequence.actions != null && sequence.actions.Length > 0;
        }
    }
}
