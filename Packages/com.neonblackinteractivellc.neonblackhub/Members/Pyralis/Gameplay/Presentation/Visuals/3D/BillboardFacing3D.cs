using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Visuals
{
/// <summary>
/// Reusable 3D presentation helper for camera-facing sprites and left/right mirroring.
/// This component owns visual orientation only; gameplay movement and hitbox logic stay elsewhere.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Animation,
    Relevance = "Forces a 3D object to face the camera and supports left/right mirroring.",
    Axioms = AuthoringWorldAxiom.Dimensions3D,
    NativeSetup = new[] 
    { 
        "Attach to a 3D visual object.",
        "Assign Target (usually this Transform).",
        "Assign Camera Override (optional)."
    },
    AssignmentFields = new[] { nameof(target), nameof(mirroredVisualRoot), nameof(spriteRenderer), nameof(cameraOverride), nameof(facingMode) },
    FirstProof = "Move the camera and verify the object rotates to face it.",
    ExpertAdvice = "Use Y-Axis only for ground-based actors. Use Full Facing for projectiles or floating items.",
    DocumentationURL = "https://docs.neonblack.com/pyralis/visuals"
)]
[AddComponentMenu("NeonBlack/Gameplay/Runtime 3D/Presentation/Billboard Facing 3D")]
public class BillboardFacing3D : MonoBehaviour, IRuntimeValidationProvider
{
    public IEnumerable<string> GetRuntimeValidationIssues()
    {
        if (target == null) yield return "Target transform is unassigned.";
        if (cameraOverride == null) yield return "Camera Override is empty. Will fallback to Main Camera.";
    }
    public enum FacingMode
    {
        YAxisOnly,
        FullFacing
    }

    [Header("Presentation Targets")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform mirroredVisualRoot;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private UnityEngine.Camera cameraOverride;

    [Header("Settings")]
    [SerializeField] private FacingMode facingMode = FacingMode.YAxisOnly;
    [SerializeField] private bool spriteDefaultFacesRight = true;

    public UnityEngine.Camera CameraOverride => cameraOverride;

    public void Configure(
        Transform newTarget,
        Transform newMirroredVisualRoot,
        SpriteRenderer newSpriteRenderer,
        UnityEngine.Camera newCamera,
        FacingMode newFacingMode,
        bool newSpriteDefaultFacesRight)
    {
        target = newTarget != null ? newTarget : transform;
        mirroredVisualRoot = newMirroredVisualRoot;
        spriteRenderer = newSpriteRenderer;
        cameraOverride = newCamera;
        facingMode = newFacingMode;
        spriteDefaultFacesRight = newSpriteDefaultFacesRight;
    }

    public void ApplyBillboard()
    {
        UnityEngine.Camera activeCamera = cameraOverride;
        Transform presentationTarget = target != null ? target : transform;
        if (activeCamera == null || presentationTarget == null)
            return;

        switch (facingMode)
        {
            case FacingMode.YAxisOnly:
                Vector3 direction = activeCamera.transform.position - presentationTarget.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f)
                    presentationTarget.rotation = Quaternion.LookRotation(direction);
                break;

            case FacingMode.FullFacing:
                presentationTarget.rotation = activeCamera.transform.rotation;
                break;
        }
    }

    public void ApplyFacing(bool facingRight)
    {
        bool wantFlip = spriteDefaultFacesRight ? !facingRight : facingRight;

        if (facingMode == FacingMode.FullFacing && mirroredVisualRoot != null && mirroredVisualRoot != transform)
        {
            Vector3 scale = mirroredVisualRoot.localScale;
            scale.x = wantFlip ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            mirroredVisualRoot.localScale = scale;
            return;
        }

        if (spriteRenderer != null)
            spriteRenderer.flipX = wantFlip;
    }

    public void SetCameraOverride(UnityEngine.Camera camera)
    {
        cameraOverride = camera;
    }
}
}
