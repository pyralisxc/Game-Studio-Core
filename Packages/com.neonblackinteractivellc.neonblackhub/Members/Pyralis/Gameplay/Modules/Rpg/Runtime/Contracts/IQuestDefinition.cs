namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
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
