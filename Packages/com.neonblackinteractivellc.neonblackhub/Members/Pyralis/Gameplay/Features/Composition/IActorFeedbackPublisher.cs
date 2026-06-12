using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Composition
{
    [AuthoringContract(
        ModuleId = "actor.feedback",
        Capability = AuthoringCapability.VFX,
        Relevance = "Publishes feedback events (damage, heal, death, status) for visual and audio presentation.",
        ExpertAdvice = "Publish signals from logic (Combat, Pickups) to trigger visual/audio feedback on the actor.",
        ProfileType = typeof(ActorFeedbackProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorFeedbackPublisher) },
        SupportedLanes = new[]
        {
            ActorPresentationMode.Sprite2D,
            ActorPresentationMode.Billboard2_5D,
            ActorPresentationMode.ThirdPerson3D
        },
        NativeSetup = new[]
        {
            "create ActorFeedbackProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with ActorFeedbackFeatureRuntime",
            "assign profile asset",
            "add module to PawnDefinition.featureModules or enemy actor module list",
            "add at least one IActorFeedbackReceiver in the actor hierarchy"
        },
        FirstProof = "Trigger a damage event and verify visual feedback (flash, popup) occurs.",
        FirstProofTargetId = "proof.ui-hud-menu",
        DocumentationURL = "https://docs.neonblack.com/pyralis/actor-feedback",
        AssignmentFields = new[]
        {
            "FeatureModuleDefinition.moduleId",
            "FeatureModuleDefinition.runtimePrefab",
            "FeatureModuleDefinition.profileAsset",
            "FeatureModuleDefinition.supportedPresentationModes",
            "PawnDefinition.featureModules"
        },
        CustomizationMoments = new[]
        {
            "ActorFeedbackProfile.publishDamageEvents",
            "ActorFeedbackProfile.publishHealingEvents",
            "ActorFeedbackProfile.publishDeathEvents",
            "ActorFeedbackProfile.publishStatusEvents",
            "IActorFeedbackReceiver.HandleFeedbackEvent"
        }
    )]
    public interface IActorFeedbackPublisher
    {
        void PublishDamage(float amount, GameObject source = null);
        void PublishHeal(float amount, GameObject source = null);
        void PublishDeath();
        void PublishStatusApplied(StatusEffectDefinition effectDefinition, GameObject source = null);
        void PublishScore(int amount);
        void PublishCombo(int comboStep);
        void PublishParry();
        void PublishStagger(float intensity = 0f);
        void PublishGuardBreak();
        void PublishFinisher(int comboStep);
    }
}
