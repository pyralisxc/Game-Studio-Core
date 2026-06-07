using UnityEngine;
using NeonBlack.Gameplay.Core.Enums;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Pure C# movement model for the 2.5D brawler character controller.
    /// No MonoBehaviour dependency â€” instantiate with <c>new BrawlerMovementModel()</c>.
    ///
    /// <para>
    /// Each frame the MonoBehaviour layer calls <see cref="Tick"/> with input and last frame's
    /// physics results. The model returns the world-space velocity to pass to
    /// <c>CharacterController.Move</c>. The MB adds knockback and calls Move, then stores the
    /// physics outcome in a <see cref="MovementPhysicsFrame"/> for the next tick.
    /// </para>
    ///
    /// <code>
    ///   var model = new BrawlerMovementModel()
    ///   {
    ///   model.Configure(config);
    ///   // each Update:
    ///   Vector3 vel = model.Tick(input, physicsFrame, Time.deltaTime);
    ///   controller.Move((vel + knockback) * deltaTime);
    ///   physicsFrame = CollectPhysicsResults(collisionFlags, probeHit);
    /// </code>
    /// </summary>
    public sealed class BrawlerMovementModel
    {
        private readonly MovementState _state = new MovementState();
        private MovementConfig         _config;

        /// <summary>Read-only access to the model's current state for the MonoBehaviour layer.</summary>
        public MovementState State => _state;

        // â”€â”€ Configuration â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //

        /// <summary>Apply a new set of tuning values. Call from Awake and on any profile change.</summary>
        public void Configure(MovementConfig config) => _config = config;

        // â”€â”€ Main tick â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //

        /// <summary>
        /// Advance the model one frame.
        /// Returns the world-space velocity to pass to <c>CharacterController.Move(vel * deltaTime)</c>.
        /// Knockback from <c>KnockbackReceiver</c> should be added by the MB before calling Move.
        /// </summary>
        public Vector3 Tick(MovementInput input, MovementPhysicsFrame physics, float deltaTime)
        {
            // â”€â”€ Clear one-frame trigger flags â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            _state.TriggerJump        = false;
            _state.TriggerDiveRoll    = false;
            _state.TriggerPowerSlide  = false;
            _state.TriggerMoveToIdle  = false;
            _state.TriggerJustLanded  = false;
            _state.TriggerKnockedBack = false;
            _state.LandImpactSpeed    = 0f;

            // â”€â”€ Absorb last frame's physics results â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            bool wasGrounded      = _state.IsGrounded;
            _state.IsGrounded     = physics.GroundedByCollision || physics.GroundedByProbe;
            _state.GroundNormal   = physics.GroundNormal;
            _state.HasWallContact = physics.HasWallContact;

            // â”€â”€ Landing detection â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            if (!wasGrounded && _state.IsGrounded)
            {
                _state.TriggerJustLanded = true;
                _state.LandImpactSpeed   = Mathf.Abs(_state.PreLandVelocityY);
                _state.JumpsUsed         = 0;
                _state.JumpCut           = false;
                if (_config.LandSlowDuration > 0f)
                    _state.LandSlowTimer = _config.LandSlowDuration;
                // Buffered jump: player pressed jump just before landing.
                if (_state.JumpBufferCounter > 0f)
                {
                    _state.JumpsUsed = 1;
                    ApplyJump();
                }
            }

            // â”€â”€ Coyote time â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            if (wasGrounded && _state.JumpsUsed == 0)
                _state.CoyoteCounter = _config.CoyoteTime;
            else
                _state.CoyoteCounter -= deltaTime;

            // â”€â”€ Jump buffer counter â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            _state.JumpBufferCounter -= deltaTime;

            // â”€â”€ Process jump input â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            if (input.JumpPressed)
            {
                bool topDownNoJump = _config.MovementMode == MovementMode.TopDown && !_config.TopDownAllowJump;
                if (!topDownNoJump && !_state.IsCrouching)
                {
                    if (_state.CoyoteCounter > 0f)
                    {
                        _state.JumpBufferCounter = _config.JumpBufferTime;
                        _state.JumpsUsed = 1;
                        ApplyJump();
                    }
                    else if (_state.JumpsUsed > 0 && _state.JumpsUsed < _config.MaxJumps)
                    {
                        _state.JumpsUsed++;
                        ApplyJump();
                    }
                    else if (_state.JumpsUsed == 0)
                    {
                        _state.JumpBufferCounter = _config.JumpBufferTime;
                    }
                }
            }

            // â”€â”€ Jump cut â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            if (input.JumpReleased && _state.VelocityY > 0f)
                _state.JumpCut = true;

            // â”€â”€ Sprint â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            _state.IsSprinting = input.SprintHeld;

            // â”€â”€ Cooldown timers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            if (_state.RollTimer > 0f)       _state.RollTimer       -= deltaTime;
            if (_state.DodgeTimer > 0f)      _state.DodgeTimer      -= deltaTime;
            if (_state.PowerSlideTimer > 0f) _state.PowerSlideTimer -= deltaTime;
            if (_state.ClimbTimer > 0f)      _state.ClimbTimer      -= deltaTime;
            if (_state.LandSlowTimer > 0f)   _state.LandSlowTimer   -= deltaTime;

            // â”€â”€ Gravity â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            bool topDownNoGravity = _config.MovementMode == MovementMode.TopDown && !_config.TopDownAllowJump;

            if (_state.IsClimbing)
            {
                _state.VelocityY = 0f;
                _state.VelocityX = 0f;
                _state.VelocityZ = 0f;
            }
            else if (topDownNoGravity)
            {
                _state.VelocityY = 0f;
            }
            else
            {
                if (wasGrounded && _state.VelocityY < 0f)
                    _state.VelocityY = -2f;

                if (_state.JumpCut && _state.VelocityY > 0f)
                {
                    _state.VelocityY *= _config.JumpCutMultiplier;
                    _state.JumpCut    = false;
                }

                bool canWallSlide = !_state.IsGrounded && _state.HasWallContact && _state.VelocityY < 0f;
                if (canWallSlide)
                {
                    _state.IsWallSliding = true;
                    _state.VelocityY    += _config.Gravity * _config.WallSlideGravityMultiplier * deltaTime;
                    if (_state.VelocityY < _config.WallSlideFallSpeedCap)
                        _state.VelocityY = _config.WallSlideFallSpeedCap;
                }
                else
                {
                    _state.IsWallSliding = false;
                    _state.VelocityY    += _config.Gravity * deltaTime;
                }
            }

            // â”€â”€ Horizontal movement â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            if (!_state.IsClimbing && !_state.IsHanging && !_state.IsDodging && !_state.IsPowerSliding)
            {
                float targetSpeed = _state.IsCrouching ? _config.CrouchSpeed
                                  : _state.IsSprinting ? _config.SprintSpeed
                                  : _config.WalkSpeed;

                float targetX = input.Move.x * targetSpeed;
                float targetZ = _config.MovementMode != MovementMode.TwoD
                    ? input.Move.y * targetSpeed * _config.DepthSpeedMultiplier
                    : 0f;

                // Attack slowdown.
                if (input.AttackTimerActive || input.KickTimerActive)
                {
                    bool  isAerial = !_state.IsGrounded || _state.VelocityY > 0f;
                    float mult     = isAerial ? input.AerialAttackMoveMultiplier : input.AttackMoveMultiplier;
                    targetX *= mult;
                    targetZ *= mult;
                }

                // Land slow.
                if (_state.LandSlowTimer > 0f)
                {
                    targetX *= _config.LandSlowMultiplier;
                    targetZ *= _config.LandSlowMultiplier;
                }

                float accelStep = (targetSpeed / Mathf.Max(_config.AccelerationTime, 0.001f)) * deltaTime;
                float decelStep = (targetSpeed / Mathf.Max(_config.DecelerationTime, 0.001f)) * deltaTime;

                _state.VelocityX = Mathf.MoveTowards(_state.VelocityX, targetX, Mathf.Abs(targetX) > 0.01f ? accelStep : decelStep);
                _state.VelocityZ = Mathf.MoveTowards(_state.VelocityZ, targetZ, Mathf.Abs(targetZ) > 0.01f ? accelStep : decelStep);
            }

            // â”€â”€ Slope slide â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            float slopeAngle  = Vector3.Angle(_state.GroundNormal, Vector3.up);
            bool  onSteepSlope = _state.IsGrounded && slopeAngle >= _config.SlideAngle
                                && !_state.IsDodging && !_state.IsClimbing;

            if (onSteepSlope)
            {
                _state.IsSliding = true;
                Vector3 slideDir    = Vector3.ProjectOnPlane(Vector3.down, _state.GroundNormal).normalized;
                Vector3 lateralAxis = Vector3.Cross(slideDir, Vector3.up).normalized;

                float targetSlideX = slideDir.x * _config.SlideSpeed
                                   + lateralAxis.x * input.Move.x * _config.SlideSpeed * _config.SlideSteering;
                float targetSlideZ = slideDir.z * _config.SlideSpeed
                                   + lateralAxis.z * input.Move.x * _config.SlideSpeed * _config.SlideSteering;

                float blendStep = _config.SlideSpeed / Mathf.Max(_config.SlideBlendTime, 0.001f) * deltaTime;
                _state.VelocityX = Mathf.MoveTowards(_state.VelocityX, targetSlideX, blendStep);
                _state.VelocityZ = Mathf.MoveTowards(_state.VelocityZ, targetSlideZ, blendStep);
            }
            else
            {
                _state.IsSliding = false;
            }

            // â”€â”€ Dodge tick â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            if (_state.IsDodging)
            {
                float dodgeSpeed    = _config.DodgeDistance / Mathf.Max(_config.DodgeDuration, 0.01f);
                _state.VelocityX    = _state.DodgeDir.x * dodgeSpeed;
                _state.VelocityZ    = _state.DodgeDir.y * dodgeSpeed;
                _state.DodgeElapsed += deltaTime;
                if (_state.DodgeElapsed >= _config.DodgeDuration)
                {
                    _state.IsDodging  = false;
                    _state.IsActing   = false;
                    _state.DodgeTimer = _config.DodgeCooldown;
                    _state.RollTimer  = _config.RollCooldown;
                }
            }

            // â”€â”€ Power slide tick â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            if (_state.IsPowerSliding)
            {
                _state.VelocityX          = _state.PowerSlideVelocityX;
                _state.VelocityZ          = 0f;
                _state.PowerSlideElapsed += deltaTime;
                if (_state.PowerSlideElapsed >= _config.PowerSlideDuration)
                {
                    _state.IsPowerSliding  = false;
                    _state.IsActing        = false;
                    _state.PowerSlideTimer = _config.PowerSlideCooldown;
                }
            }

            // â”€â”€ IsMoving / MoveToIdle signal â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            bool isMoving = input.Move.sqrMagnitude > 0.01f;
            if (_state.PrevMoving && !isMoving)
                _state.TriggerMoveToIdle = true;
            _state.IsMoving   = isMoving;
            _state.PrevMoving = isMoving;

            // â”€â”€ Facing (screen-relative via camera right) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
            UpdateFacing(new Vector3(_state.VelocityX, 0f, _state.VelocityZ), input.CameraRight);

            // â”€â”€ Cache pre-land velocity for next frame's squash check â”€â”€â”€â”€â”€â”€â”€â”€ //
            _state.PreLandVelocityY = _state.VelocityY;
            _state.WasGrounded      = _state.IsGrounded;

            return new Vector3(_state.VelocityX, _state.VelocityY, _state.VelocityZ);
        }

        // â”€â”€ Action triggers (called by MonoBehaviour on input events) â”€â”€â”€â”€â”€â”€â”€â”€ //

        /// <summary>
        /// Attempt to start a dodge. Returns true and sets dodge state if conditions are met.
        /// The MB is responsible for granting IFrames (<c>HealthComponent.ForceIFrames</c>)
        /// when this returns true.
        /// </summary>
        public bool TryStartDodge(Vector2 moveInput)
        {
            if (_state.IsDodging || _state.DodgeTimer > 0f || _state.RollTimer > 0f
                || _state.IsActing || !_state.IsGrounded)
                return false;

            _state.DodgeDir      = moveInput.magnitude > 0.1f
                ? moveInput.normalized
                : new Vector2(_state.FacingRight ? 1f : -1f, 0f);
            _state.IsDodging     = true;
            _state.DodgeElapsed  = 0f;
            _state.IsActing      = true;
            _state.TriggerDiveRoll = true;
            return true;
        }

        /// <summary>
        /// Attempt to start a power slide (sprint + crouch while grounded).
        /// Returns true and sets slide state if conditions are met.
        /// The MB handles hitbox firing and the Slide animator trigger.
        /// </summary>
        public bool TryStartPowerSlide()
        {
            if (_state.IsPowerSliding || _state.PowerSlideTimer > 0f
                || _state.IsActing || !_state.IsGrounded || !_state.IsSprinting)
                return false;

            float speed = _config.PowerSlideDistance / Mathf.Max(_config.PowerSlideDuration, 0.01f);
            _state.PowerSlideVelocityX = (_state.FacingRight ? 1f : -1f) * speed;
            _state.IsPowerSliding      = true;
            _state.PowerSlideElapsed   = 0f;
            _state.IsActing            = true;
            _state.TriggerPowerSlide   = true;
            return true;
        }

        /// <summary>Notify the model that crouch state changed. Capsule resize is the MB's job.</summary>
        public void SetCrouching(bool crouching) => _state.IsCrouching = crouching;

        /// <summary>Notify the model that a climb animation has started. Suspends gravity and movement.</summary>
        public void NotifyClimbStart(float cooldownOverride = -1f)
        {
            _state.IsClimbing = true;
            _state.IsActing   = true;
            _state.VelocityY  = 0f;
            _state.VelocityX  = 0f;
            _state.VelocityZ  = 0f;
            _state.JumpsUsed  = 0;
            _state.ClimbTimer = cooldownOverride >= 0f ? cooldownOverride : _config.ClimbCooldown;
        }

        /// <summary>Notify the model that the climb animation has finished.</summary>
        public void NotifyClimbEnd()
        {
            _state.IsClimbing = false;
            _state.IsActing   = false;
        }

        /// <summary>Notify the model that the hang state has started.</summary>
        public void NotifyHangStart(int maxJumps)
        {
            _state.IsHanging = true;
            _state.VelocityY = 0f;
            _state.VelocityX = 0f;
            _state.VelocityZ = 0f;
            _state.JumpsUsed = maxJumps; // consume all jumps while hanging
        }

        /// <summary>
        /// Notify the model that the hang state has ended.
        /// VelocityY is NOT reset here â€” the MB sets it as needed (e.g. -1f to nudge clear of the ledge).
        /// </summary>
        public void NotifyHangEnd() => _state.IsHanging = false;

        /// <summary>Directly set VelocityY â€” used by the MB after hang drop to nudge the character clear.</summary>
        public void SetVelocityY(float y) => _state.VelocityY = y;

        /// <summary>
        /// Set the IsActing flag from the MB (driven by Animator state tags).
        /// The model manages this internally but the MB can clear it once a combat state exits.
        /// </summary>
        public void SetActing(bool acting) => _state.IsActing = acting;

        /// <summary>Queue the KnockedBack animator trigger for this frame.</summary>
        public void TriggerKnockBack() => _state.TriggerKnockedBack = true;

        // â”€â”€ Private helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //

        private void ApplyJump()
        {
            _state.VelocityY         = Mathf.Sqrt(_config.JumpHeight * -2f * _config.Gravity);
            _state.CoyoteCounter     = 0f;
            _state.JumpBufferCounter = 0f;
            _state.JumpCut           = false;
            _state.TriggerJump       = true;
        }

        private void UpdateFacing(Vector3 horizVel, Vector3 cameraRight)
        {
            if (horizVel.sqrMagnitude <= 0.01f) return;

            if (cameraRight.sqrMagnitude > 0.01f)
            {
                float dot = Vector3.Dot(horizVel, cameraRight);
                if (Mathf.Abs(dot) > 0.01f)
                    _state.FacingRight = dot > 0f;
            }
            else if (Mathf.Abs(horizVel.x) > 0.01f)
            {
                _state.FacingRight = horizVel.x > 0f;
            }
        }
    }
}
