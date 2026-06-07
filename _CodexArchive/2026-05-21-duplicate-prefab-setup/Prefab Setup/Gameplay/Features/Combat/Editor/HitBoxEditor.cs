using NeonBlack.Gameplay.Features.Combat;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Inspector for HitBox.
/// Lets Enemy AI range override behave as a checkbox-controlled slider.
/// </summary>
[CustomEditor(typeof(HitBox))]
public class HitBoxEditor : Editor
{
    private SerializedProperty _owner;
    private SerializedProperty _hitFXPrefab;
    private SerializedProperty _hitSFX;
    private SerializedProperty _freezeFrameDuration;
    private SerializedProperty _cameraShakeIntensity;
    private SerializedProperty _cameraShakeDuration;
    private SerializedProperty _enableEnemyAttackRangeOverride;
    private SerializedProperty _enemyAttackRangeOverride;

    private void OnEnable()
    {
        _owner                          = serializedObject.FindProperty("owner");
        _hitFXPrefab                    = serializedObject.FindProperty("hitFXPrefab");
        _hitSFX                         = serializedObject.FindProperty("hitSFX");
        _freezeFrameDuration            = serializedObject.FindProperty("freezeFrameDuration");
        _cameraShakeIntensity           = serializedObject.FindProperty("cameraShakeIntensity");
        _cameraShakeDuration            = serializedObject.FindProperty("cameraShakeDuration");
        _enableEnemyAttackRangeOverride = serializedObject.FindProperty("enableEnemyAttackRangeOverride");
        _enemyAttackRangeOverride       = serializedObject.FindProperty("enemyAttackRangeOverride");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Owner", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_owner);
        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField("Hit FX", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_hitFXPrefab);
        EditorGUILayout.PropertyField(_hitSFX);
        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField("Hit Pause", EditorStyles.boldLabel);
        EditorGUILayout.Slider(_freezeFrameDuration, 0f, 0.3f,
            new GUIContent("Freeze Frame Duration",
                "Seconds to freeze time on hit. 0 = disabled. 0.04–0.08 = light, 0.1–0.15 = heavy."));
        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField("Camera Shake", EditorStyles.boldLabel);
        EditorGUILayout.Slider(_cameraShakeIntensity, 0f, 1f,
            new GUIContent("Shake Intensity",
                "Peak displacement in world units. 0 = disabled. 0.1–0.2 = punch, 0.25–0.4 = heavy."));
        EditorGUILayout.Slider(_cameraShakeDuration, 0f, 0.5f,
            new GUIContent("Shake Duration",
                "Seconds the camera shake lasts on hit."));
        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField("Enemy AI Range Override", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_enableEnemyAttackRangeOverride,
            new GUIContent("Enable Enemy Attack Range Override"));

        using (new EditorGUI.DisabledScope(!_enableEnemyAttackRangeOverride.boolValue))
        {
            EditorGUILayout.Slider(_enemyAttackRangeOverride, 0.1f, 25f,
                new GUIContent("Enemy Attack Range Override"));
        }

        if (_enableEnemyAttackRangeOverride.boolValue)
        {
            EditorGUILayout.HelpBox(
                "EnemyAI can use this range for attacks that target this hitbox zone.",
                MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
