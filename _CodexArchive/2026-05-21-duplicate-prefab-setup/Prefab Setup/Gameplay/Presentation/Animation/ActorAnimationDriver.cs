using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Animation
{
    [AddComponentMenu("NeonBlack/Gameplay/Animation/Actor Animation Driver")]
    public class ActorAnimationDriver : MonoBehaviour, IActorAnimationController
    {
        [Header("Scene References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform billboardTarget;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BillboardFacing3D billboardFacing;
        [SerializeField] private UnityEngine.Camera cameraOverride;
        [SerializeField] private ActorShadowDriver shadowDriver;

        [Header("Runtime Profiles")]
        [SerializeField] private PawnPresentationProfile presentationProfile;
        [SerializeField] private PawnAnimationProfile animationProfile;

        private RuntimeAnimatorController _profileController;

        public PawnPresentationProfile PresentationProfile => presentationProfile;
        public PawnAnimationProfile AnimationProfile => animationProfile;
        public ActorPresentationMode PresentationMode => presentationProfile != null
            ? presentationProfile.presentationMode
            : ActorPresentationMode.Sprite2D;

        public Animator Animator => animator;

        private void Awake()
        {
            ResolveReferences();
            ApplyProfiles(presentationProfile, animationProfile);
        }

        private void LateUpdate()
        {
            ApplyBillboard();
            shadowDriver?.TickShadow();
        }

        public void ApplyProfiles(PawnPresentationProfile presentation, PawnAnimationProfile animation)
        {
            presentationProfile = presentation;
            animationProfile = animation;
            ResolveReferences();

            _profileController = animationProfile != null && animationProfile.baseController != null
                ? animationProfile.baseController
                : animator != null ? animator.runtimeAnimatorController : null;

            if (animator != null && _profileController != null)
                animator.runtimeAnimatorController = _profileController;

            ApplyPresentationDefaults();
            shadowDriver?.ApplyProfile(presentationProfile);
            TriggerSignal(ActorAnimationSignal.Spawn);
        }

        public void SetRuntimeControllerOverride(RuntimeAnimatorController controller)
        {
            if (animator == null)
                return;

            animator.runtimeAnimatorController = controller != null ? controller : _profileController;
        }

        public void SetBoolSignal(ActorAnimationSignal signal, bool value)
        {
            if (animator == null || animationProfile == null)
                return;

            foreach (ActorAnimationBinding binding in animationProfile.bindings)
            {
                if (!Matches(binding, signal, null) || binding.bindingType != ActorAnimationBindingType.Bool)
                    continue;

                if (string.IsNullOrWhiteSpace(binding.parameterName))
                    continue;

                animator.SetBool(binding.parameterName, binding.useSignalBool ? value : binding.boolValue);
            }
        }

        public void SetFloatSignal(ActorAnimationSignal signal, float value)
        {
            if (animator == null || animationProfile == null)
                return;

            foreach (ActorAnimationBinding binding in animationProfile.bindings)
            {
                if (!Matches(binding, signal, null) || binding.bindingType != ActorAnimationBindingType.Float)
                    continue;

                if (string.IsNullOrWhiteSpace(binding.parameterName))
                    continue;

                animator.SetFloat(binding.parameterName, binding.useSignalFloat ? value : binding.floatValue);
            }
        }

        public void SetIntSignal(ActorAnimationSignal signal, int value)
        {
            if (animator == null || animationProfile == null)
                return;

            foreach (ActorAnimationBinding binding in animationProfile.bindings)
            {
                if (!Matches(binding, signal, null) || binding.bindingType != ActorAnimationBindingType.Int)
                    continue;

                if (string.IsNullOrWhiteSpace(binding.parameterName))
                    continue;

                animator.SetInteger(binding.parameterName, binding.useSignalInt ? value : binding.intValue);
            }
        }

        public void TriggerSignal(ActorAnimationSignal signal, int intValue = 1, float floatValue = 1f, bool boolValue = true)
        {
            if (animator == null || animationProfile == null)
                return;

            foreach (ActorAnimationBinding binding in animationProfile.bindings)
            {
                if (!Matches(binding, signal, null))
                    continue;

                ApplyBinding(binding, intValue, floatValue, boolValue, trigger: true);
            }
        }

        public void TriggerCustom(string customKey, int intValue = 1, float floatValue = 1f, bool boolValue = true)
        {
            if (animator == null || animationProfile == null || string.IsNullOrWhiteSpace(customKey))
                return;

            foreach (ActorAnimationBinding binding in animationProfile.bindings)
            {
                if (!Matches(binding, ActorAnimationSignal.Custom, customKey))
                    continue;

                ApplyBinding(binding, intValue, floatValue, boolValue, trigger: true);
            }
        }

        public void SetFacing(bool facingRight)
        {
            if (presentationProfile == null)
                return;

            bool flip = presentationProfile.spriteDefaultFacesRight ? !facingRight : facingRight;
            Transform target = billboardTarget != null ? billboardTarget : visualRoot;
            if (target == null)
                return;

            switch (presentationProfile.presentationMode)
            {
                case ActorPresentationMode.Sprite2D:
                    if (spriteRenderer != null)
                        spriteRenderer.flipX = flip;
                    else
                    {
                        Vector3 scale = target.localScale;
                        scale.x = flip ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                        target.localScale = scale;
                    }
                    break;

                case ActorPresentationMode.Billboard2_5D:
                    if (billboardFacing != null)
                    {
                        billboardFacing.ApplyFacing(facingRight);
                    }
                    else if (spriteRenderer != null)
                    {
                        spriteRenderer.flipX = flip;
                    }
                    else
                    {
                        Vector3 scale = target.localScale;
                        scale.x = flip ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                        target.localScale = scale;
                    }
                    break;

                case ActorPresentationMode.Rigged3D:
                    target.localRotation = Quaternion.Euler(0f, flip ? 180f : 0f, 0f);
                    break;
            }
        }

        public void ApplyBillboard()
        {
            if (presentationProfile == null || presentationProfile.presentationMode != ActorPresentationMode.Billboard2_5D)
                return;

            if (billboardFacing != null)
            {
                billboardFacing.ApplyBillboard();
                return;
            }

            Transform target = billboardTarget != null ? billboardTarget : visualRoot;
            UnityEngine.Camera cam = cameraOverride != null ? cameraOverride : UnityEngine.Camera.main;
            if (target == null || cam == null)
                return;

            switch (presentationProfile.billboardFacingMode)
            {
                case BillboardFacingMode.YAxisOnly:
                    Vector3 dir = cam.transform.position - target.position;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.001f)
                        target.rotation = Quaternion.LookRotation(dir);
                    break;

                case BillboardFacingMode.FullFacing:
                    target.rotation = cam.transform.rotation;
                    break;
            }
        }

        private void ApplyPresentationDefaults()
        {
            if (presentationProfile == null)
                return;

            if (spriteRenderer != null)
                spriteRenderer.color = presentationProfile.primaryTint;

            if (billboardFacing != null)
            {
                Transform target = billboardTarget != null ? billboardTarget : visualRoot;
                UnityEngine.Camera cam = cameraOverride != null ? cameraOverride : UnityEngine.Camera.main;
                billboardFacing.Configure(
                    target != null ? target : transform,
                    target != null && target != transform ? target : null,
                    spriteRenderer,
                    cam,
                    presentationProfile.billboardFacingMode == BillboardFacingMode.FullFacing
                        ? BillboardFacing3D.FacingMode.FullFacing
                        : BillboardFacing3D.FacingMode.YAxisOnly,
                    presentationProfile.spriteDefaultFacesRight);
            }
        }

        private void ApplyBinding(ActorAnimationBinding binding, int intValue, float floatValue, bool boolValue, bool trigger)
        {
            if (string.IsNullOrWhiteSpace(binding.parameterName))
                return;

            switch (binding.bindingType)
            {
                case ActorAnimationBindingType.Bool:
                    animator.SetBool(binding.parameterName, binding.useSignalBool ? boolValue : binding.boolValue);
                    break;

                case ActorAnimationBindingType.Float:
                    animator.SetFloat(binding.parameterName, binding.useSignalFloat ? floatValue : binding.floatValue);
                    break;

                case ActorAnimationBindingType.Int:
                    animator.SetInteger(binding.parameterName, binding.useSignalInt ? intValue : binding.intValue);
                    break;

                case ActorAnimationBindingType.Trigger:
                    if (trigger)
                        animator.SetTrigger(binding.parameterName);
                    break;
            }
        }

        private bool Matches(ActorAnimationBinding binding, ActorAnimationSignal signal, string customKey)
        {
            if (binding == null || binding.signal != signal)
                return false;

            if (signal != ActorAnimationSignal.Custom)
                return true;

            return string.Equals(binding.customKey, customKey, System.StringComparison.Ordinal);
        }

        private void ResolveReferences()
        {
            animator ??= GetComponentInChildren<Animator>(true);
            spriteRenderer ??= GetComponentInChildren<SpriteRenderer>(true);
            billboardFacing ??= GetComponent<BillboardFacing3D>();
            shadowDriver ??= GetComponent<ActorShadowDriver>();
            visualRoot ??= animator != null ? animator.transform : spriteRenderer != null ? spriteRenderer.transform : transform;
            billboardTarget ??= visualRoot;
        }
    }
}
