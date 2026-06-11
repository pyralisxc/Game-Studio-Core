using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.VFX | AuthoringCapability.UI,
        Relevance = "Defines the visual feedback (flashes, popups) for hazard activation and explosion.",
        NativeSetup = new[] { "Create Asset.", "Assign Flash presets.", "Configure popup text and colors." },
        AssignmentFields = new[] { nameof(activationFlashPreset), nameof(explosionFlashPreset) },
        FirstProof = "Trigger a hazard and verify the flashes and popups match the profile.",
        ExpertAdvice = "Use popupFontSize to ensure warnings are visible at the game's camera distance.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/visuals"
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Hazard Feedback Profile", fileName = "HazardFeedbackProfile")]
    public class HazardFeedbackProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (flashOnActivation && activationFlashPreset == null) yield return "Activation Flash is enabled but Preset is missing.";
            if (flashOnExplosion && explosionFlashPreset == null) yield return "Explosion Flash is enabled but Preset is missing.";
        }

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
