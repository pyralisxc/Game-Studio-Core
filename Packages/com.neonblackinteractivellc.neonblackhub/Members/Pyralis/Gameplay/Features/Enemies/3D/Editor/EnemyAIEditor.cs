using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Enemies;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for EnemyAI. Keeps the runtime fields grouped by setup step
/// and adds beginner-facing path guidance without replacing normal Unity authoring.
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
        DrawGuidance(is3D);

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

    private void DrawGuidance(bool is3D)
    {
        PyralisInspectorGuide.DrawFieldGuide(
            "Inspector Field Guide: Enemy AI",
            new PyralisGuideSection(
                "What this component does",
                "EnemyAI is for enemies that can detect a target, move, patrol, and optionally choose attacks.",
                new[]
                {
                    "Use it for side-scroller enemies, brawler enemies, arena enemies, and simple NPC attackers.",
                    "Skip it for board/card/tabletop pieces unless a piece needs autonomous movement.",
                    "Keep player input, health, hitboxes, score, and UI as separate components so the enemy stays modular."
                },
                PyralisInspectorGuide.SetupManualPath("Prefabs/Enemy_Setup.md")),
            new PyralisGuideSection(
                "Path choices",
                is3D ? "Current path: 3D/brawler enemy." : "Current path: 2D side-scroller enemy.",
                new[]
                {
                    "TwoD path: set Movement Mode to TwoD, tune X-axis chase speed, and use a simple ground collider.",
                    "ThreeD path: set Movement Mode to ThreeD, assign a Visual Root and Presentation Camera when the rendered object is offset from the root.",
                    "Board/card path: keep this component off the piece and drive turns through session, participant, scoring, or custom rules."
                },
                PyralisInspectorGuide.SetupManualPath("RUNTIME_PATTERN_COOKBOOK.md")),
            new PyralisGuideSection(
                "Beginner wiring",
                "Start with the few fields that decide whether the enemy can find, move to, and attack the player.",
                new[]
                {
                    "For one-off scenes, assign Target Override to the player pawn or target object.",
                    "For session scenes, leave Target Override empty and provide the player through participant infrastructure.",
                    "For 3D/billboard enemies, assign Presentation Camera so facing math follows the intended camera rig.",
                    "Set Ground Layer and Ground Check Radius when this enemy relies on grounded movement.",
                    "Assign Hit Box Zones and an Attack Sequence only when this enemy should deal contact or melee damage.",
                    "Assign Enemy Feature Profile when you want inspector validation for presentation, animation, or required child objects."
                },
                PyralisInspectorGuide.SetupManualPath("Prefabs/Health_Combat_Setup.md")),
            new PyralisGuideSection(
                "Combat choices",
                "The attack fields support several genres without forcing one combat model.",
                new[]
                {
                    "Simple brawler/fighter: use Attack Sequence order and per-attack cooldowns.",
                    "Smarter AI: enable Priority Selection and tune range, damage, knockback, and asset priority weights.",
                    "Projectile enemy: call a ProjectileLauncher from the attack event or animation event instead of putting projectile logic here.",
                    "Turn-based/menu combat: use definitions and session rules to select actions, then invoke health/combat services from that action."
                },
                PyralisInspectorGuide.SetupManualPath("Prefabs/Combat_Definitions_Setup.md")),
            new PyralisGuideSection(
                "Common mistakes",
                "If the enemy does nothing, the issue is usually wiring rather than AI logic.",
                new[]
                {
                    "Target Override must be assigned for simple scenes without participant infrastructure.",
                    "Session scenes need a player provider registered by GameplaySessionBootstrap or the gameplay lifetime scope.",
                    "Ground layer must include the collider the enemy stands on.",
                    "Aggro Range must be larger than the distance to the intended target during testing.",
                    "Attack Range Override of 0 means range is measured from hitbox bounds at runtime."
                }));
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
        DrawFeatureProfileValidation();
        DrawEnemyAISetupValidation();
    }

    private void DrawEnemyAISetupValidation()
    {
        EnemyAI enemy = (EnemyAI)target;
        List<string> issues = GetSetupValidationIssues(enemy);
        PyralisInspectorGuide.DrawValidationIssues(issues, "Enemy AI setup looks solid and ready for brawler testing!");
    }

    private List<string> GetSetupValidationIssues(EnemyAI enemy)
    {
        List<string> issues = new List<string>();

        if (enemy == null)
            return issues;

        // 1. HealthComponent presence & faction check
        HealthComponent health = enemy.GetComponent<HealthComponent>();
        if (health == null)
        {
            issues.Add("HealthComponent is missing on this GameObject. Enemies require a HealthComponent component.");
        }
        else if (health.faction != Faction.Enemy)
        {
            issues.Add($"HealthComponent fraction is set to '{health.faction}'. For brawler/NPC setups, enemies should be set to '{Faction.Enemy}' to avoid friendly fire or target selection issues.");
        }

        EnemyCombatModule combat = enemy.GetComponent<EnemyCombatModule>();
        EnemyDetectionModule detection = enemy.GetComponent<EnemyDetectionModule>();

        HitBoxSlot[] slots = combat != null ? (HitBoxSlot[])typeof(EnemyCombatModule).GetField("hitBoxZones", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(combat) : null;
        EnemyAttack[] attacks = combat != null ? (EnemyAttack[])typeof(EnemyCombatModule).GetField("attackSequence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(combat) : null;

        // 2. Hitbox Slots integrity
        HashSet<string> definedZones = new HashSet<string>();
        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                HitBoxSlot slot = slots[i];
                if (slot == null)
                {
                    issues.Add($"Hit Box Zones has an unassigned slot at index {i}.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(slot.zoneName))
                {
                    issues.Add($"Hit Box Zone at index {i} has an empty or whitespace Zone Name.");
                }
                else
                {
                    definedZones.Add(slot.zoneName);
                }

                if (slot.hitBox == null)
                {
                    issues.Add($"Hit Box Zone '{slot.zoneName}' is defined but its HitBox reference is unassigned.");
                }
            }
        }

        // 3. Attack Sequence checking
        if (attacks != null)
        {
            for (int i = 0; i < attacks.Length; i++)
            {
                EnemyAttack attack = attacks[i];
                if (attack == null)
                {
                    issues.Add($"Attack Sequence has an unassigned slot at index {i}.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(attack.hitBoxZone))
                {
                    issues.Add($"Attack '{attack.name}' at index {i} has no target Hit Box Zone assigned.");
                }
                else if (!definedZones.Contains(attack.hitBoxZone))
                {
                    issues.Add($"Attack '{attack.name}' triggers Hit Box Zone '{attack.hitBoxZone}', but no matching slot name exists in the EnemyAI Hit Box Zones list.");
                }
            }
        }

        // 4. Animator verification (and expected parameters check)
        Animator animator = enemy.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            issues.Add("No Animator component found in enemy hierarchy. Animator parameters cannot be driven.");
        }
        else if (animator.runtimeAnimatorController == null)
        {
            issues.Add("Animator component exists but has no RuntimeAnimatorController assigned.");
        }
        else
        {
            var controller = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            if (controller != null)
            {
                HashSet<string> controllerParams = new HashSet<string>();
                foreach (var param in controller.parameters)
                {
                    controllerParams.Add(param.name);
                }

                // Check default expected parameters: State-machine parameters
                string[] requiredParams = { "IsMoving", "IsGrounded", "Death", "Hit" };
                foreach (var req in requiredParams)
                {
                    if (!controllerParams.Contains(req))
                    {
                        issues.Add($"Animator Controller is missing expected state-machine parameter '{req}'.");
                    }
                }

                // Check specific triggers from configured attacks
                if (attacks != null)
                {
                    foreach (var attack in attacks)
                    {
                        if (attack != null && !string.IsNullOrWhiteSpace(attack.animatorTrigger))
                        {
                            if (!controllerParams.Contains(attack.animatorTrigger))
                            {
                                issues.Add($"Animator Controller is missing attack trigger parameter '{attack.animatorTrigger}' defined by attack '{attack.name}'.");
                            }
                        }
                    }
                }
            }
        }

        // 5. Detection / Chase safety limits
        if (detection != null)
        {
            float aggroRange = (float)(typeof(EnemyDetectionModule).GetField("aggroRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(detection) ?? 0f);
            float leashRange = (float)(typeof(EnemyDetectionModule).GetField("leashRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(detection) ?? 0f);

            if (aggroRange >= leashRange)
            {
                issues.Add($"Aggro Range ({aggroRange}) is greater than or equal to Leash Range ({leashRange}). Aggro Range should be smaller than Leash Range to prevent rapid chase-drop loops.");
            }
        }

        return issues;
    }

    private void DrawFeatureProfileValidation()
    {
        if (!(_enemyFeatureProfile.objectReferenceValue is EnemyFeatureProfile featureProfile))
            return;

        EnemyAI enemy = (EnemyAI)target;
        ActorAnimationDriver animationDriver = enemy.GetComponent<ActorAnimationDriver>();
        ActorPresentationMode presentationMode = animationDriver != null
            ? animationDriver.PresentationMode
            : ActorPresentationMode.Billboard2_5D;

        List<string> issues = featureProfile.GetValidationIssues(enemy.gameObject, presentationMode);
        PyralisInspectorGuide.DrawValidationIssues(issues, "Enemy feature profile wiring looks valid for this presentation mode.");
    }
}
