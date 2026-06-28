using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat.Editor
{
    [CustomEditor(typeof(EnemyCombatModule))]
    public sealed class EnemyCombatModuleEditor : UnityEditor.Editor
    {
        private SerializedProperty _combatProfile;
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

        private void OnEnable()
        {
            _combatProfile = serializedObject.FindProperty("combatProfile");
            _hitBoxZones = serializedObject.FindProperty("hitBoxZones");
            _attackSequence = serializedObject.FindProperty("attackSequence");
            _attackMode = serializedObject.FindProperty("attackMode");
            _usePrioritySelection = serializedObject.FindProperty("usePrioritySelection");
            _attackPriorityProfile = serializedObject.FindProperty("attackPriorityProfile");
            _preferAttacksCurrentlyInRange = serializedObject.FindProperty("preferAttacksCurrentlyInRange");
            _rangeWeight = serializedObject.FindProperty("rangeWeight");
            _damageWeight = serializedObject.FindProperty("damageWeight");
            _knockbackWeight = serializedObject.FindProperty("knockbackWeight");
            _assetPriorityWeight = serializedObject.FindProperty("assetPriorityWeight");
            _attackCooldown = serializedObject.FindProperty("attackCooldown");
            _attackRangeOverride = serializedObject.FindProperty("attackRangeOverride");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Combat Profile", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_combatProfile);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Hitboxes", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_hitBoxZones, new GUIContent("Hit Box Zones"), true);
            EditorGUILayout.Space(4f);

            EditorGUILayout.LabelField("Attack Sequence", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_attackSequence, new GUIContent("Attack Sequence"), true);
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

            serializedObject.ApplyModifiedProperties();
        }
    }
}
