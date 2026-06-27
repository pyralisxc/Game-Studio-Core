using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Core.Actions
{
    [Serializable]
    public struct ActionTargetRule
    {
        public ActionTargetKind targetKind;
        public int minTargets;
        public int maxTargets;
        public float maxRange;
        public bool allowEmptyTargets;
        public bool allowSelfTarget;
        public bool allowFriendlyTargets;
        public bool allowHostileTargets;
        public bool allowNeutralTargets;
        public bool requiresLineOfSight;

        public static ActionTargetRule None()
        {
            return new ActionTargetRule
            {
                targetKind = ActionTargetKind.None,
                allowEmptyTargets = true
            };
        }

        public static ActionTargetRule Single(ActionTargetKind kind)
        {
            return new ActionTargetRule
            {
                targetKind = kind,
                minTargets = 1,
                maxTargets = 1,
                allowSelfTarget = true,
                allowFriendlyTargets = true,
                allowHostileTargets = true,
                allowNeutralTargets = true
            };
        }

        public void Sanitize()
        {
            maxRange = Mathf.Max(0f, maxRange);

            if (targetKind == ActionTargetKind.None)
            {
                minTargets = 0;
                maxTargets = 0;
                allowEmptyTargets = true;
                return;
            }

            maxTargets = Mathf.Max(1, maxTargets);
            minTargets = Mathf.Clamp(minTargets, allowEmptyTargets ? 0 : 1, maxTargets);

            if (!allowFriendlyTargets && !allowHostileTargets && !allowNeutralTargets)
            {
                allowFriendlyTargets = true;
                allowHostileTargets = true;
                allowNeutralTargets = true;
            }
        }

        public List<string> GetValidationIssues()
        {
            var issues = new List<string>();
            ActionTargetRule sanitized = this;
            sanitized.Sanitize();

            if (targetKind != sanitized.targetKind)
                issues.Add("Target kind changed during sanitation.");

            if (targetKind != ActionTargetKind.None && sanitized.maxTargets < 1)
                issues.Add("Targeted actions require at least one possible target.");

            if (targetKind == ActionTargetKind.None && (minTargets > 0 || maxTargets > 0))
                issues.Add("No-target actions should not require target counts.");

            return issues;
        }

        public ActionValidationResult ValidateTargets(ActionExecutionContext context)
        {
            if (context == null)
                return ActionValidationResult.Failure("Action context is required.");

            ActionTargetRule sanitized = this;
            sanitized.Sanitize();

            ActionTargetDescriptor[] targets = context.Targets ?? Array.Empty<ActionTargetDescriptor>();
            int targetCount = CountEffectiveTargets(targets);

            if (sanitized.targetKind == ActionTargetKind.None)
            {
                return targetCount == 0
                    ? ActionValidationResult.Success()
                    : ActionValidationResult.Failure("This action does not accept targets.");
            }

            if (targetCount == 0 && sanitized.allowEmptyTargets)
                return ActionValidationResult.Success();

            if (targetCount < sanitized.minTargets)
                return ActionValidationResult.Failure($"Action requires at least {sanitized.minTargets} target(s).");

            if (sanitized.maxTargets > 0 && targetCount > sanitized.maxTargets)
                return ActionValidationResult.Failure($"Action accepts at most {sanitized.maxTargets} target(s).");

            for (int i = 0; i < targets.Length; i++)
            {
                ActionTargetDescriptor target = targets[i];
                if (target.targetKind == ActionTargetKind.None)
                    continue;

                if (target.targetKind != sanitized.targetKind)
                    return ActionValidationResult.Failure($"Target {i} must be `{sanitized.targetKind}`.");

                if (!sanitized.allowSelfTarget && IsSelfTarget(context, target))
                    return ActionValidationResult.Failure("This action cannot target itself.");

                if (!sanitized.AllowsFaction(context.SourceFaction, target.targetFaction))
                    return ActionValidationResult.Failure("Target faction is not allowed for this action.");

                if (sanitized.maxRange > 0f && context.SourceTransform != null && target.TryGetPosition(out Vector3 targetPosition))
                {
                    float distance = Vector3.Distance(context.SourceTransform.position, targetPosition);
                    if (distance > sanitized.maxRange)
                        return ActionValidationResult.Failure($"Target is out of range ({distance:0.##} > {sanitized.maxRange:0.##}).");
                }
            }

            return ActionValidationResult.Success();
        }

        private bool AllowsFaction(Faction sourceFaction, Faction targetFaction)
        {
            if (targetFaction == Faction.Neutral)
                return allowNeutralTargets;

            if (sourceFaction == Faction.Neutral)
                return allowNeutralTargets || allowHostileTargets || allowFriendlyTargets;

            return targetFaction == sourceFaction ? allowFriendlyTargets : allowHostileTargets;
        }

        private static bool IsSelfTarget(ActionExecutionContext context, ActionTargetDescriptor target)
        {
            if (target.targetObject == null)
                return false;

            return target.targetObject == context.SourceObject || target.targetObject == context.OwnerObject;
        }

        private static int CountEffectiveTargets(ActionTargetDescriptor[] targets)
        {
            int count = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i].targetKind != ActionTargetKind.None)
                    count++;
            }

            return count;
        }
    }
}
