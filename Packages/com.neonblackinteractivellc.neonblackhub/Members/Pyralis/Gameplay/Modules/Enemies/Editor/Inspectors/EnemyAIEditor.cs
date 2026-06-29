using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Modules.Enemies;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for EnemyAI. Keeps local runtime fields grouped.
/// </summary>
[CustomEditor(typeof(EnemyAI))]
public class EnemyAIEditor : Editor
{
    private SerializedObject _detectionSerialized;

    private SerializedProperty _aggroRange;
    private SerializedProperty _leashRange;
    private SerializedProperty _obstacleMask;
    private SerializedProperty _requireLineOfSight;
    private SerializedProperty _targetOverride;

    private SerializedProperty _movementMode;
    private SerializedProperty _moveSpeed;
    private SerializedProperty _waypointTolerance;

    private SerializedProperty _visualRoot;
    private SerializedProperty _spriteDefaultFacesRight;
    private SerializedProperty _presentationCamera;

    private SerializedProperty _patrolPoints;
    private SerializedProperty _randomPatrolDistance;
    private SerializedProperty _enemyProfile;

    private void OnEnable()
    {
        EnemyAI ai = (EnemyAI)target;
        var detection = ai.GetComponent<EnemyDetectionModule>();

        if (detection != null)
        {
            _detectionSerialized = new SerializedObject(detection);
            _aggroRange = _detectionSerialized.FindProperty("aggroRange");
            _leashRange = _detectionSerialized.FindProperty("leashRange");
            _obstacleMask = _detectionSerialized.FindProperty("obstacleMask");
            _requireLineOfSight = _detectionSerialized.FindProperty("requireLineOfSight");
            _targetOverride = _detectionSerialized.FindProperty("targetOverride");
        }

        _movementMode = serializedObject.FindProperty("movementMode");
        _moveSpeed = serializedObject.FindProperty("moveSpeed");
        _waypointTolerance = serializedObject.FindProperty("waypointTolerance");

        _visualRoot = serializedObject.FindProperty("visualRoot");
        _spriteDefaultFacesRight = serializedObject.FindProperty("spriteDefaultFacesRight");
        _presentationCamera = serializedObject.FindProperty("presentationCamera");

        _patrolPoints = serializedObject.FindProperty("patrolPoints");
        _randomPatrolDistance = serializedObject.FindProperty("randomPatrolDistance");
        _enemyProfile = serializedObject.FindProperty("enemyProfile");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _detectionSerialized?.Update();

        bool is3D = _movementMode.enumValueIndex == (int)MovementMode.ThreeD;

        if (_detectionSerialized != null)
        {
            EditorGUILayout.LabelField("Detection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_aggroRange);
            EditorGUILayout.PropertyField(_leashRange);
            EditorGUILayout.PropertyField(_obstacleMask);
            EditorGUILayout.PropertyField(_requireLineOfSight);
            EditorGUILayout.Space(4f);
        }

        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_movementMode);
        EditorGUILayout.PropertyField(_moveSpeed);
        EditorGUILayout.PropertyField(_waypointTolerance);

        EditorGUILayout.HelpBox(
            is3D
                ? "ThreeD chases on the XZ plane for rigged 3D characters, arena games, or enemies with depth movement."
                : "TwoD chases on the X axis for side-scrollers. Use a flat 3D ground collider under the play space.",
            MessageType.Info);
        EditorGUILayout.Space(4f);

        if (is3D)
        {
            EditorGUILayout.LabelField("Visuals (Rigged 3D)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_visualRoot);
            EditorGUILayout.PropertyField(_spriteDefaultFacesRight);
            EditorGUILayout.PropertyField(_presentationCamera);
            if (_presentationCamera.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Presentation Camera is empty. Assign the gameplay camera for screen-left/right facing and billboarding, or call SetPresentationCamera when the enemy spawns.", MessageType.Warning);
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Patrol Points (leave empty for random patrol)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_patrolPoints);
        EditorGUILayout.PropertyField(_randomPatrolDistance);
        EditorGUILayout.Space(4f);

        DrawProfiles();

        serializedObject.ApplyModifiedProperties();
        _detectionSerialized?.ApplyModifiedProperties();
    }

    private void DrawProfiles()
    {
        EditorGUILayout.LabelField("Profiles", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_targetOverride);
        EditorGUILayout.PropertyField(_enemyProfile);
    }
}
