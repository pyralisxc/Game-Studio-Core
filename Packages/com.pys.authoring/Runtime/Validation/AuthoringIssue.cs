namespace Pys.Authoring.Contracts
{
    public enum AuthoringIssueSeverity
    {
        Info,
        Optional,
        Recommended,
        Required
    }

    public sealed class AuthoringIssue
    {
        public AuthoringIssue(
            string issueCode,
            string message,
            AuthoringIssueSeverity severity = AuthoringIssueSeverity.Required,
            string fieldPath = null,
            string targetLabel = null,
            string nativeAction = null,
            string successCheck = null,
            AuthoringActionKind actionKind = AuthoringActionKind.None)
        {
            IssueCode = issueCode ?? string.Empty;
            Message = message ?? string.Empty;
            Severity = severity;
            FieldPath = fieldPath ?? string.Empty;
            TargetLabel = targetLabel ?? string.Empty;
            NativeAction = nativeAction ?? string.Empty;
            SuccessCheck = successCheck ?? string.Empty;
            ActionKind = actionKind;
        }

        public string IssueCode { get; }

        public string Message { get; }

        public AuthoringIssueSeverity Severity { get; }

        public string FieldPath { get; }

        public string TargetLabel { get; }

        public string NativeAction { get; }

        public string SuccessCheck { get; }

        public AuthoringActionKind ActionKind { get; }
    }
}
