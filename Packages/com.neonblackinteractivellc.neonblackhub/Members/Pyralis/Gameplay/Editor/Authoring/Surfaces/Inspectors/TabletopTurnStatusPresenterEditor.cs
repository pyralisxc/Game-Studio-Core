using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Tabletop;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    [CustomEditor(typeof(TabletopTurnStatusPresenter))]
    public sealed class TabletopTurnStatusPresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PyralisInspectorHandoff.DrawAuthoringButton("Tabletop Turn Status Presenter", null);

            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
