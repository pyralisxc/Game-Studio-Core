using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.GameFlow;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Characters
{
    [AddComponentMenu("NeonBlack/Gameplay/Characters/2D/Pawn 2D Movement Component")]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public sealed class Pawn2DMovementComponent : MonoBehaviour, IPawnMotor, IMovementModule, IActorReactionResponder, IActorMovementModifierReceiver
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

        [Header("Dash")]
        [SerializeField] private bool dashEnabled = true;
        [SerializeField] private float dashSpeed = 12f;
        [SerializeField, Range(0.05f, 0.5f)] private float dashDuration = 0.15f;
        [SerializeField, Range(0.1f, 3f)] private float dashCooldown = 0.8f;

        [Header("Dead Zones")]
        [SerializeField] private InputZoneSet inputZones;

        private readonly Motor2DModel model = new Motor2DModel();

        private Rigidbody2D rb2d;
        private Camera mainCamera;
        private Vector2 moveDirection;
        private bool facingRight = true;
        private bool combatActionLocked;
        private bool statusActionLocked;
        private bool movementEnabled = true;
        private float reactionLockTimer;
        private float statusMoveSpeedMultiplier = 1f;
        private bool spriteDefaultFacesRight = true;

        private float TotalMargin => spriteRadius + edgePadding;

        public Vector2 MoveDirection
        {
            get => moveDirection;
            set => moveDirection = value;
        }

        public Vector2 CurrentVelocity => model.State.CurrentVelocity;
        public bool FacingRight => facingRight;
        public bool IsDashing => model.State.IsDashing;
        public bool IsDead => model.State.IsDead;
        public float DashCooldownRemaining => Mathf.Max(0f, model.State.DashCooldownTimer);
        public bool IsActionLocked => combatActionLocked || statusActionLocked || reactionLockTimer > 0f;
        public float MoveSpeed => moveSpeed * statusMoveSpeedMultiplier;
        public bool IsGrounded => true;

        private void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            rb2d.bodyType = RigidbodyType2D.Kinematic;
            rb2d.gravityScale = 0f;
            mainCamera = CameraAspectController.Main != null ? CameraAspectController.Main : Camera.main;
            if (mainCamera == null)
                Debug.LogError("[Pawn2DMovementComponent] Camera.main is null. Make sure your Main Camera is tagged 'MainCamera'.");
            model.Configure(BuildMotorConfig());
        }

        private void Start()
        {
            ClampPositionToBounds();
        }

        private void FixedUpdate()
        {
            if (mainCamera == null)
                return;
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                return;
            if (!movementEnabled || model.State.IsDead)
                return;

            GetBounds(out float hw, out float hh);

            Vector2 velocity = model.Tick(BuildMotorInput(), Time.fixedDeltaTime);
            Vector2 newPos = rb2d.position + velocity * Time.fixedDeltaTime;

            Vector2 centrePos = newPos + spriteRadiusOffset;
            Vector3 camPos = mainCamera.transform.position;

            if (screenWrap)
            {
                if (centrePos.x > camPos.x + hw) centrePos.x = camPos.x - hw;
                if (centrePos.x < camPos.x - hw) centrePos.x = camPos.x + hw;
                if (centrePos.y > camPos.y + hh) centrePos.y = camPos.y - hh;
                if (centrePos.y < camPos.y - hh) centrePos.y = camPos.y + hh;
            }
            else
            {
                centrePos.x = Mathf.Clamp(centrePos.x, camPos.x - hw, camPos.x + hw);
                centrePos.y = Mathf.Clamp(centrePos.y, camPos.y - hh, camPos.y + hh);
            }

            newPos = centrePos - spriteRadiusOffset;

            if (inputZones != null && inputZones.IsInAnyDeadZone(newPos))
            {
                Vector2 currentPos = rb2d.position;
                Vector2 slideX = new Vector2(newPos.x, currentPos.y);
                if (!inputZones.IsInAnyDeadZone(slideX))
                {
                    newPos = slideX;
                }
                else
                {
                    Vector2 slideY = new Vector2(currentPos.x, newPos.y);
                    newPos = !inputZones.IsInAnyDeadZone(slideY) ? slideY : currentPos;
                }

                if (model.State.IsDashing)
                    model.CancelDash();
            }

            rb2d.MovePosition(newPos);
        }

        private void Update()
        {
            if (reactionLockTimer > 0f)
            {
                reactionLockTimer = Mathf.Max(0f, reactionLockTimer - Time.deltaTime);
                if (reactionLockTimer <= 0f)
                    moveDirection = Vector2.zero;
            }

            if (moveDirection.x > 0.05f)
                facingRight = true;
            else if (moveDirection.x < -0.05f)
                facingRight = false;
        }

        public void Move(Vector2 input)
        {
            moveDirection = input;
        }

        public void Jump()
        {
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;
            if (!enabled)
                moveDirection = Vector2.zero;
        }

        public bool TryDash(Vector2 direction) => model.TryDash(direction);

        public void ResetForRound(Vector3 position)
        {
            model.ResetForRound();
            combatActionLocked = false;
            statusActionLocked = false;
            reactionLockTimer = 0f;
            moveDirection = Vector2.zero;
            transform.position = position;

            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
                rb2d.position = position;
            }
        }

        public void NotifyDeath()
        {
            if (model.State.IsDead)
                return;

            model.NotifyDead();
            moveDirection = Vector2.zero;
        }

        public void ResetMoveToIdle()
        {
            moveDirection = Vector2.zero;
        }

        public void SetActionLock(bool locked)
        {
            combatActionLocked = locked;
            if (locked)
                moveDirection = Vector2.zero;
        }

        public void ApplyReactionLock(float duration)
        {
            reactionLockTimer = Mathf.Max(reactionLockTimer, duration);
            moveDirection = Vector2.zero;
        }

        public void ClearReactionLock()
        {
            reactionLockTimer = 0f;
        }

        public void SetStatusMoveSpeedMultiplier(float multiplier)
        {
            statusMoveSpeedMultiplier = Mathf.Max(multiplier, 0f);
            model.Configure(BuildMotorConfig());
        }

        public void SetStatusActionLock(bool locked)
        {
            statusActionLocked = locked;
            if (locked)
                moveDirection = Vector2.zero;
        }

        public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile profile)
        {
            if (profile == null)
                return;

            moveSpeed = profile.walkSpeed;
            acceleration = profile.acceleration;
            deceleration = profile.deceleration;
            screenWrap = profile.allowScreenWrap;
            model.Configure(BuildMotorConfig());
        }

        public void ApplyPresentationProfile(PawnPresentationProfile profile)
        {
            if (profile == null)
                return;

            spriteDefaultFacesRight = profile.spriteDefaultFacesRight;
        }

        public void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile)
        {
            ApplyPresentationProfile(presentationProfile);
        }

        private void GetBounds(out float hw, out float hh)
        {
            if (CameraAspectController.Instance != null)
            {
                hw = Mathf.Max(0f, CameraAspectController.Instance.HalfWidth - TotalMargin);
                hh = Mathf.Max(0f, CameraAspectController.Instance.HalfHeight - TotalMargin);
            }
            else
            {
                hw = Mathf.Max(0f, mainCamera.orthographicSize * mainCamera.aspect - TotalMargin);
                hh = Mathf.Max(0f, mainCamera.orthographicSize - TotalMargin);
            }
        }

        private void ClampPositionToBounds()
        {
            if (rb2d == null || mainCamera == null)
                return;

            GetBounds(out float hw, out float hh);
            Vector3 camPos = mainCamera.transform.position;
            Vector2 pivotPos = rb2d.position;
            Vector2 centrePos = pivotPos + spriteRadiusOffset;
            Vector2 clampedCentre;
            clampedCentre.x = Mathf.Clamp(centrePos.x, camPos.x - hw, camPos.x + hw);
            clampedCentre.y = Mathf.Clamp(centrePos.y, camPos.y - hh, camPos.y + hh);
            rb2d.MovePosition(clampedCentre - spriteRadiusOffset);
        }

        private Motor2DInput BuildMotorInput() => new Motor2DInput
        {
            MoveDirection = IsActionLocked ? Vector2.zero : moveDirection
        };

        private Motor2DConfig BuildMotorConfig() => new Motor2DConfig
        {
            MoveSpeed = moveSpeed * statusMoveSpeedMultiplier,
            Acceleration = acceleration,
            Deceleration = deceleration,
            StopThreshold = stopThreshold,
            DashEnabled = dashEnabled,
            DashSpeed = dashSpeed * statusMoveSpeedMultiplier,
            DashDuration = dashDuration,
            DashCooldown = dashCooldown
        };

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!showBoundsGizmo)
                return;

            Vector3 centre = transform.position + (Vector3)spriteRadiusOffset;
            UnityEditor.Handles.color = new Color(0f, 1f, 0.4f, 0.35f);
            UnityEditor.Handles.DrawSolidDisc(centre, Vector3.back, spriteRadius);
            UnityEditor.Handles.color = new Color(0f, 1f, 0.4f, 1f);
            UnityEditor.Handles.DrawWireDisc(centre, Vector3.back, spriteRadius);
            UnityEditor.Handles.DrawSolidDisc(centre, Vector3.back, 0.02f);

            float total = spriteRadius + edgePadding;
            UnityEditor.Handles.color = new Color(1f, 0.6f, 0f, 0.6f);
            UnityEditor.Handles.DrawWireDisc(centre, Vector3.back, total);

            UnityEditor.EditorGUI.BeginChangeCheck();
            Vector3 newCentre = UnityEditor.Handles.PositionHandle(centre, Quaternion.identity);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                UnityEditor.Undo.RecordObject(this, "Move Sprite Radius Offset");
                spriteRadiusOffset = newCentre - transform.position;
            }
        }
#endif
    }
}
