using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Camera;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace NeonBlack.Gameplay.Features.Zones
{
/// <summary>
/// Trigger volume that switches the CinemachineCameraRigController to a chosen
/// CameraRigProfile when the player enters, and optionally reverts to another
/// profile on exit. Useful for transitioning between wide establishing shots
/// and tight combat framing.
///
/// Setup:
///   1. Create an empty GameObject, add a BoxCollider (Is Trigger = ON).
///   2. Add this component.
///   3. Assign On Enter Profile â€” the CameraRigProfile asset to activate on entry.
///   4. Optionally assign On Exit Profile to revert when the player leaves.
///      Leave empty for no revert.
///   5. The player GameObject must have the tag: Player.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class CameraZone : MonoBehaviour
{
    [Header("Runtime References")]
    [Tooltip("Optional explicit camera rig reference. When left empty, Pyralis injects the active shared camera rig.")]
    [SerializeField] private CinemachineCameraRigController cameraRigController;

    [Header("Profiles")]
    [Tooltip("CameraRigProfile asset to switch to when the player enters.")]
    [SerializeField] private CameraRigProfile onEnterProfile;

    [Tooltip("CameraRigProfile asset to switch back to when the player exits. Leave empty for no revert.")]
    [SerializeField] private CameraRigProfile onExitProfile;

    [Tooltip("Blend duration in seconds for the profile transition.")]
    [SerializeField] private float transitionDuration = 0.5f;

    [Tooltip("When true the zone can only trigger once. "
           + "Useful for cutscene-style profile locks that should not snap back.")]
    [SerializeField] private bool oneShot = false;

    [Header("Events")]
    public UnityEvent OnPlayerEntered;
    public UnityEvent OnPlayerExited;

    // -- Tag ----------------------------------------------------------------- //
    [Header("Tag")]
    [Tooltip("Tag used to identify the player GameObject.")]
    [SerializeField] private string _playerTag = "Player";

    // -- Private ------------------------------------------------------------- //
    private bool _fired;

    [Inject]
    private void Construct(CinemachineCameraRigController injectedCameraRigController = null)
    {
        cameraRigController = injectedCameraRigController != null
            ? injectedCameraRigController
            : cameraRigController;
    }

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;
        if (oneShot && _fired)           return;
        _fired = true;

        SwitchCamera(onEnterProfile);
        OnPlayerEntered?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;
        if (onExitProfile == null)        return;

        SwitchCamera(onExitProfile);
        OnPlayerExited?.Invoke();
    }

    private void SwitchCamera(CameraRigProfile profile)
    {
        if (profile == null) return;
        cameraRigController?.SwitchProfile(profile, transitionDuration);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color  = new Color(0.8f, 0.2f, 1f, 0.1f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);

        Gizmos.color = new Color(0.8f, 0.2f, 1f, 0.5f);
        Gizmos.DrawWireCube(box.center, box.size);
    }
#endif
}
}
