using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Core.Contracts
{
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
