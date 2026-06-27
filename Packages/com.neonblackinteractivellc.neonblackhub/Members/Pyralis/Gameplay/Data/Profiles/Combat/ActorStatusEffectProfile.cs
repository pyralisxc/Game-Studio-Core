using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Stats,
        Relevance = "Defines common status effect vulnerabilities and immunities for an actor.",
        NativeSetup = new[] { "List starting effects.", "Set default shield reduction." },
        AssignmentFields = new[] { nameof(defaultShieldDamageReduction) },
        Proof = "Verify the actor is spawned with the specified starting effects.",
        ExpertAdvice = "Use defaultShieldDamageReduction to scale incoming damage when the actor has an active shield effect.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/combat",
        CapabilityPath = "Combat/Actions/Actor Status Effect Profile",
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.Combat }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Actor Status Effect Profile", fileName = "ActorStatusEffectProfile")]
    public class ActorStatusEffectProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (defaultShieldDamageReduction < 0f || defaultShieldDamageReduction > 1f)
                yield return PyralisRuntimeValidationIssue.Required("Default Shield Damage Reduction must be between 0 and 1.");
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
