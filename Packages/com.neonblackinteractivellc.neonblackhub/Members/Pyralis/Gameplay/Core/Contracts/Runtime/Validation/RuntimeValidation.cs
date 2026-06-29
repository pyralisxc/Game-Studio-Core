using System.Collections.Generic;
using System.Text;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public enum RuntimeValidationSeverity
    {
        Info,
        Optional,
        Recommended,
        Required
    }

    public sealed class RuntimeValidationIssue
    {
        public RuntimeValidationIssue(
            string message,
            string fieldPath = null,
            string targetLabel = null,
            string nativeAction = null,
            string successCheck = null,
            RuntimeValidationSeverity severity = RuntimeValidationSeverity.Required,
            string issueCode = null)
        {
            Message = message ?? string.Empty;
            FieldPath = fieldPath ?? string.Empty;
            TargetLabel = targetLabel ?? string.Empty;
            NativeAction = nativeAction ?? string.Empty;
            SuccessCheck = successCheck ?? string.Empty;
            Severity = severity;
            IssueCode = issueCode ?? string.Empty;
        }

        public string Message { get; }
        public string FieldPath { get; }
        public string TargetLabel { get; }
        public string NativeAction { get; }
        public string SuccessCheck { get; }
        public RuntimeValidationSeverity Severity { get; }
        public string IssueCode { get; }

        public static RuntimeValidationIssue Required(
            string message,
            string fieldPath = null,
            string targetLabel = null,
            string nativeAction = null,
            string successCheck = null,
            string issueCode = null)
        {
            return new RuntimeValidationIssue(
                message,
                fieldPath,
                targetLabel,
                nativeAction,
                successCheck,
                RuntimeValidationSeverity.Required,
                issueCode);
        }

        public static RuntimeValidationIssue Recommended(
            string message,
            string fieldPath = null,
            string targetLabel = null,
            string nativeAction = null,
            string successCheck = null,
            string issueCode = null)
        {
            return new RuntimeValidationIssue(
                message,
                fieldPath,
                targetLabel,
                nativeAction,
                successCheck,
                RuntimeValidationSeverity.Recommended,
                issueCode);
        }

        public static RuntimeValidationIssue Optional(
            string message,
            string fieldPath = null,
            string targetLabel = null,
            string nativeAction = null,
            string successCheck = null,
            string issueCode = null)
        {
            return new RuntimeValidationIssue(
                message,
                fieldPath,
                targetLabel,
                nativeAction,
                successCheck,
                RuntimeValidationSeverity.Optional,
                issueCode);
        }
    }

    public static class RuntimeValidationIssueUtility
    {
        public static RuntimeValidationIssue WithParentContext(
            RuntimeValidationIssue issue,
            string messagePrefix,
            string issueCodePrefix,
            string fieldPath = null,
            string targetLabel = null,
            string nativeAction = null,
            string successCheck = null)
        {
            if (issue == null)
                return null;

            string message = !string.IsNullOrWhiteSpace(messagePrefix)
                ? messagePrefix + issue.Message
                : issue.Message;

            string issueCode = CombineIssueCode(issueCodePrefix, issue.IssueCode);

            return new RuntimeValidationIssue(
                message,
                CombineFieldPath(fieldPath, issue.FieldPath),
                !string.IsNullOrWhiteSpace(issue.TargetLabel) ? issue.TargetLabel : targetLabel,
                !string.IsNullOrWhiteSpace(issue.NativeAction) ? issue.NativeAction : nativeAction,
                !string.IsNullOrWhiteSpace(issue.SuccessCheck) ? issue.SuccessCheck : successCheck,
                issue.Severity,
                issueCode);
        }

        public static IEnumerable<RuntimeValidationIssue> FromLocalValidationMessages(
            IEnumerable<string> messages,
            object owner)
        {
            if (messages == null)
                yield break;

            string ownerName = owner != null ? owner.GetType().Name : "RuntimeValidation";

            foreach (string message in messages)
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    yield return RuntimeValidationIssue.Required(
                        message,
                        targetLabel: ownerName,
                        nativeAction: $"Inspect {ownerName} and resolve the local validation issue: {message}",
                        successCheck: $"{ownerName} no longer reports this local validation issue.",
                        issueCode: BuildIssueCode(ownerName, message));
                }
            }
        }

        private static string BuildIssueCode(string ownerName, string message)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(string.IsNullOrWhiteSpace(ownerName) ? "RuntimeValidation" : ownerName.Trim());
            builder.Append(".Local.");

            int written = 0;
            for (int i = 0; i < message.Length && written < 80; i++)
            {
                char character = message[i];
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                    written++;
                    continue;
                }

                if (builder.Length > 0 && builder[builder.Length - 1] != '.')
                    builder.Append('.');
            }

            while (builder.Length > 0 && builder[builder.Length - 1] == '.')
                builder.Length--;

            return builder.ToString();
        }

        private static string CombineFieldPath(string parentFieldPath, string childFieldPath)
        {
            if (string.IsNullOrWhiteSpace(parentFieldPath))
                return childFieldPath ?? string.Empty;

            if (string.IsNullOrWhiteSpace(childFieldPath))
                return parentFieldPath;

            return parentFieldPath + "." + childFieldPath;
        }

        private static string CombineIssueCode(string issueCodePrefix, string childIssueCode)
        {
            if (string.IsNullOrWhiteSpace(issueCodePrefix))
                return childIssueCode ?? string.Empty;

            if (string.IsNullOrWhiteSpace(childIssueCode))
                return issueCodePrefix;

            return issueCodePrefix + "." + childIssueCode;
        }
    }

    /// <summary>
    /// Optional validation surface for authored runtime components that need
    /// configuration checks beyond simple interface presence.
    /// </summary>
    public interface IRuntimeValidationProvider
    {
        IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues();
    }
}
