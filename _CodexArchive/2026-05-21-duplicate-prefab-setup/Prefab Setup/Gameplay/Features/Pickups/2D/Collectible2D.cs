using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using VContainer;

namespace NeonBlack.Gameplay.Features.Pickups
{

/// <summary>
/// Canonical 2D collectible for NeonBlack Gameplay.
/// Setup: Create a prefab with this component, a SpriteRenderer, and a CircleCollider2D (Is Trigger).
/// The CollectibleSpawner2D manages pooling.
/// NOTE: CircleCollider2D is used instead of PolygonCollider2D \u2014 collectibles only need
/// overlap detection, and Circle is an order of magnitude cheaper in Physics2D.
/// </summary>
[AddComponentMenu("NeonBlack/Gameplay/Pickups/Collectible 2D")]
[RequireComponent(typeof(CircleCollider2D))]
public class Collectible2D : MonoBehaviour, IPickupCollectible
{
    public int FeedbackScoreValue => 1;
    [Header("Idle Animation")]
    [SerializeField, Tooltip("Speed of the idle bob animation.")]
    private float _bobSpeed = 2f;
    [SerializeField, Tooltip("Peak vertical offset of the idle bob animation in world units.")]
    private float _bobHeight = 0.05f;

    [Header("Spawn Immunity")]
    [SerializeField, Tooltip("Seconds after spawning during which this collectible cannot be destroyed by a hazard slam.\nPrevents a spawning hazard from immediately sweeping its own collectibles on the next slam cycle.")]
    private float _spawnImmunityDuration = 1.5f;

    [Header("Deferred Adapters")]
    [SerializeField, Tooltip("Optional award sink override. When empty, the collectible resolves an IPickupAwardSink from parents or the scene.")]
    private MonoBehaviour _awardSinkSource;

    private Vector3 _originPos;
    private bool  _alive;
    private float _localTime;   // independent per collectible so they don't all bob in sync
    private float _spawnTime;   // Time.time when this collectible was last activated
    private IPickupAwardSink _awardSink;
    private CollectibleSpawner2D _spawner;

    private void OnEnable()
    {
        _originPos = transform.position;
        _alive     = true;
        _spawnTime = Time.time;
        _localTime = Random.Range(0f, Mathf.PI * 2f); // random phase offset so collectibles don't look identical
        _awardSink ??= ResolveAwardSink();
        _spawner ??= GetComponentInParent<CollectibleSpawner2D>();
    }

    [Inject]
    private void Construct(IPickupAwardSink awardSink = null)
    {
        _awardSink = awardSink;
    }

    /// <summary>
    /// Called by CollectibleSpawner2D.Update() instead of Unity's MonoBehaviour.Update().
    /// Centralizing all collectible ticks in one MonoBehaviour eliminates the native\u2192managed
    /// bridge overhead of N individual Update() calls, which becomes significant at 200\u20131000 collectibles.
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (!_alive) return;
        _localTime += deltaTime;
        float yOffset = Mathf.Sin(_localTime * _bobSpeed) * _bobHeight;
        transform.position = new Vector3(_originPos.x, _originPos.y + yOffset, _originPos.z);
    }

    /// <summary>Player collected this collectible. Awards a point and returns it to the pool.</summary>
    public void Collect()
    {
        Collect(null);
    }

    public void CollectBy(GameObject collector)
    {
        Collect(collector);
    }

    public void Collect(GameObject collector)
    {
        if (!_alive) return;
        _alive = false;
        _awardSink ??= ResolveAwardSink();
        _awardSink?.ApplyAward(new PickupAwardPayload(collector, transform.position, FeedbackScoreValue, PickupAwardOutcome.Collected));
        ReturnToPool();
    }

    /// <summary>Hazard destroyed this collectible. No points awarded.</summary>
    /// <returns>False if the collectible is still in its spawn-immunity window and was NOT destroyed.</returns>
    public bool DestroyByHazard()
    {
        return RemoveFromPlay();
    }

    public bool RemoveFromPlay()
    {
        if (!_alive) return false;
        // Spawn immunity: collectibles cannot be hazard-destroyed for a brief window after
        // appearing. This prevents a spawning hazard from immediately sweeping its own
        // collectibles on the very next slam cycle.
        if (Time.time - _spawnTime < _spawnImmunityDuration) return false;
        _alive = false;
        _awardSink ??= ResolveAwardSink();
        _awardSink?.ApplyAward(new PickupAwardPayload(null, transform.position, 0, PickupAwardOutcome.DestroyedWithoutAward));
        ReturnToPool();
        return true;
    }

    public bool RemoveWithoutScore()
    {
        return RemoveFromPlay();
    }

    private void ReturnToPool()
    {
        // CollectibleSpawner2D.ReturnCollectible handles SetActive(false) \u2014 don't double-disable here
        _spawner ??= GetComponentInParent<CollectibleSpawner2D>();
        if (_spawner != null)
            _spawner.ReturnCollectible(this);
        else
            gameObject.SetActive(false);
    }

    private IPickupAwardSink ResolveAwardSink()
    {
        if (_awardSinkSource is IPickupAwardSink configuredSink)
            return configuredSink;

        IPickupAwardSink parentSink = GetComponentInParent<IPickupAwardSink>();
        if (parentSink != null)
            return parentSink;

        return Object.FindAnyObjectByType<CollectibleFeedback2D>();
    }
}

} // namespace NeonBlack.Gameplay.Features.Pickups
