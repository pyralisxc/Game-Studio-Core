using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Visuals;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
    [AddComponentMenu("NeonBlack/Gameplay/Hazards/Hazard Feedback Runtime")]
    public class HazardFeedbackRuntime : MonoBehaviour, IRuntimeValidationProvider
    {
        private sealed class Popup
        {
            public GameObject Root;
            public TextMeshPro Label;
            public float Timer;
            public float Lifetime;
            public Vector3 Velocity;
            public Color BaseColor;
        }

        [SerializeField] private SpriteFlasher spriteFlasher;
        [SerializeField] private bool autoFindSpriteFlasher = true;

        private readonly Queue<Popup> _pool = new Queue<Popup>();
        private readonly List<Popup> _active = new List<Popup>(4);
        private HazardFeedbackProfile _profile;
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
            if (autoFindSpriteFlasher && spriteFlasher == null)
                spriteFlasher = GetComponent<SpriteFlasher>() ?? GetComponentInChildren<SpriteFlasher>(true);
        }

        private void LateUpdate()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Popup popup = _active[i];
                if (popup == null || popup.Root == null)
                {
                    _active.RemoveAt(i);
                    continue;
                }

                popup.Timer -= Time.deltaTime;
                popup.Root.transform.position += popup.Velocity * Time.deltaTime;
                if (_camera != null)
                    popup.Root.transform.rotation = _camera.transform.rotation;

                float fade = Mathf.Clamp01(popup.Timer / Mathf.Max(popup.Lifetime, 0.001f));
                Color color = popup.BaseColor;
                color.a *= fade;
                popup.Label.color = color;

                if (popup.Timer > 0f)
                    continue;

                popup.Root.SetActive(false);
                _pool.Enqueue(popup);
                _active.RemoveAt(i);
            }
        }

        public void ApplyProfile(HazardFeedbackProfile profile)
        {
            _profile = profile;
            _profile?.Sanitize();
        }

        public void PlayActivationFeedback()
        {
            if (_profile == null)
                return;

            if (_profile.flashOnActivation && _profile.activationFlashPreset != null)
                spriteFlasher?.PlayOneShot(_profile.activationFlashPreset);
            if (_profile.showActivationPopup)
                SpawnPopup(_profile.activationPopupText, _profile.activationPopupColor);
        }

        public void PlayExplosionFeedback()
        {
            if (_profile == null)
                return;

            if (_profile.flashOnExplosion && _profile.explosionFlashPreset != null)
                spriteFlasher?.PlayOneShot(_profile.explosionFlashPreset);
            if (_profile.showExplosionPopup)
                SpawnPopup(_profile.explosionPopupText, _profile.explosionPopupColor);
        }

        public void PlayBounceFeedback()
        {
            if (_profile == null)
                return;

            if (_profile.flashOnBounce && _profile.bounceFlashPreset != null)
                spriteFlasher?.PlayOneShot(_profile.bounceFlashPreset);
        }

        public void PlayCollectibleFeedback(int amount)
        {
            if (_profile == null || !_profile.showCollectiblePopup || amount <= 0)
                return;

            SpawnPopup($"{_profile.collectiblePopupPrefix}{amount}", _profile.collectiblePopupColor);
        }

        public void PlayExitFeedback()
        {
            if (_profile == null || !_profile.showExitPopup)
                return;

            SpawnPopup(_profile.exitPopupText, _profile.exitPopupColor);
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (_profile == null)
                yield break;

            if ((_profile.flashOnActivation && _profile.activationFlashPreset != null
                || _profile.flashOnExplosion && _profile.explosionFlashPreset != null
                || _profile.flashOnBounce && _profile.bounceFlashPreset != null)
                && spriteFlasher == null)
            {
                yield return "`HazardFeedbackRuntime` needs a SpriteFlasher when flash presets are authored in the HazardFeedbackProfile.";
            }
        }

        private void SpawnPopup(string text, Color color)
        {
            if (_profile == null || string.IsNullOrWhiteSpace(text))
                return;

            Popup popup = _pool.Count > 0 ? _pool.Dequeue() : CreatePopup();
            popup.Timer = _profile.popupLifetime;
            popup.Lifetime = _profile.popupLifetime;
            popup.BaseColor = color;
            popup.Velocity = Vector3.up * _profile.popupRiseSpeed;
            popup.Root.transform.position = transform.position + _profile.popupOffset;
            if (_camera != null)
                popup.Root.transform.rotation = _camera.transform.rotation;
            popup.Label.text = text;
            popup.Label.fontSize = _profile.popupFontSize;
            popup.Label.color = color;
            popup.Root.SetActive(true);
            _active.Add(popup);
        }

        private Popup CreatePopup()
        {
            GameObject root = new GameObject("HazardFeedbackPopup");
            root.transform.SetParent(transform, false);
            TextMeshPro label = root.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = _profile != null ? _profile.popupFontSize : 3f;
            label.fontStyle = FontStyles.Bold;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            root.SetActive(false);

            return new Popup
            {
                Root = root,
                Label = label,
                Lifetime = _profile != null ? _profile.popupLifetime : 0.75f,
                Velocity = Vector3.up,
                BaseColor = Color.white
            };
        }
    }
}
