using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Pickups
{
    public partial class CollectibleFeedback2D
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (_collectClip == null)
                yield return "Collect Clip is unassigned.";
            if (_collectFX == null)
                yield return "Collect FX particle system is unassigned.";

            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null && audio.outputAudioMixerGroup == null)
                yield return "AudioSource is missing an Output Mixer Group. Volume settings will not apply.";
        }
    }
}
