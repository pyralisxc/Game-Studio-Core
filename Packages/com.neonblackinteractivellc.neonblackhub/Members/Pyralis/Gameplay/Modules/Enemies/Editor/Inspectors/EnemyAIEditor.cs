using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Combat;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Modules.Enemies;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for EnemyAI. Keeps runtime fields grouped while the
/// Pyralis Authoring Window owns route setup and first-proof guidance.
/// </summary>
[CustomEditor(typeof(EnemyAI))]
public class EnemyAIEditor : Editor
{
    private SerializedObject _detectionSerialized;
    private SerializedObject _combatSerialized;

    private SerializedProperty _aggroRange;
    private SerializedProperty _leashRange;
    private SerializedProperty _obstacleMask;
    private SerializedProperty _requireLineOfSight;
    private SerializedProperty _targetOverride;

    private SerializedProperty _hitBoxZones;
    private SerializedProperty _attackSequence;
    private SerializedProperty _attackMode;
    private SerializedProperty _usePrioritySelection;
    private SerializedProperty _attackPriorityProfile;
    private SerializedProperty _preferAttacksCurrentlyInRange;
    private SerializedProperty _rangeWeight;
    private SerializedProperty _damageWeight;
    private SerializedProperty _knockbackWeight;
    private SerializedProperty _assetPriorityWeight;
    private SerializedProperty _attackCooldown;
    private SerializedProperty _attackRangeOverride;

    private SerializedProperty _movementMode;
    private SerializedProperty _moveSpeed;
    private SerializedProperty _waypointTolerance;

    private SerializedProperty _visualRoot;
    private SerializedProperty _spriteDefaultFacesRight;
    private SerializedProperty _presentationCamera;

    private SerializedProperty _patrolPoints;
    private SerializedProperty _randomPatrolDistance;
    private SerializedProperty _enemyFeatureProfile;

    private void OnEnable()
    {
        EnemyAI ai = (EnemyAI)target;
        var detection = ai.GetComponent<EnemyDetectionModule>();
        var combat = ai.GetComponent<EnemyCombatModule>();

        if (detection != null)
        {
            _detectionSerialized = new SerializedObject(detection);
            _aggroRange = _detectionSerialized.FindProperty("aggroRange");
            _leashRange = _detectionSerialized.FindProperty("leashRange");
            _obstacleMask = _detectionSerialized.FindProperty("obstacleMask");
            _requireLineOfSight = _detectionSerialized.FindProperty("requireLineOfSight");
            _targetOverride = _detectionSerialized.FindProperty("targetOverride");
        }

        if (combat != null)
        {
            _combatSerialized = new SerializedObject(combat);
            _hitBoxZones = _combatSerialized.FindProperty("hitBoxZones");
            _attackSequence = _combatSerialized.FindProperty("attackSequence");
            _attackMode = _combatSerialized.FindProperty("attackMode");
            _usePrioritySelection = _combatSerialized.FindProperty("usePrioritySelection");
            _attackPriorityProfile = _combatSerialized.FindProperty("attackPriorityProfile");
            _preferAttacksCurrentlyInRange = _combatSerialized.FindProperty("preferAttacksCurrentlyInRange");
            _rangeWeight = _combatSerialized.FindProperty("rangeWeight");
            _damageWeight = _combatSerialized.FindProperty("damageWeight");
            _knockbackWeight = _combatSerialized.FindProperty("knockbackWeight");
            _assetPriorityWeight = _combatSerialized.FindProperty("assetPriorityWeight");
            _attackCooldown = _combatSerialized.FindProperty("attackCooldown");
            _attackRangeOverride = _combatSerialized.FindProperty("attackRangeOverride");
        }

        _movementMode = serializedObject.FindProperty("movementMode");
        _moveSpeed = serializedObject.FindProperty("moveSpeed");
        _waypointTolerance = serializedObject.FindProperty("waypointTolerance");

        _visualRoot = serializedObject.FindProperty("visualRoot");
        _spriteDefaultFacesRight = serializedObject.FindProperty("spriteDefaultFacesRight");
        _presentationCamera = serializedObject.FindProperty("presentationCamera");

        _patrolPoints = serializedObject.FindProperty("patrolPoints");
        _randomPatrolDistance = serializedObject.FindProperty("randomPatrolDistance");
        _enemyFeatureProfile = serializedObject.FindProperty("enemyFeatureProfile");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _detectionSerialized?.Update();
        _combatSerialized?.Update();

        bool is3D = _movementMode.enumValueIndex == (int)MovementMode.ThreeD;
        PyralisInspectorHandoff.DrawAuthoringButton(
            "Enemy AI",
            "Edit detection, movement, combat, and feature fields here. Use Pyralis Authoring for route setup and proof guidance.");

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
                ? "ThreeD chases on the XZ plane for brawlers, arena games, or enemies with depth movement."
                : "TwoD chases on the X axis for side-scrollers. Use a flat 3D ground collider under the play space.",
            MessageType.Info);
        EditorGUILayout.Space(4f);

        if (is3D)
        {
            EditorGUILayout.LabelField("Visuals (3D Brawler)", EditorStyles.boldLabel);
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

        DrawCombat();

        serializedObject.ApplyModifiedProperties();
        _detectionSerialized?.ApplyModifiedProperties();
        _combatSerialized?.ApplyModifiedProperties();
    }

    private void DrawCombat()
    {
        EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(_hitBoxZones, new GUIContent("Hit Box Zones"), true);
        EditorGUILayout.Space(2f);
        EditorGUILayout.PropertyField(_attackSequence, new GUIContent("Attack Sequence"), true);
        EditorGUILayout.Space(2f);
        EditorGUILayout.PropertyField(_attackMode);
        EditorGUILayout.PropertyField(
            _usePrioritySelection,
            new GUIContent(
                "Use Priority Selection",
                "If enabled, AI picks attacks using selected profile weights."));

        if (_usePrioritySelection.boolValue)
        {
            EditorGUILayout.PropertyField(_attackPriorityProfile);
            EditorGUILayout.PropertyField(_preferAttacksCurrentlyInRange);

            if (_attackPriorityProfile.enumValueIndex == 4)
            {
                EditorGUILayout.PropertyField(_rangeWeight);
                EditorGUILayout.PropertyField(_damageWeight);
                EditorGUILayout.PropertyField(_knockbackWeight);
                EditorGUILayout.PropertyField(_assetPriorityWeight);
            }
        }

        EditorGUILayout.PropertyField(
            _attackCooldown,
            new GUIContent(
                "Attack Cooldown",
                "Fallback interval between attacks. EnemyAttack.attackCooldown overrides this when greater than 0."));

        EditorGUILayout.PropertyField(_attackRangeOverride, new GUIContent("Attack Range Override"));
        if (_attackRangeOverride.floatValue <= 0f)
        {
            EditorGUILayout.HelpBox(
                "Attack Range Override = 0 means range is auto-measured from hitbox collider bounds at Awake. Each EnemyAttack asset can also specify its own range.",
                MessageType.None);
        }

        EditorGUILayout.PropertyField(_targetOverride);
        EditorGUILayout.PropertyField(_enemyFeatureProfile);
    }
}
