using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;

namespace NeonBlack.Gameplay.Presentation.Animation
{
    [AuthoringContract(
        Capability = AuthoringCapability.Animation, 
        Relevance = "Inspector Add Component path for actor animation and presentation mapping.",
        FirstProof = "Enter Play Mode and verify the actor visual reflects the chosen Presentation Mode. Confirm that movement signals (if mapped) trigger animation state changes in the Animator.",
        ExpertAdvice = "ActorAnimationDriver bridges Pyralis signals (like Move, Jump, Attack) to your Animator parameters. Ensure the PawnAnimationProfile bindings match your Animator parameter names exactly.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/animation")]
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
        private readonly List<RuntimeAnimationBinding> _compiledBindings = new List<RuntimeAnimationBinding>();
        private readonly HashSet<string> _reportedInvalidBindings = new HashSet<string>();

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

            RebuildBindingCache();
            ApplyPresentationDefaults();
            shadowDriver?.ApplyProfile(presentationProfile);
            TriggerSignal(ActorAnimationSignal.Spawn);
        }

        public void SetRuntimeControllerOverride(RuntimeAnimatorController controller)
        {
            if (animator == null)
                return;

            animator.runtimeAnimatorController = controller != null ? controller : _profileController;
            RebuildBindingCache();
        }

        public void SetCameraOverride(UnityEngine.Camera camera)
        {
            cameraOverride = camera;
            ApplyPresentationDefaults();
        }

        public void SetBoolSignal(ActorAnimationSignal signal, bool value)
        {
            if (animator == null || animationProfile == null)
                return;

            foreach (RuntimeAnimationBinding binding in _compiledBindings)
            {
                if (!Matches(binding.Binding, signal, null) || binding.Binding.bindingType != ActorAnimationBindingType.Bool)
                    continue;

                animator.SetBool(binding.ParameterHash, binding.Binding.useSignalBool ? value : binding.Binding.boolValue);
            }
        }

        public void SetFloatSignal(ActorAnimationSignal signal, float value)
        {
            if (animator == null || animationProfile == null)
                return;

            foreach (RuntimeAnimationBinding binding in _compiledBindings)
            {
                if (!Matches(binding.Binding, signal, null) || binding.Binding.bindingType != ActorAnimationBindingType.Float)
                    continue;

                animator.SetFloat(binding.ParameterHash, binding.Binding.useSignalFloat ? value : binding.Binding.floatValue);
            }
        }

        public void SetIntSignal(ActorAnimationSignal signal, int value)
        {
            if (animator == null || animationProfile == null)
                return;

            foreach (RuntimeAnimationBinding binding in _compiledBindings)
            {
                if (!Matches(binding.Binding, signal, null) || binding.Binding.bindingType != ActorAnimationBindingType.Int)
                    continue;

                animator.SetInteger(binding.ParameterHash, binding.Binding.useSignalInt ? value : binding.Binding.intValue);
            }
        }

        public void SetBoolCustom(string customKey, bool value)
        {
            if (animator == null || animationProfile == null || string.IsNullOrWhiteSpace(customKey))
                return;

            foreach (RuntimeAnimationBinding binding in _compiledBindings)
            {
                if (!Matches(binding.Binding, ActorAnimationSignal.Custom, customKey) || binding.Binding.bindingType != ActorAnimationBindingType.Bool)
                    continue;

                animator.SetBool(binding.ParameterHash, binding.Binding.useSignalBool ? value : binding.Binding.boolValue);
            }
        }

        public void SetFloatCustom(string customKey, float value)
        {
            if (animator == null || animationProfile == null || string.IsNullOrWhiteSpace(customKey))
                return;

            foreach (RuntimeAnimationBinding binding in _compiledBindings)
            {
                if (!Matches(binding.Binding, ActorAnimationSignal.Custom, customKey) || binding.Binding.bindingType != ActorAnimationBindingType.Float)
                    continue;

                animator.SetFloat(binding.ParameterHash, binding.Binding.useSignalFloat ? value : binding.Binding.floatValue);
            }
        }

        public void SetIntCustom(string customKey, int value)
        {
            if (animator == null || animationProfile == null || string.IsNullOrWhiteSpace(customKey))
                return;

            foreach (RuntimeAnimationBinding binding in _compiledBindings)
            {
                if (!Matches(binding.Binding, ActorAnimationSignal.Custom, customKey) || binding.Binding.bindingType != ActorAnimationBindingType.Int)
                    continue;

                animator.SetInteger(binding.ParameterHash, binding.Binding.useSignalInt ? value : binding.Binding.intValue);
            }
        }

        public void TriggerSignal(ActorAnimationSignal signal, int intValue = 1, float floatValue = 1f, bool boolValue = true)
        {
            if (animator == null || animationProfile == null)
                return;

            foreach (RuntimeAnimationBinding binding in _compiledBindings)
            {
                if (!Matches(binding.Binding, signal, null))
                    continue;

                ApplyBinding(binding, intValue, floatValue, boolValue, trigger: true);
            }
        }

        public void TriggerCustom(string customKey, int intValue = 1, float floatValue = 1f, bool boolValue = true)
        {
            if (animator == null || animationProfile == null || string.IsNullOrWhiteSpace(customKey))
                return;

            foreach (RuntimeAnimationBinding binding in _compiledBindings)
            {
                if (!Matches(binding.Binding, ActorAnimationSignal.Custom, customKey))
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
            UnityEngine.Camera cam = cameraOverride;
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
                billboardFacing.Configure(
                    target != null ? target : transform,
                    target != null && target != transform ? target : null,
                    spriteRenderer,
                    cameraOverride,
                    presentationProfile.billboardFacingMode == BillboardFacingMode.FullFacing
                        ? BillboardFacing3D.FacingMode.FullFacing
                        : BillboardFacing3D.FacingMode.YAxisOnly,
                    presentationProfile.spriteDefaultFacesRight);
            }
        }

        private void RebuildBindingCache()
        {
            _compiledBindings.Clear();

            if (animator == null || animationProfile == null || animationProfile.bindings == null)
                return;

            Dictionary<string, AnimatorControllerParameterType> parameters = BuildAnimatorParameterLookup(animator);

            foreach (ActorAnimationBinding binding in animationProfile.bindings)
            {
                if (binding == null || string.IsNullOrWhiteSpace(binding.parameterName))
                    continue;

                string parameterName = binding.parameterName.Trim();
                if (!parameters.TryGetValue(parameterName, out AnimatorControllerParameterType parameterType))
                {
                    ReportInvalidBindingOnce(binding, $"Animator parameter '{parameterName}' was not found on '{AnimatorControllerName()}'.");
                    continue;
                }

                if (!BindingTypeMatchesParameter(binding.bindingType, parameterType))
                {
                    ReportInvalidBindingOnce(binding, $"Animator parameter '{parameterName}' is {parameterType}, but binding '{binding.signal}' expects {binding.bindingType}.");
                    continue;
                }

                _compiledBindings.Add(new RuntimeAnimationBinding(binding, Animator.StringToHash(parameterName)));
            }
        }

        private static Dictionary<string, AnimatorControllerParameterType> BuildAnimatorParameterLookup(Animator targetAnimator)
        {
            Dictionary<string, AnimatorControllerParameterType> lookup = new Dictionary<string, AnimatorControllerParameterType>();
            if (targetAnimator == null)
                return lookup;

            AnimatorControllerParameter[] parameters = targetAnimator.parameters;
            if (parameters == null)
                return lookup;

            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (!lookup.ContainsKey(parameter.name))
                    lookup.Add(parameter.name, parameter.type);
            }

            return lookup;
        }

        private void ReportInvalidBindingOnce(ActorAnimationBinding binding, string message)
        {
            string key = $"{AnimatorControllerName()}|{binding.signal}|{binding.customKey}|{binding.parameterName}|{binding.bindingType}|{message}";
            if (!_reportedInvalidBindings.Add(key))
                return;

            Debug.LogWarning($"[ActorAnimationDriver] {message} The binding will be skipped.", this);
        }

        private string AnimatorControllerName()
        {
            return animator != null && animator.runtimeAnimatorController != null
                ? animator.runtimeAnimatorController.name
                : "Unassigned Animator Controller";
        }

        private static bool BindingTypeMatchesParameter(ActorAnimationBindingType bindingType, AnimatorControllerParameterType parameterType)
        {
            return bindingType switch
            {
                ActorAnimationBindingType.Bool => parameterType == AnimatorControllerParameterType.Bool,
                ActorAnimationBindingType.Float => parameterType == AnimatorControllerParameterType.Float,
                ActorAnimationBindingType.Int => parameterType == AnimatorControllerParameterType.Int,
                ActorAnimationBindingType.Trigger => parameterType == AnimatorControllerParameterType.Trigger,
                _ => false
            };
        }

        private void ApplyBinding(RuntimeAnimationBinding binding, int intValue, float floatValue, bool boolValue, bool trigger)
        {
            switch (binding.Binding.bindingType)
            {
                case ActorAnimationBindingType.Bool:
                    animator.SetBool(binding.ParameterHash, binding.Binding.useSignalBool ? boolValue : binding.Binding.boolValue);
                    break;

                case ActorAnimationBindingType.Float:
                    animator.SetFloat(binding.ParameterHash, binding.Binding.useSignalFloat ? floatValue : binding.Binding.floatValue);
                    break;

                case ActorAnimationBindingType.Int:
                    animator.SetInteger(binding.ParameterHash, binding.Binding.useSignalInt ? intValue : binding.Binding.intValue);
                    break;

                case ActorAnimationBindingType.Trigger:
                    if (trigger)
                        animator.SetTrigger(binding.ParameterHash);
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

        private readonly struct RuntimeAnimationBinding
        {
            public RuntimeAnimationBinding(ActorAnimationBinding binding, int parameterHash)
            {
                Binding = binding;
                ParameterHash = parameterHash;
            }

            public ActorAnimationBinding Binding { get; }
            public int ParameterHash { get; }
        }
    }
}
