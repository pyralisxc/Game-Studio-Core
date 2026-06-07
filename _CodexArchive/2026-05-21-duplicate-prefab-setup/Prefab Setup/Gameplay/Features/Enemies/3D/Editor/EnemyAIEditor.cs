using UnityEditor;
using UnityEngine;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Enemies;
using NeonBlack.Gameplay.Presentation.Animation;
using System.Collections.Generic;

/// <summary>
/// Custom Inspector for EnemyAI.
/// Hides visual-root/billboard fields in TwoD mode.
/// Shows mode-appropriate hints.
/// </summary>
[CustomEditor(typeof(EnemyAI))]
public class EnemyAIEditor : Editor
{
    // â”€â”€ Serialized Properties â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    // Detection
    SerializedProperty _aggroRange;
    SerializedProperty _leashRange;
    SerializedProperty _obstacleMask;
    SerializedProperty _requireLineOfSight;

    // Movement
    SerializedProperty _movementMode;
    SerializedProperty _moveSpeed;
    SerializedProperty _gravity;
    SerializedProperty _waypointTolerance;

    // Visuals (ThreeD only)
    SerializedProperty _visualRoot;
    SerializedProperty _spriteDefaultFacesRight;

    // Patrol
    SerializedProperty _patrolPoints;
    SerializedProperty _randomPatrolDistance;

    // Combat
    SerializedProperty _hitBoxZones;
    SerializedProperty _attackSequence;
    SerializedProperty _attackMode;
    SerializedProperty _usePrioritySelection;
    SerializedProperty _attackPriorityProfile;
    SerializedProperty _preferAttacksCurrentlyInRange;
    SerializedProperty _rangeWeight;
    SerializedProperty _damageWeight;
    SerializedProperty _knockbackWeight;
    SerializedProperty _assetPriorityWeight;
    SerializedProperty _attackCooldown;
    SerializedProperty _attackRangeOverride;
    SerializedProperty _playerTag;
    SerializedProperty _enemyFeatureProfile;

    // Ground Check
    SerializedProperty _groundLayer;
    SerializedProperty _groundCheckRadius;

    private void OnEnable()
    {
        _aggroRange             = serializedObject.FindProperty("aggroRange");
        _leashRange             = serializedObject.FindProperty("leashRange");
        _obstacleMask           = serializedObject.FindProperty("obstacleMask");
        _requireLineOfSight     = serializedObject.FindProperty("requireLineOfSight");

        _movementMode           = serializedObject.FindProperty("movementMode");
        _moveSpeed              = serializedObject.FindProperty("moveSpeed");
        _gravity                = serializedObject.FindProperty("gravity");
        _waypointTolerance      = serializedObject.FindProperty("waypointTolerance");

        _visualRoot             = serializedObject.FindProperty("visualRoot");
        _spriteDefaultFacesRight = serializedObject.FindProperty("spriteDefaultFacesRight");

        _patrolPoints           = serializedObject.FindProperty("patrolPoints");
        _randomPatrolDistance   = serializedObject.FindProperty("randomPatrolDistance");

        _hitBoxZones            = serializedObject.FindProperty("hitBoxZones");
        _attackSequence         = serializedObject.FindProperty("attackSequence");
        _attackMode             = serializedObject.FindProperty("attackMode");
        _usePrioritySelection   = serializedObject.FindProperty("usePrioritySelection");
        _attackPriorityProfile  = serializedObject.FindProperty("attackPriorityProfile");
        _preferAttacksCurrentlyInRange = serializedObject.FindProperty("preferAttacksCurrentlyInRange");
        _rangeWeight            = serializedObject.FindProperty("rangeWeight");
        _damageWeight           = serializedObject.FindProperty("damageWeight");
        _knockbackWeight        = serializedObject.FindProperty("knockbackWeight");
        _assetPriorityWeight    = serializedObject.FindProperty("assetPriorityWeight");
        _attackCooldown         = serializedObject.FindProperty("attackCooldown");
        _attackRangeOverride    = serializedObject.FindProperty("attackRangeOverride");
        _playerTag              = serializedObject.FindProperty("playerTag");
        _enemyFeatureProfile    = serializedObject.FindProperty("enemyFeatureProfile");

        _groundLayer            = serializedObject.FindProperty("groundLayer");
        _groundCheckRadius      = serializedObject.FindProperty("groundCheckRadius");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        bool is3D = _movementMode.enumValueIndex == (int)MovementMode.ThreeD;

        // â”€â”€ Detection â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        EditorGUILayout.LabelField("Detection", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_aggroRange);
        EditorGUILayout.PropertyField(_leashRange);
        EditorGUILayout.PropertyField(_obstacleMask);
        EditorGUILayout.PropertyField(_requireLineOfSight);
        EditorGUILayout.Space(4);

        // â”€â”€ Movement â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_movementMode);
        EditorGUILayout.PropertyField(_moveSpeed);
        EditorGUILayout.PropertyField(_gravity);
        EditorGUILayout.PropertyField(_waypointTolerance);

        if (is3D)
        {
            EditorGUILayout.HelpBox(
                "ThreeD: chases on XZ plane (brawler depth). " +
                "Ground must be a 3D collider on the Ground layer.",
                MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "TwoD: chases on X axis only (side-scroller). " +
                "Ground must be a 3D collider on the Ground layer â€” a flat BoxCollider under your tilemap works.",
                MessageType.Info);
        }
        EditorGUILayout.Space(4);

        // â”€â”€ Visuals (ThreeD only) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        if (is3D)
        {
            EditorGUILayout.LabelField("Visuals  (3D Brawler)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_visualRoot);
            EditorGUILayout.PropertyField(_spriteDefaultFacesRight);
        }
        else
        {
            EditorGUILayout.LabelField("Visuals  (hidden â€” not needed in TwoD)",
                EditorStyles.miniLabel);
        }
        EditorGUILayout.Space(4);

        // â”€â”€ Patrol â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        EditorGUILayout.LabelField("Patrol Points  (leave empty for random patrol)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_patrolPoints);
        EditorGUILayout.PropertyField(_randomPatrolDistance);
        EditorGUILayout.Space(4);

        // â”€â”€ Combat â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(_hitBoxZones, new GUIContent("Hit Box Zones"), true);
        EditorGUILayout.Space(2);
        EditorGUILayout.PropertyField(_attackSequence, new GUIContent("Attack Sequence"), true);
        EditorGUILayout.Space(2);
        EditorGUILayout.PropertyField(_attackMode);
        EditorGUILayout.PropertyField(_usePrioritySelection,
            new GUIContent("Use Priority Selection",
                           "If enabled, AI picks attacks using selected profile (damage/knockback/range/priority)."));

        if (_usePrioritySelection.boolValue)
        {
            EditorGUILayout.PropertyField(_attackPriorityProfile);
            EditorGUILayout.PropertyField(_preferAttacksCurrentlyInRange);

            // WeightedScore is enum index 4 in EnemyAI.AttackPriorityProfile.
            if (_attackPriorityProfile.enumValueIndex == 4)
            {
                EditorGUILayout.PropertyField(_rangeWeight);
                EditorGUILayout.PropertyField(_damageWeight);
                EditorGUILayout.PropertyField(_knockbackWeight);
                EditorGUILayout.PropertyField(_assetPriorityWeight);
            }
        }

        EditorGUILayout.PropertyField(_attackCooldown,
            new GUIContent("Attack Cooldown",
                           "Fallback interval between attacks (seconds). " +
                           "Overridden per-attack by EnemyAttack.attackCooldown when > 0."));

        EditorGUILayout.PropertyField(_attackRangeOverride,
            new GUIContent("Attack Range Override"));
        if (_attackRangeOverride.floatValue <= 0f)
            EditorGUILayout.HelpBox(
                "Attack Range Override = 0 â†’ range is auto-measured from hitbox collider bounds at Awake. " +
                "Each EnemyAttack asset can also specify its own range if > 0.",
                MessageType.None);

        EditorGUILayout.PropertyField(_playerTag);
        EditorGUILayout.PropertyField(_enemyFeatureProfile);
        if (_enemyFeatureProfile.objectReferenceValue is EnemyFeatureProfile featureProfile)
        {
            ActorAnimationDriver animationDriver = ((EnemyAI)target).GetComponent<ActorAnimationDriver>();
            ActorPresentationMode presentationMode = animationDriver != null ? animationDriver.PresentationMode : ActorPresentationMode.Billboard2_5D;
            List<string> issues = featureProfile.GetValidationIssues(((EnemyAI)target).gameObject, presentationMode);
            for (int i = 0; i < issues.Count; i++)
                EditorGUILayout.HelpBox(issues[i], MessageType.Warning);
        }
        EditorGUILayout.Space(4);

        // â”€â”€ Ground Check â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
        EditorGUILayout.LabelField("Ground Check", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_groundLayer);
        EditorGUILayout.PropertyField(_groundCheckRadius);

        serializedObject.ApplyModifiedProperties();
    }
}
