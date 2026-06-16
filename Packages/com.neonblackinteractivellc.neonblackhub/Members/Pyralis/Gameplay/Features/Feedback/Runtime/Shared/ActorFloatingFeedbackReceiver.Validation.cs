using System.Collections.Generic;

namespace NeonBlack.Gameplay.Features.Feedback
{
    public partial class ActorFloatingFeedbackReceiver
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (!showDamageNumbers
                && !showHealNumbers
                && !showScorePopups
                && !showComboPopups
                && !showStatusPopups
                && !showCombatAlertPopups)
            {
                yield return "`ActorFloatingFeedbackReceiver` is configured to hide every feedback category.";
            }
        }
    }
}
