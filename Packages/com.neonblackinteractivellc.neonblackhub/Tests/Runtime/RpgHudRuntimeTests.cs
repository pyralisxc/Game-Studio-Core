using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Rpg.UI;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgHudRuntimeTests
    {
        [Test]
        public void HubHudPromptList_ApplyPrompts_SortsUnlockedBeforeLockedThenByPriority()
        {
            HubHudPromptList list = new HubHudPromptList();

            list.ApplyPrompts(new[]
            {
                Prompt("locked", "Locked", locked: true, priority: 0),
                Prompt("low", "Low", locked: false, priority: 10),
                Prompt("high", "High", locked: false, priority: 1)
            });

            Assert.That(list.Prompts.Select(prompt => prompt.InteractableId), Is.EqualTo(new[] { "high", "low", "locked" }));
            Assert.That(list.SelectedPrompt.InteractableId, Is.EqualTo("high"));
        }

        [Test]
        public void HubHudPromptList_SelectNextAndPrevious_WrapsAvailablePrompts()
        {
            HubHudPromptList list = new HubHudPromptList();
            list.ApplyPrompts(new[] { Prompt("a", "A"), Prompt("b", "B"), Prompt("c", "C") });

            list.SelectPrevious();
            Assert.That(list.SelectedPrompt.InteractableId, Is.EqualTo("c"));

            list.SelectNext();
            Assert.That(list.SelectedPrompt.InteractableId, Is.EqualTo("a"));
        }

        [Test]
        public void HubHudPromptList_SelectPrompt_IgnoresUnknownIds()
        {
            HubHudPromptList list = new HubHudPromptList();
            list.ApplyPrompts(new[] { Prompt("a", "A"), Prompt("b", "B") });

            bool selected = list.SelectPrompt("missing");

            Assert.That(selected, Is.False);
            Assert.That(list.SelectedPrompt.InteractableId, Is.EqualTo("a"));
        }

        [Test]
        public void HubHudPresentationState_ApplyResult_CapturesPanelDialogueSceneAndNotifications()
        {
            HubHudPresentationState state = new HubHudPresentationState();
            HubInteractionResult result = new HubInteractionResult(
                HubInteractionStatus.Selected,
                string.Empty,
                Prompt("talk", "Talk"),
                PlayerPanelRoute.Dialogue,
                "town-square",
                "elder_intro",
                "elder",
                new[] { new HubNotificationPayload("Quest", "Quest updated", string.Empty, "info", 2.5f) });

            state.ApplyResult(result);

            Assert.That(state.LastStatus, Is.EqualTo(HubInteractionStatus.Selected));
            Assert.That(state.ActivePanelRoute, Is.EqualTo(PlayerPanelRoute.Dialogue));
            Assert.That(state.RequestedSceneId, Is.EqualTo("town-square"));
            Assert.That(state.RequestedDialogueGraphId, Is.EqualTo("elder_intro"));
            Assert.That(state.RequestedNpcId, Is.EqualTo("elder"));
            Assert.That(state.Notifications.Select(notification => notification.Body), Is.EqualTo(new[] { "Quest updated" }));
        }

        [Test]
        public void HubInteractionHudPresenter_ReportsMissingPromptSurface()
        {
            GameObject root = new GameObject("HUD");
            try
            {
                HubInteractionHudPresenter presenter = root.AddComponent<HubInteractionHudPresenter>();

                string[] issues = ((IRuntimeValidationProvider)presenter).GetRuntimeValidationIssues().ToArray();

                Assert.That(issues.Any(issue => issue.Contains("prompt label")), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static HubPromptPayload Prompt(string id, string text, bool locked = false, int priority = 0)
        {
            return new HubPromptPayload(new RpgOwnerKey(RpgOwnerKind.Participant, "player"), "hub", id, text, string.Empty, locked, priority);
        }
    }
}
