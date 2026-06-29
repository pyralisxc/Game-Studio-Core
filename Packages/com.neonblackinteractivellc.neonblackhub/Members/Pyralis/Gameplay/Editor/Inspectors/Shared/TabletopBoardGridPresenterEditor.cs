using NeonBlack.Gameplay.Modules.Tabletop;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopBoardGridPresenter))]
    public sealed class TabletopBoardGridPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
