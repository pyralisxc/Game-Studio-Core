using NeonBlack.Gameplay.Modules.Tabletop;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopTurnStatusPresenter))]
    public sealed class TabletopTurnStatusPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
