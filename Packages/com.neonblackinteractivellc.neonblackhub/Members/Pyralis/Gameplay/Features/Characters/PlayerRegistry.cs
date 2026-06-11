using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Characters;
using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
/// <summary>
/// Lightweight static registry that exposes the current player Transform.
/// Preferred path for systems that need a quick global reference to the primary participant
/// without participating in the full DI lifecycle.
/// </summary>
public class PlayerRegistry : MonoBehaviour, IPlayerProvider
{
    /// <summary>The current living participant/player Transform, or null if none exists.</summary>
    public static Transform Player => ResolveEffectivePlayer();

    /// <summary>The current living participant/player's supported 2D motor, or null if none exists.</summary>
    public static Motor2D Motor2D => ResolveEffectiveMotor2D();

    private static Transform _player;
    private static Motor2D _motor2D;

    private void Awake()
    {
        // Still register this instance as a candidate for IPlayerProvider resolution
        // but the static accessors now prefer the shared roster service.
        _player    = transform;
        _motor2D   = GetComponent<Motor2D>();
    }

    private void OnDestroy()
    {
        if (_player == transform)  { _player = null; _motor2D = null; }
    }

    private void OnDisable()
    {
        if (_player == transform)  { _player = null; _motor2D = null; }
    }

    private void OnEnable()
    {
        _player    = transform;
        _motor2D   = GetComponent<Motor2D>();
    }

    // IPlayerProvider
    public UnityEngine.Transform GetPlayerTransform() => transform;
    public UnityEngine.GameObject GetPlayerGameObject() => gameObject;

    // IGameService
    public void Initialize() { }
    public void Shutdown() { }

    private static Transform ResolveEffectivePlayer()
    {
        // 1. Try to resolve via the high-level participant roster service (the preferred path)
        if (ParticipantQueryUtility.TryResolvePlayerProvider(out IPlayerProvider provider) && provider != null)
        {
            // If the provider IS a PlayerRegistry, we use the static registration
            // to avoid infinite recursion if ParticipantQueryUtility returned this class.
            if (provider is PlayerRegistry)
                return _player;

            return provider.GetPlayerTransform();
        }

        // 2. Fallback to the local static registration
        return _player;
    }

    private static Motor2D ResolveEffectiveMotor2D()
    {
        Transform player = ResolveEffectivePlayer();
        return player != null ? player.GetComponent<Motor2D>() : null;
    }

}
}
