using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions
{
    [AuthoringContract(
        Capability = AuthoringCapability.Session | AuthoringCapability.TurnBased, 
        Relevance = "Project-window creation path for one selectable command or resolver-backed action.",
        AssignmentFields = new[] { nameof(actionId), nameof(displayName), nameof(targetRule) },
        FirstProof = "Verify the action is selectable in the character menu or action bar.",
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Definitions/Action Definition", fileName = "ActionDefinition", order = 60)]
    public class ActionDefinition : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            if (string.IsNullOrWhiteSpace(actionId))
                yield return "Action id is required.";

            if (string.IsNullOrWhiteSpace(displayName))
                yield return "Display name is required.";

            if (string.IsNullOrWhiteSpace(actionFamily))
                yield return "Action family is required so tools can group related actions.";

            if (cooldown < 0f)
                yield return "Cooldown cannot be negative.";

            if (resourceCost < 0)
                yield return "Resource cost cannot be negative.";

            List<string> targetIssues = targetRule.GetValidationIssues();
            foreach (var issue in targetIssues)
                yield return issue;
        }

        public string actionId = "action.new";
        public string displayName = "Action";
        public string actionFamily = "General";
        public ActionExecutionTiming executionTiming = ActionExecutionTiming.Immediate;
        public float cooldown;
        public int resourceCost;
        public ActionTargetRule targetRule = ActionTargetRule.None();

        [TextArea(2, 5)]
        public string notes = string.Empty;

        public void Sanitize()
        {
            actionId = !string.IsNullOrWhiteSpace(actionId) ? actionId.Trim() : name;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : actionId;
            actionFamily = !string.IsNullOrWhiteSpace(actionFamily) ? actionFamily.Trim() : "General";
            cooldown = Mathf.Max(0f, cooldown);
            resourceCost = Mathf.Max(0, resourceCost);
            targetRule.Sanitize();
        }

        public List<string> GetValidationIssues()
        {
            var issues = new List<string>();

            if (string.IsNullOrWhiteSpace(actionId))
                issues.Add("Action id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (string.IsNullOrWhiteSpace(actionFamily))
                issues.Add("Action family is required so tools can group related actions.");

            if (cooldown < 0f)
                issues.Add("Cooldown cannot be negative.");

            if (resourceCost < 0)
                issues.Add("Resource cost cannot be negative.");

            List<string> targetIssues = targetRule.GetValidationIssues();
            for (int i = 0; i < targetIssues.Count; i++)
                issues.Add(targetIssues[i]);

            return issues;
        }

        public ActionValidationResult ValidateTargets(ActionExecutionContext context)
        {
            return targetRule.ValidateTargets(context);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
