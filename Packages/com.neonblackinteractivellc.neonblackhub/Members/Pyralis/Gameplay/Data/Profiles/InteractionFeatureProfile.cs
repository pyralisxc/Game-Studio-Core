using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Puzzle | AuthoringCapability.Session,
        Relevance = "Defines how an actor interacts with world objects.",
        NativeSetup = new[] { "Create Asset.", "Set Interaction Cooldown." },
        AssignmentFields = new[] { nameof(enableInteraction) },
        FirstProof = "Verify the actor can trigger interaction events on compatible world objects.",
        ExpertAdvice = "Use interactionCooldown to prevent rapid-fire interaction spamming.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/interaction"
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Interaction Feature Profile", fileName = "InteractionFeatureProfile")]
    public class InteractionFeatureProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
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
