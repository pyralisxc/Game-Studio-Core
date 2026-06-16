using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public partial class Motor3D
    {
        private void Update()
        {
            ResolveFeatureModules();

            if (_reactionLockTimer > 0f || _statusActionLocked)
            {
                TickLockedFrame();
                return;
            }

            FrameInput frameInput = Input.CollectFrameInput();
            Combat?.UpdateCombatTimers();
            Presentation.UpdateLookAround(frameInput);

            ApplyPostureInput(frameInput);
            DispatchActionInput(frameInput);
            ApplyDodgeInput(frameInput);

            Vector3 velocity = Movement.Tick(frameInput);
            if (HandleTraversalFrame(frameInput))
                return;

            Movement.ApplyMovement(velocity);
            ApplyPresentation();
        }

        private void TickLockedFrame()
        {
            if (_reactionLockTimer > 0f)
                _reactionLockTimer = Mathf.Max(0f, _reactionLockTimer - Time.deltaTime);

            Combat?.UpdateCombatTimers();
            Movement.ApplyMovement(Vector3.zero);
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
                Combat?.HandleAttack();
            if (frameInput.KickPressed)
                Combat?.HandleKick();
            if (frameInput.BlockPressed)
                BeginGuardOrBlock();
            if (frameInput.BlockReleased)
                EndGuardOrBlock();
            if (frameInput.WeaponCycleDelta != 0)
                Combat?.CycleWeapon(frameInput.WeaponCycleDelta);
        }

        private void BeginGuardOrBlock()
        {
            if (GuardFeature != null)
                GuardFeature.BeginGuard();
            else
                Combat?.HandleBlockStart();
        }

        private void EndGuardOrBlock()
        {
            if (GuardFeature != null)
                GuardFeature.EndGuard();
            else
                Combat?.HandleBlockEnd();
        }

        private void ApplyDodgeInput(FrameInput frameInput)
        {
            if (frameInput.RollPressed && Movement.TryStartDodge(frameInput.Move))
                Health?.ForceIFrames(Movement.DodgeDuration);
        }

        private bool HandleTraversalFrame(FrameInput frameInput)
        {
            if ((TraversalFeature != null && TraversalFeature.HandleHangFrame(frameInput))
                || (TraversalFeature == null && Traversal.HandleHangFrame(frameInput)))
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
                if (InteractionFeature != null)
                    InteractionFeature.TryHandleInteraction();
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
