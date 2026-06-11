using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing quests and objectives.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Quests,
        Relevance = "Interface for starting and tracking quest objectives.",
        ExpertAdvice = "Use ReportObjectiveProgress to update quest states. Quests are indexed by RpgOwnerKey for persistence.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/rpg"
    )]
    public interface IQuestService
{
        bool TryStartQuest(RpgOwnerKey owner, IQuestDefinition quest, out string issue);
        bool ReportObjectiveProgress(RpgOwnerKey owner, IQuestDefinition quest, string objectiveId, int amount, out QuestProgressState progress, out string issue);
        QuestProgressState GetProgress(RpgOwnerKey owner, string questId);
    }
}