using System.Collections.Generic;
using System;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
/// <summary>
/// 2D version of HitBox Ã¢â‚¬â€ attach to a child with a Trigger Collider2D.
/// Works with TilemapCollider2D scenes where characters use Rigidbody2D.
///
/// Setup:
///   1. Add a child GameObject (e.g. "HitBox2D_Fist").
///   2. Add a BoxCollider2D Ã¢â‚¬â€ check "Is Trigger".
///   3. Add this component.
///   4. Set Owner to the root character GameObject.
///   5. Optionally assign a WeaponData asset.
///   6. Call Enable() / Disable() from PlayerActions2D or via Coroutine.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HitBox2D : MonoBehaviour
{
    public event Action<GameObject> HitConfirmed;

    [Header("Owner")]
    [Tooltip("Root GameObject of the attacker Ã¢â‚¬â€ used for faction check and knockback direction.")]
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

    private Collider2D   _col;
    private AudioSource  _audio;
    private readonly HashSet<GameObject> _hitIds = new HashSet<GameObject>();

    private void Awake()
    {
        _col           = GetComponent<Collider2D>();
        _col.isTrigger = true;
        _audio         = GetComponent<AudioSource>();
        Disable();
    }

    /// <summary>Activate the hitbox for one swing Ã¢â‚¬â€ clears the already-hit set.</summary>
    public void Enable()
    {
        _hitIds.Clear();
        _col.enabled = true;
    }

    /// <summary>Deactivate the hitbox at the end of a swing.</summary>
    public void Disable()
    {
        _col.enabled = false;
        _hitIds.Clear();
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Animation Event relay Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬ //
    public void EnableHitBox()  => Enable();
    public void DisableHitBox() => Disable();

    public void ConfigureDamage(float damage, float knockback)
    {
        baseDamage = damage;
        knockbackForce = knockback;
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Physics Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬ //
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_col.enabled) return;

        if (_hitIds.Contains(other.gameObject)) return;

        HealthComponent hp = other.GetComponentInParent<HealthComponent>();
        if (hp == null) return;

        // Faction check Ã¢â‚¬â€ no friendly fire.
        // Neutral is treated as "unassigned" so it never blocks hits.
        HealthComponent ownerHp = owner != null
            ? owner.GetComponentInParent<HealthComponent>()
            : GetComponentInParent<HealthComponent>();
        if (ownerHp != null
            && ownerHp.faction != Faction.Neutral
            && hp.faction    != Faction.Neutral
            && hp.faction    == ownerHp.faction) return;

        _hitIds.Add(other.gameObject);

        float dmg = weapon != null ? weapon.damage        : baseDamage;
        float kb  = weapon != null ? weapon.knockbackForce : knockbackForce;

        hp.TakeDamage(dmg, other.bounds.center, owner);

        // Freeze frame hit pause
        if (freezeFrameDuration > 0f && TimeManager.Instance != null)
            TimeManager.Instance.Freeze(freezeFrameDuration);

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
}
}
