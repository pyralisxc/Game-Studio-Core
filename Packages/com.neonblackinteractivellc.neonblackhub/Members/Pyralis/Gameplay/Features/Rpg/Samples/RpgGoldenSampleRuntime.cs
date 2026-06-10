using NeonBlack.Gameplay.Core.Rpg;
using VContainer;

namespace NeonBlack.Gameplay.Features.Rpg.Samples
{
    public sealed class RpgGoldenSampleRuntime
    {
        [Inject]
        public RpgGoldenSampleRuntime(
            RpgOwnerKey owner,
            HubDefinitionModel hub,
            INpcProfile guide,
            IDialogueGraph dialogueGraph,
            IQuestDefinition quest,
            IVendorDefinition vendorDefinition,
            IEquipmentSlot capeSlot,
            IEquippableItem cape,
            ISkillTree skillTree,
            RpgZoneDefinition meadow,
            InventoryService inventory,
            ProgressionService progression,
            QuestService quests,
            EquipmentService equipment,
            SkillTreeService skills,
            DialogueService dialogue,
            VendorService vendor,
            RpgOpenZoneService zoneState,
            HubInteractionService hubInteractions)
        {
            Owner = owner;
            Hub = hub;
            Guide = guide;
            DialogueGraph = dialogueGraph;
            Quest = quest;
            VendorDefinition = vendorDefinition;
            CapeSlot = capeSlot;
            Cape = cape;
            SkillTree = skillTree;
            Meadow = meadow;
            Inventory = inventory;
            Progression = progression;
            Quests = quests;
            Equipment = equipment;
            Skills = skills;
            Dialogue = dialogue;
            Vendor = vendor;
            ZoneState = zoneState;
            HubInteractions = hubInteractions;
        }

        public RpgOwnerKey Owner { get; }
        public HubDefinitionModel Hub { get; }
        public INpcProfile Guide { get; }
        public IDialogueGraph DialogueGraph { get; }
        public IQuestDefinition Quest { get; }
        public IVendorDefinition VendorDefinition { get; }
        public IEquipmentSlot CapeSlot { get; }
        public IEquippableItem Cape { get; }
        public ISkillTree SkillTree { get; }
        public RpgZoneDefinition Meadow { get; }
        public InventoryService Inventory { get; }
        public ProgressionService Progression { get; }
        public QuestService Quests { get; }
        public EquipmentService Equipment { get; }
        public SkillTreeService Skills { get; }
        public DialogueService Dialogue { get; }
        public VendorService Vendor { get; }
        public RpgOpenZoneService ZoneState { get; }
        public HubInteractionService HubInteractions { get; }

        public HubInteractionResult SelectHubInteraction(string interactableId)
        {
            return HubInteractions.SelectInteraction(Owner, Hub, interactableId);
        }

        public bool StartGuideDialogue(out string issue)
        {
            SelectHubInteraction(RpgGoldenSampleIds.DialogueInteractableId);
            return Dialogue.TryStartSession(Owner, Guide, DialogueGraph, out _, out issue);
        }

        public bool AcceptGuideQuest(out string issue)
        {
            return Dialogue.TrySelectChoice(Owner, DialogueGraph, RpgGoldenSampleIds.GuideAcceptChoiceId, out _, out issue);
        }

        public bool BuyPotion(out string issue)
        {
            return Vendor.TryBuy(Owner, VendorDefinition, RpgGoldenSampleIds.PotionOfferId, 1, out _, out issue);
        }

        public bool EnterMeadow(out string issue)
        {
            HubInteractionResult result = SelectHubInteraction(RpgGoldenSampleIds.PortalInteractableId);
            if (result.Status == HubInteractionStatus.Invalid)
            {
                issue = result.Issue;
                return false;
            }

            return ZoneState.EnterZone(Owner, RpgGoldenSampleIds.MeadowZoneId, RpgGoldenSampleIds.MeadowEntranceId, RpgGoldenSampleIds.HubId, out issue);
        }

        public bool CollectHerbsAndCompleteQuest(out string issue)
        {
            ZoneState.SetPickupState(Owner, RpgGoldenSampleIds.MeadowZoneId, RpgGoldenSampleIds.HerbPickupId, RpgZoneEntityStatus.Collected);
            ZoneState.SetEncounterState(Owner, RpgGoldenSampleIds.MeadowZoneId, RpgGoldenSampleIds.BanditEncounterId, RpgZoneEntityStatus.Cleared);
            ZoneState.SetResourceState(Owner, RpgGoldenSampleIds.MeadowZoneId, RpgGoldenSampleIds.OreResourceId, 1, false);
            ZoneState.SetNpcState(Owner, RpgGoldenSampleIds.MeadowZoneId, RpgGoldenSampleIds.GuideNpcId, "spawn.guide", true, "after-quest");

            if (!Inventory.TryAddItem(Owner, RpgGoldenSampleIds.HerbItemId, 3, out issue))
                return false;

            return Quests.ReportObjectiveProgress(Owner, Quest, RpgGoldenSampleIds.QuestObjectiveId, 3, out _, out issue);
        }

        public bool EquipCape(out string issue)
        {
            return Equipment.TryEquip(Owner, CapeSlot, Cape, out issue);
        }

        public bool UnlockWisdomSkill(out string issue)
        {
            return Skills.TryUnlock(Owner, SkillTree, RpgGoldenSampleIds.SkillRootNodeId, out issue);
        }

        public RpgOwnerSaveData CaptureSave(HubInteractionResult lastHubResult)
        {
            return RpgOwnerSaveData.Capture(
                Owner,
                Progression,
                Inventory,
                Equipment,
                Quests,
                Skills,
                Dialogue,
                new RpgHubReturnSnapshot(
                    RpgGoldenSampleIds.HubId,
                    RpgGoldenSampleIds.HubSceneId,
                    "spawn.hub",
                    lastHubResult.Prompt.InteractableId,
                    lastHubResult.SceneId),
                ZoneState);
        }

        public void ApplySave(RpgOwnerSaveData save)
        {
            save.ApplyTo(
                Progression,
                Inventory,
                Equipment,
                Quests,
                Skills,
                Dialogue,
                ResolveEquippable,
                ZoneState);
        }

        private IEquippableItem ResolveEquippable(string itemId)
        {
            return itemId == RpgGoldenSampleIds.CapeItemId ? Cape : null;
        }
    }
}
