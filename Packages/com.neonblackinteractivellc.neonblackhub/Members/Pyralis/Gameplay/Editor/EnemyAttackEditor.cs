using NeonBlack.Gameplay.Features.Combat;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(EnemyAttack))]
    public class EnemyAttackEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            EnemyAttack attack = (EnemyAttack)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Enemy Attack",
                "An enemy attack defines one attack option: animation signal, hitbox zone, damage, timing, range override, and AI selection weight.",
                whenToUse: new[]
                {
                    "Use this for individual enemy melee swings, lunges, ranged attacks, specials, and weighted attack choices.",
                    "Put attacks into EnemyCombatProfile for sequential, random, or priority-based enemy combat."
                },
                createBefore: new[]
                {
                    "Enemy prefab with animator signals and hitbox zones.",
                    "EnemyCombatProfile that will reference this attack."
                },
                assignFirst: new[]
                {
                    "Set Animation Signal or Custom Animation Key.",
                    "Set Hit Box Zone, Damage, Knockback, and Timing.",
                    "Tune Weight and AI Priority when using random/priority attack selection."
                },
                safeToCustomize: new[]
                {
                    "Attack Range and Radius of zero let the enemy/default detection decide.",
                    "Attack Cooldown of zero lets the profile/default cooldown decide.",
                    "Custom animation keys are only needed when the shared signal is not specific enough."
                },
                validation: new[]
                {
                    "Hit Box Zone exists on the enemy prefab.",
                    "Timing matches the animation.",
                    "Weight and AI Priority are positive for selectable attacks."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Enemy_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(attack), "Enemy attack is ready for enemy combat profile assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(EnemyAttack attack)
        {
            List<string> issues = new List<string>();

            if (attack == null)
                return issues;

            if (string.IsNullOrWhiteSpace(attack.hitBoxZone))
                issues.Add("Hit Box Zone should match a HitBox slot on the enemy prefab.");
            if (attack.useCustomAnimationKey && string.IsNullOrWhiteSpace(attack.customAnimationKey))
                issues.Add("Custom Animation Key is required when Use Custom Animation Key is enabled.");
            if (attack.weight <= 0f)
                issues.Add("Weight should be greater than zero for random attack selection.");
            if (attack.aiPriority <= 0f)
                issues.Add("AI Priority should be greater than zero for priority attack selection.");

            return issues;
        }
    }
}
