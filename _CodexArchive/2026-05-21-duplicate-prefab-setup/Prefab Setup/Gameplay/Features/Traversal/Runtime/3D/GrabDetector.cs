using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
/// <summary>
/// Child trigger used to detect climb zones for CharacterController-based actors.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GrabDetector : MonoBehaviour
{
    private NeonBlack.Gameplay.Core.Contracts.IClimbTraversalActor _player;

    private void Awake()
    {
        MonoBehaviour[] behaviours = GetComponentsInParent<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is NeonBlack.Gameplay.Core.Contracts.IClimbTraversalActor traversalActor)
            {
                _player = traversalActor;
                break;
            }
        }

        if (_player == null)
            Debug.LogWarning("[GrabDetector] No climb traversal actor found in parent hierarchy.", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == transform.root)
            return;

        if (_player == null)
        {
            Debug.LogWarning("[GrabDetector] _player is null - GrabDetector has no climb traversal actor in parent.", this);
            return;
        }

        ClimbZone zone = other.GetComponent<ClimbZone>();
        if (zone == null)
            return;

        zone.TryGrab(_player, transform.position);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root == transform.root || _player == null)
            return;

        ClimbZone zone = other.GetComponent<ClimbZone>();
        if (zone == null)
            return;

        zone.NotifyExit(_player);
    }
}
}
