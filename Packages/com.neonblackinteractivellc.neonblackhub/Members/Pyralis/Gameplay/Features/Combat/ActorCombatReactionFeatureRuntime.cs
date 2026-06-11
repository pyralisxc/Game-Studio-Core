using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
    [AddComponentMenu("NeonBlack/Gameplay/Combat/Actor Combat Reaction Feature Runtime")]
    [AuthoringContract(
        ModuleId = "actor.combat.reaction",
        Capability = AuthoringCapability.Combat,
        Relevance = "Adds guard, parry, damage modification, hurt/stagger locks, and combat reaction feedback for an actor.",
        Lane = "Combat",
        ProfileType = typeof(ActorCombatReactionProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorGuardFeature), typeof(IDamageModifier) },
        RequiredInterfaceNames = new[] { "NeonBlack.Gameplay.Features.Combat.IActorReactionResponder" },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Features.Combat.HealthComponent" },
        ConsumedRoles = new[] { "Guard" },
        NativeSetup = new[]
        {
            "Create ActorCombatReactionProfile.",
            "Create FeatureModuleDefinition.",
            "Assign runtime prefab with ActorCombatReactionFeatureRuntime.",
            "Assign profile asset.",
            "Add module to PawnDefinition.featureModules.",
            "Bind Guard in InputProfile."
        },
        FirstProof = "Enter Play Mode and verify guard/parry triggers correctly against enemy attacks.",
        AssignmentFields = new[] { nameof(reactionProfile) },
        ExpertAdvice = "Pair the actor root with HealthComponent, a movement/reaction responder, KnockbackReceiver when knockback is used, ActorAnimationDriver for guard/hurt/stagger signals, and impact feedback sinks when hit pause or camera shake is enabled.",
        CustomizationMoments = new[]
        {
            "ActorCombatReactionProfile.blockDamageReduction",
            "ActorCombatReactionProfile.parryWindowDuration",
            "ActorCombatReactionProfile.staggerDamageThreshold"
        }
    )]
    public class ActorCombatReactionFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime, IDamageModifier, IActorGuardFeature
{
        [SerializeField] private ActorCombatReactionProfile reactionProfile;
        private ActorFeatureContext _context;
        private IActorHealthState _health;
        private KnockbackReceiver _knockback;
        private IActorReactionResponder _reactionResponder;
        private IActorFeedbackPublisher _feedbackPublisher;
        private bool _isGuarding;
        private float _parryTimer;

        public string ModuleId => "actor.combat.reaction";
        public bool IsGuarding => _isGuarding;
        public float BlockDamageReduction => reactionProfile != null ? reactionProfile.blockDamageReduction : 0f;
        public float BlockFrontalAngle => reactionProfile != null ? reactionProfile.blockFrontalAngle : 90f;

        public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
        {
            ActorFeatureContext context = initializationContext != null ? initializationContext.ActorContext : null;
            var definition = initializationContext != null ? initializationContext.Definition : null;
            _context = context;
            _health = context != null ? context.Health : null;
            _knockback = context != null ? context.Knockback as KnockbackReceiver : null;
            _reactionResponder = context != null && context.ActorObject != null
                ? context.ActorObject.GetComponent<IActorReactionResponder>()
                : null;
            _feedbackPublisher = context != null && context.ActorObject != null
                ? context.ActorObject.GetComponent<IActorFeedbackPublisher>()
                : null;

            reactionProfile = initializationContext.GetProfile<ActorCombatReactionProfile>(definition != null ? definition.profileAsset : null);
            reactionProfile?.Sanitize();

            if (_health != null)
            {
                _health.Damaged += HandleDamaged;
                _health.Died += HandleDeath;
            }
        }

        public void ShutdownFeature()
        {
            if (_health != null)
            {
                _health.Damaged -= HandleDamaged;
                _health.Died -= HandleDeath;
            }

            _isGuarding = false;
            _context = null;
            _health = null;
            _knockback = null;
            _reactionResponder = null;
            _feedbackPublisher = null;
        }

        public void BeginGuard()
        {
            if (reactionProfile == null || !reactionProfile.enableGuard)
                return;

            _isGuarding = true;
            _parryTimer = reactionProfile.enableParry ? reactionProfile.parryWindowDuration : 0f;
            _context?.Animation?.TriggerSignal(ActorAnimationSignal.BlockStart);
            _context?.Animation?.SetBoolSignal(ActorAnimationSignal.BlockLoop, true);
        }

        public void EndGuard()
        {
            if (!_isGuarding)
                return;

            _isGuarding = false;
            _parryTimer = 0f;
            _context?.Animation?.TriggerSignal(ActorAnimationSignal.BlockEnd);
            _context?.Animation?.SetBoolSignal(ActorAnimationSignal.BlockLoop, false);
        }

        public bool TryModifyIncomingDamage(GameObject source, ref float incomingDamage)
        {
            if (!_isGuarding || reactionProfile == null || !reactionProfile.enableGuard || source == null || _context == null)
                return false;

            Vector3 toAttacker = source.transform.position - _context.ActorTransform.position;
            if (_context.PresentationMode != ActorPresentationMode.Sprite2D)
                toAttacker.y = 0f;

            if (toAttacker.sqrMagnitude <= 0.001f)
                return false;

            Vector3 facingDir = ResolveFacingDirection();
            float threshold = Mathf.Cos(reactionProfile.blockFrontalAngle * Mathf.Deg2Rad);
            if (Vector3.Dot(facingDir.normalized, toAttacker.normalized) < threshold)
                return false;

            if (_parryTimer > 0f)
            {
                incomingDamage = 0f;
                _feedbackPublisher?.PublishParry();
                if (source != null)
                {
                    source.GetComponentInParent<IActorReactionResponder>()?.ApplyReactionLock(reactionProfile.parryReactionLockDuration);
                    source.GetComponentInParent<KnockbackReceiver>()?.ClearKnockback();
                }
                _context?.Animation?.TriggerCustom("Parry");
                EndGuard();
                return true;
            }

            incomingDamage *= reactionProfile.blockDamageReduction;
            return true;
        }

        private void HandleDamaged(float damage)
        {
            if (reactionProfile == null)
                return;

            EndGuard();

            bool staggered = reactionProfile.enableReactionLocks
                && reactionProfile.staggerDamageThreshold > 0f
                && damage >= reactionProfile.staggerDamageThreshold;
            bool guardBroken = _isGuarding && staggered;

            float lockDuration = staggered
                ? (guardBroken ? reactionProfile.shieldBreakLockDuration : reactionProfile.staggerLockDuration)
                : reactionProfile.hurtLockDuration;
            if (reactionProfile.enableReactionLocks && lockDuration > 0f)
                _reactionResponder?.ApplyReactionLock(lockDuration);

            if (staggered)
            {
                if (reactionProfile.clearKnockbackOnStagger)
                    _knockback?.ClearKnockback();
                _context?.Animation?.TriggerSignal(ActorAnimationSignal.Stagger);
                _feedbackPublisher?.PublishStagger(damage);
                if (guardBroken)
                    _feedbackPublisher?.PublishGuardBreak();
            }
            else
            {
                _context?.Animation?.TriggerSignal(ActorAnimationSignal.Hurt);
            }
        }

        private void HandleDeath()
        {
            EndGuard();
            _reactionResponder?.ClearReactionLock();
            if (reactionProfile != null && reactionProfile.clearKnockbackOnDeath)
                _knockback?.ClearKnockback();
        }

        private void Update()
        {
            if (_parryTimer > 0f)
                _parryTimer = Mathf.Max(0f, _parryTimer - Time.deltaTime);
        }

        private Vector3 ResolveFacingDirection()
        {
            if (_context != null && _context.ActorObject != null)
            {
                IFacingDirectionProvider facingProvider = _context.ActorObject.GetComponent<IFacingDirectionProvider>();
                if (facingProvider != null)
                    return facingProvider.FacingRight ? Vector3.right : Vector3.left;
            }

            return _context != null ? _context.ActorTransform.right : Vector3.right;
        }
    }
}
