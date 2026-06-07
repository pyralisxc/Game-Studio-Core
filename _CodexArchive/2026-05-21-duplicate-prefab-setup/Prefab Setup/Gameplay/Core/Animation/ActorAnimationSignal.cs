namespace NeonBlack.Gameplay.Presentation.Animation
{
    public enum ActorAnimationSignal
    {
        Idle,
        Move,
        Sprint,
        Crouch,
        Jump,
        Fall,
        Land,
        Dash,
        Slide,
        AttackPrimary,
        AttackSecondary,
        AttackAerial,
        BlockStart,
        BlockLoop,
        BlockEnd,
        Hurt,
        Stagger,
        Death,
        ClimbStart,
        ClimbLoop,
        ClimbEnd,
        Hang,
        Shimmy,
        Interact,
        LookAround,
        Spawn,
        Despawn,
        SideClimb,
        ForwardClimb,
        LedgeDrop,
        Custom
    }

    public enum ActorAnimationBindingType
    {
        Bool,
        Trigger,
        Float,
        Int
    }
}
