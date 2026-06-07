using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Rpg
{
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

    public readonly struct RpgOpenZoneSnapshot
    {
        public RpgOpenZoneSnapshot(RpgZoneTravelSnapshot travel, RpgZoneSnapshot[] zones)
        {
            Travel = travel;
            Zones = zones ?? Array.Empty<RpgZoneSnapshot>();
        }

        public RpgZoneTravelSnapshot Travel { get; }
        public RpgZoneSnapshot[] Zones { get; }
    }

    public readonly struct RpgZoneTravelSnapshot
    {
        public RpgZoneTravelSnapshot(string currentZoneId, string previousZoneId, string lastEntranceId, string lastExitId, string returnHubId)
        {
            CurrentZoneId = Normalize(currentZoneId);
            PreviousZoneId = Normalize(previousZoneId);
            LastEntranceId = Normalize(lastEntranceId);
            LastExitId = Normalize(lastExitId);
            ReturnHubId = Normalize(returnHubId);
        }

        public string CurrentZoneId { get; }
        public string PreviousZoneId { get; }
        public string LastEntranceId { get; }
        public string LastExitId { get; }
        public string ReturnHubId { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public readonly struct RpgZoneSnapshot
    {
        public RpgZoneSnapshot(
            string zoneId,
            string[] flags,
            RpgZoneEntitySnapshot[] encounters,
            RpgZoneResourceSnapshot[] resources,
            RpgZoneEntitySnapshot[] pickups,
            RpgZoneNpcSnapshot[] npcs)
        {
            ZoneId = Normalize(zoneId);
            Flags = flags ?? Array.Empty<string>();
            Encounters = encounters ?? Array.Empty<RpgZoneEntitySnapshot>();
            Resources = resources ?? Array.Empty<RpgZoneResourceSnapshot>();
            Pickups = pickups ?? Array.Empty<RpgZoneEntitySnapshot>();
            Npcs = npcs ?? Array.Empty<RpgZoneNpcSnapshot>();
        }

        public string ZoneId { get; }
        public string[] Flags { get; }
        public RpgZoneEntitySnapshot[] Encounters { get; }
        public RpgZoneResourceSnapshot[] Resources { get; }
        public RpgZoneEntitySnapshot[] Pickups { get; }
        public RpgZoneNpcSnapshot[] Npcs { get; }
        public bool IsValid => !string.IsNullOrEmpty(ZoneId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public readonly struct RpgZoneEntitySnapshot
    {
        public RpgZoneEntitySnapshot(string entityId, RpgZoneEntityStatus status)
        {
            EntityId = Normalize(entityId);
            Status = status;
        }

        public string EntityId { get; }
        public RpgZoneEntityStatus Status { get; }
        public bool IsValid => !string.IsNullOrEmpty(EntityId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public readonly struct RpgZoneResourceSnapshot
    {
        public RpgZoneResourceSnapshot(string resourceId, int quantity, bool depleted)
        {
            ResourceId = Normalize(resourceId);
            Quantity = quantity < 0 ? 0 : quantity;
            Depleted = depleted;
        }

        public string ResourceId { get; }
        public int Quantity { get; }
        public bool Depleted { get; }
        public bool IsValid => !string.IsNullOrEmpty(ResourceId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    public readonly struct RpgZoneNpcSnapshot
    {
        public RpgZoneNpcSnapshot(string npcId, string spawnPointId, bool active, string dialogueStateId)
        {
            NpcId = Normalize(npcId);
            SpawnPointId = Normalize(spawnPointId);
            Active = active;
            DialogueStateId = Normalize(dialogueStateId);
        }

        public string NpcId { get; }
        public string SpawnPointId { get; }
        public bool Active { get; }
        public string DialogueStateId { get; }
        public bool IsValid => !string.IsNullOrEmpty(NpcId);

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
