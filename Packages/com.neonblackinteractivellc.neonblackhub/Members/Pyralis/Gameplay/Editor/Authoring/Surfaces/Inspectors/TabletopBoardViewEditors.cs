using NeonBlack.Gameplay.Features.Tabletop;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopBoardSpaceView))]
    public sealed class TabletopBoardSpaceViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Tabletop Board Space View",
                "Generated or prefab-authored selectable surface for one BoardCoordinate.",
                whenToUse: new[]
                {
                    "Use this on board-space prefabs assigned to TabletopBoardGridPresenter.",
                    "Keep legal move checks in BoardMoveActionResolver and BoardMovePolicyDefinition."
                },
                assignFirst: new[]
                {
                    "Let TabletopBoardGridPresenter initialize this view at runtime.",
                    "Keep a Collider on the object if selection should use OnMouseDown."
                },
                validation: new[]
                {
                    "Selecting the object should call back into TabletopBoardGridPresenter.",
                    "Invalid selections should report LastIssue without moving board state."
                },
                manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/Board_Card_Tabletop_Setup.md")));

            DrawDefaultInspector();
        }
    }

    [CustomEditor(typeof(TabletopBoardPieceView))]
    public sealed class TabletopBoardPieceViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            PyralisInspectorGuide.DrawGuide(new PyralisGuideContent(
                "Guided Authoring: Tabletop Board Piece View",
                "Generated or prefab-authored visual for one logical BoardPieceState.",
                whenToUse: new[]
                {
                    "Use this on piece prefabs assigned to TabletopBoardGridPresenter.",
                    "Keep piece identity and movement synchronized from BoardRuntimeState."
                },
                assignFirst: new[]
                {
                    "Let TabletopBoardGridPresenter initialize PieceId and Coordinate at runtime.",
                    "Add project visuals as children or prefab components; do not move logical state from this view."
                },
                validation: new[]
                {
                    "Selecting the object should call back into TabletopBoardGridPresenter.",
                    "After a legal move, this view should follow the piece coordinate reported by BoardRuntimeState.",
                    "Captured or missing logical pieces should hide their view."
                },
                manualPath: PyralisInspectorGuide.AuthoringDocPath("Prefabs/Board_Card_Tabletop_Setup.md")));

            DrawDefaultInspector();
        }
    }
}
