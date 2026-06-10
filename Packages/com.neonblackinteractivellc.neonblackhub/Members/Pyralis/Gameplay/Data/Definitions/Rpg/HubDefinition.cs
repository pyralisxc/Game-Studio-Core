using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.hub.definition",
        Capability = AuthoringCapability.Session,
        Lane = "RPG",
        AssignmentFields = new[] { nameof(hubId), nameof(displayName), nameof(sceneId), nameof(interactables) },
        FirstProof = "Proof that the hub contains valid interactables and correctly links to a scene."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Hub Definition", fileName = "HubDefinition")]
    public class HubDefinition : ScriptableObject, IHubDefinition
{
        public string hubId = "hub.new";
        public string displayName = "New Hub";
        public string sceneId = "scene.hub";
        public string defaultReturnPointId = "spawn.default";
        public string[] tags = Array.Empty<string>();
        public HubInteractableDefinition[] interactables = Array.Empty<HubInteractableDefinition>();

        public string HubId => Normalize(hubId);
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? HubId : displayName.Trim();
        public string SceneId => Normalize(sceneId);
        public string DefaultReturnPointId => Normalize(defaultReturnPointId);
        public string[] Tags => tags ?? Array.Empty<string>();
        public HubInteractableDefinition[] InteractableDefinitions => interactables ?? Array.Empty<HubInteractableDefinition>();
        public HubInteractable[] Interactables => InteractableDefinitions.Select(interactable => interactable.CreateRuntimeInteractable()).ToArray();

        public void Sanitize()
        {
            hubId = HubId;
            displayName = DisplayName;
            sceneId = SceneId;
            defaultReturnPointId = DefaultReturnPointId;
            tags = Tags.Select(Normalize).Where(tag => !string.IsNullOrWhiteSpace(tag)).Distinct().ToArray();
            interactables = InteractableDefinitions;

            for (int i = 0; i < interactables.Length; i++)
                interactables[i].Sanitize();
        }

        public bool TryGetInteractable(string interactableId, out HubInteractable interactable)
        {
            string normalizedInteractableId = Normalize(interactableId);
            HubInteractableDefinition[] definitions = InteractableDefinitions;
            for (int i = 0; i < definitions.Length; i++)
            {
                if (definitions[i].InteractableId == normalizedInteractableId)
                {
                    interactable = definitions[i].CreateRuntimeInteractable();
                    return true;
                }
            }

            interactable = default;
            return false;
        }

        public HubDefinitionModel CreateRuntimeHub()
        {
            return new HubDefinitionModel(HubId, DisplayName, SceneId, DefaultReturnPointId, Tags, Interactables);
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();
            if (string.IsNullOrWhiteSpace(hubId))
                issues.Add("Hub id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (string.IsNullOrWhiteSpace(sceneId))
                issues.Add("Scene id is required.");

            HubInteractableDefinition[] definitions = InteractableDefinitions;
            if (definitions.Length == 0)
                issues.Add("At least one hub interactable is required.");

            HashSet<string> ids = new HashSet<string>();
            for (int i = 0; i < definitions.Length; i++)
                ValidateInteractable(definitions[i], i, ids, issues);

            return issues;
        }

        private static void ValidateInteractable(HubInteractableDefinition interactable, int index, HashSet<string> ids, List<string> issues)
        {
            string id = interactable.InteractableId;
            if (string.IsNullOrWhiteSpace(id))
            {
                issues.Add($"Hub Interactables[{index}] Interactable id is required.");
                return;
            }

            if (!ids.Add(id))
                issues.Add($"Hub interactable `{id}` is assigned more than once.");

            if (string.IsNullOrWhiteSpace(interactable.displayName))
                issues.Add($"Hub interactable `{id}` display name is required.");

            if (string.IsNullOrWhiteSpace(interactable.promptText))
                issues.Add($"Hub interactable `{id}` prompt text is required.");

            if ((interactable.kind == HubInteractionKind.Portal || interactable.kind == HubInteractionKind.MinigameEntrance)
                && string.IsNullOrWhiteSpace(interactable.SceneId))
            {
                issues.Add($"Hub interactable `{id}` Scene id is required for portal or minigame entrance interactions.");
            }

            if (interactable.kind == HubInteractionKind.NPCDialogue && string.IsNullOrWhiteSpace(interactable.DialogueGraphId))
                issues.Add($"Hub interactable `{id}` Dialogue graph id is required for NPC dialogue interactions.");

            ValidateConditions(id, interactable.Conditions, issues);
            ValidateEffects(id, interactable.Effects, issues);
        }

        private static void ValidateConditions(string interactableId, HubConditionDefinition[] conditions, List<string> issues)
        {
            for (int i = 0; i < conditions.Length; i++)
            {
                if (ConditionRequiresTarget(conditions[i].kind) && string.IsNullOrWhiteSpace(conditions[i].TargetId))
                    issues.Add($"Hub interactable `{interactableId}` Conditions[{i}] Target id is required.");

                if (conditions[i].RequiredQuantity < 1)
                    issues.Add($"Hub interactable `{interactableId}` Conditions[{i}] required quantity must be at least 1.");
            }
        }

        private static void ValidateEffects(string interactableId, HubEffectDefinition[] effects, List<string> issues)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (EffectRequiresTarget(effects[i].kind) && string.IsNullOrWhiteSpace(effects[i].TargetId))
                    issues.Add($"Hub interactable `{interactableId}` Effects[{i}] Target id is required.");

                if (effects[i].kind == HubEffectKind.OpenPanel && effects[i].panelRoute == PlayerPanelRoute.None)
                    issues.Add($"Hub interactable `{interactableId}` Effects[{i}] Panel route is required.");

                if (effects[i].Quantity < 1)
                    issues.Add($"Hub interactable `{interactableId}` Effects[{i}] quantity must be at least 1.");
            }
        }

        private void OnValidate()
        {
            Sanitize();
        }

        private static bool ConditionRequiresTarget(HubConditionKind kind)
        {
            return kind != HubConditionKind.Always;
        }

        private static bool EffectRequiresTarget(HubEffectKind kind)
        {
            return kind == HubEffectKind.StartDialogue
                || kind == HubEffectKind.StartQuest
                || kind == HubEffectKind.ReportQuestObjective
                || kind == HubEffectKind.GrantItem
                || kind == HubEffectKind.SetDialogueFlag
                || kind == HubEffectKind.NavigateScene
                || kind == HubEffectKind.CustomEvent;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
