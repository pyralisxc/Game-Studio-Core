using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public enum PyralisRuntimeValidationSeverity
    {
        Info,
        Optional,
        Recommended,
        Required
    }

    public sealed class PyralisRuntimeValidationIssue
    {
        public PyralisRuntimeValidationIssue(
            string message,
            string fieldPath = null,
            string targetLabel = null,
            string nativeAction = null,
            string successCheck = null,
            PyralisRuntimeValidationSeverity severity = PyralisRuntimeValidationSeverity.Required)
        {
            Message = message ?? string.Empty;
            FieldPath = fieldPath ?? string.Empty;
            TargetLabel = targetLabel ?? string.Empty;
            NativeAction = nativeAction ?? string.Empty;
            SuccessCheck = successCheck ?? string.Empty;
            Severity = severity;
        }

        public string Message { get; }
        public string FieldPath { get; }
        public string TargetLabel { get; }
        public string NativeAction { get; }
        public string SuccessCheck { get; }
        public PyralisRuntimeValidationSeverity Severity { get; }

        public static PyralisRuntimeValidationIssue Required(
            string message,
            string fieldPath = null,
            string targetLabel = null,
            string nativeAction = null,
            string successCheck = null)
        {
            return new PyralisRuntimeValidationIssue(
                message,
                fieldPath,
                targetLabel,
                nativeAction,
                successCheck,
                PyralisRuntimeValidationSeverity.Required);
        }
    }

    public static class PyralisRuntimeValidationIssueUtility
    {
        public static IEnumerable<PyralisRuntimeValidationIssue> RequiredFrom(IEnumerable<string> messages)
        {
            if (messages == null)
                yield break;

            foreach (string message in messages)
            {
                if (!string.IsNullOrWhiteSpace(message))
                    yield return PyralisRuntimeValidationIssue.Required(message);
            }
        }
    }

    /// <summary>
    /// Optional validation surface for authored runtime components that need
    /// configuration checks beyond simple interface presence.
    /// </summary>
    public interface IRuntimeValidationProvider
    {
        IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues();
    }
}
