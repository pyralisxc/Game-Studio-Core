using UnityEngine;

namespace NeonBlack.Gameplay.Features.Traversal
{
/// <summary>
/// Attach to a trigger Collider GameObject at ledge hand height.
/// GrabDetector on the player calls into this zone when a ledge grab is possible.
/// </summary>
public class ClimbZone : MonoBehaviour, NeonBlack.Gameplay.Core.Contracts.IClimbZone
{
    public enum ClimbType { Side, Forward }

    [Header("Climb Setup")]
    [Tooltip("Side climbs onto the edge from the side. Forward climbs over the top.")]
    [SerializeField] private ClimbType climbType = ClimbType.Side;
    [Tooltip("Local-space point where the player's feet should land after climbing.")]
    [SerializeField] private Vector3 standUpOffset = new Vector3(0f, 1f, 0f);
    [Tooltip("Duration of the climb animation in seconds.")]
    [SerializeField] private float climbDuration = 0.6f;

    [Header("Hang State")]
    [Tooltip("When enabled the player hangs first, then can shimmy or climb.")]
    [SerializeField] private bool hangOnGrab = true;
    [Tooltip("Horizontal shimmy speed while hanging.")]
    [SerializeField] private float shimmySpeed = 2.5f;
    [Tooltip("Total width the player can shimmy across, centered on this zone.")]
    [SerializeField] private float shimmyWidth = 3f;

    [Header("Grab Filter")]
    [Tooltip("Require the grab detector to approach from below the ledge height.")]
    [SerializeField] private bool requireApproachFromBelow = true;
    [Tooltip("Maximum downward speed allowed for a valid grab.")]
    [SerializeField] private float maxFallSpeed = -12f;

    [Header("Auto Ledge Grab")]
    [Tooltip("When enabled the player grabs automatically while airborne.")]
    [SerializeField] private bool autoGrab = false;
    [Tooltip("Largest vertical velocity that still allows an auto-grab.")]
    [SerializeField] private float maxGrabVelocityY = 2f;

    [Header("Climb Path - Bezier Control Points")]
    [SerializeField] private Vector3 controlPoint1 = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private Vector3 controlPoint2 = new Vector3(0f, 1.2f, 0f);

    private NeonBlack.Gameplay.Core.Contracts.IClimbTraversalActor _currentPlayer;
    private bool _isActive = true;

    public ClimbType Type => climbType;
    public NeonBlack.Gameplay.Core.Contracts.ClimbTraversalType TraversalType =>
        climbType == ClimbType.Side
            ? NeonBlack.Gameplay.Core.Contracts.ClimbTraversalType.Side
            : NeonBlack.Gameplay.Core.Contracts.ClimbTraversalType.Forward;
    public float ClimbDuration => climbDuration;
    public bool HangOnGrab => hangOnGrab;
    public float ShimmySpeed => shimmySpeed;
    public float ShimmyWidth => shimmyWidth;
    public float MaxFallSpeed => maxFallSpeed;
    public bool AutoGrab => autoGrab;
    public float MaxGrabVelocityY => maxGrabVelocityY;
    public bool RequireApproachFromBelow => requireApproachFromBelow;
    public Vector3 WorldPosition => transform.position;
    public Vector3 ClimbTargetPosition => transform.TransformPoint(standUpOffset);

    public Vector3 SamplePath(float t, Vector3 startPos)
    {
        Vector3 p0 = startPos;
        Vector3 p1 = transform.TransformPoint(controlPoint1);
        Vector3 p2 = transform.TransformPoint(controlPoint2);
        Vector3 p3 = ClimbTargetPosition;
        float u = 1f - t;
        return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
    }

    public void TryGrab(NeonBlack.Gameplay.Core.Contracts.IClimbTraversalActor actor, Vector3 grabDetectorWorldPos)
    {
        if (!_isActive || actor == null)
            return;

        if (requireApproachFromBelow && grabDetectorWorldPos.y > transform.position.y)
        {
            Debug.Log(
                $"[ClimbZone] Rejected: approach-from-below. GrabDetector Y={grabDetectorWorldPos.y:F2} > ClimbZone Y={transform.position.y:F2}",
                this);
            return;
        }

        if (actor.CurrentVelocity.y < maxFallSpeed)
        {
            Debug.Log(
                $"[ClimbZone] Rejected: fall-speed. velocityY={actor.CurrentVelocity.y:F2} < maxFallSpeed={maxFallSpeed}",
                this);
            return;
        }

        _currentPlayer = actor;

        if (autoGrab)
            actor.TryLedgeGrab(this, maxGrabVelocityY);
        else
            actor.SetClimbZone(this);
    }

    public void NotifyExit(NeonBlack.Gameplay.Core.Contracts.IClimbTraversalActor actor)
    {
        if (_currentPlayer != actor)
            return;

        _currentPlayer.ClearClimbZone();
        _currentPlayer = null;
    }

    public void DisableTemporarily() => _isActive = false;

    public void EnableAfterClimb() => _isActive = true;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 standPos = ClimbTargetPosition;
        Vector3 origin = transform.position;
        Vector3 cp1w = transform.TransformPoint(controlPoint1);
        Vector3 cp2w = transform.TransformPoint(controlPoint2);

        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.9f);
        Gizmos.DrawSphere(standPos, 0.1f);
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.3f);
        Gizmos.DrawWireSphere(standPos + Vector3.up * 0.9f, 0.3f);
        Gizmos.DrawWireSphere(standPos + Vector3.up * 1.65f, 0.15f);

        Gizmos.color = Color.cyan;
        const int steps = 28;
        Vector3 prev = origin;
        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            Vector3 next = SamplePath(t, origin);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawLine(origin, cp1w);
        Gizmos.DrawLine(standPos, cp2w);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(cp1w, 0.08f);
        Gizmos.DrawSphere(cp2w, 0.08f);
    }
#endif
}
}
