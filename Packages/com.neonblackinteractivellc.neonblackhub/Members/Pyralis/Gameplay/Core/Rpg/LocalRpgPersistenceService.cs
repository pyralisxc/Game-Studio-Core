using System.IO;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public sealed class LocalRpgPersistenceService : IRpgPersistenceService
    {
        private readonly string _basePath;

        public LocalRpgPersistenceService()
        {
            _basePath = Path.Combine(Application.persistentDataPath, "Saves", "Rpg");
            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        public void Save(RpgOwnerSaveData data)
        {
            if (data == null || !data.Owner.IsValid)
                return;

            string json = JsonUtility.ToJson(data, true);
            string path = GetPath(data.Owner);
            File.WriteAllText(path, json);
        }

        public RpgOwnerSaveData Load(RpgOwnerKey owner)
        {
            if (!owner.IsValid)
                return null;

            string path = GetPath(owner);
            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<RpgOwnerSaveData>(json);
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
            return Path.Combine(_basePath, $"{owner.Kind}_{owner.StableId}.json");
        }
    }
}
