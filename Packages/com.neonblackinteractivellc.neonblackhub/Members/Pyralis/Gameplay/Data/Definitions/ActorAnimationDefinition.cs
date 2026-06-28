using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions
{
    [AuthoringContract(
        Category = "Animation",
        CapabilityPath = "Animation/Definitions/Actor Animation Definition",
        Surface = AuthoringSurface.Goal,
        Summary = "Defines the animation signal contract supported by an actor setup.",
        RequiredFields = new[] { nameof(supportsSprite2D), nameof(supportsBillboard2_5D), nameof(supportsRigged3D) },
        SetupSteps = new[] { "Set supported presentation modes.", "Optionally list supported signals." },
        SuccessChecks = new[] { "Verify animation signals trigger correctly in the prefab's Animator." },
        RoleTags = new[] { "IntentRouteEssential", "AnimationDefinitionRouteSupport" },
        Tags = new[] { "capability:Animation", "runtime:AnimationPresentation" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Actor Animation Definition", fileName = "ActorAnimationDefinition", order = 70)]
    public class ActorAnimationDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (!supportsSprite2D && !supportsBillboard2_5D && !supportsRigged3D)
                yield return PyralisRuntimeValidationIssue.Required("At least one presentation mode should be supported.");
        }

        public string displayName = "Gameplay Actor Animation";
        public bool supportsSprite2D = true;
        public bool supportsBillboard2_5D = true;
        public bool supportsRigged3D = true;
        public ActorAnimationSignal[] supportedSignals = Array.Empty<ActorAnimationSignal>();

        [TextArea(2, 5)]
        public string notes = string.Empty;

        public bool SupportsSignal(ActorAnimationSignal signal)
        {
            if (supportedSignals == null || supportedSignals.Length == 0)
                return true;

            return supportedSignals.Contains(signal);
        }

        public bool SupportsPresentationMode(ActorPresentationMode mode)
        {
            return mode switch
            {
                ActorPresentationMode.Sprite2D => supportsSprite2D,
                ActorPresentationMode.Billboard2_5D => supportsBillboard2_5D,
                ActorPresentationMode.ThirdPerson3D => supportsRigged3D,
                _ => true
            };
        }

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = "Gameplay Actor Animation";

            if (supportedSignals == null)
                supportedSignals = Array.Empty<ActorAnimationSignal>();

            supportedSignals = supportedSignals.Distinct().ToArray();
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
