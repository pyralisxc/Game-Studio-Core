using System.Collections.Generic;
using UnityEngine;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Interaction
{
    public partial class CollectibleFeedback2D
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            bool hasAudioFeedback = _collectClip != null || _destroyClip != null;
            if (!hasAudioFeedback)
                yield break;

            AudioSource audio = GetComponent<AudioSource>();
            if (audio == null)
                yield return RuntimeValidationIssue.Required("AudioSource component is missing for assigned collectible audio feedback.");
            else if (audio.outputAudioMixerGroup == null)
                yield return RuntimeValidationIssue.Recommended("AudioSource is missing an Output Mixer Group. Volume settings will not apply to assigned collectible audio feedback.");
        }
    }
}
