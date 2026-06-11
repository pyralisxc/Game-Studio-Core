using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing dialogue sessions and NPC interactions.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Dialogue,
        Relevance = "Interface for starting and managing branching conversations with NPCs.",
        ExpertAdvice = "Use TryStartSession to begin a conversation. This service manages the dialogue graph state and available choices.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/rpg"
    )]
    public interface IDialogueService
{
        bool TryStartSession(RpgOwnerKey owner, INpcProfile npc, IDialogueGraph graph, out DialogueSessionState state, out string issue);
        DialogueChoice[] GetAvailableChoices(RpgOwnerKey owner, IDialogueGraph graph);
        void SetDialogueFlag(RpgOwnerKey owner, string flagId, bool value);
        bool HasDialogueFlag(RpgOwnerKey owner, string flagId);
    }
}