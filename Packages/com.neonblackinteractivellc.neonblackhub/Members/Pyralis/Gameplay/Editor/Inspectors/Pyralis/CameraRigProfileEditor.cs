using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(CameraRigProfile))]
    public class CameraRigProfileEditor : PyralisBaseEditor
    {
        protected override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            CameraRigProfile profile = (CameraRigProfile)target;

            if (!profile.useCinemachine)
                EditorGUILayout.HelpBox("Pyralis expects Cinemachine-backed gameplay rigs by default. Disable this only when the scene owns camera composition manually.", MessageType.Info);
        }
    }
}
