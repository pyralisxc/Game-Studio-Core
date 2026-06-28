using NeonBlack.Gameplay.Data.Tabletop;
using NeonBlack.Gameplay.Modules.Tabletop.Runtime;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopBoardSelectionBridge))]
    public sealed class TabletopBoardSelectionBridgeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
