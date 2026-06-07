using System;
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
            NpcProfile npc = DialogueTestFactory.CreateNpc();
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

        [Test]
        public void DialogueService_Continue_AdvancesLineNodeToNextNode()
        {
            DialogueService service = new DialogueService();
            DialogueGraph graph = DialogueTestFactory.CreateGraph();
            NpcProfile npc = DialogueTestFactory.CreateNpc();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            Assert.That(service.TryStartSession(owner, npc, graph, out _, out string startIssue), Is.True, startIssue);

            Assert.That(service.TryContinue(owner, graph, out DialogueSessionState state, out string issue), Is.True, issue);

            Assert.That(state.CurrentNodeId, Is.EqualTo("node.end"));
            Assert.That(state.Ended, Is.True);
        }

        private static class DialogueTestFactory
        {
            public static NpcProfile CreateNpc()
            {
                return new NpcProfile("npc.elder", "Village Elder", "quest-giver", new[] { "hub" }, "faction.village", string.Empty);
            }

            public static DialogueGraph CreateGraph()
            {
                return new DialogueGraph(
                    "dialogue.elder",
                    "node.start",
                    new[]
                    {
                        new DialogueNode(
                            "node.start",
                            DialogueNodeKind.Line,
                            "npc.elder",
                            "Welcome.",
                            Array.Empty<DialogueChoice>(),
                            Array.Empty<DialogueEffect>(),
                            "node.end"),
                        new DialogueNode(
                            "node.end",
                            DialogueNodeKind.Terminal,
                            "npc.elder",
                            string.Empty,
                            Array.Empty<DialogueChoice>(),
                            Array.Empty<DialogueEffect>(),
                            string.Empty)
                    });
            }

            public static DialogueGraph CreateGraphWithConditions()
            {
                return new DialogueGraph(
                    "dialogue.elder",
                    "node.start",
                    new[]
                    {
                        new DialogueNode(
                            "node.start",
                            DialogueNodeKind.ChoiceHub,
                            "npc.elder",
                            "What do you need?",
                            new[]
                            {
                                new DialogueChoice(
                                    "choice.hello",
                                    "Just saying hello.",
                                    "node.end",
                                    Array.Empty<DialogueCondition>(),
                                    Array.Empty<DialogueEffect>()),
                                new DialogueChoice(
                                    "choice.herb",
                                    "I brought the herb.",
                                    "node.end",
                                    new[]
                                    {
                                        new DialogueCondition(DialogueConditionKind.ItemCount, "item.herb", string.Empty, 1, true),
                                        new DialogueCondition(DialogueConditionKind.DialogueFlag, "flag.met.elder", string.Empty, 1, true)
                                    },
                                    Array.Empty<DialogueEffect>())
                            },
                            Array.Empty<DialogueEffect>(),
                            string.Empty),
                        new DialogueNode(
                            "node.end",
                            DialogueNodeKind.Terminal,
                            "npc.elder",
                            string.Empty,
                            Array.Empty<DialogueChoice>(),
                            Array.Empty<DialogueEffect>(),
                            string.Empty)
                    });
            }

            public static DialogueGraph CreateGraphWithEffects()
            {
                return new DialogueGraph(
                    "dialogue.elder",
                    "node.start",
                    new[]
                    {
                        new DialogueNode(
                            "node.start",
                            DialogueNodeKind.ChoiceHub,
                            "npc.elder",
                            "Will you help?",
                            new[]
                            {
                                new DialogueChoice(
                                    "choice.accept",
                                    "Yes.",
                                    "node.end",
                                    Array.Empty<DialogueCondition>(),
                                    new[]
                                    {
                                        new DialogueEffect(DialogueEffectKind.SetDialogueFlag, "flag.elder.accepted", string.Empty, 1, true),
                                        new DialogueEffect(DialogueEffectKind.StartQuest, "quest.elder", string.Empty, 1, true),
                                        new DialogueEffect(DialogueEffectKind.GrantItem, "item.token", string.Empty, 1, true),
                                        new DialogueEffect(DialogueEffectKind.GrantExperience, string.Empty, string.Empty, 5, true)
                                    })
                            },
                            Array.Empty<DialogueEffect>(),
                            string.Empty),
                        new DialogueNode(
                            "node.end",
                            DialogueNodeKind.Terminal,
                            "npc.elder",
                            string.Empty,
                            Array.Empty<DialogueChoice>(),
                            Array.Empty<DialogueEffect>(),
                            string.Empty)
                    });
            }
        }

        private sealed class TestQuest : IQuestDefinition
        {
            public string QuestId => "quest.elder";
            public bool Repeatable => false;
            public QuestObjective[] Objectives => new[] { new QuestObjective("objective.accept", QuestObjectiveKind.ProjectEvent, "event.accept", 1) };
            public QuestReward[] Rewards => Array.Empty<QuestReward>();

            public bool TryGetObjective(string objectiveId, out QuestObjective objective)
            {
                if (objectiveId == "objective.accept")
                {
                    objective = Objectives[0];
                    return true;
                }

                objective = default;
                return false;
            }
        }
    }
}
