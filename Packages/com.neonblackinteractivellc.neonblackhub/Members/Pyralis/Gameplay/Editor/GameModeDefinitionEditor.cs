using NeonBlack.Gameplay.Data.Definitions;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(GameModeDefinition))]
    public class GameModeDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            PyralisInspectorHandoff.DrawAuthoringButton();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
