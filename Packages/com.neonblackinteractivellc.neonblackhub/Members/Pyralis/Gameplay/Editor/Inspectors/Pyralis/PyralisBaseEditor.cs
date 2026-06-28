using System.Linq;
using Pys.Authoring.Contracts;
using Pys.Authoring.Editor.Contracts;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor
{
    /// <summary>
    /// Base class for all Pyralis custom inspectors.
    /// Automatically integrates reflective AuthoringContract guidance and IRuntimeValidationProvider issues.
    /// </summary>
    public abstract class PyralisBaseEditor : UnityEditor.Editor
    {
        private ResolvedAuthoringContract _contract;
        private bool _checkedContract;

        protected virtual void OnEnable()
        {
            if (target == null) return;
            _contract = AuthoringContractResolver.Resolve(target.GetType()).FirstOrDefault();
            _checkedContract = true;
        }

        public override void OnInspectorGUI()
        {
            if (!_checkedContract) OnEnable();

            serializedObject.Update();

            if (_contract != null)
            {
                PyralisResolvedInspectorValidation.DrawHeader(_contract);
            }

            DrawCustomInspector();

            if (_contract != null)
            {
                PyralisResolvedInspectorValidation.DrawValidationFooter(_contract, target, serializedObject);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Override this to draw your custom inspector content.
        /// Defaults to DrawDefaultInspector().
        /// </summary>
        protected virtual void DrawCustomInspector()
        {
            DrawDefaultInspector();
        }
    }
}
