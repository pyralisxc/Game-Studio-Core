using System;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public static class RpgHubProofRouteTestFixture
    {
        public static class Ids
        {
            public const string OwnerStableId = "seat-1";
            public const string HubId = "hub.rpg-proof";
            public const string HubSceneId = "scene.rpg-proof-hub";
            public const string ArenaSceneId = "scene.rpg-proof-arena";
            public const string DialogueInteractableId = "proof.talk.elder";
            public const string QuestBoardInteractableId = "proof.quest.board";
            public const string VendorInteractableId = "proof.vendor.apothecary";
            public const string LoadoutInteractableId = "proof.loadout.station";
            public const string TrainerInteractableId = "proof.trainer.hero";
            public const string PortalInteractableId = "proof.portal.arena";
            public const string DialogueGraphId = "dialogue.rpg-proof.elder";
            public const string DialogueStartNodeId = "node.start";
            public const string DialogueEndNodeId = "node.end";
            public const string DialogueAcceptChoiceId = "choice.accept";
            public const string NpcId = "npc.rpg-proof.elder";
            public const string QuestId = "quest.rpg-proof.herbs";
            public const string QuestObjectiveId = "objective.collect.herb";
            public const string HerbItemId = "item.rpg-proof.herb";
            public const string PotionItemId = "item.rpg-proof.potion";
            public const string GoldItemId = "item.rpg-proof.gold";
            public const string VendorId = "vendor.rpg-proof.apothecary";
            public const string VendorOfferId = "offer.rpg-proof.potion";
            public const string WeaponSlotId = "slot.rpg-proof.weapon";
            public const string SwordItemId = "item.rpg-proof.sword";
            public const string SkillTreeId = "tree.rpg-proof.hero";
            public const string SkillRootNodeId = "skill.rpg-proof.root";
        }

        public static RpgOwnerKey CreateDefaultOwner()
        {
            return new RpgOwnerKey(RpgOwnerKind.Participant, Ids.OwnerStableId);
        }

        public static HubDefinitionModel CreateHub()
        {
            return new HubDefinitionModel(
                Ids.HubId,
                "RPG Proof Hub",
                Ids.HubSceneId,
                "spawn.default",
                new[] { "proof", "rpg", "hub" },
                new[]
                {
                    new HubInteractable(Ids.DialogueInteractableId, "Village Elder", "Talk to the Elder", "The elder is not ready.", "dialogue", HubInteractionKind.NPCDialogue, HubInteractionAvailability.Available, PlayerPanelRoute.Dialogue, string.Empty, Ids.DialogueGraphId, Ids.NpcId, Array.Empty<HubInteractionCondition>(), Array.Empty<HubInteractionEffect>(), 10, "Dialogue route opened."),
                    new HubInteractable(Ids.QuestBoardInteractableId, "Quest Board", "Open Quest Board", "No quests are posted.", "quest", HubInteractionKind.QuestBoard, HubInteractionAvailability.Available, PlayerPanelRoute.QuestBoard, string.Empty, string.Empty, string.Empty, Array.Empty<HubInteractionCondition>(), Array.Empty<HubInteractionEffect>(), 20, "Quest board route opened."),
                    new HubInteractable(Ids.VendorInteractableId, "Apothecary", "Open Apothecary", "The shop is closed.", "vendor", HubInteractionKind.Vendor, HubInteractionAvailability.Available, PlayerPanelRoute.Vendor, string.Empty, string.Empty, Ids.VendorId, Array.Empty<HubInteractionCondition>(), Array.Empty<HubInteractionEffect>(), 30, "Vendor route opened."),
                    new HubInteractable(Ids.LoadoutInteractableId, "Loadout Station", "Open Loadout", "The station is offline.", "loadout", HubInteractionKind.LoadoutStation, HubInteractionAvailability.Available, PlayerPanelRoute.Loadout, string.Empty, string.Empty, string.Empty, Array.Empty<HubInteractionCondition>(), Array.Empty<HubInteractionEffect>(), 40, "Loadout route opened."),
                    new HubInteractable(Ids.TrainerInteractableId, "Hero Trainer", "Train Skills", "Training is locked.", "skill", HubInteractionKind.Trainer, HubInteractionAvailability.Available, PlayerPanelRoute.Trainer, string.Empty, string.Empty, Ids.SkillTreeId, Array.Empty<HubInteractionCondition>(), Array.Empty<HubInteractionEffect>(), 50, "Trainer route opened."),
                    new HubInteractable(Ids.PortalInteractableId, "Arena Portal", "Enter Arena", "The arena is sealed.", "portal", HubInteractionKind.Portal, HubInteractionAvailability.Available, PlayerPanelRoute.None, Ids.ArenaSceneId, string.Empty, string.Empty, Array.Empty<HubInteractionCondition>(), Array.Empty<HubInteractionEffect>(), 60, "Arena scene requested.")
                });
        }

        public static DialogueGraph CreateDialogueGraph()
        {
            return new DialogueGraph(
                Ids.DialogueGraphId,
                Ids.DialogueStartNodeId,
                new[]
                {
                    new DialogueNode(
                        Ids.DialogueStartNodeId,
                        DialogueNodeKind.ChoiceHub,
                        Ids.NpcId,
                        "Welcome to the proof hub. Try the board, shop, loadout, trainer, and arena portal.",
                        new[] { new DialogueChoice(Ids.DialogueAcceptChoiceId, "I am ready.", Ids.DialogueEndNodeId, Array.Empty<DialogueCondition>(), Array.Empty<DialogueEffect>()) },
                        Array.Empty<DialogueEffect>(),
                        string.Empty),
                    new DialogueNode(
                        Ids.DialogueEndNodeId,
                        DialogueNodeKind.Terminal,
                        Ids.NpcId,
                        string.Empty,
                        Array.Empty<DialogueChoice>(),
                        Array.Empty<DialogueEffect>(),
                        string.Empty)
                });
        }

        public static NpcProfile CreateNpc()
        {
            return new NpcProfile(Ids.NpcId, "Village Elder", "proof-guide", new[] { "proof" }, "faction.proof", string.Empty);
        }

        public static IQuestDefinition CreateQuest()
        {
            return new ProofQuestDefinition();
        }

        public static IVendorDefinition CreateVendor()
        {
            return new ProofVendorDefinition();
        }

        private sealed class ProofQuestDefinition : IQuestDefinition
        {
            private static readonly QuestObjective[] ProofObjectives =
            {
                new QuestObjective(Ids.QuestObjectiveId, QuestObjectiveKind.CollectItem, Ids.HerbItemId, 3)
            };

            private static readonly QuestReward[] ProofRewards =
            {
                new QuestReward(10, 1, Array.Empty<QuestItemReward>())
            };

            public string QuestId => Ids.QuestId;
            public bool Repeatable => false;
            public QuestObjective[] Objectives => ProofObjectives;
            public QuestReward[] Rewards => ProofRewards;

            public bool TryGetObjective(string objectiveId, out QuestObjective objective)
            {
                if (objectiveId == Ids.QuestObjectiveId)
                {
                    objective = ProofObjectives[0];
                    return true;
                }

                objective = default;
                return false;
            }
        }

        private sealed class ProofVendorDefinition : IVendorDefinition
        {
            private static readonly VendorOffer[] ProofOffers =
            {
                new VendorOffer(Ids.VendorOfferId, "Potion", Ids.PotionItemId, Ids.GoldItemId, 3, 1, true, true)
            };

            public string VendorId => Ids.VendorId;
            public string DisplayName => "Apothecary";
            public VendorOffer[] Offers => ProofOffers;

            public bool TryGetOffer(string offerId, out VendorOffer offer)
            {
                if (offerId == Ids.VendorOfferId)
                {
                    offer = ProofOffers[0];
                    return true;
                }

                offer = default;
                return false;
            }
        }
    }
}
