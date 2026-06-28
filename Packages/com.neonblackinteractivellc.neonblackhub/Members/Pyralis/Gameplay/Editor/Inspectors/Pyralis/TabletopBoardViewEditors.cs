using NeonBlack.Gameplay.Modules.Tabletop;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopBoardSpaceView))]
    public sealed class TabletopBoardSpaceViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();
        }
    }

    [CustomEditor(typeof(TabletopBoardPieceView))]
    public sealed class TabletopBoardPieceViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();
        }
    }
}
