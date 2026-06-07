using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Characters;
using UnityEngine;

namespace NeonBlack.Gameplay.Characters
{
/// <summary>
/// Lightweight static registry that exposes the current player Transform.
/// All systems that need the active player position (HazardSpawner, collectible spawners, Hazard, etc.)
/// read from here instead of each running their own FindGameObjectWithTag("Player") call.
///
/// Additionally, this component can act as a local `IPlayerProvider` fallback
/// when participant infrastructure is not present.
///
/// Setup:
///   1. Attach this component to the Player GameObject in your game scene.
///   2. All consumers will automatically pick up the registration on Awake.
///
/// On death / disable the entry is cleared so consumers receive null and can
/// safely guard against a missing player.
/// </summary>
public class PlayerRegistry : MonoBehaviour, IPlayerProvider
{
    /// <summary>The current living player's Transform, or null if none exists.</summary>
    public static Transform Player => _player != null ? _player : ResolveFallbackPlayer();

    /// <summary>The current living player's supported 2D motor, or null if none exists.</summary>
    public static Motor2D Motor2D => _motor2D != null ? _motor2D : ResolveFallbackMotor2D();

    private static Transform _player;
    private static Motor2D _motor2D;

    private void Awake()
    {
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

    // â”€â”€ IPlayerProvider â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    public UnityEngine.Transform GetPlayerTransform() => Player;
    public UnityEngine.GameObject GetPlayerGameObject() => Player != null ? Player.gameObject : null;

    // â”€â”€ IGameService â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ //
    public void Initialize() { }
    public void Shutdown() { }

    private static Transform ResolveFallbackPlayer()
    {
        if (ParticipantQueryUtility.TryResolvePlayerProvider(out IPlayerProvider provider) && provider != null)
        {
            if (provider is PlayerRegistry)
                return _player;

            return provider.GetPlayerTransform();
        }

        return null;
    }

    private static Motor2D ResolveFallbackMotor2D()
    {
        Transform player = ResolveFallbackPlayer();
        return player != null ? player.GetComponent<Motor2D>() : null;
    }

}
}
