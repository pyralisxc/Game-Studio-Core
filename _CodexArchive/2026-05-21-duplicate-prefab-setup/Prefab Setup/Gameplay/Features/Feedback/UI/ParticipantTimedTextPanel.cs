using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Feedback.UI
{
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

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (label == null)
                yield return "`ParticipantTimedTextPanel` should reference a TextMeshProUGUI label.";
        }
    }
}
