using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct SkillNode
    {
        public SkillNode(string nodeId, int cost, bool repeatable, string[] prerequisiteNodeIds, StatModifier[] statModifiers)
        {
            NodeId = string.IsNullOrWhiteSpace(nodeId) ? string.Empty : nodeId.Trim();
            Cost = cost < 0 ? 0 : cost;
            Repeatable = repeatable;
            PrerequisiteNodeIds = prerequisiteNodeIds ?? Array.Empty<string>();
            StatModifiers = statModifiers ?? Array.Empty<StatModifier>();
        }

        public string NodeId { get; }
        public int Cost { get; }
        public bool Repeatable { get; }
        public string[] PrerequisiteNodeIds { get; }
        public StatModifier[] StatModifiers { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(NodeId);
    }
}
