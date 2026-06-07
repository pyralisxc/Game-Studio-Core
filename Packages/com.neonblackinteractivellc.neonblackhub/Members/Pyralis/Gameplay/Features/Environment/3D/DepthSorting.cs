using UnityEngine;

namespace NeonBlack.Gameplay.Features.Environment
{
/// <summary>
/// Adjusts a SpriteRenderer's sorting order every frame based on the GameObject's
/// world-space Z position, so characters closer to the camera always render in front.
///
/// Setup:
/// 1. Attach to every character's sprite child GameObject, the one with the SpriteRenderer.
///    Do not attach to the root if the root has no SpriteRenderer.
/// 2. Set Sorting Layer to the same layer all your characters share, for example "Characters".
/// 3. Leave Base Order at 0 unless you need to offset a specific character above all others.
/// 4. Sorting Scale controls how many sorting steps per world unit of Z depth.
///    Higher values mean more distinct layering when characters are close together.
///    2-5 is a good range for human-scale characters.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DepthSorting : MonoBehaviour
{
    [Tooltip("Baseline sorting order. All characters start here and diverge by Z position.\n" +
             "Keep this at 0 unless a specific character needs to always render above others.")]
    [SerializeField] private int baseOrder = 0;

    [Tooltip("How many sorting order steps per world unit of Z depth.\n" +
             "Higher = more distinct separation when characters are close.\n" +
             "Recommended: 2-5 for human-scale characters.")]
    [SerializeField] private float sortingScale = 3f;

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        float worldZ = transform.parent != null ? transform.parent.position.z : transform.position.z;
        _sr.sortingOrder = baseOrder - Mathf.RoundToInt(worldZ * sortingScale);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
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
