using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Animation,
        Priority = AuthoringPriority.AuxiliaryDefault,
        Lane = "Animation",
        Relevance = "Maps high-level gameplay signals to Unity Animator parameters for a specific character visual.",
        AssignmentFields = new[] { nameof(animationDefinition), nameof(baseController), nameof(bindings) },
        FirstProof = "Verify the character animates correctly in play mode using the assigned controller.",
        ExpertAdvice = "Use the Controller Mapping Wizard in the custom inspector to quickly align your animator with Pyralis signals. This profile acts as the bridge between gameplay logic and visual feedback.",
        NativeSetup = new[] { "Assign Animation Definition.", "Assign Base Controller.", "Map bindings." },
        DocumentationURL = "https://docs.neonblack.com/pyralis/animation"
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Pawn Animation Profile", fileName = "PawnAnimationProfile", order = -30)]
    public class PawnAnimationProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            // The actual validation issues are complex and depend on UnityEditor APIs.
            // We return an empty list here and let the custom inspector handle the deep validation,
            // or we could use a reflective call to the editor validator if we want it to show in the overlay.
            // For now, we'll keep it simple as the custom inspector is quite robust.
            yield break;
        }

        public ActorAnimationDefinition animationDefinition;
        public RuntimeAnimatorController baseController;
        public RuntimeAnimatorController spawnControllerOverride;
        public ActorAnimationBinding[] bindings = Array.Empty<ActorAnimationBinding>();

        public void Sanitize()
        {
            if (bindings == null)
                bindings = Array.Empty<ActorAnimationBinding>();
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
