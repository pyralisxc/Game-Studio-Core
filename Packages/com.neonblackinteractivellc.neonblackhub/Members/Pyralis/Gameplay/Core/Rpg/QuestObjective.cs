namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct QuestObjective
    {
        public QuestObjective(string objectiveId, QuestObjectiveKind kind, string targetId, int requiredQuantity)
        {
            ObjectiveId = string.IsNullOrWhiteSpace(objectiveId) ? string.Empty : objectiveId.Trim();
            Kind = kind;
            TargetId = string.IsNullOrWhiteSpace(targetId) ? string.Empty : targetId.Trim();
            RequiredQuantity = requiredQuantity < 1 ? 1 : requiredQuantity;
        }

        public string ObjectiveId { get; }
        public QuestObjectiveKind Kind { get; }
        public string TargetId { get; }
        public int RequiredQuantity { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(ObjectiveId) && !string.IsNullOrWhiteSpace(TargetId);
    }
}
