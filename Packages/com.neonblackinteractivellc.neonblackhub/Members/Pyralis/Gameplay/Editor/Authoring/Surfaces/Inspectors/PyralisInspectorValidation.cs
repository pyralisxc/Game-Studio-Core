using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using System.Text;
using NeonBlack.Gameplay.Features.Composition;
using UnityEditor;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public enum PyralisInspectorValidationIssueSeverity
    {
        RequiredFix,
        Recommended,
        Optional
    }

    public readonly struct PyralisInspectorValidationIssue
    {
        public readonly string Message;
        public readonly PyralisInspectorValidationIssueSeverity Severity;

        public PyralisInspectorValidationIssue(string message, PyralisInspectorValidationIssueSeverity severity = PyralisInspectorValidationIssueSeverity.RequiredFix)
        {
            Message = message;
            Severity = severity;
        }

        public static PyralisInspectorValidationIssue Required(string message)
        {
            return new PyralisInspectorValidationIssue(message, PyralisInspectorValidationIssueSeverity.RequiredFix);
        }

        public static PyralisInspectorValidationIssue Recommended(string message)
        {
            return new PyralisInspectorValidationIssue(message, PyralisInspectorValidationIssueSeverity.Recommended);
        }

        public static PyralisInspectorValidationIssue Optional(string message)
        {
            return new PyralisInspectorValidationIssue(message, PyralisInspectorValidationIssueSeverity.Optional);
        }
    }

    public static class PyralisInspectorValidation
    {
        public static void DrawValidationIssues(IReadOnlyList<string> issues, string readyMessage = "No setup issues found.")
        {
            if (issues == null || issues.Count == 0)
            {
                EditorGUILayout.HelpBox(readyMessage, MessageType.Info);
                return;
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < issues.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(issues[i]))
                    continue;

                builder.Append("- ");
                builder.AppendLine(issues[i]);
            }

            if (builder.Length > 0)
                EditorGUILayout.HelpBox(builder.ToString().Trim(), MessageType.Warning);
        }

        public static void DrawValidationMessages(IReadOnlyList<PyralisInspectorValidationIssue> issues, string readyMessage = "No setup issues found.")
        {
            if (issues == null || issues.Count == 0)
            {
                EditorGUILayout.HelpBox(readyMessage, MessageType.Info);
                return;
            }

            StringBuilder required = new StringBuilder();
            StringBuilder recommended = new StringBuilder();
            StringBuilder optional = new StringBuilder();

            for (int i = 0; i < issues.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(issues[i].Message))
                    continue;

                StringBuilder target = issues[i].Severity switch
                {
                    PyralisInspectorValidationIssueSeverity.RequiredFix => required,
                    PyralisInspectorValidationIssueSeverity.Recommended => recommended,
                    _ => optional
                };

                target.Append("- ");
                target.AppendLine(issues[i].Message);
            }

            if (required.Length > 0)
                EditorGUILayout.HelpBox("Required fixes\n" + required.ToString().Trim(), MessageType.Error);
            if (recommended.Length > 0)
                EditorGUILayout.HelpBox("Recommended checks\n" + recommended.ToString().Trim(), MessageType.Warning);
            if (optional.Length > 0)
                EditorGUILayout.HelpBox("Optional context\n" + optional.ToString().Trim(), MessageType.Info);
        }
    }

    public static class ResolvedAuthoringContractInspectorText
    {
        public static string FeatureModuleSetup(IFeatureModuleRuntime runtime)
        {
            return FeatureModuleSetup(runtime != null ? runtime.ModuleId : null);
        }

        public static string FeatureModuleSetup(string moduleId)
        {
            NeonBlack.Gameplay.Core.Contracts.ResolvedAuthoringContract contract = NeonBlack.Gameplay.Core.Contracts.ResolvedAuthoringContractRegistry.FindByModuleId(moduleId);
if (contract == null)
                return string.IsNullOrWhiteSpace(moduleId)
                    ? "Use a FeatureModuleDefinition whose module id matches this feature runtime."
                    : "Use a FeatureModuleDefinition with module id `" + moduleId + "`.";

            string profileName = RequiredProfileName(contract, null);
            if (string.IsNullOrWhiteSpace(profileName))
                return "Use a FeatureModuleDefinition with module id `" + contract.StableId + "`.";

            return "Use a FeatureModuleDefinition with module id `" + contract.StableId + "` and a " + profileName + ".";
        }

        public static string RequiredProfileName(IFeatureModuleRuntime runtime, string fallback)
        {
            string moduleId = runtime != null ? runtime.ModuleId : null;
            NeonBlack.Gameplay.Core.Contracts.ResolvedAuthoringContract contract = NeonBlack.Gameplay.Core.Contracts.ResolvedAuthoringContractRegistry.FindByModuleId(moduleId);
return RequiredProfileName(contract, fallback);
        }

        public static string RequiredProfileName(string moduleId, string fallback)
        {
            NeonBlack.Gameplay.Core.Contracts.ResolvedAuthoringContract contract = NeonBlack.Gameplay.Core.Contracts.ResolvedAuthoringContractRegistry.FindByModuleId(moduleId);
return RequiredProfileName(contract, fallback);
        }

        private static string RequiredProfileName(NeonBlack.Gameplay.Core.Contracts.ResolvedAuthoringContract contract, string fallback)
        {
            if (contract != null && contract.RequiredProfileType != null)
                return contract.RequiredProfileType.Name;

            return fallback ?? string.Empty;
        }
    }
}
