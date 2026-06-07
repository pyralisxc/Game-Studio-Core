using NeonBlack.Gameplay.Features.Combat;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(CombatActionDefinition))]
    public class CombatActionDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            CombatActionDefinition definition = (CombatActionDefinition)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Combat Action Definition",
                "A combat action is one authored move inside a combo, attack chain, or projectile combat lane.",
                whenToUse: new[]
                {
                    "Use this for punches, kicks, launchers, finishers, specials, and projectile-firing steps.",
                    "Put actions into CombatSequenceDefinition assets to build brawler/fighter style input chains."
                },
                createBefore: new[]
                {
                    "WeaponData if this action should override damage, range, timing, or hit zone.",
                    "Animator signal or hitbox zone used by the actor prefab."
                },
                assignFirst: new[]
                {
                    "Set Input Type and Archetype.",
                    "Set Animation Signal and Combo Step.",
                    "Assign Weapon or fallback hitbox zone."
                },
                safeToCustomize: new[]
                {
                    "Cooldown Override can stay -1 to use the weapon or actor default.",
                    "Requires Hit Confirm For Next Branch is good for deliberate combo chains.",
                    "Fallback Hit Box Zone should match a HitBox slot on the actor."
                },
                validation: new[]
                {
                    "Display Name is readable.",
                    "Combo Step is one or greater.",
                    "Fallback Hit Box Zone matches the actor prefab when no weapon is assigned."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(definition), "Combat action definition is ready for sequence assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(CombatActionDefinition definition)
        {
            List<string> issues = new List<string>();

            if (definition == null)
                return issues;

            if (string.IsNullOrWhiteSpace(definition.displayName))
                issues.Add("Display Name is required.");
            if (definition.comboStep < 1)
                issues.Add("Combo Step must be one or greater.");
            if (string.IsNullOrWhiteSpace(definition.fallbackHitBoxZone) && definition.weapon == null)
                issues.Add("Fallback Hit Box Zone should be set when no Weapon is assigned.");

            return issues;
        }
    }
}
