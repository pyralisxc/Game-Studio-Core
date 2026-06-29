using System.Collections.Generic;

using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Modules.Feedback
{
    public partial class ActorFloatingFeedbackReceiver
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (!showDamageNumbers
                && !showHealNumbers
                && !showScorePopups
                && !showComboPopups
                && !showStatusPopups
                && !showCombatAlertPopups)
            {
                yield return RuntimeValidationIssue.Required("`ActorFloatingFeedbackReceiver` is configured to hide every feedback category.");
            }
        }
    }
}
