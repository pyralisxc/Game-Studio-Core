using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing interactions in RPG hubs.
    /// </summary>
    public interface IHubInteractionService
    {
        bool RegisterQuest(IQuestDefinition quest);
        bool RegisterSkillTree(string treeId, ISkillTree tree);
        HubInteractionResult[] GetAvailableInteractions(RpgOwnerKey owner, IHubDefinition hub);
        HubInteractionResult SelectInteraction(RpgOwnerKey owner, IHubDefinition hub, string interactableId);
    }
}