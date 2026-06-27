using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Camera
{
    [AuthoringContract(
        Capability = AuthoringCapability.Camera,
        CapabilityPath = "World & Meta/Camera/Pawn Camera Target",
        Relevance = "Visible pawn camera socket. Cinemachine follows these transforms when the active CameraRigProfile focuses participant pawns.",
        RoleTags = new[] { "PawnCameraSocket", "ParticipantFollow", "CameraTarget" },
        NativeSetup = new[]
        {
            "Add to the pawn prefab root or a stable camera socket child.",
            "Assign Follow Target to the transform the camera should track.",
            "Assign Look At Target only when the camera should aim at a different transform."
        },
        AssignmentFields = new[] { nameof(followTarget), nameof(lookAtTarget) },
        ProofTargetId = "proof.1p-pawn-movement",
        Proof = "Enter Play Mode and verify the scene camera follows the spawned participant pawn through this socket.",
        Surface = AuthoringContractSurface.RuntimeComponent,
        ExpertAdvice = "For top-down 2D, use a stable map-plane root as Follow Target so visual hops do not shake the camera. For 3D or side-view, use a chest/head/center socket if that frames better than the prefab root.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/camera"
    )]
    [DisallowMultipleComponent]
    [AddComponentMenu("NeonBlack/Pyralis/Pawn Camera Target")]
    public sealed class PawnCameraTarget : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private Transform lookAtTarget;

        public Transform FollowTarget => followTarget != null ? followTarget : transform;
        public Transform LookAtTarget => lookAtTarget != null ? lookAtTarget : FollowTarget;
    }
}
