using NeonBlack.Gameplay.Core.Rules.Board;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopBoardSelectionBridge))]
    public sealed class TabletopBoardSelectionBridgeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorHandoff.DrawAuthoringButton("Tabletop Board Selection Bridge", null);

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
