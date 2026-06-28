using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    public partial class EnemyAI
    {
        private void ApplyProfile(EnemyProfile profile)
        {
            if (profile == null) return;
            if (profile.combatProfile != null) _runtime?.CombatProfileReceiver?.ApplyCombatProfile(profile.combatProfile);
        }

        private void ResolveDirectCapabilities()
        {
            _reactionState = GetComponent<IEnemyReactionState>();
        }
    }
}
