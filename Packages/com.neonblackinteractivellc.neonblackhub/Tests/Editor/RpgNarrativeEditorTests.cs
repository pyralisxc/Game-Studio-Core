using System.IO;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Editor.Rpg;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class RpgNarrativeEditorTests
    {
        [Test]
        public void DialogueGraphEditorModel_AddNode_AppendsUniqueNodeAndPreservesStart()
        {
            DialogueGraphDefinition graph = CreateGraph();

            bool added = DialogueGraphEditorModel.AddNode(graph, "node.second", DialogueNodeKind.Line, "npc.elder", "Second line.", false);

            Assert.That(added, Is.True);
            Assert.That(graph.startNodeId, Is.EqualTo("node.start"));
            Assert.That(graph.nodes.Select(node => node.NodeId), Does.Contain("node.second"));

            Object.DestroyImmediate(graph);
        }

        [Test]
        public void DialogueGraphEditorModel_AddChoice_AddsChoiceToExistingNode()
        {
            DialogueGraphDefinition graph = CreateGraph();
            DialogueGraphEditorModel.AddNode(graph, "node.end", DialogueNodeKind.Terminal, string.Empty, string.Empty, false);

            bool added = DialogueGraphEditorModel.AddChoice(graph, "node.start", "choice.accept", "Accept", "node.end", false);

            Assert.That(added, Is.True);
            Assert.That(graph.nodes[0].Choices.Single().ChoiceId, Is.EqualTo("choice.accept"));
            Assert.That(graph.nodes[0].Choices.Single().NextNodeId, Is.EqualTo("node.end"));

            Object.DestroyImmediate(graph);
        }

        [Test]
        public void DialogueGraphEditorModel_RemoveNode_ClearsStartAndChoiceReferences()
        {
            DialogueGraphDefinition graph = CreateGraph();
            DialogueGraphEditorModel.AddNode(graph, "node.end", DialogueNodeKind.Terminal, string.Empty, string.Empty, false);
            DialogueGraphEditorModel.AddChoice(graph, "node.start", "choice.end", "End", "node.end", false);

            bool removed = DialogueGraphEditorModel.RemoveNode(graph, "node.end", false);

            Assert.That(removed, Is.True);
            Assert.That(graph.nodes.Any(node => node.NodeId == "node.end"), Is.False);
            Assert.That(graph.nodes[0].Choices.Single().NextNodeId, Is.Empty);

            Object.DestroyImmediate(graph);
        }

        [Test]
        public void DialogueGraphEditorModel_CanPreview_RejectsGraphWithMissingStartNode()
        {
            DialogueGraphDefinition graph = CreateGraph();
            graph.startNodeId = "node.missing";

            Assert.That(DialogueGraphEditorModel.CanPreview(graph, out string issue), Is.False);
            Assert.That(issue, Does.Contain("node.missing"));

            Object.DestroyImmediate(graph);
        }

        [Test]
        public void DialogueGraphEditorWindow_Source_ExposesMenuValidationAndPreview()
        {
            string editorRoot = Path.Combine(Application.dataPath, "..", "Packages", "com.neonblackinteractivellc.neonblackhub", "Members", "Pyralis", "Gameplay", "Editor");
            string[] matches = Directory.GetFiles(editorRoot, "DialogueGraphEditorWindow.cs", SearchOption.AllDirectories);
            Assert.That(matches.Length, Is.EqualTo(1));
            string path = matches[0];
            string source = File.ReadAllText(path);

            Assert.That(source, Does.Contain("MenuItem(\"NeonBlack/Gameplay/RPG Narrative Editor\")"));
            Assert.That(source, Does.Contain("ObjectField"));
            Assert.That(source, Does.Contain("GetValidationIssues"));
            Assert.That(source, Does.Contain("DialogueService"));
            Assert.That(source, Does.Contain("TryStartSession"));
        }

        private static DialogueGraphDefinition CreateGraph()
        {
            DialogueGraphDefinition graph = ScriptableObject.CreateInstance<DialogueGraphDefinition>();
            graph.graphId = "dialogue.editor-test";
            graph.displayName = "Editor Test";
            graph.startNodeId = "node.start";
            graph.nodes = new[]
            {
                new DialogueNodeDefinition("node.start", DialogueNodeKind.Line, "npc.elder", "Hello.", string.Empty)
            };
            return graph;
        }
    }
}
