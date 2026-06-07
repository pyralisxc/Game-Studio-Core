using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    /// <summary>
    /// Pure C# movement model for the top-down 2D arcade pawn.
    /// No MonoBehaviour or Unity physics dependency; unit-testable standalone.
    ///
    /// <para>
    /// The MonoBehaviour layer (<see cref="Motor2D"/>) calls <see cref="Tick"/> each
    /// <c>FixedUpdate</c>, receives the smoothed velocity, then handles all world-space
    /// concerns: bounds clamping, dead-zone wall enforcement, and
    /// <c>Rigidbody2D.MovePosition</c>.
    /// </para>
    ///
    /// <code>
    ///   var model = new Motor2DModel()
    ///   {
    ///   model.Configure(config);
    ///   // each FixedUpdate:
    ///   Vector2 vel = model.Tick(new Motor2DInput { MoveDirection = dir }, Time.fixedDeltaTime);
    ///   Vector2 newPos = _rb2d.position + vel * Time.fixedDeltaTime;
    ///   // clamp / dead-zone newPos as needed.
    ///   _rb2d.MovePosition(newPos);
    /// </code>
    /// </summary>
    public sealed class Motor2DModel
    {
        private readonly Motor2DState _state = new Motor2DState();
        private Motor2DConfig         _config;

        /// <summary>Read-only access to current state for the MonoBehaviour layer.</summary>
        public Motor2DState State => _state;

        // Configuration

        /// <summary>Apply tuning values. Call from Awake and after any profile change.</summary>
        public void Configure(Motor2DConfig config) => _config = config;

        // Main tick

        /// <summary>
        /// Advance the model one fixed timestep.
        /// Returns the world-space velocity to add to the current position.
        /// The caller multiplies by <c>deltaTime</c> before applying to <c>Rigidbody2D.MovePosition</c>.
        /// </summary>
        public Vector2 Tick(Motor2DInput input, float deltaTime)
        {
            // Clear one-frame triggers.
            _state.TriggerDashStarted = false;

            if (_state.IsDead)
            {
                _state.CurrentVelocity = Vector2.zero;
                return Vector2.zero;
            }

            // Dash timer.
            if (_state.IsDashing)
            {
                _state.DashTimer -= deltaTime;
                if (_state.DashTimer <= 0f)
                    _state.IsDashing = false;
            }

            if (_state.DashCooldownTimer > 0f)
                _state.DashCooldownTimer -= deltaTime;

            // Velocity.
            if (_state.IsDashing)
            {
                _state.CurrentVelocity = _state.DashDir * _config.DashSpeed;
            }
            else
            {
                Vector2 target = input.MoveDirection * _config.MoveSpeed;
                float   rate   = input.MoveDirection.sqrMagnitude > 0.01f
                    ? _config.Acceleration
                    : _config.Deceleration;

                _state.CurrentVelocity = rate > 0f
                    ? Vector2.MoveTowards(_state.CurrentVelocity, target, rate * deltaTime)
                    : target;

                // Snap to zero once drift falls below the stop threshold.
                if (input.MoveDirection.sqrMagnitude <= 0.01f
                    && _state.CurrentVelocity.magnitude < _config.StopThreshold * _config.MoveSpeed)
                {
                    _state.CurrentVelocity = Vector2.zero;
                }
            }

            return _state.CurrentVelocity;
        }

        // Action API

        /// <summary>
        /// Attempt to start a dash in <paramref name="direction"/>.
        /// Returns <c>true</c> and sets dash state if the conditions are met.
        /// </summary>
        public bool TryDash(Vector2 direction)
        {
            if (_state.IsDead)                              return false;
            if (!_config.DashEnabled)                       return false;
            if (_state.IsDashing)                           return false;
            if (_state.DashCooldownTimer > 0f)              return false;
            if (direction.sqrMagnitude < 0.01f)             return false;

            _state.DashDir           = direction.normalized;
            _state.IsDashing         = true;
            _state.DashTimer         = _config.DashDuration;
            _state.DashCooldownTimer = _config.DashCooldown;
            _state.TriggerDashStarted = true;
            return true;
        }

        /// <summary>
        /// Cancel any active dash immediately (e.g. dead-zone wall contact).
        /// </summary>
        public void CancelDash()
        {
            _state.IsDashing = false;
            _state.DashTimer = 0f;
            _state.CurrentVelocity = Vector2.zero;
        }

        /// <summary>
        /// Signal that the pawn has died. Zeroes velocity and locks the model.
        /// The MonoBehaviour layer drives all death visuals/audio separately.
        /// </summary>
        public void NotifyDead()
        {
            _state.IsDead          = true;
            _state.IsDashing       = false;
            _state.DashTimer       = 0f;
            _state.CurrentVelocity = Vector2.zero;
        }

        /// <summary>
        /// Fully reset all runtime state for round restart.
        /// </summary>
        public void ResetForRound()
        {
            _state.IsDead            = false;
            _state.IsDashing         = false;
            _state.DashDir           = Vector2.zero;
            _state.DashTimer         = 0f;
            _state.DashCooldownTimer = 0f;
            _state.CurrentVelocity   = Vector2.zero;
            _state.TriggerDashStarted = false;
        }
    }
}
