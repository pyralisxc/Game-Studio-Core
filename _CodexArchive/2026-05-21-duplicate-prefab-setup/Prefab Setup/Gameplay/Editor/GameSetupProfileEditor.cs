using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    [CustomEditor(typeof(GameSetupProfile))]
    public class GameSetupProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();

            GameSetupProfile profile = (GameSetupProfile)target;

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Setup Readiness", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Assigned Patterns", CountAssigned(profile.runtimePatterns).ToString());
            EditorGUILayout.LabelField("Requires Pawn", RequiresPawn(profile.runtimePatterns) ? "Yes" : "No");
            EditorGUILayout.LabelField("Supports Non-Pawn Surfaces", SupportsNonPawnSurfaces(profile.runtimePatterns) ? "Yes" : "No");
            EditorGUILayout.LabelField("Uses Projectile Pattern", HasFamily(profile.runtimePatterns, RuntimeCapabilityFamily.GunsProjectiles) ? "Yes" : "No");
            EditorGUILayout.LabelField("Uses Turn/Menu Pattern", HasFamily(profile.runtimePatterns, RuntimeCapabilityFamily.ActionTargeting) ? "Yes" : "No");
            EditorGUILayout.LabelField("Uses Board/Card/Tabletop Pattern", HasFamily(profile.runtimePatterns, RuntimeCapabilityFamily.BoardCardTabletop) ? "Yes" : "No");

            EditorGUILayout.Space(4f);
            DrawPatternSummary(profile.runtimePatterns);

            List<string> issues = profile.GetValidationIssues();
            DrawIssues(issues);

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawPatternSummary(RuntimePatternDefinition[] patterns)
        {
            EditorGUILayout.LabelField("Runtime Patterns", EditorStyles.boldLabel);

            if (patterns == null || patterns.Length == 0)
            {
                EditorGUILayout.HelpBox("Assign at least one runtime pattern before wiring scene systems.", MessageType.Warning);
                return;
            }

            for (int i = 0; i < patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = patterns[i];
                if (pattern == null)
                {
                    EditorGUILayout.LabelField($"Pattern {i}", "(Missing)");
                    continue;
                }

                string label = !string.IsNullOrWhiteSpace(pattern.displayName) ? pattern.displayName : pattern.patternId;
                EditorGUILayout.LabelField(label, $"{pattern.capabilityFamily} / {pattern.participantEmbodiment}");
            }
        }

        private static void DrawIssues(List<string> issues)
        {
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Setup profile is ready for pre-scene planning.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(4f);
            for (int i = 0; i < issues.Count; i++)
                EditorGUILayout.HelpBox(issues[i], MessageType.Warning);
        }

        private static bool RequiresPawn(RuntimePatternDefinition[] patterns)
        {
            if (patterns == null)
                return false;

            for (int i = 0; i < patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = patterns[i];
                if (pattern != null && pattern.participantEmbodiment == ParticipantEmbodimentRequirement.RequiredPawn)
                    return true;
            }

            return false;
        }

        private static bool SupportsNonPawnSurfaces(RuntimePatternDefinition[] patterns)
        {
            if (patterns == null)
                return false;

            for (int i = 0; i < patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = patterns[i];
                if (pattern == null || pattern.supportedControlSurfaces == null)
                    continue;

                for (int surfaceIndex = 0; surfaceIndex < pattern.supportedControlSurfaces.Length; surfaceIndex++)
                {
                    if (pattern.supportedControlSurfaces[surfaceIndex] != RuntimeControlSurface.Pawn)
                        return true;
                }
            }

            return false;
        }

        private static bool HasFamily(RuntimePatternDefinition[] patterns, RuntimeCapabilityFamily family)
        {
            if (patterns == null)
                return false;

            for (int i = 0; i < patterns.Length; i++)
            {
                if (patterns[i] != null && patterns[i].capabilityFamily == family)
                    return true;
            }

            return false;
        }

        private static int CountAssigned(RuntimePatternDefinition[] patterns)
        {
            if (patterns == null)
                return 0;

            int count = 0;
            for (int i = 0; i < patterns.Length; i++)
            {
                if (patterns[i] != null)
                    count++;
            }

            return count;
        }
    }
}
