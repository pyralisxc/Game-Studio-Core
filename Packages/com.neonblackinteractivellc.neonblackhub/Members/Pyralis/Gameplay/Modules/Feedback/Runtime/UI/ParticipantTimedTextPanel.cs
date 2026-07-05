using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using TMPro;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Feedback.UI
{
    [AuthoringContract(
        Category = "U I",
        CapabilityPath = "UI/HUD/Participant Timed Text Panel",
        Surface = AuthoringSurface.Goal,
        Summary = "Displays temporary text messages (e.g., 'Level Up', 'K.O.') on the HUD.",
        SetupSteps = new[] { "Attach to a UI panel inside a Canvas.", "Assign a TMP label when this panel should render messages." },
        SuccessChecks = new[] { "Call ShowText() from a script and verify the label appears on screen." },
        Tags = new[] { "capability:UI" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/UI/Participant Timed Text Panel")]
    public class ParticipantTimedTextPanel : MonoBehaviour, IRuntimeValidationProvider
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float defaultDisplayTime = 0.8f;

        private float _timer;

        private void Update()
        {
            if (label == null || _timer <= 0f)
                return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
                label.gameObject.SetActive(false);
        }

        public void ShowText(string text, float duration = -1f)
        {
            if (label == null || string.IsNullOrWhiteSpace(text))
                return;

            label.text = text;
            label.gameObject.SetActive(true);
            _timer = duration > 0f ? duration : defaultDisplayTime;
        }

        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (label == null)
                yield return RuntimeValidationIssue.Recommended("`ParticipantTimedTextPanel` has no TextMeshProUGUI label assigned, so it cannot render timed HUD text.");
        }
    }
}
