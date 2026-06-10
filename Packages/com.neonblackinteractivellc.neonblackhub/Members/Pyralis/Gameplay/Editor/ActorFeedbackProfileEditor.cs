using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ActorFeedbackProfile))]
    public class ActorFeedbackProfileEditor : UnityEditor.Editor
    {
        private const string ModuleId = "actor.feedback";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            ActorFeedbackProfile profile = (ActorFeedbackProfile)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Actor Feedback Profile",
                "An actor feedback profile controls which gameplay events an actor publishes to floating text, HUD, audio, VFX, and other feedback receivers.",
                whenToUse: new[]
                {
                    "Use this when an actor should emit damage, healing, death, score, combo, or status feedback.",
                    "Pair it with the matching FeatureModuleDefinition and a feedback receiver on the actor or HUD."
                },
                createBefore: new[]
                {
                    "Feedback receiver component or prefab that listens for actor feedback.",
                    PyralisAuthoringContractGuideText.FeatureModuleSetup(ModuleId)
                },
                assignFirst: new[]
                {
                    "Enable the event categories the game should visibly or audibly respond to.",
                    "Assign this profile to the matching feature module."
                },
                safeToCustomize: new[]
                {
                    "Turn off categories that would create noise for quiet actors or simple prototypes.",
                    "Score and combo events can stay off when the game has no scoring loop yet."
                },
                validation: new[]
                {
                    "At least one event category is enabled when feedback is expected.",
                    "The actor has a compatible feedback receiver or HUD presenter."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Health_Combat_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(profile), "Actor feedback profile is ready for a feedback feature module.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(ActorFeedbackProfile profile)
        {
            List<string> issues = new List<string>();

            if (profile != null &&
                !profile.publishDamageEvents &&
                !profile.publishHealingEvents &&
                !profile.publishDeathEvents &&
                !profile.publishStatusEvents &&
                !profile.publishScoreEvents &&
                !profile.publishComboEvents)
            {
                issues.Add("Every feedback category is disabled. This is valid for silence, but unusual for an actor.feedback module.");
            }

            return issues;
        }
    }
}
