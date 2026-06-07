using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class RpgHubDefinitionTests
    {
        [Test]
        public void HubDefinition_GetValidationIssues_RequiresHubId()
        {
            HubDefinition hub = CreateHub();
            hub.hubId = string.Empty;

            Assert.That(hub.GetValidationIssues().Any(issue => issue.Contains("Hub id")), Is.True);

            Object.DestroyImmediate(hub);
        }

        [Test]
        public void HubDefinition_GetValidationIssues_FlagsDuplicateInteractableIds()
        {
            HubDefinition hub = CreateHub(
                CreateInteractable("interactable.same", HubInteractionKind.QuestBoard),
                CreateInteractable("interactable.same", HubInteractionKind.QuestBoard));

            Assert.That(hub.GetValidationIssues().Any(issue => issue.Contains("assigned more than once")), Is.True);

            Object.DestroyImmediate(hub);
        }

        [Test]
        public void HubDefinition_GetValidationIssues_FlagsPortalMissingSceneId()
        {
            HubInteractableDefinition interactable = CreateInteractable("interactable.portal", HubInteractionKind.Portal);
            interactable.sceneId = string.Empty;
            HubDefinition hub = CreateHub(interactable);

            Assert.That(hub.GetValidationIssues().Any(issue => issue.Contains("Scene id")), Is.True);

            Object.DestroyImmediate(hub);
        }

        [Test]
        public void HubDefinition_GetValidationIssues_FlagsNpcDialogueMissingDialogueGraph()
        {
            HubInteractableDefinition interactable = CreateInteractable("interactable.elder", HubInteractionKind.NPCDialogue);
            interactable.dialogueGraphId = string.Empty;
            HubDefinition hub = CreateHub(interactable);

            Assert.That(hub.GetValidationIssues().Any(issue => issue.Contains("Dialogue graph id")), Is.True);

            Object.DestroyImmediate(hub);
        }

        [Test]
        public void HubDefinition_GetValidationIssues_FlagsPanelEffectMissingRoute()
        {
            HubInteractableDefinition interactable = CreateInteractable("interactable.panel", HubInteractionKind.Custom);
            interactable.effects = new[] { new HubEffectDefinition { kind = HubEffectKind.OpenPanel, panelRoute = PlayerPanelRoute.None } };
            HubDefinition hub = CreateHub(interactable);

            Assert.That(hub.GetValidationIssues().Any(issue => issue.Contains("Panel route")), Is.True);

            Object.DestroyImmediate(hub);
        }

        [Test]
        public void HubDefinition_GetValidationIssues_FlagsRequiredConditionAndEffectTargets()
        {
            HubInteractableDefinition interactable = CreateInteractable("interactable.locked", HubInteractionKind.Trainer);
            interactable.conditions = new[] { new HubConditionDefinition { kind = HubConditionKind.ItemCount, targetId = string.Empty, requiredQuantity = 1, expected = true } };
            interactable.effects = new[] { new HubEffectDefinition { kind = HubEffectKind.GrantItem, targetId = string.Empty, quantity = 1 } };
            HubDefinition hub = CreateHub(interactable);

            Assert.That(hub.GetValidationIssues().Any(issue => issue.Contains("Condition") && issue.Contains("Target id")), Is.True);
            Assert.That(hub.GetValidationIssues().Any(issue => issue.Contains("Effect") && issue.Contains("Target id")), Is.True);

            Object.DestroyImmediate(hub);
        }

        private static HubDefinition CreateHub(params HubInteractableDefinition[] interactables)
        {
            HubDefinition hub = ScriptableObject.CreateInstance<HubDefinition>();
            hub.hubId = "hub.test";
            hub.displayName = "Test Hub";
            hub.sceneId = "scene.hub";
            hub.defaultReturnPointId = "spawn.default";
            hub.interactables = interactables.Length > 0
                ? interactables
                : new[] { CreateInteractable("interactable.quest-board", HubInteractionKind.QuestBoard) };
            return hub;
        }

        private static HubInteractableDefinition CreateInteractable(string id, HubInteractionKind kind)
        {
            return new HubInteractableDefinition
            {
                interactableId = id,
                displayName = "Test Interactable",
                promptText = "Use",
                lockedPromptText = "Locked",
                iconId = "icon.test",
                kind = kind,
                availability = HubInteractionAvailability.Available,
                panelRoute = kind == HubInteractionKind.NPCDialogue ? PlayerPanelRoute.Dialogue : PlayerPanelRoute.QuestBoard,
                sceneId = kind == HubInteractionKind.Portal || kind == HubInteractionKind.MinigameEntrance ? "scene.target" : string.Empty,
                dialogueGraphId = kind == HubInteractionKind.NPCDialogue ? "dialogue.elder" : string.Empty,
                npcId = kind == HubInteractionKind.NPCDialogue ? "npc.elder" : string.Empty
            };
        }
    }
}
