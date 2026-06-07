using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Rpg.UI;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgHubSceneControllerRuntimeTests
    {
        [Test]
        public void HubInteractionSceneController_RefreshAvailableInteractions_FeedsHudPresenter()
        {
            GameObject root = new GameObject("Hub Controller");
            try
            {
                HubInteractionHudPresenter presenter = root.AddComponent<HubInteractionHudPresenter>();
                HubInteractionSceneController controller = root.AddComponent<HubInteractionSceneController>();
                controller.ConfigureForTests(CreateHub(), presenter, Owner(), new HubInteractionService());

                HubInteractionResult[] results = controller.RefreshAvailableInteractions();

                Assert.That(results.Length, Is.EqualTo(2));
                Assert.That(presenter.PromptList.Prompts.Select(prompt => prompt.InteractableId), Is.EqualTo(new[] { "talk.elder", "portal.arena" }));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void HubInteractionSceneController_ConfirmSelectedPrompt_ShowsInteractionResult()
        {
            GameObject root = new GameObject("Hub Controller");
            try
            {
                HubInteractionHudPresenter presenter = root.AddComponent<HubInteractionHudPresenter>();
                HubInteractionSceneController controller = root.AddComponent<HubInteractionSceneController>();
                controller.ConfigureForTests(CreateHub(), presenter, Owner(), new HubInteractionService());
                controller.RefreshAvailableInteractions();

                presenter.ConfirmSelectedPrompt();

                Assert.That(presenter.PresentationState.LastStatus, Is.EqualTo(HubInteractionStatus.Selected));
                Assert.That(presenter.PresentationState.RequestedDialogueGraphId, Is.EqualTo("dialogue.elder"));
                Assert.That(controller.LastResult.DialogueGraphId, Is.EqualTo("dialogue.elder"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void HubInteractionSceneController_TryHandleInteraction_RefreshesPromptsForActorInteractionFeature()
        {
            GameObject root = new GameObject("Hub Controller");
            try
            {
                HubInteractionHudPresenter presenter = root.AddComponent<HubInteractionHudPresenter>();
                HubInteractionSceneController controller = root.AddComponent<HubInteractionSceneController>();
                controller.ConfigureForTests(CreateHub(), presenter, Owner(), new HubInteractionService());

                bool handled = controller.TryHandleInteraction(null);

                Assert.That(handled, Is.True);
                Assert.That(presenter.PromptList.HasPrompt, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static RpgOwnerKey Owner()
        {
            return new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
        }

        private static HubDefinitionModel CreateHub()
        {
            return new HubDefinitionModel(
                "hub.test",
                "Test Hub",
                "scene.hub",
                "spawn.default",
                Array.Empty<string>(),
                new[]
                {
                    new HubInteractable(
                        "portal.arena",
                        "Arena",
                        "Enter Arena",
                        "Arena locked",
                        string.Empty,
                        HubInteractionKind.Portal,
                        HubInteractionAvailability.Available,
                        PlayerPanelRoute.None,
                        "scene.arena",
                        string.Empty,
                        string.Empty,
                        Array.Empty<HubInteractionCondition>(),
                        Array.Empty<HubInteractionEffect>(),
                        20,
                        string.Empty),
                    new HubInteractable(
                        "talk.elder",
                        "Elder",
                        "Talk",
                        "The elder is busy",
                        string.Empty,
                        HubInteractionKind.NPCDialogue,
                        HubInteractionAvailability.Available,
                        PlayerPanelRoute.Dialogue,
                        string.Empty,
                        "dialogue.elder",
                        "npc.elder",
                        Array.Empty<HubInteractionCondition>(),
                        Array.Empty<HubInteractionEffect>(),
                        10,
                        string.Empty)
                });
        }
    }
}
