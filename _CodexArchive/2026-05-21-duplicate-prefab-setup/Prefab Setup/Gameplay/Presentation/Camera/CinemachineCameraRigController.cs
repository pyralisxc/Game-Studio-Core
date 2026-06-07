using Unity.Cinemachine;
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
    [ExecuteAlways]
    public class CinemachineCameraRigController : MonoBehaviour
    {
        [SerializeField] private CameraRigProfile cameraRigProfile;
        [SerializeField] private PlayfieldProfile playfieldProfile;
        [SerializeField] private ParticipantRosterService participantRoster;
        [SerializeField] private MonoBehaviour sharedCameraBehaviour;
        [SerializeField] private MonoBehaviour[] splitScreenCameraBehaviours;
        [SerializeField] private UnityEngine.Camera targetCamera;

        [Header("Scroll Zoom")]
        [Tooltip("Allow mouse scroll wheel to nudge the zoom at runtime.")]
        [SerializeField] private bool allowScrollZoom = false;
        [Tooltip("How fast scroll adjusts zoom. Higher = faster.")]
        [SerializeField] private float scrollZoomSpeed = 2f;

        private Transform _sharedFocusTarget;

        // Ã¢â€â‚¬Ã¢â€â‚¬ Scroll zoom Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬ //
        private float _scrollZoomOffset;

        // Ã¢â€â‚¬Ã¢â€â‚¬ Profile blend Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬ //
        private CameraRigProfile _blendFromProfile;
        private float _blendT        = 1f;   // 0 = blend start, 1 = complete
        private float _blendDuration = 0.5f;

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

            if (targetCamera == null)
                targetCamera = UnityEngine.Camera.main;
            EnsureSharedFocusTarget();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            if (participantRoster == null || cameraRigProfile == null) return;

            // Advance profile blend.
            if (_blendT < 1f)
                _blendT = Mathf.MoveTowards(_blendT, 1f, Time.deltaTime / Mathf.Max(0.001f, _blendDuration));

            HandleScrollZoom();

            if (cameraRigProfile.presentationMode == CameraRigProfile.CameraPresentationMode.Shared)
                ApplySharedCamera();
            else
                ApplySplitScreenCameras();
        }

        public void SetProfile(CameraRigProfile profile)
        {
            cameraRigProfile = profile;
            cameraRigProfile?.Sanitize();
        }

        /// <summary>
        /// Smoothly transitions to a new CameraRigProfile over the given duration.
        /// Safe to call every frame Ã¢â‚¬â€ does nothing if already on that profile.
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

        private void ApplySharedCamera()
        {
            if (sharedCameraBehaviour == null || participantRoster.Participants.Count == 0)
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

                Vector3 position = participant.PawnInstance.transform.position;
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

                maxDistance = Mathf.Max(maxDistance, Vector3.Distance(centroid, participant.PawnInstance.transform.position));
            }

            _sharedFocusTarget.position = ClampToPlayfield(centroid);
            if (sharedCameraBehaviour is CinemachineVirtualCameraBase sharedVcam)
            {
                sharedVcam.Follow = _sharedFocusTarget;
                sharedVcam.LookAt = _sharedFocusTarget;
            }
            ApplyCameraLensDefaults(maxDistance);
        }

        private void ApplySplitScreenCameras()
        {
            if (splitScreenCameraBehaviours == null || splitScreenCameraBehaviours.Length == 0)
                return;

            int count = Mathf.Min(splitScreenCameraBehaviours.Length, participantRoster.Participants.Count);
            for (int i = 0; i < count; i++)
            {
                MonoBehaviour cameraBehaviour = splitScreenCameraBehaviours[i];
                ParticipantHandle participant = participantRoster.Participants[i];
                if (cameraBehaviour == null || participant.PawnInstance == null)
                    continue;

                Transform followTarget = participant.PawnInstance.transform;
                if (cameraBehaviour is CinemachineVirtualCameraBase vcam)
                {
                    vcam.Follow = followTarget;
                    vcam.LookAt = followTarget;
                }
            }
        }

        private void ApplyCameraLensDefaults(float participantSpread)
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
                float damping     = Mathf.Max(0.01f, zoomDamp);
                targetCamera.orthographicSize = Mathf.Lerp(targetCamera.orthographicSize, desiredSize, Time.deltaTime * damping);
            }
        }

        private float BlendFloat(System.Func<CameraRigProfile, float> selector)
        {
            float to = selector(cameraRigProfile);
            if (_blendFromProfile == null || _blendT >= 1f) return to;
            return Mathf.Lerp(selector(_blendFromProfile), to, _blendT);
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
