using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Hazard Feedback Profile", fileName = "HazardFeedbackProfile")]
    public class HazardFeedbackProfile : ScriptableObject
    {
        public bool flashOnActivation = true;
        public FlashPresetSO activationFlashPreset;
        public bool flashOnExplosion = true;
        public FlashPresetSO explosionFlashPreset;
        public bool flashOnBounce = false;
        public FlashPresetSO bounceFlashPreset;

        public bool showActivationPopup = false;
        public string activationPopupText = "Warning";
        public bool showExplosionPopup = true;
        public string explosionPopupText = "Boom";
        public bool showCollectiblePopup = false;
        public string collectiblePopupPrefix = "+";
        public bool showExitPopup = false;
        public string exitPopupText = "Clear";

        public float popupLifetime = 0.75f;
        public float popupRiseSpeed = 1.5f;
        public float popupFontSize = 3f;
        public Vector3 popupOffset = new Vector3(0f, 1.5f, 0f);
        public Color activationPopupColor = new Color(1f, 0.9f, 0.25f, 1f);
        public Color explosionPopupColor = new Color(1f, 0.35f, 0.35f, 1f);
        public Color collectiblePopupColor = new Color(0.4f, 1f, 0.75f, 1f);
        public Color exitPopupColor = new Color(0.8f, 0.95f, 1f, 1f);

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(activationPopupText))
                activationPopupText = "Warning";
            if (string.IsNullOrWhiteSpace(explosionPopupText))
                explosionPopupText = "Boom";
            if (string.IsNullOrWhiteSpace(collectiblePopupPrefix))
                collectiblePopupPrefix = "+";
            if (string.IsNullOrWhiteSpace(exitPopupText))
                exitPopupText = "Clear";

            popupLifetime = Mathf.Max(0.05f, popupLifetime);
            popupRiseSpeed = Mathf.Max(0f, popupRiseSpeed);
            popupFontSize = Mathf.Max(0.1f, popupFontSize);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
