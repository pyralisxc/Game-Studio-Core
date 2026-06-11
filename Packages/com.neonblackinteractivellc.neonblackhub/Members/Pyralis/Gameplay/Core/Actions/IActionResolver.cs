using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Core.Actions
{
    [AuthoringContract(
        Capability = AuthoringCapability.Session,
        Relevance = "Resolves gameplay actions like movement, attacks, or logic triggers.",
        Axioms = AuthoringWorldAxiom.None,
        ExpertAdvice = "Implement this interface to handle specific ActionId strings within the ActionQueueService.",
        FirstProof = "Check if CanResolve returns true for its intended ActionId.",
        NativeSetup = new[] { "Implement IActionResolver in a new class.", "Register the implementation with the ActionQueueService." },
        DocumentationURL = "https://docs.neonblack.com/pyralis/action-resolvers"
    )]
public interface IActionResolver
{
        bool CanResolve(ActionExecutionContext context);
        ActionValidationResult ValidateAction(ActionExecutionContext context);
        ActionResolutionResult ResolveAction(ActionExecutionContext context);
    }
}
