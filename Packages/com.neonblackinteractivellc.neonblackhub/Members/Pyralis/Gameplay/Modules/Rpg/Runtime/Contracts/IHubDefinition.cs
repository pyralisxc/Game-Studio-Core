namespace NeonBlack.Gameplay.Modules.Rpg.Runtime
{
    public interface IHubDefinition
    {
        string HubId { get; }
        string DisplayName { get; }
        string SceneId { get; }
        string DefaultReturnPointId { get; }
        string[] Tags { get; }
        HubInteractable[] Interactables { get; }
        bool TryGetInteractable(string interactableId, out HubInteractable interactable);
    }
}
