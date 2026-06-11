using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.skilltree.definition",
        Capability = AuthoringCapability.Stats,
        Lane = "RPG",
        AssignmentFields = new[] { nameof(treeId), nameof(displayName), nameof(nodes) },
        FirstProof = "Proof that the skill tree contains valid nodes and prerequisites are correctly linked."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Skill Tree", fileName = "SkillTreeDefinition")]
    public class SkillTreeDefinition : ScriptableObject, ISkillTree, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            return GetValidationIssues();
        }

        public string treeId = "skilltree.new";
        public string displayName = "New Skill Tree";
        public SkillNodeDefinition[] nodes = System.Array.Empty<SkillNodeDefinition>();

        public void Sanitize()
        {
            treeId = !string.IsNullOrWhiteSpace(treeId) ? treeId.Trim() : treeId;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : treeId;
            nodes ??= System.Array.Empty<SkillNodeDefinition>();
            for (int i = 0; i < nodes.Length; i++)
                nodes[i].Sanitize();
        }

        public bool TryGetNode(string nodeId, out SkillNodeDefinition node)
        {
            string normalizedNodeId = Normalize(nodeId);
            if (string.IsNullOrEmpty(normalizedNodeId) || nodes == null)
            {
                node = default;
                return false;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].NodeId == normalizedNodeId)
                {
                    node = nodes[i];
                    return true;
                }
            }

            node = default;
            return false;
        }

        bool ISkillTree.TryGetNode(string nodeId, out SkillNode node)
        {
            if (!TryGetNode(nodeId, out SkillNodeDefinition definition))
            {
                node = default;
                return false;
            }

            node = definition.CreateRuntimeNode();
            return true;
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(treeId))
                issues.Add("Skill tree id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            HashSet<string> nodeIds = new HashSet<string>();
            if (nodes == null)
                return issues;

            for (int i = 0; i < nodes.Length; i++)
            {
                SkillNodeDefinition node = nodes[i];
                string nodeId = node.NodeId;
                if (string.IsNullOrWhiteSpace(nodeId))
                {
                    issues.Add($"Nodes[{i}] node id is required.");
                    continue;
                }

                if (!nodeIds.Add(nodeId))
                    issues.Add($"Skill node `{nodeId}` is assigned more than once.");

                if (string.IsNullOrWhiteSpace(node.DisplayName))
                    issues.Add($"Skill node `{nodeId}` display name is required.");

                if (node.cost < 0)
                    issues.Add($"Skill node `{nodeId}` cost cannot be negative.");

                StatModifierDefinition[] modifiers = node.StatModifiers;
                for (int modifierIndex = 0; modifierIndex < modifiers.Length; modifierIndex++)
                {
                    if (!modifiers[modifierIndex].IsValid)
                        issues.Add($"Skill node `{nodeId}` Stat Modifiers[{modifierIndex}] Stat id is required.");
                }
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                string nodeId = nodes[i].NodeId;
                string[] prerequisites = nodes[i].PrerequisiteIds;
                for (int prerequisiteIndex = 0; prerequisiteIndex < prerequisites.Length; prerequisiteIndex++)
                {
                    if (!nodeIds.Contains(prerequisites[prerequisiteIndex]))
                        issues.Add($"Skill node `{nodeId}` references missing prerequisite `{prerequisites[prerequisiteIndex]}`.");
                }
            }

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
