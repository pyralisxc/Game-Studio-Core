using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Pickups;
using NeonBlack.Gameplay.Features.Scoring;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(Collectible2D))]
    public sealed class Collectible2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Collectible 2D",
                new PyralisGuideSection(
                    "What This Is",
                    "Collectible2D is the pooled 2D pickup object used by CollectibleSpawner2D and actor pickup collectors. It awards score through an IPickupAwardSink, then returns to the pool or disables itself.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pickups_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Use this on the collectible prefab assigned to CollectibleSpawner2D.",
                        "Keep the CircleCollider2D enabled and set as a trigger.",
                        "Make sure the collectible layer is included by PickupFeatureProfile collectible layers.",
                        "Use CollectibleFeedback2D or another IPickupAwardSink when pickups should award score or play feedback."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not put gameplay score logic directly on the collectible when a feedback or award sink should own scoring.",
                        "Do not make spawn immunity so long that hazards cannot clear newly spawned pickups.",
                        "Do not use this 2D collectible with the 3D pickup collector."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(PickupScoringRuntimeEditorUtility.GetCollectible2DMessages(serializedObject, (Collectible2D)target), "Collectible2D is ready for 2D pickup collection.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(Collectible3D))]
    public sealed class Collectible3DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Collectible 3D",
                new PyralisGuideSection(
                    "What This Is",
                    "Collectible3D is the 3D pickup object collected by ActorPickupCollectorFeature3D. It awards score through an IPickupAwardSink and disables itself after collection.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pickups_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on a 3D pickup prefab with an enabled Collider.",
                        "Make sure the collectible layer is included by PickupFeatureProfile collectible layers 3D.",
                        "Assign Award Sink Source only when this pickup should route awards to a specific sink.",
                        "Use CollectibleFeedback2D, a parent IPickupAwardSink, or a gameplay service when collection should update score or feedback."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not use a 2D collider on this 3D collectible.",
                        "Do not leave the collectible on a layer excluded by the collector profile.",
                        "Do not rely on pooling behavior here unless a 3D spawner or custom pool reactivates it."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(PickupScoringRuntimeEditorUtility.GetCollectible3DMessages(serializedObject, (Collectible3D)target), "Collectible3D is ready for 3D pickup collection.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(CollectibleFeedback2D))]
    public sealed class CollectibleFeedback2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Collectible Feedback 2D",
                new PyralisGuideSection(
                    "What This Is",
                    "CollectibleFeedback2D is the default pickup award sink. It adds score through an explicit score award service and plays collect or destroy audio and particles.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pickups_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place one feedback component in scenes that use Collectible2D or Collectible3D scoring.",
                        "Assign Score Award Source to ParticipantScoreService or another ISessionScoreAwardSink when pickups should add points.",
                        "Assign collect and destroy clips or effects only for feedback that should play.",
                        "Add an AudioSource with the SFX AudioMixer output group when sounds are assigned."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not rely on a scene singleton; route collectibles to this component through Award Sink Source, parent lookup, or gameplay service registration.",
                        "Do not leave the AudioSource output group empty when settings should control SFX volume.",
                        "Do not expect score to change without Score Award Source or a custom award sink."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(PickupScoringRuntimeEditorUtility.GetCollectibleFeedbackMessages(serializedObject, (CollectibleFeedback2D)target), "CollectibleFeedback2D is ready for pickup awards.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ActorPickupCollectorFeature2D))]
    public sealed class ActorPickupCollectorFeature2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PickupScoringRuntimeEditorUtility.DrawCollectorGuide(
                "Inspector Field Guide: Actor Pickup Collector Feature 2D",
                "ActorPickupCollectorFeature2D installs the 2D pickup collection behavior for an actor feature runtime.",
                PyralisAuthoringContractGuideText.FeatureModuleSetup((ActorPickupCollectorFeature2D)target),
                "The actor root needs a Collider2D for auto-collect overlap checks. Interaction collection also needs an interaction input bridge.");

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(PickupScoringRuntimeEditorUtility.GetCollectorMessages(serializedObject, "PickupFeatureProfile"), "ActorPickupCollectorFeature2D is ready for feature-module pickup collection.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ActorPickupCollectorFeature3D))]
    public sealed class ActorPickupCollectorFeature3DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PickupScoringRuntimeEditorUtility.DrawCollectorGuide(
                "Inspector Field Guide: Actor Pickup Collector Feature 3D",
                "ActorPickupCollectorFeature3D installs the 3D pickup collection behavior for an actor feature runtime.",
                PyralisAuthoringContractGuideText.FeatureModuleSetup((ActorPickupCollectorFeature3D)target),
                "The actor root needs a Transform position and profile overlap radius/layers that match the 3D collectible prefabs.");

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(PickupScoringRuntimeEditorUtility.GetCollectorMessages(serializedObject, "PickupFeatureProfile"), "ActorPickupCollectorFeature3D is ready for feature-module pickup collection.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(StillnessBonus2D))]
    public sealed class StillnessBonus2DEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawFieldGuide(
                "Inspector Field Guide: Stillness Bonus 2D",
                new PyralisGuideSection(
                    "What This Is",
                    "StillnessBonus2D awards score when a 2D pawn stays below the movement threshold for a configured interval.",
                    manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Scoring_Setup.md")),
                new PyralisGuideSection(
                    "Required Fields",
                    null,
                    new[]
                    {
                        "Place this on the 2D player actor with Motor2D.",
                        "Assign Gameplay State Source directly, or let GameManager configure tracked players at runtime.",
                        "Assign Score Award Source to ParticipantScoreService or another ISessionScoreAwardSink.",
                        "Assign an AudioSource with an SFX mixer output group if Bonus Clip is assigned.",
                        "Tune threshold and interval against the real movement profile so idle drift does not reset rewards."
                    }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not use this on 3D or non-Motor2D actors.",
                        "Do not set the stillness threshold so high that normal slow movement earns rewards.",
                        "Do not expect the timer to run outside active gameplay state."
                    }));

            DrawDefaultInspector();
            PyralisInspectorGuide.DrawValidationMessages(PickupScoringRuntimeEditorUtility.GetStillnessBonusMessages(serializedObject, (StillnessBonus2D)target), "StillnessBonus2D is ready for 2D stillness scoring.");
            serializedObject.ApplyModifiedProperties();
        }
    }

    internal static class PickupScoringRuntimeEditorUtility
    {
        public static void DrawCollectorGuide(string title, string whatThisIs, string moduleSetup, string actorSetup)
        {
            PyralisInspectorGuide.DrawFieldGuide(
                title,
                new PyralisGuideSection(
                    "What This Is",
                    whatThisIs,
                    manualPath: PyralisInspectorGuide.SetupManualPath("RUNTIME_PATTERN_COOKBOOK.md")),
                new PyralisGuideSection(
                    "Feature Module Fields",
                    null,
                    new[]
                    {
                        moduleSetup,
                        "Set Runtime Prefab to a prefab containing this collector component.",
                        "Set Profile Asset to a PickupFeatureProfile with collectible layers and collection modes configured.",
                        "Use Supported Presentation Modes to keep 2D and 3D pickup collectors on the right actors."
                    }),
                new PyralisGuideSection(
                    "Actor Fields",
                    null,
                    new[] { actorSetup }),
                new PyralisGuideSection(
                    "Watch For",
                    null,
                    new[]
                    {
                        "Do not place this runtime component directly on a scene actor unless a custom bootstrap initializes it.",
                        "Do not leave all pickup collection modes disabled in the profile.",
                        "Do not forget a pickup feedback or score route if collection should affect player-facing score."
                    }));
        }

        public static List<PyralisGuideIssue> GetCollectible2DMessages(SerializedObject serializedObject, Collectible2D collectible)
        {
            List<PyralisGuideIssue> messages = GetAwardSinkMessages(serializedObject, "_awardSinkSource");
            CircleCollider2D collider = collectible != null ? collectible.GetComponent<CircleCollider2D>() : null;

            if (collider == null)
                messages.Add(PyralisGuideIssue.Required("CircleCollider2D is required for 2D pickup overlap collection."));
            else if (!collider.isTrigger)
                messages.Add(PyralisGuideIssue.Required("CircleCollider2D should be set to Is Trigger for collectible overlap detection."));

            SerializedProperty immunity = serializedObject.FindProperty("_spawnImmunityDuration");
            if (immunity != null && immunity.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required("Spawn Immunity Duration cannot be negative."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetCollectible3DMessages(SerializedObject serializedObject, Collectible3D collectible)
        {
            List<PyralisGuideIssue> messages = GetAwardSinkMessages(serializedObject, "awardSinkSource");
            Collider collider = collectible != null ? collectible.GetComponent<Collider>() : null;

            if (collider == null)
                messages.Add(PyralisGuideIssue.Required("A 3D Collider is required for ActorPickupCollectorFeature3D overlap checks."));
            else if (!collider.enabled)
                messages.Add(PyralisGuideIssue.Required("The collectible Collider is disabled, so pickup collectors cannot find it."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetCollectibleFeedbackMessages(SerializedObject serializedObject, CollectibleFeedback2D feedback)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            bool hasSound =
                serializedObject.FindProperty("_collectClip")?.objectReferenceValue != null
                || serializedObject.FindProperty("_destroyClip")?.objectReferenceValue != null;

            AudioSource audioSource = feedback != null ? feedback.GetComponent<AudioSource>() : null;
            if (hasSound && audioSource == null)
                messages.Add(PyralisGuideIssue.Recommended("Add an AudioSource with the SFX mixer output group before assigning pickup sounds."));
            else if (hasSound && audioSource.outputAudioMixerGroup == null)
                messages.Add(PyralisGuideIssue.Required("AudioSource Output AudioMixerGroup is empty, so settings cannot control pickup SFX volume."));

            SerializedProperty baseVolume = serializedObject.FindProperty("_baseVolume");
            if (baseVolume != null && (baseVolume.floatValue < 0f || baseVolume.floatValue > 1f))
                messages.Add(PyralisGuideIssue.Required("Base Volume should stay between 0 and 1."));

            SerializedProperty scoreAward = serializedObject.FindProperty("_scoreAwardSource");
            if (scoreAward != null && scoreAward.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Score Award Source is empty. Collection feedback will still play, but collected pickups will not add score unless a service is injected."));
            else if (scoreAward != null
                && scoreAward.objectReferenceValue is Component scoreComponent
                && scoreComponent.GetComponent<ISessionScoreAwardSink>() == null)
                messages.Add(PyralisGuideIssue.Required("Score Award Source must reference a component that implements ISessionScoreAwardSink."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetCollectorMessages(SerializedObject serializedObject, string expectedProfileName)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty profile = serializedObject.FindProperty("pickupProfile");

            if (profile != null && profile.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional(profile.displayName + " is empty. This is expected when FeatureModuleDefinition provides the " + expectedProfileName + " at runtime."));

            return messages;
        }

        public static List<PyralisGuideIssue> GetStillnessBonusMessages(SerializedObject serializedObject, StillnessBonus2D bonus)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();

            if (bonus != null && bonus.GetComponent("Motor2D") == null)
                messages.Add(PyralisGuideIssue.Required("Motor2D is required because stillness is measured from the 2D motor velocity."));

            SerializedProperty reward = serializedObject.FindProperty("_collectiblesPerBonus");
            SerializedProperty interval = serializedObject.FindProperty("_stillnessInterval");
            SerializedProperty threshold = serializedObject.FindProperty("_stillnessThreshold");
            SerializedProperty bonusClip = serializedObject.FindProperty("_bonusClip");
            SerializedProperty gameplayState = serializedObject.FindProperty("_gameplayStateSource");
            SerializedProperty scoreAward = serializedObject.FindProperty("_scoreAwardSource");

            if (reward != null && reward.intValue < 1)
                messages.Add(PyralisGuideIssue.Required("Collectibles Per Bonus must be at least 1."));

            if (interval != null && interval.floatValue <= 0f)
                messages.Add(PyralisGuideIssue.Required("Stillness Interval must be greater than zero."));

            if (threshold != null && threshold.floatValue < 0f)
                messages.Add(PyralisGuideIssue.Required("Stillness Threshold cannot be negative."));

            if (gameplayState != null && gameplayState.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Gameplay State Source is empty. GameManager can configure tracked players at runtime; otherwise assign an IGameplayStateReader."));
            else if (gameplayState != null
                && gameplayState.objectReferenceValue is Component gameplayComponent
                && gameplayComponent.GetComponent<IGameplayStateReader>() == null)
                messages.Add(PyralisGuideIssue.Required("Gameplay State Source must reference a component that implements IGameplayStateReader."));

            if (scoreAward != null && scoreAward.objectReferenceValue == null)
                messages.Add(PyralisGuideIssue.Optional("Score Award Source is empty. GameManager can configure tracked players at runtime; otherwise assign an ISessionScoreAwardSink."));
            else if (scoreAward != null
                && scoreAward.objectReferenceValue is Component scoreComponent
                && scoreComponent.GetComponent<ISessionScoreAwardSink>() == null)
                messages.Add(PyralisGuideIssue.Required("Score Award Source must reference a component that implements ISessionScoreAwardSink."));

            AudioSource audioSource = bonus != null ? bonus.GetComponent<AudioSource>() : null;
            if (bonusClip != null && bonusClip.objectReferenceValue != null && audioSource == null)
                messages.Add(PyralisGuideIssue.Recommended("Add an AudioSource with the SFX mixer output group before assigning the bonus sound."));
            else if (bonusClip != null && bonusClip.objectReferenceValue != null && audioSource.outputAudioMixerGroup == null)
                messages.Add(PyralisGuideIssue.Required("AudioSource Output AudioMixerGroup is empty, so settings cannot control stillness bonus SFX volume."));

            return messages;
        }

        private static List<PyralisGuideIssue> GetAwardSinkMessages(SerializedObject serializedObject, string fieldName)
        {
            List<PyralisGuideIssue> messages = new List<PyralisGuideIssue>();
            SerializedProperty awardSink = serializedObject.FindProperty(fieldName);

            if (awardSink != null
                && awardSink.objectReferenceValue != null
                && awardSink.objectReferenceValue is not IPickupAwardSink)
            {
                messages.Add(PyralisGuideIssue.Required(awardSink.displayName + " must reference a component that implements IPickupAwardSink."));
            }

            return messages;
        }
    }
}
