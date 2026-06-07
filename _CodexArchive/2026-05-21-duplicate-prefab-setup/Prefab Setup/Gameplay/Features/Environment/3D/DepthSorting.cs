using UnityEngine;

namespace NeonBlack.Gameplay.Features.Environment
{
/// <summary>
/// Adjusts a SpriteRenderer's sorting order every frame based on the GameObject's
/// world-space Z position, so characters closer to the camera always render in front.
///
/// Setup:
/// 1. Attach to every character's SPRITE child GameObject (the one with the SpriteRenderer).
///    Do NOT attach to the root â€” the root has no SpriteRenderer.
/// 2. Set Sorting Layer to the same layer all your characters share (e.g. "Characters").
/// 3. Leave Base Order at 0 unless you need to offset a specific character above all others.
/// 4. Sorting Scale controls how many sorting steps per world unit of Z depth.
///    Higher values = more distinct layering when characters are close together.
///    2-5 is a good range for human-scale characters.
///
/// How it works:
/// Characters with SMALLER world Z (closer to the camera, which looks from behind/above)
/// get a HIGHER sorting order and render in front.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DepthSorting : MonoBehaviour
{
    [Tooltip("Baseline sorting order. All characters start here and diverge by Z position.\n" +
             "Keep this at 0 unless a specific character needs to always render above others.")]
    [SerializeField] private int baseOrder = 0;

    [Tooltip("How many sorting order steps per world unit of Z depth.\n" +
             "Higher = more distinct separation when characters are close.\n" +
             "Recommended: 2â€“5 for human-scale characters.")]
    [SerializeField] private float sortingScale = 3f;

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        // Smaller Z = closer to camera = should render in front = higher order.
        // We use the root's Z (parent) so child offsets don't skew the calculation.
        float worldZ = transform.parent != null ? transform.parent.position.z : transform.position.z;
        _sr.sortingOrder = baseOrder - Mathf.RoundToInt(worldZ * sortingScale);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw a line downward to visualize which Z depth this sprite is sorted at.
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.6f);
        Vector3 pos = transform.position;
        Gizmos.DrawLine(pos + Vector3.up * 0.1f, pos - Vector3.up * 0.5f);

        UnityEditor.Handles.color = new Color(0f, 0.8f, 1f, 0.9f);
        float worldZ = transform.parent != null ? transform.parent.position.z : pos.z;
        int order = baseOrder - Mathf.RoundToInt(worldZ * sortingScale);
        UnityEditor.Handles.Label(pos + Vector3.up * 1.2f, $"Sort: {order}");
    }
#endif
}
}
