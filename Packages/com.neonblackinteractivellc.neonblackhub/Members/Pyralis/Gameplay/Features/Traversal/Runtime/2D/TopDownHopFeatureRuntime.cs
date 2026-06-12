using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
    /// <summary>
    /// Top-down/isometric hop action. The actor stays on its map-plane position while
    /// a visual transform lifts on an arc.
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/Traversal/Top Down Hop Feature Runtime")]
    [AuthoringContract(
        ModuleId = "actor.traversal.topdown-hop",
        Capability = AuthoringCapability.Traversal,
        Relevance = "Enables top-down/isometric hop actions where actors arc visually while maintaining their map position.",
        Lane = "Traversal",
        ProfileType = typeof(TopDownHopProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorGameplayActionReceiver) },
        SupportedLanes = new[] { ActorPresentationMode.Sprite2D, ActorPresentationMode.Billboard2_5D },
        UnsupportedLanes = new[] { ActorPresentationMode.ThirdPerson3D },
        UnsupportedLaneMessage = "Rigged3D actors should use the 3D traversal jump path instead of the top-down visual-hop module.",
        ConsumedRoles = new[] { "Jump" },
        NativeSetup = new[]
        {
            "create TopDownHopProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with TopDownHopFeatureRuntime",
            "assign profile asset",
            "add module to PawnDefinition.featureModules",
            "bind Jump in InputProfile"
        },
        FirstProof = "Press the Jump key and verify the actor performs a visual hop animation.",
        FirstProofTargetId = "proof.1p-pawn-movement",
        AssignmentFields = new[]
        {
            "FeatureModuleDefinition.moduleId",
            "FeatureModuleDefinition.runtimePrefab",
            "FeatureModuleDefinition.profileAsset",
            "PawnDefinition.featureModules",
            "InputProfile.gameplayActions"
        },
        ExpertAdvice = "A purely visual traversal module. It does not change the physical collider position, making it perfect for Tilemap-based games where depth jitter must be avoided.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/traversal"
    )]
public sealed class TopDownHopFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime, IActorGameplayActionReceiver
{
        [SerializeField] private TopDownHopProfile hopProfile;
        [SerializeField, Tooltip("Optional visual transform to lift. If empty, the runtime uses a child SpriteRenderer or Animator.")]
        private Transform visualTransform;

        private Transform _actorTransform;
        private ActorAnimationDriver _animationDriver;
        private Vector3 _baseLocalPosition;
        private float _hopTimer;
        private float _cooldownTimer;
        private bool _isHopping;

        public bool IsHopping => _isHopping;
        public float HopProgress => _isHopping && hopProfile != null
            ? Mathf.Clamp01(_hopTimer / Mathf.Max(0.01f, hopProfile.duration))
            : 0f;
        public string ModuleId => "actor.traversal.topdown-hop";

        private void Awake()
        {
            ResolveReferences(gameObject);
        }

        public void InitializeFeature(FeatureRuntimeInitializationContext context)
        {
            _actorTransform = context != null ? context.ActorTransform : transform;
            if (context != null)
                hopProfile = context.GetProfile<TopDownHopProfile>(context.Definition != null ? context.Definition.profileAsset : null) ?? hopProfile;

            ResolveReferences(_actorTransform != null ? _actorTransform.gameObject : gameObject);
            hopProfile?.Sanitize();
        }

        public void ShutdownFeature()
        {
            ResetVisual();
            _isHopping = false;
            _hopTimer = 0f;
            _cooldownTimer = 0f;
        }

        public bool TryHandleGameplayAction(string actionKey)
        {
            if (hopProfile == null)
                return false;

            hopProfile.Sanitize();
            if (!string.Equals(actionKey, hopProfile.actionRole.ToString(), System.StringComparison.Ordinal))
                return false;

            if (_isHopping && !hopProfile.allowRestartWhileHopping)
                return true;

            if (_cooldownTimer > 0f)
                return true;

            StartHop();
            return true;
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer = Mathf.Max(0f, _cooldownTimer - Time.deltaTime);

            if (!_isHopping || hopProfile == null || visualTransform == null)
                return;

            _hopTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_hopTimer / Mathf.Max(0.01f, hopProfile.duration));
            float lift = Mathf.Sin(progress * Mathf.PI) * hopProfile.height;
            visualTransform.localPosition = _baseLocalPosition + Vector3.up * lift;

            if (progress >= 1f)
            {
                ResetVisual();
                _isHopping = false;
                _cooldownTimer = hopProfile.cooldown;
            }
        }

        private void StartHop()
        {
            ResolveReferences(_actorTransform != null ? _actorTransform.gameObject : gameObject);
            if (visualTransform == null)
                return;

            _baseLocalPosition = visualTransform.localPosition;
            _hopTimer = 0f;
            _isHopping = true;

            if (hopProfile != null && hopProfile.triggerJumpAnimation)
                _animationDriver?.TriggerSignal(ActorAnimationSignal.Jump);
        }

        private void ResetVisual()
        {
            if (visualTransform != null)
                visualTransform.localPosition = _baseLocalPosition;
        }

        private void ResolveReferences(GameObject actorObject)
        {
            if (actorObject == null)
                actorObject = gameObject;

            _actorTransform ??= actorObject.transform;
            _animationDriver ??= actorObject.GetComponent<ActorAnimationDriver>();

            if (visualTransform != null)
            {
                _baseLocalPosition = visualTransform.localPosition;
                return;
            }

            SpriteRenderer spriteRenderer = actorObject.GetComponentInChildren<SpriteRenderer>(true);
            if (spriteRenderer != null && spriteRenderer.transform != actorObject.transform)
                visualTransform = spriteRenderer.transform;
            else
            {
                Animator animator = actorObject.GetComponentInChildren<Animator>(true);
                visualTransform = animator != null && animator.transform != actorObject.transform
                    ? animator.transform
                    : null;
            }

            _baseLocalPosition = visualTransform != null ? visualTransform.localPosition : Vector3.zero;
        }
    }
}
