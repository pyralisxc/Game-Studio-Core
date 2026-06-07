using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Features.Rpg.Samples;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgGoldenSampleRuntimeTests
    {
        [Test]
        public void GoldenRpgSampleRuntime_DrivesCompleteInspectableRouteAndRestoresSaveState()
        {
            RpgGoldenSampleRuntime sample = RpgGoldenSampleFactory.CreateRuntime();

            Assert.That(sample.Hub.Interactables.Select(interactable => interactable.InteractableId), Does.Contain(RpgGoldenSampleIds.DialogueInteractableId));
            Assert.That(sample.Hub.Interactables.Select(interactable => interactable.InteractableId), Does.Contain(RpgGoldenSampleIds.PortalInteractableId));

            Assert.That(sample.StartGuideDialogue(out string issue), Is.True, issue);
            Assert.That(sample.AcceptGuideQuest(out issue), Is.True, issue);
            Assert.That(sample.Quests.GetProgress(sample.Owner, RpgGoldenSampleIds.QuestId).Status, Is.EqualTo(QuestStatus.Active));

            Assert.That(sample.BuyPotion(out issue), Is.True, issue);
            Assert.That(sample.Inventory.GetItemCount(sample.Owner, RpgGoldenSampleIds.PotionItemId), Is.EqualTo(1));

            Assert.That(sample.EnterMeadow(out issue), Is.True, issue);
            Assert.That(sample.CollectHerbsAndCompleteQuest(out issue), Is.True, issue);
            Assert.That(sample.Quests.GetProgress(sample.Owner, RpgGoldenSampleIds.QuestId).Status, Is.EqualTo(QuestStatus.Completed));
            Assert.That(sample.Inventory.GetItemCount(sample.Owner, RpgGoldenSampleIds.CapeItemId), Is.EqualTo(1));
            Assert.That(sample.ZoneState.GetZoneState(sample.Owner, RpgGoldenSampleIds.MeadowZoneId).GetPickupStatus(RpgGoldenSampleIds.HerbPickupId), Is.EqualTo(RpgZoneEntityStatus.Collected));

            Assert.That(sample.EquipCape(out issue), Is.True, issue);
            Assert.That(sample.Equipment.GetEquippedItemId(sample.Owner, RpgGoldenSampleIds.CapeSlotId), Is.EqualTo(RpgGoldenSampleIds.CapeItemId));

            Assert.That(sample.UnlockWisdomSkill(out issue), Is.True, issue);
            Assert.That(sample.Skills.IsUnlocked(sample.Owner, RpgGoldenSampleIds.SkillRootNodeId), Is.True);

            HubInteractionResult portalResult = sample.SelectHubInteraction(RpgGoldenSampleIds.PortalInteractableId);
            Assert.That(portalResult.SceneId, Is.EqualTo(RpgGoldenSampleIds.MeadowSceneId));

            RpgOwnerSaveData save = sample.CaptureSave(portalResult);
            RpgGoldenSampleRuntime restored = RpgGoldenSampleFactory.CreateRuntime();
            restored.ApplySave(save);

            Assert.That(restored.Inventory.GetItemCount(restored.Owner, RpgGoldenSampleIds.PotionItemId), Is.EqualTo(1));
            Assert.That(restored.Inventory.GetItemCount(restored.Owner, RpgGoldenSampleIds.CapeItemId), Is.EqualTo(1));
            Assert.That(restored.Quests.GetProgress(restored.Owner, RpgGoldenSampleIds.QuestId).Status, Is.EqualTo(QuestStatus.Completed));
            Assert.That(restored.Equipment.GetEquippedItemId(restored.Owner, RpgGoldenSampleIds.CapeSlotId), Is.EqualTo(RpgGoldenSampleIds.CapeItemId));
            Assert.That(restored.Skills.IsUnlocked(restored.Owner, RpgGoldenSampleIds.SkillRootNodeId), Is.True);
            Assert.That(restored.Dialogue.HasDialogueFlag(restored.Owner, RpgGoldenSampleIds.GuideAcceptedFlagId), Is.True);
            Assert.That(restored.ZoneState.GetTravelSnapshot(restored.Owner).CurrentZoneId, Is.EqualTo(RpgGoldenSampleIds.MeadowZoneId));
            Assert.That(restored.ZoneState.GetZoneState(restored.Owner, RpgGoldenSampleIds.MeadowZoneId).GetEncounterStatus(RpgGoldenSampleIds.BanditEncounterId), Is.EqualTo(RpgZoneEntityStatus.Cleared));
            Assert.That(save.HubReturn.RequestedSceneId, Is.EqualTo(RpgGoldenSampleIds.MeadowSceneId));
        }
    }
}
