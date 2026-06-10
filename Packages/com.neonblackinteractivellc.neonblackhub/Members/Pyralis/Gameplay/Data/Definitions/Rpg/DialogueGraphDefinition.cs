using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.dialogue.graph",
        Capability = AuthoringCapability.Dialogue,
        Lane = "RPG",
        AssignmentFields = new[] { nameof(graphId), nameof(displayName), nameof(startNodeId), nameof(nodes) },
        FirstProof = "Proof that the dialogue graph can be traversed and contains at least one terminal node."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Dialogue Graph", fileName = "DialogueGraphDefinition")]
    public class DialogueGraphDefinition : ScriptableObject, IDialogueGraph
{
        public string graphId = "dialogue.new";
        public string displayName = "New Dialogue";
        public string startNodeId = "node.start";
        public DialogueNodeDefinition[] nodes = Array.Empty<DialogueNodeDefinition>();

        public string GraphId => Normalize(graphId);
        public string StartNodeId => Normalize(startNodeId);
        public DialogueNodeDefinition[] NodeDefinitions => nodes ?? Array.Empty<DialogueNodeDefinition>();
        public DialogueNode[] Nodes => NodeDefinitions.Select(node => node.CreateRuntimeNode()).ToArray();

        public void Sanitize()
        {
            graphId = GraphId;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : graphId;
            startNodeId = StartNodeId;
            nodes = NodeDefinitions;
            for (int i = 0; i < nodes.Length; i++)
                nodes[i].Sanitize();
        }

        public bool TryGetNodeDefinition(string nodeId, out DialogueNodeDefinition node)
        {
            string normalizedNodeId = Normalize(nodeId);
            if (string.IsNullOrEmpty(normalizedNodeId))
            {
                node = default;
                return false;
            }

            DialogueNodeDefinition[] definitions = NodeDefinitions;
            for (int i = 0; i < definitions.Length; i++)
            {
                if (definitions[i].NodeId == normalizedNodeId)
                {
                    node = definitions[i];
                    return true;
                }
            }

            node = default;
            return false;
        }

        public bool TryGetNode(string nodeId, out DialogueNode node)
        {
            if (!TryGetNodeDefinition(nodeId, out DialogueNodeDefinition definition))
            {
                node = default;
                return false;
            }

            node = definition.CreateRuntimeNode();
            return true;
        }

        public DialogueGraph CreateRuntimeGraph()
        {
            return new DialogueGraph(GraphId, StartNodeId, Nodes);
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(graphId))
                issues.Add("Dialogue graph id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (string.IsNullOrWhiteSpace(startNodeId))
                issues.Add("Start node id is required.");

            DialogueNodeDefinition[] definitions = NodeDefinitions;
            if (definitions.Length == 0)
                issues.Add("At least one dialogue node is required.");

            HashSet<string> nodeIds = new HashSet<string>();
            for (int i = 0; i < definitions.Length; i++)
            {
                DialogueNodeDefinition node = definitions[i];
                string nodeId = node.NodeId;
                if (string.IsNullOrWhiteSpace(nodeId))
                {
                    issues.Add($"Nodes[{i}] Node id is required.");
                    continue;
                }

                if (!nodeIds.Add(nodeId))
                    issues.Add($"Dialogue node `{nodeId}` is assigned more than once.");

                if (node.kind != DialogueNodeKind.Terminal && string.IsNullOrWhiteSpace(node.SpeakerId))
                    issues.Add($"Dialogue node `{nodeId}` Speaker id is required.");

                if (node.kind == DialogueNodeKind.Line && string.IsNullOrWhiteSpace(node.lineText))
                    issues.Add($"Dialogue node `{nodeId}` line text is required.");
            }

            if (!string.IsNullOrWhiteSpace(startNodeId) && !nodeIds.Contains(StartNodeId))
                issues.Add($"Start node `{StartNodeId}` could not be found.");

            for (int i = 0; i < definitions.Length; i++)
            {
                ValidateNodeLinks(definitions[i], nodeIds, issues);
                ValidateNodeConditionsAndEffects(definitions[i], issues);
            }

            return issues;
        }

        private static void ValidateNodeLinks(DialogueNodeDefinition node, HashSet<string> nodeIds, List<string> issues)
        {
            string nodeId = node.NodeId;
            if (!string.IsNullOrWhiteSpace(node.NextNodeId) && !nodeIds.Contains(node.NextNodeId))
                issues.Add($"Dialogue node `{nodeId}` references missing next node `{node.NextNodeId}`.");

            DialogueChoiceDefinition[] choices = node.Choices;
            for (int i = 0; i < choices.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(choices[i].ChoiceId))
                    issues.Add($"Dialogue node `{nodeId}` Choices[{i}] Choice id is required.");

                if (string.IsNullOrWhiteSpace(choices[i].Text))
                    issues.Add($"Dialogue node `{nodeId}` Choices[{i}] text is required.");

                if (!string.IsNullOrWhiteSpace(choices[i].NextNodeId) && !nodeIds.Contains(choices[i].NextNodeId))
                    issues.Add($"Dialogue node `{nodeId}` Choices[{i}] references missing next node `{choices[i].NextNodeId}`.");
            }
        }

        private static void ValidateNodeConditionsAndEffects(DialogueNodeDefinition node, List<string> issues)
        {
            ValidateEffects(node.NodeId, "Node Effects", node.Effects, issues);

            DialogueChoiceDefinition[] choices = node.Choices;
            for (int choiceIndex = 0; choiceIndex < choices.Length; choiceIndex++)
            {
                DialogueConditionDefinition[] conditions = choices[choiceIndex].Conditions;
                for (int conditionIndex = 0; conditionIndex < conditions.Length; conditionIndex++)
                {
                    if (ConditionRequiresTarget(conditions[conditionIndex].kind) && string.IsNullOrWhiteSpace(conditions[conditionIndex].TargetId))
                        issues.Add($"Dialogue node `{node.NodeId}` Choices[{choiceIndex}] Conditions[{conditionIndex}] Target id is required.");

                    if (conditions[conditionIndex].requiredQuantity < 1)
                        issues.Add($"Dialogue node `{node.NodeId}` Choices[{choiceIndex}] Conditions[{conditionIndex}] required quantity must be at least 1.");
                }

                ValidateEffects(node.NodeId, $"Choices[{choiceIndex}] Effects", choices[choiceIndex].Effects, issues);
            }
        }

        private static void ValidateEffects(string nodeId, string ownerLabel, DialogueEffectDefinition[] effects, List<string> issues)
        {
            DialogueEffectDefinition[] safeEffects = effects ?? Array.Empty<DialogueEffectDefinition>();
            for (int i = 0; i < safeEffects.Length; i++)
            {
                if (EffectRequiresTarget(safeEffects[i].kind) && string.IsNullOrWhiteSpace(safeEffects[i].TargetId))
                    issues.Add($"Dialogue node `{nodeId}` {ownerLabel}[{i}] Target id is required.");

                if (safeEffects[i].quantity < 1)
                    issues.Add($"Dialogue node `{nodeId}` {ownerLabel}[{i}] quantity must be at least 1.");
            }
        }

        private void OnValidate()
        {
            Sanitize();
        }

        private static bool ConditionRequiresTarget(DialogueConditionKind kind)
        {
            return kind != DialogueConditionKind.Always;
        }

        private static bool EffectRequiresTarget(DialogueEffectKind kind)
        {
            return kind != DialogueEffectKind.GrantExperience && kind != DialogueEffectKind.GrantSkillPoints;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
