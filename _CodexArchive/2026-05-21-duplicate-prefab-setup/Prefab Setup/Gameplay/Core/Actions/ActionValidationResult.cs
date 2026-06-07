namespace NeonBlack.Gameplay.Core.Actions
{
    public readonly struct ActionValidationResult
    {
        public bool IsValid { get; }
        public string Message { get; }

        private ActionValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message ?? string.Empty;
        }

        public static ActionValidationResult Success(string message = "") => new ActionValidationResult(true, message);

        public static ActionValidationResult Failure(string message) => new ActionValidationResult(false, message);
    }
}
