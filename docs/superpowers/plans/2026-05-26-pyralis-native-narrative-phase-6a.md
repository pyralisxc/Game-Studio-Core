# Pyralis Native Narrative Phase 6A Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the Pyralis-native NPC/dialogue runtime spine and guided authoring foundation that can read RPG state, dispatch gameplay effects, and support a later visual narrative editor.

**Architecture:** Core owns dialogue runtime contracts, state, condition evaluation, and effect dispatch without referencing Unity authoring assets. Data owns ScriptableObject definitions for NPCs, dialogue graphs, nodes, choices, conditions, and effects that adapt into Core value types/interfaces. Editor owns guided inspectors and validation display, following the existing RPG authoring pattern.

**Tech Stack:** Unity 6000.4, C#/.NET Standard 2.1, ScriptableObjects, NUnit Unity EditMode/PlayMode tests, project pre-scene validation gate.

---

## File Structure

Core files under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/`:

- `DialogueConditionKind.cs`: condition enum for quest, inventory, skill, dialogue flag, project flag, faction placeholder, and custom hook checks.
- `DialogueEffectKind.cs`: effect enum for setting flags, quest progress, inventory/progression rewards, vendor/trainer/portal placeholders, and custom events.
- `DialogueNodeKind.cs`: node enum for line, choice hub, and terminal nodes.
- `DialogueCondition.cs`: runtime condition value.
- `DialogueEffect.cs`: runtime effect value.
- `DialogueChoice.cs`: runtime player choice value.
- `DialogueNode.cs`: runtime dialogue node value.
- `DialogueGraph.cs`: runtime graph value and node lookup.
- `NpcProfile.cs`: runtime NPC identity value.
- `DialogueSessionState.cs`: owner-scoped session snapshot.
- `IDialogueGraph.cs`: graph contract.
- `INpcProfile.cs`: NPC contract.
- `IDialogueConditionResolver.cs`: optional project condition hook.
- `IDialogueEffectSink.cs`: optional project effect hook.
- `DialogueService.cs`: evaluates available choices, advances sessions, tracks flags, and dispatches effects.

Data files under `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Data/Definitions/Rpg/`:

- `NpcDefinition.cs`: ScriptableObject NPC identity authoring.
- `DialogueConditionDefinition.cs`: serializable authoring condition.
- `DialogueEffectDefinition.cs`: serializable authoring effect.
- `DialogueChoiceDefinition.cs`: serializable authoring choice.
- `DialogueNodeDefinition.cs`: serializable authoring node.
- `DialogueGraphDefinition.cs`: ScriptableObject graph authoring and validation.

Editor file:

- Modify `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/RpgDefinitionEditors.cs`: add guided inspectors for `NpcDefinition` and `DialogueGraphDefinition`.

Tests:

- `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/RpgDialogueRuntimeTests.cs`
- `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/RpgDialogueDefinitionTests.cs`

Docs:

- Update `RPG_SYSTEMS_ROADMAP.md`, `RUNTIME_PARITY_MATRIX.md`, `FEATURE_INVENTORY.md`, and `FEATURE_DEVELOPMENT_ROADMAP.md`.

Generated project includes:

- Add new Core/Data/Test files to `NeonBlack.Gameplay.Core.csproj`, `NeonBlack.Gameplay.Data.csproj`, `NeonBlack.Gameplay.Tests.csproj`, and `NeonBlack.Gameplay.Editor.Tests.csproj` if Unity has not regenerated them.

---

### Task 1: Runtime Tests For Dialogue Spine

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/RpgDialogueRuntimeTests.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/RpgDialogueRuntimeTests.cs.meta`
- Modify: `NeonBlack.Gameplay.Tests.csproj`

- [ ] **Step 1: Write failing runtime tests**

Create tests covering:

```csharp
using NeonBlack.Gameplay.Core.Rpg;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgDialogueRuntimeTests
    {
        [Test]
        public void DialogueService_StartSession_TracksOwnerAndNpcSeparately()
        {
            DialogueService service = new DialogueService();
            DialogueGraph graph = DialogueTestFactory.CreateGraph();
            NpcProfile npc = new NpcProfile("npc.elder", "Village Elder", "quest-giver", new[] { "hub" }, "faction.village", string.Empty);
            RpgOwnerKey firstOwner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            RpgOwnerKey secondOwner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-2");

            Assert.That(service.TryStartSession(firstOwner, npc, graph, out DialogueSessionState firstState, out string firstIssue), Is.True, firstIssue);
            Assert.That(service.TryStartSession(secondOwner, npc, graph, out DialogueSessionState secondState, out string secondIssue), Is.True, secondIssue);

            Assert.That(firstState.Owner, Is.EqualTo(firstOwner));
            Assert.That(secondState.Owner, Is.EqualTo(secondOwner));
            Assert.That(firstState.CurrentNodeId, Is.EqualTo("node.start"));
            Assert.That(secondState.CurrentNodeId, Is.EqualTo("node.start"));
        }

        [Test]
        public void DialogueService_GetAvailableChoices_FiltersByInventoryAndDialogueFlag()
        {
            InventoryService inventory = new InventoryService();
            DialogueService service = new DialogueService(inventory: inventory);
            DialogueGraph graph = DialogueTestFactory.CreateGraphWithConditions();
            NpcProfile npc = DialogueTestFactory.CreateNpc();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryStartSession(owner, npc, graph, out _, out _), Is.True);
            DialogueChoice[] initialChoices = service.GetAvailableChoices(owner, graph);
            Assert.That(initialChoices.Length, Is.EqualTo(1));
            Assert.That(initialChoices[0].ChoiceId, Is.EqualTo("choice.hello"));

            Assert.That(inventory.TryAddItem(owner, "item.herb", 1, out string issue), Is.True, issue);
            service.SetDialogueFlag(owner, "flag.met.elder", true);

            DialogueChoice[] unlockedChoices = service.GetAvailableChoices(owner, graph);
            Assert.That(unlockedChoices.Length, Is.EqualTo(2));
            Assert.That(unlockedChoices[1].ChoiceId, Is.EqualTo("choice.herb"));
        }

        [Test]
        public void DialogueService_SelectChoice_DispatchesFlagQuestAndRewardEffects()
        {
            ProgressionService progression = new ProgressionService(null);
            InventoryService inventory = new InventoryService();
            QuestService quests = new QuestService(progression, inventory);
            DialogueService service = new DialogueService(progression, inventory, quests);
            DialogueGraph graph = DialogueTestFactory.CreateGraphWithEffects();
            TestQuest quest = new TestQuest();
            NpcProfile npc = DialogueTestFactory.CreateNpc();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryStartSession(owner, npc, graph, out _, out _), Is.True);
            Assert.That(service.RegisterQuest(quest), Is.True);
            Assert.That(service.TrySelectChoice(owner, graph, "choice.accept", out DialogueSessionState state, out string issue), Is.True, issue);

            Assert.That(service.HasDialogueFlag(owner, "flag.elder.accepted"), Is.True);
            Assert.That(quests.GetProgress(owner, "quest.elder").Status, Is.EqualTo(QuestStatus.Active));
            Assert.That(inventory.GetItemCount(owner, "item.token"), Is.EqualTo(1));
            Assert.That(progression.GetState(owner).Experience, Is.EqualTo(5));
            Assert.That(state.CurrentNodeId, Is.EqualTo("node.end"));
        }

        [Test]
        public void DialogueService_SelectChoice_RejectsUnavailableChoice()
        {
            DialogueService service = new DialogueService();
            DialogueGraph graph = DialogueTestFactory.CreateGraphWithConditions();
            NpcProfile npc = DialogueTestFactory.CreateNpc();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryStartSession(owner, npc, graph, out _, out _), Is.True);
            Assert.That(service.TrySelectChoice(owner, graph, "choice.herb", out _, out string issue), Is.False);
            Assert.That(issue, Does.Contain("not available"));
        }
    }
}
```

- [ ] **Step 2: Run build to verify RED**

Run: `dotnet build "Game Studio Core.slnx" --no-restore`

Expected: build fails because `DialogueService`, `DialogueGraph`, `NpcProfile`, and dialogue value types do not exist.

---

### Task 2: Core Dialogue Runtime

**Files:**
- Create all Core files listed in File Structure.
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Core/Rpg/QuestService.cs` only if a read/query helper is required by dialogue effects.
- Modify: `NeonBlack.Gameplay.Core.csproj`

- [ ] **Step 1: Implement Core value types and contracts**

Define small immutable structs/enums:

```csharp
public enum DialogueConditionKind { Always, QuestStatus, ItemCount, SkillUnlocked, DialogueFlag, ProjectFlag, Faction, Custom }
public enum DialogueEffectKind { SetDialogueFlag, StartQuest, ReportQuestObjective, GrantItem, GrantExperience, GrantSkillPoints, OpenVendor, OpenTrainer, OpenPortal, CustomEvent }
public enum DialogueNodeKind { Line, ChoiceHub, Terminal }
```

`DialogueCondition` should carry `Kind`, `TargetId`, `ComparisonValue`, `RequiredQuantity`, and `Expected` bool where needed. `DialogueEffect` should carry `Kind`, `TargetId`, `SecondaryTargetId`, `Quantity`, and `BoolValue`.

`DialogueGraph` should expose `GraphId`, `StartNodeId`, `Nodes`, and `TryGetNode`.

`NpcProfile` should expose `NpcId`, `DisplayName`, `Role`, `Tags`, `FactionId`, and `ActorLinkId`.

- [ ] **Step 2: Implement `DialogueService`**

`DialogueService` constructor should accept optional services:

```csharp
public DialogueService(
    ProgressionService progression = null,
    InventoryService inventory = null,
    QuestService quests = null,
    SkillTreeService skills = null,
    IDialogueConditionResolver customConditionResolver = null,
    IDialogueEffectSink customEffectSink = null)
```

Core methods:

- `TryStartSession(RpgOwnerKey owner, INpcProfile npc, IDialogueGraph graph, out DialogueSessionState state, out string issue)`
- `GetAvailableChoices(RpgOwnerKey owner, IDialogueGraph graph)`
- `TrySelectChoice(RpgOwnerKey owner, IDialogueGraph graph, string choiceId, out DialogueSessionState state, out string issue)`
- `SetDialogueFlag(RpgOwnerKey owner, string flagId, bool value)`
- `HasDialogueFlag(RpgOwnerKey owner, string flagId)`
- `RegisterQuest(IQuestDefinition quest)`

Condition handling for Phase 6A:

- `Always`: true.
- `ItemCount`: use `InventoryService.GetItemCount`.
- `DialogueFlag`: use internal dialogue flag store.
- `QuestStatus`: use `QuestService.GetProgress`.
- `Custom`: delegate to `IDialogueConditionResolver`.
- Other kinds should return false until their service integration exists, unless a custom resolver handles them.

Effect handling for Phase 6A:

- `SetDialogueFlag`: update internal flag store.
- `StartQuest`: look up registered quest and call `QuestService.TryStartQuest`.
- `ReportQuestObjective`: look up registered quest and call `QuestService.ReportObjectiveProgress`.
- `GrantItem`: call `InventoryService.TryAddItem`.
- `GrantExperience`: call `ProgressionService.AddExperience`.
- `GrantSkillPoints`: call `ProgressionService.GrantSkillPoints`.
- `CustomEvent`: delegate to `IDialogueEffectSink`.
- Vendor/trainer/portal placeholders should delegate to `IDialogueEffectSink` and otherwise no-op with success.

- [ ] **Step 3: Run build**

Run: `dotnet build "Game Studio Core.slnx" --no-restore`

Expected: runtime tests compile or fail only because test helper classes are missing.

---

### Task 3: Runtime Test Helpers And Green Runtime Tests

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Runtime/RpgDialogueRuntimeTests.cs`

- [ ] **Step 1: Add local test helpers**

Add `DialogueTestFactory` and `TestQuest` inside the test file. `TestQuest` should implement `IQuestDefinition` with quest id `quest.elder`, one `ProjectEvent` objective, and one small XP/item reward if needed.

Factories should create:

- one graph with start node `node.start`
- one graph with choices filtered by `ItemCount` and `DialogueFlag`
- one graph with effects that set `flag.elder.accepted`, start `quest.elder`, grant `item.token`, grant 5 XP, and route to `node.end`

- [ ] **Step 2: Run build and fix minimal runtime issues**

Run: `dotnet build "Game Studio Core.slnx" --no-restore`

Expected: runtime test assembly builds.

---

### Task 4: Editor Definition Tests

**Files:**
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/RpgDialogueDefinitionTests.cs`
- Create: `Packages/com.neonblackinteractivellc.neonblackhub/Tests/Editor/RpgDialogueDefinitionTests.cs.meta`
- Modify: `NeonBlack.Gameplay.Editor.Tests.csproj`

- [ ] **Step 1: Write failing editor tests**

Test cases:

- `NpcDefinition_GetValidationIssues_RequiresNpcId`
- `DialogueGraphDefinition_GetValidationIssues_FlagsDuplicateNodeIds`
- `DialogueGraphDefinition_GetValidationIssues_FlagsBrokenNextNode`
- `DialogueGraphDefinition_GetValidationIssues_FlagsInvalidConditionTarget`
- `DialogueGraphDefinition_TryGetNodeDefinition_FindsNodeById`

- [ ] **Step 2: Run build to verify RED**

Run: `dotnet build "Game Studio Core.slnx" --no-restore`

Expected: build fails because `NpcDefinition`, `DialogueGraphDefinition`, and related authoring definitions do not exist.

---

### Task 5: Data Definitions And Guided Editors

**Files:**
- Create all Data files listed in File Structure.
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/RpgDefinitionEditors.cs`
- Modify: `NeonBlack.Gameplay.Data.csproj`

- [ ] **Step 1: Implement Data definitions**

`NpcDefinition : ScriptableObject, INpcProfile`

Fields:

- `npcId`
- `displayName`
- `role`
- `tags`
- `factionId`
- `actorLinkId`

`DialogueGraphDefinition : ScriptableObject, IDialogueGraph`

Fields:

- `graphId`
- `displayName`
- `startNodeId`
- `nodes`

Serializable definitions should sanitize strings, clamp quantities to at least 1, and provide `CreateRuntime...` conversion methods.

- [ ] **Step 2: Implement validation**

`NpcDefinition.GetValidationIssues()`:

- missing NPC id
- missing display name
- duplicate/empty tags after sanitize

`DialogueGraphDefinition.GetValidationIssues()`:

- missing graph id
- missing start node id
- missing start node reference
- duplicate node ids
- missing node speaker/text for line nodes
- broken next node ids
- broken choice next node ids
- invalid condition targets
- invalid effect targets for effects that require a target

- [ ] **Step 3: Add guided inspectors**

Add `NpcDefinitionEditor` and `DialogueGraphDefinitionEditor` to `RpgDefinitionEditors.cs` with Pyralis guide text and validation summaries. Keep them consistent with existing RPG definition editors.

- [ ] **Step 4: Run build**

Run: `dotnet build "Game Studio Core.slnx" --no-restore`

Expected: green build or only targeted validation issues to fix.

---

### Task 6: Docs And Validation

**Files:**
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RPG_SYSTEMS_ROADMAP.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/RUNTIME_PARITY_MATRIX.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_INVENTORY.md`
- Modify: `Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/FEATURE_DEVELOPMENT_ROADMAP.md`

- [ ] **Step 1: Update docs**

Mark Phase 6 as foundation added and document:

- runtime NPC/dialogue spine
- guided authoring assets
- supported conditions/effects
- current limitations: no visual graph canvas, no localization/VO/lip-sync, no external adapters, no full hub/vendor/trainer implementation yet

- [ ] **Step 2: Run fast build**

Run: `dotnet restore "Game Studio Core.slnx"; dotnet build "Game Studio Core.slnx" --no-restore`

Expected: build succeeds.

- [ ] **Step 3: Run full project gate**

Run:

```powershell
& ".\Tools\Validation\Run-PreSceneValidation.ps1"
```

Expected:

- pre-scene validation passed
- Unity EditMode passes with increased test count
- Unity PlayMode passes with increased test count
- final build passes
- residue scan clean

---

## Self-Review

Spec coverage:

- Native-first strategy: covered by Core/Data/Editor split and no external package dependency.
- Runtime spine: covered by Tasks 1-3.
- Guided authoring: covered by Tasks 4-5.
- Platform-aware conditions/effects: covered by Task 2.
- Testing: covered by Tasks 1, 4, and 6.
- Visual editor: intentionally not implemented in Phase 6A; plan preserves data model for Phase 6B.

Placeholder scan:

- No TBD/TODO/fill-in placeholders.

Type consistency:

- Runtime types use `Dialogue...` and `Npc...` naming consistently.
- Data definitions adapt to Core via `CreateRuntime...` and interface implementation.
- Services follow existing RPG service constructor and `Try...(... out string issue)` patterns.
