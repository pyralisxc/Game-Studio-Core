using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Rpg
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

public enum RpgZoneResetPolicy
    {
        CampaignPersistent = 0,
        ResetOnRun = 1,
        ResetOnVisit = 2,
        Ephemeral = 3
    }

    public enum RpgZoneResetScope
    {
        NewVisit = 0,
        NewRun = 1,
        All = 2
    }

    public enum RpgZoneEntityStatus
    {
        Unknown = 0,
        Active = 1,
        Cleared = 2,
        Collected = 3,
        Disabled = 4
    }

    public readonly struct RpgZoneDefinition
    {
        public RpgZoneDefinition(
            string zoneId,
            string displayName,
            string sceneId,
            RpgZoneResetPolicy resetPolicy,
            string[] entranceIds = null,
            string[] exitIds = null)
        {
            ZoneId = Normalize(zoneId);
            DisplayName = Normalize(displayName);
            SceneId = Normalize(sceneId);
            ResetPolicy = resetPolicy;
            EntranceIds = NormalizeArray(entranceIds);
            ExitIds = NormalizeArray(exitIds);
        }

        public string ZoneId { get; }
        public string DisplayName { get; }
        public string SceneId { get; }
        public RpgZoneResetPolicy ResetPolicy { get; }
        public string[] EntranceIds { get; }
        public string[] ExitIds { get; }
        public bool IsValid => !string.IsNullOrEmpty(ZoneId);

        private static string[] NormalizeArray(string[] values)
        {
            if (values == null || values.Length == 0)
                return Array.Empty<string>();

            List<string> normalized = new List<string>(values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                string value = Normalize(values[i]);
                if (!string.IsNullOrEmpty(value))
                    normalized.Add(value);
            }

            return normalized.ToArray();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgOpenZoneSnapshot
    {
        [SerializeField] private RpgZoneTravelSnapshot travel;
        [SerializeField] private RpgZoneSnapshot[] zones;

        public RpgOpenZoneSnapshot(RpgZoneTravelSnapshot travel, RpgZoneSnapshot[] zones)
        {
            this.travel = travel;
            this.zones = zones ?? Array.Empty<RpgZoneSnapshot>();
        }

        public RpgZoneTravelSnapshot Travel => travel;
        public RpgZoneSnapshot[] Zones => zones;
    }

    [Serializable]
    public struct RpgZoneTravelSnapshot
    {
        [SerializeField] private string currentZoneId;
        [SerializeField] private string previousZoneId;
        [SerializeField] private string lastEntranceId;
        [SerializeField] private string lastExitId;
        [SerializeField] private string returnHubId;

        public RpgZoneTravelSnapshot(string currentZoneId, string previousZoneId, string lastEntranceId, string lastExitId, string returnHubId)
        {
            this.currentZoneId = Normalize(currentZoneId);
            this.previousZoneId = Normalize(previousZoneId);
            this.lastEntranceId = Normalize(lastEntranceId);
            this.lastExitId = Normalize(lastExitId);
            this.returnHubId = Normalize(returnHubId);
        }

        public string CurrentZoneId => currentZoneId;
        public string PreviousZoneId => previousZoneId;
        public string LastEntranceId => lastEntranceId;
        public string LastExitId => lastExitId;
        public string ReturnHubId => returnHubId;

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgZoneSnapshot
    {
        [SerializeField] private string zoneId;
        [SerializeField] private string[] flags;
        [SerializeField] private RpgZoneEntitySnapshot[] encounters;
        [SerializeField] private RpgZoneResourceSnapshot[] resources;
        [SerializeField] private RpgZoneEntitySnapshot[] pickups;
        [SerializeField] private RpgZoneNpcSnapshot[] npcs;

        public RpgZoneSnapshot(
            string zoneId,
            string[] flags,
            RpgZoneEntitySnapshot[] encounters,
            RpgZoneResourceSnapshot[] resources,
            RpgZoneEntitySnapshot[] pickups,
            RpgZoneNpcSnapshot[] npcs)
        {
            this.zoneId = Normalize(zoneId);
            this.flags = flags ?? Array.Empty<string>();
            this.encounters = encounters ?? Array.Empty<RpgZoneEntitySnapshot>();
            this.resources = resources ?? Array.Empty<RpgZoneResourceSnapshot>();
            this.pickups = pickups ?? Array.Empty<RpgZoneEntitySnapshot>();
            this.npcs = npcs ?? Array.Empty<RpgZoneNpcSnapshot>();
        }

        public string ZoneId => zoneId;
        public string[] Flags => flags;
        public RpgZoneEntitySnapshot[] Encounters => encounters;
        public RpgZoneResourceSnapshot[] Resources => resources;
        public RpgZoneEntitySnapshot[] Pickups => pickups;
        public RpgZoneNpcSnapshot[] Npcs => npcs;
        public bool IsValid => !string.IsNullOrEmpty(ZoneId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgZoneEntitySnapshot
    {
        [SerializeField] private string entityId;
        [SerializeField] private RpgZoneEntityStatus status;

        public RpgZoneEntitySnapshot(string entityId, RpgZoneEntityStatus status)
        {
            this.entityId = Normalize(entityId);
            this.status = status;
        }

        public string EntityId => entityId;
        public RpgZoneEntityStatus Status => status;
        public bool IsValid => !string.IsNullOrEmpty(EntityId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgZoneResourceSnapshot
    {
        [SerializeField] private string resourceId;
        [SerializeField] private int quantity;
        [SerializeField] private bool depleted;

        public RpgZoneResourceSnapshot(string resourceId, int quantity, bool depleted)
        {
            this.resourceId = Normalize(resourceId);
            this.quantity = quantity < 0 ? 0 : quantity;
            this.depleted = depleted;
        }

        public string ResourceId => resourceId;
        public int Quantity => quantity;
        public bool Depleted => depleted;
        public bool IsValid => !string.IsNullOrEmpty(ResourceId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    [Serializable]
    public struct RpgZoneNpcSnapshot
    {
        [SerializeField] private string npcId;
        [SerializeField] private string spawnPointId;
        [SerializeField] private bool active;
        [SerializeField] private string dialogueStateId;

        public RpgZoneNpcSnapshot(string npcId, string spawnPointId, bool active, string dialogueStateId)
        {
            this.npcId = Normalize(npcId);
            this.spawnPointId = Normalize(spawnPointId);
            this.active = active;
            this.dialogueStateId = Normalize(dialogueStateId);
        }

        public string NpcId => npcId;
        public string SpawnPointId => spawnPointId;
        public bool Active => active;
        public string DialogueStateId => dialogueStateId;
        public bool IsValid => !string.IsNullOrEmpty(NpcId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}