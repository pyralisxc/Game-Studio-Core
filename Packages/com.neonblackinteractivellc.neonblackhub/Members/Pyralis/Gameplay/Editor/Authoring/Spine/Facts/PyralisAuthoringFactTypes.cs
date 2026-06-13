using System;
using System.Collections.Generic;
using System.Reflection;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringFactKind
    {
        RouteFamily,
        RouteIntent,
        RuntimeCapability,
        FeatureContract,
        SetupNode,
        Definition,
        Profile,
        SceneComponent,
        UnitySurface,
        AssignmentField,
        CustomizationMoment,
        Issue,
        Proof
    }

    public enum PyralisAuthoringFactSourceKind
    {
        HandAuthoredGuideCard,
        SetupFlow,
        FeatureContract,
        Validator,
        InspectorGuide,
        Reflection,
        Convention,
        SceneEvidence
    }

    public enum PyralisAuthoringIssueSeverity
    {
        Info,
        Optional,
        Recommended,
        Warning,
        Required,
        Blocked,
        Bug
    }

    public sealed class PyralisAuthoringFact
    {
public PyralisAuthoringFact(
            string stableId,
            string displayName,
            PyralisAuthoringFactKind kind,
            PyralisAuthoringFactSourceKind sourceKind,
            PyralisAuthoringConfidence confidence,
            string summary,
            string routeRelevance,
            string firstProof,
            string[] goalTags = null,
            string[] laneTags = null,
            string[] unsupportedLaneTags = null,
            string[] requiredDefinitions = null,
            string[] requiredProfiles = null,
            string[] requiredSceneComponents = null,
            string[] requiredUnitySurfaces = null,
            string[] assignmentFields = null,
            string[] customizationMoments = null,
            string[] canWait = null,
            PyralisAuthoringNativeAction[] nativeActions = null,
            string workIntent = null,
            string[] relatedStableIds = null,
            AuthoringWorldAxiom axioms = AuthoringWorldAxiom.None,
            AuthoringCapability capability = AuthoringCapability.None,
            AuthoringPriority priority = AuthoringPriority.Unspecified,
            int priorityValueOverride = 0,
            string deprecatedInVersion = null,
            string removableInVersion = null,
            string documentationURL = null,
            string expertAdvice = null)
        {
            StableId = stableId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Kind = kind;
            SourceKind = sourceKind;
            Confidence = confidence;
            Summary = summary ?? string.Empty;
            RouteRelevance = routeRelevance ?? string.Empty;
            FirstProof = firstProof ?? string.Empty;
            GoalTags = goalTags ?? System.Array.Empty<string>();
            LaneTags = laneTags ?? System.Array.Empty<string>();
            UnsupportedLaneTags = unsupportedLaneTags ?? System.Array.Empty<string>();
            RequiredDefinitions = requiredDefinitions ?? System.Array.Empty<string>();
            RequiredProfiles = requiredProfiles ?? System.Array.Empty<string>();
            RequiredSceneComponents = requiredSceneComponents ?? System.Array.Empty<string>();
            RequiredUnitySurfaces = requiredUnitySurfaces ?? System.Array.Empty<string>();
            AssignmentFields = assignmentFields ?? System.Array.Empty<string>();
            CustomizationMoments = customizationMoments ?? System.Array.Empty<string>();
            CanWait = canWait ?? System.Array.Empty<string>();
            NativeActions = nativeActions ?? System.Array.Empty<PyralisAuthoringNativeAction>();
            WorkIntent = workIntent ?? string.Empty;
            RelatedStableIds = relatedStableIds ?? System.Array.Empty<string>();
            Axioms = axioms;
            Capability = capability;
            PriorityValueOverride = priorityValueOverride;
            DeprecatedInVersion = deprecatedInVersion ?? string.Empty;
            RemovableInVersion = removableInVersion ?? string.Empty;
            Priority = (int)priority;
            DocumentationURL = documentationURL ?? string.Empty;
            ExpertAdvice = expertAdvice ?? string.Empty;
        }

        public string StableId { get; }
        public string DisplayName { get; }
        public PyralisAuthoringFactKind Kind { get; }
        public PyralisAuthoringFactSourceKind SourceKind { get; }
        public PyralisAuthoringConfidence Confidence { get; }
        public string Summary { get; }
        public string RouteRelevance { get; }
        public string FirstProof { get; }
        public string[] GoalTags { get; }
        public string[] LaneTags { get; }
        public string[] UnsupportedLaneTags { get; }
        public string[] RequiredDefinitions { get; }
        public string[] RequiredProfiles { get; }
        public string[] RequiredSceneComponents { get; }
        public string[] RequiredUnitySurfaces { get; }
        public string[] AssignmentFields { get; }
        public string[] CustomizationMoments { get; }
        public string[] CanWait { get; }
        public PyralisAuthoringNativeAction[] NativeActions { get; }
        public string WorkIntent { get; }
        public string[] RelatedStableIds { get; }
        public AuthoringWorldAxiom Axioms { get; }
        public AuthoringCapability Capability { get; }
        public int Priority { get; }
        public int PriorityValueOverride { get; }
        public string DeprecatedInVersion { get; }
        public string RemovableInVersion { get; }
        public string DocumentationURL { get; }
        public string ExpertAdvice { get; }

        public bool MatchesStableId(string stableId)
        {
            return string.Equals(StableId, stableId, System.StringComparison.Ordinal);
        }

        public bool HasGoal(string goal)
        {
            if (GoalTags == null || string.IsNullOrEmpty(goal)) return false;
            for (int i = 0; i < GoalTags.Length; i++)
            {
                string tag = GoalTags[i];
                if (string.Equals(tag, goal, StringComparison.OrdinalIgnoreCase)) return true;

                // Hierarchical match: 
                // - A tag like 'Combat/Reaction' matches a search for 'Combat'
                // - A tag like 'Combat' matches a search for 'Combat/Reaction' (as a parent category)
                if (tag.StartsWith(goal + "/", StringComparison.OrdinalIgnoreCase) ||
                    goal.StartsWith(tag + "/", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Splits a goal string into its hierarchical parts (e.g., "Combat/Reaction" -> ["Combat", "Reaction"]).
        /// </summary>
        public static string[] GetGoalPathParts(string goal)
        {
            if (string.IsNullOrEmpty(goal)) return Array.Empty<string>();
            return goal.Split('/', StringSplitOptions.RemoveEmptyEntries);
        }

        public bool HasLane(string lane)
        {
            if (LaneTags == null) return false;
            for (int i = 0; i < LaneTags.Length; i++)
                if (string.Equals(LaneTags[i], lane, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public bool IsExplicitlyUnsupported(string lane)
        {
            if (UnsupportedLaneTags == null) return false;
            for (int i = 0; i < UnsupportedLaneTags.Length; i++)
                if (string.Equals(UnsupportedLaneTags[i], lane, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }

    public sealed class PyralisAuthoringIssue
    {
        public PyralisAuthoringIssue(
            string issueCode,
            PyralisAuthoringIssueSeverity severity,
            string workIntent,
            PyralisAuthoringEvidenceState evidenceState,
            string targetObject,
            string fieldOrComponent,
            PyralisAuthoringNativeAction? nativeAction,
            string reason)
        {
            IssueCode = issueCode ?? string.Empty;
            Severity = severity;
            WorkIntent = workIntent ?? string.Empty;
            EvidenceState = evidenceState;
            TargetObject = targetObject ?? string.Empty;
            FieldOrComponent = fieldOrComponent ?? string.Empty;
            NativeAction = nativeAction;
            Reason = reason ?? string.Empty;
        }

        public string IssueCode { get; }
        public PyralisAuthoringIssueSeverity Severity { get; }
        public string WorkIntent { get; }
        public PyralisAuthoringEvidenceState EvidenceState { get; }
        public string TargetObject { get; }
        public string FieldOrComponent { get; }
        public PyralisAuthoringNativeAction? NativeAction { get; }
        public string Reason { get; }
}
}
