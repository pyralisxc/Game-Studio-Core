using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    [AuthoringContract(
        Capability = AuthoringCapability.Animation,
        Relevance = "Defines the animation signal contract supported by an actor setup.",
        NativeSetup = new[] { "Create Asset.", "Set supported presentation modes.", "Optionally list supported signals." },
        AssignmentFields = new[] { nameof(supportsSprite2D), nameof(supportsBillboard2_5D), nameof(supportsRigged3D) },
        FirstProof = "Verify animation signals trigger correctly in the prefab's Animator.",
        ExpertAdvice = "Leave Supported Signals empty to accept all standard signals. Use specific signals only if the animator is restricted."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Actor Animation Definition", fileName = "ActorAnimationDefinition", order = 70)]
    public class ActorAnimationDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (!supportsSprite2D && !supportsBillboard2_5D && !supportsRigged3D)
                yield return "At least one presentation mode should be supported.";
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
