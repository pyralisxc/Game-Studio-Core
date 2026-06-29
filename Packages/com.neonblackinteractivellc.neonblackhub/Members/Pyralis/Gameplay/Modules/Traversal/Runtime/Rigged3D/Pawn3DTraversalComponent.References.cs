using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Participants;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Traversal
{
    public sealed partial class Pawn3DTraversalComponent
    {
        private IPawnTraversalMovementController _movement;
        private CharacterController _controller;
        private IActorAnimationController _animationDriver;

        private void Awake()
        {
            _movement = GetComponent<IPawnTraversalMovementController>();
            _controller = GetComponent<CharacterController>();
            _animationDriver = GetComponent<IActorAnimationController>();
            ApplySerializedTraversalProfile();
        }

        private bool EnsureDependencies()
        {
            _movement ??= GetComponent<IPawnTraversalMovementController>();
            _controller ??= GetComponent<CharacterController>();
            _animationDriver ??= GetComponent<IActorAnimationController>();
            return _movement != null && _controller != null;
        }
    }
}
