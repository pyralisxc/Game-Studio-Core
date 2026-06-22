using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Combat;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for HitBox. Keeps hit feedback tuning close to damage setup
/// and explains how hitboxes fit melee, projectile, and enemy AI paths.
/// </summary>
[CustomEditor(typeof(HitBox))]
public class HitBoxEditor : Editor
{
    private SerializedProperty _owner;
    private SerializedProperty _hitFXPrefab;
    private SerializedProperty _hitSFX;
    private SerializedProperty _freezeFrameDuration;
    private SerializedProperty _hitPauseSink;
    private SerializedProperty _cameraShakeIntensity;
    private SerializedProperty _cameraShakeDuration;
    private SerializedProperty _cameraShakeSink;
    private SerializedProperty _enableEnemyAttackRangeOverride;
    private SerializedProperty _enemyAttackRangeOverride;

    private void OnEnable()
    {
        _owner = serializedObject.FindProperty("owner");
        _hitFXPrefab = serializedObject.FindProperty("hitFXPrefab");
        _hitSFX = serializedObject.FindProperty("hitSFX");
        _freezeFrameDuration = serializedObject.FindProperty("freezeFrameDuration");
        _hitPauseSink = serializedObject.FindProperty("hitPauseSink");
        _cameraShakeIntensity = serializedObject.FindProperty("cameraShakeIntensity");
        _cameraShakeDuration = serializedObject.FindProperty("cameraShakeDuration");
        _cameraShakeSink = serializedObject.FindProperty("cameraShakeSink");
        _enableEnemyAttackRangeOverride = serializedObject.FindProperty("enableEnemyAttackRangeOverride");
        _enemyAttackRangeOverride = serializedObject.FindProperty("enemyAttackRangeOverride");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PyralisInspectorHandoff.DrawAuthoringButton("HitBox", null);

        EditorGUILayout.LabelField("Owner", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_owner);
        EditorGUILayout.Space(4f);

        EditorGUILayout.LabelField("Hit FX", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_hitFXPrefab);
        EditorGUILayout.PropertyField(_hitSFX);
        EditorGUILayout.Space(4f);

        EditorGUILayout.LabelField("Hit Pause", EditorStyles.boldLabel);
        EditorGUILayout.Slider(
            _freezeFrameDuration,
            0f,
            0.3f,
            new GUIContent(
                "Freeze Frame Duration",
                "Seconds to freeze time on hit. 0 = disabled. 0.04-0.08 = light, 0.10-0.15 = heavy."));
        EditorGUILayout.PropertyField(_hitPauseSink, new GUIContent("Hit Pause Sink"));
        if (_freezeFrameDuration.floatValue > 0f && !ImplementsInterface(_hitPauseSink, "NeonBlack.Gameplay.Core.Contracts.IHitPauseSink"))
            EditorGUILayout.HelpBox("Recommended: assign TimeManager or another IHitPauseSink when Freeze Frame Duration is greater than zero.", MessageType.Warning);
        EditorGUILayout.Space(4f);

        EditorGUILayout.LabelField("Camera Shake", EditorStyles.boldLabel);
        EditorGUILayout.Slider(
            _cameraShakeIntensity,
            0f,
            1f,
            new GUIContent(
                "Shake Intensity",
                "Peak displacement in world units. 0 = disabled. 0.10-0.20 = punch, 0.25-0.40 = heavy."));
        EditorGUILayout.Slider(
            _cameraShakeDuration,
            0f,
            0.5f,
            new GUIContent("Shake Duration", "Seconds the camera shake lasts on hit."));
        EditorGUILayout.PropertyField(_cameraShakeSink, new GUIContent("Camera Shake Sink"));
        if (_cameraShakeIntensity.floatValue > 0f
            && _cameraShakeDuration.floatValue > 0f
            && !ImplementsInterface(_cameraShakeSink, "NeonBlack.Gameplay.Core.Contracts.ICameraShakeSink"))
        {
            EditorGUILayout.HelpBox("Recommended: assign CameraShake or another ICameraShakeSink when camera shake is enabled.", MessageType.Warning);
        }
        EditorGUILayout.Space(4f);

        EditorGUILayout.LabelField("Enemy AI Range Override", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(
            _enableEnemyAttackRangeOverride,
            new GUIContent("Enable Enemy Attack Range Override"));

        using (new EditorGUI.DisabledScope(!_enableEnemyAttackRangeOverride.boolValue))
        {
            EditorGUILayout.Slider(
                _enemyAttackRangeOverride,
                0.1f,
                25f,
                new GUIContent("Enemy Attack Range Override"));
        }

        if (_enableEnemyAttackRangeOverride.boolValue)
            EditorGUILayout.HelpBox("EnemyAI can use this range for attacks that target this hitbox zone.", MessageType.Info);

        DrawColliderValidation((HitBox)target);

        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawColliderValidation(HitBox hitBox)
    {
        Collider collider = hitBox.GetComponent<Collider>();
        if (collider == null)
        {
            EditorGUILayout.HelpBox("Required Fix: Add a BoxCollider or SphereCollider. HitBox uses it as the overlap-query sizing volume.", MessageType.Warning);
            return;
        }

        if (!(collider is BoxCollider) && !(collider is SphereCollider))
            EditorGUILayout.HelpBox("Required Fix: HitBox only supports BoxCollider and SphereCollider sizing volumes.", MessageType.Warning);

        if (collider.isTrigger)
            EditorGUILayout.HelpBox("Recommended: Turn Is Trigger off. HitBox ignores trigger callbacks and disables this collider at runtime.", MessageType.Info);
    }

    private static bool ImplementsInterface(SerializedProperty property, string interfaceName)
    {
        if (property == null || property.objectReferenceValue == null)
            return false;

        System.Type type = property.objectReferenceValue.GetType();
        System.Type[] interfaces = type.GetInterfaces();
        for (int i = 0; i < interfaces.Length; i++)
        {
            if (interfaces[i].FullName == interfaceName)
                return true;
        }

        return false;
    }
}
