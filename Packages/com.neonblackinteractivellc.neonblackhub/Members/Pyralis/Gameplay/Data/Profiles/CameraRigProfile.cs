using UnityEngine;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Defines camera presentation choices for shared or split participant views.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Camera, 
        Relevance = "Project-window creation path for camera framing, follow, zoom, and 2D orthographic route choices.",
        AssignmentFields = new[] { nameof(presentationMode), nameof(useCinemachine), nameof(followOffset) },
        FirstProof = "Verify the camera follows the focus object at the specified offset.",
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Profiles/Camera Rig Profile", fileName = "CameraRigProfile", order = -70)]
    public class CameraRigProfile : ScriptableObject
    {
        public enum CameraPresentationMode
        {
            Shared,
            SplitScreen
        }

        public CameraPresentationMode presentationMode = CameraPresentationMode.Shared;
        public bool useCinemachine = true;
        public bool orthographic = false;
        public bool lockToPlayfield = true;
        [Tooltip("When enabled, Pyralis places and rotates the shared Cinemachine camera from this profile each frame. Disable this to hand-author the Cinemachine camera transform in the scene.")]
        public bool useProfileTransform = true;
        [Tooltip("World-space offset from the shared follow focus to the Cinemachine camera when Use Profile Transform is enabled.")]
        public Vector3 followOffset = new Vector3(0f, 0f, -10f);
        [Tooltip("Camera rotation in degrees when Use Profile Transform is enabled. X is pitch, Y is yaw, Z is roll.")]
        public Vector3 viewEulerAngles = Vector3.zero;
        public float defaultDistance = 10f;
        public float minZoom = 3f;
        public float maxZoom = 18f;
        public float followDamping = 8f;
        public float zoomDamping = 8f;
        public float orthographicSize = 5f;
        public float shakeAmplitude = 1f;
        public float shakeFrequency = 1f;

        public void Sanitize()
        {
            minZoom = Mathf.Max(0.01f, minZoom);
            maxZoom = Mathf.Max(minZoom, maxZoom);
            defaultDistance = Mathf.Max(0.01f, defaultDistance);
            followDamping = Mathf.Max(0f, followDamping);
            zoomDamping = Mathf.Max(0f, zoomDamping);
            orthographicSize = Mathf.Max(0.01f, orthographicSize);
            shakeAmplitude = Mathf.Max(0f, shakeAmplitude);
            shakeFrequency = Mathf.Max(0f, shakeFrequency);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
