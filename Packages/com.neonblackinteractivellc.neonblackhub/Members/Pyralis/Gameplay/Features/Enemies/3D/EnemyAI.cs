using System.Collections;
using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    /// <summary>
    /// Patrol, chase, and attack state-machine AI for 2.5D enemies.
    ///
    /// Setup:
    ///  1. Add a CharacterController to this GameObject.
    ///  2. Add a HealthComponent (set faction to Enemy).
    ///  3. Add a KnockbackReceiver.
    ///  4. Add an Animator with the following parameters:
    ///  IsMoving (bool), IsGrounded (bool), Attack (trigger), Death (trigger)
    ///  5. Add a HitBox child and assign it to the Attack Hitbox field.
    ///  6. Optionally assign Patrol Points for fixed patrol routes.
    ///  Leave empty for a back-and-forth random patrol around the spawn point.
    ///  7. Assign a WeaponData asset for damage/range values.
    ///
    /// Behavior overview:
    ///  Idle -> hears/sees player within AggroRange -> Chase
    ///  Chase -> player within AttackRange -> Attack
    ///  Chase -> loses player beyond LeashRange -> Return to patrol
    ///  Attack -> plays attack animation/hitbox fires -> returns to Chase or Idle
    ///  Any -> health reaches 0 -> Death
    /// </summary>
    [AddComponentMenu("NeonBlack/Gameplay/Enemies/Enemy AI")]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(HealthComponent))]
    public class EnemyAI : MonoBehaviour, IActorMovementModifierReceiver, IActorCombatModifierReceiver, IEnemyActorState
    {
        //  States  //
        private enum State { Patrol, Chase, Attack, Dead }
        private enum AttackPriorityProfile
        {
            LongestRange,
            HighestDamage,
            HighestKnockback,
            HighestAssetPriority,
            WeightedScore
        }

        //  Inspector  //
        [Header("Detection")]
        [Tooltip("Radius within which the enemy notices the player.")]
        [SerializeField] private float aggroRange = 8f;
        [Tooltip("Once chasing, the enemy gives up if the player gets this far away.")]
        [SerializeField] private float leashRange = 16f;
        [Tooltip("Layers that block line-of-sight (optional). Leave default to ignore LoS.")]
        [SerializeField] private LayerMask obstacleMask;
        [Tooltip("If true, requires an unobstructed line-of-sight to aggro.")]
        [SerializeField] private bool requireLineOfSight = false;
        [Tooltip("Optional direct target override for simple scenes. Leave empty to use the active player provider from participant infrastructure.")]
        [SerializeField] private Transform targetOverride;

        [Header("Movement")]
        [Tooltip("ThreeD  move on X/Z (2.5D brawler).\nTwoD  move on X only (side-scroller).")]
        [SerializeField] private MovementMode movementMode = MovementMode.ThreeD;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float gravity = -20f;
        [Tooltip("How closely the enemy must reach each patrol point before moving to the next.")]
        [SerializeField] private float waypointTolerance = 0.4f;

        [Header("Visuals")]
        [Tooltip("Child Transform that holds the sprite and hitbox children. localScale.x flips +/-1 to face left/right so hitboxes mirror automatically. Leave empty to fall back to SpriteRenderer.flipX.")]
        [SerializeField] private Transform visualRoot;

        [Tooltip("Enable if your sprite sheet is drawn facing RIGHT. Disable if it is drawn facing LEFT.")]
        [SerializeField] private bool spriteDefaultFacesRight = true;
        [SerializeField] private BillboardFacing3D billboardFacing;
        [SerializeField] private Camera presentationCamera;

        [Header("Patrol Points  (leave empty for random patrol)")]
        [Tooltip("Assign world-space Transforms for fixed patrol routes. If empty, the enemy patrols left/right from its spawn position.")]
        [SerializeField] private Transform[] patrolPoints;

        [Tooltip("Distance for random left/right patrol when no waypoints are assigned.")]
        [SerializeField] private float randomPatrolDistance = 4f;

        [Header("Combat")]
        [Tooltip("Optional actor-level feature composition profile for this enemy.")]
        [SerializeField] private EnemyFeatureProfile enemyFeatureProfile;

        [Tooltip("Optional shared enemy combat profile. When assigned, it becomes the source of truth for attack sequencing and selection weights.")]
        [SerializeField] private EnemyCombatProfile combatProfile;

        [Tooltip("Optional shared enemy reaction profile used by reaction feature modules.")]
        [SerializeField] private EnemyReactionProfile reactionProfile;

        [Tooltip("Named hitbox zones on this enemy. Add one per region of space (e.g. Punch, Wide, Aerial). Zone names must match EnemyAttack.hitBoxZone on each attack asset.")]
        [SerializeField] private HitBoxSlot[] hitBoxZones;

        [Tooltip("Attack definitions. Each slot is an EnemyAttack asset specifying the trigger, zone, damage, and timing.")]
        [SerializeField] private EnemyAttack[] attackSequence;

        [Tooltip("Sequential: cycle through attacks in order.\nRandom: pick by weight each swing.")]
        [SerializeField] private AttackMode attackMode = AttackMode.Sequential;

        [Tooltip("If enabled, attack selection uses priority profile rules (damage/knockback/range/asset priority).")]
        [SerializeField] private bool usePrioritySelection = true;

        [Tooltip("How the AI prioritizes attacks when selecting the next move.")]
        [SerializeField] private AttackPriorityProfile attackPriorityProfile = AttackPriorityProfile.WeightedScore;

        [Tooltip("When enabled, AI only considers attacks that can currently reach the player. If none can reach, it falls back to all attacks.")]
        [SerializeField] private bool preferAttacksCurrentlyInRange = true;

        [Tooltip("Range contribution when Attack Priority Profile is Weighted Score.")]
        [SerializeField] private float rangeWeight = 1.0f;

        [Tooltip("Damage contribution when Attack Priority Profile is Weighted Score.")]
        [SerializeField] private float damageWeight = 1.0f;

        [Tooltip("Knockback contribution when Attack Priority Profile is Weighted Score.")]
        [SerializeField] private float knockbackWeight = 0.75f;

        [Tooltip("EnemyAttack.aiPriority contribution when Attack Priority Profile is Weighted Score.")]
        [SerializeField] private float assetPriorityWeight = 1.0f;

        [Tooltip("Seconds between attacks. Overridden by EnemyAttack.attackCooldown if > 0.")]
        [SerializeField] private float attackCooldown = 0.5f;

        [Tooltip("How close before attacking. 0 = auto-measured from hitbox collider bounds at Awake. Set > 0 to override.")]
        [SerializeField] private float attackRangeOverride = 0f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundLayer = Physics.DefaultRaycastLayers;
        [SerializeField] private float groundCheckRadius = 0.2f;

        //  Animator Hashes  //
        private static readonly int H_IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int H_Grounded = Animator.StringToHash("IsGrounded");
        private static readonly int H_Death = Animator.StringToHash("Death");
        private static readonly int H_Hit = Animator.StringToHash("Hit");
        // Attack trigger hashes are pre-computed in Awake from EnemyAttack.animatorTrigger strings.

        //  Private  //
        private CharacterController _controller;
        private HealthComponent _health;
        private Animator _animator;
        private ActorAnimationDriver _animationDriver;
        private ActorFeatureHost _featureHost;
        private SpriteRenderer _spriteRenderer;
        private KnockbackReceiver _knockbackReceiver;
        private IEnemyReactionState _reactionState;

        private Transform _player;
        private State _state = State.Patrol;
        private float _attackTimer;
        private float _verticalVel;
        private bool _isGrounded;
        private bool _statusActionLocked;
        private float _statusMoveSpeedMultiplier = 1f;
        private float _outgoingDamageMultiplier = 1f;
        private float _outgoingKnockbackMultiplier = 1f;

        private int _sequenceIndex;  // position in attackSequence (Sequential mode)
        private float _computedAttackRange; // resolved in Awake from hitboxes or override
        private float _minAttackRangeFromAttacks; // minimum range across all attacks
        private readonly Dictionary<EnemyAttack, int> _attackTriggerHashes = new Dictionary<EnemyAttack, int>();
        private readonly List<EnemyAttack> _attackCandidates = new List<EnemyAttack>(8);
        private Vector3 _spawnPos;
        private int _patrolIndex;
        private Vector3 _randomPatrolTarget;
        private bool _hasRandomTarget;
        private Camera _cam;

        public bool IsPatrolling => _state == State.Patrol;
        public bool IsChasing => _state == State.Chase;
        public bool IsAttacking => _state == State.Attack;

        //  Lifecycle  //
        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _health = GetComponent<HealthComponent>();
            _knockbackReceiver = GetComponent<KnockbackReceiver>();
            _animator = GetComponentInChildren<Animator>();
            _animationDriver = GetComponent<ActorAnimationDriver>();
            _featureHost = GetComponent<ActorFeatureHost>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _spawnPos = transform.position;
            _cam = presentationCamera;
            ApplyFeatureProfile(enemyFeatureProfile);
            ApplyCombatProfile(combatProfile);
            if (billboardFacing == null)
                billboardFacing = GetComponent<BillboardFacing3D>();
            if (billboardFacing != null)
            {
                billboardFacing.Configure(
                visualRoot != null ? visualRoot : transform,
                visualRoot,
                _spriteRenderer,
                _cam,
                BillboardFacing3D.FacingMode.YAxisOnly,
                spriteDefaultFacesRight);
            }
            // Cache the hitbox's world-X offset from the character root.
            // Using world-relative distance (not localPosition) makes FaceTarget
            // correct regardless of where the hitbox sits in the hierarchy.
            // Cache each zone's X offset from root, measured before any scale flip.
            if (hitBoxZones != null)
                foreach (var slot in hitBoxZones)
                    slot.absOffsetX = slot.hitBox != null
                    ? Mathf.Max(Mathf.Abs(slot.hitBox.transform.position.x - transform.position.x), 0.5f)
                    : 0.5f;

            // Compute global attack range  manual override wins, otherwise measure from all zones.
            if (attackRangeOverride > 0f)
            {
                _computedAttackRange = attackRangeOverride;
            }
            else if (hitBoxZones != null && hitBoxZones.Length > 0)
            {
                _computedAttackRange = 0f;
                foreach (var slot in hitBoxZones)
                    if (slot.hitBox != null)
                        _computedAttackRange = Mathf.Max(_computedAttackRange,
                        MeasureHitBoxRange(slot.hitBox, slot.absOffsetX));
                if (_computedAttackRange < 0.01f) _computedAttackRange = 1.0f;
            }
            else
            {
                _computedAttackRange = 1.0f;
            }

            // Compute minimum attack range from all attacks for state transition
            _minAttackRangeFromAttacks = GetMinAttackRange();

            // Pre-hash all attack trigger strings  never call StringToHash in Update.
            if (attackSequence != null)
                foreach (var atk in attackSequence)
                    if (atk != null && !string.IsNullOrEmpty(atk.animatorTrigger)
                    && !_attackTriggerHashes.ContainsKey(atk))
                        _attackTriggerHashes[atk] = Animator.StringToHash(atk.animatorTrigger);

            // Hook death event
            _health.OnDeath.AddListener(OnDeath);
            _health.OnDamaged.AddListener(OnHit);

            _player = ResolvePlayerTarget();

            InitializeFeatureModules();
        }

        private Transform ResolvePlayerTarget()
        {
            if (targetOverride != null)
                return targetOverride;

            if (ParticipantQueryUtility.TryResolvePlayerProvider(out var provider) && provider != null)
                return provider.GetPlayerTransform();

            return null;
        }

        private void Update()
        {
            if (_state == State.Dead) return;

            ApplyGravity();

            if ((_reactionState != null && _reactionState.IsReactionLocked) || _statusActionLocked)
            {
                UpdateAnimator();
                return;
            }

            switch (_state)
            {
                case State.Patrol: UpdatePatrol(); break;
                case State.Chase: UpdateChase(); break;
                case State.Attack: UpdateAttack(); break;
            }

            UpdateAnimator();
        }

        //  Gravity  //
        private void ApplyGravity()
        {
            Vector3 feetPos = transform.position + _controller.center
            + Vector3.down * (_controller.height * 0.5f - _controller.radius);
            _isGrounded = Physics.CheckSphere(feetPos, groundCheckRadius, groundLayer,
            QueryTriggerInteraction.Ignore);

            if (_isGrounded && _verticalVel < 0f) _verticalVel = -2f;
            _verticalVel += gravity * Time.deltaTime;

            // Always tick knockback decay.
            if (_knockbackReceiver != null)
                _knockbackReceiver.Tick(Time.deltaTime);

            // Apply gravity + knockback only for states that do NOT call MoveToward.
            // Patrol and Chase fold gravity and knockback into their MoveToward call,
            // so issuing a second Move here would double-apply both.
            if (_state != State.Patrol && _state != State.Chase)
            {
                Vector3 kb = _knockbackReceiver != null ? _knockbackReceiver.Velocity : Vector3.zero;
                _controller.Move(new Vector3(kb.x, _verticalVel + kb.y, kb.z) * Time.deltaTime);
            }
        }

        //  Patrol  //
        private void UpdatePatrol()
        {
            if (CanSeePlayer())
            {
                _state = State.Chase;
                return;
            }

            Vector3 target = GetPatrolTarget();
            MoveToward(target, moveSpeed * 0.6f * _statusMoveSpeedMultiplier);

            // Advance to next waypoint
            float distXZ = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(target.x, target.z));

            if (distXZ < waypointTolerance)
            {
                if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
                }
                else
                {
                    _hasRandomTarget = false; // pick a new random target next frame
                }
            }
        }

        private Vector3 GetPatrolTarget()
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
                return patrolPoints[_patrolIndex].position;

            // Random patrol: pick a point left or right of spawn
            if (!_hasRandomTarget)
            {
                float offset = Random.value > 0.5f ? randomPatrolDistance : -randomPatrolDistance;
                _randomPatrolTarget = _spawnPos + new Vector3(offset, 0f, 0f);
                _hasRandomTarget = true;
            }
            return _randomPatrolTarget;
        }

        //  Chase  //
        private void UpdateChase()
        {
            if (_player == null) { _state = State.Patrol; return; }

            float dist = HorizontalDistance(_player.position);

            // Leash check
            if (dist > leashRange)
            {
                _state = State.Patrol;
                return;
            }

            // Enter Attack if ANY attack is in range (use minimum attack range)
            if (dist <= _minAttackRangeFromAttacks * 1.5f)
            {
                _state = State.Attack;
                return;
            }

            MoveToward(_player.position, moveSpeed * _statusMoveSpeedMultiplier);
        }

        //  Attack  //
        private void UpdateAttack()
        {
            if (_player == null || attackSequence == null || attackSequence.Length == 0)
            { _state = State.Patrol; return; }

            _attackTimer -= Time.deltaTime;
            FaceTarget(_player.position);

            float dist = HorizontalDistance(_player.position);
            if (dist > _minAttackRangeFromAttacks * 1.4f)
            {
                _state = State.Chase;
                return;
            }

            if (_attackTimer <= 0f)
            {
                EnemyAttack atk = PickNextAttack(dist);
                if (atk == null) return;

                float cooldown = atk.attackCooldown > 0f ? atk.attackCooldown : attackCooldown;
                _attackTimer = cooldown;

                // Fire animator trigger (hashed at Awake  no per-frame StringToHash).
                TriggerAttackAnimation(atk);

                // Code-driven hitbox timing  no Animation Events required.
                HitBox box = GetZoneHitBox(atk.hitBoxZone);
                if (box == null && hitBoxZones != null && hitBoxZones.Length > 0)
                {
                    // Fall back to the first zone and warn so the mismatch is visible in the Console.
                    Debug.LogWarning(
                        $"[EnemyAI] '{name}': attack '{atk.name}' has hitBoxZone = \"{atk.hitBoxZone}\" but no zone with that name exists in Hit Box Zones. Falling back to first zone \"{hitBoxZones[0].zoneName}\". Fix: match the Zone Name in Hit Box Zones to the Hit Box Zone field on the EnemyAttack asset (case-sensitive).",
                        this);
                    box = hitBoxZones[0].hitBox;
                }
                if (box != null)
                    StartCoroutine(EnemyHitBoxRoutine(
                    box,
                    atk.damage * _outgoingDamageMultiplier,
                    atk.knockbackForce * _outgoingKnockbackMultiplier,
                    atk.hitDelay,
                    atk.hitDuration,
                    atk.attackRadius));
            }
        }

        public void SetStatusMoveSpeedMultiplier(float multiplier)
        {
            _statusMoveSpeedMultiplier = Mathf.Max(multiplier, 0f);
        }

        public void SetStatusActionLock(bool locked)
        {
            _statusActionLocked = locked;
        }

        public void SetOutgoingDamageMultiplier(float multiplier)
        {
            _outgoingDamageMultiplier = Mathf.Max(multiplier, 0f);
        }

        public void SetOutgoingKnockbackMultiplier(float multiplier)
        {
            _outgoingKnockbackMultiplier = Mathf.Max(multiplier, 0f);
        }

        public void SetPresentationCamera(Camera camera)
        {
            presentationCamera = camera;
            _cam = camera;
            billboardFacing?.SetCameraOverride(camera);
        }

        /// <summary>Returns the next EnemyAttack using pattern mode and optional priority profile scoring.</summary>
        private EnemyAttack PickNextAttack(float distToPlayer)
        {
            if (attackSequence == null || attackSequence.Length == 0) return null;

            _attackCandidates.Clear();
            foreach (var a in attackSequence)
            {
                if (a == null) continue;
                float effectiveRange = GetAttackEffectiveRange(a);
                if (!preferAttacksCurrentlyInRange || distToPlayer <= effectiveRange)
                    _attackCandidates.Add(a);
            }

            // If no attacks are currently in range, fall back to all defined attacks.
            if (_attackCandidates.Count == 0)
            {
                foreach (var a in attackSequence)
                    if (a != null) _attackCandidates.Add(a);
            }
            if (_attackCandidates.Count == 0) return null;

            if (!usePrioritySelection)
                return PickByPatternOnly(_attackCandidates);

            return PickByPriority(_attackCandidates, distToPlayer);
        }

        private EnemyAttack PickByPatternOnly(List<EnemyAttack> candidates)
        {
            if (candidates == null || candidates.Count == 0) return null;

            if (attackMode == AttackMode.Sequential)
            {
                int len = attackSequence.Length;
                for (int i = 0; i < len; i++)
                {
                    int idx = (_sequenceIndex + i) % len;
                    EnemyAttack atk = attackSequence[idx];
                    if (atk != null && candidates.Contains(atk))
                    {
                        _sequenceIndex = idx + 1;
                        return atk;
                    }
                }
                EnemyAttack fallback = candidates[0];
                _sequenceIndex++;
                return fallback;
            }

            float total = 0f;
            foreach (var a in candidates)
                total += Mathf.Max(a.weight, 0f);
            if (total <= 0f) return candidates[0];

            float roll = Random.Range(0f, total);
            float cumulative = 0f;
            foreach (var a in candidates)
            {
                cumulative += Mathf.Max(a.weight, 0f);
                if (roll <= cumulative) return a;
            }
            return candidates[candidates.Count - 1];
        }

        private EnemyAttack PickByPriority(List<EnemyAttack> candidates, float distToPlayer)
        {
            if (candidates == null || candidates.Count == 0) return null;

            if (attackMode == AttackMode.Random)
            {
                // Random mode still respects profile by biasing roll with profile score.
                float total = 0f;
                foreach (var a in candidates)
                    total += Mathf.Max(0.01f, EvaluateAttackScore(a, distToPlayer)) * Mathf.Max(a.weight, 0f);

                if (total <= 0f) return candidates[0];

                float roll = Random.Range(0f, total);
                float cumulative = 0f;
                foreach (var a in candidates)
                {
                    cumulative += Mathf.Max(0.01f, EvaluateAttackScore(a, distToPlayer)) * Mathf.Max(a.weight, 0f);
                    if (roll <= cumulative) return a;
                }
                return candidates[candidates.Count - 1];
            }

            // Sequential mode chooses the best-scoring candidate while preserving sequence order for ties.
            EnemyAttack best = null;
            float bestScore = float.MinValue;
            int bestIndex = int.MaxValue;

            for (int i = 0; i < attackSequence.Length; i++)
            {
                EnemyAttack atk = attackSequence[i];
                if (atk == null || !candidates.Contains(atk)) continue;

                float score = EvaluateAttackScore(atk, distToPlayer);
                if (score > bestScore)
                {
                    best = atk;
                    bestScore = score;
                    bestIndex = i;
                }
                else if (Mathf.Abs(score - bestScore) < 0.0001f && i < bestIndex)
                {
                    best = atk;
                    bestIndex = i;
                }
            }

            if (best != null)
            {
                _sequenceIndex = bestIndex + 1;
                return best;
            }

            return candidates[0];
        }

        private float EvaluateAttackScore(EnemyAttack atk, float distToPlayer)
        {
            float range = GetAttackEffectiveRange(atk);
            float damage = Mathf.Max(0f, atk.damage);
            float knockback = Mathf.Max(0f, atk.knockbackForce);
            float priority = Mathf.Max(0f, atk.aiPriority);

            switch (attackPriorityProfile)
            {
                case AttackPriorityProfile.LongestRange:
                    return range;
                case AttackPriorityProfile.HighestDamage:
                    return damage;
                case AttackPriorityProfile.HighestKnockback:
                    return knockback;
                case AttackPriorityProfile.HighestAssetPriority:
                    return priority;
                default:
                    // Weighted profile: include a closeness term so attacks near ideal distance are favored.
                    float distanceFit = range > 0f ? Mathf.Clamp01(1f - Mathf.Abs(distToPlayer - range) / range) : 0f;
                    return (range * rangeWeight)
                    + (damage * damageWeight)
                    + (knockback * knockbackWeight)
                    + (priority * assetPriorityWeight)
                    + distanceFit;
            }
        }

        private float GetAttackEffectiveRange(EnemyAttack atk)
        {
            if (atk == null) return _computedAttackRange;

            HitBox zone = GetZoneHitBox(atk.hitBoxZone);
            if (zone != null && zone.TryGetEnemyAttackRangeOverride(out float hitBoxRangeOverride))
                return hitBoxRangeOverride;

            if (atk.attackRange > 0f)
                return atk.attackRange;

            // attackRadius extends reach beyond the base measured range.
            return _computedAttackRange + Mathf.Max(0f, atk.attackRadius);
        }

        private float GetMinAttackRange()
        {
            if (attackSequence == null || attackSequence.Length == 0)
                return Mathf.Max(0.5f, _computedAttackRange);

            float minRange = float.MaxValue;
            bool found = false;

            foreach (var atk in attackSequence)
            {
                if (atk == null) continue;
                found = true;
                minRange = Mathf.Min(minRange, Mathf.Max(0.1f, GetAttackEffectiveRange(atk)));
            }

            return found ? minRange : Mathf.Max(0.5f, _computedAttackRange);
        }

        /// <summary>Looks up a HitBox from hitBoxZones by zone name. Returns null if not found.</summary>
        private HitBox GetZoneHitBox(string zoneName)
        {
            if (hitBoxZones == null || string.IsNullOrEmpty(zoneName)) return null;
            foreach (var slot in hitBoxZones)
                if (slot.zoneName == zoneName) return slot.hitBox;
            return null;
        }

        //  Movement helpers  //
        private void MoveToward(Vector3 worldTarget, float speed)
        {
            Vector3 dir = worldTarget - transform.position;
            dir.y = 0f;
            // In TwoD mode ignore Z depth  enemy moves left/right along X only.
            if (movementMode == MovementMode.TwoD) dir.z = 0f;

            Vector3 kb = _knockbackReceiver != null ? _knockbackReceiver.Velocity : Vector3.zero;

            if (dir.sqrMagnitude < 0.01f)
            {
                // At the target position  still need to apply gravity and knockback.
                _controller.Move(new Vector3(kb.x, _verticalVel + kb.y, kb.z) * Time.deltaTime);
                return;
            }

            dir.Normalize();
            Vector3 move = dir * speed;
            _controller.Move(new Vector3(move.x + kb.x, _verticalVel + kb.y, move.z + kb.z) * Time.deltaTime);

            FaceTarget(worldTarget);
        }

        private void FaceTarget(Vector3 worldTarget)
        {
            // Project the direction-to-target onto the camera's screen-right axis so
            // facing matches screen left/right regardless of world orientation.
            Vector3 toTarget = worldTarget - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0025f) return;  // ~0.05 threshold, same as before

            float dot;
            if (_cam != null)
            {
                Vector3 camRight = _cam.transform.right;
                camRight.y = 0f;
                dot = Vector3.Dot(toTarget, camRight);
            }
            else
            {
                dot = toTarget.x;  // camera-less fallback: world X
            }
            if (Mathf.Abs(dot) <= 0.05f) return;

            bool faceRight = dot > 0f;

            // Prefer flipping the visual root's X scale so child hitboxes mirror correctly.
            if (billboardFacing != null)
            {
                billboardFacing.ApplyFacing(faceRight);
            }
            else if (visualRoot != null)
            {
                // (faceRight == spriteDefaultFacesRight)  naturally oriented  +scale
                // If they differ the sprite must be mirrored  -scale
                Vector3 s = visualRoot.localScale;
                s.x = (faceRight == spriteDefaultFacesRight) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
                visualRoot.localScale = s;
            }
            else if (_spriteRenderer != null)
            {
                // spriteDefaultFacesRight=true  flipX only when facing LEFT
                _spriteRenderer.flipX = spriteDefaultFacesRight ? !faceRight : faceRight;
            }

            // Mirror all hitbox zones to the correct world side.
            if (hitBoxZones != null)
                foreach (var slot in hitBoxZones)
                    slot.MirrorToSide(transform, faceRight);
        }

        //  Range helpers  //
        /// <summary>
        /// Measures the realistic attack range from a HitBox's collider:
        /// the hitbox's X offset from the root plus its half-extent along X.
        /// This matches how far the collider actually reaches in the attack direction.
        /// </summary>
        private float MeasureHitBoxRange(HitBox box, float absOffsetX)
        {
            if (box == null) return 1.0f;
            var col = box.GetComponent<Collider>();
            if (col == null) return 1.0f;
            float halfExtent = col is BoxCollider bc
            ? bc.size.x * 0.5f * Mathf.Abs(box.transform.lossyScale.x)
            : col.bounds.extents.x;
            return absOffsetX + halfExtent;
        }

        //  Detection  //
        private bool CanSeePlayer()
        {
            if (_player == null) return false;

            float dist = HorizontalDistance(_player.position);
            if (dist > aggroRange) return false;

            if (!requireLineOfSight) return true;

            // Line-of-sight raycast
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 to = _player.position + Vector3.up * 0.5f;
            return !Physics.Linecast(origin, to, obstacleMask);
        }

        private float HorizontalDistance(Vector3 other)
        {
            Vector3 diff = other - transform.position;
            diff.y = 0f;
            // In TwoD mode the enemy only navigates along X, so ignore Z depth.
            if (movementMode == MovementMode.TwoD) diff.z = 0f;
            return diff.magnitude;
        }

        //  Animator  //
        private void UpdateAnimator()
        {
            if (_animator == null) return;

            // Derive IsMoving from intent so it flips instantly (not from stale velocity).
            bool isMoving = _state == State.Chase ||
            (_state == State.Patrol && _controller.velocity.sqrMagnitude > 0.05f);
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.Move, isMoving);
            _animationDriver?.SetBoolSignal(ActorAnimationSignal.Idle, !isMoving);
            _animator.SetBool(H_IsMoving, isMoving);
            _animator.SetBool(H_Grounded, _isGrounded);
        }

        //  Health callbacks  //
        private void OnDeath()
        {
            _state = State.Dead;
            _animationDriver?.TriggerSignal(ActorAnimationSignal.Death);
            _animator?.SetTrigger(H_Death);
            _controller.enabled = false;

            // Disable all hitboxes
            foreach (var hb in GetComponentsInChildren<HitBox>())
                hb.Disable();
        }

        private void OnHit(float damage)
        {
            _animationDriver?.TriggerSignal(ActorAnimationSignal.Hurt);
            _animator?.SetTrigger(H_Hit);

            // If hit while patrolling and the player is in leash range, start chasing
            if (_state == State.Patrol && _player != null &&
            HorizontalDistance(_player.position) < leashRange)
            {
                _state = State.Chase;
            }
        }

        private void OnDestroy()
        {
            if (_health == null) return;
            _health.OnDeath.RemoveListener(OnDeath);
            _health.OnDamaged.RemoveListener(OnHit);
        }

        private IEnumerator EnemyHitBoxRoutine(HitBox box, float damage, float knockback, float delay, float duration, float attackRadius = 0f)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            // Store original collider size for restoration
            Collider col = box.GetComponent<Collider>();
            Vector3 originalScale = col != null ? col.transform.localScale : Vector3.one;

            // Expand collider if attack radius is specified (> 0)
            if (attackRadius > 0f && col != null)
            {
                // Scale the collider based on attack radius
                float radiusScale = 1f + (attackRadius * 0.5f); // Modest expansion factor
                col.transform.localScale = originalScale * radiusScale;
            }

            box.Enable(damage, knockback);
            yield return new WaitForSeconds(Mathf.Max(duration, 0.01f));
            box.Disable();

            // Restore original collider scale
            if (col != null)
                col.transform.localScale = originalScale;
        }

        private void ApplyCombatProfile(EnemyCombatProfile profile)
        {
            if (profile == null)
                return;

            profile.Sanitize();
            attackSequence = profile.attackSequence;
            attackMode = profile.attackMode;
            usePrioritySelection = profile.usePrioritySelection;
            preferAttacksCurrentlyInRange = profile.preferAttacksCurrentlyInRange;
            attackCooldown = profile.attackCooldown;
            attackRangeOverride = profile.attackRangeOverride;
            rangeWeight = profile.rangeWeight;
            damageWeight = profile.damageWeight;
            knockbackWeight = profile.knockbackWeight;
            assetPriorityWeight = profile.assetPriorityWeight;
        }

        private void ApplyFeatureProfile(EnemyFeatureProfile profile)
        {
            if (profile == null)
                return;

            combatProfile = profile.combatProfile != null ? profile.combatProfile : combatProfile;
            reactionProfile = profile.reactionProfile != null ? profile.reactionProfile : reactionProfile;
        }

        private void InitializeFeatureModules()
        {
            FeatureModuleDefinition[] definitions = enemyFeatureProfile != null ? enemyFeatureProfile.featureModules : null;
            if (definitions == null || definitions.Length == 0)
                return;

            _featureHost ??= gameObject.AddComponent<ActorFeatureHost>();
            _featureHost.InitializeFeatures(BuildFeatureContext(), definitions);
            _featureHost.TryGetInstalledFeature(out _reactionState);
        }

        private ActorFeatureContext BuildFeatureContext()
        {
            return new ActorFeatureContext(
            gameObject,
            health: _health,
            animation: _animationDriver,
            knockback: _knockbackReceiver,
            enemyActorState: this,
            presentationMode: _animationDriver != null ? _animationDriver.PresentationMode : ActorPresentationMode.Billboard2_5D,
            authoredProfiles: new ScriptableObject[]
            {
  enemyFeatureProfile,
  combatProfile,
  reactionProfile
            });
        }

        private void TriggerAttackAnimation(EnemyAttack attack)
        {
            if (attack == null)
                return;

            if (_animationDriver != null)
            {
                int step = Mathf.Max(attack.animationStep, 1);
                if (attack.useCustomAnimationKey && !string.IsNullOrWhiteSpace(attack.customAnimationKey))
                    _animationDriver.TriggerCustom(attack.customAnimationKey, intValue: step);
                else
                {
                    _animationDriver.SetIntSignal(attack.animationSignal, step);
                    _animationDriver.TriggerSignal(attack.animationSignal, intValue: step);
                }
            }

            if (!string.IsNullOrEmpty(attack.animatorTrigger)
            && _attackTriggerHashes.TryGetValue(attack, out int hash))
                _animator?.SetTrigger(hash);
        }

#if UNITY_EDITOR
        //  Gizmos  //
        private void OnDrawGizmosSelected()
        {
            // Aggro range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aggroRange);

            // Leash range
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, leashRange);

            // Attack range (live value at runtime; editor-time fallback to override or 1)
            float atkRange = Application.isPlaying
            ? _minAttackRangeFromAttacks
            : (attackRangeOverride > 0f ? attackRangeOverride : 1.0f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }
#endif
    }
}
