using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Camera
{
/// <summary>
/// Fades out renderers that occlude the line of sight between the camera
/// and a tracked target. Attach to the same GameObject as your Main Camera.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Camera,
    Relevance = "Fades renderers that block the line of sight between the camera and a tracked target.",
    NativeSetup = new[] 
    { 
        "Attach to your Main Camera.",
        "Drag the player Transform into Target (or set at runtime).",
        "Limit Occlusion Mask to world geometry layers."
    },
    AssignmentFields = new[] { nameof(target), nameof(fadeAlpha), nameof(fadeDistance), nameof(occlusionMask) },
    FirstProof = "Walk the player behind world geometry and verify it fades out.",
    ExpertAdvice = "Keep the player layer out of Occlusion Mask. Use this for 3D line-of-sight fading; 2D sprite visibility is usually handled via sorting layers."
)]
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
    private readonly List<Renderer> _rendererScratch = new List<Renderer>(16);
    private readonly List<Renderer> _restoreScratch = new List<Renderer>(16);
    private readonly RaycastHit[] _hitBuffer = new RaycastHit[MaxOcclusionHits];

    private class RendererState
    {
        public Material[] materials;
        public Color[] originalColors;   // one per material
        public float[] originalSurface;  // 0 = opaque, 1 = transparent

        public void Reset(int materialCount)
        {
            if (originalColors == null || originalColors.Length < materialCount)
                originalColors = new Color[materialCount];
            if (originalSurface == null || originalSurface.Length < materialCount)
                originalSurface = new float[materialCount];
        }
    }

    private static readonly Stack<RendererState> _statePool = new Stack<RendererState>();

    private static RendererState GetStateFromPool(int materialCount)
    {
        RendererState state = _statePool.Count > 0 ? _statePool.Pop() : new RendererState();
        state.Reset(materialCount);
        return state;
    }

    private static void ReturnStateToPool(RendererState state)
    {
        if (state == null) return;
        state.materials = null;
        _statePool.Push(state);
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

    private readonly List<Material> _materialBuffer = new List<Material>(8);
    private MaterialPropertyBlock _propertyBlock;

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
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
        foreach (var pair in _fadedRenderers)
        {
            if (!_currentlyOccluding.Contains(pair.Key))
                _restoreScratch.Add(pair.Key);
        }

        for (int i = 0; i < _restoreScratch.Count; i++)
            RestoreRenderer(_restoreScratch[i]);
    }

    private void FadeRenderer(Renderer r)
    {
        if (_fadedRenderers.ContainsKey(r)) return;   // Already faded.
        
        _materialBuffer.Clear();
        r.GetSharedMaterials(_materialBuffer);
        if (_materialBuffer.Count == 0) return;

        // NOTE: We still need 'r.materials' if we want to change Surface Type/Render Queue 
        // because PropertyBlocks cannot change shader keywords or render queues.
        // However, we only do this ONCE per activation, and RendererState is pooled.
        Material[] materials = r.materials; 
        RendererState state = GetStateFromPool(materials.Length);
        state.materials = materials;

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
            ReturnStateToPool(state);
            return;
        }

        Material[] materials = state.materials;
        if (materials != null)
        {
            int materialCount = Mathf.Min(materials.Length, state.originalColors.Length);
            for (int i = 0; i < materialCount; i++)
            {
                Material mat = materials[i];
                if (mat == null) continue;

                if (mat.HasProperty(SurfaceId))
                    mat.SetFloat(SurfaceId, state.originalSurface[i]);

                if (mat.HasProperty(BaseColorId))
                    mat.SetColor(BaseColorId, state.originalColors[i]);
            }
        }

        _fadedRenderers.Remove(r);
        ReturnStateToPool(state);
    }

    private void RestoreAll()
    {
        _restoreScratch.Clear();
        foreach (var pair in _fadedRenderers)
            _restoreScratch.Add(pair.Key);

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
