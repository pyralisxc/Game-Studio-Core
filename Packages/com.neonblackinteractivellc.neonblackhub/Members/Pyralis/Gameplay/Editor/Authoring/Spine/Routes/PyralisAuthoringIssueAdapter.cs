using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringIssueAdapter
    {
        public static PyralisAuthoringIssue Create(PyralisAuthoringValidationIssue issue)
        {
            if (issue == null)
                return null;

            return Create(
                issue.IssueCode,
                issue.Category,
                issue.Problem,
                issue.AffectedMember,
                issue.PrimaryActionLabel,
                issue.Expected,
                issue.Found,
                issue.SuccessLooksLike);
        }

        public static PyralisAuthoringIssue Create(
            string issueCode,
            PyralisAuthoringValidationCategory category,
            string problem,
            string affectedMember,
            string primaryActionLabel,
            string expected = "",
            string found = "",
            string successLooksLike = "")
        {
            PyralisAuthoringNativeAction? nativeAction = CreateNativeAction(
                issueCode,
                category,
                affectedMember,
                primaryActionLabel,
                successLooksLike);

            return new PyralisAuthoringIssue(
                issueCode,
                GetSeverity(issueCode, category),
                GetWorkIntent(issueCode, category),
                GetEvidenceState(issueCode, expected, found),
                GetTargetObject(category),
                affectedMember,
                nativeAction,
                problem);
        }

        public static List<PyralisAuthoringIssue> CreateAll(PyralisAuthoringValidationModel model)
        {
            List<PyralisAuthoringIssue> issues = new List<PyralisAuthoringIssue>();
            if (model == null)
                return issues;

            for (int i = 0; i < model.Issues.Count; i++)
            {
                PyralisAuthoringIssue typedIssue = model.Issues[i]?.TypedIssue;
                if (typedIssue != null)
                    issues.Add(typedIssue);
            }

            return issues;
        }

        private static PyralisAuthoringIssueSeverity GetSeverity(string issueCode, PyralisAuthoringValidationCategory category)
        {
            if (string.IsNullOrWhiteSpace(issueCode))
                return PyralisAuthoringIssueSeverity.Recommended;

            if (issueCode.StartsWith("prefabReadiness.recommended.", System.StringComparison.Ordinal)
                || issueCode.StartsWith("sceneSurface.", System.StringComparison.Ordinal))
            {
                return PyralisAuthoringIssueSeverity.Recommended;
            }

            if (issueCode.StartsWith("route.", System.StringComparison.Ordinal))
                return PyralisAuthoringIssueSeverity.Recommended;

            if (category == PyralisAuthoringValidationCategory.CodeContract)
                return PyralisAuthoringIssueSeverity.Bug;

            return category == PyralisAuthoringValidationCategory.Other
? PyralisAuthoringIssueSeverity.Recommended
                : PyralisAuthoringIssueSeverity.Required;
        }

        private static string GetWorkIntent(string issueCode, PyralisAuthoringValidationCategory category)
        {
            if (!string.IsNullOrWhiteSpace(issueCode)
                && (issueCode.StartsWith("sceneSurface.", System.StringComparison.Ordinal)
                    || issueCode.StartsWith("prefabReadiness.recommended.", System.StringComparison.Ordinal)))
            {
                return PyralisSetupFlowWorkIntent.ProofEnhancer.ToString();
            }

            switch (category)
            {
                case PyralisAuthoringValidationCategory.SceneObjects:
                    return PyralisSetupFlowWorkIntent.ProofEnhancer.ToString();
                case PyralisAuthoringValidationCategory.Other:
                    return PyralisSetupFlowWorkIntent.FeatureCard.ToString();
                default:
                    return PyralisSetupFlowWorkIntent.RequiredSetup.ToString();
            }
        }

        private static PyralisAuthoringEvidenceState GetEvidenceState(string issueCode, string expected, string found)
        {
            if (!string.IsNullOrWhiteSpace(issueCode)
                && issueCode.StartsWith("prefabReadiness.", System.StringComparison.Ordinal))
            {
                return PyralisAuthoringEvidenceState.CandidateDetected;
            }

            if (!string.IsNullOrWhiteSpace(found))
                return PyralisAuthoringEvidenceState.Missing;

            if (!string.IsNullOrWhiteSpace(expected))
                return PyralisAuthoringEvidenceState.Missing;

            return PyralisAuthoringEvidenceState.Missing;
        }

        private static PyralisAuthoringNativeAction? CreateNativeAction(
            string issueCode,
            PyralisAuthoringValidationCategory category,
            string affectedMember,
            string primaryActionLabel,
            string successLooksLike)
        {
            PyralisAuthoringActionSurface surface = category == PyralisAuthoringValidationCategory.SceneObjects
                ? PyralisAuthoringActionSurface.Hierarchy
                : PyralisAuthoringActionSurface.Inspector;

            string verb = string.IsNullOrWhiteSpace(primaryActionLabel) ? "Inspect" : primaryActionLabel;
            string target = GetTargetObject(category);
            string field = string.IsNullOrWhiteSpace(affectedMember) ? "the affected field or component" : affectedMember;
            string success = string.IsNullOrWhiteSpace(successLooksLike) ? "the issue no longer appears in Validate" : successLooksLike;

            if (!string.IsNullOrWhiteSpace(issueCode)
                && issueCode.StartsWith("sceneSurface.", System.StringComparison.Ordinal))
            {
                verb = "Create or link";
            }

            return new PyralisAuthoringNativeAction(verb, surface, target, field, success);
        }

        private static string GetTargetObject(PyralisAuthoringValidationCategory category)
        {
            switch (category)
            {
                case PyralisAuthoringValidationCategory.SessionSetup:
                    return "Session setup chain";
                case PyralisAuthoringValidationCategory.GameRules:
                    return "GameModeDefinition";
                case PyralisAuthoringValidationCategory.SetupProfile:
                    return "GameSetupProfile";
                case PyralisAuthoringValidationCategory.PlayersSeats:
                    return "ParticipantDefinition or SessionDefinition";
                case PyralisAuthoringValidationCategory.PawnsActors:
                    return "PawnDefinition or pawn prefab";
                case PyralisAuthoringValidationCategory.SceneObjects:
                    return "scene Hierarchy";
                default:
                    return "selected authoring object";
            }
        }
    }

}
