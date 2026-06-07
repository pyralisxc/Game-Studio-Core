using NeonBlack.Gameplay.Data.Definitions;
using System.Collections.Generic;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ActorAnimationDefinition))]
    public class ActorAnimationDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            ActorAnimationDefinition definition = (ActorAnimationDefinition)target;

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Actor Animation Definition",
                "An actor animation definition documents which presentation modes and gameplay animation signals an animator setup supports.",
                whenToUse: new[]
                {
                    "Use this when a pawn, enemy, or shared actor prefab needs a clear animation signal contract.",
                    "Leave Supported Signals empty when the animator/controller can accept the standard Pyralis signal set."
                },
                createBefore: new[]
                {
                    "Animator Controller or animation graph used by the actor.",
                    "PawnAnimationProfile or PawnDefinition that will reference this animation contract."
                },
                assignFirst: new[]
                {
                    "Set supported presentation modes: Sprite2D, Billboard2.5D, Rigged3D.",
                    "List Supported Signals only if this animator intentionally supports a limited subset."
                },
                safeToCustomize: new[]
                {
                    "Display Name is editor-facing.",
                    "Supported Signals can stay empty when the actor supports the full standard Pyralis signal set.",
                    "Notes should explain controller expectations or required parameter names."
                },
                validation: new[]
                {
                    "At least one presentation mode is enabled.",
                    "Pawn Animation Profile signal names match the animator/controller in the prefab.",
                    "Limited signal lists are intentional."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(GetValidationIssues(definition), "Actor animation definition is ready for pawn or enemy animation setup.");

            serializedObject.ApplyModifiedProperties();
        }

        private static List<string> GetValidationIssues(ActorAnimationDefinition definition)
        {
            List<string> issues = new List<string>();

            if (definition == null)
                return issues;

            if (!definition.supportsSprite2D && !definition.supportsBillboard2_5D && !definition.supportsRigged3D)
                issues.Add("At least one presentation mode should be supported.");

            return issues;
        }
    }
}
