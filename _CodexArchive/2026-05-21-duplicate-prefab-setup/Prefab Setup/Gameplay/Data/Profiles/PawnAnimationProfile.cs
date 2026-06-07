using System;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Pawn Animation Profile", fileName = "PawnAnimationProfile")]
    public class PawnAnimationProfile : ScriptableObject
    {
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
