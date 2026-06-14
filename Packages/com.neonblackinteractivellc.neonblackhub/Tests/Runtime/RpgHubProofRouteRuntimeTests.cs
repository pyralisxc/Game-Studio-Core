using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Rpg.UI;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgHubProofRouteRuntimeTests
    {
        [Test]
        public void RpgHubProofRouteTestFixture_CreateHub_DrivesPromptPanelAndSceneLoop()
        {
            GameObject root = new GameObject("RPG Hub Proof Route");
            SkillTreeDefinition skillTree = CreateSkillTree();
            EquipmentSlotDefinition weaponSlot = CreateSlot();
            EquippableItemDefinition sword = CreateSword();
            try
            {
                RpgOwnerKey owner = RpgHubProofRouteTestFixture.CreateDefaultOwner();
                InventoryService inventory = new InventoryService();
                Assert.That(inventory.TryAddItem(owner, RpgHubProofRouteTestFixture.Ids.GoldItemId, 5, out string goldIssue), Is.True, goldIssue);

                ProgressionService progression = CreateProgressionWithSkillPoints(owner);
                QuestService quests = new QuestService(progression, inventory);
                SkillTreeService skills = new SkillTreeService(progression);
                DialogueService dialogue = new DialogueService(progression, inventory, quests, skills);
                EquipmentService equipment = new EquipmentService();
                VendorService vendorService = new VendorService(inventory);

                IQuestDefinition quest = CreateQuest();
                HubInteractionService interactionService = new HubInteractionService(inventory, quests, skills, dialogue);
                Assert.That(interactionService.RegisterQuest(quest), Is.True);
                Assert.That(interactionService.RegisterSkillTree(skillTree.treeId, skillTree), Is.True);

                HubInteractionHudPresenter hud = root.AddComponent<HubInteractionHudPresenter>();
                HubInteractionSceneController controller = root.AddComponent<HubInteractionSceneController>();
                RpgHubPanelRouter router = root.AddComponent<RpgHubPanelRouter>();

                RpgDialoguePanelPresenter dialoguePanel = CreatePanel<RpgDialoguePanelPresenter>(root.transform, PlayerPanelRoute.Dialogue);
                RpgQuestBoardPanelPresenter questPanel = CreatePanel<RpgQuestBoardPanelPresenter>(root.transform, PlayerPanelRoute.QuestBoard);
                RpgVendorPanelPresenter vendorPanel = CreatePanel<RpgVendorPanelPresenter>(root.transform, PlayerPanelRoute.Vendor);
                RpgLoadoutPanelPresenter loadoutPanel = CreatePanel<RpgLoadoutPanelPresenter>(root.transform, PlayerPanelRoute.Loadout);
                RpgSkillTreePanelPresenter trainerPanel = CreatePanel<RpgSkillTreePanelPresenter>(root.transform, PlayerPanelRoute.Trainer);

                dialoguePanel.ConfigureForTests(owner, dialogue, new IDialogueGraph[] { RpgHubProofRouteTestFixture.CreateDialogueGraph() }, new INpcProfile[] { RpgHubProofRouteTestFixture.CreateNpc() });
                questPanel.ConfigureForTests(owner, quests, new[] { quest });
                vendorPanel.ConfigureForTests(owner, vendorService, new IVendorDefinition[] { RpgHubProofRouteTestFixture.CreateVendor() });
                loadoutPanel.ConfigureForTests(owner, equipment, new IEquipmentSlot[] { weaponSlot }, new IEquippableItem[] { sword });
                trainerPanel.ConfigureForTests(owner, progression, skills, new[] { skillTree });
                router.ConfigureForTests(hud, root.GetComponentsInChildren<RpgPanelRoutePresenter>(true));
                controller.ConfigureForTests(RpgHubProofRouteTestFixture.CreateHub(), hud, owner, interactionService);

                HubInteractionResult[] prompts = controller.RefreshAvailableInteractions();

                Assert.That(prompts.Count(result => result.Status == HubInteractionStatus.Available), Is.EqualTo(6));

                controller.ConfirmInteractable(RpgHubProofRouteTestFixture.Ids.DialogueInteractableId);
                Assert.That(dialoguePanel.CurrentNode.NodeId, Is.EqualTo(RpgHubProofRouteTestFixture.Ids.DialogueStartNodeId));
                Assert.That(dialoguePanel.SelectChoice(RpgHubProofRouteTestFixture.Ids.DialogueAcceptChoiceId), Is.True, dialoguePanel.LastIssue);

                controller.ConfirmInteractable(RpgHubProofRouteTestFixture.Ids.QuestBoardInteractableId);
                Assert.That(questPanel.StartSelectedQuest(), Is.True, questPanel.LastIssue);
                Assert.That(quests.GetProgress(owner, RpgHubProofRouteTestFixture.Ids.QuestId).Status, Is.EqualTo(QuestStatus.Active));

                controller.ConfirmInteractable(RpgHubProofRouteTestFixture.Ids.VendorInteractableId);
                Assert.That(vendorPanel.BuySelectedOffer(), Is.True, vendorPanel.LastIssue);
                Assert.That(inventory.GetItemCount(owner, RpgHubProofRouteTestFixture.Ids.PotionItemId), Is.EqualTo(1));

                controller.ConfirmInteractable(RpgHubProofRouteTestFixture.Ids.LoadoutInteractableId);
                Assert.That(loadoutPanel.EquipSelectedItem(), Is.True, loadoutPanel.LastIssue);
                Assert.That(equipment.GetEquippedItemId(owner, RpgHubProofRouteTestFixture.Ids.WeaponSlotId), Is.EqualTo(RpgHubProofRouteTestFixture.Ids.SwordItemId));

                controller.ConfirmInteractable(RpgHubProofRouteTestFixture.Ids.TrainerInteractableId);
                Assert.That(trainerPanel.UnlockSelectedNode(), Is.True, trainerPanel.LastIssue);
                Assert.That(skills.IsUnlocked(owner, RpgHubProofRouteTestFixture.Ids.SkillRootNodeId), Is.True);

                controller.ConfirmInteractable(RpgHubProofRouteTestFixture.Ids.PortalInteractableId);
                Assert.That(hud.PresentationState.RequestedSceneId, Is.EqualTo(RpgHubProofRouteTestFixture.Ids.ArenaSceneId));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sword);
                UnityEngine.Object.DestroyImmediate(weaponSlot);
                UnityEngine.Object.DestroyImmediate(skillTree);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static T CreatePanel<T>(Transform parent, PlayerPanelRoute route)
            where T : Component
        {
            GameObject panelObject = new GameObject(route + " Panel");
            panelObject.transform.SetParent(parent);
            panelObject.SetActive(false);
            RpgPanelRoutePresenter routePresenter = panelObject.AddComponent<RpgPanelRoutePresenter>();
            routePresenter.ConfigureForTests(route, panelObject);
            return panelObject.AddComponent<T>();
        }

        private static ProgressionService CreateProgressionWithSkillPoints(RpgOwnerKey owner)
        {
            ProgressionCurveDefinition curve = ScriptableObject.CreateInstance<ProgressionCurveDefinition>();
            curve.SetTestThresholds(new[] { 0, 10, 20, 30 });
            curve.SetTestSkillPointGrants(new[] { 0, 1, 1, 1 });
            ProgressionService progression = new ProgressionService(curve);
            progression.AddExperience(owner, 30);
            UnityEngine.Object.DestroyImmediate(curve);
            return progression;
        }

        private static IQuestDefinition CreateQuest()
        {
            return RpgHubProofRouteTestFixture.CreateQuest();
        }

        private static EquipmentSlotDefinition CreateSlot()
        {
            EquipmentSlotDefinition slot = ScriptableObject.CreateInstance<EquipmentSlotDefinition>();
            slot.slotId = RpgHubProofRouteTestFixture.Ids.WeaponSlotId;
            slot.displayName = "Weapon";
            return slot;
        }

        private static EquippableItemDefinition CreateSword()
        {
            EquippableItemDefinition item = ScriptableObject.CreateInstance<EquippableItemDefinition>();
            item.itemId = RpgHubProofRouteTestFixture.Ids.SwordItemId;
            item.displayName = "Proof Sword";
            item.allowedSlotIds = new[] { RpgHubProofRouteTestFixture.Ids.WeaponSlotId };
            return item;
        }

        private static SkillTreeDefinition CreateSkillTree()
        {
            SkillTreeDefinition tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            tree.treeId = RpgHubProofRouteTestFixture.Ids.SkillTreeId;
            tree.displayName = "Proof Hero";
            tree.nodes = new[] { new SkillNodeDefinition(RpgHubProofRouteTestFixture.Ids.SkillRootNodeId, "Root Training", 1) };
            return tree;
        }
    }
}
