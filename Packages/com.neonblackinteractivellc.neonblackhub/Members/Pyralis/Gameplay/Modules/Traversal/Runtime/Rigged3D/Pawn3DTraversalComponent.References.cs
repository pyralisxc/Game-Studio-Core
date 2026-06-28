using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Traversal
{
    public sealed partial class Pawn3DTraversalComponent
    {
        private IPawnTraversalMovementController _movement;
        private CharacterController _controller;
        private ActorAnimationDriver _animationDriver;

        private void Awake()
        {
            _movement = GetComponent<IPawnTraversalMovementController>();
            _controller = GetComponent<CharacterController>();
            _animationDriver = GetComponent<ActorAnimationDriver>();
            ApplySerializedTraversalProfile();
        }

        private bool EnsureDependencies()
        {
            _movement ??= GetComponent<IPawnTraversalMovementController>();
            _controller ??= GetComponent<CharacterController>();
            _animationDriver ??= GetComponent<ActorAnimationDriver>();
            return _movement != null && _controller != null;
        }
    }
}
