using System;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Rpg.UI;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgQuestBoardPanelPresenterRuntimeTests
    {
        [Test]
        public void RpgQuestBoardPanelPresenter_ShowInteractionResult_ListsQuestEntries()
        {
            GameObject root = new GameObject("Quest Board");
            QuestDefinition quest = CreateQuest("quest.herbs", "Gather Herbs");
            try
            {
                RpgQuestBoardPanelPresenter presenter = root.AddComponent<RpgQuestBoardPanelPresenter>();
                RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
                presenter.ConfigureForTests(owner, new QuestService(), new IQuestDefinition[] { quest });

                Assert.That(presenter.ShowInteractionResult(Result()), Is.True, presenter.LastIssue);

                Assert.That(presenter.Entries.Length, Is.EqualTo(1));
                Assert.That(presenter.Entries[0].QuestId, Is.EqualTo("quest.herbs"));
                Assert.That(presenter.Entries[0].Title, Is.EqualTo("Gather Herbs"));
                Assert.That(presenter.Entries[0].Status, Is.EqualTo(QuestStatus.NotStarted));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(quest);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RpgQuestBoardPanelPresenter_StartSelectedQuest_UpdatesQuestStatus()
        {
            GameObject root = new GameObject("Quest Board");
            QuestDefinition quest = CreateQuest("quest.herbs", "Gather Herbs");
            try
            {
                QuestService service = new QuestService();
                RpgQuestBoardPanelPresenter presenter = root.AddComponent<RpgQuestBoardPanelPresenter>();
                RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
                presenter.ConfigureForTests(owner, service, new IQuestDefinition[] { quest });
                presenter.ShowInteractionResult(Result());

                Assert.That(presenter.StartSelectedQuest(), Is.True, presenter.LastIssue);

                Assert.That(service.GetProgress(owner, "quest.herbs").Status, Is.EqualTo(QuestStatus.Active));
                Assert.That(presenter.Entries[0].Status, Is.EqualTo(QuestStatus.Active));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(quest);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static HubInteractionResult Result()
        {
            return new HubInteractionResult(
                HubInteractionStatus.Selected,
                string.Empty,
                default,
                PlayerPanelRoute.QuestBoard,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<HubNotificationPayload>());
        }

        private static QuestDefinition CreateQuest(string questId, string displayName)
        {
            QuestDefinition quest = ScriptableObject.CreateInstance<QuestDefinition>();
            quest.questId = questId;
            quest.displayName = displayName;
            quest.objectives = new[] { new QuestObjectiveDefinition("objective.collect", QuestObjectiveKind.CollectItem, "item.herb", 3) };
            quest.rewards = new[] { new QuestRewardDefinition { experience = 5 } };
            return quest;
        }
    }
}
