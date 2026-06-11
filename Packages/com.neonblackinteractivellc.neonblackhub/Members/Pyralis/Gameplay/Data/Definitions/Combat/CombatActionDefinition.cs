using System.Collections.Generic;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    /// <summary>
    /// Authored shared combat move that can be reused by 2D, 2.5D, and rigged 3D actors.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Combat | AuthoringCapability.Animation, 
        Relevance = "Project-window creation path for one combat action.",
        AssignmentFields = new[] { nameof(displayName), nameof(inputType), nameof(animationSignal) },
        FirstProof = "Verify the combat action triggers the correct animation and applies damage/weapon effects.",
        NativeSetup = new[] { "Create Asset" },
        ExpertAdvice = "Use comboStep to sequence multi-hit attacks. Use cooldownOverride if this move should be slower or faster than the weapon default."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Combat/Combat Action Definition", fileName = "CombatActionDefinition")]
    public class CombatActionDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (comboStep < 1) yield return "Combo Step must be at least 1.";
            if (comboWindow < 0f) yield return "Combo Window cannot be negative.";
            if (weapon == null) yield return "No Weapon Data assigned. Attack may not have damage or range stats.";
        }

        public string displayName = "Combat Action";
        public CombatInputType inputType = CombatInputType.Primary;
        public CombatActionArchetype archetype = CombatActionArchetype.Strike;
        public ActorAnimationSignal animationSignal = ActorAnimationSignal.AttackPrimary;
        public int comboStep = 1;
        public bool requiresHitConfirmForNextBranch = true;
        public bool finisherResetsCombo = false;
        public float comboWindow = 0.35f;
        public float cooldownOverride = -1f;
        public string fallbackHitBoxZone = "Punch";
        public WeaponData weapon;

        public void Sanitize()
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = !string.IsNullOrWhiteSpace(name) ? name : "Combat Action";
            }

            comboStep = Mathf.Max(1, comboStep);
            comboWindow = Mathf.Max(0f, comboWindow);
            cooldownOverride = cooldownOverride < 0f ? -1f : cooldownOverride;
            if (string.IsNullOrWhiteSpace(fallbackHitBoxZone))
            {
                fallbackHitBoxZone = "Punch";
            }
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
