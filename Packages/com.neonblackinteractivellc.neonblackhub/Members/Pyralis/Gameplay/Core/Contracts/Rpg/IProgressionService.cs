using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing character progression and experience.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Progression,
        Relevance = "Interface for character experience and leveling systems.",
        ExpertAdvice = "Query this service to add XP or check level state. Use RpgOwnerKey to identify the target actor.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/rpg"
    )]
    public interface IProgressionService
{
        ProgressionState GetState(RpgOwnerKey owner);
        ProgressionState AddExperience(RpgOwnerKey owner, int amount);
        ProgressionState GrantSkillPoints(RpgOwnerKey owner, int amount);
        bool TrySpendSkillPoints(RpgOwnerKey owner, int amount, out string issue);
    }
}