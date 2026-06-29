using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Category = "Animation",
        CapabilityPath = "Presentation/Feedback/Pawn Animation Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Maps high-level gameplay signals to Unity Animator parameters for a specific character visual.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/animation",
        RequiredFields = new[] { nameof(animationDefinition), nameof(baseController), nameof(bindings) },
        SetupSteps = new[] { "Assign Animation Definition.", "Assign Base Controller.", "Map bindings." },
        SuccessChecks = new[] { "Verify the character animates correctly in play mode using the assigned controller." },
        Tags = new[] { "capability:Animation", "runtime:AnimationPresentation", "lane:Animation", "priority:AuxiliaryDefault" },
        Selectable = false
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Pawn Animation Profile", fileName = "PawnAnimationProfile", order = -30)]
    public class PawnAnimationProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
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
