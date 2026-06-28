using System;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Types.Animation;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Stats,
        ModuleId = "actor.status",
        Relevance = "Handles the application and management of combat status effects and stat modifiers on actors.",
        ExpertAdvice = "Receives timed effects like Burn or Slow. Pair with ActorStatusEffectComponent for implementation.",
        ProfileType = typeof(ActorStatusEffectProfile),
        RequiredInterfaces = new Type[]
        {
            typeof(IActorStatusEffectReceiver),
            typeof(IDamageModifier)
        },
        SupportedLanes = new[]
        {
            ActorPresentationMode.Sprite2D,
            ActorPresentationMode.Billboard2_5D,
            ActorPresentationMode.ThirdPerson3D
        },
        NativeSetup = new[]
        {
            "add ActorStatusEffectComponent to the actor root",
            "assign ActorStatusEffectProfile",
            "ensure HealthComponent and status modifier receivers are present on actors that consume status effects"
        },
        Proof = "Apply a status effect to an actor and verify it appears in the active effect list.",
        ProofTargetId = "proof.custom-object-effect",
        DocumentationURL = "https://docs.neonblack.com/pyralis/actor-status",
        AssignmentFields = new[]
        {
            "ActorStatusEffectComponent.statusProfile"
        },
        CustomizationMoments = new[]
        {
            "ActorStatusEffectProfile.startingEffects",
            "ActorStatusEffectProfile.allowRefreshExistingEffects",
            "ActorStatusEffectProfile.defaultShieldDamageReduction",
            "StatusEffectDefinition.effectKind",
            "StatusEffectDefinition.stackMode"
        })]
    public interface IActorStatusEffectReceiver : IActorStatusEffectSink
    {
    }
}
