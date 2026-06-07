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
[AddComponentMenu("NeonBlack/Gameplay/Camera/Camera Occlusion Fader 3D")]
public class CameraOcclusionFader : MonoBehaviour
{
    private const int MaxOcclusionHits = 64;

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

    // Runtime renderer state.
    // Maps each Renderer to its original per-material colour/surface state.
    private readonly Dictionary<Renderer, RendererState> _fadedRenderers
        = new Dictionary<Renderer, RendererState>();

    private readonly HashSet<Renderer> _currentlyOccluding = new HashSet<Renderer>();
    private readonly List<Renderer> _rendererScratch = new List<Renderer>(8);
    private readonly List<Renderer> _restoreScratch = new List<Renderer>(8);
    private readonly RaycastHit[] _hitBuffer = new RaycastHit[MaxOcclusionHits];

    private struct RendererState
    {
        public Material[] materials;
        public Color[] originalColors;   // one per material
        public float[] originalSurface;  // 0 = opaque, 1 = transparent
    }

    // Static property IDs cached for performance.
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

        int hitCount = Physics.RaycastNonAlloc(origin, direction, _hitBuffer, distance, occlusionMask);
        for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
        {
            RaycastHit hit = _hitBuffer[hitIndex];
            if (hit.collider.gameObject == target.gameObject) continue;

            _rendererScratch.Clear();
            hit.collider.GetComponentsInChildren(false, _rendererScratch);
            for (int rendererIndex = 0; rendererIndex < _rendererScratch.Count; rendererIndex++)
            {
                Renderer r = _rendererScratch[rendererIndex];
                if (r == null) continue;
                _currentlyOccluding.Add(r);

                if (hit.distance < fadeDistance)
                    FadeRenderer(r);
                else
                    RestoreRenderer(r);
            }
        }

        // Restore any renderer that is no longer occluding.
        _restoreScratch.Clear();
        foreach (var kvp in _fadedRenderers)
        {
            if (!_currentlyOccluding.Contains(kvp.Key))
                _restoreScratch.Add(kvp.Key);
        }

        for (int i = 0; i < _restoreScratch.Count; i++)
            RestoreRenderer(_restoreScratch[i]);
    }

    private void FadeRenderer(Renderer r)
    {
        if (_fadedRenderers.ContainsKey(r)) return;   // Already faded.
        if (r.sharedMaterials == null || r.sharedMaterials.Length == 0) return;

        Material[] materials = r.materials;
        var state = new RendererState
        {
            materials = materials,
            originalColors  = new Color[materials.Length],
            originalSurface = new float[materials.Length]
        };

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
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

        Material[] materials = state.materials;
        if (materials == null)
        {
            _fadedRenderers.Remove(r);
            return;
        }

        int materialCount = Mathf.Min(materials.Length, state.originalColors != null ? state.originalColors.Length : 0);
        materialCount = Mathf.Min(materialCount, state.originalSurface != null ? state.originalSurface.Length : 0);
        for (int i = 0; i < materialCount; i++)
        {
            Material mat = materials[i];
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
        _restoreScratch.Clear();
        foreach (Renderer renderer in _fadedRenderers.Keys)
            _restoreScratch.Add(renderer);

        for (int i = 0; i < _restoreScratch.Count; i++)
            RestoreRenderer(_restoreScratch[i]);
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
