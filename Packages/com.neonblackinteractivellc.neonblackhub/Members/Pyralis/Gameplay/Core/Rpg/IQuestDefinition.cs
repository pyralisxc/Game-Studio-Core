namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IQuestDefinition
    {
        string QuestId { get; }
        bool Repeatable { get; }
        QuestObjective[] Objectives { get; }
        QuestReward[] Rewards { get; }
        bool TryGetObjective(string objectiveId, out QuestObjective objective);
    }
}
