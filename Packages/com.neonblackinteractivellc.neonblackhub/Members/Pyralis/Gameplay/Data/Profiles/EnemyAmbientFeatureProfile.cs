using System;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Animation,
        ModuleId = "enemy.ambient",
        ProfileType = typeof(EnemyAmbientFeatureProfile),
        RequiredInterfaceNames = new[] { "NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime" },
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.Rigged3D },
        AssignmentFields = new[] { nameof(EnemyAmbientFeatureProfile.enableAmbientLookAround), nameof(EnemyAmbientFeatureProfile.lookAroundInterval) },
        FirstProof = "Assign this profile to an enemy and verify ambient behaviors match the defined intervals.",
        NativeSetup = new[]
        {
            "create EnemyAmbientFeatureProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with EnemyAmbientFeatureRuntime",
            "assign profile asset",
            "add module to FeatureModuleDefinition array on enemy actor",
            "assign EnemyAI or compatible enemy runtime host"
        },
        CustomizationMoments = new[]
        {
            "EnemyAmbientFeatureProfile.enableAmbientLookAround",
            "EnemyAmbientFeatureProfile.lookAroundInterval",
            "EnemyAmbientFeatureProfile.requirePatrolState",
            "EnemyAmbientFeatureProfile.suppressDuringReactionLock",
            "FeatureModuleDefinition.supportedPresentationModes"
        }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Enemy Ambient Feature Profile", fileName = "EnemyAmbientFeatureProfile")]
    public class EnemyAmbientFeatureProfile : ScriptableObject
    {
        public bool enableAmbientLookAround = true;
        public float lookAroundInterval = 3f;
        public bool requirePatrolState = true;
        public bool suppressDuringReactionLock = true;

        public void Sanitize()
        {
            lookAroundInterval = Mathf.Max(0.1f, lookAroundInterval);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
