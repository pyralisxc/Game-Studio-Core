using UnityEngine;
using UnityEngine.Events;
using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Combat
{
/// <summary>
/// Universal health component - attach to any GameObject that can be damaged:
/// the player, enemies, destructible props, etc.
///
/// Setup:
///   - Add this component to the root of your character/object.
///   - Set Max Health in the Inspector.
///   - Set Faction to prevent friendly fire (Player, Enemy, Neutral).
///   - Wire the UnityEvents (OnDamaged, OnHealed, OnDeath) in the Inspector
///     OR subscribe to them in code from other scripts.
///
/// Dealing damage from other scripts:
///   target.GetComponent<HealthComponent>().TakeDamage(10f, hitPoint, attacker);
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Combat, 
    Priority = 10,
    Relevance = "Inspector Add Component path for any damageable player, enemy, prop, or custom object.",
    AssignmentFields = new[] { nameof(maxHealth), nameof(faction), nameof(iFrameDuration) },
    FirstProof = "Enter Play Mode and trigger TakeDamage via a custom script or trigger. Verify the CurrentHealth decreases in the Inspector and the OnDamaged UnityEvent fires.",
    NativeSetup = new[] { "Add Component", "Wire UnityEvents for feedback" }
,
        ExpertAdvice = "The HealthComponent is a neutral actor. Use the Faction property to group players and enemies. It dispatches UnityEvents for visual feedback (HitFlash, UI).",
        DocumentationURL = "https://docs.neonblack.com/pyralis/health")]
[AddComponentMenu("NeonBlack/Gameplay/Combat/Health Component")]
public class HealthComponent : MonoBehaviour, IActorHealthModifierReceiver, IActorHealthState
{
    // Inspector
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private bool  destroyOnDeath = false;
    [Tooltip("How long to wait after death before destroying the GameObject (if destroyOnDeath is true).")]
    [SerializeField] private float deathDestroyDelay = 2f;

    [Header("Faction  (prevents friendly fire)")]
    [Tooltip("Objects with the same faction will not damage each other.")]
    public Faction faction = Faction.Neutral;

    [Header("Invincibility Frames")]
    [Tooltip("Seconds after being hit during which no further damage is taken. 0 = no iframes.")]
    [SerializeField] private float iFrameDuration = 0.15f;

    [Header("Health Recovery")]
    [Tooltip("Enable automatic health regeneration over time.")]
    [SerializeField] private bool  regenEnabled  = false;
    [Tooltip("HP restored per second while regenerating.")]
    [SerializeField] private float regenRate      = 5f;
    [Tooltip("Seconds after taking damage before regeneration resumes. 0 = regen is never interrupted by damage.")]
    [SerializeField] private float regenDelay     = 3f;
    [Tooltip("Regeneration stops once HP reaches this fraction of max health (1 = full, 0.5 = half, etc.).")]
    [Range(0f, 1f)]
    [SerializeField] private float regenCapFrac   = 1f;

    [Header("Events")]
    [Tooltip("Fired whenever damage is taken. Parameter = damage amount.")]
    public UnityEvent<float> OnDamaged;
    [Tooltip("Fired whenever health is restored. Parameter = amount healed.")]
    public UnityEvent<float> OnHealed;
    [Tooltip("Fired once when health reaches zero.")]
    public UnityEvent OnDeath;

    // Public State (read-only from outside)
    public float CurrentHealth { get; private set; }
    public float MaxHealth     => maxHealth;
    public bool  IsDead        { get; private set; }
    public float HealthPercent => maxHealth > 0f ? CurrentHealth / maxHealth : 0f;
    public Faction Faction => faction;

    public event System.Action<float> Damaged;
    public event System.Action<float> Healed;
    public event System.Action Died;

    // Private
    private float _iFrameTimer;
    private float _regenTimer;   // counts down to zero before regen ticks
    private float _incomingDamageMultiplier = 1f;
    private float _regenRateMultiplier = 1f;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Update()
    {
        if (_iFrameTimer > 0f)
            _iFrameTimer -= Time.deltaTime;

        // Regeneration
        if (regenEnabled && !IsDead && CurrentHealth < maxHealth * regenCapFrac)
        {
            if (_regenTimer > 0f)
                _regenTimer -= Time.deltaTime;
            else
                Heal(regenRate * _regenRateMultiplier * Time.deltaTime);
        }
    }

    // Public API

    /// <summary>
    /// Apply damage from an external source.
    /// </summary>
    /// <param name="amount">Raw damage amount (positive number).</param>
    /// <param name="hitPoint">World-space point where the hit landed (used for knockback/VFX).</param>
    /// <param name="source">The GameObject that dealt the damage (used for faction check).</param>
    public void TakeDamage(float amount, Vector3 hitPoint, GameObject source = null)
    {
        TryTakeDamage(amount, hitPoint, source);
    }

    public bool TryTakeDamage(float amount, Vector3 hitPoint, GameObject source = null)
    {
        if (IsDead)                  return false;
        if (_iFrameTimer > 0f)       return false;
        if (amount <= 0f)            return false;

        // Faction check - no friendly fire
        if (source != null)
        {
            var sourceHealth = source.GetComponentInParent<HealthComponent>();
            if (sourceHealth != null && sourceHealth.faction == faction && faction != Faction.Neutral)
                return false;
        }

        // Allow any attached modifier (block/parry/shield/etc.) to adjust incoming damage.
        var damageModifiers = GetComponentsInChildren<IDamageModifier>(true);
        if (damageModifiers != null && source != null)
        {
            float modifiedDamage = amount;
            bool modified = false;
            for (int i = 0; i < damageModifiers.Length; i++)
            {
                if (damageModifiers[i] == null)
                    continue;

                if (damageModifiers[i].TryModifyIncomingDamage(source, ref modifiedDamage))
                    modified = true;
            }

            if (modified)
            {
                amount = modifiedDamage;
                if (amount <= 0f) return false; // full negation - eat the hit entirely
            }
        }

        amount *= Mathf.Max(_incomingDamageMultiplier, 0f);
        if (amount <= 0f) return false;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);
        _iFrameTimer  = iFrameDuration;
        _regenTimer   = regenDelay;   // restart the regen countdown

        OnDamaged?.Invoke(amount);
        Damaged?.Invoke(amount);

        if (CurrentHealth <= 0f)
            Die();

        return true;
    }

    /// <summary>Restore health, clamped to maxHealth.</summary>
    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;

        float before = CurrentHealth;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        float actual = CurrentHealth - before;

        if (actual > 0f)
        {
            OnHealed?.Invoke(actual);
            Healed?.Invoke(actual);
        }
    }

    /// <summary>Instantly set health to max (e.g. respawn).</summary>
    public void FullHeal()
    {
        float healed  = maxHealth - CurrentHealth;
        IsDead        = false;
        CurrentHealth = maxHealth;
        if (healed > 0f)
            OnHealed?.Invoke(healed);
    }

    /// <summary>
    /// Silently set current health to an exact value without firing events or
    /// triggering i-frames. Use for initialisation, respawn tuning, or save/load.
    /// Clamps to [0, MaxHealth]. Does NOT revive a dead character.
    /// </summary>
    public void SetCurrentHealth(float amount)
    {
        CurrentHealth = Mathf.Clamp(amount, 0f, maxHealth);
    }

    /// <summary>
    /// Grant immunity to damage for <paramref name="duration"/> seconds
    /// (extends the internal i-frame timer). Useful for respawn shields.
    /// </summary>
    public void ForceIFrames(float duration)
    {
        _iFrameTimer = Mathf.Max(_iFrameTimer, duration);
    }

    /// <summary>Kill this object immediately regardless of current health.</summary>
    public void ForceKill()
    {
        if (IsDead) return;
        CurrentHealth = 0f;
        Die();
    }

    /// <summary>
    /// Changes the maximum health. Optionally scales current health proportionally
    /// so the HP bar percentage is preserved.
    /// </summary>
    public void SetMaxHealth(float newMax, bool scaleCurrentHealth = false)
    {
        if (newMax <= 0f) return;
        if (scaleCurrentHealth && maxHealth > 0f)
            CurrentHealth = CurrentHealth / maxHealth * newMax;
        maxHealth     = newMax;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
    }

    public void SetIncomingDamageMultiplier(float multiplier)
    {
        _incomingDamageMultiplier = Mathf.Max(multiplier, 0f);
    }

    public void SetRegenRateMultiplier(float multiplier)
    {
        _regenRateMultiplier = Mathf.Max(multiplier, 0f);
    }

    // Internal
    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        OnDeath?.Invoke();
        Died?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject, deathDestroyDelay);
    }
}
}
