using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ActorStatusEffectProfile))]
    public class ActorStatusEffectProfileEditor : UnityEditor.Editor
    {
        private const string ModuleId = "actor.status";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            ActorStatusEffectProfile profile = (ActorStatusEffectProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Actor Status Effect Profile",
                "An actor status effect profile defines starting status effects and default shield damage reduction for an actor.",
                whenToUse: new[]
                {
                    "Use this for actors that spawn with buffs, debuffs, shields, armor, regen, or other status state.",
                    "Pair it with the matching FeatureModuleDefinition."
                },
                createBefore: new[]
                {
                    "StatusEffectDefinition assets used as starting effects.",
                    PyralisAuthoringContractGuideText.FeatureModuleSetup(ModuleId)
                },
                assignFirst: new[]
                {
                    "Add Starting Effects if the actor should spawn with status state.",
                    "Set Allow Refresh Existing Effects.",
                    "Tune Default Shield Damage Reduction."
                },
                safeToCustomize: new[]
                {
                    "Starting Effects can stay empty for actors that only receive effects at runtime.",
                    "Default Shield Damage Reduction is normalized from 0 to 1."
                },
                validation: new[]
                {
                    "Starting Effects has no missing entries.",
                    "Runtime status feature is present when status behavior is expected."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Health_Combat_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Actor status effect profile is ready for a status feature module.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(ActorStatusEffectProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile?.startingEffects == null)
                return issues;

            for (int i = 0; i < profile.startingEffects.Length; i++)
            {
                if (profile.startingEffects[i] == null)
                    issues.Add($"Starting Effects[{i}] is empty.");
            }

            return issues;
        }
    }
}
