using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
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
