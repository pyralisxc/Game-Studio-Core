using NeonBlack.Gameplay.Core.Types.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    public sealed partial class Pawn2DPresentationComponent
    {
        public void PlayDashFeedback()
        {
            PlayPresentationClip(dashClip);
            animationDriver?.TriggerSignal(ActorAnimationSignal.Dash);
        }

        public void PlayDeathFeedback()
        {
            ResetTransientVisualState();
            animationDriver?.TriggerSignal(ActorAnimationSignal.Death);
            PlayPresentationClip(deathClip);
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            try { Handheld.Vibrate(); } catch { }
#endif
        }

        private void PlayPresentationClip(AudioClip clip)
        {
            if (clip == null || audioSource == null)
                return;

            audioSource.PlayOneShot(clip);
        }
    }
}
