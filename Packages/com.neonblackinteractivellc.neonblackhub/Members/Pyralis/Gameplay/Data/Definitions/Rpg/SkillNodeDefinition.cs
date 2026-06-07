using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct SkillNodeDefinition
    {
        public string nodeId;
        public string displayName;
        public int cost;
        public bool repeatable;
        public string[] prerequisiteNodeIds;
        public StatModifierDefinition[] statModifiers;

        public SkillNodeDefinition(string nodeId, string displayName, int cost, string[] prerequisiteNodeIds = null)
        {
            this.nodeId = string.IsNullOrWhiteSpace(nodeId) ? string.Empty : nodeId.Trim();
            this.displayName = string.IsNullOrWhiteSpace(displayName) ? this.nodeId : displayName.Trim();
            this.cost = cost;
            repeatable = false;
            this.prerequisiteNodeIds = prerequisiteNodeIds ?? Array.Empty<string>();
            statModifiers = Array.Empty<StatModifierDefinition>();
        }

        public string NodeId => string.IsNullOrWhiteSpace(nodeId) ? string.Empty : nodeId.Trim();
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? NodeId : displayName.Trim();
        public int Cost => cost < 0 ? 0 : cost;
        public string[] PrerequisiteIds => prerequisiteNodeIds == null
            ? Array.Empty<string>()
            : prerequisiteNodeIds
                .Select(id => string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToArray();

        public StatModifierDefinition[] StatModifiers => statModifiers ?? Array.Empty<StatModifierDefinition>();

        public void Sanitize()
        {
            nodeId = NodeId;
            displayName = DisplayName;
            cost = Cost;
            prerequisiteNodeIds = PrerequisiteIds;
            if (statModifiers == null)
            {
                statModifiers = Array.Empty<StatModifierDefinition>();
                return;
            }

            for (int i = 0; i < statModifiers.Length; i++)
                statModifiers[i].Sanitize();
        }

        public SkillNode CreateRuntimeNode()
        {
            StatModifier[] modifiers = StatModifiers
                .Where(modifier => modifier.IsValid)
                .Select(modifier => new StatModifier(modifier.StatId, modifier.Value, string.Empty))
                .ToArray();

            return new SkillNode(NodeId, Cost, repeatable, PrerequisiteIds, modifiers);
        }
    }
}
