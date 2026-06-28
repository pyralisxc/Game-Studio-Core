using System;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Types.Animation;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat,
        ModuleId = "enemy.reaction",
        ProfileType = typeof(EnemyReactionProfile),
        RequiredInterfaces = new[] { typeof(IEnemyReactionState) },
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.ThirdPerson3D },
        Proof = "Verify that IsReactionLocked is true when the enemy is staggered or hit.",
        ProofTargetId = "proof.npc-enemy-behavior",
        NativeSetup = new[]
        {
            "add EnemyReactionComponent to the enemy root",
            "assign EnemyReactionProfile",
            "ensure HealthComponent and Health/animation sources are present for reaction pathways"
        },
        AssignmentFields = new[]
        {
            "EnemyReactionComponent.reactionProfile"
        },
        CustomizationMoments = new[]
        {
            "EnemyReactionProfile.enableReactions",
            "EnemyReactionProfile.hurtLockDuration",
            "EnemyReactionProfile.staggerDamageThreshold",
            "EnemyReactionProfile.hitPauseDuration",
            "EnemyReactionProfile.cameraShakeIntensity"
        }
    )]
    public interface IEnemyReactionState
    {
        bool IsReactionLocked { get; }
    }
}
