using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Characters
{
    public interface IPawnCombatModule
    {
        void ApplyCombatProfile(PawnProfileApplicationContext context, PawnCombatProfile combatProfile);
    }
}
