using System.Collections.Generic;
using UnityEngine;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AuthoringContract(
        Category = "Combat, Tactics Defensive",
        CapabilityPath = "Combat/Actions/Pawn Block Module",
        Surface = AuthoringSurface.Goal,
        Summary = "Pawn module for blocking and damage reduction.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/combat",
        SuccessChecks = new[] { "Hold the block button and verify damage from the front is reduced." },
        Tags = new[] { "capability:Combat", "capability:TacticsDefensive" }
    )]
    public class PawnBlockModule : MonoBehaviour, IRuntimeValidationProvider
{
        [Header("Block Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float blockDamageReduction = 0.2f;
        [Range(10f, 180f)]
        [SerializeField] private float blockFrontalAngle = 90f;

        private IActorAnimationController _animationDriver;
        private IActorCombatMovementState _motor;
        private bool _isBlocking;

        public bool IsBlocking => _isBlocking;
        public float BlockDamageReduction => blockDamageReduction;
        public float BlockFrontalAngle => blockFrontalAngle;

        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (blockDamageReduction < 0f || blockDamageReduction > 1f)
                yield return RuntimeValidationIssue.Required("Block Damage Reduction must be between 0 and 1.");
            if (blockFrontalAngle <= 0f || blockFrontalAngle > 180f)
                yield return RuntimeValidationIssue.Required("Block Frontal Angle must be greater than 0 and at most 180.");
        }

        private void Awake()
        {
            _animationDriver = GetComponent<IActorAnimationController>();
            _motor = GetComponent<IActorCombatMovementState>();
        }

        public void HandleBlockStart()
        {
            if (_motor != null && _motor.IsActing)
                return;

            _isBlocking = true;
            _animationDriver?.TriggerSignal(ActorAnimationSignal.BlockStart);
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.BlockLoop, true);
        }

        public void HandleBlockEnd()
        {
            _isBlocking = false;
            _animationDriver?.TriggerSignal(ActorAnimationSignal.BlockEnd);
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.BlockLoop, false);
        }

        public void Tick()
        {
             _animationDriver?.SetBoolSignal(ActorAnimationSignal.BlockLoop, _isBlocking);
        }
    }
}
