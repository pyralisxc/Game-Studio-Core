using System.Collections.Generic;
using UnityEngine;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Pickups
{
    public partial class CollectibleFeedback2D
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (_collectClip == null)
                yield return PyralisRuntimeValidationIssue.Required("Collect Clip is unassigned.");
            if (_collectFX == null)
                yield return PyralisRuntimeValidationIssue.Required("Collect FX particle system is unassigned.");

            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null && audio.outputAudioMixerGroup == null)
                yield return PyralisRuntimeValidationIssue.Required("AudioSource is missing an Output Mixer Group. Volume settings will not apply.");
        }
    }
}
