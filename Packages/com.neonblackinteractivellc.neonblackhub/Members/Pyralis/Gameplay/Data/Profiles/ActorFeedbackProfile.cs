using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.UI | AuthoringCapability.VFX, 
        Relevance = "Project-window creation path for actor feedback and route-readable reaction polish.",
        AssignmentFields = new[] { nameof(publishDamageEvents), nameof(publishDeathEvents), nameof(publishScoreEvents) },
        FirstProof = "Verify that damage events trigger floating text or HUD updates.",
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Actor Feedback Profile", fileName = "ActorFeedbackProfile")]
    public class ActorFeedbackProfile : ScriptableObject
    {
        public bool publishDamageEvents = true;
        public bool publishHealingEvents = true;
        public bool publishDeathEvents = true;
        public bool publishStatusEvents = true;
        public bool publishScoreEvents = true;
        public bool publishComboEvents = true;
    }
}
