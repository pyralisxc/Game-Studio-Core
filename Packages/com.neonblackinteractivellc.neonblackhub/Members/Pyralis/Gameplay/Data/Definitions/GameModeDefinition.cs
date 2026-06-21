using NeonBlack.Gameplay.Data.Definitions.Rules;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    /// <summary>
    /// Data-authored game mode composition and session rules.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Rules, 
        Priority = AuthoringPriority.Primary,
        SetupNodeId = "mode.definition",
        Lane = "Rules",
        Relevance = "Defines the project-owned rules, required feature modules, and scene targets for a gameplay session.",
        AssignmentFields = new[] { nameof(playfieldProfile), nameof(cameraRigProfile), nameof(requiredFeatureModules), nameof(gameplayScene) },
        FirstProof = "Assign this Game Mode Definition to a Session Definition asset.",
        ExpertAdvice = "Start neutral, then enable only the systems this route actually uses. Use Required Feature Modules for project-owned global systems, and assign board or turn-order assets only for tabletop-style routes.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/game-mode"
    )]
[CreateAssetMenu(menuName = "NeonBlack/Definitions/Game Mode Definition", fileName = "GameModeDefinition", order = 10)]
    public class GameModeDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return BuildRuntimeValidationIssues();
        }

        private List<PyralisRuntimeValidationIssue> BuildRuntimeValidationIssues()
        {
            List<PyralisRuntimeValidationIssue> issues = new List<PyralisRuntimeValidationIssue>();

            if (!enableRespawn && startingLives > 0)
            {
                issues.Add(PyralisRuntimeValidationIssue.Required(
                    "Starting lives are only meaningful when respawn is enabled.",
                    nameof(startingLives),
                    nameof(GameModeDefinition),
                    "Set GameModeDefinition.startingLives to 0 or enable respawn for this route.",
                    "GameModeDefinition respawn/lives settings agree.",
                    "GameModeDefinition.StartingLives.RequiresRespawn"));
            }

            AppendNestedRuntimeValidationIssues(issues);
            return issues;
        }

        [Header("Scenes")]
        public string mainMenuScene = "MainMenu";
        public string gameplayScene = "Opening";

        [Header("Profiles")]
        public PlayfieldProfile playfieldProfile;
        public CameraRigProfile cameraRigProfile;

        [Header("Systems")]
        public FeatureModuleDefinition[] requiredFeatureModules;
        public bool enableCombat = false;
        public bool enablePickups = false;
        public bool enableHazards = false;
        public bool enableScore = false;
        public bool enableRespawn = false;

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
            List<PyralisRuntimeValidationIssue> runtimeIssues = BuildRuntimeValidationIssues();
            for (int i = 0; i < runtimeIssues.Count; i++)
            {
                if (runtimeIssues[i] != null && !string.IsNullOrWhiteSpace(runtimeIssues[i].Message))
                    issues.Add(runtimeIssues[i].Message);
            }

            return issues;
        }

        private void AppendNestedRuntimeValidationIssues(List<PyralisRuntimeValidationIssue> issues)
        {
            if (turnOrderDefinition != null)
            {
                foreach (PyralisRuntimeValidationIssue issue in turnOrderDefinition.GetRuntimeValidationIssues())
                {
                    AddChildIssue(
                        issues,
                        issue,
                        $"Turn order definition `{turnOrderDefinition.turnOrderId}`: ",
                        "GameModeDefinition.TurnOrder",
                        nameof(turnOrderDefinition));
                }
            }

            if (boardDefinition != null)
            {
                foreach (PyralisRuntimeValidationIssue issue in boardDefinition.GetRuntimeValidationIssues())
                {
                    AddChildIssue(
                        issues,
                        issue,
                        $"Board definition `{boardDefinition.boardId}`: ",
                        "GameModeDefinition.Board",
                        nameof(boardDefinition));
                }
            }

            if (boardTerminalConditions != null)
            {
                HashSet<string> terminalConditionIds = new HashSet<string>();
                for (int i = 0; i < boardTerminalConditions.Length; i++)
                {
                    BoardTerminalConditionDefinition condition = boardTerminalConditions[i];
                    if (condition == null)
                    {
                        issues.Add(PyralisRuntimeValidationIssue.Required(
                            $"Board terminal condition[{i}] is null.",
                            $"{nameof(boardTerminalConditions)}[{i}]",
                            nameof(GameModeDefinition),
                            "Assign a BoardTerminalConditionDefinition or remove the empty array entry.",
                            "GameModeDefinition board terminal conditions contain no empty entries.",
                            "GameModeDefinition.BoardTerminalCondition.Null"));
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(condition.conditionId) && !terminalConditionIds.Add(condition.conditionId))
                    {
                        issues.Add(PyralisRuntimeValidationIssue.Required(
                            $"Board terminal condition `{condition.conditionId}` is assigned more than once.",
                            nameof(boardTerminalConditions),
                            nameof(GameModeDefinition),
                            "Remove the duplicate board terminal condition or give each condition a unique id.",
                            "GameModeDefinition board terminal condition ids are unique.",
                            "GameModeDefinition.BoardTerminalCondition.Duplicate"));
                    }

                    foreach (PyralisRuntimeValidationIssue issue in condition.GetRuntimeValidationIssues())
                    {
                        AddChildIssue(
                            issues,
                            issue,
                            $"Board terminal condition `{condition.conditionId}`: ",
                            "GameModeDefinition.BoardTerminalCondition." + GetSafeIssueSegment(condition.conditionId),
                            $"{nameof(boardTerminalConditions)}[{i}]");
                    }
                }
            }

            HashSet<string> moduleIds = new HashSet<string>();
            if (requiredFeatureModules == null)
                return;

            for (int i = 0; i < requiredFeatureModules.Length; i++)
            {
                FeatureModuleDefinition module = requiredFeatureModules[i];
                if (module == null)
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        $"Required Feature Modules[{i}] is null.",
                        $"{nameof(requiredFeatureModules)}[{i}]",
                        nameof(GameModeDefinition),
                        "Assign a FeatureModuleDefinition or remove the empty array entry.",
                        "GameModeDefinition required feature modules contain no empty entries.",
                        "GameModeDefinition.RequiredFeatureModule.Null"));
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(module.moduleId) && !moduleIds.Add(module.moduleId))
                {
                    issues.Add(PyralisRuntimeValidationIssue.Required(
                        $"Required feature module `{module.moduleId}` is assigned more than once.",
                        nameof(requiredFeatureModules),
                        nameof(GameModeDefinition),
                        "Remove the duplicate required feature module or give each module a unique id.",
                        "GameModeDefinition required feature module ids are unique.",
                        "GameModeDefinition.RequiredFeatureModule.Duplicate"));
                }

                foreach (PyralisRuntimeValidationIssue issue in module.GetRuntimeValidationIssues())
                {
                    AddChildIssue(
                        issues,
                        issue,
                        $"Required feature `{module.moduleId}`: ",
                        "GameModeDefinition.RequiredFeatureModule." + GetSafeIssueSegment(module.moduleId),
                        $"{nameof(requiredFeatureModules)}[{i}]");
                }
            }
        }

        private static void AddChildIssue(
            List<PyralisRuntimeValidationIssue> issues,
            PyralisRuntimeValidationIssue issue,
            string messagePrefix,
            string issueCodePrefix,
            string fieldPath)
        {
            PyralisRuntimeValidationIssue contextualIssue =
                PyralisRuntimeValidationIssueUtility.WithParentContext(
                    issue,
                    messagePrefix,
                    issueCodePrefix,
                    fieldPath,
                    nameof(GameModeDefinition),
                    "Open the referenced GameModeDefinition child asset and resolve the named issue.",
                    "GameModeDefinition child definitions report no validation issues.");

            if (contextualIssue != null && !string.IsNullOrWhiteSpace(contextualIssue.Message))
                issues.Add(contextualIssue);
        }

        private static string GetSafeIssueSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unnamed";

            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
