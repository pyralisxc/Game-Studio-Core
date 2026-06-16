using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Features.Enemies
{
    public partial class EnemyCombatModule
    {
        public void ApplyCombatProfile(EnemyCombatProfile profile)
        {
            if (profile == null) return;

            profile.Sanitize();
            attackSequence = profile.attackSequence;
            attackMode = profile.attackMode;
            usePrioritySelection = profile.usePrioritySelection;
            preferAttacksCurrentlyInRange = profile.preferAttacksCurrentlyInRange;
            attackCooldown = profile.attackCooldown;
            attackRangeOverride = profile.attackRangeOverride;
            rangeWeight = profile.rangeWeight;
            damageWeight = profile.damageWeight;
            knockbackWeight = profile.knockbackWeight;
            assetPriorityWeight = profile.assetPriorityWeight;
        }
    }
}
