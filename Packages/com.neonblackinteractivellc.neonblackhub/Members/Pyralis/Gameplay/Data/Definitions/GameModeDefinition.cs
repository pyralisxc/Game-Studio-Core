using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Data-authored game mode composition and session rules.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Session, 
        Relevance = "Defines the specific game rules, required features, and scene setup for a gameplay session.",
        AssignmentFields = new[] { nameof(setupProfile), nameof(cameraRigProfile), nameof(requiredFeatureModules), nameof(boardDefinition), nameof(turnOrderDefinition) },
        FirstProof = "Assign this Game Mode Definition to a Session Definition asset."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Game Mode Definition", fileName = "GameModeDefinition", order = 10)]
    public class GameModeDefinition : ScriptableObject
    {
        [Header("Scenes")]
        public string mainMenuScene = "MainMenu";
        public string gameplayScene = "Opening";

        [Header("Profiles")]
        public GameSetupProfile setupProfile;
        public PlayfieldProfile playfieldProfile;
        public CameraRigProfile cameraRigProfile;

        [Header("Systems")]
        public FeatureModuleDefinition[] requiredFeatureModules;
        public bool enableCombat = true;
        public bool enablePickups = false;
        public bool enableHazards = false;
        public bool enableScore = false;
        public bool enableRespawn = true;

        [Header("Rules")]
        public TurnOrderDefinition turnOrderDefinition;
        public BoardDefinition boardDefinition;
        public BoardTerminalConditionDefinition[] boardTerminalConditions;
        public float respawnDelay = 3f;
        public int startingLives = 0;
        public int maxParticipantsOverride = 0;

        public void Sanitize()
        {
            respawnDelay = Mathf.Max(0f, respawnDelay);
            startingLives = Mathf.Max(0, startingLives);
            maxParticipantsOverride = Mathf.Max(0, maxParticipantsOverride);
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (!enableRespawn && startingLives > 0)
                issues.Add("Starting lives are only meaningful when respawn is enabled.");

            if (turnOrderDefinition != null)
            {
                List<string> turnIssues = turnOrderDefinition.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < turnIssues.Count; issueIndex++)
                    issues.Add($"Turn order definition `{turnOrderDefinition.turnOrderId}`: {turnIssues[issueIndex]}");
            }

            if (boardDefinition != null)
            {
                List<string> boardIssues = boardDefinition.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < boardIssues.Count; issueIndex++)
                    issues.Add($"Board definition `{boardDefinition.boardId}`: {boardIssues[issueIndex]}");
            }

            if (boardTerminalConditions != null)
            {
                HashSet<string> terminalConditionIds = new HashSet<string>();
                for (int i = 0; i < boardTerminalConditions.Length; i++)
                {
                    BoardTerminalConditionDefinition condition = boardTerminalConditions[i];
                    if (condition == null)
                    {
                        issues.Add($"Board terminal condition[{i}] is null.");
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(condition.conditionId) && !terminalConditionIds.Add(condition.conditionId))
                        issues.Add($"Board terminal condition `{condition.conditionId}` is assigned more than once.");

                    List<string> conditionIssues = condition.GetValidationIssues();
                    for (int issueIndex = 0; issueIndex < conditionIssues.Count; issueIndex++)
                        issues.Add($"Board terminal condition `{condition.conditionId}`: {conditionIssues[issueIndex]}");
                }
            }

            if (setupProfile == null)
            {
                issues.Add("Setup profile is not assigned.");
            }
            else
            {
                List<string> setupIssues = setupProfile.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < setupIssues.Count; issueIndex++)
                    issues.Add($"Setup profile `{setupProfile.setupName}`: {setupIssues[issueIndex]}");
            }

            HashSet<string> moduleIds = new HashSet<string>();
            if (requiredFeatureModules == null)
                return issues;

            for (int i = 0; i < requiredFeatureModules.Length; i++)
            {
                FeatureModuleDefinition module = requiredFeatureModules[i];
                if (module == null)
                {
                    issues.Add($"Required Feature Modules[{i}] is null.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(module.moduleId) && !moduleIds.Add(module.moduleId))
                    issues.Add($"Required feature module `{module.moduleId}` is assigned more than once.");

                List<string> moduleIssues = module.GetValidationIssues();
                for (int issueIndex = 0; issueIndex < moduleIssues.Count; issueIndex++)
                    issues.Add($"Required feature `{module.moduleId}`: {moduleIssues[issueIndex]}");
            }

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
