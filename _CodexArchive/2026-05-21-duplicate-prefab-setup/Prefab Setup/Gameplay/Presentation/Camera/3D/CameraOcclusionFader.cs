using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Camera
{
/// <summary>
/// Fades out renderers that occlude the line of sight between the camera
/// and a tracked target. Attach to the same GameObject as your Main Camera.
///
/// Uses URP-compatible material properties. Only affects renderers whose
/// materials use a shader with _BaseColor and _Surface properties (standard
/// URP Lit / Unlit shaders). Set Target to the player transform at runtime
/// via <see cref="SetTarget"/>.
///
/// Setup:
///   1. Attach to your Main Camera.
///   2. Drag the player Transform into Target.
///   3. Tune Fade Alpha and Fade Distance as needed.
/// </summary>
public class CameraOcclusionFader : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The Transform to check line-of-sight toward. Typically the player.")]
    [SerializeField] private Transform target;

    [Header("Fade Settings")]
    [Tooltip("Alpha applied to occluding objects (0 = invisible, 1 = opaque).")]
    [Range(0f, 1f)]
    [SerializeField] private float fadeAlpha = 0.25f;

    [Tooltip("Objects closer than this distance to the camera are faded.")]
    [SerializeField] private float fadeDistance = 2.5f;

    [Tooltip("Layer mask for occlusion raycasts. Limit this to geometry layers for performance.")]
    [SerializeField] private LayerMask occlusionMask = Physics.AllLayers;

    // Ã¢â€â‚¬Ã¢â€â‚¬ Private Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬ //
    // Maps each Renderer to its original per-material colour/surface state.
    private readonly Dictionary<Renderer, RendererState> _fadedRenderers
        = new Dictionary<Renderer, RendererState>();

    private readonly HashSet<Renderer> _currentlyOccluding = new HashSet<Renderer>();

    private struct RendererState
    {
        public Color[] originalColors;   // one per material
        public float[] originalSurface;  // 0 = opaque, 1 = transparent
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Static property IDs (cached for performance) Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬ //
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int SurfaceId   = Shader.PropertyToID("_Surface");

    private void OnDisable()
    {
        RestoreAll();
    }

    /// <summary>Assigns the follow target at runtime (e.g. after player respawn).</summary>
    public void SetTarget(Transform newTarget)
    {
        if (target != newTarget)
            RestoreAll();   // Clear faded state before changing target.
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        _currentlyOccluding.Clear();

        Vector3 origin    = transform.position;
        Vector3 direction = (target.position - origin).normalized;
        float   distance  = Vector3.Distance(origin, target.position);

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, occlusionMask);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == target.gameObject) continue;

            Renderer[] renderers = hit.collider.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                _currentlyOccluding.Add(r);

                if (hit.distance < fadeDistance)
                    FadeRenderer(r);
                else
                    RestoreRenderer(r);
            }
        }

        // Restore any renderer that is no longer occluding.
        var toRestore = new List<Renderer>();
        foreach (var kvp in _fadedRenderers)
        {
            if (!_currentlyOccluding.Contains(kvp.Key))
                toRestore.Add(kvp.Key);
        }
        foreach (Renderer r in toRestore)
            RestoreRenderer(r);
    }

    private void FadeRenderer(Renderer r)
    {
        if (_fadedRenderers.ContainsKey(r)) return;   // Already faded.
        if (r.sharedMaterials == null || r.sharedMaterials.Length == 0) return;

        var state = new RendererState
        {
            originalColors  = new Color[r.materials.Length],
            originalSurface = new float[r.materials.Length]
        };

        for (int i = 0; i < r.materials.Length; i++)
        {
            Material mat = r.materials[i];
            if (mat == null) continue;

            state.originalColors[i]  = mat.HasProperty(BaseColorId) ? mat.GetColor(BaseColorId) : Color.white;
            state.originalSurface[i] = mat.HasProperty(SurfaceId)   ? mat.GetFloat(SurfaceId)   : 0f;

            // Switch material to transparent surface type (URP).
            if (mat.HasProperty(SurfaceId))
                mat.SetFloat(SurfaceId, 1f);

            // Apply fade alpha.
            if (mat.HasProperty(BaseColorId))
            {
                Color c = state.originalColors[i];
                c.a = fadeAlpha;
                mat.SetColor(BaseColorId, c);
            }
        }

        _fadedRenderers[r] = state;
    }

    private void RestoreRenderer(Renderer r)
    {
        if (!_fadedRenderers.TryGetValue(r, out RendererState state)) return;
        if (r == null)
        {
            _fadedRenderers.Remove(r);
            return;
        }

        for (int i = 0; i < r.materials.Length; i++)
        {
            Material mat = r.materials[i];
            if (mat == null) continue;

            if (mat.HasProperty(SurfaceId))
                mat.SetFloat(SurfaceId, state.originalSurface[i]);

            if (mat.HasProperty(BaseColorId))
                mat.SetColor(BaseColorId, state.originalColors[i]);
        }

        _fadedRenderers.Remove(r);
    }

    private void RestoreAll()
    {
        // Iterate over a copy so we can remove safely.
        var keys = new List<Renderer>(_fadedRenderers.Keys);
        foreach (Renderer r in keys)
            RestoreRenderer(r);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (target == null) return;
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.4f);
        Gizmos.DrawLine(transform.position, target.position);
    }
#endif
}
}
