using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Presentation.Camera
{
    [AuthoringContract(
        StableId = "camera.pawn-target",
        Category = "Camera",
        CapabilityPath = "World & Meta/Camera/Pawn Camera Target",
        Surface = AuthoringSurface.RuntimeComponent,
        Summary = "Visible pawn camera socket. Cinemachine follows these transforms when the active CameraRigProfile focuses participant pawns.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/camera",
        RequiredFields = new[] { nameof(followTarget), nameof(lookAtTarget) },
        SetupSteps = new[]
        {
            "Add to the pawn prefab root or a stable camera socket child.",
            "Assign Follow Target to the transform the camera should track.",
            "Assign Look At Target only when the camera should aim at a different transform."
        },
        SuccessChecks = new[] { "Enter Play Mode and verify the scene camera follows the spawned participant pawn through this socket." },
        RoleTags = new[] { "PawnCameraSocket", "ParticipantFollow", "CameraTarget" },
        Tags = new[] { "capability:Camera" },
        Selectable = false
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
