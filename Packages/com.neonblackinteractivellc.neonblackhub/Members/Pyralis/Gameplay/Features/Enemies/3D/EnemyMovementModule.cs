using UnityEngine;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Presentation.Visuals;
using NeonBlack.Gameplay.Features.Combat;

namespace NeonBlack.Gameplay.Features.Enemies
{
    [RequireComponent(typeof(CharacterController))]
    public class EnemyMovementModule : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private MovementMode movementMode = MovementMode.ThreeD;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer = Physics.DefaultRaycastLayers;

        private CharacterController _controller;
        private KnockbackReceiver _knockbackReceiver;
        private BillboardFacing3D _billboardFacing;
        
        private float _verticalVel;
        private bool _isGrounded;

        public bool IsGrounded => _isGrounded;
        public float VerticalVelocity => _verticalVel;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _knockbackReceiver = GetComponent<KnockbackReceiver>();
            _billboardFacing = GetComponent<BillboardFacing3D>();
        }

        public void Tick(float deltaTime)
        {
            ApplyGravity(deltaTime);
            
            if (_knockbackReceiver != null)
                _knockbackReceiver.Tick(deltaTime);
        }

        private void ApplyGravity(float deltaTime)
        {
            Vector3 feetPos = transform.position + _controller.center
            + Vector3.down * (_controller.height * 0.5f - _controller.radius);
            _isGrounded = Physics.CheckSphere(feetPos, groundCheckRadius, groundLayer,
            QueryTriggerInteraction.Ignore);

            if (_isGrounded && _verticalVel < 0f) _verticalVel = -2f;
            _verticalVel += gravity * deltaTime;
        }

        public void MoveToward(Vector3 worldTarget, float speed, float statusMoveSpeedMultiplier, Camera cam, Transform visualRoot, bool spriteDefaultFacesRight, HitBoxSlot[] hitBoxZones)
        {
            Vector3 dir = worldTarget - transform.position;
            dir.y = 0f;
            if (movementMode == MovementMode.TwoD) dir.z = 0f;

            Vector3 kb = _knockbackReceiver != null ? _knockbackReceiver.Velocity : Vector3.zero;

            if (dir.sqrMagnitude < 0.01f)
            {
                _controller.Move(new Vector3(kb.x, _verticalVel + kb.y, kb.z) * Time.deltaTime);
                return;
            }

            dir.Normalize();
            Vector3 move = dir * speed * statusMoveSpeedMultiplier;
            _controller.Move(new Vector3(move.x + kb.x, _verticalVel + kb.y, move.z + kb.z) * Time.deltaTime);

            FaceTarget(worldTarget, cam, visualRoot, spriteDefaultFacesRight, hitBoxZones);
        }

        public void ApplyStationaryMotion(float deltaTime)
        {
            Vector3 kb = _knockbackReceiver != null ? _knockbackReceiver.Velocity : Vector3.zero;
            _controller.Move(new Vector3(kb.x, _verticalVel + kb.y, kb.z) * deltaTime);
        }

        public void FaceTarget(Vector3 worldTarget, Camera cam, Transform visualRoot, bool spriteDefaultFacesRight, HitBoxSlot[] hitBoxZones)
        {
            Vector3 toTarget = worldTarget - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0025f) return;

            float dot;
            if (cam != null)
            {
                Vector3 camRight = cam.transform.right;
                camRight.y = 0f;
                dot = Vector3.Dot(toTarget, camRight);
            }
            else
            {
                dot = toTarget.x;
            }
            if (Mathf.Abs(dot) <= 0.05f) return;

            bool faceRight = dot > 0f;

            if (_billboardFacing != null)
            {
                _billboardFacing.ApplyFacing(faceRight);
            }
            else if (visualRoot != null)
            {
                Vector3 s = visualRoot.localScale;
                s.x = (faceRight == spriteDefaultFacesRight) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
                visualRoot.localScale = s;
            }
            else
            {
                SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                    sr.flipX = spriteDefaultFacesRight ? !faceRight : faceRight;
            }

            if (hitBoxZones != null)
                foreach (var slot in hitBoxZones)
                    slot.MirrorToSide(transform, faceRight);
        }
    }
}
