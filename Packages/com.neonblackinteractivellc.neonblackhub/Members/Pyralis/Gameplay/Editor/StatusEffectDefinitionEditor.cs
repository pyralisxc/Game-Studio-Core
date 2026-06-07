using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Presentation.Animation;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(StatusEffectDefinition))]
    public class StatusEffectDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            StatusEffectDefinition definition = (StatusEffectDefinition)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Status Effect Definition",
                "A status effect definition describes a reusable timed combat modifier such as stun, slow, poison, burn, shield, armor, regen, or damage boost.",
                whenToUse: new[]
                {
                    "Use this for buffs, debuffs, damage-over-time, healing-over-time, and combat state effects.",
                    "Assign status effects to ActorStatusEffectProfile or future projectile/action impact systems."
                },
                createBefore: new[]
                {
                    "Runtime status system or feature module that applies these effects.",
                    "Animation signal/custom key if applying the effect should trigger presentation."
                },
                assignFirst: new[]
                {
                    "Set Effect Id, Display Name, Kind, and Stack Mode.",
                    "Set Duration, Magnitude, Tick Interval, and Max Stacks.",
                    "Set Apply Signal or Custom Animation Key when presentation is needed."
                },
                safeToCustomize: new[]
                {
                    "Magnitude meaning depends on Effect Kind and the consuming runtime.",
                    "Duration of zero can represent instant effects when the runtime supports it.",
                    "Tick Interval matters most for periodic effects."
                },
                validation: new[]
                {
                    "Effect Id is stable and unique.",
                    "Stacking rules match expected gameplay.",
                    "Tick Interval is sensible for periodic effects."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Health_Combat_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(definition), "Status effect definition is ready for status profile or combat assignment.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(StatusEffectDefinition definition)
        {
            List<string> issues = new List<string>();

            if (definition == null)
                return issues;

            if (string.IsNullOrWhiteSpace(definition.effectId))
                issues.Add("Effect Id is required.");
            if (string.IsNullOrWhiteSpace(definition.displayName))
                issues.Add("Display Name is required.");
            if (definition.maxStacks < 1)
                issues.Add("Max Stacks must be at least one.");
            if (definition.applySignal == ActorAnimationSignal.Custom && string.IsNullOrWhiteSpace(definition.customAnimationKey))
                issues.Add("Custom Animation Key is required when Apply Signal is Custom.");

            return issues;
        }
    }
}
