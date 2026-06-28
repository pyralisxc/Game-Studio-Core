namespace NeonBlack.Gameplay.Editor
{
    /// <summary>Base class for Pyralis custom inspectors that only need a local draw hook.</summary>
    public abstract class PyralisBaseEditor : UnityEditor.Editor
    {
        protected virtual void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawCustomInspector();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>Override this to draw custom inspector content. Defaults to DrawDefaultInspector().</summary>
        protected virtual void DrawCustomInspector()
        {
            DrawDefaultInspector();
        }
    }
}
