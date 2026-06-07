using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(PawnAnimationProfile))]
    public class PawnAnimationProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            PawnAnimationProfile profile = (PawnAnimationProfile)target;
            if (profile.animationDefinition == null)
                EditorGUILayout.HelpBox("Assign an Actor Animation Definition so supported signals are explicit.", MessageType.Warning);

            if (profile.baseController == null)
                EditorGUILayout.HelpBox("Assign a base Animator Controller. The new animation stack is Unity-Animator-driven.", MessageType.Warning);

            if (profile.bindings == null || profile.bindings.Length == 0)
                EditorGUILayout.HelpBox("Add bindings so supported animation signals can drive Animator parameters.", MessageType.Info);
            else if (profile.animationDefinition != null)
            {
                foreach (ActorAnimationBinding binding in profile.bindings)
                {
                    if (binding == null)
                        continue;

                    if (!profile.animationDefinition.SupportsSignal(binding.signal))
                        EditorGUILayout.HelpBox(
                            $"Binding '{binding.parameterName}' uses {binding.signal}, but that signal is not listed on the assigned definition.",
                            MessageType.Warning);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
