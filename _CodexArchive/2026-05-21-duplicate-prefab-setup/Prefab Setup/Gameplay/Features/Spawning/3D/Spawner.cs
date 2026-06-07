using UnityEngine;

namespace NeonBlack.Gameplay.Features.Spawning
{
/// <summary>
/// General-purpose prefab + sprite spawner with optional patrol movement.
///
/// Setup:
///   â€¢ Add this component to any empty GameObject.
///   â€¢ Drag prefabs into Prefabs OR drag sprite slices into Sprites.
///   â€¢ Enable Patrol to have the spawner move left and right automatically.
/// </summary>
public class Spawner : MonoBehaviour
{
    // â”€â”€ Prefabs â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Prefabs  (drag full prefabs here)")]
    [Tooltip("Drag any prefabs or sprite GameObjects here.")]
    [SerializeField] private GameObject[] prefabs;

    // â”€â”€ Sprites â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Sprites  (drag sprite slices here)")]
    [Tooltip("Drag individual sprite slices from your sprite sheet here. " +
             "Each one will be wrapped in a new SpriteRenderer GameObject when spawned.")]
    [SerializeField] private Sprite[] sprites;

    [Tooltip("Sorting layer name for sprite-spawned objects.")]
    [SerializeField] private string spriteSortingLayer = "Default";

    [Tooltip("Sorting order for sprite-spawned objects.")]
    [SerializeField] private int spriteSortingOrder = 0;

    [Tooltip("Scale applied to sprite-spawned objects.")]
    [SerializeField] private float spriteScale = 1f;

    [Tooltip("Automatically add a Rigidbody, BoxCollider, and FallingItem to sprite-spawned " +
             "objects. Enable this for the main-menu catch mini-game.")]
    [SerializeField] private bool addPhysicsToSprites = false;


    // â”€â”€ Spawn Mode â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Spawn Mode")]
    [Tooltip("How to pick which prefab/sprite to spawn.")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.Random;

    public enum SpawnMode { Random, Sequential }

    // â”€â”€ Spawn Location â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Spawn Location")]
    [Tooltip("Random offset radius around the spawn point (0 = exact position).")]
    [SerializeField] private float spawnRadius = 0f;

    // â”€â”€ Patrol Movement â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Patrol Movement")]
    [Tooltip("Move the spawner back and forth while the game runs.")]
    [SerializeField] private bool patrol = false;

    [Tooltip("Patrol axis: X = left/right, Z = forward/back, Y = up/down.")]
    [SerializeField] private PatrolAxis patrolAxis = PatrolAxis.X;

    public enum PatrolAxis { X, Z, Y }

    [Tooltip("Total distance in each direction from the start position.")]
    [SerializeField] private float patrolDistance = 5f;

    [Tooltip("Movement speed of the spawner.")]
    [SerializeField] private float patrolSpeed = 3f;

    // â”€â”€ Timing â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Timing")]
    [Tooltip("Seconds between each spawn. 0 = only spawn manually via SpawnOne().")]
    [SerializeField] private float spawnInterval = 2f;

    [Tooltip("Spawn automatically when the scene starts.")]
    [SerializeField] private bool autoSpawn = true;

    // â”€â”€ Limits â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Limits")]
    [Tooltip("Maximum number of spawned objects alive at once. 0 = unlimited.")]
    [SerializeField] private int maxAlive = 0;

    [Tooltip("Total spawns before this spawner stops. 0 = unlimited.")]
    [SerializeField] private int maxTotal = 0;

    // â”€â”€ Parent â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    [Header("Hierarchy")]
    [Tooltip("Optional parent for spawned objects (keeps hierarchy tidy). Leave empty for scene root.")]
    [SerializeField] private Transform spawnParent;

    // â”€â”€ Private State â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    private float   _timer;
    private int     _sequentialIndex;
    private int     _totalSpawned;
    private int     _aliveCount;
    private Vector3 _startPos;
    private float   _patrolT;        // 0â†’1 ping-pong value
    private bool    _patrolForward = true;

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //

    private void Start()
    {
        _startPos = transform.position;

        if (autoSpawn && spawnInterval > 0f)
            _timer = spawnInterval;
    }

    private void Update()
    {
        if (patrol) HandlePatrol();

        if (!autoSpawn || spawnInterval <= 0f) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            SpawnOne();
            _timer = spawnInterval;
        }
    }

    private void HandlePatrol()
    {
        float step = patrolSpeed * Time.deltaTime / patrolDistance;
        _patrolT += _patrolForward ? step : -step;

        if (_patrolT >= 1f) { _patrolT = 1f; _patrolForward = false; }
        if (_patrolT <= 0f) { _patrolT = 0f; _patrolForward = true;  }

        Vector3 dir = patrolAxis == PatrolAxis.X ? Vector3.right
                    : patrolAxis == PatrolAxis.Z ? Vector3.forward
                    : Vector3.up;

        transform.position = _startPos + dir * ((_patrolT * 2f - 1f) * patrolDistance);
    }

    /// <summary>
    /// Spawns a single object. Call this from a button, event, or other script.
    /// </summary>
    public void SpawnOne()
    {
        // Respect limits
        if (maxTotal > 0 && _totalSpawned >= maxTotal) return;
        if (maxAlive > 0 && _aliveCount   >= maxAlive) return;

        Vector3 pos = transform.position;
        if (spawnRadius > 0f)
        {
            Vector2 rand = Random.insideUnitCircle * spawnRadius;
            pos += new Vector3(rand.x, 0f, rand.y);
        }

        int totalOptions = (prefabs?.Length ?? 0) + (sprites?.Length ?? 0);
        if (totalOptions == 0)
        {
            Debug.LogWarning("[Spawner] No prefabs or sprites assigned!", this);
            return;
        }

        GameObject spawned = null;

        // Decide whether to pick from prefabs or sprites
        int prefabCount  = prefabs?.Length ?? 0;
        int spriteCount  = sprites?.Length ?? 0;
        int pickedIndex  = spawnMode == SpawnMode.Random
                         ? Random.Range(0, totalOptions)
                         : (_sequentialIndex++ % totalOptions);

        if (pickedIndex < prefabCount)
        {
            // Prefab path
            spawned = Instantiate(prefabs[pickedIndex], pos, transform.rotation, spawnParent);
        }
        else
        {
            // Sprite slice path â€” build a new GameObject with a SpriteRenderer
            int si = pickedIndex - prefabCount;
            spawned = new GameObject(sprites[si].name);
            spawned.transform.SetParent(spawnParent);
            spawned.transform.position = pos;
            spawned.transform.localScale = Vector3.one * spriteScale;

            var sr = spawned.AddComponent<SpriteRenderer>();
            sr.sprite           = sprites[si];
            sr.sortingLayerName = spriteSortingLayer;
            sr.sortingOrder     = spriteSortingOrder;

            if (addPhysicsToSprites)
            {
                var col = spawned.AddComponent<BoxCollider>();
                // Size the collider to the sprite's bounds
                col.size = new Vector3(
                    sprites[si].bounds.size.x * spriteScale,
                    sprites[si].bounds.size.y * spriteScale,
                    0.2f);

                var rb = spawned.AddComponent<Rigidbody>();
                rb.useGravity        = true;
                rb.constraints       = RigidbodyConstraints.FreezePositionZ
                                     | RigidbodyConstraints.FreezeRotation;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        _aliveCount++;
        _totalSpawned++;
        var tracker = spawned.AddComponent<SpawnTracker>();
        tracker.OnDestroyed = () => _aliveCount--;
    }

    // Draws the patrol path and spawn radius in the Scene view.
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? _startPos : transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(spawnRadius, 0.2f));

        if (patrol)
        {
            Vector3 dir = patrolAxis == PatrolAxis.X ? Vector3.right
                        : patrolAxis == PatrolAxis.Z ? Vector3.forward
                        : Vector3.up;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin - dir * patrolDistance, origin + dir * patrolDistance);
            Gizmos.DrawWireSphere(origin - dir * patrolDistance, 0.2f);
            Gizmos.DrawWireSphere(origin + dir * patrolDistance, 0.2f);
        }
    }
}

/// <summary>
/// Lightweight component automatically added to every spawned object so the
/// Spawner can track how many are still alive without needing a list.
/// </summary>
public class SpawnTracker : MonoBehaviour
{
    public System.Action OnDestroyed;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}
}
