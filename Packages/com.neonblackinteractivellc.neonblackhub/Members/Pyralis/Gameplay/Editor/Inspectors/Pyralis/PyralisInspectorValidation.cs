using NeonBlack.Gameplay.Core.Contracts;
using System.Collections.Generic;
using System.Text;
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

}
