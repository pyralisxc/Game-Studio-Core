using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Core.Contracts
{
    [AuthoringContract(
        Capability = AuthoringCapability.Animation, 
        Relevance = "Controls actor animation states, transitions, and parameter syncing.", 
        Axioms = AuthoringWorldAxiom.None,
        AssignmentFields = new[] { nameof(IActorAnimationController.PresentationMode) },
        FirstProof = "Verify that TriggerSignal successfully fires a trigger on the underlying Animator.",
        NativeSetup = new[] { "Implement interface in a presentation component" },
        ExpertAdvice = "Provides a logic-only abstraction for animations. Other systems (Movement, Combat) should call these methods instead of driving the Animator directly to maintain decoupling.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/animation"
    )]
public interface IActorAnimationController
{
        ActorPresentationMode PresentationMode { get; }
        void SetBoolSignal(ActorAnimationSignal signal, bool value);
        void SetFloatSignal(ActorAnimationSignal signal, float value);
        void SetIntSignal(ActorAnimationSignal signal, int value);
        void SetBoolCustom(string customKey, bool value);
        void SetFloatCustom(string customKey, float value);
        void SetIntCustom(string customKey, int value);
        void TriggerSignal(ActorAnimationSignal signal, int intValue = 1, float floatValue = 1f, bool boolValue = true);
        void TriggerCustom(string customKey, int intValue = 1, float floatValue = 1f, bool boolValue = true);
        void SetFacing(bool facingRight);
    }
}
