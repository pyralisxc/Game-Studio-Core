using Unity.Cinemachine;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace NeonBlack.Gameplay.Presentation.Camera
{
    /// <summary>
    /// Manages a Cinemachine virtual camera for a gameplay session. Tracks all active
    /// participants and keeps them framed, with support for shared and split-screen modes,
    /// playfield bounds clamping, runtime profile switching, and optional scroll zoom.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Camera,
        CapabilityPath = "World & Meta/Camera/Cinemachine Rig",
        RuntimeFamilies = new[] { RuntimeCapabilityFamily.CameraInput },
        Relevance = "Cinemachine Camera Rig Controller is the Pyralis scene camera runtime. Use this Inspector for assigned references and tuning values.",
        RoleTags = new[] { "CameraRig", "Cinemachine", "ParticipantFollow" },
        NativeSetup = new[] 
        { 
            "Add to CameraRig root.",
            "Assign Target Camera (physical Unity Camera with Cinemachine Brain).",
            "Assign Camera Rig Profile (shared/split behavior, zoom, damping).",
            "Assign Shared Camera Behaviour (Cinemachine virtual camera).",
            "For pawn-follow cameras, add PawnCameraTarget to the pawn prefab or use the pawn root fallback."
        },
        AssignmentFields = new[] { nameof(cameraRigProfile), nameof(sharedCameraBehaviour), nameof(targetCamera), nameof(explicitFocusTarget) },
        FirstProofTargetId = "proof.camera-cursor-world",
        FirstProof = "Enter Play Mode. Verify the camera rig frames the spawned player pawn automatically. In Split-Screen, verify two viewports are created.",
        ExpertAdvice = "Pyralis chooses the gameplay focus target; Cinemachine composes the view. Use Manual Cinemachine when you want scene-authored Follow/LookAt, Participant Group for shared pawn framing, Participant Pawns for per-participant cameras, Playfield Center for board/menu/playfield views, and Explicit Scene Target for a hand-authored anchor.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/camera"
    )]
[AddComponentMenu("NeonBlack/Gameplay/Camera/Cinemachine Camera Rig Controller")]
    [ExecuteAlways]
    public class CinemachineCameraRigController : MonoBehaviour, ICameraBoundsProvider
{
        [SerializeField] private CameraRigProfile cameraRigProfile;
        [SerializeField] private PlayfieldProfile playfieldProfile;
        [SerializeField] private ParticipantRosterService participantRoster;
        [SerializeField] private MonoBehaviour sharedCameraBehaviour;
        [SerializeField] private MonoBehaviour[] splitScreenCameraBehaviours;
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private Transform explicitFocusTarget;

        [Header("2D Bounds Framing")]
        [Tooltip("When the target camera is orthographic, keep at least this world rectangle visible and expose the visible area as ICameraBoundsProvider.")]
        [SerializeField] private bool enforceMinimumVisibleArea2D = true;
        [SerializeField, Tooltip("Minimum world units visible horizontally in landscape. Default 19.2 = 1920px at 100px/unit.")]
        private float minWorldWidth2D = 19.2f;
        [SerializeField, Tooltip("Minimum world units visible vertically in landscape. Default 10.8 = 1080px at 100px/unit.")]
        private float minWorldHeight2D = 10.8f;
        [SerializeField, Tooltip("Minimum world units visible horizontally in portrait. Set to 0 to reuse the landscape values.")]
        private float portraitMinWorldWidth2D = 10.8f;
        [SerializeField, Tooltip("Minimum world units visible vertically in portrait. Set to 0 to reuse the landscape values.")]
        private float portraitMinWorldHeight2D = 19.2f;
        [SerializeField, Tooltip("Add black bars to enforce exactly the minimum rectangle. Leave false so extra screen area shows more world.")]
        private bool letterbox2D = false;

        [Header("Scroll Zoom")]
        [Tooltip("Allow mouse scroll wheel to nudge the zoom at runtime.")]
        [SerializeField] private bool allowScrollZoom = false;
        [Tooltip("How fast scroll adjusts zoom. Higher = faster.")]
        [SerializeField] private float scrollZoomSpeed = 2f;

        private Transform _sharedFocusTarget;
        private bool _sharedFocusInitialized;

        // Scroll zoom state.
        private float _scrollZoomOffset;
        private int _cachedScreenWidth;
        private int _cachedScreenHeight;

        // Profile blend state.
        private CameraRigProfile _blendFromProfile;
        private float _blendT        = 1f;   // 0 = blend start, 1 = complete
        private float _blendDuration = 0.5f;

        public Transform RuntimeSharedFocusTarget => _sharedFocusTarget;
        public Object RuntimeSharedCameraBehaviour => sharedCameraBehaviour;
        public Object RuntimeTargetCamera => ResolveTargetCamera();
        public int RuntimeParticipantCount => participantRoster != null ? participantRoster.Participants.Count : 0;
        public bool RuntimeUsingProfileTransform => cameraRigProfile != null && cameraRigProfile.useProfileTransform;
        public CameraRigProfile.CameraFocusMode RuntimeFocusMode => cameraRigProfile != null ? cameraRigProfile.focusMode : CameraRigProfile.CameraFocusMode.ParticipantGroup;
        public float RuntimeFollowDamping => cameraRigProfile != null ? BlendFloat(p => p.followDamping) : 0f;
        public Vector3 RuntimeFollowOffset => cameraRigProfile != null ? BlendVector(p => p.followOffset) : Vector3.zero;
        public Vector3 RuntimeViewEulerAngles => cameraRigProfile != null ? BlendVector(p => p.viewEulerAngles) : Vector3.zero;
        public float RuntimeProfileOrthographicSize => cameraRigProfile != null ? BlendFloat(p => p.orthographicSize) : 0f;
        public float RuntimeSharedCinemachineOrthographicSize => TryGetCinemachineLens(sharedCameraBehaviour, out LensSettings lens) ? lens.OrthographicSize : 0f;
        public bool RuntimeEnforceMinimumVisibleArea2D => enforceMinimumVisibleArea2D;
        public float RuntimeMinimumOrthographicSize2D => CalculateRequiredOrthographicSize2D();
        public bool RuntimeOrthographicSizeClampedBy2DVisibleArea
        {
            get
            {
                UnityEngine.Camera camera = ResolveTargetCamera();
                return enforceMinimumVisibleArea2D
                    && camera != null
                    && camera.orthographic
                    && camera.orthographicSize <= CalculateRequiredOrthographicSize2D() + 0.01f;
            }
        }

        [Inject]
        private void Construct(ParticipantRosterService rosterService = null)
        {
            participantRoster = rosterService != null
                ? rosterService
                : participantRoster;
        }

        private void Awake()
        {
            if (!Application.isPlaying) return;

            ResolveTargetCamera();
            EnsureSharedFocusTarget();
            ApplyMinimumVisibleArea2D(force: true);
        }

        private void OnEnable()
        {
            SubscribeToRoster(participantRoster);
        }

        private void OnDisable()
        {
            SubscribeToRoster(null);
        }

        private void Update()
        {
            if (Application.isPlaying)
                return;

            ResolveTargetCamera();
            ApplyCameraLensDefaults(0f, instant: true);
            ApplyMinimumVisibleArea2D(force: false);
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            if (participantRoster == null || cameraRigProfile == null)
            {
                ApplyMinimumVisibleArea2D(force: true);
                return;
            }

            // Advance profile blend.
            if (_blendT < 1f)
                _blendT = Mathf.MoveTowards(_blendT, 1f, Time.deltaTime / Mathf.Max(0.001f, _blendDuration));

            HandleScrollZoom();

            ApplyCameraFocus();

            ApplyMinimumVisibleArea2D(force: true);
        }

        public bool TryGetCameraBounds2D(float margin, out CameraBounds2D bounds)
        {
            UnityEngine.Camera camera = ResolveTargetCamera();
            if (camera == null || !camera.orthographic)
            {
                bounds = default;
                return false;
            }

            ApplyMinimumVisibleArea2D(force: true);
            bounds = new CameraBounds2D(
                camera,
                camera.transform.position,
                Mathf.Max(0.1f, camera.orthographicSize * camera.aspect - margin),
                Mathf.Max(0.1f, camera.orthographicSize - margin));
            return true;
        }

        public void SetProfile(CameraRigProfile profile)
        {
            cameraRigProfile = profile;
            cameraRigProfile?.Sanitize();
        }

        public void SetParticipantRoster(ParticipantRosterService rosterService)
        {
            SubscribeToRoster(rosterService);
        }

        /// <summary>
        /// Smoothly transitions to a new CameraRigProfile over the given duration.
        /// Safe to call every frame; does nothing if already on that profile.
        /// </summary>
        public void SwitchProfile(CameraRigProfile profile, float transitionDuration = 0.5f)
        {
            if (profile == null || profile == cameraRigProfile) return;
            _blendFromProfile = cameraRigProfile;
            _blendDuration    = Mathf.Max(0f, transitionDuration);
            _blendT           = 0f;
            cameraRigProfile  = profile;
            cameraRigProfile.Sanitize();
        }

        public void SetGameMode(GameModeDefinition gameModeDefinition)
        {
            if (gameModeDefinition == null)
                return;

            cameraRigProfile = gameModeDefinition.cameraRigProfile;
            playfieldProfile = gameModeDefinition.playfieldProfile;
            cameraRigProfile?.Sanitize();
            playfieldProfile?.Sanitize();
        }

        public void SetTargetCamera(UnityEngine.Camera camera)
        {
            targetCamera = camera;
            ApplyMinimumVisibleArea2D(force: true);
        }

        public void SetExplicitFocusTarget(Transform target)
        {
            explicitFocusTarget = target;
            _sharedFocusInitialized = false;
        }

        private void ApplyCameraFocus()
        {
            if (cameraRigProfile == null)
                return;

            if (cameraRigProfile.focusMode == CameraRigProfile.CameraFocusMode.ManualCinemachine)
            {
                ApplyCameraLensDefaults(0f);
                return;
            }

            if (cameraRigProfile.focusMode == CameraRigProfile.CameraFocusMode.PlayfieldCenter)
            {
                ApplySingleFocusTarget(ResolvePlayfieldCenter(), 0f);
                return;
            }

            if (cameraRigProfile.focusMode == CameraRigProfile.CameraFocusMode.ExplicitSceneTarget)
            {
                if (explicitFocusTarget != null)
                    ApplySingleFocusTarget(explicitFocusTarget.position, 0f, explicitFocusTarget);
                else
                    ApplyCameraLensDefaults(0f);
                return;
            }

            if (cameraRigProfile.focusMode == CameraRigProfile.CameraFocusMode.ParticipantPawns
                && cameraRigProfile.presentationMode == CameraRigProfile.CameraPresentationMode.SplitScreen)
            {
                ApplySplitScreenCameras();
                return;
            }

            if (cameraRigProfile.focusMode == CameraRigProfile.CameraFocusMode.ParticipantPawns)
            {
                ApplyPrimaryParticipantCamera();
                return;
            }

            ApplySharedParticipantGroupCamera();
        }

        private void ApplySharedParticipantGroupCamera()
        {
            if (sharedCameraBehaviour == null || participantRoster == null || participantRoster.Participants.Count == 0)
                return;

            EnsureSharedFocusTarget();

            int targetCount = 0;
            Vector3 centroid = Vector3.zero;
            float maxDistance = 0f;
            for (int i = 0; i < participantRoster.Participants.Count; i++)
            {
                ParticipantHandle participant = participantRoster.Participants[i];
                if (participant.PawnInstance == null)
                    continue;

                Transform followTarget = ResolveParticipantFollowTarget(participant);
                Vector3 position = followTarget.position;
                centroid += position;
                targetCount++;
            }

            if (targetCount == 0)
                return;

            centroid /= targetCount;

            for (int i = 0; i < participantRoster.Participants.Count; i++)
            {
                ParticipantHandle participant = participantRoster.Participants[i];
                if (participant.PawnInstance == null)
                    continue;

                Transform followTarget = ResolveParticipantFollowTarget(participant);
                maxDistance = Mathf.Max(maxDistance, Vector3.Distance(centroid, followTarget.position));
            }

            Vector3 desiredFocus = ClampToPlayfield(centroid);
            ApplySingleFocusTarget(desiredFocus, maxDistance);
        }

        private void ApplyPrimaryParticipantCamera()
        {
            if (participantRoster == null || participantRoster.Participants.Count == 0)
                return;

            for (int i = 0; i < participantRoster.Participants.Count; i++)
            {
                ParticipantHandle participant = participantRoster.Participants[i];
                if (participant?.PawnInstance == null)
                    continue;

                Transform followTarget = ResolveParticipantFollowTarget(participant);
                Transform lookAtTarget = ResolveParticipantLookAtTarget(participant);
                ApplyCinemachineTarget(sharedCameraBehaviour, followTarget, lookAtTarget);
                ApplyCameraLensDefaults(0f);
                return;
            }
        }

        private void ApplySplitScreenCameras()
        {
            if (participantRoster == null || splitScreenCameraBehaviours == null || splitScreenCameraBehaviours.Length == 0)
                return;

            int count = Mathf.Min(splitScreenCameraBehaviours.Length, participantRoster.Participants.Count);
            for (int i = 0; i < count; i++)
            {
                MonoBehaviour cameraBehaviour = splitScreenCameraBehaviours[i];
                ParticipantHandle participant = participantRoster.Participants[i];
                if (cameraBehaviour == null || participant.PawnInstance == null)
                    continue;

                ApplyCinemachineTarget(
                    cameraBehaviour,
                    ResolveParticipantFollowTarget(participant),
                    ResolveParticipantLookAtTarget(participant));
            }
        }

        private void ApplySingleFocusTarget(Vector3 desiredFocus, float participantSpread, Transform directTarget = null)
        {
            EnsureSharedFocusTarget();
            Transform followTarget = directTarget != null ? directTarget : _sharedFocusTarget;
            Transform lookAtTarget = followTarget;

            if (directTarget == null)
                MoveSharedFocusTarget(ClampToPlayfield(desiredFocus));

            ApplyCinemachineTarget(sharedCameraBehaviour, followTarget, lookAtTarget);
            ApplyCameraLensDefaults(participantSpread);
        }

        private void MoveSharedFocusTarget(Vector3 desiredFocus)
        {
            float followDamping = BlendFloat(p => p.followDamping);
            if (!_sharedFocusInitialized || followDamping <= 0f)
            {
                _sharedFocusTarget.position = desiredFocus;
                _sharedFocusInitialized = true;
                return;
            }

            _sharedFocusTarget.position = Vector3.Lerp(
                _sharedFocusTarget.position,
                desiredFocus,
                Mathf.Clamp01(Time.deltaTime * followDamping));
        }

        private void ApplyCinemachineTarget(MonoBehaviour cameraBehaviour, Transform followTarget, Transform lookAtTarget)
        {
            if (cameraBehaviour is not CinemachineVirtualCameraBase vcam || followTarget == null)
                return;

            vcam.Follow = followTarget;
            vcam.LookAt = lookAtTarget != null ? lookAtTarget : followTarget;
            if (cameraRigProfile != null
                && cameraRigProfile.presentationMode == CameraRigProfile.CameraPresentationMode.Shared
                && followTarget == _sharedFocusTarget)
            {
                ApplySharedCameraTransform(vcam);
            }
        }

        private Transform ResolveParticipantFollowTarget(ParticipantHandle participant)
        {
            if (participant?.PawnInstance == null)
                return null;

            PawnCameraTarget cameraTarget = participant.PawnInstance.GetComponentInChildren<PawnCameraTarget>(true);
            return cameraTarget != null ? cameraTarget.FollowTarget : participant.PawnInstance.transform;
        }

        private Transform ResolveParticipantLookAtTarget(ParticipantHandle participant)
        {
            if (participant?.PawnInstance == null)
                return null;

            PawnCameraTarget cameraTarget = participant.PawnInstance.GetComponentInChildren<PawnCameraTarget>(true);
            return cameraTarget != null ? cameraTarget.LookAtTarget : participant.PawnInstance.transform;
        }

        private void ApplyCameraLensDefaults(float participantSpread, bool instant = false)
        {
            if (targetCamera == null || cameraRigProfile == null)
                return;

            targetCamera.orthographic = cameraRigProfile.orthographic;
            if (cameraRigProfile.orthographic)
            {
                // Blend between profiles when transitioning.
                float minZoom    = BlendFloat(p => p.minZoom);
                float maxZoom    = BlendFloat(p => p.maxZoom);
                float zoomDamp   = BlendFloat(p => p.zoomDamping);
                float orthoSize  = BlendFloat(p => p.orthographicSize);

                float desiredSize = Mathf.Clamp(orthoSize + participantSpread + _scrollZoomOffset, minZoom, maxZoom);
                targetCamera.orthographicSize = instant || zoomDamp <= 0f
                    ? desiredSize
                    : Mathf.Lerp(targetCamera.orthographicSize, desiredSize, Mathf.Clamp01(Time.deltaTime * zoomDamp));
            }

            ApplyCinemachineLensDefaults(targetCamera.orthographic, targetCamera.orthographicSize);
        }

        private void ApplySharedCameraTransform(CinemachineVirtualCameraBase sharedVcam)
        {
            if (sharedVcam == null || cameraRigProfile == null || !cameraRigProfile.useProfileTransform)
                return;

            Vector3 followOffset = BlendVector(p => p.followOffset);
            Vector3 viewEulerAngles = BlendVector(p => p.viewEulerAngles);
            sharedVcam.transform.SetPositionAndRotation(
                _sharedFocusTarget.position + followOffset,
                Quaternion.Euler(viewEulerAngles));
        }

        private float BlendFloat(System.Func<CameraRigProfile, float> selector)
        {
            float to = selector(cameraRigProfile);
            if (_blendFromProfile == null || _blendT >= 1f) return to;
            return Mathf.Lerp(selector(_blendFromProfile), to, _blendT);
        }

        private Vector3 BlendVector(System.Func<CameraRigProfile, Vector3> selector)
        {
            Vector3 to = selector(cameraRigProfile);
            if (_blendFromProfile == null || _blendT >= 1f) return to;
            return Vector3.Lerp(selector(_blendFromProfile), to, _blendT);
        }

        private UnityEngine.Camera ResolveTargetCamera()
        {
            if (targetCamera == null)
                targetCamera = GetComponentInChildren<UnityEngine.Camera>(true);

            return targetCamera;
        }

        private void ApplyMinimumVisibleArea2D(bool force)
        {
            UnityEngine.Camera camera = ResolveTargetCamera();
            if (!enforceMinimumVisibleArea2D || camera == null || !camera.orthographic)
                return;

            if (!force && _cachedScreenWidth == Screen.width && _cachedScreenHeight == Screen.height)
                return;

            _cachedScreenWidth = Screen.width;
            _cachedScreenHeight = Screen.height;
            camera.rect = new Rect(0f, 0f, 1f, 1f);

            GetMinimumVisibleDimensions(out float minWidth, out float minHeight);
            if (letterbox2D)
                ApplyLetterbox2D(camera, minWidth, minHeight);
            else
                ApplyShowAll2D(camera, minWidth, minHeight);

            ApplyCinemachineLensDefaults(camera.orthographic, camera.orthographicSize);
        }

        private void GetMinimumVisibleDimensions(out float minWidth, out float minHeight)
        {
            bool isPortrait = Screen.height >= Screen.width;
            if (isPortrait && portraitMinWorldWidth2D > 0f && portraitMinWorldHeight2D > 0f)
            {
                minWidth = portraitMinWorldWidth2D;
                minHeight = portraitMinWorldHeight2D;
                return;
            }

            minWidth = minWorldWidth2D;
            minHeight = minWorldHeight2D;
        }

        private static void ApplyShowAll2D(UnityEngine.Camera camera, float minWidth, float minHeight)
        {
            camera.orthographicSize = Mathf.Max(camera.orthographicSize, CalculateRequiredOrthographicSize2D(minWidth, minHeight));
        }

        private void ApplyCinemachineLensDefaults(bool orthographic, float orthographicSize)
        {
            ApplyCinemachineLensDefaults(sharedCameraBehaviour, orthographic, orthographicSize);

            if (splitScreenCameraBehaviours == null)
                return;

            for (int i = 0; i < splitScreenCameraBehaviours.Length; i++)
                ApplyCinemachineLensDefaults(splitScreenCameraBehaviours[i], orthographic, orthographicSize);
        }

        private static void ApplyCinemachineLensDefaults(MonoBehaviour cameraBehaviour, bool orthographic, float orthographicSize)
        {
            if (cameraBehaviour is not CinemachineCamera cinemachineCamera)
                return;

            LensSettings lens = cinemachineCamera.Lens;
            lens.ModeOverride = orthographic
                ? LensSettings.OverrideModes.Orthographic
                : LensSettings.OverrideModes.Perspective;
            if (orthographic)
                lens.OrthographicSize = orthographicSize;
            cinemachineCamera.Lens = lens;
        }

        private static bool TryGetCinemachineLens(MonoBehaviour cameraBehaviour, out LensSettings lens)
        {
            if (cameraBehaviour is CinemachineCamera cinemachineCamera)
            {
                lens = cinemachineCamera.Lens;
                return true;
            }

            lens = default;
            return false;
        }

        private float CalculateRequiredOrthographicSize2D()
        {
            if (!enforceMinimumVisibleArea2D)
                return 0f;

            GetMinimumVisibleDimensions(out float minWidth, out float minHeight);
            return letterbox2D
                ? minHeight * 0.5f
                : CalculateRequiredOrthographicSize2D(minWidth, minHeight);
        }

        private static float CalculateRequiredOrthographicSize2D(float minWidth, float minHeight)
        {
            float screenAspect = Mathf.Max(0.01f, (float)Screen.width / Mathf.Max(1, Screen.height));
            return Mathf.Max(minHeight * 0.5f, (minWidth * 0.5f) / screenAspect);
        }

        private static void ApplyLetterbox2D(UnityEngine.Camera camera, float minWidth, float minHeight)
        {
            float minAspect = minWidth / Mathf.Max(0.01f, minHeight);
            float screenAspect = Mathf.Max(0.01f, (float)Screen.width / Mathf.Max(1, Screen.height));

            camera.orthographicSize = minHeight * 0.5f;
            if (Mathf.Approximately(screenAspect, minAspect))
                return;

            if (screenAspect > minAspect)
            {
                float width = minAspect / screenAspect;
                camera.rect = new Rect((1f - width) * 0.5f, 0f, width, 1f);
            }
            else
            {
                float height = screenAspect / minAspect;
                camera.rect = new Rect(0f, (1f - height) * 0.5f, 1f, height);
            }
        }

        private void HandleScrollZoom()
        {
            if (!allowScrollZoom) return;
            if (Mouse.current == null) return;
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.01f) return;
            _scrollZoomOffset -= scroll * scrollZoomSpeed * Time.deltaTime;
            // Keep offset within a reasonable range so it can't escape profile bounds.
            float maxOffset = (cameraRigProfile.maxZoom - cameraRigProfile.minZoom) * 0.5f;
            _scrollZoomOffset = Mathf.Clamp(_scrollZoomOffset, -maxOffset, maxOffset);
        }

        private Vector3 ClampToPlayfield(Vector3 candidate)
        {
            if (playfieldProfile == null || !cameraRigProfile.lockToPlayfield || !playfieldProfile.clampToBounds)
                return candidate;

            playfieldProfile.Sanitize();
            candidate.x = Mathf.Clamp(candidate.x, playfieldProfile.minBounds.x, playfieldProfile.maxBounds.x);
            candidate.y = Mathf.Clamp(candidate.y, playfieldProfile.minBounds.y, playfieldProfile.maxBounds.y);
            candidate.z = Mathf.Clamp(candidate.z, playfieldProfile.minDepth, playfieldProfile.maxDepth);
            return candidate;
        }

        private Vector3 ResolvePlayfieldCenter()
        {
            if (playfieldProfile == null || !playfieldProfile.clampToBounds)
                return explicitFocusTarget != null ? explicitFocusTarget.position : transform.position;

            Vector2 center = (playfieldProfile.minBounds + playfieldProfile.maxBounds) * 0.5f;
            float depth = (playfieldProfile.minDepth + playfieldProfile.maxDepth) * 0.5f;
            return new Vector3(center.x, center.y, depth);
        }

        private void SubscribeToRoster(ParticipantRosterService roster)
        {
            if (participantRoster != null)
            {
                participantRoster.ParticipantPawnAssigned -= HandleParticipantPawnAssigned;
                participantRoster.ParticipantPawnCleared -= HandleParticipantPawnCleared;
            }

            participantRoster = roster;
            if (participantRoster == null)
                return;

            participantRoster.ParticipantPawnAssigned -= HandleParticipantPawnAssigned;
            participantRoster.ParticipantPawnAssigned += HandleParticipantPawnAssigned;
            participantRoster.ParticipantPawnCleared -= HandleParticipantPawnCleared;
            participantRoster.ParticipantPawnCleared += HandleParticipantPawnCleared;
        }

        private void HandleParticipantPawnAssigned(ParticipantHandle participant, GameObject pawn)
        {
            _sharedFocusInitialized = false;
        }

        private void HandleParticipantPawnCleared(ParticipantHandle participant, GameObject pawn)
        {
            _sharedFocusInitialized = false;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (minWorldWidth2D <= 0f)
                minWorldWidth2D = 19.2f;

            if (minWorldHeight2D <= 0f)
                minWorldHeight2D = 10.8f;

            if (portraitMinWorldWidth2D <= 0f)
                portraitMinWorldWidth2D = 10.8f;

            if (portraitMinWorldHeight2D <= 0f)
                portraitMinWorldHeight2D = 19.2f;

            ApplyCameraLensDefaults(0f, instant: true);
            ApplyMinimumVisibleArea2D(force: true);
        }
#endif

        private void EnsureSharedFocusTarget()
        {
            if (_sharedFocusTarget != null)
                return;

            GameObject focusTarget = new GameObject("GameplaySharedCameraFocus");
            focusTarget.hideFlags = HideFlags.HideAndDontSave;
            focusTarget.transform.SetParent(transform, false);
            _sharedFocusTarget = focusTarget.transform;
        }
    }
}
