using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Camera;
using NeonBlack.Gameplay.Characters;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace NeonBlack.Gameplay.Features.Zones
{
/// <summary>
/// Trigger volume that switches the CinemachineCameraRigController to a chosen
/// CameraRigProfile when the player enters.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Camera | AuthoringCapability.Puzzle,
    Relevance = "3D trigger volume that switches CameraRigProfile when the player enters.",
    NativeSetup = new[] 
    { 
        "Assign On Enter Profile to the camera framing this zone should activate.",
        "Assign Camera Rig Controller manually, or let dependency injection provide it.",
        "Set Player Tag to the tag used by entering pawn objects."
    },
    AssignmentFields = new[] { nameof(onEnterProfile), nameof(onExitProfile), nameof(transitionDuration), nameof(_playerTag) },
    FirstProof = "Enter the trigger volume with a Player-tagged object and verify the camera switches profiles.",
    ExpertAdvice = "Combat arena path: enter a tighter profile and exit back to the default profile. Cutscene path: enable One Shot and leave On Exit Profile empty. Exploration path: use wider profiles for overlooks or large platforming spaces.",
    DocumentationURL = "https://docs.neonblack.com/pyralis/camera"
)]
[AddComponentMenu("NeonBlack/Gameplay/Camera/Camera Zone 3D")]
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
        if (!IsPlayer(other.gameObject)) return;
        if (oneShot && _fired)           return;
        _fired = true;

        SwitchCamera(onEnterProfile);
        OnPlayerEntered?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other.gameObject)) return;
        if (onExitProfile == null)        return;

        SwitchCamera(onExitProfile);
        OnPlayerExited?.Invoke();
    }

    private bool IsPlayer(GameObject go)
    {
        if (go.CompareTag(_playerTag))
            return true;

        // Multi-participant support: check if the object belongs to any registered participant.
        return ParticipantQueryUtility.TryResolveParticipant(go, out _);
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
