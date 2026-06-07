using NeonBlack.Gameplay.Features.Tabletop;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopBoardGridPresenter))]
    public sealed class TabletopBoardGridPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Tabletop Board Grid Presenter",
                "Builds a visible, selectable scene board from a BoardDefinition and routes selections through TabletopBoardSelectionBridge.",
                whenToUse: new[]
                {
                    "A tabletop route needs a first scene proof before custom board art or UI exists.",
                    "You want board-space GameObjects with colliders that can select pieces and destinations.",
                    "The scene should exercise BoardDefinition, BoardMovePolicyDefinition, ActionQueueService, and BoardMoveActionResolver together."
                },
                createBefore: new[]
                {
                    "BoardDefinition with starting pieces.",
                    "BoardMovePolicyDefinition when movement should be constrained.",
                    "Optional prefabs for spaces and pieces if the generated defaults are not enough."
                },
                assignFirst: new[]
                {
                    "Assign Board Definition.",
                    "Assign Move Policy Definition if the mode has one.",
                    "Assign Turn Order Definition when active seats should gate and advance local turns.",
                    "Leave Selection Bridge empty to let the presenter add one on the same GameObject."
                },
                safeToCustomize: new[]
                {
                    "Replace Space Prefab and fallback Piece Prefab with project art while keeping TabletopBoardSpaceView and TabletopBoardPieceView.",
                    "Assign per-piece visuals on BoardPieceDefinition when different piece types need different imported prefabs.",
                    "Turn Resolve Queued Move Immediately off when animations, command logs, turns, or networking own action resolution."
                },
                validation: new[]
                {
                    "Press Play and select a piece space, then a destination space.",
                    "A legal move should update the logical BoardRuntimeState and piece view position.",
                    "When Turn Order Definition is assigned, only the active seat can move and a resolved local move advances to the next seat.",
                    "Invalid setup or illegal moves should report LastIssue instead of mutating board state."
                },
                manualPath: PyralisInspectorGuide.SetupManualPath("Prefabs/Board_Card_Tabletop_Setup.md")));

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
