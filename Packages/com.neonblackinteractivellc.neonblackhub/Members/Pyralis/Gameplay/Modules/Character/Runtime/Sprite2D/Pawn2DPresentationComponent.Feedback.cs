using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    public sealed partial class Pawn2DPresentationComponent
    {
        public void PlayDashFeedback()
        {
            if (dashClip != null)
                audioSource.PlayOneShot(dashClip);
            animationDriver?.TriggerSignal(ActorAnimationSignal.Dash);
        }

        public void PlayDeathFeedback()
        {
            ResetTransientVisualState();
            animationDriver?.TriggerSignal(ActorAnimationSignal.Death);
            if (deathClip != null)
                audioSource.PlayOneShot(deathClip);
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            try { Handheld.Vibrate(); } catch { }
#endif
        }
    }
}
