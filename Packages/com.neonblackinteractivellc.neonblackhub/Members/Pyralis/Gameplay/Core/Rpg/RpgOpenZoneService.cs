using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Rpg
{
    [AuthoringContract(
        Capability = AuthoringCapability.Session | AuthoringCapability.Environment,
        ModuleId = "rpg.openzone",
        Lane = "RPG",
        FirstProof = "Verify that zone state (flags, pickups) is persisted after exiting and re-entering a zone.",
        NativeSetup = new[]
        {
            "configure WorldZone flags",
            "link EncounterRestoration to zone entry",
            "register persistent zone state"
        }
    )]
    public sealed class RpgOpenZoneService
{
        private readonly Dictionary<string, RpgZoneDefinition> _definitions = new Dictionary<string, RpgZoneDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<RpgOwnerKey, OwnerZoneState> _ownerStates = new Dictionary<RpgOwnerKey, OwnerZoneState>();

        public bool RegisterZone(RpgZoneDefinition definition)
        {
            if (!definition.IsValid)
                return false;

            _definitions[definition.ZoneId] = definition;
            return true;
        }

        public bool EnterZone(RpgOwnerKey owner, string zoneId, string entranceId, string returnHubId, out string issue)
        {
            issue = string.Empty;
            string normalizedZoneId = Normalize(zoneId);
            if (!ValidateOwnerZone(owner, normalizedZoneId, out issue))
                return false;

            OwnerZoneState ownerState = GetOrCreateOwnerState(owner);
            string previousZoneId = ownerState.Travel.CurrentZoneId;
            ownerState.Travel = new RpgZoneTravelSnapshot(
                normalizedZoneId,
                previousZoneId,
                Normalize(entranceId),
                string.Empty,
                Normalize(returnHubId));
            GetOrCreateZoneState(ownerState, normalizedZoneId);
            return true;
        }

        public bool ExitZone(RpgOwnerKey owner, string exitId, string nextZoneId, out string issue)
        {
            issue = string.Empty;
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            OwnerZoneState ownerState = GetOrCreateOwnerState(owner);
            ownerState.Travel = new RpgZoneTravelSnapshot(
                Normalize(nextZoneId),
                ownerState.Travel.CurrentZoneId,
                ownerState.Travel.LastEntranceId,
                Normalize(exitId),
                ownerState.Travel.ReturnHubId);
            return true;
        }

        public RpgZoneTravelSnapshot GetTravelSnapshot(RpgOwnerKey owner)
        {
            return owner.IsValid && _ownerStates.TryGetValue(owner, out OwnerZoneState state)
                ? state.Travel
                : default;
        }

        public RpgZoneRuntimeState GetZoneState(RpgOwnerKey owner, string zoneId)
        {
            string normalizedZoneId = Normalize(zoneId);
            if (!owner.IsValid || string.IsNullOrEmpty(normalizedZoneId))
                return new RpgZoneRuntimeState(normalizedZoneId);

            OwnerZoneState ownerState = GetOrCreateOwnerState(owner);
            return GetOrCreateZoneState(ownerState, normalizedZoneId);
        }

        public void SetZoneFlag(RpgOwnerKey owner, string zoneId, string flagId, bool value)
        {
            RpgZoneRuntimeState state = GetZoneState(owner, zoneId);
            state.SetFlag(flagId, value);
        }

        public void SetEncounterState(RpgOwnerKey owner, string zoneId, string encounterId, RpgZoneEntityStatus status)
        {
            GetZoneState(owner, zoneId).SetEncounterStatus(encounterId, status);
        }

        public void SetResourceState(RpgOwnerKey owner, string zoneId, string resourceId, int quantity, bool depleted)
        {
            GetZoneState(owner, zoneId).SetResource(resourceId, quantity, depleted);
        }

        public void SetPickupState(RpgOwnerKey owner, string zoneId, string pickupId, RpgZoneEntityStatus status)
        {
            GetZoneState(owner, zoneId).SetPickupStatus(pickupId, status);
        }

        public void SetNpcState(RpgOwnerKey owner, string zoneId, string npcId, string spawnPointId, bool active, string dialogueStateId)
        {
            GetZoneState(owner, zoneId).SetNpc(npcId, spawnPointId, active, dialogueStateId);
        }

        public RpgOpenZoneSnapshot GetSnapshot(RpgOwnerKey owner)
        {
            if (!owner.IsValid || !_ownerStates.TryGetValue(owner, out OwnerZoneState ownerState))
                return default;

            List<string> zoneIds = new List<string>(ownerState.Zones.Keys);
            zoneIds.Sort(StringComparer.Ordinal);
            RpgZoneSnapshot[] zones = new RpgZoneSnapshot[zoneIds.Count];
            for (int i = 0; i < zoneIds.Count; i++)
                zones[i] = ownerState.Zones[zoneIds[i]].ToSnapshot();

            return new RpgOpenZoneSnapshot(ownerState.Travel, zones);
        }

        public void RestoreSnapshot(RpgOwnerKey owner, RpgOpenZoneSnapshot snapshot)
        {
            if (!owner.IsValid)
                return;

            OwnerZoneState ownerState = GetOrCreateOwnerState(owner);
            ownerState.Travel = snapshot.Travel;
            ownerState.Zones.Clear();

            RpgZoneSnapshot[] zones = snapshot.Zones ?? Array.Empty<RpgZoneSnapshot>();
            for (int i = 0; i < zones.Length; i++)
            {
                if (!zones[i].IsValid)
                    continue;

                ownerState.Zones[zones[i].ZoneId] = RpgZoneRuntimeState.FromSnapshot(zones[i]);
            }
        }

        public void Reset(RpgOwnerKey owner, RpgZoneResetScope scope)
        {
            if (!owner.IsValid || !_ownerStates.TryGetValue(owner, out OwnerZoneState ownerState))
                return;

            if (scope == RpgZoneResetScope.All)
            {
                ownerState.Travel = default;
                ownerState.Zones.Clear();
                return;
            }

            List<string> zoneIds = new List<string>(ownerState.Zones.Keys);
            for (int i = 0; i < zoneIds.Count; i++)
            {
                RpgZoneResetPolicy policy = _definitions.TryGetValue(zoneIds[i], out RpgZoneDefinition definition)
                    ? definition.ResetPolicy
                    : RpgZoneResetPolicy.CampaignPersistent;

                if (ShouldReset(policy, scope))
                    ownerState.Zones.Remove(zoneIds[i]);
            }
        }

        private bool ValidateOwnerZone(RpgOwnerKey owner, string zoneId, out string issue)
        {
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (string.IsNullOrEmpty(zoneId))
            {
                issue = "Zone id is required.";
                return false;
            }

            if (!_definitions.ContainsKey(zoneId))
            {
                issue = $"Zone `{zoneId}` is not registered.";
                return false;
            }

            issue = string.Empty;
            return true;
        }

        private OwnerZoneState GetOrCreateOwnerState(RpgOwnerKey owner)
        {
            if (_ownerStates.TryGetValue(owner, out OwnerZoneState state))
                return state;

            state = new OwnerZoneState();
            _ownerStates[owner] = state;
            return state;
        }

        private static RpgZoneRuntimeState GetOrCreateZoneState(OwnerZoneState ownerState, string zoneId)
        {
            if (ownerState.Zones.TryGetValue(zoneId, out RpgZoneRuntimeState state))
                return state;

            state = new RpgZoneRuntimeState(zoneId);
            ownerState.Zones[zoneId] = state;
            return state;
        }

        private static bool ShouldReset(RpgZoneResetPolicy policy, RpgZoneResetScope scope)
        {
            if (scope == RpgZoneResetScope.NewVisit)
                return policy == RpgZoneResetPolicy.ResetOnVisit || policy == RpgZoneResetPolicy.Ephemeral;

            if (scope == RpgZoneResetScope.NewRun)
                return policy == RpgZoneResetPolicy.ResetOnRun || policy == RpgZoneResetPolicy.ResetOnVisit || policy == RpgZoneResetPolicy.Ephemeral;

            return false;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private sealed class OwnerZoneState
        {
            public RpgZoneTravelSnapshot Travel;
            public readonly Dictionary<string, RpgZoneRuntimeState> Zones = new Dictionary<string, RpgZoneRuntimeState>(StringComparer.Ordinal);
        }
    }

    public sealed class RpgZoneRuntimeState
    {
        private readonly HashSet<string> _flags = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, RpgZoneEntityStatus> _encounters = new Dictionary<string, RpgZoneEntityStatus>(StringComparer.Ordinal);
        private readonly Dictionary<string, RpgZoneResourceSnapshot> _resources = new Dictionary<string, RpgZoneResourceSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, RpgZoneEntityStatus> _pickups = new Dictionary<string, RpgZoneEntityStatus>(StringComparer.Ordinal);
        private readonly Dictionary<string, RpgZoneNpcSnapshot> _npcs = new Dictionary<string, RpgZoneNpcSnapshot>(StringComparer.Ordinal);

        public RpgZoneRuntimeState(string zoneId)
        {
            ZoneId = Normalize(zoneId);
        }

        public string ZoneId { get; }

        public bool HasFlag(string flagId)
        {
            return _flags.Contains(Normalize(flagId));
        }

        public void SetFlag(string flagId, bool value)
        {
            string normalizedFlagId = Normalize(flagId);
            if (string.IsNullOrEmpty(normalizedFlagId))
                return;

            if (value)
                _flags.Add(normalizedFlagId);
            else
                _flags.Remove(normalizedFlagId);
        }

        public RpgZoneEntityStatus GetEncounterStatus(string encounterId)
        {
            return _encounters.TryGetValue(Normalize(encounterId), out RpgZoneEntityStatus status)
                ? status
                : RpgZoneEntityStatus.Unknown;
        }

        public void SetEncounterStatus(string encounterId, RpgZoneEntityStatus status)
        {
            string normalizedEncounterId = Normalize(encounterId);
            if (!string.IsNullOrEmpty(normalizedEncounterId))
                _encounters[normalizedEncounterId] = status;
        }

        public RpgZoneResourceSnapshot GetResource(string resourceId)
        {
            return _resources.TryGetValue(Normalize(resourceId), out RpgZoneResourceSnapshot resource)
                ? resource
                : default;
        }

        public void SetResource(string resourceId, int quantity, bool depleted)
        {
            string normalizedResourceId = Normalize(resourceId);
            if (!string.IsNullOrEmpty(normalizedResourceId))
                _resources[normalizedResourceId] = new RpgZoneResourceSnapshot(normalizedResourceId, quantity, depleted);
        }

        public RpgZoneEntityStatus GetPickupStatus(string pickupId)
        {
            return _pickups.TryGetValue(Normalize(pickupId), out RpgZoneEntityStatus status)
                ? status
                : RpgZoneEntityStatus.Unknown;
        }

        public void SetPickupStatus(string pickupId, RpgZoneEntityStatus status)
        {
            string normalizedPickupId = Normalize(pickupId);
            if (!string.IsNullOrEmpty(normalizedPickupId))
                _pickups[normalizedPickupId] = status;
        }

        public RpgZoneNpcSnapshot GetNpc(string npcId)
        {
            return _npcs.TryGetValue(Normalize(npcId), out RpgZoneNpcSnapshot npc)
                ? npc
                : default;
        }

        public void SetNpc(string npcId, string spawnPointId, bool active, string dialogueStateId)
        {
            string normalizedNpcId = Normalize(npcId);
            if (!string.IsNullOrEmpty(normalizedNpcId))
                _npcs[normalizedNpcId] = new RpgZoneNpcSnapshot(normalizedNpcId, spawnPointId, active, dialogueStateId);
        }

        public RpgZoneSnapshot ToSnapshot()
        {
            return new RpgZoneSnapshot(
                ZoneId,
                CreateFlagSnapshot(),
                CreateStatusSnapshot(_encounters),
                CreateResourceSnapshot(),
                CreateStatusSnapshot(_pickups),
                CreateNpcSnapshot());
        }

        public static RpgZoneRuntimeState FromSnapshot(RpgZoneSnapshot snapshot)
        {
            RpgZoneRuntimeState state = new RpgZoneRuntimeState(snapshot.ZoneId);
            string[] flags = snapshot.Flags ?? Array.Empty<string>();
            for (int i = 0; i < flags.Length; i++)
                state.SetFlag(flags[i], true);

            RestoreStatus(snapshot.Encounters, state._encounters);
            RestoreStatus(snapshot.Pickups, state._pickups);

            RpgZoneResourceSnapshot[] resources = snapshot.Resources ?? Array.Empty<RpgZoneResourceSnapshot>();
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i].IsValid)
                    state._resources[resources[i].ResourceId] = resources[i];
            }

            RpgZoneNpcSnapshot[] npcs = snapshot.Npcs ?? Array.Empty<RpgZoneNpcSnapshot>();
            for (int i = 0; i < npcs.Length; i++)
            {
                if (npcs[i].IsValid)
                    state._npcs[npcs[i].NpcId] = npcs[i];
            }

            return state;
        }

        private string[] CreateFlagSnapshot()
        {
            string[] snapshot = new string[_flags.Count];
            _flags.CopyTo(snapshot);
            Array.Sort(snapshot, StringComparer.Ordinal);
            return snapshot;
        }

        private RpgZoneEntitySnapshot[] CreateStatusSnapshot(Dictionary<string, RpgZoneEntityStatus> source)
        {
            List<string> ids = new List<string>(source.Keys);
            ids.Sort(StringComparer.Ordinal);
            RpgZoneEntitySnapshot[] snapshot = new RpgZoneEntitySnapshot[ids.Count];
            for (int i = 0; i < ids.Count; i++)
                snapshot[i] = new RpgZoneEntitySnapshot(ids[i], source[ids[i]]);

            return snapshot;
        }

        private RpgZoneResourceSnapshot[] CreateResourceSnapshot()
        {
            List<string> ids = new List<string>(_resources.Keys);
            ids.Sort(StringComparer.Ordinal);
            RpgZoneResourceSnapshot[] snapshot = new RpgZoneResourceSnapshot[ids.Count];
            for (int i = 0; i < ids.Count; i++)
                snapshot[i] = _resources[ids[i]];

            return snapshot;
        }

        private RpgZoneNpcSnapshot[] CreateNpcSnapshot()
        {
            List<string> ids = new List<string>(_npcs.Keys);
            ids.Sort(StringComparer.Ordinal);
            RpgZoneNpcSnapshot[] snapshot = new RpgZoneNpcSnapshot[ids.Count];
            for (int i = 0; i < ids.Count; i++)
                snapshot[i] = _npcs[ids[i]];

            return snapshot;
        }

        private static void RestoreStatus(RpgZoneEntitySnapshot[] snapshots, Dictionary<string, RpgZoneEntityStatus> target)
        {
            RpgZoneEntitySnapshot[] safeSnapshots = snapshots ?? Array.Empty<RpgZoneEntitySnapshot>();
            for (int i = 0; i < safeSnapshots.Length; i++)
            {
                if (safeSnapshots[i].IsValid)
                    target[safeSnapshots[i].EntityId] = safeSnapshots[i].Status;
            }
        }

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
