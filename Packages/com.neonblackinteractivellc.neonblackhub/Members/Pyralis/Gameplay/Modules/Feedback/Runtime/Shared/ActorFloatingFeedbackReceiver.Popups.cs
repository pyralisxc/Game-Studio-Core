using TMPro;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Feedback
{
    public partial class ActorFloatingFeedbackReceiver
    {
        private void LateUpdate()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                FloatingPopup popup = _active[i];
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

        private void SpawnPopup(string text, Color color)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            FloatingPopup popup = _pool.Count > 0 ? _pool.Dequeue() : CreatePopup();
            popup.Timer = popupLifetime;
            popup.Lifetime = popupLifetime;
            popup.BaseColor = color;
            popup.Velocity = new Vector3(Random.Range(-popupScatter, popupScatter), popupRiseSpeed, 0f);
            popup.Root.transform.position = transform.position + popupOffset + new Vector3(Random.Range(-popupScatter, popupScatter), 0f, 0f);
            if (_camera != null)
                popup.Root.transform.rotation = _camera.transform.rotation;
            popup.Label.text = text;
            popup.Label.fontSize = popupFontSize;
            popup.Label.color = color;
            popup.Root.SetActive(true);
            _active.Add(popup);
        }

        private FloatingPopup CreatePopup()
        {
            GameObject root = new GameObject("ActorFeedbackPopup");
            root.transform.SetParent(transform, false);
            TextMeshPro label = root.AddComponent<TextMeshPro>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = popupFontSize;
            label.fontStyle = FontStyles.Bold;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            root.SetActive(false);

            return new FloatingPopup
            {
                Root = root,
                Label = label,
                Lifetime = popupLifetime,
                Velocity = Vector3.up * popupRiseSpeed,
                BaseColor = Color.white
            };
        }
    }
}
