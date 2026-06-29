using VContainer;
using NeonBlack.Gameplay.Data.Rpg;
using NeonBlack.Gameplay.Modules.Rpg.Runtime;

namespace NeonBlack.Gameplay.Glue.ServiceRegistration
{
    internal readonly struct PyralisRpgRuntimeServices
    {
        public PyralisRpgRuntimeServices(
            InventoryService inventoryService,
            ProgressionService progressionService,
            QuestService questService,
            EquipmentService equipmentService,
            SkillTreeService skillTreeService,
            DialogueService dialogueService,
            VendorService vendorService,
            RpgOpenZoneService openZoneService,
            HubInteractionService hubInteractionService)
        {
            InventoryService = inventoryService;
            ProgressionService = progressionService;
            QuestService = questService;
            EquipmentService = equipmentService;
            SkillTreeService = skillTreeService;
            DialogueService = dialogueService;
            VendorService = vendorService;
            OpenZoneService = openZoneService;
            HubInteractionService = hubInteractionService;
        }

        public InventoryService InventoryService { get; }
        public ProgressionService ProgressionService { get; }
        public QuestService QuestService { get; }
        public EquipmentService EquipmentService { get; }
        public SkillTreeService SkillTreeService { get; }
        public DialogueService DialogueService { get; }
        public VendorService VendorService { get; }
        public RpgOpenZoneService OpenZoneService { get; }
        public HubInteractionService HubInteractionService { get; }
    }

    internal static class PyralisRpgServiceInstaller
    {
        public static PyralisRpgRuntimeServices Register(
            IContainerBuilder builder,
            IItemCatalog itemCatalog,
            IProgressionCurve progressionCurve)
        {
            builder.Register<LocalRpgPersistenceService>(VContainer.Lifetime.Singleton).As<IRpgPersistenceService>();

            if (itemCatalog != null)
                builder.RegisterInstance(itemCatalog).As<IItemCatalog>();

            if (progressionCurve != null)
                builder.RegisterInstance(progressionCurve).As<IProgressionCurve>();

            InventoryService inventoryService = new InventoryService(itemCatalog);
            ProgressionService progressionService = new ProgressionService(progressionCurve);
            QuestService questService = new QuestService(progressionService, inventoryService);
            EquipmentService equipmentService = new EquipmentService();
            SkillTreeService skillTreeService = new SkillTreeService(progressionService);
            DialogueService dialogueService = new DialogueService(progressionService, inventoryService, questService, skillTreeService);
            VendorService vendorService = new VendorService(inventoryService);
            RpgOpenZoneService openZoneService = new RpgOpenZoneService();
            HubInteractionService hubInteractionService = new HubInteractionService(inventoryService, questService, skillTreeService, dialogueService);

            builder.RegisterInstance(inventoryService).AsImplementedInterfaces().AsSelf();
            builder.RegisterInstance(progressionService).AsImplementedInterfaces().AsSelf();
            builder.RegisterInstance(questService).AsImplementedInterfaces().AsSelf();
            builder.RegisterInstance(equipmentService).AsImplementedInterfaces().AsSelf();
            builder.RegisterInstance(skillTreeService).AsImplementedInterfaces().AsSelf();
            builder.RegisterInstance(dialogueService).AsImplementedInterfaces().AsSelf();
            builder.RegisterInstance(vendorService).AsImplementedInterfaces().AsSelf();
            builder.RegisterInstance(openZoneService).AsSelf();
            builder.RegisterInstance(hubInteractionService).AsImplementedInterfaces().AsSelf();

            return new PyralisRpgRuntimeServices(
                inventoryService,
                progressionService,
                questService,
                equipmentService,
                skillTreeService,
                dialogueService,
                vendorService,
                openZoneService,
                hubInteractionService);
        }
    }
}
