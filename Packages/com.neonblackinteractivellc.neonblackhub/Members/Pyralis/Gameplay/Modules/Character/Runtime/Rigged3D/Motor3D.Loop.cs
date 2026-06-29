using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Input;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Character
{
    public partial class Motor3D
    {
        protected override void OnGameplayTick(in GameplayTickContext context)
        {
            ResolveDirectCapabilities();

            if (_reactionLockTimer > 0f || _statusActionLocked)
            {
                TickLockedFrame(context.DeltaTime);
                return;
            }

            FrameInput frameInput = Input.CollectFrameInput();
            Presentation.UpdateLookAround(frameInput);

            ApplyPostureInput(frameInput);
            DispatchActionInput(frameInput);
            ApplyDodgeInput(frameInput);

            Vector3 velocity = Movement.Tick(frameInput, context.DeltaTime);
            if (HandleTraversalFrame(frameInput, context.DeltaTime))
                return;

            Movement.ApplyMovement(velocity, context.DeltaTime);
            ApplyPresentation();
        }

        private void TickLockedFrame(float deltaTime)
        {
            if (_reactionLockTimer > 0f)
                _reactionLockTimer = Mathf.Max(0f, _reactionLockTimer - deltaTime);

            Movement.ApplyMovement(Vector3.zero, deltaTime);
            ApplyPresentation();
        }

        private void ApplyPostureInput(FrameInput frameInput)
        {
            if (frameInput.CrouchPressed && !Movement.TryStartPowerSlide())
                Movement.SetCrouch(true);

            if (frameInput.CrouchReleased)
                Movement.SetCrouch(false);
        }

        private void DispatchActionInput(FrameInput frameInput)
        {
            if (frameInput.AttackPressed)
                CombatRequests?.TryHandleCombatCommand(new ActorCombatCommand(ActorCombatCommandKind.PrimaryAttack, gameObject));
            if (frameInput.KickPressed)
                CombatRequests?.TryHandleCombatCommand(new ActorCombatCommand(ActorCombatCommandKind.SecondaryAttack, gameObject));
            if (frameInput.BlockPressed)
                BeginGuardOrBlock();
            if (frameInput.BlockReleased)
                EndGuardOrBlock();
            if (frameInput.WeaponCycleDelta != 0)
                CombatRequests?.TryHandleCombatCommand(new ActorCombatCommand(
                    ActorCombatCommandKind.CycleWeapon,
                    gameObject,
                    direction: frameInput.WeaponCycleDelta));
        }

        private void BeginGuardOrBlock()
        {
            if (GuardFeature != null)
                GuardFeature.BeginGuard();
            else
                CombatRequests?.TryHandleCombatCommand(new ActorCombatCommand(ActorCombatCommandKind.BlockStart, gameObject));
        }

        private void EndGuardOrBlock()
        {
            if (GuardFeature != null)
                GuardFeature.EndGuard();
            else
                CombatRequests?.TryHandleCombatCommand(new ActorCombatCommand(ActorCombatCommandKind.BlockEnd, gameObject));
        }

        private void ApplyDodgeInput(FrameInput frameInput)
        {
            if (frameInput.RollPressed && Movement.TryStartDodge(frameInput.Move))
                DamageImmunity?.ForceIFrames(Movement.DodgeDuration);
        }

        private bool HandleTraversalFrame(FrameInput frameInput, float deltaTime)
        {
            if ((TraversalFeature != null && TraversalFeature.HandleHangFrame(frameInput, deltaTime))
                || (TraversalFeature == null && Traversal.HandleHangFrame(frameInput, deltaTime)))
            {
                ApplyPresentation();
                return true;
            }

            if (TraversalFeature != null)
                TraversalFeature.ProbeTraversal();
            else
                Traversal.ProbeLedge();

            if (frameInput.InteractPressed)
            {
                if (InteractionRequests != null)
                    InteractionRequests.TryHandleInteraction();
                else
                    Traversal.HandleInteract();
            }

            return false;
        }

        private void ApplyPresentation()
        {
            float shimmyVelocity = TraversalFeature != null
                ? TraversalFeature.ShimmyVelocityX
                : Traversal.ShimmyVelocityX;
            Presentation.Apply(shimmyVelocity);
        }
    }
}
