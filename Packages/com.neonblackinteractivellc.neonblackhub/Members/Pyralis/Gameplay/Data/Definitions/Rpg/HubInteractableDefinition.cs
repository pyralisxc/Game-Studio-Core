using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [Serializable]
    public struct HubInteractableDefinition
    {
        public string interactableId;
        public string displayName;
        public string promptText;
        public string lockedPromptText;
        public string iconId;
        public HubInteractionKind kind;
        public HubInteractionAvailability availability;
        public PlayerPanelRoute panelRoute;
        public string sceneId;
        public string dialogueGraphId;
        public string npcId;
        public HubConditionDefinition[] conditions;
        public HubEffectDefinition[] effects;
        public int priority;
        public string notificationText;

        public string InteractableId => Normalize(interactableId);
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? InteractableId : displayName.Trim();
        public string PromptText => string.IsNullOrWhiteSpace(promptText) ? DisplayName : promptText.Trim();
        public string LockedPromptText => string.IsNullOrWhiteSpace(lockedPromptText) ? PromptText : lockedPromptText.Trim();
        public string IconId => Normalize(iconId);
        public string SceneId => Normalize(sceneId);
        public string DialogueGraphId => Normalize(dialogueGraphId);
        public string NpcId => Normalize(npcId);
        public HubConditionDefinition[] Conditions => conditions ?? Array.Empty<HubConditionDefinition>();
        public HubEffectDefinition[] Effects => effects ?? Array.Empty<HubEffectDefinition>();

        public void Sanitize()
        {
            interactableId = InteractableId;
            displayName = DisplayName;
            promptText = PromptText;
            lockedPromptText = LockedPromptText;
            iconId = IconId;
            sceneId = SceneId;
            dialogueGraphId = DialogueGraphId;
            npcId = NpcId;
            notificationText ??= string.Empty;
            conditions = Conditions;
            effects = Effects;

            for (int i = 0; i < conditions.Length; i++)
                conditions[i].Sanitize();

            for (int i = 0; i < effects.Length; i++)
                effects[i].Sanitize();
        }

        public HubInteractable CreateRuntimeInteractable()
        {
            return new HubInteractable(
                InteractableId,
                DisplayName,
                PromptText,
                LockedPromptText,
                IconId,
                kind,
                availability,
                panelRoute,
                SceneId,
                DialogueGraphId,
                NpcId,
                Conditions.Select(condition => condition.CreateRuntimeCondition()).ToArray(),
                Effects.Select(effect => effect.CreateRuntimeEffect()).ToArray(),
                priority,
                notificationText);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
