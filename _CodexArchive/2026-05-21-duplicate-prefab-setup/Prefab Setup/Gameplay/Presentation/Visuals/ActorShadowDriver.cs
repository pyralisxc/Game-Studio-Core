using NeonBlack.Gameplay.Data.Profiles;
using UnityEngine;
using UnityEngine.Rendering;

namespace NeonBlack.Gameplay.Presentation.Visuals
{
    [AddComponentMenu("NeonBlack/Gameplay/Visuals/Actor Shadow Driver")]
    public class ActorShadowDriver : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform shadowRoot;
        [SerializeField] private SpriteRenderer shadowSpriteRenderer;
        [SerializeField] private Renderer[] modelRenderers;

        [Header("Runtime")]
        [SerializeField] private PawnPresentationProfile presentationProfile;

        private GameObject _generatedShadowObject;

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
            if (shadowRoot == null || shadowSpriteRenderer == null)
                return;

            shadowRoot.localPosition = presentationProfile.shadowLocalOffset;
            shadowRoot.localRotation = Quaternion.identity;

            float heightOffset = 0f;
            if (visualRoot != null)
                heightOffset = Mathf.Max(0f, visualRoot.position.y - transform.position.y);

            float scaleMultiplier = Mathf.Max(0.1f, 1f - heightOffset * presentationProfile.shadowHeightScaleResponse);
            shadowRoot.localScale = Vector3.Scale(presentationProfile.shadowScale, new Vector3(scaleMultiplier, scaleMultiplier, 1f));

            shadowSpriteRenderer.color = presentationProfile.shadowColor;
            if (presentationProfile.shadowSprite != null)
                shadowSpriteRenderer.sprite = presentationProfile.shadowSprite;
            if (!string.IsNullOrWhiteSpace(presentationProfile.shadowSortingLayerName))
                shadowSpriteRenderer.sortingLayerName = presentationProfile.shadowSortingLayerName;
            shadowSpriteRenderer.sortingOrder = presentationProfile.shadowSortingOrder;
            SetShadowVisible(presentationProfile.shadowSprite != null || presentationProfile.shadowPrefab != null);
        }

        private ActorShadowMode ResolveShadowMode()
        {
            if (presentationProfile == null)
                return ActorShadowMode.None;

            if (presentationProfile.shadowMode != ActorShadowMode.Auto)
                return presentationProfile.shadowMode;

            if (presentationProfile.presentationMode == Animation.ActorPresentationMode.Rigged3D &&
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
                if (_generatedShadowObject == null || _generatedShadowObject.name != presentationProfile.shadowPrefab.name + " (Runtime)")
                {
                    if (_generatedShadowObject != null)
                        DestroyGeneratedShadow();

                    _generatedShadowObject = Instantiate(presentationProfile.shadowPrefab, transform);
                    _generatedShadowObject.name = presentationProfile.shadowPrefab.name + " (Runtime)";
                    shadowRoot = _generatedShadowObject.transform;
                    shadowSpriteRenderer = _generatedShadowObject.GetComponentInChildren<SpriteRenderer>(true);
                }

                return;
            }

            if (_generatedShadowObject == null)
            {
                _generatedShadowObject = new GameObject("RuntimeShadow");
                _generatedShadowObject.transform.SetParent(transform, false);
                shadowRoot = _generatedShadowObject.transform;
                shadowSpriteRenderer = _generatedShadowObject.AddComponent<SpriteRenderer>();
            }
        }

        private void SetShadowVisible(bool visible)
        {
            if (shadowRoot != null)
                shadowRoot.gameObject.SetActive(visible);
        }

        private void DestroyGeneratedShadow()
        {
            if (_generatedShadowObject == null)
                return;

            if (Application.isPlaying)
                Destroy(_generatedShadowObject);
            else
                DestroyImmediate(_generatedShadowObject);

            _generatedShadowObject = null;
            shadowRoot = null;
            shadowSpriteRenderer = null;
        }
    }
}
