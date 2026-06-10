using System;
using System.IO;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public sealed class BinaryRpgPersistenceService : IRpgPersistenceService
    {
        private readonly string _basePath;

        public BinaryRpgPersistenceService()
        {
            _basePath = Path.Combine(Application.persistentDataPath, "Saves", "RpgBinary");
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        public void Save(RpgOwnerSaveData data)
        {
            if (data == null || !data.Owner.IsValid)
                return;

            string path = GetPath(data.Owner);
            using (var stream = File.Open(path, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                Serialize(writer, data);
            }
        }

        public RpgOwnerSaveData Load(RpgOwnerKey owner)
        {
            if (!owner.IsValid)
                return null;

            string path = GetPath(owner);
            if (!File.Exists(path))
                return null;

            using (var stream = File.Open(path, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                return Deserialize(reader);
            }
        }

        public bool HasSaveData(RpgOwnerKey owner)
        {
            return owner.IsValid && File.Exists(GetPath(owner));
        }

        public void DeleteSaveData(RpgOwnerKey owner)
        {
            if (!owner.IsValid)
                return;

            string path = GetPath(owner);
            if (File.Exists(path))
                File.Delete(path);
        }

        private string GetPath(RpgOwnerKey owner)
        {
            return Path.Combine(_basePath, $"{owner.Kind}_{owner.StableId}.dat");
        }

        private void Serialize(BinaryWriter writer, RpgOwnerSaveData data)
        {
            writer.Write(data.SchemaVersion);
            writer.Write((int)data.Owner.Kind);
            writer.Write(data.Owner.StableId);

            // Progression
            writer.Write(data.Progression.Experience);
            writer.Write(data.Progression.Level);
            writer.Write(data.Progression.SkillPoints);

            // Inventory
            writer.Write(data.Inventory.Length);
            foreach (var item in data.Inventory)
            {
                writer.Write(item.ItemId);
                writer.Write(item.Quantity);
            }

            // Equipment
            writer.Write(data.Equipment.Length);
            foreach (var eq in data.Equipment)
            {
                writer.Write(eq.SlotId);
                writer.Write(eq.ItemId);
            }

            // Quests
            writer.Write(data.Quests.Length);
            foreach (var q in data.Quests)
            {
                writer.Write(q.QuestId);
                writer.Write((int)q.Status);
                writer.Write(q.RewardsGranted);
                
                writer.Write(q.Objectives.Length);
                foreach (var obj in q.Objectives)
                {
                    writer.Write(obj.ObjectiveId);
                    writer.Write(obj.Progress);
                }
            }

            // Skill Unlocks
            writer.Write(data.SkillUnlocks.Length);
            foreach (var s in data.SkillUnlocks)
            {
                writer.Write(s.NodeId);
                writer.Write(s.Count);
            }

            // Dialogue
            writer.Write(data.Dialogue.Flags.Length);
            foreach (var f in data.Dialogue.Flags) writer.Write(f);
            
            writer.Write(data.Dialogue.Session.IsValid);
            if (data.Dialogue.Session.IsValid)
            {
                writer.Write(data.Dialogue.Session.NpcId);
                writer.Write(data.Dialogue.Session.GraphId);
                writer.Write(data.Dialogue.Session.CurrentNodeId);
                writer.Write(data.Dialogue.Session.Ended);
            }

            // Hub Return
            writer.Write(data.HubReturn.HubId);
            writer.Write(data.HubReturn.HubSceneId);
            writer.Write(data.HubReturn.SpawnPointId);
            writer.Write(data.HubReturn.LastInteractableId);
            writer.Write(data.HubReturn.RequestedSceneId);
        }

        private RpgOwnerSaveData Deserialize(BinaryReader reader)
        {
            int version = reader.ReadInt32();
            RpgOwnerKind kind = (RpgOwnerKind)reader.ReadInt32();
            string stableId = reader.ReadString();
            RpgOwnerKey owner = new RpgOwnerKey(kind, stableId);

            RpgProgressionSnapshot progression = new RpgProgressionSnapshot(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

            int invCount = reader.ReadInt32();
            RpgInventoryItemSnapshot[] inventory = new RpgInventoryItemSnapshot[invCount];
            for (int i = 0; i < invCount; i++) inventory[i] = new RpgInventoryItemSnapshot(reader.ReadString(), reader.ReadInt32());

            int eqCount = reader.ReadInt32();
            RpgEquipmentSnapshot[] equipment = new RpgEquipmentSnapshot[eqCount];
            for (int i = 0; i < eqCount; i++) equipment[i] = new RpgEquipmentSnapshot(reader.ReadString(), reader.ReadString());

            int qCount = reader.ReadInt32();
            RpgQuestSnapshot[] quests = new RpgQuestSnapshot[qCount];
            for (int i = 0; i < qCount; i++)
            {
                string qId = reader.ReadString();
                QuestStatus qStatus = (QuestStatus)reader.ReadInt32();
                bool qRewards = reader.ReadBoolean();
                int objCount = reader.ReadInt32();
                RpgQuestObjectiveSnapshot[] objectives = new RpgQuestObjectiveSnapshot[objCount];
                for (int j = 0; objCount > 0 && j < objCount; j++) objectives[j] = new RpgQuestObjectiveSnapshot(reader.ReadString(), reader.ReadInt32());
                quests[i] = new RpgQuestSnapshot(qId, qStatus, qRewards, objectives);
            }

            int skillCount = reader.ReadInt32();
            RpgSkillUnlockSnapshot[] skillUnlocks = new RpgSkillUnlockSnapshot[skillCount];
            for (int i = 0; i < skillCount; i++) skillUnlocks[i] = new RpgSkillUnlockSnapshot(reader.ReadString(), reader.ReadInt32());

            int flagCount = reader.ReadInt32();
            string[] flags = new string[flagCount];
            for (int i = 0; i < flagCount; i++) flags[i] = reader.ReadString();

            RpgDialogueSessionSnapshot session = default;
            if (reader.ReadBoolean())
            {
                session = new RpgDialogueSessionSnapshot(reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadBoolean());
            }
            RpgDialogueSnapshot dialogue = new RpgDialogueSnapshot(flags, session);

            RpgHubReturnSnapshot hubReturn = new RpgHubReturnSnapshot(
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadString());

            return new RpgOwnerSaveData(owner, version, progression, inventory, equipment, quests, skillUnlocks, dialogue, hubReturn);
        }
    }
}