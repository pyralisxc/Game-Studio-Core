using NeonBlack.Gameplay.Data.Rpg;

namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    /// <summary>
    /// Service for managing skill tree unlocks and applications.
    /// </summary>
    public interface ISkillTreeService
{
        bool TryUnlock(RpgOwnerKey owner, ISkillTree tree, string nodeId, out string issue);
        bool IsUnlocked(RpgOwnerKey owner, string nodeId);
        int GetUnlockCount(RpgOwnerKey owner, string nodeId);
        void ApplySkillEffects(RpgOwnerKey owner, ISkillTree tree, StatSheet statSheet);
    }
}
