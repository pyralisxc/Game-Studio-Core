using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(CameraRigProfile))]
    public class CameraRigProfileEditor : PyralisBaseEditor
    {
        private Object sceneCameraOrRigObject;

        protected override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            CameraRigProfile profile = (CameraRigProfile)target;

            if (!profile.useCinemachine)
                EditorGUILayout.HelpBox("Pyralis expects Cinemachine-backed gameplay rigs by default. Disable this only when the scene owns camera composition manually.", MessageType.Info);

            DrawCinemachineRecipeTools(profile);
            DrawReflectedSettings(profile);
        }

        private void DrawCinemachineRecipeTools(CameraRigProfile profile)
        {
            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Cinemachine Recipe", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(
                    "Use a scene Cinemachine rig as the drafting table. Sync saves supported Cinemachine values into this profile; Apply pushes valid saved values back to the selected rig.",
                    EditorStyles.wordWrappedMiniLabel);

                sceneCameraOrRigObject = EditorGUILayout.ObjectField(
                    "Scene Camera / Rig",
                    sceneCameraOrRigObject,
                    typeof(Object),
                    true);

                Component sceneCameraOrRig = ResolveSceneCameraOrRig(sceneCameraOrRigObject);
                if (sceneCameraOrRigObject != null && sceneCameraOrRig == null)
                {
                    EditorGUILayout.HelpBox(
                        "Select a scene GameObject or Component that owns or contains the Cinemachine rig.",
                        MessageType.Warning);
                }

                using (new EditorGUI.DisabledScope(sceneCameraOrRig == null))
                {
                    if (GUILayout.Button("Sync From Scene Camera"))
                    {
                        Undo.RecordObject(profile, "Sync CameraRigProfile From Scene Camera");
                        profile.ReplaceReflectedSettings(CameraRigProfileApplier.SyncFromSceneCamera(sceneCameraOrRig));
                        EditorUtility.SetDirty(profile);
                        serializedObject.Update();
                        EditorUtility.DisplayDialog("Camera Profile Sync", $"Synced {profile.ReflectedSettings.Count} supported Cinemachine settings.", "OK");
                    }

                    if (GUILayout.Button("Apply To Scene Camera"))
                    {
                        CameraProfileApplyReport report = CameraRigProfileApplier.ApplyToSceneCamera(profile, sceneCameraOrRig);
                        EditorUtility.DisplayDialog("Camera Profile Apply", report.ToString(), "OK");
                    }

                    if (GUILayout.Button("Validate Against Scene Camera"))
                    {
                        Undo.RecordObject(profile, "Validate CameraRigProfile Against Scene Camera");
                        profile.ReplaceReflectedSettings(CameraRigProfileApplier.ValidateAgainstSceneCamera(profile, sceneCameraOrRig));
                        EditorUtility.SetDirty(profile);
                        serializedObject.Update();
                        EditorUtility.DisplayDialog("Camera Profile Validation", $"Validated {profile.ReflectedSettings.Count} saved settings.", "OK");
                    }
                }

                using (new EditorGUI.DisabledScope(profile.ReflectedSettings.Count == 0))
                {
                    if (GUILayout.Button("Clear Missing Fields"))
                    {
                        Undo.RecordObject(profile, "Clear Missing Camera Profile Fields");
                        profile.ClearMissingReflectedSettings();
                        EditorUtility.SetDirty(profile);
                        serializedObject.Update();
                    }
                }
            }
        }

        private static void DrawReflectedSettings(CameraRigProfile profile)
        {
            if (profile.ReflectedSettings.Count == 0)
                return;

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Saved Cinemachine Settings", EditorStyles.miniBoldLabel);
                for (int i = 0; i < profile.ReflectedSettings.Count; i++)
                {
                    CameraProfileEntry entry = profile.ReflectedSettings[i];
                    if (entry == null)
                        continue;

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField($"{entry.DisplayLabel} ({entry.Status})", EditorStyles.miniBoldLabel);
                        EditorGUILayout.LabelField("Component", entry.ComponentDisplayName);
                        EditorGUILayout.LabelField("Path", entry.PropertyPath);
                        EditorGUILayout.LabelField("Value", entry.SerializedValue);
                        if (!string.IsNullOrEmpty(entry.LastReflectedSource))
                            EditorGUILayout.LabelField("Source", entry.LastReflectedSource);

                        if (entry.Status != CameraProfileMappingStatus.Valid)
                            EditorGUILayout.HelpBox(GetStatusMessage(entry), MessageType.Warning);
                    }
                }
            }
        }

        private static string GetStatusMessage(CameraProfileEntry entry)
        {
            return entry.Status switch
            {
                CameraProfileMappingStatus.MissingComponent => "The selected scene rig no longer has this Cinemachine component.",
                CameraProfileMappingStatus.MissingProperty => "The selected scene rig no longer exposes this supported property.",
                CameraProfileMappingStatus.Unsupported => "This saved mapping is not in the current supported Cinemachine registry.",
                CameraProfileMappingStatus.Stale => "This saved mapping needs to be synced or validated again.",
                _ => string.Empty
            };
        }

        private static Component ResolveSceneCameraOrRig(Object candidate)
        {
            if (candidate is Component component)
                return component;

            if (candidate is GameObject gameObject)
                return gameObject.transform;

            return null;
        }
    }
}
