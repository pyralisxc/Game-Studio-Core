using System.Collections.Generic;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Feedback
{
    public partial class ActorFloatingFeedbackReceiver
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (!showDamageNumbers
                && !showHealNumbers
                && !showScorePopups
                && !showComboPopups
                && !showStatusPopups
                && !showCombatAlertPopups)
            {
                yield return PyralisRuntimeValidationIssue.Required("`ActorFloatingFeedbackReceiver` is configured to hide every feedback category.");
            }
        }
    }
}
