using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Core.Actions
{
    [AuthoringContract(Capability = AuthoringCapability.Session, Relevance = "Resolves gameplay actions like movement, attacks, or logic triggers.", Axioms = AuthoringWorldAxiom.None)]
public interface IActionResolver
{
        bool CanResolve(ActionExecutionContext context);
        ActionValidationResult ValidateAction(ActionExecutionContext context);
        ActionResolutionResult ResolveAction(ActionExecutionContext context);
    }
}
