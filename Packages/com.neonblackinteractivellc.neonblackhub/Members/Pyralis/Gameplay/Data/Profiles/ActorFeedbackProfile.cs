using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
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
