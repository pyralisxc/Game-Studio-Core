using NeonBlack.Gameplay.Features.Combat;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(WeaponData))]
    public class WeaponDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            WeaponData weapon = (WeaponData)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Weapon Data",
                "Weapon data is the reusable authored damage, timing, range, animation override, hit zone, and projectile setup for combat actors.",
                whenToUse: new[]
                {
                    "Use this for melee weapons, ranged weapons, thrown weapons, and action-specific damage overrides.",
                    "Assign it directly to pawn combat, enemy attacks, or combat action definitions."
                },
                createBefore: new[]
                {
                    "ProjectileDefinition and optional FireModeDefinition if Weapon Type is Ranged or Thrown.",
                    "Animator override controller only if the weapon changes animation presentation."
                },
                assignFirst: new[]
                {
                    "Set Weapon Name, Damage, Knockback, and Attack Cooldown.",
                    "Set Hit Box Zone for melee/thrown contact.",
                    "For ranged/thrown weapons, assign Projectile Definition and optional Fire Mode Definition.",
                },
                safeToCustomize: new[]
                {
                    "Attack Range of zero lets actor/default range logic decide.",
                    "Hit Delay and Hit Duration should match animation timing.",
                    "Override Controller can stay empty for most shared controllers."
                },
                validation: new[]
                {
                    "Ranged/thrown weapons have a Projectile Definition.",
                    "Melee weapons have a hitbox zone used by the actor.",
                    "Timing values are non-negative."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(weapon), "Weapon data is ready for combat profile or action assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(WeaponData weapon)
        {
            List<string> issues = new List<string>();

            if (weapon == null)
                return issues;

            if (string.IsNullOrWhiteSpace(weapon.weaponName))
                issues.Add("Weapon Name is required.");
            if ((weapon.weaponType == WeaponType.Ranged || weapon.weaponType == WeaponType.Thrown) && weapon.projectileDefinition == null)
                issues.Add("Ranged/thrown weapons require a Projectile Definition.");
            if (weapon.weaponType == WeaponType.Melee && string.IsNullOrWhiteSpace(weapon.hitBoxZone))
                issues.Add("Melee weapons should name the actor Hit Box Zone they use.");

            return issues;
        }
    }
}
