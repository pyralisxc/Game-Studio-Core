using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Modules.Encounters
{
    public partial class ArenaZone
    {
        [Header("Runtime References")]
        [Tooltip("Optional explicit camera rig reference. When left empty, Pyralis injects the active shared camera rig.")]
        [SerializeField] private CinemachineCameraRigController cameraRigController;

        [Header("Camera Profile")]
        [Tooltip("CameraRigProfile asset to switch to when the player enters. Leave empty to keep current.")]
        [SerializeField] private CameraRigProfile onEnterCameraProfile;

        [Tooltip("CameraRigProfile asset to switch to when the zone is cleared. Leave empty to keep current.")]
        [SerializeField] private CameraRigProfile onClearCameraProfile;

        [Tooltip("Blend duration in seconds for the camera profile transition.")]
        [SerializeField] private float cameraTransitionDuration = 0.5f;

        [Inject]
        private void Construct(CinemachineCameraRigController injectedCameraRigController = null)
        {
            cameraRigController = injectedCameraRigController != null
                ? injectedCameraRigController
                : cameraRigController;
        }

        private void SwitchCamera(CameraRigProfile profile)
        {
            if (profile == null)
                return;

            cameraRigController?.SwitchProfile(profile, cameraTransitionDuration);
        }
    }
}
