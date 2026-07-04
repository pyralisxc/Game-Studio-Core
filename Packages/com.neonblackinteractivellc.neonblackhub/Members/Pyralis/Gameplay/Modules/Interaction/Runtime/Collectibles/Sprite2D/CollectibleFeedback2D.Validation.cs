using System.Collections.Generic;
using UnityEngine;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Interaction
{
    public partial class CollectibleFeedback2D
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (_collectClip == null)
                yield return RuntimeValidationIssue.Required("Collect Clip is unassigned.");
            if (_collectFX == null)
                yield return RuntimeValidationIssue.Required("Collect FX particle system is unassigned.");

            AudioSource audio = GetComponent<AudioSource>();
            if (audio == null)
                yield return RuntimeValidationIssue.Required("AudioSource component is missing.");
            else if (audio.outputAudioMixerGroup == null)
                yield return RuntimeValidationIssue.Required("AudioSource is missing an Output Mixer Group. Volume settings will not apply.");
        }
    }
}
