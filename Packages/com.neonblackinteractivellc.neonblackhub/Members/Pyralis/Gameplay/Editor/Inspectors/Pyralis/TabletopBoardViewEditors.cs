using NeonBlack.Gameplay.Modules.Tabletop;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopBoardSpaceView))]
    public sealed class TabletopBoardSpaceViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            PyralisInspectorHandoff.DrawAuthoringButton("Tabletop Board Space View", null);

            DrawDefaultInspector();
        }
    }

    [CustomEditor(typeof(TabletopBoardPieceView))]
    public sealed class TabletopBoardPieceViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            PyralisInspectorHandoff.DrawAuthoringButton("Tabletop Board Piece View", null);

            DrawDefaultInspector();
        }
    }
}
