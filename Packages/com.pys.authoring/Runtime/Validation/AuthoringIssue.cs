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
            AuthoringActionKind actionKind = AuthoringActionKind.None,
            string ownerStableId = null,
            string[] relatedStableIds = null)
        {
            IssueCode = issueCode ?? string.Empty;
            Message = message ?? string.Empty;
            Severity = severity;
            FieldPath = fieldPath ?? string.Empty;
            TargetLabel = targetLabel ?? string.Empty;
            NativeAction = nativeAction ?? string.Empty;
            SuccessCheck = successCheck ?? string.Empty;
            ActionKind = actionKind;
            OwnerStableId = ownerStableId ?? string.Empty;
            RelatedStableIds = relatedStableIds ?? new string[0];
        }

        public string IssueCode { get; }

        public string Message { get; }

        public AuthoringIssueSeverity Severity { get; }

        public string FieldPath { get; }

        public string TargetLabel { get; }

        public string NativeAction { get; }

        public string SuccessCheck { get; }

        public AuthoringActionKind ActionKind { get; }

        public string OwnerStableId { get; }

        public string[] RelatedStableIds { get; }
    }
}
