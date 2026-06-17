using System.Collections.Generic;

namespace NeonBlack.Gameplay.Features.Characters
{
    public partial class PawnCombatBehaviour2D
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (hitBoxZones == null || hitBoxZones.Length == 0)
                yield return "Hit Box Zones is empty. Melee attacks need HitBox2D slots.";
            if (attackCooldown < 0f)
                yield return "Attack Cooldown cannot be negative.";
        }
    }
}
