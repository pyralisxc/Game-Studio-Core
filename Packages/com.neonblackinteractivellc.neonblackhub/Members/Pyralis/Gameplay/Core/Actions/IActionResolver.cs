namespace NeonBlack.Gameplay.Core.Actions
{
    public interface IActionResolver
    {
        bool CanResolve(ActionExecutionContext context);
        ActionValidationResult ValidateAction(ActionExecutionContext context);
        ActionResolutionResult ResolveAction(ActionExecutionContext context);
    }
}
