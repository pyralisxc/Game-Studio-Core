using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing skill tree unlocks and applications.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.SkillTree,
        Relevance = "Interface for unlocking abilities and applying skill effects to actors.",
        ExpertAdvice = "Use IsUnlocked to gate gameplay logic. Skill points are usually managed via the Progression service.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/skill-tree"
    )]
    public interface ISkillTreeService
{
        bool TryUnlock(RpgOwnerKey owner, ISkillTree tree, string nodeId, out string issue);
        bool IsUnlocked(RpgOwnerKey owner, string nodeId);
        int GetUnlockCount(RpgOwnerKey owner, string nodeId);
        void ApplySkillEffects(RpgOwnerKey owner, ISkillTree tree, StatSheet statSheet);
    }
}