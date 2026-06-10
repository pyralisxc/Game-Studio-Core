using System;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Rpg
{
    [Serializable]
    public sealed class RpgOwnerSaveData
    {
        public const int CurrentSchemaVersion = 1;

        [SerializeField] private RpgOwnerKey owner;
        [SerializeField] private int schemaVersion;
        [SerializeField] private RpgProgressionSnapshot progression;
        [SerializeField] private RpgInventoryItemSnapshot[] inventory;
        [SerializeField] private RpgEquipmentSnapshot[] equipment;
        [SerializeField] private RpgQuestSnapshot[] quests;
        [SerializeField] private RpgSkillUnlockSnapshot[] skillUnlocks;
        [SerializeField] private RpgDialogueSnapshot dialogue;
        [SerializeField] private RpgHubReturnSnapshot hubReturn;
        [SerializeField] private RpgOpenZoneSnapshot openZones;

        public RpgOwnerSaveData(
            RpgOwnerKey owner,
            int schemaVersion,
            RpgProgressionSnapshot progression,
            RpgInventoryItemSnapshot[] inventory,
            RpgEquipmentSnapshot[] equipment,
            RpgQuestSnapshot[] quests,
            RpgSkillUnlockSnapshot[] skillUnlocks,
            RpgDialogueSnapshot dialogue,
            RpgHubReturnSnapshot hubReturn,
            RpgOpenZoneSnapshot openZones = default)
        {
            this.owner = owner;
            this.schemaVersion = schemaVersion < 1 ? CurrentSchemaVersion : schemaVersion;
            this.progression = progression;
            this.inventory = inventory ?? Array.Empty<RpgInventoryItemSnapshot>();
            this.equipment = equipment ?? Array.Empty<RpgEquipmentSnapshot>();
            this.quests = quests ?? Array.Empty<RpgQuestSnapshot>();
            this.skillUnlocks = skillUnlocks ?? Array.Empty<RpgSkillUnlockSnapshot>();
            this.dialogue = dialogue;
            this.hubReturn = hubReturn;
            this.openZones = openZones;
        }

        public RpgOwnerKey Owner => owner;
        public int SchemaVersion => schemaVersion;
        public RpgProgressionSnapshot Progression => progression;
        public RpgInventoryItemSnapshot[] Inventory => inventory;
        public RpgEquipmentSnapshot[] Equipment => equipment;
        public RpgQuestSnapshot[] Quests => quests;
        public RpgSkillUnlockSnapshot[] SkillUnlocks => skillUnlocks;
        public RpgDialogueSnapshot Dialogue => dialogue;
        public RpgHubReturnSnapshot HubReturn => hubReturn;
        public RpgOpenZoneSnapshot OpenZones => openZones;

        public static RpgOwnerSaveData Capture(
            RpgOwnerKey owner,
            ProgressionService progression,
            InventoryService inventory,
            EquipmentService equipment,
            QuestService quests,
            SkillTreeService skills,
            DialogueService dialogue,
            RpgHubReturnSnapshot hubReturn,
            RpgOpenZoneService openZones = null)
        {
            return new RpgOwnerSaveData(
                owner,
                CurrentSchemaVersion,
                progression != null ? progression.GetSnapshot(owner) : default,
                inventory != null ? inventory.GetSnapshot(owner) : Array.Empty<RpgInventoryItemSnapshot>(),
                equipment != null ? equipment.GetSnapshot(owner) : Array.Empty<RpgEquipmentSnapshot>(),
                quests != null ? quests.GetSnapshot(owner) : Array.Empty<RpgQuestSnapshot>(),
                skills != null ? skills.GetSnapshot(owner) : Array.Empty<RpgSkillUnlockSnapshot>(),
                dialogue != null ? dialogue.GetSnapshot(owner) : default,
                hubReturn,
                openZones != null ? openZones.GetSnapshot(owner) : default);
        }

        public void ApplyTo(
            ProgressionService progression,
            InventoryService inventory,
            EquipmentService equipment,
            QuestService quests,
            SkillTreeService skills,
            DialogueService dialogue,
            Func<string, IEquippableItem> equippableResolver = null,
            RpgOpenZoneService openZones = null)
        {
            if (!Owner.IsValid)
                return;

            progression?.RestoreSnapshot(Owner, Progression);
            inventory?.RestoreSnapshot(Owner, Inventory);
            quests?.RestoreSnapshot(Owner, Quests);
            skills?.RestoreSnapshot(Owner, SkillUnlocks);
            dialogue?.RestoreSnapshot(Owner, Dialogue);
            equipment?.RestoreSnapshot(Owner, Equipment, equippableResolver);
            openZones?.RestoreSnapshot(Owner, OpenZones);
        }
    }

    [Serializable]
    public struct RpgProgressionSnapshot
    {
        [SerializeField] private int experience;
        [SerializeField] private int level;
        [SerializeField] private int skillPoints;

        public RpgProgressionSnapshot(int experience, int level, int skillPoints)
        {
            this.experience = experience < 0 ? 0 : experience;
            this.level = level < 1 ? 1 : level;
            this.skillPoints = skillPoints < 0 ? 0 : skillPoints;
        }

        public int Experience => experience;
        public int Level => level;
        public int SkillPoints => skillPoints;

        public ProgressionState ToState()
        {
            return new ProgressionState(Experience, Level, SkillPoints);
        }
    }

    [Serializable]
    public struct RpgInventoryItemSnapshot
    {
        [SerializeField] private string itemId;
        [SerializeField] private int quantity;

        public RpgInventoryItemSnapshot(string itemId, int quantity)
        {
            this.itemId = Normalize(itemId);
            this.quantity = quantity < 0 ? 0 : quantity;
        }

        public string ItemId => itemId;
        public int Quantity => quantity;
        public bool IsValid => !string.IsNullOrEmpty(ItemId) && Quantity > 0;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgEquipmentSnapshot
    {
        [SerializeField] private string slotId;
        [SerializeField] private string itemId;

        public RpgEquipmentSnapshot(string slotId, string itemId)
        {
            this.slotId = Normalize(slotId);
            this.itemId = Normalize(itemId);
        }

        public string SlotId => slotId;
        public string ItemId => itemId;
        public bool IsValid => !string.IsNullOrEmpty(SlotId) && !string.IsNullOrEmpty(ItemId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgQuestSnapshot
    {
        [SerializeField] private string questId;
        [SerializeField] private QuestStatus status;
        [SerializeField] private bool rewardsGranted;
        [SerializeField] private RpgQuestObjectiveSnapshot[] objectives;

        public RpgQuestSnapshot(string questId, QuestStatus status, bool rewardsGranted, RpgQuestObjectiveSnapshot[] objectives)
        {
            this.questId = Normalize(questId);
            this.status = status;
            this.rewardsGranted = rewardsGranted;
            this.objectives = objectives ?? Array.Empty<RpgQuestObjectiveSnapshot>();
        }

        public string QuestId => questId;
        public QuestStatus Status => status;
        public bool RewardsGranted => rewardsGranted;
        public RpgQuestObjectiveSnapshot[] Objectives => objectives;
        public bool IsValid => !string.IsNullOrEmpty(QuestId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgQuestObjectiveSnapshot
    {
        [SerializeField] private string objectiveId;
        [SerializeField] private int progress;

        public RpgQuestObjectiveSnapshot(string objectiveId, int progress)
        {
            this.objectiveId = Normalize(objectiveId);
            this.progress = progress < 0 ? 0 : progress;
        }

        public string ObjectiveId => objectiveId;
        public int Progress => progress;
        public bool IsValid => !string.IsNullOrEmpty(ObjectiveId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgSkillUnlockSnapshot
    {
        [SerializeField] private string nodeId;
        [SerializeField] private int count;

        public RpgSkillUnlockSnapshot(string nodeId, int count)
        {
            this.nodeId = Normalize(nodeId);
            this.count = count < 0 ? 0 : count;
        }

        public string NodeId => nodeId;
        public int Count => count;
        public bool IsValid => !string.IsNullOrEmpty(NodeId) && Count > 0;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgDialogueSnapshot
    {
        [SerializeField] private string[] flags;
        [SerializeField] private RpgDialogueSessionSnapshot session;

        public RpgDialogueSnapshot(string[] flags, RpgDialogueSessionSnapshot session)
        {
            this.flags = flags ?? Array.Empty<string>();
            this.session = session;
        }

        public string[] Flags => flags;
        public RpgDialogueSessionSnapshot Session => session;
    }

    [Serializable]
    public struct RpgDialogueSessionSnapshot
    {
        [SerializeField] private string npcId;
        [SerializeField] private string graphId;
        [SerializeField] private string currentNodeId;
        [SerializeField] private bool ended;

        public RpgDialogueSessionSnapshot(string npcId, string graphId, string currentNodeId, bool ended)
        {
            this.npcId = Normalize(npcId);
            this.graphId = Normalize(graphId);
            this.currentNodeId = Normalize(currentNodeId);
            this.ended = ended;
        }

        public string NpcId => npcId;
        public string GraphId => graphId;
        public string CurrentNodeId => currentNodeId;
        public bool Ended => ended;
        public bool IsValid => !string.IsNullOrEmpty(NpcId) && !string.IsNullOrEmpty(GraphId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgHubReturnSnapshot
    {
        [SerializeField] private string hubId;
        [SerializeField] private string hubSceneId;
        [SerializeField] private string spawnPointId;
        [SerializeField] private string lastInteractableId;
        [SerializeField] private string requestedSceneId;

        public RpgHubReturnSnapshot(string hubId, string hubSceneId, string spawnPointId, string lastInteractableId, string requestedSceneId)
        {
            this.hubId = Normalize(hubId);
            this.hubSceneId = Normalize(hubSceneId);
            this.spawnPointId = Normalize(spawnPointId);
            this.lastInteractableId = Normalize(lastInteractableId);
            this.requestedSceneId = Normalize(requestedSceneId);
        }

        public string HubId => hubId;
        public string HubSceneId => hubSceneId;
        public string spawnPointId_ => spawnPointId; // keeping internal field name consistent with constructor param name if possible, but let's use property
        public string SpawnPointId => spawnPointId;
        public string LastInteractableId => lastInteractableId;
        public string RequestedSceneId => requestedSceneId;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
