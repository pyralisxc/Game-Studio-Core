using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing character progression and experience.
    /// </summary>
    public interface IProgressionService
    {
        ProgressionState GetState(RpgOwnerKey owner);
        ProgressionState AddExperience(RpgOwnerKey owner, int amount);
        ProgressionState GrantSkillPoints(RpgOwnerKey owner, int amount);
        bool TrySpendSkillPoints(RpgOwnerKey owner, int amount, out string issue);
    }
}