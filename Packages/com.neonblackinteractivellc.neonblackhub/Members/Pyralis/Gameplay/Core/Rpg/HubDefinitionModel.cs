using System;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public readonly struct HubDefinitionModel : IHubDefinition
    {
        public HubDefinitionModel(string hubId, string displayName, string sceneId, string defaultReturnPointId, string[] tags, HubInteractable[] interactables)
        {
            HubId = Normalize(hubId);
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? HubId : displayName.Trim();
            SceneId = Normalize(sceneId);
            DefaultReturnPointId = Normalize(defaultReturnPointId);
            Tags = tags ?? Array.Empty<string>();
            Interactables = interactables ?? Array.Empty<HubInteractable>();
        }

        public string HubId { get; }
        public string DisplayName { get; }
        public string SceneId { get; }
        public string DefaultReturnPointId { get; }
        public string[] Tags { get; }
        public HubInteractable[] Interactables { get; }

        public bool TryGetInteractable(string interactableId, out HubInteractable interactable)
        {
            string normalizedId = Normalize(interactableId);
            for (int i = 0; i < Interactables.Length; i++)
            {
                if (Interactables[i].InteractableId == normalizedId)
                {
                    interactable = Interactables[i];
                    return true;
                }
            }

            interactable = default;
            return false;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
