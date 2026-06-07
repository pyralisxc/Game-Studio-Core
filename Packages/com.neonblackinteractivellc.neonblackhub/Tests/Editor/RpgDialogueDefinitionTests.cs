using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class RpgDialogueDefinitionTests
    {
        [Test]
        public void NpcDefinition_GetValidationIssues_RequiresNpcId()
        {
            NpcDefinition npc = ScriptableObject.CreateInstance<NpcDefinition>();
            npc.npcId = string.Empty;
            npc.displayName = "Village Elder";

            Assert.That(npc.GetValidationIssues().Any(issue => issue.Contains("NPC id")), Is.True);

            Object.DestroyImmediate(npc);
        }

        [Test]
        public void DialogueGraphDefinition_GetValidationIssues_FlagsDuplicateNodeIds()
        {
            DialogueGraphDefinition graph = CreateGraph(
                new DialogueNodeDefinition("node.start", DialogueNodeKind.Line, "npc.elder", "Hello.", "node.end"),
                new DialogueNodeDefinition("node.start", DialogueNodeKind.Line, "npc.elder", "Again.", "node.end"));

            Assert.That(graph.GetValidationIssues().Any(issue => issue.Contains("assigned more than once")), Is.True);

            Object.DestroyImmediate(graph);
        }

        [Test]
        public void DialogueGraphDefinition_GetValidationIssues_FlagsBrokenNextNode()
        {
            DialogueGraphDefinition graph = CreateGraph(
                new DialogueNodeDefinition("node.start", DialogueNodeKind.Line, "npc.elder", "Hello.", "node.missing"));

            Assert.That(graph.GetValidationIssues().Any(issue => issue.Contains("node.missing")), Is.True);

            Object.DestroyImmediate(graph);
        }

        [Test]
        public void DialogueGraphDefinition_GetValidationIssues_FlagsInvalidConditionTarget()
        {
            DialogueNodeDefinition node = new DialogueNodeDefinition("node.start", DialogueNodeKind.ChoiceHub, "npc.elder", "Choose.", string.Empty)
            {
                choices = new[]
                {
                    new DialogueChoiceDefinition
                    {
                        choiceId = "choice.locked",
                        text = "Locked",
                        nextNodeId = "node.end",
                        conditions = new[] { new DialogueConditionDefinition { kind = DialogueConditionKind.ItemCount, targetId = string.Empty, requiredQuantity = 1, expected = true } }
                    }
                }
            };
            DialogueGraphDefinition graph = CreateGraph(
                node,
                new DialogueNodeDefinition("node.end", DialogueNodeKind.Terminal, "npc.elder", string.Empty, string.Empty));

            Assert.That(graph.GetValidationIssues().Any(issue => issue.Contains("Condition") && issue.Contains("Target id")), Is.True);

            Object.DestroyImmediate(graph);
        }

        [Test]
        public void DialogueGraphDefinition_GetValidationIssues_FlagsInvalidEffectTarget()
        {
            DialogueNodeDefinition node = new DialogueNodeDefinition("node.start", DialogueNodeKind.ChoiceHub, "npc.elder", "Choose.", string.Empty)
            {
                choices = new[]
                {
                    new DialogueChoiceDefinition
                    {
                        choiceId = "choice.reward",
                        text = "Reward",
                        nextNodeId = "node.end",
                        effects = new[] { new DialogueEffectDefinition { kind = DialogueEffectKind.GrantItem, targetId = string.Empty, quantity = 1, boolValue = true } }
                    }
                }
            };
            DialogueGraphDefinition graph = CreateGraph(
                node,
                new DialogueNodeDefinition("node.end", DialogueNodeKind.Terminal, "npc.elder", string.Empty, string.Empty));

            Assert.That(graph.GetValidationIssues().Any(issue => issue.Contains("Effect") && issue.Contains("Target id")), Is.True);

            Object.DestroyImmediate(graph);
        }

        [Test]
        public void DialogueGraphDefinition_TryGetNodeDefinition_FindsNodeById()
        {
            DialogueGraphDefinition graph = CreateGraph(
                new DialogueNodeDefinition("node.start", DialogueNodeKind.Line, "npc.elder", "Hello.", "node.end"));

            Assert.That(graph.TryGetNodeDefinition("node.start", out DialogueNodeDefinition node), Is.True);
            Assert.That(node.lineText, Is.EqualTo("Hello."));

            Object.DestroyImmediate(graph);
        }

        private static DialogueGraphDefinition CreateGraph(params DialogueNodeDefinition[] nodes)
        {
            DialogueGraphDefinition graph = ScriptableObject.CreateInstance<DialogueGraphDefinition>();
            graph.graphId = "dialogue.test";
            graph.displayName = "Test Dialogue";
            graph.startNodeId = "node.start";
            graph.nodes = nodes;
            return graph;
        }
    }
}
