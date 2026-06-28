using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Category = "Puzzle, Session",
        CapabilityPath = "Interaction/Profiles/Interaction Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Defines how an actor interacts with world objects.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/interaction",
        RequiredFields = new[] { nameof(enableInteraction) },
        SetupSteps = new[] { "Set Interaction Cooldown." },
        SuccessChecks = new[] { "Verify the actor can trigger interaction events on compatible world objects." },
        Tags = new[] { "capability:Puzzle", "capability:Session", "runtime:CharacterPawnGameplay" },
        Selectable = false
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Interaction Profile", fileName = "InteractionProfile")]
    public class InteractionProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            yield break;
        }

        public bool enableInteraction = true;
        public float interactionCooldown = 0.1f;
        public bool triggerInteractAnimationWhenUnhandled = true;

        public void Sanitize()
        {
            interactionCooldown = Mathf.Max(0f, interactionCooldown);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
