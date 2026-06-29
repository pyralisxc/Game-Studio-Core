using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions.Combat;
using System;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Modules.Combat
{
/// <summary>
/// 2D version of HitBox - attach to a child with a Trigger Collider2D.
/// Works with TilemapCollider2D scenes where characters use Rigidbody2D.
///
/// Setup:
///   1. Add a child GameObject (e.g. "HitBox2D_Fist").
///   2. Add a BoxCollider2D - check "Is Trigger".
///   3. Add this component.
///   4. Set Owner to the root character GameObject.
///   5. Optionally assign a WeaponData asset.
///   6. Call Fire() from combat logic to arm the active hit window.
/// </summary>
[AuthoringContract(
        StableId = "combat.hitbox.2d",
        Category = "Combat Sensors",
        CapabilityPath = "Combat/Sensors/Hit Box2D",
        Surface = AuthoringSurface.Goal,
        Summary = "Trigger-based 2D hitbox for melee attacks in Tilemap or 2D physics scenes.",
        RequiredFields = new[] { nameof(owner), nameof(weapon), nameof(hitFXPrefab), nameof(hitSFX), nameof(hitPauseSink) },
        SetupSteps = new[] { "Add to a child GameObject of a 2D actor.", "Assign a Trigger Collider2D." },
        SuccessChecks = new[] { "The 2D hitbox damages a valid target during its active window." },
        Tags = new[] { "capability:CombatSensors", "axiom:Realtime", "axiom:Dimensions2D" }
    )]
[RequireComponent(typeof(Collider2D))]
public class HitBox2D : GameplayTickBehaviour
{
    public event Action<GameObject> HitConfirmed;

    [Header("Owner")]
    [Tooltip("Root GameObject of the attacker - used for faction check and knockback direction.")]
    [SerializeField] private GameObject owner;

    [Header("Damage  (overridden by Weapon if assigned)")]
    [SerializeField] private float baseDamage     = 15f;
    [SerializeField] private float knockbackForce = 8f;

    [Header("Weapon  (optional)")]
    [SerializeField] private WeaponData weapon;

    [Header("Hit FX")]
    [SerializeField] private GameObject hitFXPrefab;
    [SerializeField] private AudioClip  hitSFX;

    [Header("Hit Pause")]
    [SerializeField] private float freezeFrameDuration = 0.05f;
    [SerializeField] private MonoBehaviour hitPauseSink;

    private Collider2D   _col;
    private AudioSource  _audio;
    private IHitPauseSink _hitPauseSink;
    private readonly HashSet<GameObject> _hitIds = new HashSet<GameObject>();
    private bool _isFiring;
    private float _fireRemaining;
    private float _nextRepeatTime;
    private float _fireElapsed;
    private float _repeatRate;

    protected override GameplayTickDomain TickDomain => GameplayTickDomain.Combat;
    protected override bool UsesGameplayTick => true;

    private void Awake()
    {
        _col           = GetComponent<Collider2D>();
        _col.isTrigger = true;
        _audio         = GetComponent<AudioSource>();
        owner        ??= GetComponentInParent<HealthComponent>()?.gameObject;
        _hitPauseSink = ResolveHitPauseSink();
        _col.enabled = false;
    }

    /// <summary>
    /// Fires the hitbox for a specified duration.
    /// If repeatRate > 0, the hit list is cleared periodically allowing multiple hits on the same target.
    /// </summary>
    public void Fire(float duration = 0.1f, float repeatRate = 0f)
    {
        _hitIds.Clear();
        _col.enabled = true;
        _isFiring = true;
        _fireRemaining = Mathf.Max(0f, duration);
        _fireElapsed = 0f;
        _repeatRate = Mathf.Max(0f, repeatRate);
        _nextRepeatTime = _repeatRate > 0f ? _repeatRate : float.MaxValue;

        if (_fireRemaining <= 0f)
            EndFireWindow();
    }

    protected override void OnGameplayTick(in GameplayTickContext context)
    {
        if (!_isFiring)
            return;

        _fireElapsed += context.DeltaTime;
        _fireRemaining -= context.DeltaTime;

        if (_fireElapsed >= _nextRepeatTime)
        {
            _hitIds.Clear();
            _nextRepeatTime += _repeatRate;
        }

        if (_fireRemaining <= 0f)
            EndFireWindow();
    }

    private void EndFireWindow()
    {
        _col.enabled = false;
        _hitIds.Clear();
        _isFiring = false;
        _fireRemaining = 0f;
        _fireElapsed = 0f;
        _nextRepeatTime = float.MaxValue;
        _repeatRate = 0f;
    }

    public void ConfigureDamage(float damage, float knockback)
    {
        baseDamage = damage;
        knockbackForce = knockback;
    }

    public void SetHitPauseSink(IHitPauseSink sink)
    {
        _hitPauseSink = sink;
        hitPauseSink = sink as MonoBehaviour;
    }

    // Physics.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_col.enabled) return;

        HealthComponent hp = other.GetComponentInParent<HealthComponent>();
        if (hp == null) return;
        if (IsOwnerHealth(hp)) return;
        if (_hitIds.Contains(hp.gameObject)) return;

        // Faction check - no friendly fire.
        // Neutral is treated as "unassigned" so it never blocks hits.
        HealthComponent ownerHp = owner != null
            ? owner.GetComponentInParent<HealthComponent>()
            : GetComponentInParent<HealthComponent>();
        if (ownerHp != null
            && ownerHp.faction != Faction.Neutral
            && hp.faction    != Faction.Neutral
            && hp.faction    == ownerHp.faction) return;

        float dmg = weapon != null ? weapon.damage        : baseDamage;
        float kb  = weapon != null ? weapon.knockbackForce : knockbackForce;

        if (!hp.TryTakeDamage(dmg, other.bounds.center, owner))
            return;

        _hitIds.Add(hp.gameObject);

        // Freeze frame hit pause
        if (freezeFrameDuration > 0f)
            ResolveHitPauseSink()?.Freeze(freezeFrameDuration);

        // Apply 2D knockback directly to the Rigidbody2D
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null && kb > 0f)
        {
            Vector2 dir = ((Vector2)other.bounds.center - (Vector2)transform.position).normalized;
            if (dir == Vector2.zero) dir = Vector2.right;
            rb.AddForce(dir * kb, ForceMode2D.Impulse);
        }

        if (hitFXPrefab != null)
            Instantiate(hitFXPrefab, (Vector3)other.bounds.center, Quaternion.identity);

        if (hitSFX != null)
        {
            if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
            _audio.PlayOneShot(hitSFX);
        }

        HitConfirmed?.Invoke(hp.gameObject);
    }

    private bool IsOwnerHealth(HealthComponent health)
    {
        if (health == null || owner == null)
            return false;

        return health.gameObject == owner
            || health.transform.IsChildOf(owner.transform)
            || owner.transform.IsChildOf(health.transform);
    }

    private IHitPauseSink ResolveHitPauseSink()
    {
        if (_hitPauseSink != null)
            return _hitPauseSink;

        _hitPauseSink = hitPauseSink as IHitPauseSink;
        return _hitPauseSink;
    }
}
}
