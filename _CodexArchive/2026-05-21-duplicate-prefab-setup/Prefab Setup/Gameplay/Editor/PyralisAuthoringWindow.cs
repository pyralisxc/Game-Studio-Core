using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public class PyralisAuthoringWindow : EditorWindow
    {
        [MenuItem("NeonBlack/Gameplay/Pyralis Authoring Window")]
        public static void Open()
        {
            GetWindow<PyralisAuthoringWindow>("Pyralis Authoring");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Create Authoring Assets", EditorStyles.boldLabel);
            DrawCreateButton<SessionDefinition>("Session Definition");
            DrawCreateButton<GameSetupProfile>("Game Setup Profile");
            DrawCreateButton<RuntimePatternDefinition>("Runtime Pattern Definition");
            DrawCreateButton<PawnDefinition>("Pawn Definition");
            DrawCreateButton<GameModeDefinition>("Game Mode Definition");
            DrawCreateButton<FeatureModuleDefinition>("Feature Module Definition");

            EditorGUILayout.Space(12f);
            EditorGUILayout.LabelField("Validate Selection", EditorStyles.boldLabel);

            Object selection = Selection.activeObject;
            if (selection == null)
            {
                EditorGUILayout.HelpBox("Select a Session, Pawn, Game Mode, or Feature Module asset to validate it here.", MessageType.Info);
                return;
            }

            List<string> issues = GetIssues(selection);
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("No validation issues found for the selected asset.", MessageType.Info);
                return;
            }

            for (int i = 0; i < issues.Count; i++)
                EditorGUILayout.HelpBox(issues[i], MessageType.Warning);
        }

        private static void DrawCreateButton<T>(string label) where T : ScriptableObject
        {
            if (!GUILayout.Button($"Create {label}"))
                return;

            string path = EditorUtility.SaveFilePanelInProject(
                $"Create {label}",
                typeof(T).Name,
                "asset",
                $"Choose a location for the new {label}.");
            if (string.IsNullOrWhiteSpace(path))
                return;

            T asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }

        private static List<string> GetIssues(Object selection)
        {
            return selection switch
            {
                SessionDefinition session => GetSessionIssues(session),
                PawnDefinition pawn => pawn.GetValidationIssues(),
                GameModeDefinition mode => mode.GetValidationIssues(),
                FeatureModuleDefinition module => module.GetValidationIssues(),
                RuntimePatternDefinition pattern => pattern.GetValidationIssues(),
                GameSetupProfile setup => setup.GetValidationIssues(),
                _ => new List<string> { "Selected asset does not expose Pyralis validation rules yet." }
            };
        }

        private static List<string> GetSessionIssues(SessionDefinition session)
        {
            var issues = new List<string>();

            if (string.IsNullOrWhiteSpace(session.sessionName))
                issues.Add("Session name is required.");

            if (session.maxParticipants < 1)
                issues.Add("Max participants must be at least 1.");

            if (session.defaultGameMode == null)
                issues.Add("Default game mode is not assigned.");

            if (session.defaultInputProfile == null)
                issues.Add("Default input profile is not assigned.");

            if (session.defaultParticipants == null || session.defaultParticipants.Length == 0)
            {
                issues.Add("At least one default participant should be assigned.");
            }
            else
            {
                for (int i = 0; i < session.defaultParticipants.Length; i++)
                {
                    if (session.defaultParticipants[i] == null)
                        issues.Add($"Default participant slot {i} is empty.");
                }
            }

            return issues;
        }
    }
}
