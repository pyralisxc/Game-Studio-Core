using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.UI | AuthoringCapability.VFX, 
        Relevance = "Configures which gameplay events (damage, death, score) trigger visual feedback or HUD notifications.",
        NativeSetup = new[] { "Create Asset.", "Toggle desired event publications." },
        AssignmentFields = new[] { nameof(publishDamageEvents), nameof(publishDeathEvents), nameof(publishScoreEvents) },
        FirstProof = "Verify that damage events trigger floating text or HUD updates.",
        ExpertAdvice = "Use these toggles to silence feedback for specific actor archetypes (e.g., destructible props vs. bosses).",
        DocumentationURL = "https://docs.neonblack.com/pyralis/visuals"
    )]
[CreateAssetMenu(menuName = "NeonBlack/Profiles/Actor Feedback Profile", fileName = "ActorFeedbackProfile")]
    public class ActorFeedbackProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (!publishDamageEvents && !publishHealingEvents && !publishDeathEvents && !publishStatusEvents && !publishScoreEvents)
                yield return "All feedback events are disabled. This profile will produce no output.";
        }

        public bool publishDamageEvents = true;
        public bool publishHealingEvents = true;
        public bool publishDeathEvents = true;
        public bool publishStatusEvents = true;
        public bool publishScoreEvents = true;
        public bool publishComboEvents = true;
    }
}
