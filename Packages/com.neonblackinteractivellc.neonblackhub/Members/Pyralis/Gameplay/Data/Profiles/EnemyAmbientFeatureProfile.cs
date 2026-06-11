using System;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Animation,
        ModuleId = "enemy.ambient",
        Relevance = "Configuration for idle and non-combat 'living world' behaviors for enemies.",
        ProfileType = typeof(EnemyAmbientFeatureProfile),
        RequiredInterfaceNames = new[] { "NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime" },
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.ThirdPerson3D },
        AssignmentFields = new[] { nameof(enableAmbientLookAround), nameof(lookAroundInterval) },
        FirstProof = "Assign this profile to an enemy and verify ambient behaviors match the defined intervals.",
        NativeSetup = new[]
        {
            "Create EnemyAmbientFeatureProfile asset.",
            "Create FeatureModuleDefinition and assign this profile.",
            "Add the module to the enemy's feature list."
        },
        ExpertAdvice = "Use ambient behaviors to make patrols feel less robotic. Ensure 'requirePatrolState' is true if these should only play when the AI isn't alerted.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/enemies"
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
