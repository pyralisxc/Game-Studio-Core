using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Modules.Combat
{
    public partial class PawnCombatBehaviour2D
    {
        public void ApplyCombatProfile(PawnProfileApplicationContext context, PawnCombatProfile profile)
        {
            if (profile == null)
                return;

            baseDamage = profile.baseDamage;
            baseKnockback = profile.baseKnockback;
            attackCooldown = profile.attackCooldown;
            kickCooldown = profile.kickCooldown;
            comboResetTime = profile.comboResetTime;
            combatWindow = profile.combatWindow;
            attackWeapon = profile.attackWeapon;
            kickWeapon = profile.kickWeapon;
            primarySequence = profile.primarySequence;
            secondarySequence = profile.secondarySequence;
            ApplyActiveWeapon();
        }
    }
}
