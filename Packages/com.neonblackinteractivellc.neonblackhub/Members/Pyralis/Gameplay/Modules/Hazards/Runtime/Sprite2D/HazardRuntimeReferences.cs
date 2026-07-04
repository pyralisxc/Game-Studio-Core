using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Hazards
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

            references.AudioSource = ResolveOptionalAudioSource(owner);
            references.FeedbackRuntime = owner.GetComponent<HazardFeedbackRuntime>()
                ?? owner.GetComponentInChildren<HazardFeedbackRuntime>(true);
            references.CameraShakeSink = ResolveCameraShakeSink(cameraShakeSink);
            references.Settings = ResolveSettings(settingsSource);
            references.HasRootRigidbody2D = owner.GetComponent<Rigidbody2D>() != null;
            return references;
        }

        public static IActorHazardImpactTarget ResolveHazardImpactTarget(Collider2D collider)
        {
            if (collider == null)
                return null;

            IActorHazardImpactTarget target = collider.GetComponent<IActorHazardImpactTarget>();
            return target ?? collider.GetComponentInParent<IActorHazardImpactTarget>();
        }

        public static bool IsActorTarget(Collider2D collider)
        {
            return collider != null
                && (collider.GetComponentInParent<IActorHazardImpactTarget>() != null
                    || collider.GetComponentInParent<IActorHealthState>() != null);
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

        private static AudioSource ResolveOptionalAudioSource(GameObject owner)
        {
            AudioSource audioSource = owner.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.spatialBlend = 0f;
                audioSource.playOnAwake = false;
            }

            return audioSource;
        }
    }
}
