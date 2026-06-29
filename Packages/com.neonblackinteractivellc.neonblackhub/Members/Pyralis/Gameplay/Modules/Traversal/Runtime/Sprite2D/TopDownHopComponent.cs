using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Types.Animation;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Traversal
{
    /// <summary>
    /// Top-down/isometric hop action. The actor stays on its map-plane position while
    /// a visual transform lifts on an arc.
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/Traversal/Top Down Hop Component")]
    [AuthoringContract(
        StableId = "feature.actor.traversal.topdown-hop",
        Category = "Movement, Traversal",
        CapabilityPath = "Movement/Traversal/FakeGravityJump",
        Surface = AuthoringSurface.Goal,
        Summary = "Enables fake-gravity jump actions where a Sprite2D or billboard actor arcs visually while maintaining map-plane position.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/traversal",
        RequiredFields = new[]
        {
            "TopDownHopComponent.hopProfile",
            "InputProfile.gameplayActions"
        },
        RequiredInterfaces = new[] { typeof(IActorGameplayActionReceiver) },
        SetupSteps = new[]
        {
            "add TopDownHopComponent to the pawn root",
            "assign TopDownHopProfile",
            "bind Jump in InputProfile"
        },
        SuccessChecks = new[] { "Press the Jump key and verify the actor performs a visual hop animation." },
        RoleTags = new[] { "VisualHop", "FakeGravityJump", "JumpConsumer" },
        Tags = new[] { "capability:Movement", "capability:Traversal", "runtime:CharacterPawnGameplay", "axiom:Dimensions2D", "axiom:GravityNone", "axiom:Realtime", "lane:Traversal" }
    )]
public sealed class TopDownHopComponent : GameplayTickBehaviour, IActorGameplayActionReceiver
{
        [SerializeField] private TopDownHopProfile hopProfile;
        [SerializeField, Tooltip("Optional visual transform to lift. If empty, the runtime uses a child SpriteRenderer or Animator.")]
        private Transform visualTransform;

        private Transform _actorTransform;
        private IActorAnimationController _animationDriver;
        private Vector3 _baseLocalPosition;
        private float _hopTimer;
        private float _cooldownTimer;
        private bool _isHopping;

        public bool IsHopping => _isHopping;
        public float HopProgress => _isHopping && hopProfile != null
            ? Mathf.Clamp01(_hopTimer / Mathf.Max(0.01f, hopProfile.duration))
            : 0f;
        protected override GameplayTickDomain TickDomain => GameplayTickDomain.Traversal;
        protected override bool UsesGameplayTick => true;
        private void Awake()
        {
            ResolveReferences(gameObject);
            hopProfile?.Sanitize();
        }

        private void OnDisable()
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

        protected override void OnGameplayTick(in GameplayTickContext context)
        {
            if (_cooldownTimer > 0f)
                _cooldownTimer = Mathf.Max(0f, _cooldownTimer - context.DeltaTime);

            if (!_isHopping || hopProfile == null || visualTransform == null)
                return;

            _hopTimer += context.DeltaTime;
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
            _animationDriver ??= actorObject.GetComponent<IActorAnimationController>();

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
