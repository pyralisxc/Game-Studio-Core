using System;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Rpg.UI;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgDialoguePanelPresenterRuntimeTests
    {
        [Test]
        public void RpgDialoguePanelPresenter_ShowInteractionResult_StartsDialogueAndListsChoices()
        {
            GameObject root = new GameObject("Dialogue Panel");
            try
            {
                RpgDialoguePanelPresenter presenter = root.AddComponent<RpgDialoguePanelPresenter>();
                DialogueGraph graph = CreateChoiceGraph();
                NpcProfile npc = CreateNpc();
                RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
                presenter.ConfigureForTests(owner, new DialogueService(), new IDialogueGraph[] { graph }, new INpcProfile[] { npc });

                Assert.That(presenter.ShowInteractionResult(Result("dialogue.elder", "npc.elder")), Is.True);

                Assert.That(presenter.CurrentNode.NodeId, Is.EqualTo("node.start"));
                Assert.That(presenter.CurrentNode.LineText, Is.EqualTo("Will you help?"));
                Assert.That(presenter.AvailableChoices.Length, Is.EqualTo(1));
                Assert.That(presenter.AvailableChoices[0].ChoiceId, Is.EqualTo("choice.accept"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RpgDialoguePanelPresenter_SelectChoice_AdvancesCurrentNode()
        {
            GameObject root = new GameObject("Dialogue Panel");
            try
            {
                RpgDialoguePanelPresenter presenter = root.AddComponent<RpgDialoguePanelPresenter>();
                DialogueGraph graph = CreateChoiceGraph();
                NpcProfile npc = CreateNpc();
                RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
                presenter.ConfigureForTests(owner, new DialogueService(), new IDialogueGraph[] { graph }, new INpcProfile[] { npc });
                presenter.ShowInteractionResult(Result("dialogue.elder", "npc.elder"));

                Assert.That(presenter.SelectChoice("choice.accept"), Is.True, presenter.LastIssue);

                Assert.That(presenter.CurrentState.CurrentNodeId, Is.EqualTo("node.end"));
                Assert.That(presenter.CurrentState.Ended, Is.True);
                Assert.That(presenter.AvailableChoices.Length, Is.EqualTo(0));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static HubInteractionResult Result(string graphId, string npcId)
        {
            return new HubInteractionResult(
                HubInteractionStatus.Selected,
                string.Empty,
                default,
                PlayerPanelRoute.Dialogue,
                string.Empty,
                graphId,
                npcId,
                Array.Empty<HubNotificationPayload>());
        }

        private static NpcProfile CreateNpc()
        {
            return new NpcProfile("npc.elder", "Village Elder", "quest-giver", Array.Empty<string>(), "faction.village", string.Empty);
        }

        private static DialogueGraph CreateChoiceGraph()
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
    }
}
