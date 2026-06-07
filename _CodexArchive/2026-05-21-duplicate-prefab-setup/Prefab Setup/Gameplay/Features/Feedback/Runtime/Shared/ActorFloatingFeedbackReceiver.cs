using System.Collections.Generic;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Feedback
{
    [AddComponentMenu("NeonBlack/Gameplay/Feedback/Actor Floating Feedback Receiver")]
    public class ActorFloatingFeedbackReceiver : MonoBehaviour, IActorFeedbackReceiver, IRuntimeValidationProvider
    {
        private sealed class FloatingPopup
        {
            public GameObject Root;
            public TextMeshPro Label;
            public float Timer;
            public float Lifetime;
            public Vector3 Velocity;
            public Color BaseColor;
        }

        [Header("Damage And Healing")]
        [SerializeField] private bool showDamageNumbers = true;
        [SerializeField] private bool showHealNumbers = true;
        [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Popup Events")]
        [SerializeField] private bool showScorePopups = true;
        [SerializeField] private bool showComboPopups = true;
        [SerializeField] private bool showStatusPopups = true;
        [SerializeField] private bool showCombatAlertPopups = true;
        [SerializeField] private Vector3 popupOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private float popupLifetime = 0.75f;
        [SerializeField] private float popupRiseSpeed = 1.5f;
        [SerializeField] private float popupScatter = 0.2f;
        [SerializeField] private float popupFontSize = 3f;
        [SerializeField] private Color scoreColor = new Color(1f, 0.92f, 0.25f, 1f);
        [SerializeField] private Color comboColor = new Color(1f, 0.45f, 0.15f, 1f);
        [SerializeField] private Color statusColor = new Color(0.45f, 0.95f, 1f, 1f);
        [SerializeField] private Color combatAlertColor = new Color(1f, 0.3f, 0.55f, 1f);

        private readonly Queue<FloatingPopup> _pool = new Queue<FloatingPopup>();
        private readonly List<FloatingPopup> _active = new List<FloatingPopup>(8);
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

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

        public void HandleFeedbackEvent(ActorFeedbackEvent feedbackEvent)
        {
            switch (feedbackEvent.EventType)
            {
                case ActorFeedbackEventType.Damage:
                    if (showDamageNumbers)
                        Combat.DamageNumberSpawner.Instance?.Spawn(feedbackEvent.FloatValue, transform.position + damageNumberOffset);
                    break;

                case ActorFeedbackEventType.Heal:
                    if (showHealNumbers)
                        Combat.DamageNumberSpawner.Instance?.SpawnHeal(feedbackEvent.FloatValue, transform.position + damageNumberOffset);
                    break;

                case ActorFeedbackEventType.Score:
                    if (showScorePopups)
                        SpawnPopup($"+{feedbackEvent.IntValue}", scoreColor);
                    break;

                case ActorFeedbackEventType.Combo:
                    if (showComboPopups)
                        SpawnPopup($"Combo {feedbackEvent.IntValue}", comboColor);
                    break;

                case ActorFeedbackEventType.StatusApplied:
                    if (showStatusPopups && feedbackEvent.StatusEffect != null)
                        SpawnPopup(feedbackEvent.StatusEffect.effectId, statusColor);
                    break;

                case ActorFeedbackEventType.Parry:
                    if (showCombatAlertPopups)
                        SpawnPopup("Parry", combatAlertColor);
                    break;

                case ActorFeedbackEventType.Stagger:
                    if (showCombatAlertPopups)
                        SpawnPopup("Stagger", combatAlertColor);
                    break;

                case ActorFeedbackEventType.GuardBreak:
                    if (showCombatAlertPopups)
                        SpawnPopup("Guard Break", combatAlertColor);
                    break;

                case ActorFeedbackEventType.Finisher:
                    if (showCombatAlertPopups)
                        SpawnPopup($"Finisher {feedbackEvent.IntValue}", combatAlertColor);
                    break;
            }
        }

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
