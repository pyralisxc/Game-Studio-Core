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

            PyralisInspectorHandoff.DrawAuthoringButton("Tabletop Board Grid Presenter", null);

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
