using VContainer;
using NeonBlack.Gameplay.Data.Rpg;

namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public static class PyralisRpgServiceInstaller
    {
        public static void Register(
            IContainerBuilder builder,
            IItemCatalog itemCatalog,
            IProgressionCurve progressionCurve)
        {
            builder.Register<LocalRpgPersistenceService>(Lifetime.Singleton).As<IRpgPersistenceService>();

            if (itemCatalog != null)
                builder.RegisterInstance(itemCatalog).As<IItemCatalog>();

            if (progressionCurve != null)
                builder.RegisterInstance(progressionCurve).As<IProgressionCurve>();

            builder.Register<InventoryService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<ProgressionService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<QuestService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<EquipmentService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<SkillTreeService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<DialogueService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<VendorService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<RpgOpenZoneService>(Lifetime.Singleton).AsSelf();
            builder.Register<HubInteractionService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }
    }
}
