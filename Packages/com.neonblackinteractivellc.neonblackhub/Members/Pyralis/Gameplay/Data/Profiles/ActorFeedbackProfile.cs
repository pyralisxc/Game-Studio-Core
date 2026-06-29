using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Category = "U I, V F X",
        CapabilityPath = "Presentation/Feedback/Actor Feedback Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Configures which gameplay events (damage, death, score) trigger visual feedback or HUD notifications.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/visuals",
        RequiredFields = new[] { nameof(publishDamageEvents), nameof(publishDeathEvents), nameof(publishScoreEvents) },
        SetupSteps = new[] { "Toggle desired event publications." },
        SuccessChecks = new[] { "Verify that damage events trigger floating text or HUD updates." },
        Tags = new[] { "capability:UI", "capability:VFX", "runtime:AnimationPresentation" },
        Selectable = false
    )]
[CreateAssetMenu(menuName = "NeonBlack/Profiles/Actor Feedback Profile", fileName = "ActorFeedbackProfile")]
    public class ActorFeedbackProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (!publishDamageEvents && !publishHealingEvents && !publishDeathEvents && !publishStatusEvents && !publishScoreEvents)
                yield return RuntimeValidationIssue.Required("All feedback events are disabled. This profile will produce no output.");
        }

        public bool publishDamageEvents = true;
        public bool publishHealingEvents = true;
        public bool publishDeathEvents = true;
        public bool publishStatusEvents = true;
        public bool publishScoreEvents = true;
        public bool publishComboEvents = true;
    }
}
