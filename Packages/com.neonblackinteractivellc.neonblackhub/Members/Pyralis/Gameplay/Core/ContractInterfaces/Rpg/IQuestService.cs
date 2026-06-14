using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Core.Contracts.Rpg
{
    /// <summary>
    /// Service for managing quests and objectives.
    /// </summary>
    public interface IQuestService
{
        bool TryStartQuest(RpgOwnerKey owner, IQuestDefinition quest, out string issue);
        bool ReportObjectiveProgress(RpgOwnerKey owner, IQuestDefinition quest, string objectiveId, int amount, out QuestProgressState progress, out string issue);
        QuestProgressState GetProgress(RpgOwnerKey owner, string questId);
    }
}
