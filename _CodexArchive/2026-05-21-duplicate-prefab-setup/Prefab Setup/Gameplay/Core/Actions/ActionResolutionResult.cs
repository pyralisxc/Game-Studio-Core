namespace NeonBlack.Gameplay.Core.Actions
{
    public readonly struct ActionResolutionResult
    {
        public ActionResolutionStatus Status { get; }
        public string Message { get; }
        public object Payload { get; }
        public bool Succeeded => Status == ActionResolutionStatus.Succeeded;

        private ActionResolutionResult(ActionResolutionStatus status, string message, object payload)
        {
            Status = status;
            Message = message ?? string.Empty;
            Payload = payload;
        }

        public static ActionResolutionResult Success(string message = "", object payload = null)
        {
            return new ActionResolutionResult(ActionResolutionStatus.Succeeded, message, payload);
        }

        public static ActionResolutionResult Failure(string message, object payload = null)
        {
            return new ActionResolutionResult(ActionResolutionStatus.Failed, message, payload);
        }

        public static ActionResolutionResult Rejected(string message)
        {
            return new ActionResolutionResult(ActionResolutionStatus.Rejected, message, null);
        }

        public static ActionResolutionResult Pending(string message = "")
        {
            return new ActionResolutionResult(ActionResolutionStatus.Pending, message, null);
        }
    }
}
