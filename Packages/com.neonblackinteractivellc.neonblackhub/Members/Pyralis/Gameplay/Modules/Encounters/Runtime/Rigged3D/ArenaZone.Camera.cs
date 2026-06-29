using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Presentation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Encounters
{
    public partial class ArenaZone
    {
        [Header("Runtime References")]
        [Tooltip("Optional explicit camera rig reference for arena camera profile switches.")]
        [SerializeField] private MonoBehaviour cameraRigController;

        [Header("Camera Profile")]
        [Tooltip("CameraRigProfile asset to switch to when the player enters. Leave empty to keep current.")]
        [SerializeField] private CameraRigProfile onEnterCameraProfile;

        [Tooltip("CameraRigProfile asset to switch to when the zone is cleared. Leave empty to keep current.")]
        [SerializeField] private CameraRigProfile onClearCameraProfile;

        [Tooltip("Blend duration in seconds for the camera profile transition.")]
        [SerializeField] private float cameraTransitionDuration = 0.5f;

        private ICameraRigProfileSwitcher _cameraRigProfileSwitcher;

        private void SwitchCamera(CameraRigProfile profile)
        {
            if (profile == null)
                return;

            ResolveCameraRigProfileSwitcher()?.SwitchProfile(profile, cameraTransitionDuration);
        }

        private ICameraRigProfileSwitcher ResolveCameraRigProfileSwitcher()
        {
            if (_cameraRigProfileSwitcher != null)
                return _cameraRigProfileSwitcher;

            _cameraRigProfileSwitcher = cameraRigController as ICameraRigProfileSwitcher;
            return _cameraRigProfileSwitcher;
        }
    }
}
