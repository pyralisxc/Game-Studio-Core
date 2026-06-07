using NeonBlack.Gameplay.Data.Definitions;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(ActorAnimationDefinition))]
    public class ActorAnimationDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            ActorAnimationDefinition definition = (ActorAnimationDefinition)target;
            if (definition.supportedSignals == null || definition.supportedSignals.Length == 0)
                EditorGUILayout.HelpBox("Leave Supported Signals empty to allow all signals, or list the exact supported signal surface for this definition.", MessageType.Info);

            if (!definition.supportsSprite2D && !definition.supportsBillboard2_5D && !definition.supportsRigged3D)
                EditorGUILayout.HelpBox("At least one presentation mode should be supported.", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
