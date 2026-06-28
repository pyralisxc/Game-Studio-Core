using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Core.Actions
{
    [AuthoringContract(
        Category = "Session",
        Surface = AuthoringSurface.Goal,
        Summary = "Resolves gameplay actions like movement, attacks, or logic triggers.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/action-resolvers",
        SetupSteps = new[] { "Implement IActionResolver in a new class.", "Register the implementation with the ActionQueueService." },
        SuccessChecks = new[] { "Check if CanResolve returns true for its intended ActionId." },
        Tags = new[] { "capability:Session" }
    )]
    public interface IActionResolver
    {
        bool CanResolve(ActionExecutionContext context);
        ActionValidationResult ValidateAction(ActionExecutionContext context);
        ActionResolutionResult ResolveAction(ActionExecutionContext context);
    }
}
