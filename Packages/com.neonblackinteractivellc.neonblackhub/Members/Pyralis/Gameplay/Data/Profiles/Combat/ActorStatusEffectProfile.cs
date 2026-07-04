using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Category = "Combat, Stats",
        CapabilityPath = "Combat/Actions/Actor Status Effect Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Defines common status effect vulnerabilities and immunities for an actor.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/combat",
        SetupSteps = new[] { "List starting effects.", "Set default shield reduction." },
        SuccessChecks = new[] { "Verify the actor is spawned with the specified starting effects." },
        Tags = new[] { "capability:Combat", "capability:Stats", "runtime:Combat" },
        Selectable = false
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Actor Status Effect Profile", fileName = "ActorStatusEffectProfile")]
    public class ActorStatusEffectProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (defaultShieldDamageReduction < 0f || defaultShieldDamageReduction > 1f)
                yield return RuntimeValidationIssue.Required("Default Shield Damage Reduction must be between 0 and 1.");
        }

        public StatusEffectDefinition[] startingEffects;
        public bool allowRefreshExistingEffects = true;
        [Range(0f, 1f)] public float defaultShieldDamageReduction = 0.5f;

        public void Sanitize()
        {
            defaultShieldDamageReduction = Mathf.Clamp01(defaultShieldDamageReduction);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
