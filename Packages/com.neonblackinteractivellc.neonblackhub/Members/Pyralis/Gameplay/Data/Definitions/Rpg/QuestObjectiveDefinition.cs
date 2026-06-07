using System;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct QuestObjectiveDefinition
    {
        public string objectiveId;
        public QuestObjectiveKind kind;
        public string targetId;
        public int requiredQuantity;

        public QuestObjectiveDefinition(string objectiveId, QuestObjectiveKind kind, string targetId, int requiredQuantity)
        {
            this.objectiveId = string.IsNullOrWhiteSpace(objectiveId) ? string.Empty : objectiveId.Trim();
            this.kind = kind;
            this.targetId = string.IsNullOrWhiteSpace(targetId) ? string.Empty : targetId.Trim();
            this.requiredQuantity = requiredQuantity < 1 ? 1 : requiredQuantity;
        }

        public string ObjectiveId => string.IsNullOrWhiteSpace(objectiveId) ? string.Empty : objectiveId.Trim();
        public string TargetId => string.IsNullOrWhiteSpace(targetId) ? string.Empty : targetId.Trim();
        public int RequiredQuantity => requiredQuantity < 1 ? 1 : requiredQuantity;

        public bool IsValid => !string.IsNullOrWhiteSpace(ObjectiveId) && !string.IsNullOrWhiteSpace(TargetId);

        public void Sanitize()
        {
            objectiveId = ObjectiveId;
            targetId = TargetId;
            requiredQuantity = RequiredQuantity;
        }

        public QuestObjective CreateRuntimeObjective()
        {
            return new QuestObjective(ObjectiveId, kind, TargetId, RequiredQuantity);
        }
    }
}
