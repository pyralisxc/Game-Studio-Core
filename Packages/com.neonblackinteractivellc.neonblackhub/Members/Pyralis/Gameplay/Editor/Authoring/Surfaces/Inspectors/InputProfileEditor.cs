using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(InputProfile))]
    public sealed class InputProfileEditor : PyralisBaseEditor
    {
        protected override void DrawCustomInspector()
        {
            base.DrawCustomInspector();
            DrawInputActionSync();
        }

        private void DrawInputActionSync()
        {
            InputProfile profile = (InputProfile)target;

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Input Actions Sync", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(
                    "Use Unity's Input Action Asset as the binding source. This sync only updates InputProfile gameplay action rows; it does not edit the Input Action Asset.",
                    EditorStyles.wordWrappedMiniLabel);

                using (new EditorGUI.DisabledScope(profile.actions == null))
                {
                    if (GUILayout.Button("Sync Action Names From Asset"))
                    {
                        Undo.RecordObject(profile, "Sync InputProfile From Input Actions");
                        bool changed = InputProfileInputActionSync.SyncFromAssignedActions(profile, includeCustomActions: true, out string summary);
                        if (changed)
                        {
                            EditorUtility.SetDirty(profile);
                            serializedObject.Update();
                        }

                        EditorUtility.DisplayDialog("InputProfile Sync", summary, "OK");
                    }
                }

                if (profile.actions == null)
                    EditorGUILayout.HelpBox("Assign an Input Action Asset before syncing.", MessageType.Info);
            }
        }
    }

    public static class InputProfileInputActionSync
    {
        private static readonly GameplayInputActionRole[] BuiltInRoles =
        {
            GameplayInputActionRole.Move,
            GameplayInputActionRole.Look,
            GameplayInputActionRole.Jump,
            GameplayInputActionRole.Dash,
            GameplayInputActionRole.AttackPrimary,
            GameplayInputActionRole.AttackSecondary,
            GameplayInputActionRole.Interact,
            GameplayInputActionRole.Block,
            GameplayInputActionRole.Sprint,
            GameplayInputActionRole.Crouch,
            GameplayInputActionRole.Previous,
            GameplayInputActionRole.Next,
            GameplayInputActionRole.Roll,
            GameplayInputActionRole.LookAround
        };

        public static bool SyncFromAssignedActions(InputProfile profile, bool includeCustomActions, out string summary)
        {
            summary = "No InputProfile was selected.";
            if (profile == null)
                return false;

            InputActionAsset asset = profile.actions;
            if (asset == null)
            {
                summary = "Assign an Input Action Asset before syncing.";
                return false;
            }

            InputActionMap map = ResolveActionMap(profile, asset, out bool primaryMapChanged);
            if (map == null)
            {
                summary = "The assigned Input Action Asset has no action maps.";
                return false;
            }

            List<GameplayInputActionBinding> next = new List<GameplayInputActionBinding>();
            HashSet<string> consumedActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> emittedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            GameplayInputActionBinding[] existing = profile.actionBindings ?? Array.Empty<GameplayInputActionBinding>();
            int updated = 0;
            int added = 0;

            for (int i = 0; i < existing.Length; i++)
            {
                GameplayInputActionBinding binding = existing[i];
                if (binding == null)
                    continue;

                GameplayInputActionBinding copy = CopyBinding(binding);
                if (copy.role != GameplayInputActionRole.Custom)
                {
                    InputAction action = FindActionForRole(map, copy.role);
                    if (action != null)
                    {
                        if (!string.Equals(copy.actionName, action.name, StringComparison.Ordinal)
                            || copy.valueType != GetValueType(action)
                            || !string.IsNullOrWhiteSpace(copy.actionMap))
                        {
                            updated++;
                        }

                        ApplyAction(copy, map, action);
                        consumedActions.Add(action.name);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(copy.actionName) && map.FindAction(copy.actionName, false) != null)
                {
                    consumedActions.Add(copy.actionName);
                }

                copy.Sanitize(profile.primaryActionMap);
                if (!emittedKeys.Add(copy.EffectiveKey))
                    continue;

                next.Add(copy);
            }

            for (int i = 0; i < BuiltInRoles.Length; i++)
            {
                GameplayInputActionRole role = BuiltInRoles[i];
                if (HasRole(next, role))
                    continue;

                InputAction action = FindActionForRole(map, role);
                if (action == null)
                    continue;

                GameplayInputActionBinding binding = GameplayInputActionBinding.BuiltIn(
                    role,
                    action.name,
                    GetValueType(action),
                    role == GameplayInputActionRole.Move);
                ApplyAction(binding, map, action);
                binding.Sanitize(profile.primaryActionMap);
                next.Add(binding);
                consumedActions.Add(action.name);
                added++;
            }

            if (includeCustomActions)
            {
                foreach (InputAction action in map.actions)
                {
                    if (action == null || consumedActions.Contains(action.name) || HasCustomAction(next, action.name))
                        continue;

                    GameplayInputActionBinding binding = new GameplayInputActionBinding
                    {
                        role = GameplayInputActionRole.Custom,
                        customKey = action.name,
                        actionName = action.name,
                        valueType = GetValueType(action),
                        requiredForProof = false
                    };
                    ApplyAction(binding, map, action);
                    binding.Sanitize(profile.primaryActionMap);
                    next.Add(binding);
                    added++;
                }
            }

            bool changed = primaryMapChanged || updated > 0 || added > 0 || !BindingsEqual(existing, next);
            if (changed)
            {
                profile.actionBindings = next.ToArray();
                profile.Sanitize();
            }

            summary = changed
                ? $"Synced `{map.name}` from `{asset.name}`. Updated {updated} row(s), added {added} row(s)."
                : $"`{profile.name}` already matches `{map.name}` in `{asset.name}`.";
            return changed;
        }

        private static InputActionMap ResolveActionMap(InputProfile profile, InputActionAsset asset, out bool primaryMapChanged)
        {
            primaryMapChanged = false;
            InputActionMap map = !string.IsNullOrWhiteSpace(profile.primaryActionMap)
                ? asset.FindActionMap(profile.primaryActionMap, false)
                : null;
            if (map != null)
                return map;

            if (asset.actionMaps.Count == 0)
                return null;

            map = asset.actionMaps[0];
            if (map != null && !string.Equals(profile.primaryActionMap, map.name, StringComparison.Ordinal))
            {
                profile.primaryActionMap = map.name;
                primaryMapChanged = true;
            }

            return map;
        }

        private static GameplayInputActionBinding CopyBinding(GameplayInputActionBinding source)
        {
            return new GameplayInputActionBinding
            {
                role = source.role,
                customKey = source.customKey,
                actionMap = source.actionMap,
                actionName = source.actionName,
                valueType = source.valueType,
                requiredForProof = source.requiredForProof,
                animationSignalKey = source.animationSignalKey
            };
        }

        private static void ApplyAction(GameplayInputActionBinding binding, InputActionMap map, InputAction action)
        {
            binding.actionMap = string.Empty;
            binding.actionName = action.name;
            binding.valueType = GetValueType(action);
        }

        private static bool HasRole(IReadOnlyList<GameplayInputActionBinding> bindings, GameplayInputActionRole role)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i] != null && bindings[i].role == role)
                    return true;
            }

            return false;
        }

        private static bool HasCustomAction(IReadOnlyList<GameplayInputActionBinding> bindings, string actionName)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                GameplayInputActionBinding binding = bindings[i];
                if (binding == null || binding.role != GameplayInputActionRole.Custom)
                    continue;

                if (string.Equals(binding.actionName, actionName, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(binding.customKey, actionName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool BindingsEqual(IReadOnlyList<GameplayInputActionBinding> existing, IReadOnlyList<GameplayInputActionBinding> next)
        {
            if (existing == null)
                return next == null || next.Count == 0;

            if (next == null || existing.Count != next.Count)
                return false;

            for (int i = 0; i < existing.Count; i++)
            {
                GameplayInputActionBinding left = existing[i];
                GameplayInputActionBinding right = next[i];
                if (left == null || right == null)
                    return left == right;

                if (left.role != right.role
                    || !string.Equals(left.customKey, right.customKey, StringComparison.Ordinal)
                    || !string.Equals(left.actionMap, right.actionMap, StringComparison.Ordinal)
                    || !string.Equals(left.actionName, right.actionName, StringComparison.Ordinal)
                    || left.valueType != right.valueType
                    || left.requiredForProof != right.requiredForProof
                    || !string.Equals(left.animationSignalKey, right.animationSignalKey, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private static InputAction FindActionForRole(InputActionMap map, GameplayInputActionRole role)
        {
            string[] candidates = GetActionNameCandidates(role);
            for (int i = 0; i < candidates.Length; i++)
            {
                InputAction action = map.FindAction(candidates[i], false);
                if (action != null)
                    return action;
            }

            return null;
        }

        private static string[] GetActionNameCandidates(GameplayInputActionRole role)
        {
            return role switch
            {
                GameplayInputActionRole.AttackPrimary => new[] { "AttackPrimary", "PrimaryAttack", "Attack", "Fire" },
                GameplayInputActionRole.AttackSecondary => new[] { "AttackSecondary", "SecondaryAttack", "Kick", "AltFire" },
                GameplayInputActionRole.LookAround => new[] { "LookAround", "FreeLook", "CameraLook" },
                GameplayInputActionRole.Previous => new[] { "Previous", "Prev", "CyclePrevious" },
                GameplayInputActionRole.Next => new[] { "Next", "CycleNext" },
                _ => new[] { role.ToString(), GameplayInputActionBinding.GetDefaultActionName(role) }
            };
        }

        private static GameplayInputValueType GetValueType(InputAction action)
        {
            string expected = action.expectedControlType ?? string.Empty;
            if (expected.IndexOf("Vector2", StringComparison.OrdinalIgnoreCase) >= 0)
                return GameplayInputValueType.Vector2;
            if (expected.IndexOf("Button", StringComparison.OrdinalIgnoreCase) >= 0)
                return GameplayInputValueType.Button;
            if (expected.IndexOf("Axis", StringComparison.OrdinalIgnoreCase) >= 0
                || expected.IndexOf("Float", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return GameplayInputValueType.Axis;
            }

            return action.type == InputActionType.PassThrough
                ? GameplayInputValueType.PassThrough
                : GameplayInputValueType.Button;
        }
    }
}
