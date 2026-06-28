using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
    [AddComponentMenu("NeonBlack/Gameplay/Combat/Actor Combat Reaction Component")]
    [AuthoringContract(
        StableId = "feature.actor.combat.reaction",
        Category = "Combat",
        Surface = AuthoringSurface.Profile,
        Summary = "Adds guard, parry, damage modification, hurt/stagger locks, and combat reaction feedback for an actor.",
        RequiredFields = new[] { nameof(reactionProfile) },
        RequiredComponentNames = new[] { "NeonBlack.Gameplay.Modules.Combat.HealthComponent" },
        RequiredInterfaces = new[] { typeof(IActorGuardController), typeof(IDamageModifier) },
        RequiredInterfaceNames = new[] { "NeonBlack.Gameplay.Core.Contracts.IActorReactionResponder" },
        SetupSteps = new[]
        {
            "Create ActorCombatReactionProfile.",
            "Add ActorCombatReactionComponent to the actor root.",
            "Assign ActorCombatReactionProfile.",
            "Bind Guard in InputProfile."
        },
        SuccessChecks = new[] { "Enter Play Mode and verify guard/parry triggers correctly against enemy attacks." },
        Tags = new[] { "capability:Combat", "lane:Combat" },
        Selectable = false
    )]
    public class ActorCombatReactionComponent : MonoBehaviour, IDamageModifier, IActorGuardController
{
        [SerializeField] private ActorCombatReactionProfile reactionProfile;
        private IActorHealthState _health;
        private KnockbackReceiver _knockback;
        private IActorReactionResponder _reactionResponder;
        private IActorFeedbackPublisher _feedbackPublisher;
        private IActorAnimationController _animation;
        private bool _isGuarding;
        private float _parryTimer;

        public bool IsGuarding => _isGuarding;
        public float BlockDamageReduction => reactionProfile != null ? reactionProfile.blockDamageReduction : 0f;
        public float BlockFrontalAngle => reactionProfile != null ? reactionProfile.blockFrontalAngle : 90f;

        private void Awake()
        {
            _health = GetComponent<IActorHealthState>();
            _knockback = GetComponent<KnockbackReceiver>();
            _reactionResponder = GetComponent<IActorReactionResponder>();
            _feedbackPublisher = GetComponent<IActorFeedbackPublisher>();
            _animation = GetComponent<IActorAnimationController>();
            reactionProfile?.Sanitize();

            if (_health != null)
            {
                _health.Damaged += HandleDamaged;
                _health.Died += HandleDeath;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Damaged -= HandleDamaged;
                _health.Died -= HandleDeath;
            }

            _isGuarding = false;
            _health = null;
            _knockback = null;
            _reactionResponder = null;
            _feedbackPublisher = null;
            _animation = null;
        }

        public void BeginGuard()
        {
            if (reactionProfile == null || !reactionProfile.enableGuard)
                return;

            _isGuarding = true;
            _parryTimer = reactionProfile.enableParry ? reactionProfile.parryWindowDuration : 0f;
            _animation?.TriggerSignal(ActorAnimationSignal.BlockStart);
            _animation?.SetBoolSignal(ActorAnimationSignal.BlockLoop, true);
        }

        public void EndGuard()
        {
            if (!_isGuarding)
                return;

            _isGuarding = false;
            _parryTimer = 0f;
            _animation?.TriggerSignal(ActorAnimationSignal.BlockEnd);
            _animation?.SetBoolSignal(ActorAnimationSignal.BlockLoop, false);
        }

        public bool TryModifyIncomingDamage(GameObject source, ref float incomingDamage)
        {
            if (!_isGuarding || reactionProfile == null || !reactionProfile.enableGuard || source == null)
                return false;

            Vector3 toAttacker = source.transform.position - transform.position;
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
                _animation?.TriggerCustom("Parry");
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
                _animation?.TriggerSignal(ActorAnimationSignal.Stagger);
                _feedbackPublisher?.PublishStagger(damage);
                if (guardBroken)
                    _feedbackPublisher?.PublishGuardBreak();
            }
            else
            {
                _animation?.TriggerSignal(ActorAnimationSignal.Hurt);
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
            IFacingDirectionProvider facingProvider = GetComponent<IFacingDirectionProvider>();
            if (facingProvider != null)
                return facingProvider.FacingRight ? Vector3.right : Vector3.left;

            return transform.right;
        }
    }
}
