using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Rpg
{
    public static class DialogueGraphEditorModel
    {
        public static bool AddNode(
            DialogueGraphDefinition graph,
            string nodeId,
            DialogueNodeKind kind,
            string speakerId,
            string lineText,
            bool recordUndo = true)
        {
            if (graph == null)
                return false;

            string normalizedNodeId = Normalize(nodeId);
            if (string.IsNullOrEmpty(normalizedNodeId) || graph.NodeDefinitions.Any(node => node.NodeId == normalizedNodeId))
                return false;

            BeginEdit(graph, "Add Dialogue Node", recordUndo);
            List<DialogueNodeDefinition> nodes = graph.NodeDefinitions.ToList();
            nodes.Add(new DialogueNodeDefinition(normalizedNodeId, kind, speakerId, lineText, string.Empty));
            graph.nodes = nodes.ToArray();
            if (string.IsNullOrWhiteSpace(graph.startNodeId))
                graph.startNodeId = normalizedNodeId;

            EndEdit(graph);
            return true;
        }

        public static bool RemoveNode(DialogueGraphDefinition graph, string nodeId, bool recordUndo = true)
        {
            if (graph == null)
                return false;

            string normalizedNodeId = Normalize(nodeId);
            DialogueNodeDefinition[] nodes = graph.NodeDefinitions;
            if (string.IsNullOrEmpty(normalizedNodeId) || nodes.All(node => node.NodeId != normalizedNodeId))
                return false;

            BeginEdit(graph, "Remove Dialogue Node", recordUndo);
            List<DialogueNodeDefinition> remainingNodes = nodes.Where(node => node.NodeId != normalizedNodeId).ToList();
            for (int i = 0; i < remainingNodes.Count; i++)
            {
                DialogueNodeDefinition node = remainingNodes[i];
                if (node.NextNodeId == normalizedNodeId)
                    node.nextNodeId = string.Empty;

                DialogueChoiceDefinition[] choices = node.Choices;
                for (int choiceIndex = 0; choiceIndex < choices.Length; choiceIndex++)
                {
                    if (choices[choiceIndex].NextNodeId == normalizedNodeId)
                        choices[choiceIndex].nextNodeId = string.Empty;
                }

                node.choices = choices;
                remainingNodes[i] = node;
            }

            graph.nodes = remainingNodes.ToArray();
            if (graph.StartNodeId == normalizedNodeId)
                graph.startNodeId = graph.nodes.Length > 0 ? graph.nodes[0].NodeId : string.Empty;

            EndEdit(graph);
            return true;
        }

        public static bool AddChoice(
            DialogueGraphDefinition graph,
            string nodeId,
            string choiceId,
            string text,
            string nextNodeId,
            bool recordUndo = true)
        {
            if (graph == null)
                return false;

            string normalizedNodeId = Normalize(nodeId);
            string normalizedChoiceId = Normalize(choiceId);
            if (string.IsNullOrEmpty(normalizedNodeId) || string.IsNullOrEmpty(normalizedChoiceId))
                return false;

            DialogueNodeDefinition[] nodes = graph.NodeDefinitions;
            int nodeIndex = Array.FindIndex(nodes, node => node.NodeId == normalizedNodeId);
            if (nodeIndex < 0 || nodes[nodeIndex].Choices.Any(choice => choice.ChoiceId == normalizedChoiceId))
                return false;

            BeginEdit(graph, "Add Dialogue Choice", recordUndo);
            DialogueNodeDefinition editedNode = nodes[nodeIndex];
            List<DialogueChoiceDefinition> choices = editedNode.Choices.ToList();
            choices.Add(new DialogueChoiceDefinition
            {
                choiceId = normalizedChoiceId,
                text = string.IsNullOrWhiteSpace(text) ? normalizedChoiceId : text.Trim(),
                nextNodeId = Normalize(nextNodeId),
                conditions = Array.Empty<DialogueConditionDefinition>(),
                effects = Array.Empty<DialogueEffectDefinition>()
            });

            editedNode.choices = choices.ToArray();
            nodes[nodeIndex] = editedNode;
            graph.nodes = nodes;
            EndEdit(graph);
            return true;
        }

        public static string[] GetNodeIds(DialogueGraphDefinition graph)
        {
            return graph == null
                ? Array.Empty<string>()
                : graph.NodeDefinitions.Select(node => node.NodeId).Where(id => !string.IsNullOrWhiteSpace(id)).ToArray();
        }

        public static bool CanPreview(DialogueGraphDefinition graph, out string issue)
        {
            if (graph == null)
            {
                issue = "Assign a dialogue graph before previewing.";
                return false;
            }

            graph.Sanitize();
            List<string> issues = graph.GetValidationIssues();
            if (issues.Count > 0)
            {
                issue = issues[0];
                return false;
            }

            issue = string.Empty;
            return true;
        }

        private static void BeginEdit(DialogueGraphDefinition graph, string label, bool recordUndo)
        {
            if (recordUndo)
                Undo.RecordObject(graph, label);
        }

        private static void EndEdit(DialogueGraphDefinition graph)
        {
            graph.Sanitize();
            EditorUtility.SetDirty(graph);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
