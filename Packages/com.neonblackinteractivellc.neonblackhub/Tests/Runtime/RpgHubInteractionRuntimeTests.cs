using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgHubInteractionRuntimeTests
    {
        [Test]
        public void HubInteractionService_GetAvailableInteractions_ReturnsUnlockedPromptPayload()
        {
            HubInteractionService service = new HubInteractionService();
            HubDefinitionModel hub = CreateHub(new HubInteractable(
                "interactable.portal",
                "Training Gate",
                "Enter training",
                "Need a pass",
                "icon.portal",
                HubInteractionKind.Portal,
                HubInteractionAvailability.Available,
                PlayerPanelRoute.Portal,
                "scene.training",
                string.Empty,
                string.Empty,
                Array.Empty<HubInteractionCondition>(),
                Array.Empty<HubInteractionEffect>(),
                10,
                "Training unlocked."));
            RpgOwnerKey owner = Owner();

            HubInteractionResult[] results = service.GetAvailableInteractions(owner, hub);

            Assert.That(results.Length, Is.EqualTo(1));
            Assert.That(results[0].Status, Is.EqualTo(HubInteractionStatus.Available));
            Assert.That(results[0].Prompt.Text, Is.EqualTo("Enter training"));
            Assert.That(results[0].Prompt.IconId, Is.EqualTo("icon.portal"));
            Assert.That(results[0].Prompt.Locked, Is.False);
            Assert.That(results[0].PanelRoute, Is.EqualTo(PlayerPanelRoute.Portal));
        }

        [Test]
        public void HubInteractionService_GetAvailableInteractions_CanShowLockedPrompt()
        {
            HubInteractionService service = new HubInteractionService();
            HubDefinitionModel hub = CreateHub(new HubInteractable(
                "interactable.trainer",
                "Skill Trainer",
                "Train",
                "Bring a training token",
                "icon.trainer",
                HubInteractionKind.Trainer,
                HubInteractionAvailability.LockedVisible,
                PlayerPanelRoute.Trainer,
                string.Empty,
                string.Empty,
                string.Empty,
                new[] { new HubInteractionCondition(HubConditionKind.ItemCount, "item.training-token", string.Empty, 1, true) },
                Array.Empty<HubInteractionEffect>(),
                0,
                string.Empty));
            RpgOwnerKey owner = Owner();

            HubInteractionResult[] results = service.GetAvailableInteractions(owner, hub);

            Assert.That(results.Length, Is.EqualTo(1));
            Assert.That(results[0].Status, Is.EqualTo(HubInteractionStatus.Locked));
            Assert.That(results[0].Prompt.Text, Is.EqualTo("Bring a training token"));
            Assert.That(results[0].Prompt.Locked, Is.True);
        }

        [Test]
        public void HubInteractionService_GetAvailableInteractions_HidesLockedSecretInteraction()
        {
            HubInteractionService service = new HubInteractionService();
            HubDefinitionModel hub = CreateHub(new HubInteractable(
                "interactable.secret",
                "Secret Door",
                "Open",
                "Locked",
                "icon.secret",
                HubInteractionKind.Portal,
                HubInteractionAvailability.HiddenUntilAvailable,
                PlayerPanelRoute.Portal,
                "scene.secret",
                string.Empty,
                string.Empty,
                new[] { new HubInteractionCondition(HubConditionKind.DialogueFlag, "flag.secret-known", string.Empty, 1, true) },
                Array.Empty<HubInteractionEffect>(),
                0,
                string.Empty));

            Assert.That(service.GetAvailableInteractions(Owner(), hub), Is.Empty);
        }

        [Test]
        public void HubInteractionService_SelectInteraction_ReturnsDialoguePanelAndSceneRequests()
        {
            InventoryService inventory = new InventoryService();
            HubInteractionService service = new HubInteractionService(inventory: inventory);
            RpgOwnerKey owner = Owner();
            Assert.That(inventory.TryAddItem(owner, "item.training-pass", 1, out string itemIssue), Is.True, itemIssue);
            HubDefinitionModel hub = CreateHub(new HubInteractable(
                "interactable.elder",
                "Village Elder",
                "Talk",
                "Busy",
                "icon.npc",
                HubInteractionKind.NPCDialogue,
                HubInteractionAvailability.Available,
                PlayerPanelRoute.Dialogue,
                "scene.village",
                "dialogue.elder",
                "npc.elder",
                new[] { new HubInteractionCondition(HubConditionKind.ItemCount, "item.training-pass", string.Empty, 1, true) },
                new[]
                {
                    new HubInteractionEffect(HubEffectKind.OpenPanel, string.Empty, string.Empty, 1, true, PlayerPanelRoute.Dialogue),
                    new HubInteractionEffect(HubEffectKind.NavigateScene, "scene.village", string.Empty, 1, true, PlayerPanelRoute.None)
                },
                0,
                "The elder is waiting."));

            HubInteractionResult result = service.SelectInteraction(owner, hub, "interactable.elder");

            Assert.That(result.Status, Is.EqualTo(HubInteractionStatus.Selected));
            Assert.That(result.DialogueGraphId, Is.EqualTo("dialogue.elder"));
            Assert.That(result.NpcId, Is.EqualTo("npc.elder"));
            Assert.That(result.PanelRoute, Is.EqualTo(PlayerPanelRoute.Dialogue));
            Assert.That(result.SceneId, Is.EqualTo("scene.village"));
            Assert.That(result.Notifications.Single().Body, Is.EqualTo("The elder is waiting."));
        }

        [Test]
        public void HubInteractionService_SelectInteraction_ReturnsExplicitIssueForInvalidOwner()
        {
            HubInteractionService service = new HubInteractionService();
            HubDefinitionModel hub = CreateHub();

            HubInteractionResult result = service.SelectInteraction(new RpgOwnerKey(RpgOwnerKind.Unknown, string.Empty), hub, "interactable.missing");

            Assert.That(result.Status, Is.EqualTo(HubInteractionStatus.Invalid));
            Assert.That(result.Issue, Does.Contain("owner"));
        }

        private static RpgOwnerKey Owner()
        {
            return new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
        }

        private static HubDefinitionModel CreateHub(params HubInteractable[] interactables)
        {
            return new HubDefinitionModel(
                "hub.test",
                "Test Hub",
                "scene.hub",
                "spawn.default",
                Array.Empty<string>(),
                interactables);
        }
    }
}
