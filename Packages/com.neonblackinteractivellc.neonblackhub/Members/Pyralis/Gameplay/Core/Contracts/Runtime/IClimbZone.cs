using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public enum ClimbTraversalType
    {
        Side,
        Forward
    }

    public interface IClimbZone
    {
        ClimbTraversalType TraversalType { get; }
        float ClimbDuration { get; }
        bool HangOnGrab { get; }
        float ShimmySpeed { get; }
        float ShimmyWidth { get; }
        bool AutoGrab { get; }
        float MaxGrabVelocityY { get; }
        Vector3 WorldPosition { get; }
        Vector3 ClimbTargetPosition { get; }

        Vector3 SamplePath(float t, Vector3 startPos);
        void DisableTemporarily();
        void EnableAfterClimb();
    }
}
