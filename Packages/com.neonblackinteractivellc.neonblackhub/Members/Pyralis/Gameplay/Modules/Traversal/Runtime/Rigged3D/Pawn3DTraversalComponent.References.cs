using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Traversal
{
    public sealed partial class Pawn3DTraversalComponent
    {
        private Pawn3DMovementComponent _movement;
        private CharacterController _controller;
        private ActorAnimationDriver _animationDriver;

        private void Awake()
        {
            _movement = GetComponent<Pawn3DMovementComponent>();
            _controller = GetComponent<CharacterController>();
            _animationDriver = GetComponent<ActorAnimationDriver>();
        }

        private bool EnsureDependencies()
        {
            _movement ??= GetComponent<Pawn3DMovementComponent>();
            _controller ??= GetComponent<CharacterController>();
            _animationDriver ??= GetComponent<ActorAnimationDriver>();
            return _movement != null && _controller != null;
        }
    }
}
