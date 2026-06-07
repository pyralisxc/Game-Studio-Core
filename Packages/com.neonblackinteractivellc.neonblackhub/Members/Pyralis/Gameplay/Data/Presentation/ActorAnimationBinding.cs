using System;

namespace NeonBlack.Gameplay.Presentation.Animation
{
    [Serializable]
    public class ActorAnimationBinding
    {
        public ActorAnimationSignal signal = ActorAnimationSignal.Idle;
        public string customKey = string.Empty;
        public ActorAnimationBindingType bindingType = ActorAnimationBindingType.Bool;
        public string parameterName = string.Empty;
        public bool useSignalBool = true;
        public bool boolValue = true;
        public bool useSignalFloat = true;
        public float floatValue = 1f;
        public bool useSignalInt = true;
        public int intValue = 1;
    }
}
