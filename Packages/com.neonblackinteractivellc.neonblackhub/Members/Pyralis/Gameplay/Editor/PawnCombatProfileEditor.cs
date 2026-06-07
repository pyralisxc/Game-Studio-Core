using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(PawnCombatProfile))]
    public class PawnCombatProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            PawnCombatProfile profile = (PawnCombatProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Pawn Combat Profile",
                "A pawn combat profile stores combat tuning and action/sequence references for pawn-backed combat.",
                whenToUse: new[]
                {
                    "Use this for brawlers, fighters, action RPG pawns, enemies, or any actor with authored attacks.",
                    "Projectile-only, menu-only, card, or board combat may use ActionDefinition/ProjectileDefinition without a pawn combat profile."
                },
                createBefore: new[]
                {
                    "PawnDefinition that will reference this profile.",
                    "CombatActionDefinition and CombatSequenceDefinition assets when using authored action chains.",
                    "Hitbox/health setup on the pawn prefab when runtime combat should cause damage."
                },
                assignFirst: new[]
                {
                    "Enable Combat.",
                    "Tune base damage, knockback, cooldowns, block reduction, and combo windows.",
                    "Assign primary, secondary, and aerial sequences as needed."
                },
                safeToCustomize: new[]
                {
                    "WeaponData references can stay empty while using sequence definitions.",
                    "Aerial sequence can stay empty for grounded-only games.",
                    "Duplicate this profile per character archetype or enemy family."
                },
                validation: new[]
                {
                    "PawnDefinition references this profile only when combat is needed.",
                    "Pawn prefab has health/hitbox/combat runtime components.",
                    "At least one combat sequence or weapon is assigned for active combat."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Pawn combat profile is ready for PawnDefinition assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(PawnCombatProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile == null || !profile.enableCombat)
                return issues;

            if (profile.primarySequence == null && profile.secondarySequence == null && profile.aerialSequence == null
                && profile.attackWeapon == null && profile.kickWeapon == null && profile.aerialWeapon == null)
            {
                issues.Add("Combat is enabled, but no combat sequence or weapon data is assigned yet.");
            }

            return issues;
        }
    }
}
