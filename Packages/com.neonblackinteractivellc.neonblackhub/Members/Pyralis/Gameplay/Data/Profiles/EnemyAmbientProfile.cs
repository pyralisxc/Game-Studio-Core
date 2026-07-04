using System;
using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        StableId = "enemy.ambient.profile",
        Category = "Combat, Animation",
        Surface = AuthoringSurface.Profile,
        Summary = "Configuration for idle and non-combat 'living world' behaviors for enemies.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/enemies",
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Enemies.EnemyAmbientComponent" },
        SetupSteps = new[]
        {
            "Create EnemyAmbientProfile asset.",
            "Add EnemyAmbientComponent to the enemy root.",
            "Assign this profile to EnemyAmbientComponent."
        },
        SuccessChecks = new[] { "Assign this profile to an enemy and verify ambient behaviors match the defined intervals." },
        Tags = new[] { "capability:Combat", "capability:Animation" },
        Selectable = false
    )]
[CreateAssetMenu(menuName = "NeonBlack/Profiles/Enemy Ambient Profile", fileName = "EnemyAmbientProfile")]
    public class EnemyAmbientProfile : ScriptableObject
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
