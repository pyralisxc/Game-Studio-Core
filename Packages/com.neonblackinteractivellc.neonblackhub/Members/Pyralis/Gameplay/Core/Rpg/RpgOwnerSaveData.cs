using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public sealed class RpgOwnerSaveData
    {
        public const int CurrentSchemaVersion = 1;

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
            Owner = owner;
            SchemaVersion = schemaVersion < 1 ? CurrentSchemaVersion : schemaVersion;
            Progression = progression;
            Inventory = inventory ?? Array.Empty<RpgInventoryItemSnapshot>();
            Equipment = equipment ?? Array.Empty<RpgEquipmentSnapshot>();
            Quests = quests ?? Array.Empty<RpgQuestSnapshot>();
            SkillUnlocks = skillUnlocks ?? Array.Empty<RpgSkillUnlockSnapshot>();
            Dialogue = dialogue;
            HubReturn = hubReturn;
            OpenZones = openZones;
        }

        public RpgOwnerKey Owner { get; }
        public int SchemaVersion { get; }
        public RpgProgressionSnapshot Progression { get; }
        public RpgInventoryItemSnapshot[] Inventory { get; }
        public RpgEquipmentSnapshot[] Equipment { get; }
        public RpgQuestSnapshot[] Quests { get; }
        public RpgSkillUnlockSnapshot[] SkillUnlocks { get; }
        public RpgDialogueSnapshot Dialogue { get; }
        public RpgHubReturnSnapshot HubReturn { get; }
        public RpgOpenZoneSnapshot OpenZones { get; }

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

    public readonly struct RpgProgressionSnapshot
    {
        public RpgProgressionSnapshot(int experience, int level, int skillPoints)
        {
            Experience = experience < 0 ? 0 : experience;
            Level = level < 1 ? 1 : level;
            SkillPoints = skillPoints < 0 ? 0 : skillPoints;
        }

        public int Experience { get; }
        public int Level { get; }
        public int SkillPoints { get; }

        public ProgressionState ToState()
        {
            return new ProgressionState(Experience, Level, SkillPoints);
        }
    }

    public readonly struct RpgInventoryItemSnapshot
    {
        public RpgInventoryItemSnapshot(string itemId, int quantity)
        {
            ItemId = Normalize(itemId);
            Quantity = quantity < 0 ? 0 : quantity;
        }

        public string ItemId { get; }
        public int Quantity { get; }
        public bool IsValid => !string.IsNullOrEmpty(ItemId) && Quantity > 0;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public readonly struct RpgEquipmentSnapshot
    {
        public RpgEquipmentSnapshot(string slotId, string itemId)
        {
            SlotId = Normalize(slotId);
            ItemId = Normalize(itemId);
        }

        public string SlotId { get; }
        public string ItemId { get; }
        public bool IsValid => !string.IsNullOrEmpty(SlotId) && !string.IsNullOrEmpty(ItemId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public readonly struct RpgQuestSnapshot
    {
        public RpgQuestSnapshot(string questId, QuestStatus status, bool rewardsGranted, RpgQuestObjectiveSnapshot[] objectives)
        {
            QuestId = Normalize(questId);
            Status = status;
            RewardsGranted = rewardsGranted;
            Objectives = objectives ?? Array.Empty<RpgQuestObjectiveSnapshot>();
        }

        public string QuestId { get; }
        public QuestStatus Status { get; }
        public bool RewardsGranted { get; }
        public RpgQuestObjectiveSnapshot[] Objectives { get; }
        public bool IsValid => !string.IsNullOrEmpty(QuestId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public readonly struct RpgQuestObjectiveSnapshot
    {
        public RpgQuestObjectiveSnapshot(string objectiveId, int progress)
        {
            ObjectiveId = Normalize(objectiveId);
            Progress = progress < 0 ? 0 : progress;
        }

        public string ObjectiveId { get; }
        public int Progress { get; }
        public bool IsValid => !string.IsNullOrEmpty(ObjectiveId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public readonly struct RpgSkillUnlockSnapshot
    {
        public RpgSkillUnlockSnapshot(string nodeId, int count)
        {
            NodeId = Normalize(nodeId);
            Count = count < 0 ? 0 : count;
        }

        public string NodeId { get; }
        public int Count { get; }
        public bool IsValid => !string.IsNullOrEmpty(NodeId) && Count > 0;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public readonly struct RpgDialogueSnapshot
    {
        public RpgDialogueSnapshot(string[] flags, RpgDialogueSessionSnapshot session)
        {
            Flags = flags ?? Array.Empty<string>();
            Session = session;
        }

        public string[] Flags { get; }
        public RpgDialogueSessionSnapshot Session { get; }
    }

    public readonly struct RpgDialogueSessionSnapshot
    {
        public RpgDialogueSessionSnapshot(string npcId, string graphId, string currentNodeId, bool ended)
        {
            NpcId = Normalize(npcId);
            GraphId = Normalize(graphId);
            CurrentNodeId = Normalize(currentNodeId);
            Ended = ended;
        }

        public string NpcId { get; }
        public string GraphId { get; }
        public string CurrentNodeId { get; }
        public bool Ended { get; }
        public bool IsValid => !string.IsNullOrEmpty(NpcId) && !string.IsNullOrEmpty(GraphId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public readonly struct RpgHubReturnSnapshot
    {
        public RpgHubReturnSnapshot(string hubId, string hubSceneId, string spawnPointId, string lastInteractableId, string requestedSceneId)
        {
            HubId = Normalize(hubId);
            HubSceneId = Normalize(hubSceneId);
            SpawnPointId = Normalize(spawnPointId);
            LastInteractableId = Normalize(lastInteractableId);
            RequestedSceneId = Normalize(requestedSceneId);
        }

        public string HubId { get; }
        public string HubSceneId { get; }
        public string SpawnPointId { get; }
        public string LastInteractableId { get; }
        public string RequestedSceneId { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
