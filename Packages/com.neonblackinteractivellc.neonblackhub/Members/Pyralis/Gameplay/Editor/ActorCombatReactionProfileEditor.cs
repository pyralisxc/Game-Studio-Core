using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ActorCombatReactionProfile))]
    public class ActorCombatReactionProfileEditor : UnityEditor.Editor
    {
        private const string ModuleId = "actor.combat.reaction";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            ActorCombatReactionProfile profile = (ActorCombatReactionProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Actor Combat Reaction Profile",
                "An actor combat reaction profile tunes guard, parry, shield break, hurt lock, stagger, and knockback cleanup behavior.",
                whenToUse: new[]
                {
                    "Use this for player or enemy actors that need blocking, parrying, stagger, or readable hit reactions.",
                    "Pair it with the matching FeatureModuleDefinition."
                },
                createBefore: new[]
                {
                    "Health/combat runtime on the actor.",
                    PyralisAuthoringContractGuideText.FeatureModuleSetup(ModuleId)
                },
                assignFirst: new[]
                {
                    "Enable Guard and/or Parry.",
                    "Tune parry window, block reduction, and frontal angle.",
                    "Tune hurt/stagger locks and knockback cleanup."
                },
                safeToCustomize: new[]
                {
                    "Disable parry for simpler brawlers.",
                    "Use shorter locks for fast arcade enemies and longer locks for heavy hits.",
                    "Clear knockback on death can improve death presentation."
                },
                validation: new[]
                {
                    "Block angle and damage reduction match expected defensive feel.",
                    "Reaction locks are not so long they fight player control.",
                    "Actor has a runtime module that consumes this profile."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Health_Combat_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Actor combat reaction profile is ready for a combat reaction feature module.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(ActorCombatReactionProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile != null && profile.enableParry && profile.parryWindowDuration <= 0f)
                issues.Add("Parry is enabled but Parry Window Duration is zero.");

            return issues;
        }
    }
}
