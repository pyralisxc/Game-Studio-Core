using NeonBlack.Gameplay.Features.Input;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    public sealed partial class Pawn2DMovementComponent
    {
        [Header("Movement - Speed")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField, Range(0f, 50f)] private float acceleration = 20f;
        [SerializeField, Range(0f, 50f)] private float deceleration = 25f;
        [SerializeField, Range(0f, 1f)] private float stopThreshold = 0.01f;

        [Header("Movement - Bounds")]
        [SerializeField] private float edgePadding = 0.05f;
        [SerializeField, Min(0f)] private float spriteRadius = 0.32f;
        [SerializeField] private Vector2 spriteRadiusOffset = Vector2.zero;
        [SerializeField] private bool showBoundsGizmo = true;
        [SerializeField] private bool screenWrap = false;
        [SerializeField, Tooltip("Use visible camera bounds as movement bounds when no PlayfieldProfile bounds are active. Leave off for normal authored movement; PlayfieldProfile owns legal movement space.")]
        private bool useCameraVisibleBoundsForMovement = false;
        [SerializeField, Tooltip("Optional gameplay state reader. When empty, the scene orchestrator should configure this component before play.")]
        private MonoBehaviour gameplayStateSource;

        [Header("Dash")]
        [SerializeField] private bool dashEnabled = true;
        [SerializeField] private float dashSpeed = 12f;
        [SerializeField, Range(0.05f, 0.5f)] private float dashDuration = 0.15f;
        [SerializeField, Range(0.1f, 3f)] private float dashCooldown = 0.8f;

        [Header("Side View Jump")]
        [SerializeField, Tooltip("Enable for side-view/platformer 2D pawns. Off means top-down/no-gravity movement with a Kinematic Rigidbody2D; on means side-view gravity with a Dynamic Rigidbody2D.")]
        private bool jumpEnabled = false;
        [SerializeField, Tooltip("Initial upward velocity applied when Jump is requested while grounded.")]
        private float jumpVelocity = 8f;
        [SerializeField, Tooltip("Gravity scale used while side-view jumping is enabled.")]
        private float gravityScale = 3f;
        [SerializeField, Tooltip("Maximum downward speed while side-view jumping is enabled.")]
        private float maxFallSpeed = 20f;
        [SerializeField, Tooltip("Layers this pawn treats as walkable ground for side-view jumping.")]
        private LayerMask groundLayer = Physics2D.DefaultRaycastLayers;
        [SerializeField, Tooltip("Ground check offset from the pawn root. Move it to the feet after the visual/collider are in place.")]
        private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
        [SerializeField, Min(0.01f), Tooltip("Radius used by the side-view ground check.")]
        private float groundCheckRadius = 0.12f;

        [Header("Dead Zones")]
        [SerializeField] private InputZoneSet inputZones;
    }
}
