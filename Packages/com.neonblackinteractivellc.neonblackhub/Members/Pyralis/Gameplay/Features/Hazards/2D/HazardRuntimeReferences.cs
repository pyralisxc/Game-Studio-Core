using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Hazards
{
    internal sealed class HazardRuntimeReferences
    {
        public AudioSource AudioSource { get; private set; }
        public HazardFeedbackRuntime FeedbackRuntime { get; private set; }
        public ICameraShakeSink CameraShakeSink { get; private set; }
        public IGameplaySettingsApplier Settings { get; private set; }
        public bool HasRootRigidbody2D { get; private set; }

        public static HazardRuntimeReferences Resolve(
            GameObject owner,
            MonoBehaviour cameraShakeSink,
            MonoBehaviour settingsSource)
        {
            HazardRuntimeReferences references = new HazardRuntimeReferences();
            if (owner == null)
                return references;

            references.AudioSource = ResolveAudioSource(owner);
            references.FeedbackRuntime = owner.GetComponent<HazardFeedbackRuntime>()
                ?? owner.GetComponentInChildren<HazardFeedbackRuntime>(true);
            references.CameraShakeSink = ResolveCameraShakeSink(cameraShakeSink);
            references.Settings = ResolveSettings(settingsSource);
            references.HasRootRigidbody2D = owner.GetComponent<Rigidbody2D>() != null;
            return references;
        }

        public static Motor2D ResolveTargetMotor(Collider2D collider)
        {
            if (collider == null)
                return null;

            Motor2D motor = collider.GetComponent<Motor2D>();
            return motor != null ? motor : collider.GetComponentInParent<Motor2D>();
        }

        public static bool IsActorTarget(Collider2D collider)
        {
            return collider != null
                && (collider.GetComponentInParent<Motor2D>() != null
                    || collider.GetComponentInParent<HealthComponent>() != null);
        }

        public static bool IsHazardTarget(Collider2D collider)
        {
            return collider != null && collider.TryGetComponent<Hazard>(out _);
        }

        public static ICameraShakeSink ResolveCameraShakeSink(MonoBehaviour cameraShakeSink)
        {
            return cameraShakeSink as ICameraShakeSink;
        }

        public static IGameplaySettingsApplier ResolveSettings(MonoBehaviour settingsSource)
        {
            if (settingsSource == null)
                return null;

            return settingsSource as IGameplaySettingsApplier
                ?? settingsSource.GetComponent<IGameplaySettingsApplier>();
        }

        private static AudioSource ResolveAudioSource(GameObject owner)
        {
            AudioSource audioSource = owner.GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = owner.AddComponent<AudioSource>();

            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
            return audioSource;
        }
    }
}
