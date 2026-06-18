using System.Collections.Generic;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Characters
{
    public partial class PawnCombatBehaviour2D
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (hitBoxZones == null || hitBoxZones.Length == 0)
                yield return PyralisRuntimeValidationIssue.Required("Hit Box Zones is empty. Melee attacks need HitBox2D slots.");
            if (attackCooldown < 0f)
                yield return PyralisRuntimeValidationIssue.Required("Attack Cooldown cannot be negative.");
        }
    }
}
