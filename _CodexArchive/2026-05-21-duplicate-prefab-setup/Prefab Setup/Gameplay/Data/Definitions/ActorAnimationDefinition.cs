using System;
using System.Linq;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Definitions/Actor Animation Definition", fileName = "ActorAnimationDefinition")]
    public class ActorAnimationDefinition : ScriptableObject
    {
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
                ActorPresentationMode.Rigged3D => supportsRigged3D,
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
