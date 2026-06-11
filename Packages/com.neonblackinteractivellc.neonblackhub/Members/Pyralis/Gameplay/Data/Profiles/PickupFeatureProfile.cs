using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Puzzle | AuthoringCapability.Inventory, 
        Relevance = "Tuning asset for the actor-level pickup collection feature.",
        AssignmentFields = new[] { nameof(enableAutoCollect), nameof(interactionRadius), nameof(collectibleLayers) },
        FirstProof = "Walk over a pickup and verify it is collected.",
        NativeSetup = new[] { "Create Asset.", "Assign to a Pawn or Interaction component." },
        ExpertAdvice = "Enable 'preferNearestPickup' for precise interaction in dense item clusters. Auto-collect is best for currency, while interaction-collect is better for loot crates.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/inventory"
    )]
[CreateAssetMenu(menuName = "NeonBlack/Profiles/Pickup Feature Profile", fileName = "PickupFeatureProfile")]
    public class PickupFeatureProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            yield break;
        }

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
