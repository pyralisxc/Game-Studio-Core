using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing interactions in RPG hubs.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Rpg,
        Relevance = "Interface for coordinating multiple RPG systems in a hub environment.",
        ExpertAdvice = "This service acts as a bridge between HubDefinitions and specific RPG systems like Quests and Skill Trees.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/rpg"
    )]
    public interface IHubInteractionService
{
        bool RegisterQuest(IQuestDefinition quest);
        bool RegisterSkillTree(string treeId, ISkillTree tree);
        HubInteractionResult[] GetAvailableInteractions(RpgOwnerKey owner, IHubDefinition hub);
        HubInteractionResult SelectInteraction(RpgOwnerKey owner, IHubDefinition hub, string interactableId);
    }
}