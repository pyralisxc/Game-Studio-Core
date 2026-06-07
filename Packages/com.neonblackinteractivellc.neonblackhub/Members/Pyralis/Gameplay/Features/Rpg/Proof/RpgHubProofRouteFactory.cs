using System;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Features.Rpg.Proof
{
    public static class RpgHubProofRouteFactory
    {
        public static RpgOwnerKey CreateDefaultOwner()
        {
            return new RpgOwnerKey(RpgOwnerKind.Participant, RpgHubProofRouteIds.OwnerStableId);
        }

        public static HubDefinitionModel CreateHub()
        {
            return new HubDefinitionModel(
                RpgHubProofRouteIds.HubId,
                "RPG Proof Hub",
                RpgHubProofRouteIds.HubSceneId,
                "spawn.default",
                new[] { "proof", "rpg", "hub" },
                new[]
                {
                    new HubInteractable(
                        RpgHubProofRouteIds.DialogueInteractableId,
                        "Village Elder",
                        "Talk to the Elder",
                        "The elder is not ready.",
                        "dialogue",
                        HubInteractionKind.NPCDialogue,
                        HubInteractionAvailability.Available,
                        PlayerPanelRoute.Dialogue,
                        string.Empty,
                        RpgHubProofRouteIds.DialogueGraphId,
                        RpgHubProofRouteIds.NpcId,
                        Array.Empty<HubInteractionCondition>(),
                        Array.Empty<HubInteractionEffect>(),
                        10,
                        "Dialogue route opened."),
                    new HubInteractable(
                        RpgHubProofRouteIds.QuestBoardInteractableId,
                        "Quest Board",
                        "Open Quest Board",
                        "No quests are posted.",
                        "quest",
                        HubInteractionKind.QuestBoard,
                        HubInteractionAvailability.Available,
                        PlayerPanelRoute.QuestBoard,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        Array.Empty<HubInteractionCondition>(),
                        Array.Empty<HubInteractionEffect>(),
                        20,
                        "Quest board route opened."),
                    new HubInteractable(
                        RpgHubProofRouteIds.VendorInteractableId,
                        "Apothecary",
                        "Open Apothecary",
                        "The shop is closed.",
                        "vendor",
                        HubInteractionKind.Vendor,
                        HubInteractionAvailability.Available,
                        PlayerPanelRoute.Vendor,
                        string.Empty,
                        string.Empty,
                        RpgHubProofRouteIds.VendorId,
                        Array.Empty<HubInteractionCondition>(),
                        Array.Empty<HubInteractionEffect>(),
                        30,
                        "Vendor route opened."),
                    new HubInteractable(
                        RpgHubProofRouteIds.LoadoutInteractableId,
                        "Loadout Station",
                        "Open Loadout",
                        "The station is offline.",
                        "loadout",
                        HubInteractionKind.LoadoutStation,
                        HubInteractionAvailability.Available,
                        PlayerPanelRoute.Loadout,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        Array.Empty<HubInteractionCondition>(),
                        Array.Empty<HubInteractionEffect>(),
                        40,
                        "Loadout route opened."),
                    new HubInteractable(
                        RpgHubProofRouteIds.TrainerInteractableId,
                        "Hero Trainer",
                        "Train Skills",
                        "Training is locked.",
                        "skill",
                        HubInteractionKind.Trainer,
                        HubInteractionAvailability.Available,
                        PlayerPanelRoute.Trainer,
                        string.Empty,
                        string.Empty,
                        RpgHubProofRouteIds.SkillTreeId,
                        Array.Empty<HubInteractionCondition>(),
                        Array.Empty<HubInteractionEffect>(),
                        50,
                        "Trainer route opened."),
                    new HubInteractable(
                        RpgHubProofRouteIds.PortalInteractableId,
                        "Arena Portal",
                        "Enter Arena",
                        "The arena is sealed.",
                        "portal",
                        HubInteractionKind.Portal,
                        HubInteractionAvailability.Available,
                        PlayerPanelRoute.None,
                        RpgHubProofRouteIds.ArenaSceneId,
                        string.Empty,
                        string.Empty,
                        Array.Empty<HubInteractionCondition>(),
                        Array.Empty<HubInteractionEffect>(),
                        60,
                        "Arena scene requested.")
                });
        }

        public static DialogueGraph CreateDialogueGraph()
        {
            return new DialogueGraph(
                RpgHubProofRouteIds.DialogueGraphId,
                RpgHubProofRouteIds.DialogueStartNodeId,
                new[]
                {
                    new DialogueNode(
                        RpgHubProofRouteIds.DialogueStartNodeId,
                        DialogueNodeKind.ChoiceHub,
                        RpgHubProofRouteIds.NpcId,
                        "Welcome to the proof hub. Try the board, shop, loadout, trainer, and arena portal.",
                        new[]
                        {
                            new DialogueChoice(
                                RpgHubProofRouteIds.DialogueAcceptChoiceId,
                                "I am ready.",
                                RpgHubProofRouteIds.DialogueEndNodeId,
                                Array.Empty<DialogueCondition>(),
                                Array.Empty<DialogueEffect>())
                        },
                        Array.Empty<DialogueEffect>(),
                        string.Empty),
                    new DialogueNode(
                        RpgHubProofRouteIds.DialogueEndNodeId,
                        DialogueNodeKind.Terminal,
                        RpgHubProofRouteIds.NpcId,
                        string.Empty,
                        Array.Empty<DialogueChoice>(),
                        Array.Empty<DialogueEffect>(),
                        string.Empty)
                });
        }

        public static NpcProfile CreateNpc()
        {
            return new NpcProfile(RpgHubProofRouteIds.NpcId, "Village Elder", "proof-guide", new[] { "proof" }, "faction.proof", string.Empty);
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
                new QuestObjective(RpgHubProofRouteIds.QuestObjectiveId, QuestObjectiveKind.CollectItem, RpgHubProofRouteIds.HerbItemId, 3)
            };

            private static readonly QuestReward[] ProofRewards =
            {
                new QuestReward(10, 1, Array.Empty<QuestItemReward>())
            };

            public string QuestId => RpgHubProofRouteIds.QuestId;
            public bool Repeatable => false;
            public QuestObjective[] Objectives => ProofObjectives;
            public QuestReward[] Rewards => ProofRewards;

            public bool TryGetObjective(string objectiveId, out QuestObjective objective)
            {
                if (objectiveId == RpgHubProofRouteIds.QuestObjectiveId)
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
                new VendorOffer(
                    RpgHubProofRouteIds.VendorOfferId,
                    "Potion",
                    RpgHubProofRouteIds.PotionItemId,
                    RpgHubProofRouteIds.GoldItemId,
                    3,
                    1,
                    true,
                    true)
            };

            public string VendorId => RpgHubProofRouteIds.VendorId;
            public string DisplayName => "Apothecary";
            public VendorOffer[] Offers => ProofOffers;

            public bool TryGetOffer(string offerId, out VendorOffer offer)
            {
                if (offerId == RpgHubProofRouteIds.VendorOfferId)
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
