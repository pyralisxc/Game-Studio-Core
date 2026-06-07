using UnityEngine;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Defines camera presentation choices for shared or split participant views.
    /// </summary>
    [CreateAssetMenu(menuName = "NeonBlack/Gameplay/Profiles/Camera Rig Profile", fileName = "CameraRigProfile")]
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
