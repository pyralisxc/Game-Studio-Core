using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
/// <summary>
/// Singleton that pools and spawns floating damage numbers.
/// No prefab required Ã¢â‚¬â€ numbers are built entirely at runtime.
/// Place one instance anywhere in your game scene.
///
/// Usage from any script:
///   DamageNumberSpawner.Instance.Spawn(25f, hitPoint);
///   DamageNumberSpawner.Instance.Spawn(50f, hitPoint, isCritical: true);
///   DamageNumberSpawner.Instance.SpawnHeal(30f, position);
/// </summary>
public class DamageNumberSpawner : MonoBehaviour
{
    public static DamageNumberSpawner Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Instance = null;

    [Header("Pool")]
    [SerializeField] private int initialPoolSize = 20;

    private readonly Queue<DamageNumber> _pool = new Queue<DamageNumber>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < initialPoolSize; i++)
            _pool.Enqueue(CreateNew());
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Public API Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬ //

    public void Spawn(float amount, Vector3 worldPos, bool isCritical = false)
    {
        GetFromPool().Play(amount, worldPos, isCritical: isCritical, isHeal: false);
    }

    public void SpawnHeal(float amount, Vector3 worldPos)
    {
        GetFromPool().Play(amount, worldPos, isCritical: false, isHeal: true);
    }

    // Ã¢â€â‚¬Ã¢â€â‚¬ Pool helpers Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬Ã¢â€â‚¬ //
    private DamageNumber GetFromPool()
    {
        if (_pool.Count == 0) return CreateNew();
        DamageNumber num = _pool.Dequeue();
        if (num == null) return CreateNew();
        return num;
    }

    private DamageNumber CreateNew()
    {
        // Build entirely at runtime Ã¢â‚¬â€ no prefab drag-and-drop needed.
        var go  = new GameObject("DamageNumber");
        go.transform.SetParent(transform);
        var num = go.AddComponent<DamageNumber>();
        go.SetActive(false);
        return num;
    }

    /// <summary>
    /// Return a number to the pool. Called automatically by DamageNumber when it finishes.
    /// </summary>
    public void Return(DamageNumber num)
    {
        if (num == null) return;
        num.gameObject.SetActive(false);
        _pool.Enqueue(num);
    }
}
}
