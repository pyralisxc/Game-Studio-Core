using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(PawnDefinition))]
    public class PawnDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            PawnDefinition definition = (PawnDefinition)target;

            DrawDefaultInspector();

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Pawn Definition",
                "A PawnDefinition is the authored body for a participant. Core controller profiles define baseline movement, input, combat, traversal, presentation, and animation. Feature Modules are the pawn ability list.",
                whenToUse: new[]
                {
                    "Use this when a participant needs a spawned actor body.",
                    "Use Feature Modules for reusable abilities such as interaction, pickups, guard/reaction, status effects, feedback, traversal, or future custom pawn abilities."
                },
                createBefore: new[]
                {
                    "Pawn prefab with PawnRoot and the lane-specific runtime stack.",
                    "Input, movement, presentation, and animation profiles needed by the pawn.",
                    "FeatureModuleDefinition assets for optional abilities the pawn should install."
                },
                assignFirst: new[]
                {
                    "Assign Pawn Prefab.",
                    "Assign Presentation Profile and Movement Profile.",
                    "Assign Default Input Profile for player-controlled pawns.",
                    "Add ability modules to Feature Modules when the pawn needs optional capabilities."
                },
                safeToCustomize: new[]
                {
                    "Leave Combat, Traversal, Animation, or Feature Modules empty when that pawn shape does not need them.",
                    "Add or remove ability modules per pawn archetype instead of hard-coding every action into the controller.",
                    "Use each FeatureModuleDefinition's supported presentation modes to keep 2D, 2.5D, and rigged 3D support explicit."
                },
                validation: new[]
                {
                    "PawnRoot installs Feature Modules through ActorFeatureHost at runtime.",
                    "Duplicate module ids or presentation-mode mismatches are reported here.",
                    "Feature runtime prefabs must implement IFeatureModuleRuntime and expose any module-specific contracts."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Pawn_Setup.md")));

            PyralisInspectorGuide.DrawValidationIssues(definition.GetValidationIssues(), "Pawn definition is ready for participant assignment.");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
