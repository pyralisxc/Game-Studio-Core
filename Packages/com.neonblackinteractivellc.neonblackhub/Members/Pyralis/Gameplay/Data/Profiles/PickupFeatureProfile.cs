using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Puzzle | AuthoringCapability.Inventory, 
        Relevance = "Project-window creation path for pickup feature setup.",
        AssignmentFields = new[] { nameof(enableAutoCollect), nameof(interactionRadius), nameof(collectibleLayers) },
        FirstProof = "Walk over a pickup and verify it is collected.",
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Pickup Feature Profile", fileName = "PickupFeatureProfile")]
    public class PickupFeatureProfile : ScriptableObject
    {
        public bool enableAutoCollect = true;
        public bool enableInteractionCollect = true;
        public float interactionRadius = 1f;
        public LayerMask collectibleLayers = Physics2D.AllLayers;
        public LayerMask collectibleLayers3D = Physics.DefaultRaycastLayers;
        public float overlapRadius3D = 1f;
        public bool preferNearestPickup = true;

        public void Sanitize()
        {
            interactionRadius = Mathf.Max(0f, interactionRadius);
            overlapRadius3D = Mathf.Max(0f, overlapRadius3D);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
