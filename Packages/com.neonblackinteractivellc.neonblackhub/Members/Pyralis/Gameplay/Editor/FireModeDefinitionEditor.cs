using NeonBlack.Gameplay.Features.Combat;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(FireModeDefinition))]
    public class FireModeDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            FireModeDefinition definition = (FireModeDefinition)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Fire Mode Definition",
                "A fire mode describes projectile emission shape: single, auto intent, burst, spread, and optional magazine data for systems that choose to use it.",
                whenToUse: new[]
                {
                    "Use this for guns, launchers, bows, magic casts, thrown barrages, and any repeated projectile source.",
                    "Separate fire mode from ProjectileDefinition so many weapons can reuse the same projectile with different firing rules."
                },
                createBefore: new[]
                {
                    "ProjectileDefinition fired by the weapon/action.",
                    "Ammo, reload, or input system if this game wants clip and automatic-fire behavior."
                },
                assignFirst: new[]
                {
                    "Set Fire Mode Id and Display Name.",
                    "Choose Automatic and Cooldown as authoring intent for the firing source.",
                    "Configure clip/reload only if a magazine runtime or weapon system will consume those values.",
                    "Configure burst, projectiles per shot, and spread."
                },
                safeToCustomize: new[]
                {
                    "Clip Size of zero can mean no clip/reload behavior.",
                    "ProjectileFirePlanner consumes burst, burst interval, projectiles per shot, and spread.",
                    "ProjectileMagazineState consumes clip size and ammo per shot when a caller owns magazine flow.",
                    "Projectiles Per Shot and Spread Angle create shotgun or fan patterns.",
                    "Burst Count is independent from automatic fire."
                },
                validation: new[]
                {
                    "Clip-based modes have Ammo Per Shot and Reload Duration when this game uses magazine flow.",
                    "Burst and spread values match the intended weapon fantasy.",
                    "Cooldown is appropriate for the animation and input rhythm owned by the firing source."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")));

            List<string> issues = definition != null ? definition.GetValidationIssues() : new List<string>();
            PyralisInspectorGuide.DrawValidationIssues(issues, "Fire mode definition is ready for weapon or launcher assignment.");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
