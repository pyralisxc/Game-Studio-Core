using System.Collections.Generic;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Combat
{
public interface IDamageNumberSink
{
    void Spawn(float amount, Vector3 worldPos, bool isCritical = false);
    void SpawnHeal(float amount, Vector3 worldPos);
}

/// <summary>
/// Scene damage-number pool that implements IDamageNumberSink.
/// No prefab required - numbers are built entirely at runtime.
/// Place one instance in the loaded gameplay systems scene and assign it to
/// consumers through their Damage Number Sink fields.
/// </summary>
public class DamageNumberSpawner : MonoBehaviour, IDamageNumberSink
{
    [Header("Pool")]
    [SerializeField] private int initialPoolSize = 20;

    [Header("Presentation")]
    [Tooltip("Camera used to billboard floating numbers. Assign explicitly for split-screen, replay, or custom camera rigs.")]
    [SerializeField] private Camera popupCamera;

    private readonly Queue<DamageNumber> _pool = new Queue<DamageNumber>();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
            _pool.Enqueue(CreateNew());
    }

    public void Spawn(float amount, Vector3 worldPos, bool isCritical = false)
    {
        DamageNumber number = GetFromPool();
        number.ConfigureRuntime(popupCamera, this);
        number.Play(amount, worldPos, isCritical: isCritical, isHeal: false);
    }

    public void SpawnHeal(float amount, Vector3 worldPos)
    {
        DamageNumber number = GetFromPool();
        number.ConfigureRuntime(popupCamera, this);
        number.Play(amount, worldPos, isCritical: false, isHeal: true);
    }

    public void SetPopupCamera(Camera camera)
    {
        popupCamera = camera;
    }

    private DamageNumber GetFromPool()
    {
        if (_pool.Count == 0) return CreateNew();
        DamageNumber num = _pool.Dequeue();
        if (num == null) return CreateNew();
        return num;
    }

    private DamageNumber CreateNew()
    {
        // Build entirely at runtime - no prefab drag-and-drop needed.
        var go  = new GameObject("DamageNumber");
        go.transform.SetParent(transform);
        var num = go.AddComponent<DamageNumber>();
        num.ConfigureRuntime(popupCamera, this);
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
