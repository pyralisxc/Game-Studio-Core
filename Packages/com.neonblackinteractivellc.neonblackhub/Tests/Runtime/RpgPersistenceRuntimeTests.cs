using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Rpg.Proof;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgPersistenceRuntimeTests
    {
        [Test]
        public void RpgOwnerSaveData_CaptureAndApply_RestoresOwnerStateAcrossFreshServices()
        {
            RpgOwnerKey owner = RpgHubProofRouteFactory.CreateDefaultOwner();
            EquipmentSlotDefinition weaponSlot = CreateSlot();
            EquippableItemDefinition sword = CreateSword();
            SkillTreeDefinition tree = CreateSkillTree();
            try
            {
                InventoryService inventory = new InventoryService();
                ProgressionService progression = CreateProgressionWithSkillPoints(owner);
                QuestService quests = new QuestService(progression, inventory);
                EquipmentService equipment = new EquipmentService();
                SkillTreeService skills = new SkillTreeService(progression);
                DialogueService dialogue = new DialogueService(progression, inventory, quests, skills);
                IQuestDefinition quest = RpgHubProofRouteFactory.CreateQuest();
                IDialogueGraph graph = RpgHubProofRouteFactory.CreateDialogueGraph();
                INpcProfile npc = RpgHubProofRouteFactory.CreateNpc();

                Assert.That(inventory.TryAddItem(owner, RpgHubProofRouteIds.GoldItemId, 7, out _), Is.True);
                Assert.That(inventory.TryAddItem(owner, RpgHubProofRouteIds.HerbItemId, 2, out _), Is.True);
                Assert.That(quests.TryStartQuest(owner, quest, out _), Is.True);
                Assert.That(quests.ReportObjectiveProgress(owner, quest, RpgHubProofRouteIds.QuestObjectiveId, 2, out _, out _), Is.True);
                Assert.That(equipment.TryEquip(owner, weaponSlot, sword, out _), Is.True);
                Assert.That(skills.TryUnlock(owner, tree, RpgHubProofRouteIds.SkillRootNodeId, out _), Is.True);
                dialogue.SetDialogueFlag(owner, "flag.met-elder", true);
                Assert.That(dialogue.TryStartSession(owner, npc, graph, out _, out _), Is.True);
                Assert.That(dialogue.TrySelectChoice(owner, graph, RpgHubProofRouteIds.DialogueAcceptChoiceId, out _, out _), Is.True);

                RpgOwnerSaveData save = RpgOwnerSaveData.Capture(
                    owner,
                    progression,
                    inventory,
                    equipment,
                    quests,
                    skills,
                    dialogue,
                    new RpgHubReturnSnapshot(
                        RpgHubProofRouteIds.HubId,
                        RpgHubProofRouteIds.HubSceneId,
                        "spawn.default",
                        RpgHubProofRouteIds.PortalInteractableId,
                        RpgHubProofRouteIds.ArenaSceneId));

                InventoryService restoredInventory = new InventoryService();
                ProgressionService restoredProgression = new ProgressionService(null);
                QuestService restoredQuests = new QuestService(restoredProgression, restoredInventory);
                EquipmentService restoredEquipment = new EquipmentService();
                SkillTreeService restoredSkills = new SkillTreeService(restoredProgression);
                DialogueService restoredDialogue = new DialogueService(restoredProgression, restoredInventory, restoredQuests, restoredSkills);

                save.ApplyTo(
                    restoredProgression,
                    restoredInventory,
                    restoredEquipment,
                    restoredQuests,
                    restoredSkills,
                    restoredDialogue,
                    itemId => itemId == RpgHubProofRouteIds.SwordItemId ? sword : null);

                Assert.That(restoredInventory.GetItemCount(owner, RpgHubProofRouteIds.GoldItemId), Is.EqualTo(7));
                Assert.That(restoredInventory.GetItemCount(owner, RpgHubProofRouteIds.HerbItemId), Is.EqualTo(2));
                Assert.That(restoredProgression.GetState(owner).SkillPoints, Is.EqualTo(progression.GetState(owner).SkillPoints));
                Assert.That(restoredQuests.GetProgress(owner, RpgHubProofRouteIds.QuestId).Status, Is.EqualTo(QuestStatus.Active));
                Assert.That(restoredQuests.GetProgress(owner, RpgHubProofRouteIds.QuestId).GetObjectiveProgress(RpgHubProofRouteIds.QuestObjectiveId), Is.EqualTo(2));
                Assert.That(restoredEquipment.GetEquippedItemId(owner, RpgHubProofRouteIds.WeaponSlotId), Is.EqualTo(RpgHubProofRouteIds.SwordItemId));
                Assert.That(restoredSkills.GetUnlockCount(owner, RpgHubProofRouteIds.SkillRootNodeId), Is.EqualTo(1));
                Assert.That(restoredDialogue.HasDialogueFlag(owner, "flag.met-elder"), Is.True);
                Assert.That(restoredDialogue.GetSessionSnapshot(owner).Ended, Is.True);
                Assert.That(save.HubReturn.RequestedSceneId, Is.EqualTo(RpgHubProofRouteIds.ArenaSceneId));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tree);
                UnityEngine.Object.DestroyImmediate(sword);
                UnityEngine.Object.DestroyImmediate(weaponSlot);
            }
        }

        [Test]
        public void RpgOwnerSaveData_ApplyTo_ToleratesUnknownAndMissingData()
        {
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-unknown");
            RpgOwnerSaveData save = new RpgOwnerSaveData(
                owner,
                8,
                new RpgProgressionSnapshot(50, 3, 2),
                new[] { new RpgInventoryItemSnapshot("item.unknown", 4), new RpgInventoryItemSnapshot("", 99) },
                new[] { new RpgEquipmentSnapshot("slot.weapon", "item.missing") },
                new[] { new RpgQuestSnapshot("quest.unknown", QuestStatus.Active, false, new[] { new RpgQuestObjectiveSnapshot("objective.unknown", 1) }) },
                new[] { new RpgSkillUnlockSnapshot("skill.unknown", 1) },
                new RpgDialogueSnapshot(new[] { "flag.unknown" }, default),
                default);

            InventoryService inventory = new InventoryService();
            ProgressionService progression = new ProgressionService(null);
            QuestService quests = new QuestService(progression, inventory);
            EquipmentService equipment = new EquipmentService();
            SkillTreeService skills = new SkillTreeService(progression);
            DialogueService dialogue = new DialogueService(progression, inventory, quests, skills);

            Assert.DoesNotThrow(() => save.ApplyTo(progression, inventory, equipment, quests, skills, dialogue, _ => null));
            Assert.That(inventory.GetItemCount(owner, "item.unknown"), Is.EqualTo(4));
            Assert.That(equipment.GetEquippedItemId(owner, "slot.weapon"), Is.Empty);
            Assert.That(quests.GetProgress(owner, "quest.unknown").Status, Is.EqualTo(QuestStatus.Active));
            Assert.That(skills.IsUnlocked(owner, "skill.unknown"), Is.True);
            Assert.That(dialogue.HasDialogueFlag(owner, "flag.unknown"), Is.True);
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

        private static EquipmentSlotDefinition CreateSlot()
        {
            EquipmentSlotDefinition slot = ScriptableObject.CreateInstance<EquipmentSlotDefinition>();
            slot.slotId = RpgHubProofRouteIds.WeaponSlotId;
            slot.displayName = "Weapon";
            return slot;
        }

        private static EquippableItemDefinition CreateSword()
        {
            EquippableItemDefinition item = ScriptableObject.CreateInstance<EquippableItemDefinition>();
            item.itemId = RpgHubProofRouteIds.SwordItemId;
            item.displayName = "Proof Sword";
            item.allowedSlotIds = new[] { RpgHubProofRouteIds.WeaponSlotId };
            return item;
        }

        private static SkillTreeDefinition CreateSkillTree()
        {
            SkillTreeDefinition tree = ScriptableObject.CreateInstance<SkillTreeDefinition>();
            tree.treeId = RpgHubProofRouteIds.SkillTreeId;
            tree.displayName = "Proof Hero";
            tree.nodes = new[] { new SkillNodeDefinition(RpgHubProofRouteIds.SkillRootNodeId, "Root Training", 1) };
            return tree;
        }
    }
}
