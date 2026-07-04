using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using UnityEngine;
using UnityEngine.Rendering;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Presentation.Visuals
{
    [AuthoringContract(
        Category = "Animation",
        CapabilityPath = "Presentation/Feedback/Actor Shadow Driver",
        Surface = AuthoringSurface.Goal,
        Summary = "Applies shadow presentation (blob or renderer) based on PawnPresentationProfile.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/visuals",
        SetupSteps = new[]
        {
            "Add to the actor root or visual root.",
            "Assign Shadow Sprite Renderer when using an authored blob-shadow child, or assign a Shadow Prefab on the Pawn Presentation Profile.",
            "Assign Model Renderers only when the automatic child renderer search is not sufficient."
        },
        SuccessChecks = new[] { "Verify a shadow appears under the actor and scales correctly with height." },
        Tags = new[] { "capability:Animation" }
    )]
    [AddComponentMenu("NeonBlack/Gameplay/Visuals/Actor Shadow Driver")]
    public class ActorShadowDriver : MonoBehaviour, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (presentationProfile == null)
                yield return RuntimeValidationIssue.Recommended("Presentation Profile is empty. This is valid when the presentation stack applies the profile at runtime.");
            
            if (shadowSpriteRenderer == null && (modelRenderers == null || modelRenderers.Length == 0))
                yield return RuntimeValidationIssue.Recommended("No authored shadow renderer or model renderers assigned. Runtime profile application may still resolve renderer-shadow output.");

            if (RequiresBlobShadowRenderer())
                yield return RuntimeValidationIssue.Required("Blob shadow mode needs either an authored Shadow Sprite Renderer or a Shadow Prefab on the Pawn Presentation Profile.");

            if (presentationProfile != null
                && presentationProfile.shadowPrefab != null
                && presentationProfile.shadowPrefab.GetComponentInChildren<SpriteRenderer>(true) == null)
            {
                yield return RuntimeValidationIssue.Required("Shadow Prefab has no child SpriteRenderer for blob shadow output.");
            }
        }
        [Header("Scene References")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform shadowRoot;
        [SerializeField] private SpriteRenderer shadowSpriteRenderer;
        [SerializeField] private Renderer[] modelRenderers;

        [Header("Runtime")]
        [SerializeField] private PawnPresentationProfile presentationProfile;

        private GameObject _runtimeShadowObject;
        private Transform _runtimeShadowRoot;
        private SpriteRenderer _runtimeShadowSpriteRenderer;

        public void ApplyProfile(PawnPresentationProfile profile)
        {
            presentationProfile = profile;
            ResolveReferences();
            ApplyRendererShadowSettings();
            UpdateShadowVisual();
        }

        public void TickShadow()
        {
            if (presentationProfile == null)
                return;

            UpdateShadowVisual();
        }

        private void ResolveReferences()
        {
            visualRoot ??= GetComponentInChildren<Animator>(true) != null
                ? GetComponentInChildren<Animator>(true).transform
                : GetComponentInChildren<SpriteRenderer>(true) != null
                    ? GetComponentInChildren<SpriteRenderer>(true).transform
                    : transform;

            if (shadowRoot == null && shadowSpriteRenderer != null)
                shadowRoot = shadowSpriteRenderer.transform;

            modelRenderers ??= GetComponentsInChildren<Renderer>(true);
        }

        private void ApplyRendererShadowSettings()
        {
            if (presentationProfile == null || modelRenderers == null)
                return;

            ShadowCastingMode castMode = presentationProfile.castModelShadows
                ? ShadowCastingMode.On
                : ShadowCastingMode.Off;

            for (int i = 0; i < modelRenderers.Length; i++)
            {
                Renderer renderer = modelRenderers[i];
                if (renderer == null || renderer == shadowSpriteRenderer)
                    continue;

                renderer.shadowCastingMode = castMode;
                renderer.receiveShadows = presentationProfile.receiveModelShadows;
            }
        }

        private void UpdateShadowVisual()
        {
            if (presentationProfile == null)
                return;

            ActorShadowMode mode = ResolveShadowMode();
            if (mode != ActorShadowMode.BlobSprite)
            {
                SetShadowVisible(false);
                return;
            }

            EnsureShadowVisual();
            Transform activeShadowRoot = ActiveShadowRoot();
            SpriteRenderer activeShadowRenderer = ActiveShadowRenderer();
            if (activeShadowRoot == null || activeShadowRenderer == null)
                return;

            activeShadowRoot.localPosition = presentationProfile.shadowLocalOffset;
            activeShadowRoot.localRotation = Quaternion.identity;

            float heightOffset = 0f;
            if (visualRoot != null)
                heightOffset = Mathf.Max(0f, visualRoot.position.y - transform.position.y);

            float scaleMultiplier = Mathf.Max(0.1f, 1f - heightOffset * presentationProfile.shadowHeightScaleResponse);
            activeShadowRoot.localScale = Vector3.Scale(presentationProfile.shadowScale, new Vector3(scaleMultiplier, scaleMultiplier, 1f));

            activeShadowRenderer.color = presentationProfile.shadowColor;
            if (presentationProfile.shadowSprite != null)
                activeShadowRenderer.sprite = presentationProfile.shadowSprite;
            if (!string.IsNullOrWhiteSpace(presentationProfile.shadowSortingLayerName))
                activeShadowRenderer.sortingLayerName = presentationProfile.shadowSortingLayerName;
            activeShadowRenderer.sortingOrder = presentationProfile.shadowSortingOrder;
            SetShadowVisible(presentationProfile.shadowSprite != null || presentationProfile.shadowPrefab != null);
        }

        private ActorShadowMode ResolveShadowMode()
        {
            if (presentationProfile == null)
                return ActorShadowMode.None;

            if (presentationProfile.shadowMode != ActorShadowMode.Auto)
                return presentationProfile.shadowMode;

            if (presentationProfile.presentationMode == ActorPresentationMode.ThirdPerson3D &&
                presentationProfile.shadowSprite == null &&
                presentationProfile.shadowPrefab == null)
            {
                return ActorShadowMode.RendererShadows;
            }

            return presentationProfile.shadowSprite != null || presentationProfile.shadowPrefab != null
                ? ActorShadowMode.BlobSprite
                : ActorShadowMode.None;
        }

        private void EnsureShadowVisual()
        {
            if (presentationProfile.shadowPrefab != null)
            {
                if (_runtimeShadowObject == null || _runtimeShadowObject.name != presentationProfile.shadowPrefab.name + " (Runtime)")
                {
                    if (_runtimeShadowObject != null)
                        DestroyRuntimeShadow();

                    _runtimeShadowObject = Instantiate(presentationProfile.shadowPrefab, transform);
                    _runtimeShadowObject.name = presentationProfile.shadowPrefab.name + " (Runtime)";
                    _runtimeShadowRoot = _runtimeShadowObject.transform;
                    _runtimeShadowSpriteRenderer = _runtimeShadowObject.GetComponentInChildren<SpriteRenderer>(true);
                }

                return;
            }

            DestroyRuntimeShadow();
        }

        private void SetShadowVisible(bool visible)
        {
            Transform activeShadowRoot = ActiveShadowRoot();
            if (activeShadowRoot != null)
                activeShadowRoot.gameObject.SetActive(visible);
        }

        private void DestroyRuntimeShadow()
        {
            if (_runtimeShadowObject == null)
                return;

            if (Application.isPlaying)
                Destroy(_runtimeShadowObject);
            else
                DestroyImmediate(_runtimeShadowObject);

            _runtimeShadowObject = null;
            _runtimeShadowRoot = null;
            _runtimeShadowSpriteRenderer = null;
        }

        private Transform ActiveShadowRoot()
        {
            return _runtimeShadowRoot != null ? _runtimeShadowRoot : shadowRoot;
        }

        private SpriteRenderer ActiveShadowRenderer()
        {
            return _runtimeShadowSpriteRenderer != null ? _runtimeShadowSpriteRenderer : shadowSpriteRenderer;
        }

        private bool RequiresBlobShadowRenderer()
        {
            if (presentationProfile == null)
                return false;

            ActorShadowMode mode = ResolveShadowMode();
            return mode == ActorShadowMode.BlobSprite
                && presentationProfile.shadowPrefab == null
                && shadowSpriteRenderer == null;
        }
    }
}
