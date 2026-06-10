namespace NeonBlack.Gameplay.Core.Rpg
{
    public interface IRpgPersistenceService
    {
        void Save(RpgOwnerSaveData data);
        RpgOwnerSaveData Load(RpgOwnerKey owner);
        bool HasSaveData(RpgOwnerKey owner);
        void DeleteSaveData(RpgOwnerKey owner);
    }
}
