# Pyralis Native Narrative Editor Phase 6B Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the first dedicated native narrative editor window for Pyralis dialogue graphs, including graph mutation helpers, validation display, and runtime preview.

**Architecture:** Keep dialogue data in the existing `DialogueGraphDefinition` model and add a thin editor-only helper for safe node/choice mutations. Build the first visual authoring surface as a stable IMGUI `EditorWindow` instead of depending on experimental graph tooling, while keeping the data model ready for a future graph-canvas UI.

**Tech Stack:** Unity 6000.4, C# editor assembly, NUnit editor tests, existing Pyralis RPG dialogue runtime and definitions.

---

### Task 1: Editor Model Tests

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/RpgNarrativeEditorTests.cs`
- Modify: `NeonBlack.Gameplay.Editor.Tests.csproj`

- [ ] **Step 1: Write failing editor model tests**

Create tests that call the wished-for `DialogueGraphEditorModel` API:

```csharp
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
```

- [ ] **Step 2: Run build and confirm red**

Run: `dotnet build "Game Studio Core.slnx" --no-restore`

Expected: fails because `DialogueGraphEditorModel` does not exist.

### Task 2: Editor Model Implementation

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/DialogueGraphEditorModel.cs`
- Modify: `NeonBlack.Gameplay.Editor.csproj`

- [ ] **Step 1: Implement safe graph mutations**

Add editor-only helpers for adding/removing nodes, adding choices, validation preview checks, and dirty/undo recording.

- [ ] **Step 2: Run editor model tests**

Run: `dotnet build "Game Studio Core.slnx" --no-restore`

Expected: build succeeds.

### Task 3: Narrative Editor Window

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/DialogueGraphEditorWindow.cs`
- Modify: `NeonBlack.Gameplay.Editor.csproj`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/RpgNarrativeEditorTests.cs`

- [ ] **Step 1: Add source contract test**

Add a test that confirms the window source exposes the menu path, graph object field, validation, and preview hooks.

- [ ] **Step 2: Implement window**

Build an IMGUI window at `NeonBlack/Gameplay/RPG Narrative Editor` with graph asset selection, node list/map, selected-node editor, choice controls, validation issues, and preview powered by `DialogueService`.

- [ ] **Step 3: Run build**

Run: `dotnet build "Game Studio Core.slnx" --no-restore`

Expected: build succeeds.

### Task 4: Documentation

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RPG_SYSTEMS_ROADMAP.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`

- [ ] **Step 1: Record the editor checkpoint**

Update durable docs to show Phase 6B has a native narrative editor window, stable IMGUI choice, preview support, and remaining future graph-canvas polish.

### Task 5: Validation

**Files:**
- No production edits expected.

- [ ] **Step 1: Run the full project gate**

Run: `& ".\Tools\Validation\Run-PreSceneValidation.ps1"`

Expected: restore/build passes, Unity EditMode passes, Unity PlayMode passes, final restore/build passes, residue scan clean.
