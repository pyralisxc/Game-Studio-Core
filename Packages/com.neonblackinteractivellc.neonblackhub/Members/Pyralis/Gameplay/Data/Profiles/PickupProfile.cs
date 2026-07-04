using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Category = "Puzzle, Inventory",
        CapabilityPath = "RPG/Inventory/Profiles/Pickup Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Tuning asset for the actor-level pickup collection feature.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/inventory",
        SetupSteps = new[] { "Assign to a Pawn or Interaction component." },
        SuccessChecks = new[] { "Walk over a pickup and verify it is collected." },
        Tags = new[] { "capability:Puzzle", "capability:Inventory", "runtime:CharacterPawnGameplay" },
        Selectable = false
    )]
[CreateAssetMenu(menuName = "NeonBlack/Profiles/Pickup Profile", fileName = "PickupProfile")]
    public class PickupProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
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
