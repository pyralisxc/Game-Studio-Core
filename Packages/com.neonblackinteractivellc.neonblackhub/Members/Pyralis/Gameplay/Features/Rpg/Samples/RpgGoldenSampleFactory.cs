using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Features.Rpg.Samples
{
    public static class RpgGoldenSampleFactory
    {
        public static RpgGoldenSampleRuntime CreateRuntime()
        {
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, RpgGoldenSampleIds.OwnerStableId);
            InventoryService inventory = new InventoryService();
            ProgressionService progression = new ProgressionService(null);
            QuestService quests = new QuestService(progression, inventory);
            SkillTreeService skills = new SkillTreeService(progression);
            DialogueService dialogue = new DialogueService(progression, inventory, quests, skills);
            EquipmentService equipment = new EquipmentService();
            VendorService vendor = new VendorService(inventory);
            RpgOpenZoneService zones = new RpgOpenZoneService();

            IQuestDefinition quest = CreateQuest();
            ISkillTree skillTree = CreateSkillTree();
            IVendorDefinition vendorDefinition = CreateVendor();
            IEquipmentSlot capeSlot = new GoldenEquipmentSlot(RpgGoldenSampleIds.CapeSlotId);
            IEquippableItem cape = new GoldenEquippableItem(
                RpgGoldenSampleIds.CapeItemId,
                RpgGoldenSampleIds.CapeSlotId,
                new[] { new StatModifier("stat.wisdom", 3f, "item.golden.wisdom-cape") });
            HubDefinitionModel hub = CreateHub();
            DialogueGraph dialogueGraph = CreateDialogueGraph();
            NpcProfile guide = new NpcProfile(RpgGoldenSampleIds.GuideNpcId, "Meadow Guide", "guide", new[] { "golden" }, "faction.village", string.Empty);
            RpgZoneDefinition meadow = new RpgZoneDefinition(
                RpgGoldenSampleIds.MeadowZoneId,
                "Golden Meadow",
                RpgGoldenSampleIds.MeadowSceneId,
                RpgZoneResetPolicy.CampaignPersistent,
                new[] { RpgGoldenSampleIds.MeadowEntranceId },
                new[] { RpgGoldenSampleIds.MeadowExitId });

            inventory.TryAddItem(owner, RpgGoldenSampleIds.GoldItemId, 10, out _);
            dialogue.RegisterQuest(quest);
            zones.RegisterZone(meadow);

            HubInteractionService hubInteractions = new HubInteractionService(inventory, quests, skills, dialogue);
            hubInteractions.RegisterQuest(quest);
            hubInteractions.RegisterSkillTree(RpgGoldenSampleIds.SkillTreeId, skillTree);

            return new RpgGoldenSampleRuntime(
                owner,
                hub,
                guide,
                dialogueGraph,
                quest,
                vendorDefinition,
                capeSlot,
                cape,
                skillTree,
                meadow,
                inventory,
                progression,
                quests,
                equipment,
                skills,
                dialogue,
                vendor,
                zones,
                hubInteractions);
        }

        private static HubDefinitionModel CreateHub()
        {
            return new HubDefinitionModel(
                RpgGoldenSampleIds.HubId,
                "Golden RPG Hub",
                RpgGoldenSampleIds.HubSceneId,
                "spawn.hub",
                new[] { "golden", "sample", "rpg" },
                new[]
                {
                    CreateInteractable(RpgGoldenSampleIds.DialogueInteractableId, "Guide", "Talk to Guide", HubInteractionKind.NPCDialogue, PlayerPanelRoute.Dialogue, string.Empty, RpgGoldenSampleIds.GuideDialogueGraphId, RpgGoldenSampleIds.GuideNpcId, 10),
                    CreateInteractable(RpgGoldenSampleIds.QuestBoardInteractableId, "Quest Board", "Open Quest Board", HubInteractionKind.QuestBoard, PlayerPanelRoute.QuestBoard, string.Empty, string.Empty, string.Empty, 20),
                    CreateInteractable(RpgGoldenSampleIds.VendorInteractableId, "Apothecary", "Open Apothecary", HubInteractionKind.Vendor, PlayerPanelRoute.Vendor, string.Empty, string.Empty, RpgGoldenSampleIds.VendorId, 30),
                    CreateInteractable(RpgGoldenSampleIds.LoadoutInteractableId, "Loadout", "Open Loadout", HubInteractionKind.LoadoutStation, PlayerPanelRoute.Loadout, string.Empty, string.Empty, string.Empty, 40),
                    CreateInteractable(RpgGoldenSampleIds.TrainerInteractableId, "Trainer", "Train Wisdom", HubInteractionKind.Trainer, PlayerPanelRoute.Trainer, string.Empty, string.Empty, RpgGoldenSampleIds.SkillTreeId, 50),
                    CreateInteractable(RpgGoldenSampleIds.PortalInteractableId, "Meadow Gate", "Enter Meadow", HubInteractionKind.Portal, PlayerPanelRoute.None, RpgGoldenSampleIds.MeadowSceneId, string.Empty, string.Empty, 60)
                });
        }

        private static HubInteractable CreateInteractable(string id, string name, string prompt, HubInteractionKind kind, PlayerPanelRoute route, string sceneId, string graphId, string npcId, int priority)
        {
            return new HubInteractable(
                id,
                name,
                prompt,
                "Locked",
                kind.ToString(),
                kind,
                HubInteractionAvailability.Available,
                route,
                sceneId,
                graphId,
                npcId,
                Array.Empty<HubInteractionCondition>(),
                Array.Empty<HubInteractionEffect>(),
                priority,
                name + " selected.");
        }

        private static DialogueGraph CreateDialogueGraph()
        {
            return new DialogueGraph(
                RpgGoldenSampleIds.GuideDialogueGraphId,
                RpgGoldenSampleIds.GuideStartNodeId,
                new[]
                {
                    new DialogueNode(
                        RpgGoldenSampleIds.GuideStartNodeId,
                        DialogueNodeKind.ChoiceHub,
                        RpgGoldenSampleIds.GuideNpcId,
                        "Bring back meadow herbs, then use your reward before entering the field again.",
                        new[]
                        {
                            new DialogueChoice(
                                RpgGoldenSampleIds.GuideAcceptChoiceId,
                                "I will gather them.",
                                RpgGoldenSampleIds.GuideEndNodeId,
                                Array.Empty<DialogueCondition>(),
                                new[]
                                {
                                    new DialogueEffect(DialogueEffectKind.SetDialogueFlag, RpgGoldenSampleIds.GuideAcceptedFlagId, string.Empty, 0, true),
                                    new DialogueEffect(DialogueEffectKind.StartQuest, RpgGoldenSampleIds.QuestId, string.Empty, 0, false)
                                })
                        },
                        Array.Empty<DialogueEffect>(),
                        string.Empty),
                    new DialogueNode(RpgGoldenSampleIds.GuideEndNodeId, DialogueNodeKind.Terminal, RpgGoldenSampleIds.GuideNpcId, string.Empty, Array.Empty<DialogueChoice>(), Array.Empty<DialogueEffect>(), string.Empty)
                });
        }

        private static IQuestDefinition CreateQuest()
        {
            return new GoldenQuestDefinition();
        }

        private static IVendorDefinition CreateVendor()
        {
            return new GoldenVendorDefinition();
        }

        private static ISkillTree CreateSkillTree()
        {
            return new GoldenSkillTree();
        }

        private sealed class GoldenQuestDefinition : IQuestDefinition
        {
            private static readonly QuestObjective[] GoldenObjectives =
            {
                new QuestObjective(RpgGoldenSampleIds.QuestObjectiveId, QuestObjectiveKind.CollectItem, RpgGoldenSampleIds.HerbItemId, 3)
            };

            private static readonly QuestReward[] GoldenRewards =
            {
                new QuestReward(25, 1, new[] { new QuestItemReward(RpgGoldenSampleIds.CapeItemId, 1) })
            };

            public string QuestId => RpgGoldenSampleIds.QuestId;
            public bool Repeatable => false;
            public QuestObjective[] Objectives => GoldenObjectives;
            public QuestReward[] Rewards => GoldenRewards;

            public bool TryGetObjective(string objectiveId, out QuestObjective objective)
            {
                if (objectiveId == RpgGoldenSampleIds.QuestObjectiveId)
                {
                    objective = GoldenObjectives[0];
                    return true;
                }

                objective = default;
                return false;
            }
        }

        private sealed class GoldenVendorDefinition : IVendorDefinition
        {
            private static readonly VendorOffer[] GoldenOffers =
            {
                new VendorOffer(RpgGoldenSampleIds.PotionOfferId, "Potion", RpgGoldenSampleIds.PotionItemId, RpgGoldenSampleIds.GoldItemId, 2, 1, true, true)
            };

            public string VendorId => RpgGoldenSampleIds.VendorId;
            public string DisplayName => "Golden Apothecary";
            public VendorOffer[] Offers => GoldenOffers;

            public bool TryGetOffer(string offerId, out VendorOffer offer)
            {
                if (offerId == RpgGoldenSampleIds.PotionOfferId)
                {
                    offer = GoldenOffers[0];
                    return true;
                }

                offer = default;
                return false;
            }
        }

        private sealed class GoldenSkillTree : ISkillTree
        {
            private readonly Dictionary<string, SkillNode> _nodes = new Dictionary<string, SkillNode>
            {
                {
                    RpgGoldenSampleIds.SkillRootNodeId,
                    new SkillNode(
                        RpgGoldenSampleIds.SkillRootNodeId,
                        1,
                        false,
                        Array.Empty<string>(),
                        new[] { new StatModifier("stat.wisdom", 2f, "skill.golden.wisdom") })
                }
            };

            public bool TryGetNode(string nodeId, out SkillNode node)
            {
                return _nodes.TryGetValue(nodeId, out node);
            }
        }

        private sealed class GoldenEquipmentSlot : IEquipmentSlot
        {
            public GoldenEquipmentSlot(string slotId)
            {
                SlotId = slotId;
            }

            public string SlotId { get; }
        }

        private sealed class GoldenEquippableItem : IEquippableItem
        {
            private readonly string _slotId;
            private readonly StatModifier[] _modifiers;

            public GoldenEquippableItem(string itemId, string slotId, StatModifier[] modifiers)
            {
                ItemId = itemId;
                _slotId = slotId;
                _modifiers = modifiers ?? Array.Empty<StatModifier>();
            }

            public string ItemId { get; }

            public bool CanEquipInSlot(string slotId)
            {
                return string.Equals(_slotId, slotId, StringComparison.Ordinal);
            }

            public StatModifier[] CreateStatModifiers(string sourceId)
            {
                StatModifier[] modifiers = new StatModifier[_modifiers.Length];
                for (int i = 0; i < _modifiers.Length; i++)
                    modifiers[i] = new StatModifier(_modifiers[i].StatId, _modifiers[i].Value, sourceId);

                return modifiers;
            }
        }
    }
}
